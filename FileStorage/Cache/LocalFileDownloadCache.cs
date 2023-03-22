using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp.Dependency;
using FileStorage.Files;

namespace FileStorage.Cache
{
    public class LocalFileDownloadCache : MemoryCacheBase<LocalFileDownloadCacheItem>, ISingletonDependency
    {
        public LocalFileDownloadCache() : base(nameof(LocalFileDownloadCache))
        {

        }
    }
}
