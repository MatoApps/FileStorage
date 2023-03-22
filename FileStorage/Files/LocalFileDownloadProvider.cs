using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Abp.Extensions;
using Abp.Dependency;
using FileStorage.Exceptions;
using FileStorage.Cache;
using FileStorage.Configuration;
using FileStorage.Interfaces;
using FileStorage.Models;

namespace FileStorage.Files
{
    public class LocalFileDownloadProvider : IFileDownloadProvider, ISingletonDependency
    {
        public const string DownloadMethod = "Local";

        // Todo: should be a configuration
        public static TimeSpan TokenCacheDuration = TimeSpan.FromMinutes(1);

        public static string BasePath = "api/services/app/File/ActionDownload";

        private readonly LocalFileDownloadOptions _options;
        private readonly LocalFileDownloadCache localFileDownloadCache;

        public LocalFileDownloadProvider(
            IOptions<LocalFileDownloadOptions> options,
            LocalFileDownloadCache localFileDownloadCache)
        {
            _options = options.Value;
            this.localFileDownloadCache = localFileDownloadCache;
        }

        public virtual async Task<FileDownloadInfoModel> CreateDownloadInfoAsync(File file)
        {
            var token = Guid.NewGuid().ToString("N");

            await localFileDownloadCache.SetAsync(token,
                new LocalFileDownloadCacheItem { FileId = file.Id },
                absoluteExpireTime: DateTimeOffset.Now.Add(TokenCacheDuration));

            var url = BasePath + $"?token={token}&id={file.Id}&mode=stream";
            var res = BasePath + $"?token={token}&id={file.Id}&mode=content";

            return new FileDownloadInfoModel
            {
                DownloadMethod = DownloadMethod,
                DownloadUrl = url,
                ResUrl = res,
                ExpectedFileName = file.FileName,
                Token = token
            };
        }

        public virtual async Task CheckTokenAsync(string token, Guid fileId)
        {
            var cacheItem = await localFileDownloadCache.GetAsync(token, null);

            if (cacheItem == null || cacheItem.FileId != fileId)
            {
                throw new LocalFileDownloadInvalidTokenException();
            }
        }
    }
}