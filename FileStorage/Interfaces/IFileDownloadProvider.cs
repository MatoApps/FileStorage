using System.Threading.Tasks;
using FileStorage.Files;
using FileStorage.Models;
using File = FileStorage.Files.File;

namespace FileStorage.Interfaces
{
    public interface IFileDownloadProvider
    {
        Task<FileDownloadInfoModel> CreateDownloadInfoAsync(File file);
    }
}