using System.Threading;
using Abp;
using JetBrains.Annotations;

namespace FileStorage.Blob
{

    public abstract class BlobProviderArgs
    {
        [NotNull]
        public string ContainerName { get; }


        [NotNull]
        public string BlobName { get; }

        public CancellationToken CancellationToken { get; }

        protected BlobProviderArgs(
            [NotNull] string containerName,
            [NotNull] string blobName,
            CancellationToken cancellationToken = default)
        {
            ContainerName = Check.NotNullOrWhiteSpace(containerName, nameof(containerName));
            BlobName = Check.NotNullOrWhiteSpace(blobName, nameof(blobName));
            CancellationToken = cancellationToken;
        }
    }
}