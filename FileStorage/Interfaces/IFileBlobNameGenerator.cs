using System.Threading.Tasks;
using FileStorage.Enums;
using FileStorage.Files;
using File = FileStorage.Files.File;

namespace FileStorage.Interfaces
{
    public interface IFileBlobNameGenerator
    {
        Task<string> CreateAsync(FileType fileType, string fileName, File parent, string mimeType, string directorySeparator);
    }
}