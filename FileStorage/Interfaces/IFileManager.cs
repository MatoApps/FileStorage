using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Abp.Domain.Services;
using FileStorage.Enums;
using FileStorage.Files;
using FileStorage.Models;
using File = FileStorage.Files.File;

namespace FileStorage.Interfaces
{
    public interface IFileManager : IDomainService
    {
        Task<File> CreateAsync([NotNull] string fileContainerName, long? ownerUserId, [NotNull] string fileName,
            [CanBeNull] string mimeType, FileType fileType, [CanBeNull] File parent, byte[] fileContent);

        Task<File> ChangeAsync(File file, [NotNull] string newFileName, [CanBeNull] File oldParent, [CanBeNull] File newParent);

        Task<File> ChangeAsync(File file, [NotNull] string newFileName, [CanBeNull] string newMimeType,
            byte[] newFileContent, [CanBeNull] File oldParent, [CanBeNull] File newParent);

        Task DeleteAsync(File file, bool isHardDelete = false, CancellationToken cancellationToken = default);

        Task<bool> TrySaveBlobAsync(File file, byte[] fileContent, bool disableBlobReuse = false,
            bool allowBlobOverriding = false, CancellationToken cancellationToken = default);

        Task<byte[]> GetBlobAsync(File file, CancellationToken cancellationToken = default);


        Task DeleteBlobAsync([NotNull] string fileContainerName, [NotNull] string blobName,
            CancellationToken cancellationToken = default);

        Task<FileDownloadInfoModel> GetDownloadInfoAsync(File file);
        bool GetIsImageFile(File file);
        Task<bool> IsFileExistAsync(string fileName, Guid? parentId, string fileContainerName, long? ownerUserId);
        Task RestoreAsync([NotNull] File file, CancellationToken cancellationToken = default);
    }
}