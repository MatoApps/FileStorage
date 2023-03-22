using Abp.Linq.Extensions;
using Microsoft.AspNetCore.Http;
using Abp.Collections.Extensions;
using FileStorage.Enums;
using FileStorage.Configuration;
using Abp.Dependency;
using File = FileStorage.Files.File;
using Microsoft.EntityFrameworkCore;
using Abp.Domain.Services;
using FileStorage.Interfaces;
using FileStorage.Exceptions;
using FileStorage.Files;
using FileStorage.Models;
using Abp.Runtime.Session;
using Abp.MimeTypes;
using Abp.Domain.Uow;
using Nito.AsyncEx;
using FileStorage.Domain.FileHandler;

namespace FileStorage
{
    public class FileDomainService : DomainService
    {
        private readonly IFileManager _fileManager;
        private readonly IFileRepository _repository;
        private readonly IFileContainerConfigurationProvider _configurationProvider;
        private readonly IFileHandlerConfiguration _fileHandlerConfiguration;
        private readonly LocalFileDownloadProvider localFileDownloadProvider;
        private readonly IAbpSession abpSession;
        private readonly IMimeTypeMap mimeTypeMap;
        private readonly IIocResolver iocResolver;
        private readonly AsyncLock directoryUnitOfWorkLock = new AsyncLock();
        public static string BasePath = "api/services/app/File";

        public FileDomainService(
            IFileManager fileManager,
            IFileRepository repository,
            IFileContainerConfigurationProvider configurationProvider,
            IFileHandlerConfiguration fileHandlerConfiguration,
            LocalFileDownloadProvider localFileDownloadProvider,
            IAbpSession abpSession,
            IMimeTypeMap mimeTypeMap,
            IIocResolver iocResolver

            )
        {
            this._fileManager=fileManager;
            this._repository=repository;
            this._configurationProvider=configurationProvider;
            this._fileHandlerConfiguration=fileHandlerConfiguration;
            this.localFileDownloadProvider=localFileDownloadProvider;
            this.abpSession=abpSession;
            this.mimeTypeMap=mimeTypeMap;
            this.iocResolver=iocResolver;
        }

        public virtual async Task<(File, FileDownloadInfoModel)> CreateAndGetDownloadInfoAsync(CreateFileInfo input)
        {
            if (input.ParentId==null && !string.IsNullOrEmpty(input.ParentPath))
            {
                var entity = await this.CreateDirectoryByPathAsync(input);
                input.ParentId=entity.Id;
            }
            var configuration = _configurationProvider.Get(input.FileContainerName);

            CheckFileSize(new Dictionary<string, long> { { input.FileName, input.Content?.LongLength ?? 0 } }, configuration);

            if (input.FileType == FileType.RegularFile)
            {
                CheckFileExtension(new[] { input.FileName }, configuration);
            }

            var file = await CreateFileEntityAsync(input);


            await _repository.InsertAsync(file);

            await TrySaveBlobAsync(file, input.Content, configuration.DisableBlobReuse, configuration.AllowBlobOverriding);

            return await GetFileWithDownloadInfoAsync(file);
        }

        public virtual async Task<(File, FileDownloadInfoModel)> GetFileWithDownloadInfoAsync(File file)
        {
            FileDownloadInfoModel downloadInfoModel = null;
            if (file.FileType == FileType.RegularFile)
            {
                downloadInfoModel = await _fileManager.GetDownloadInfoAsync(file);
                downloadInfoModel.ResUrl=$"{BasePath}/content/{file.FileContainerName}/{await GetEntityPathAsync(file)}";
            }
            else
            {
                //todo:压缩打包下载
            }

            return (file, downloadInfoModel);

        }

        public virtual void CheckFileQuantity(int count, IFileContainerConfiguration configuration)
        {
            if (count > configuration.MaxFileQuantityForEachUpload)
            {
                throw new UploadQuantityExceededLimitException(count, configuration.MaxFileQuantityForEachUpload);
            }
        }

        public virtual void CheckFileSize(Dictionary<string, long> fileNameByteSizeMapping, IFileContainerConfiguration configuration)
        {
            foreach (var pair in fileNameByteSizeMapping.Where(pair => pair.Value > configuration.MaxByteSizeForEachFile))
            {
                throw new FileSizeExceededLimitException(pair.Key, pair.Value, configuration.MaxByteSizeForEachFile);
            }

            var totalByteSize = fileNameByteSizeMapping.Values.Sum();

            if (totalByteSize > configuration.MaxByteSizeForEachUpload)
            {
                throw new UploadSizeExceededLimitException(totalByteSize, configuration.MaxByteSizeForEachUpload);
            }
        }

        public virtual void CheckFileExtension(IEnumerable<string> fileNames, IFileContainerConfiguration configuration)
        {
            foreach (var fileName in fileNames.Where(fileName => !IsFileExtensionAllowed(fileName, configuration)))
            {
                throw new FileExtensionIsNotAllowedException(fileName);
            }
        }

        public virtual bool IsFileExtensionAllowed(string fileName, IFileContainerConfiguration configuration)
        {
            var lowerFileName = fileName.ToLowerInvariant();

            foreach (var pair in configuration.FileExtensionsConfiguration.Where(x => lowerFileName.EndsWith(x.Key.ToLowerInvariant())))
            {
                return pair.Value;
            }

            return !configuration.AllowOnlyConfiguredFileExtensions;
        }
        public virtual async Task<File[]> CreateManyAsync(CreateFileInfo[] fileInfos)
        {

            var configuration = _configurationProvider.Get(fileInfos.First().FileContainerName);

            CheckFileQuantity(fileInfos.Count(), configuration);
            CheckFileSize(fileInfos.ToDictionary(x => x.FileName, x => x.Content?.LongLength ?? 0), configuration);

            CheckFileExtension(
                fileInfos.Where(x => x.FileType == FileType.RegularFile).Select(x => x.FileName).ToList(),
                configuration);

            var files = new File[fileInfos.Count()];

            for (var i = 0; i < fileInfos.Count(); i++)
            {
                var fileInfo = fileInfos[i];

                var file = await CreateFileEntityAsync(fileInfo);


                await _repository.InsertAsync(file);

                files[i] = file;
            }

            for (var i = 0; i < files.Length; i++)
            {
                await TrySaveBlobAsync(files[i], fileInfos[i].Content, configuration.DisableBlobReuse,
                    configuration.AllowBlobOverriding);
            }
            return files;
        }
        public virtual async Task<File?> TryGetEntityByNullableIdAsync(Guid? fileId)
        {
            return fileId.HasValue ? await _repository.GetAsync(fileId.Value) : null;
        }

        public virtual async Task<File> GetEntityByPathAsync(string path, string fileContainerName = "")
        {

            var pathArray = path.Split(ExplorerItem.SpliterChar)
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .ToArray();

            int index = 0;
            async Task<File> GetFile(File parent, int index)
            {
                if (index==pathArray.Length)
                {
                    return parent;
                }
                var rootPath = pathArray[index].Trim();

                var result = await _repository.GetAll()
                    .Where(c => c.FileName==rootPath)
                    .WhereIf(parent!=null, c => c.ParentId==parent.Id)
                    .WhereIf(!string.IsNullOrWhiteSpace(fileContainerName), c => c.FileContainerName==fileContainerName)
                    .FirstOrDefaultAsync();

                if (result==null)
                {
                    return null;
                }

                return await GetFile(result, index+1);

            }

            var rootFolder = await GetFile(null, 0);
            return rootFolder;

        }


        public virtual async Task<string> GetEntityPathAsync(File file, string fileContainerName = "")
        {


            var pathArray = new List<string>();

            async Task GetFile(File currentFile)
            {
                pathArray.Add(currentFile.FileName);
                if (!currentFile.ParentId.HasValue)
                {
                    return;
                }

                var parent = await _repository.GetAll()
                    .Where(c => c.Id==currentFile.ParentId)
                    .WhereIf(!string.IsNullOrWhiteSpace(fileContainerName), c => c.FileContainerName==fileContainerName)
                    .FirstOrDefaultAsync();

                if (parent==null)
                {
                    return;
                }
                await GetFile(parent);

            }

            await GetFile(file);
            pathArray.Reverse();
            var path = string.Join(ExplorerItem.SpliterChar, pathArray);
            return path;

        }

        //[UnitOfWork]
        public virtual async Task<File> CreateDirectoryByPathAsync(CreateFileInfoBase input)
        {
            return await UnitOfWorkManager.WithUnitOfWorkAsync(async () =>
            {

                var path = input.ParentPath;
                var pathArray = path.Split(ExplorerItem.SpliterChar);

                int index = 0;
                async Task<File> GetFile(File parent, int index)
                {
                    if (index==pathArray.Length)
                    {
                        return parent;
                    }
                    var rootPath = pathArray[index].Trim();
                    var fileContainerName = input.FileContainerName;
                    var result = await _repository.GetAll()
                    .Where(c => c.FileName==rootPath)
                    .WhereIf(parent!=null, c => c.ParentId==parent.Id)
                    .WhereIf(!string.IsNullOrWhiteSpace(fileContainerName), c => c.FileContainerName==fileContainerName)
                    .FirstOrDefaultAsync();

                    if (result==null || result.FileType!=FileType.Directory)
                    {

                        var file = await _fileManager.CreateAsync(fileContainerName, input.OwnerUserId, rootPath,
        null, FileType.Directory, parent, null);

                        result =   await _repository.InsertAsync(file);

                        await UnitOfWorkManager.Current.SaveChangesAsync();
                    }

                    return await GetFile(result, index+1);

                }
                using (await directoryUnitOfWorkLock.LockAsync())
                {
                    var rootFolder = await GetFile(null, 0);
                    return rootFolder;

                }
            });
        }


        public virtual string GetDownloadLimitCacheItemKey() => abpSession.UserId.ToString();

        public virtual Task<File> CreateFileEntityAsync(CreateFileInfo input)
        {
            return CreateFileEntityAsync(input.ParentPath, input.ParentId, input.OwnerUserId, input.FileContainerName, input.FileType, input.FileName, input.MimeType, input.Content);
        }

        public virtual async Task<File> CreateFileEntityAsync(string parentPath, Guid? parentId, long? ownerUserId, string fileContainerName, FileType fileType, string fileName, string mimeType, byte[] fileContent, bool generateUniqueFileName = false)
        {

            File? parent;
            if (parentId==null && !string.IsNullOrEmpty(parentPath))
            {
                parent = await this.GetEntityByPathAsync(parentPath, fileContainerName);
            }
            else
            {
                parent = await TryGetEntityByNullableIdAsync(parentId);
            }

            fileName = generateUniqueFileName ? GenerateUniqueFileName(fileName) : fileName;

            var file = await _fileManager.CreateAsync(fileContainerName, ownerUserId, fileName.Trim(),
                mimeType, fileType, parent, fileContent);

            return file;
        }

        public virtual Task UpdateFileEntityAsync(File file, UpdateFileInfo input)
        {
            return UpdateFileEntityAsync(file, input.FileName, input.MimeType, input.Content);
        }

        public virtual async Task UpdateFileEntityAsync(File file, string fileName, string mimeType, byte[] fileContent)
        {
            var parent = await TryGetEntityByNullableIdAsync(file.ParentId);

            await _fileManager.ChangeAsync(file, fileName.Trim(), mimeType, fileContent, parent, parent);


        }
        public virtual FileOperationInfoModel CreateFileOperationInfoModel(File file)
        {
            return new FileOperationInfoModel
            {
                ParentId = file.ParentId,
                FileContainerName = file.FileContainerName,
                OwnerUserId = file.OwnerUserId,
                File = file
            };
        }

        public virtual async Task<File> DownloadAsync(Guid id, string token)
        {
            await localFileDownloadProvider.CheckTokenAsync(token, id);

            var file = await _repository.GetAsync(id);

            return file;
        }

        public virtual string GenerateUniqueFileName(string fileName)
        {
            return Guid.NewGuid().ToString("N") + System.IO.Path.GetExtension(fileName);
        }

        public virtual string GenerateUniqueFileName(IFormFile inputFile)
        {
            return Guid.NewGuid().ToString("N") + System.IO.Path.GetExtension(inputFile.FileName);
        }

        public virtual async Task<bool> TrySaveBlobAsync(File file, byte[] fileContent,
            bool disableBlobReuse = false, bool allowBlobOverriding = false)
        {
            if (file.FileType is not FileType.RegularFile)
            {
                return false;
            }
            await _fileManager.TrySaveBlobAsync(file, fileContent, disableBlobReuse, allowBlobOverriding);
            await this.ProcessHandlers(file, fileContent);
            return true;
        }

        public virtual async Task ProcessHandlers(File file, byte[] fileContent)
        {
            foreach (var handlerType in _fileHandlerConfiguration.Handlers)
            {
                if (ShouldProcessHandlers(file, fileContent))
                {
                    using (var handler = iocResolver.ResolveAsDisposable<IFileHandler>(handlerType))
                    {
                        await handler.Object.Handler(file, fileContent);
                    }
                }
            }
        }

        public virtual bool ShouldProcessHandlers(File file, byte[] fileContent)
        {
            return true;
        }

        public async Task CreateLocal(string importDir, bool generateUniqueFileName, string exclusionDirs, CreateFileInfoBase input)
        {

            async Task WalkDirectoryTreeAsync(DirectoryInfo root, File parent)
            {
                // 参考资料：https://docs.microsoft.com/zh-cn/dotnet/csharp/programming-guide/file-system/how-to-iterate-through-a-directory-tree

                //            fileContainerName: "6"
                //fileName: "测试2"
                //fileType: 1
                //ownerUserId: 20
                //parentId: null
                //CreateFileEntityAsync(input, input.FileType, input.FileName, input.MimeType, input.Content)

                Logger.Info($"正在扫描文件夹：{root.FullName}");

                System.IO.FileInfo[]? files = null;
                DirectoryInfo[]? subDirs = null;

                try
                {
                    files = root.GetFiles("*.*");
                }
                catch (UnauthorizedAccessException e)
                {
                    Logger.Error($"读取文件夹无权限，名称：{root.FullName}", e);
                }
                catch (DirectoryNotFoundException e)
                {
                    Logger.Error($"文件夹不存在，名称：{root.FullName}", e);
                }
                catch (Exception e)
                {
                    Logger.Error($"扫描文件夹出错，名称：{root.FullName}", e);
                }

                if (files != null)
                {
                    foreach (var currentFile in files)
                    {
                        var fileName = currentFile.Name;
                        var postPath = currentFile.DirectoryName!.Replace((string?)importDir, "");

                        var configuration = _configurationProvider.Get(input.FileContainerName);


                        CheckFileExtension(new[] { fileName }, configuration);

                        fileName = generateUniqueFileName ? GenerateUniqueFileName(fileName) : fileName;

                        var mimeType = mimeTypeMap.GetMimeType(fileName);

                        var fileContent = await System.IO.File.ReadAllBytesAsync(currentFile.FullName);

                        var file = await _fileManager.CreateAsync(input.FileContainerName, input.OwnerUserId, fileName.Trim(),
                            mimeType, FileType.RegularFile, parent, fileContent);



                        await _repository.InsertAsync(file);

                        await TrySaveBlobAsync(file, fileContent, configuration.DisableBlobReuse, configuration.AllowBlobOverriding);


                    }
                }

                // Now find all the subdirectories under this directory.
                subDirs = root.GetDirectories();

                foreach (var dirInfo in subDirs)
                {
                    if (!string.IsNullOrEmpty(exclusionDirs)&&exclusionDirs.Contains(dirInfo.Name))
                    {
                        continue;
                    }

                    if (dirInfo.Name.EndsWith(".assets"))
                    {
                        continue;
                    }

                    var subparent = await TryGetEntityByNullableIdAsync(parent.Id);

                    var fileName = dirInfo.Name;

                    fileName = generateUniqueFileName ? GenerateUniqueFileName(fileName) : fileName;

                    var file = await _fileManager.CreateAsync(input.FileContainerName, input.OwnerUserId, fileName.Trim(),
                        null, FileType.Directory, subparent, null);

                    await _repository.InsertAsync(file);

                    await UnitOfWorkManager.Current.SaveChangesAsync();

                    // Resursive call for each subdirectory.
                    await WalkDirectoryTreeAsync(dirInfo, file);
                }


            }

            //            fileContainerName: "6"
            //fileName: "测试2"
            //fileType: 1
            //ownerUserId: 20
            //parentId: null
            //CreateFileEntityAsync(input, input.FileType, input.FileName, input.MimeType, input.Content)
            await UnitOfWorkManager.WithUnitOfWorkAsync(async () =>
           {

               var rootDirInfo = new DirectoryInfo(importDir);

               var parent = await TryGetEntityByNullableIdAsync(input.ParentId);

               var rootFileName = generateUniqueFileName ? GenerateUniqueFileName(rootDirInfo.Name) : rootDirInfo.Name;

               var file = await _fileManager.CreateAsync(input.FileContainerName, input.OwnerUserId, rootFileName.Trim(),
                    null, FileType.Directory, parent, null);

               await _repository.InsertAsync(file);

               await UnitOfWorkManager.Current.SaveChangesAsync();

               await WalkDirectoryTreeAsync(rootDirInfo, file);
           });
        }

    }
}
