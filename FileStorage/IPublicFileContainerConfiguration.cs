using System.Collections.Generic;

namespace FileStorage
{
    public interface IPublicFileContainerConfiguration
    {
        bool AllowOnlyConfiguredFileExtensions { get; set; }
        bool EnableAutoRename { get; set; }
        FileContainerType FileContainerType { get; set; }
        Dictionary<string, bool> FileExtensionsConfiguration { get; set; }
        int? GetDownloadInfoTimesLimitEachUserPerMinute { get; set; }
        long MaxByteSizeForEachFile { get; set; }
        long MaxByteSizeForEachUpload { get; set; }
        int MaxFileQuantityForEachUpload { get; set; }
    }
}