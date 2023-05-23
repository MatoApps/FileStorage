using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Abp.Linq.Extensions;
using Abp.Authorization;
using Microsoft.AspNetCore.Mvc;
using FileStorage.Application.Dto;
using FileStorage.Files;
using Abp.IO.Extensions;
using Abp.Application.Services.Dto;
using Abp.Collections.Extensions;
using Abp.UI;
using Abp.Extensions;
using FileStorage.Enums;
using FileStorage.Cache;
using FileStorage.Interfaces;
using FileStorage.Configuration;
using FileStorage.Models;
using File = FileStorage.Files.File;
using FileStorage.Exceptions;
using Abp.Domain.Uow;
using FileStorage.Uow;
using Abp;
using Microsoft.EntityFrameworkCore;
using FileStorage.Net.MimeTypes;
using Abp.Application.Services;

namespace FileStorage.Application
{
    [AllowAnonymous]
    public class FileAppService : ApplicationService, IFileAppService
    {
        private readonly IFileManager _fileManager;
        private readonly IFileRepository Repository;
        private readonly UserFileDownloadLimitCache downloadLimitCache;
        private readonly FileDomainService fileDomainService;
        private readonly IFileContainerConfigurationProvider _configurationProvider;

        public FileAppService(
            IFileManager fileManager,
            IFileRepository repository,
            UserFileDownloadLimitCache userFileDownloadLimitCache,
            FileDomainService fileDomainService,
            IFileContainerConfigurationProvider configurationProvider)
        {
            _fileManager = fileManager;
            Repository = repository;
            downloadLimitCache = userFileDownloadLimitCache;
            this.fileDomainService = fileDomainService;
            _configurationProvider = configurationProvider;
        }

        public async Task<FileBriefDto> GetAsync(GetFileInput input)
        {
            if (input.IncludeTrash)
            {
                UnitOfWorkManager.Current.DisableFilter(AbpDataFilters.SoftDelete);
                UnitOfWorkManager.Current.EnableFilter(FileDataFilters.OnlyShowSoftDelete);
            }

            if (input.Id == default && !string.IsNullOrEmpty(input.Path))
            {
                var entity = await this.fileDomainService.GetEntityByPathAsync(input.Path, input.FileContainerName);
                if (entity == null)
                {
                    throw new UserFriendlyException("找不到文件");
                }
                return ObjectMapper.Map<FileBriefDto>(entity);
            }

            var file = await Repository.GetAsync(input.Id);
            return ObjectMapper.Map<FileBriefDto>(file);

        }


        public async Task<PagedResultDto<FileBriefDto>> GetAllAsync(GetFileListInput input)
        {
            if (input.IncludeTrash)
            {
                UnitOfWorkManager.Current.DisableFilter(AbpDataFilters.SoftDelete);
                UnitOfWorkManager.Current.EnableFilter(FileDataFilters.OnlyShowSoftDelete);
            }
            var result = await this._GetAllFileAsync(input);
            var entities = result.Item1;
            var totalCount = result.Item2;

            return new PagedResultDto<FileBriefDto>(
                totalCount,
                entities.Select(ObjectMapper.Map<FileBriefDto>).ToList()
            );
        }

        public async Task<PagedResultDto<FileBriefWithThumbnailDto>> GetAllWithThumbnailAsync(GetFileListInput input)
        {
            if (input.IncludeTrash)
            {
                UnitOfWorkManager.Current.DisableFilter(AbpDataFilters.SoftDelete);
                UnitOfWorkManager.Current.EnableFilter(FileDataFilters.OnlyShowSoftDelete);
            }
            var result = await this._GetAllFileAsync(input);
            var entities = result.Item1;
            var totalCount = result.Item2;
            var fileBriefWithThumbnailDtos = ObjectMapper.Map<List<FileBriefWithThumbnailDto>>(entities);
            if (entities.Count != fileBriefWithThumbnailDtos.Count)
            {
                throw new UserFriendlyException("映射文件列表错误");
            }
            for (int i = 0; i < fileBriefWithThumbnailDtos.Count; i++)
            {
                var file = entities[i];

                bool IsImageFile = _fileManager.GetIsImageFile(file);
                if (IsImageFile)
                {
                    fileBriefWithThumbnailDtos[i].Thumbnail = $"api/services/app/File/ActionGetThumbnailImage?id={file.Id}";
                }
            }

            return new PagedResultDto<FileBriefWithThumbnailDto>(
                totalCount,
                fileBriefWithThumbnailDtos
            );
        }


        public async Task<FileBriefDto> CreateAsync(CreateFileInput input)
        {
            if (input.ParentId == null && !string.IsNullOrEmpty(input.ParentPath))
            {
                var entity = await this.fileDomainService.CreateDirectoryByPathAsync(new CreateFileInfoBase()
                {
                    FileContainerName = input.FileContainerName,
                    ParentId = input.ParentId,
                    ParentPath = input.ParentPath,
                    OwnerUserId = input.OwnerUserId,

                });
                input.ParentId = entity.Id;
            }

            var configuration = _configurationProvider.Get(input.FileContainerName);

            fileDomainService.CheckFileSize(new Dictionary<string, long> { { input.FileName, input.Content?.LongLength ?? 0 } }, configuration);

            if (input.FileType == FileType.RegularFile)
            {
                fileDomainService.CheckFileExtension(new[] { input.FileName }, configuration);
            }

            var file = await fileDomainService.CreateFileEntityAsync(new CreateFileInfo()
            {
                FileName = input.FileName,
                MimeType = input.MimeType,
                FileType = input.FileType,
                Content = input.Content,
                FileContainerName = input.FileContainerName,
                ParentId = input.ParentId,
                ParentPath = input.ParentPath,
                OwnerUserId = input.OwnerUserId,
            });


            await Repository.InsertAsync(file);

            await fileDomainService.TrySaveBlobAsync(file, input.Content, configuration.DisableBlobReuse, configuration.AllowBlobOverriding);

            var fileInfo = ObjectMapper.Map<FileBriefDto>(file);

            return fileInfo;
        }


        public async Task<CreateFileOutput> CreateWithStreamAsync(CreateFileWithStreamInput input)
        {
            if (input.ParentId == null && !string.IsNullOrEmpty(input.ParentPath))
            {
                var entity = await this.fileDomainService.CreateDirectoryByPathAsync(new CreateFileInfoBase()
                {
                    FileContainerName = input.FileContainerName,
                    ParentId = input.ParentId,
                    ParentPath = input.ParentPath,
                    OwnerUserId = input.OwnerUserId,

                });
                input.ParentId = entity.Id;
            }
            var configuration = _configurationProvider.Get(input.FileContainerName);

            fileDomainService.CheckFileSize(new Dictionary<string, long> { { input.Content.FileName, input.Content?.ContentLength ?? 0 } }, configuration);

            fileDomainService.CheckFileExtension(new[] { input.Content.FileName }, configuration);

            var fileContent = input.Content.GetStream().GetAllBytes();

            var file = await fileDomainService.CreateFileEntityAsync(
                    input.ParentPath,
                    input.ParentId,
                    input.OwnerUserId,
                    input.FileContainerName,
                    fileType: FileType.RegularFile,
                    fileName: input.Content.FileName,
                    mimeType: input.Content.ContentType,
                    fileContent: fileContent,
                    generateUniqueFileName: input.GenerateUniqueFileName
                );



            await Repository.InsertAsync(file);

            await fileDomainService.TrySaveBlobAsync(file, fileContent, configuration.DisableBlobReuse, configuration.AllowBlobOverriding);

            var fd = await fileDomainService.GetFileWithDownloadInfoAsync(file);


            return new CreateFileOutput
            {
                FileInfo = ObjectMapper.Map<FileBriefDto>(fd.Item1),
                DownloadInfo = fd.Item2
            };
        }


        [HttpDelete]
        public async Task TrashAsync(GetFileInput input)
        {
            await _DeleteAsync(input, false);
        }

        [HttpDelete]
        public async Task DeleteAsync(GetFileInput input)
        {
            await _DeleteAsync(input, true);
        }

        private async Task _DeleteAsync(GetFileInput input, bool isHardDelete)
        {
            if (input.IncludeTrash)
            {
                UnitOfWorkManager.Current.DisableFilter(AbpDataFilters.SoftDelete);
                UnitOfWorkManager.Current.EnableFilter(FileDataFilters.OnlyShowSoftDelete);
            }
            if (input.Id == default && !string.IsNullOrEmpty(input.Path))
            {
                var entity = await this.fileDomainService.GetEntityByPathAsync(input.Path, input.FileContainerName);
                if (entity == null)
                {
                    throw new UserFriendlyException("找不到文件");
                }
                await _fileManager.DeleteAsync(entity, isHardDelete: isHardDelete);
            }

            var file = await Repository.GetAsync(input.Id);

            await _fileManager.DeleteAsync(file, isHardDelete: isHardDelete);
        }

        public async Task RestoreAsync(GetFileInput input)
        {
            if (input.IncludeTrash)
            {
                UnitOfWorkManager.Current.DisableFilter(AbpDataFilters.SoftDelete);
                UnitOfWorkManager.Current.EnableFilter(FileDataFilters.OnlyShowSoftDelete);
            }
            if (input.Id == default && !string.IsNullOrEmpty(input.Path))
            {
                var entity = await this.fileDomainService.GetEntityByPathAsync(input.Path, input.FileContainerName);
                if (entity == null)
                {
                    throw new UserFriendlyException("找不到文件");
                }
                await _fileManager.RestoreAsync(entity);
            }

            var file = await Repository.GetAsync(input.Id);

            await _fileManager.RestoreAsync(file);
        }

        public virtual async Task<CreateManyFileOutput> CreateManyWithStreamAsync(CreateManyFileWithStreamInput input)
        {
            var configuration = _configurationProvider.Get(input.FileContainerName);

            fileDomainService.CheckFileQuantity(input.FileContents.Count, configuration);
            fileDomainService.CheckFileSize(input.FileContents.ToDictionary(x => x.FileName, x => x.ContentLength ?? 0), configuration);

            fileDomainService.CheckFileExtension(
                input.FileContents.Select(x => x.FileName).ToList(),
                configuration);

            var files = new File[input.FileContents.Count];
            var fileContents = new List<byte[]>(input.FileContents.Count);

            for (var i = 0; i < input.FileContents.Count; i++)
            {
                var fileContentItem = input.FileContents[i];
                var fileContent = fileContentItem.GetStream().GetAllBytes();

                var file = await fileDomainService.CreateFileEntityAsync(
                    input.ParentPath,
                    input.ParentId,
                    input.OwnerUserId,
                    input.FileContainerName,
                    fileType: FileType.RegularFile,
                    fileName: fileContentItem.FileName,
                    mimeType: fileContentItem.ContentType,
                    fileContent: fileContent,
                    generateUniqueFileName: input.GenerateUniqueFileName
                );


                await Repository.InsertAsync(file);

                files[i] = file;
                fileContents.Add(fileContent);
            }

            for (var i = 0; i < files.Length; i++)
            {
                await fileDomainService.TrySaveBlobAsync(files[i], fileContents[i], configuration.DisableBlobReuse,
                    configuration.AllowBlobOverriding);
            }

            var items = new List<CreateFileOutput>();

            foreach (var file in files)
            {
                var fd = await fileDomainService.GetFileWithDownloadInfoAsync(file);

                var item = new CreateFileOutput
                {
                    FileInfo = ObjectMapper.Map<FileBriefDto>(fd.Item1),
                    DownloadInfo = fd.Item2
                };
                items.Add(item);
            }

            return new CreateManyFileOutput { Items = items };
        }


        public virtual async Task<FileBriefDto> MoveAsync(MoveFileInput input)
        {


            if (input.NewParentId == null && !string.IsNullOrEmpty(input.NewParentPath))
            {
                var entity = await this.fileDomainService.GetEntityByPathAsync(input.NewParentPath, input.FileContainerName);
                if (entity == null || entity.FileType != FileType.Directory)
                {
                    throw new UserFriendlyException("解析文件夹路径错误");

                }
                input.NewParentId = entity.Id;
            }

            var newFileName = input.NewFileName;

            var file = await Repository.GetAsync(input.Id);

            var configuration = _configurationProvider.Get(file.FileContainerName);

            fileDomainService.CheckFileExtension(new[] { newFileName }, configuration);

            var oldParent = await fileDomainService.TryGetEntityByNullableIdAsync(file.ParentId);

            var newParent = input.NewParentId == file.ParentId
                ? oldParent
                : await fileDomainService.TryGetEntityByNullableIdAsync(input.NewParentId);

            await _fileManager.ChangeAsync(file, newFileName, oldParent, newParent);

            await Repository.UpdateAsync(file);

            return ObjectMapper.Map<FileBriefDto>(file);
        }


        public virtual async Task<FileBriefDto> CopyAsync(MoveFileInput input)
        {

            if (input.NewParentId == null && !string.IsNullOrEmpty(input.NewParentPath))
            {
                var entity = await this.fileDomainService.GetEntityByPathAsync(input.NewParentPath, input.FileContainerName);
                if (entity == null || entity.FileType != FileType.Directory)
                {
                    throw new UserFriendlyException("解析文件夹路径错误");

                }
                input.NewParentId = entity.Id;
            }

            var newFileName = input.NewFileName;

            var file = await Repository.GetAsync(input.Id);

            var fileContent = await _fileManager.GetBlobAsync(file);

            var configuration = _configurationProvider.Get(file.FileContainerName);

            var oldParent = await fileDomainService.TryGetEntityByNullableIdAsync(file.ParentId);

            var newParent = input.NewParentId == file.ParentId
              ? oldParent
              : await fileDomainService.TryGetEntityByNullableIdAsync(input.NewParentId);

            var newfile = await _fileManager.CreateAsync(file.FileContainerName, file.OwnerUserId, newFileName.Trim(),
                file.MimeType, file.FileType, newParent, fileContent);


            await Repository.InsertAsync(newfile);

            await fileDomainService.TrySaveBlobAsync(newfile, fileContent, configuration.DisableBlobReuse, configuration.AllowBlobOverriding);

            var fileInfo = ObjectMapper.Map<FileBriefDto>(newfile);

            return fileInfo;
        }

        public virtual async Task<FileDownloadInfoModel> GetDownloadInfoAsync(EntityDto<Guid> input)
        {
            var file = await Repository.GetAsync(input.Id);

            var configuration = _configurationProvider.Get(file.FileContainerName);

            if (!configuration.GetDownloadInfoTimesLimitEachUserPerMinute.HasValue)
            {
                var fdownload = await fileDomainService.GetFileWithDownloadInfoAsync(file);
                return fdownload.Item2;
            }

            var cacheItemKey = fileDomainService.GetDownloadLimitCacheItemKey();

            var absoluteExpiration = DateTime.Now.AddMinutes(1);

            var cacheItem = await downloadLimitCache.GetAsync(fileDomainService.GetDownloadLimitCacheItemKey(),
                (k) => new UserFileDownloadLimitCacheItem
                {
                    Count = 0,
                    AbsoluteExpiration = absoluteExpiration
                }, absoluteExpireTime: absoluteExpiration);

            if (cacheItem.Count >= configuration.GetDownloadInfoTimesLimitEachUserPerMinute.Value)
            {
                throw new UserGetDownloadInfoExceededLimitException();
            }

            var fd = await fileDomainService.GetFileWithDownloadInfoAsync(file);

            cacheItem.Count++;

            await downloadLimitCache.SetAsync(cacheItemKey, cacheItem, absoluteExpireTime: absoluteExpiration);

            return fd.Item2;
        }

        public async Task<FileBriefDto> UpdateAsync(UpdateFileInput input)
        {
            var file = await Repository.GetAsync(input.Id);

            var configuration = _configurationProvider.Get(file.FileContainerName);

            fileDomainService.CheckFileSize(new Dictionary<string, long> { { input.FileName, input.Content?.LongLength ?? 0 } }, configuration);
            fileDomainService.CheckFileExtension(new[] { input.FileName }, configuration);

            await fileDomainService.UpdateFileEntityAsync(file, new UpdateFileInfo()
            {
                FileName = input.FileName,
                MimeType = input.MimeType,
                Content = input.Content,
            });


            await Repository.UpdateAsync(file);

            await fileDomainService.TrySaveBlobAsync(file, input.Content, configuration.DisableBlobReuse,
                configuration.AllowBlobOverriding);

            return ObjectMapper.Map<FileBriefDto>(file);
        }

        public virtual async Task<FileBriefDto> UpdateWithStreamAsync(UpdateFileWithStreamInput input)
        {
            var file = await Repository.GetAsync(input.Id);

            var configuration = _configurationProvider.Get(file.FileContainerName);

            fileDomainService.CheckFileSize(new Dictionary<string, long> { { input.Content.FileName, input.Content?.ContentLength ?? 0 } }, configuration);
            fileDomainService.CheckFileExtension(new[] { input.Content.FileName }, configuration);

            var fileContent = input.Content.GetStream().GetAllBytes();

            await fileDomainService.UpdateFileEntityAsync(file, input.Content.FileName, input.Content.ContentType, fileContent);


            await Repository.UpdateAsync(file);

            await fileDomainService.TrySaveBlobAsync(file, fileContent, configuration.DisableBlobReuse,
                configuration.AllowBlobOverriding);

            return ObjectMapper.Map<FileBriefDto>(file);
        }


        public virtual async Task<FileBriefDto> UpdateInfoAsync(UpdateFileInfoInput input)
        {
            var fileName = input.FileName;

            var file = await Repository.GetAsync(input.Id);

            var configuration = _configurationProvider.Get(file.FileContainerName);

            fileDomainService.CheckFileExtension(new[] { fileName }, configuration);

            var parent = await fileDomainService.TryGetEntityByNullableIdAsync(file.ParentId);

            await _fileManager.ChangeAsync(file, fileName, parent, parent);



            await Repository.UpdateAsync(file);

            return ObjectMapper.Map<FileBriefDto>(file);
        }

        public virtual Task<PublicFileContainerConfiguration> GetConfigurationAsync(string fileContainerName,
            long? ownerUserId)
        {
            return Task.FromResult(
                ObjectMapper.Map<PublicFileContainerConfiguration>(
                    _configurationProvider.Get(fileContainerName)));
        }



        [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = long.MaxValue)]
        [RequestSizeLimit(long.MaxValue)]

        public virtual async Task<CreateFileOutput> ActionCreateAsync([FromForm] CreateFileActionInput input)
        {
            if (input.ParentId == null && !string.IsNullOrEmpty(input.ParentPath))
            {
                var entity = await this.fileDomainService.CreateDirectoryByPathAsync(new CreateFileInfoBase()
                {
                    FileContainerName = input.FileContainerName,
                    ParentId = input.ParentId,
                    ParentPath = input.ParentPath,
                    OwnerUserId = input.OwnerUserId,

                });
                input.ParentId = entity.Id;
            }

            if (input.File == null)
            {
                throw new NoUploadedFileException();
            }

            var fileName = input.GenerateUniqueFileName ? fileDomainService.GenerateUniqueFileName(input.File) : input.File.FileName;

            await using var memoryStream = new System.IO.MemoryStream();

            await input.File.CopyToAsync(memoryStream);

            var fileContent = memoryStream.ToArray();

            var configuration = _configurationProvider.Get(input.FileContainerName);


            fileDomainService.CheckFileSize(new Dictionary<string, long> { { fileName, fileContent.Length } }, configuration);

            fileDomainService.CheckFileExtension(new[] { fileName }, configuration);


            var createFileInput = new CreateFileInput
            {
                FileContainerName = input.FileContainerName,
                FileName = fileName,
                MimeType = input.File.ContentType,
                FileType = input.FileType,
                ParentId = input.ParentId,
                OwnerUserId = input.OwnerUserId,
                Content = fileContent
            };

            var fd = await fileDomainService.CreateAndGetDownloadInfoAsync(new CreateFileInfo()
            {
                FileName = createFileInput.FileName,
                MimeType = createFileInput.MimeType,
                FileType = createFileInput.FileType,
                Content = createFileInput.Content,
                FileContainerName = input.FileContainerName,
                ParentId = input.ParentId,
                ParentPath = input.ParentPath,
                OwnerUserId = input.OwnerUserId,
            });



            return new CreateFileOutput
            {
                FileInfo = ObjectMapper.Map<FileBriefDto>(fd.Item1),
                DownloadInfo = fd.Item2
            };

        }


        [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = long.MaxValue)]
        [RequestSizeLimit(long.MaxValue)]

        public virtual async Task<CreateManyFileOutput> ActionCreateManyAsync([FromForm] CreateManyFileActionInput input)
        {
            if (input.ParentId == null && !string.IsNullOrEmpty(input.ParentPath))
            {
                var entity = await this.fileDomainService.CreateDirectoryByPathAsync(new CreateFileInfoBase()
                {
                    FileContainerName = input.FileContainerName,
                    ParentId = input.ParentId,
                    ParentPath = input.ParentPath,
                    OwnerUserId = input.OwnerUserId,

                });
                input.ParentId = entity.Id;
            }

            if (input.Files.IsNullOrEmpty())
            {
                throw new NoUploadedFileException();
            }

            var createFileDtos = new List<CreateFileInput>();

            foreach (var file in input.Files)
            {
                var fileName = input.GenerateUniqueFileName ? fileDomainService.GenerateUniqueFileName(file) : file.FileName;

                await using var memoryStream = new System.IO.MemoryStream();

                await file.CopyToAsync(memoryStream);

                createFileDtos.Add(new CreateFileInput
                {
                    FileContainerName = input.FileContainerName,
                    FileName = fileName,
                    MimeType = file.ContentType,
                    FileType = input.FileType,
                    ParentId = input.ParentId,
                    OwnerUserId = input.OwnerUserId,
                    Content = memoryStream.ToArray()
                });
            }

            var createManyFileInput = new CreateManyFileInput
            {
                FileInfos = createFileDtos
            };

            var files = await fileDomainService.CreateManyAsync(createManyFileInput.FileInfos.Select(input => new CreateFileInfo()
            {
                FileName = input.FileName,
                MimeType = input.MimeType,
                FileType = input.FileType,
                Content = input.Content,
                FileContainerName = input.FileContainerName,
                ParentId = input.ParentId,
                ParentPath = input.ParentPath,
                OwnerUserId = input.OwnerUserId,



            }).ToArray());


            var items = new List<CreateFileOutput>();

            foreach (var file in files)
            {
                var fd = await fileDomainService.GetFileWithDownloadInfoAsync(file);

                var item = new CreateFileOutput
                {
                    FileInfo = ObjectMapper.Map<FileBriefDto>(fd.Item1),
                    DownloadInfo = fd.Item2
                };
                items.Add(item);
            }

            return new CreateManyFileOutput { Items = items };
        }




        [HttpPut]
        public virtual async Task<FileBriefDto> ActionUpdateAsync([FromForm] UpdateFileActionInput input)
        {
            if (input.File == null)
            {
                throw new NoUploadedFileException();
            }

            await using var memoryStream = new System.IO.MemoryStream();

            await input.File.CopyToAsync(memoryStream);

            var updateDto = new UpdateFileInput
            {
                FileName = input.FileName,
                MimeType = input.File.ContentType,
                Content = memoryStream.ToArray(),
                Id = input.Id,
            };

            return await UpdateAsync(updateDto);
        }


        [HttpGet]
        public virtual async Task<IActionResult> ActionDownloadAsync(FileDownloadInput input)
        {
            var file = await fileDomainService.DownloadAsync(input.Id, input.Token);

            return await _AccessFileAsync(input.Mode, file);

        }


        [HttpGet]
        [Route("api/services/app/File/{mode}/{id:guid}")]

        public virtual async Task<IActionResult> ActionAccessAsync(Guid id, string mode)
        {
            var file = await Repository.GetAsync(id);

            return await _AccessFileAsync(mode, file);


        }

        [HttpGet]
        [Route("api/services/app/File/{mode}/{fileContainerName}/{**path}")]

        public virtual async Task<IActionResult> ActionAccessAsync(string fileContainerName, string path, string mode)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new UserFriendlyException("找不到文件");
            }

            var entity = await this.fileDomainService.GetEntityByPathAsync(path, fileContainerName);
            if (entity == null)
            {
                throw new UserFriendlyException("找不到文件");
            }

            return await _AccessFileAsync(mode, entity);


        }


        private async Task<IActionResult> _AccessFileAsync(string mode, File file)
        {
            var dto = new FileDownloadOutput
            {
                FileName = file.FileName,
                MimeType = file.MimeType,
                Content = await _fileManager.GetBlobAsync(file)
            };

            if (mode == "stream")
            {
                var memoryStream = new System.IO.MemoryStream(dto.Content);

                return new FileStreamResult(memoryStream, dto.MimeType)
                {
                    FileDownloadName = dto.FileName
                };
            }
            else if (mode == "content")
            {
                return new FileContentResult(dto.Content, dto.MimeType);

            }
            else
            {
                throw new UserFriendlyException("请指定下载模式");
            }
        }


        [HttpGet]

        public virtual async Task<IActionResult> ActionGetThumbnailImageAsync(Guid id)
        {
            var file = await Repository.GetAsync(id);

            var fileThumbnailBlobName = file.BlobName + "_thumbnail";

            var fileThumbnail = new File(null, file.FileContainerName, file.FileName, MimeTypeNames.ImageJpeg, FileType.RegularFile,
                0, 0, null, fileThumbnailBlobName, file.OwnerUserId, null);

            var content = await _fileManager.GetBlobAsync(fileThumbnail);
            return new FileContentResult(content, file.MimeType);
        }



        public virtual async Task CreateLocalAsync(CreateLocalInput input)
        {
            if (input.ParentId == null && !string.IsNullOrEmpty(input.ParentPath))
            {
                var entity = await this.fileDomainService.CreateDirectoryByPathAsync(new CreateFileInfoBase()
                {
                    FileContainerName = input.FileContainerName,
                    ParentId = input.ParentId,
                    ParentPath = input.ParentPath,
                    OwnerUserId = input.OwnerUserId,

                });
                input.ParentId = entity.Id;
            }
            Task.Run(async () =>
             {
                 await fileDomainService.CreateLocal(input.ImportDir, input.GenerateUniqueFileName, input.ExclusionDirs, new CreateFileInfoBase()
                 {
                     FileContainerName = input.FileContainerName,
                     ParentId = input.ParentId,
                     ParentPath = input.ParentPath,
                     OwnerUserId = input.OwnerUserId,
                 });
             });


        }


        private async Task<(List<File>, int)> _GetAllFileAsync(GetFileListInput input)
        {
            if (input.ParentId == null && !string.IsNullOrEmpty(input.ParentPath))
            {
                var entity = await this.fileDomainService.GetEntityByPathAsync(input.ParentPath, input.FileContainerName);
                if (entity == null || entity.FileType != FileType.Directory)
                {
                    throw new UserFriendlyException("解析文件夹路径错误");

                }
                input.ParentId = entity.Id;
            }

            if (input.IncludeTrash && input.ParentId.HasValue)
            {
                throw new UserFriendlyException("不能访问已删除的目录，请恢复后再打开");
            }


            var query = CreateFilteredQuery(input);

            var totalCount = await query.CountAsync();

            query = ApplyPaging(query, input);

            var entities = await query.ToListAsync();

            return (entities, totalCount);

        }

        protected IQueryable<File> CreateFilteredQuery(GetFileListInput input)
        {
            if (input.ParentId.HasValue)
            {
                var folder = Repository.GetAll()
                               .Where(x => x.Id == input.ParentId)
                               .Where(x => x.FileContainerName == input.FileContainerName).FirstOrDefault();
                if (folder == null)
                {
                    throw new UserFriendlyException("找不到目录");
                }
            }
            if (input.IncludeTrash)
            {
                return Repository.GetAll()
     .Where(x => x.FileContainerName == input.FileContainerName)
     .WhereIf(input.DirectoryOnly, x => x.FileType == FileType.Directory)
     .WhereIf(input.ParentId.HasValue, c => c.ParentId == input.ParentId)
     .Where(c => Repository.GetAll().Where(d => d.Id == c.ParentId).Count() == 0)
     .OrderBy(x => x.FileType)
     .ThenBy(x => x.FileName);
            }
            else
            {
                return Repository.GetAll()
    .Where(x => x.ParentId == input.ParentId
    && x.FileContainerName == input.FileContainerName)
    .WhereIf(input.DirectoryOnly, x => x.FileType == FileType.Directory)
    .OrderBy(x => x.FileType)
    .ThenBy(x => x.FileName);
            }

        }




        /// <summary>
        /// Should apply paging if needed.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="input">The input.</param>
        protected virtual IQueryable<File> ApplyPaging(IQueryable<File> query, GetFileListInput input)
        {
            //Try to use paging if available
            var pagedInput = input as IPagedResultRequest;
            if (pagedInput != null)
            {
                return query.PageBy(pagedInput);
            }

            //Try to limit query result if available
            var limitedInput = input as ILimitedResultRequest;
            if (limitedInput != null)
            {
                return query.Take(limitedInput.MaxResultCount);
            }

            //No paging
            return query;
        }


    }
}
