using Abp.Dependency;
using File = FileStorage.Files.File;

namespace FileStorage.Domain.FileHandler
{
    public interface IFileHandler : ITransientDependency
    {
        Task Handler(File file, byte[] fileContent);
    }
}