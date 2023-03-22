using System;

namespace FileStorage.Files
{
    public class UserFileDownloadLimitCacheItem
    {
        public int Count { get; set; }

        public DateTime AbsoluteExpiration { get; set; }
    }
}