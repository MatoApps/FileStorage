using System.IO;
using System.Threading;
using Abp;
using JetBrains.Annotations;

namespace FileStorage.Blob
{

    public class BlobProviderSaveArgs : BlobProviderArgs
    {
        [NotNull]
        public Stream BlobStream { get; }

        public bool OverrideExisting { get; }

        public BlobProviderSaveArgs(
            [NotNull] string containerName,
            [NotNull] string blobName,
            [NotNull] Stream blobStream,
            bool overrideExisting = false,
            CancellationToken cancellationToken = default)
            : base(
                containerName,
                blobName,
                cancellationToken)
        {
            BlobStream = Check.NotNull(blobStream, nameof(blobStream));
            OverrideExisting = overrideExisting;
        }
    }
}