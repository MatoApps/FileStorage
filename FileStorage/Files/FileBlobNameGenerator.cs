using System;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Timing;
using FileStorage.Enums;
using FileStorage.Interfaces;

namespace FileStorage.Files
{
    public class FileBlobNameGenerator : IFileBlobNameGenerator, ITransientDependency
    {

        public FileBlobNameGenerator()
        {
        }

        public virtual Task<string> CreateAsync(FileType fileType, string fileName, File parent, string mimeType, string directorySeparator)
        {
            var now = DateTime.Now;

            var blobName = now.Year + directorySeparator + now.Month + directorySeparator + now.Day +
                           directorySeparator + Guid.NewGuid().ToString("N");

            return Task.FromResult(blobName);
        }
    }
}