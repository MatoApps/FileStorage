using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Domain.Uow;
using Abp.Events.Bus.Handlers;
using FileStorage.Interfaces;
using FileStorage.Models;

namespace FileStorage.Files
{
    public class FlagFileEventHandler : IEventHandler<FlagFileEto>, ITransientDependency
    {
        private readonly IFileRepository _fileRepository;

        public FlagFileEventHandler(IFileRepository fileRepository)
        {
            _fileRepository = fileRepository;
        }

        [UnitOfWork(true)]
        public virtual async void HandleEvent(FlagFileEto eventData)
        {
            var file = await _fileRepository.GetAsync(eventData.FileId);

            file.SetFlag(eventData.Flag);

            await _fileRepository.UpdateAsync(file);
        }
    }
}