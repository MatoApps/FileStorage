using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.Options;
using Abp;
using Abp.Collections.Extensions;
using Abp.Domain.Services;
using Abp.Domain.Uow;
using Abp.Events.Bus;
using FileStorage.Exceptions;
using FileStorage.Enums;
using FileStorage.Blob;
using FileStorage;
using FileStorage.Interfaces;
using FileStorage.Configuration;
using FileStorage.Models;
using Abp.Domain.Repositories;
using Abp.Domain.Entities;
using System.IO;
using System.Data.SqlTypes;
using FileStorage.Net.MimeTypes;

namespace FileStorage.Files
{
    public class FileManager : DomainService, IFileManager
    {
        private readonly IEventBus _distributedEventBus;
        private readonly IFileRepository _fileRepository;
        private readonly IFileBlobNameGenerator _fileBlobNameGenerator;
        private readonly IFileContentHashProvider _fileContentHashProvider;
        private readonly IFileContainerConfigurationProvider _configurationProvider;
        private readonly IFileDownloadProvider fileDownloadProvider;
        private readonly BlobManager blobContainer;

        public FileManager(
            IEventBus distributedEventBus,
            IFileRepository fileRepository,
            IFileBlobNameGenerator fileBlobNameGenerator,
            IFileContentHashProvider fileContentHashProvider,
            IFileContainerConfigurationProvider configurationProvider,
            IFileDownloadProvider fileDownloadProvider,
            BlobManager blobManager
            )
        {
            _distributedEventBus = distributedEventBus;
            _fileRepository = fileRepository;
            _fileBlobNameGenerator = fileBlobNameGenerator;
            _fileContentHashProvider = fileContentHashProvider;
            _configurationProvider = configurationProvider;
            this.fileDownloadProvider = fileDownloadProvider;
            blobContainer = blobManager;
        }

        public virtual async Task<File> CreateAsync(string fileContainerName, long? ownerUserId, string fileName,
            string mimeType, FileType fileType, File parent, byte[] fileContent)
        {
            Check.NotNullOrWhiteSpace(fileContainerName, nameof(File.FileContainerName));
            Check.NotNullOrWhiteSpace(fileName, nameof(File.FileName));

            var configuration = _configurationProvider.Get(fileContainerName);

            CheckFileName(fileName, configuration);
            CheckDirectoryHasNoFileContent(fileType, fileContent);

            var hashString = _fileContentHashProvider.GetHashString(fileContent);

            string blobName = null;

            if (fileType == FileType.RegularFile)
            {
                if (!configuration.DisableBlobReuse)
                {
                    var existingFile = await _fileRepository.FirstOrDefaultAsync(fileContainerName, hashString, fileContent.LongLength);

                    // Todo: should lock the file that provides a reused BLOB.
                    if (existingFile != null)
                    {
                        Check.NotNullOrWhiteSpace(existingFile.BlobName, nameof(existingFile.BlobName));

                        blobName = existingFile.BlobName;
                    }
                }

                blobName ??= await _fileBlobNameGenerator.CreateAsync(fileType, fileName, parent, mimeType,
                    configuration.AbpBlobDirectorySeparator);
            }

            if (configuration.EnableAutoRename)
            {
                if (await IsFileExistAsync(fileName, parent?.Id, fileContainerName, ownerUserId))
                {
                    fileName = await _fileRepository.GetFileNameWithNextSerialNumberAsync(fileName, parent?.Id,
                        fileContainerName, ownerUserId);
                }
            }

            await CheckFileNotExistAsync(fileName, parent?.Id, fileContainerName, ownerUserId);

            var file = new File(parent, fileContainerName, fileName, mimeType,
                fileType, 0, fileContent?.LongLength ?? 0, hashString, blobName, ownerUserId);

            return file;
        }

        public virtual async Task<File> ChangeAsync(File file, string newFileName, File oldParent, File newParent)
        {
            Check.NotNullOrWhiteSpace(newFileName, nameof(File.FileName));

            if (file.ParentId != oldParent?.Id)
            {
                throw new IncorrectParentException(oldParent);
            }

            var configuration = _configurationProvider.Get(file.FileContainerName);

            CheckFileName(newFileName, configuration);

            if (newParent?.Id != file.ParentId)
            {
                if (configuration.EnableAutoRename)
                {
                    if (await IsFileExistAsync(newFileName, newParent?.Id, file.FileContainerName, file.OwnerUserId))
                    {
                        newFileName = await _fileRepository.GetFileNameWithNextSerialNumberAsync(newFileName, newParent?.Id,
                            file.FileContainerName, file.OwnerUserId);
                    }
                }

                await CheckFileNotExistAsync(newFileName, newParent?.Id, file.FileContainerName, file.OwnerUserId);
            }

            if (newFileName != file.FileName)
            {
                await CheckFileNotExistAsync(newFileName, newParent?.Id, file.FileContainerName, file.OwnerUserId);
            }

            if (oldParent != newParent)
            {
                await CheckNotMovingDirectoryToSubDirectoryAsync(file, newParent);
            }

            file.UpdateInfo(newFileName, file.MimeType, file.SubFilesQuantity, file.ByteSize, file.Hash, file.BlobName,
                oldParent, newParent);

            return file;
        }

        [UnitOfWork]
        public virtual async Task<File> ChangeAsync(File file, string newFileName, string newMimeType, byte[] newFileContent, File oldParent, File newParent)
        {
            Check.NotNullOrWhiteSpace(newFileName, nameof(File.FileName));

            if (file.ParentId != oldParent?.Id)
            {
                throw new IncorrectParentException(oldParent);
            }

            var configuration = _configurationProvider.Get(file.FileContainerName);

            CheckFileName(newFileName, configuration);
            CheckDirectoryHasNoFileContent(file.FileType, newFileContent);

            if (newFileName != file.FileName || newParent?.Id != file.ParentId)
            {
                await CheckFileNotExistAsync(newFileName, newParent?.Id, file.FileContainerName, file.OwnerUserId);
            }

            if (oldParent != newParent)
            {
                await CheckNotMovingDirectoryToSubDirectoryAsync(file, newParent);
            }

            var oldBlobName = file.BlobName;

            var blobName = await _fileBlobNameGenerator.CreateAsync(file.FileType, newFileName, newParent, newMimeType,
                configuration.AbpBlobDirectorySeparator);

            await _distributedEventBus.TriggerAsync(new FileBlobNameChangedEto
            {
                TenantId = file.TenantId,
                FileId = file.Id,
                FileType = file.FileType,
                FileContainerName = file.FileContainerName,
                OldBlobName = oldBlobName,
                NewBlobName = blobName
            });

            var hashString = _fileContentHashProvider.GetHashString(newFileContent);

            file.UpdateInfo(newFileName, newMimeType, file.SubFilesQuantity, newFileContent?.LongLength ?? 0,
                hashString, blobName, oldParent, newParent);

            return file;
        }

        protected virtual async Task CheckNotMovingDirectoryToSubDirectoryAsync([NotNull] File file, [CanBeNull] File targetParent)
        {
            if (file.FileType != FileType.Directory)
            {
                return;
            }

            var parent = targetParent;

            while (parent != null)
            {
                if (parent.Id == file.Id)
                {
                    throw new FileIsMovedToSubDirectoryException();
                }

                parent = parent.ParentId.HasValue ? await _fileRepository.GetAsync(parent.ParentId.Value) : null;
            }
        }

        public virtual async Task DeleteAsync([NotNull] File file, bool isHardDelete = false, CancellationToken cancellationToken = default)
        {
            var parent = file.ParentId.HasValue
                ? await _fileRepository.GetAsync(file.ParentId.Value)
                : null;

            parent?.TryAddSubFileUpdatedDomainEvent();

            if (isHardDelete)
            {
                await _fileRepository.HardDeleteAsync(file);
            }
            else
            {
                await _fileRepository.DeleteAsync(file);

            }

            if (file.FileType == FileType.Directory)
            {
                await DeleteSubFilesAsync(file, file.FileContainerName, file.OwnerUserId, isHardDelete, cancellationToken);
            }
        }
        protected virtual async Task DeleteSubFilesAsync([CanBeNull] File file, [NotNull] string fileContainerName,
    long? ownerUserId, bool isHardDelete = false, CancellationToken cancellationToken = default)
        {
            var subFiles = await _fileRepository.GetListAsync(file?.Id, fileContainerName, ownerUserId,
                null, cancellationToken);

            foreach (var subFile in subFiles)
            {
                if (subFile.FileType == FileType.Directory)
                {
                    await DeleteSubFilesAsync(subFile, fileContainerName, ownerUserId, isHardDelete, cancellationToken);
                }

                if (isHardDelete)
                {
                    await _fileRepository.HardDeleteAsync(subFile);
                }
                else
                {
                    await _fileRepository.DeleteAsync(subFile);

                }
            }
        }


        public virtual async Task RestoreAsync([NotNull] File file, CancellationToken cancellationToken = default)
        {
            var currentFile = await _fileRepository.GetAsync(file.Id);
            currentFile.UnDelete();

            if (file.FileType == FileType.Directory)
            {
                await RestoreSubFilesAsync(file, file.FileContainerName, file.OwnerUserId, cancellationToken);
            }
        }


        protected virtual async Task RestoreSubFilesAsync([CanBeNull] File file, [NotNull] string fileContainerName,
            long? ownerUserId, CancellationToken cancellationToken = default)
        {
            var subFiles = await _fileRepository.GetListAsync(file?.Id, fileContainerName, ownerUserId,
                null, cancellationToken);

            foreach (var subFile in subFiles)
            {
                if (subFile.FileType == FileType.Directory)
                {
                    await RestoreSubFilesAsync(subFile, fileContainerName, ownerUserId, cancellationToken);
                }
                var currentFile = await _fileRepository.GetAsync(subFile.Id);
                currentFile.UnDelete();
            }
        }

        protected virtual void CheckFileName(string fileName, IFileContainerConfiguration configuration)
        {
            if (fileName.Contains(FileManagementConsts.DirectorySeparator))
            {
                throw new FileNameContainsSeparatorException(fileName, FileManagementConsts.DirectorySeparator);
            }
        }

        protected virtual void CheckDirectoryHasNoFileContent(FileType fileType, byte[] fileContent)
        {
            if (fileType == FileType.Directory && !fileContent.IsNullOrEmpty())
            {
                throw new DirectoryFileContentIsNotEmptyException();
            }
        }

        public virtual async Task<bool> TrySaveBlobAsync(File file, byte[] fileContent, bool disableBlobReuse = false,
            bool allowBlobOverriding = false, CancellationToken cancellationToken = default)
        {
            if (file.FileType != FileType.RegularFile)
            {
                throw new UnexpectedFileTypeException(file.Id, file.FileType);
            }

            if (!disableBlobReuse && await blobContainer.ExistsAsync(file.BlobName, cancellationToken))
            {
                return false;
            }

            using (var memoryStream = new System.IO.MemoryStream(fileContent))
            {
                await blobContainer.SaveAsync(file.BlobName, memoryStream, allowBlobOverriding, cancellationToken);
            }


            return true;
        }


        public virtual async Task<byte[]> GetBlobAsync(File file, CancellationToken cancellationToken = default)
        {
            if (file.FileType != FileType.RegularFile)
            {
                throw new UnexpectedFileTypeException(file.Id, file.FileType, FileType.RegularFile);
            }


            byte[] allBytes;
            using (var stream = await blobContainer.GetAsync(file.BlobName, cancellationToken))
            {
                using (var memoryStream = new MemoryStream())
                {
                    if (stream.CanSeek)
                    {
                        stream.Position = 0;
                    }
                    await stream.CopyToAsync(memoryStream);
                    allBytes = memoryStream.ToArray();
                }
            }
            return allBytes;
        }



        public async Task DeleteBlobAsync(string fileContainerName, string blobName,
            CancellationToken cancellationToken = default)
        {

            await blobContainer.DeleteAsync(blobName, cancellationToken);
        }

        public virtual async Task<FileDownloadInfoModel> GetDownloadInfoAsync(File file)
        {
            if (file.FileType != FileType.RegularFile)
            {
                throw new UnexpectedFileTypeException(file.Id, file.FileType, FileType.RegularFile);
            }


            return await fileDownloadProvider.CreateDownloadInfoAsync(file);
        }


        protected virtual async Task CheckFileNotExistAsync(string fileName, Guid? parentId, string fileContainerName, long? ownerUserId)
        {
            if (await IsFileExistAsync(fileName, parentId, fileContainerName, ownerUserId))
            {
                throw new FileAlreadyExistsException(fileName, parentId);
            }
        }

        public virtual async Task<bool> IsFileExistAsync(string fileName, Guid? parentId, string fileContainerName, long? ownerUserId)
        {
            return await _fileRepository.FindAsync(fileName, parentId, fileContainerName, ownerUserId) != null;
        }


        public virtual bool GetIsImageFile(File file)
        {
            return file.MimeType is
                                MimeTypeNames.ImageGif or
                                MimeTypeNames.ImageJpeg or
                                MimeTypeNames.ImagePng or
                                MimeTypeNames.ImageSvgXml or
                                MimeTypeNames.ImagePjpeg;
        }

    }

}