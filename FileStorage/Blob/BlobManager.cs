using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Abp;
using Abp.Domain.Services;
using Abp.Threading;

namespace FileStorage.Blob
{
    public class BlobManager : DomainService
    {
        private const string defaultContainerName = "CahBlobContainer";

        protected string ContainerName { get; set; }


        protected BlobProvider Provider { get; }


        protected ICancellationTokenProvider CancellationTokenProvider { get; }


        public BlobManager(
            BlobProvider provider,
            ICancellationTokenProvider cancellationTokenProvider)
        {
            ContainerName = defaultContainerName;
            Provider = provider;
            CancellationTokenProvider = cancellationTokenProvider;
        }

        public void SetContainerName(string containerName)
        {
            this.ContainerName=containerName;
        }


        public virtual async Task SaveAsync(
            string name,
            Stream stream,
            bool overrideExisting = false,
            CancellationToken cancellationToken = default)
        {

            var CurrentContainerName = await NormalizeName(ContainerName);
            var CurrentBlobName = await NormalizeName(name);



            await Provider.SaveAsync(
                new BlobProviderSaveArgs(
                    CurrentContainerName,
                    CurrentBlobName,
                    stream,
                    overrideExisting,
GetCancellationToken(cancellationToken)
                )
            );

        }

        private CancellationToken GetCancellationToken(CancellationToken cancellationToken)
        {
            return cancellationToken == default || cancellationToken == CancellationToken.None
                        ? CancellationTokenProvider.Token
                        : cancellationToken;
        }

        public virtual async Task<bool> DeleteAsync(
            string name,
            CancellationToken cancellationToken = default)
        {
            var CurrentContainerName = await NormalizeName(ContainerName);
            var CurrentBlobName = await NormalizeName(name);

            return await Provider.DeleteAsync(
                new BlobProviderDeleteArgs(
                    CurrentContainerName,
                    CurrentBlobName,
GetCancellationToken(cancellationToken)
                )
            );

        }

        public virtual async Task<bool> ExistsAsync(
            string name,
            CancellationToken cancellationToken = default)
        {
            var CurrentContainerName = await NormalizeName(ContainerName);
            var CurrentBlobName = await NormalizeName(name);

            return await Provider.ExistsAsync(
                new BlobProviderExistsArgs(
                    CurrentContainerName,
                    CurrentBlobName,
GetCancellationToken(cancellationToken)
                )
            );

        }

        public virtual async Task<Stream> GetAsync(
            string name,
            CancellationToken cancellationToken = default)
        {
            var stream = await GetOrNullAsync(name, cancellationToken);

            if (stream == null)
            {
                //TODO: Consider to throw some type of "not found" exception and handle on the HTTP status side
                throw new AbpException(
                    $"Could not found the requested BLOB '{name}' in the container '{ContainerName}'!");
            }

            return stream;
        }

        public virtual async Task<Stream> GetOrNullAsync(
            string name,
            CancellationToken cancellationToken = default)
        {
            var CurrentContainerName = await NormalizeName(ContainerName);
            var CurrentBlobName = await NormalizeName(name);

            return await Provider.GetOrNullAsync(
                new BlobProviderGetArgs(
                    CurrentContainerName,
                    CurrentBlobName,
GetCancellationToken(cancellationToken)
                )
            );

        }

        private async Task<string> NormalizeName(string fileName)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // A filename cannot contain any of the following characters: \ / : * ? " < > |
                // In order to support the directory included in the blob name, remove / and \
                fileName = Regex.Replace(fileName, "[:\\*\\?\"<>\\|]", string.Empty);
            }
            return fileName;

        }

    }
}

