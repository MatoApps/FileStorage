using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Abp.Domain.Services;
using Abp.IO;
using Abp.Runtime.Session;
using FileStorage.Configuration;
using Polly;

namespace FileStorage.Blob
{
    public class BlobProvider : DomainService
    {
        private readonly IFileContainerConfiguration fileContainerConfiguration;

        public IAbpSession AbpSession { get; set; }

        public BlobProvider(IFileContainerConfiguration fileContainerConfiguration)
        {
            AbpSession = NullAbpSession.Instance;
            this.fileContainerConfiguration=fileContainerConfiguration;
        }
        public virtual string Calculate(BlobProviderArgs args)
        {
            var blobPath = fileContainerConfiguration.LocalBlobPath;

            if (!AbpSession.TenantId.HasValue)
            {
                blobPath = Path.Combine(blobPath, "host");
            }
            else
            {
                blobPath = Path.Combine(blobPath, "tenants", AbpSession.GetTenantId().ToString("D"));
            }


            blobPath = Path.Combine(blobPath, args.ContainerName);


            blobPath = Path.Combine(blobPath, args.BlobName);

            return blobPath;
        }
        public async Task SaveAsync(BlobProviderSaveArgs args)
        {
            var filePath = Calculate(args);

            if (!args.OverrideExisting && await ExistsAsync(filePath))
            {
                throw new BlobAlreadyExistsException($"Saving BLOB '{args.BlobName}' does already exists in the container '{args.ContainerName}'! Set {nameof(args.OverrideExisting)} if it should be overwritten.");
            }

            DirectoryHelper.CreateIfNotExists(Path.GetDirectoryName(filePath));

            var fileMode = args.OverrideExisting
                ? FileMode.Create
                : FileMode.CreateNew;

            await Policy.Handle<IOException>()
                .WaitAndRetryAsync(2, retryCount => TimeSpan.FromSeconds(retryCount))
                .ExecuteAsync(async () =>
                {
                    using (var fileStream = File.Open(filePath, fileMode, FileAccess.Write))
                    {
                        await args.BlobStream.CopyToAsync(
                            fileStream,
                            args.CancellationToken
                        );

                        await fileStream.FlushAsync();
                    }
                });
        }

        public Task<bool> DeleteAsync(BlobProviderDeleteArgs args)
        {
            var filePath = Calculate(args);
            try
            {
                FileHelper.DeleteIfExists(filePath);
                return Task.FromResult(true);

            }
            catch (Exception)
            {

                return Task.FromResult(false);
            }
        }

        public Task<bool> ExistsAsync(BlobProviderExistsArgs args)
        {
            var filePath = Calculate(args);
            return ExistsAsync(filePath);
        }

        public async Task<Stream> GetOrNullAsync(BlobProviderGetArgs args)
        {
            var filePath = Calculate(args);

            if (!File.Exists(filePath))
            {
                return null;
            }

            return await Policy.Handle<IOException>()
                .WaitAndRetryAsync(2, retryCount => TimeSpan.FromSeconds(retryCount))
                .ExecuteAsync(async () =>
                {
                    using (var fileStream = File.OpenRead(filePath))
                    {
                        return await TryCopyToMemoryStreamAsync(fileStream, args.CancellationToken);
                    }
                });
        }

        protected async Task<Stream> TryCopyToMemoryStreamAsync(Stream stream, CancellationToken cancellationToken = default)
        {
            if (stream == null)
            {
                return null;
            }

            var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream, cancellationToken);
            memoryStream.Seek(0, SeekOrigin.Begin);
            return memoryStream;
        }


        protected virtual Task<bool> ExistsAsync(string filePath)
        {
            return Task.FromResult(File.Exists(filePath));
        }
    }
}

