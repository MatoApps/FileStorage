using System.Threading;
using JetBrains.Annotations;

namespace FileStorage.Blob
{

    public class BlobProviderDeleteArgs : BlobProviderArgs
    {
        public BlobProviderDeleteArgs(
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