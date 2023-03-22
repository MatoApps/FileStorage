using System.Threading.Tasks;
using JetBrains.Annotations;
using Abp.Dependency;
using Abp.Domain.Uow;
using Abp.Events.Bus.Entities;
using Abp.Events.Bus.Handlers;
using FileStorage.Enums;
using FileStorage.Models;
using FileStorage.Interfaces;
using FileStorage.Configuration;
using Abp.Domain.Entities;
using Abp.Domain.Repositories;

namespace FileStorage.Files
{
    public class UnusedBlobCleaner :
        IEventHandler<EntityDeletedEventData<FileEto>>,
        IEventHandler<FileBlobNameChangedEto>,
        ITransientDependency
    {
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly IFileManager _fileManager;
        private readonly IFileRepository _fileRepository;
        private readonly IFileContainerConfigurationProvider _configurationProvider;

        public UnusedBlobCleaner(
            IUnitOfWorkManager unitOfWorkManager,

            IFileManager fileManager,
            IFileRepository fileRepository,
            IFileContainerConfigurationProvider configurationProvider)
        {
            this._unitOfWorkManager=unitOfWorkManager;
            _fileManager = fileManager;
            _fileRepository = fileRepository;
            _configurationProvider = configurationProvider;
        }

        [UnitOfWork]
        public virtual async void HandleEvent(EntityDeletedEventData<FileEto> eventData)
        {

            await TryEnqueueCleaningJobAsync(eventData.Entity.FileType, eventData.Entity.FileContainerName,
                eventData.Entity.BlobName);
        }

        [UnitOfWork]
        public virtual async void HandleEvent(FileBlobNameChangedEto eventData)
        {

            if (eventData.NewBlobName == eventData.OldBlobName)
            {
                return;
            }

            await TryEnqueueCleaningJobAsync(eventData.FileType, eventData.FileContainerName, eventData.OldBlobName);
        }

        protected virtual async Task TryEnqueueCleaningJobAsync(FileType fileType, [NotNull] string fileContainerName,
            [CanBeNull] string blobName)
        {
            if (fileType is not FileType.RegularFile || blobName is null)
            {
                return;
            }

            if (_configurationProvider.Get(fileContainerName).RetainUnusedBlobs)
            {
                return;
            }

            // This handler will be invoked always after the UOW is completed, so delete the BLOB here.
            // See: https://github.com/abpframework/abp/issues/11100
            using (_unitOfWorkManager.Current.DisableFilter(AbpDataFilters.SoftDelete))
            {
                var currentFile = await _fileRepository.FirstOrDefaultAsync(fileContainerName, blobName);

                if (await _fileRepository.FirstOrDefaultAsync(fileContainerName, blobName) == null)
                {
                    await _fileManager.DeleteBlobAsync(fileContainerName, blobName);
                }
            }
        }
    }
}