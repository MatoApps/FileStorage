using System.Threading;
using JetBrains.Annotations;

namespace FileStorage.Blob
{

    public class BlobProviderExistsArgs : BlobProviderArgs
    {
        public BlobProviderExistsArgs(
            [NotNull] string containerName,
            [NotNull] string blobName,
            CancellationToken cancellationToken = default)
        : base(
            containerName,
            blobName,
            cancellationToken)
        {
        }
    }
}