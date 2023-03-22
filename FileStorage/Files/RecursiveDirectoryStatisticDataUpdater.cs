using Abp.Dependency;
using Abp.Domain.Uow;
using Abp.Events.Bus.Handlers;
using FileStorage.Interfaces;

namespace FileStorage.Files
{
    public class RecursiveDirectoryStatisticDataUpdater : IEventHandler<SubFileUpdatedEto>, ITransientDependency
    {
        private readonly IFileRepository _fileRepository;

        public RecursiveDirectoryStatisticDataUpdater(
            IFileRepository fileRepository)
        {
            _fileRepository = fileRepository;
        }



        [UnitOfWork(true)]
        public virtual async void HandleEvent(SubFileUpdatedEto eventData)
        {
            var parent = eventData.Parent;

            while (parent != null)
            {
                var statisticData = await _fileRepository.GetSubFilesStatisticDataAsync(parent.Id);

                parent.ForceSetStatisticData(statisticData);

                await _fileRepository.UpdateAsync(parent);

                parent = parent.ParentId.HasValue ? await _fileRepository.FirstOrDefaultAsync(parent.ParentId.Value) : null;
            }
        }
    }
}