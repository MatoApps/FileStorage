using FileStorage;
using System;

namespace FileStorage.Configuration
{
    public interface IFileContainerConfiguration : IPublicFileContainerConfiguration
    {
        string AbpBlobContainerName { get; set; }
        string AbpBlobDirectorySeparator { get; set; }
        bool AllowBlobOverriding { get; set; }
        bool DisableBlobReuse { get; set; }
        bool RetainUnusedBlobs { get; set; }
        Type SpecifiedFileDownloadProviderType { get; set; }
        string LocalBlobPath { get; set; }

    }
}