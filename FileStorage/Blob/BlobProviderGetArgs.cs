using System.Threading;
using JetBrains.Annotations;

namespace FileStorage.Blob
{

    public class BlobProviderGetArgs : BlobProviderArgs
    {
        public BlobProviderGetArgs(
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