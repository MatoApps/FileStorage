using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileStorage.Enums;
using FileStorage.Files;
using FileStorage.Interfaces;
using FileStorage.Configuration;
using FileStorage.Models;
using File = FileStorage.Files.File;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats;
using FileStorage.Net.MimeTypes;

namespace FileStorage.Domain.FileHandler
{
    public class ImageFileHandler : IFileHandler
    {
        private readonly IFileManager _fileManager;
        private readonly IFileHandlerConfiguration configuration;

        public ImageFileHandler(
            IFileManager fileManager,
            IFileHandlerConfiguration configuration
            )
        {
            _fileManager = fileManager;
            this.configuration = configuration;
        }
        public async Task Handler(File file, byte[] fileContent)
        {
            var lowerFileName = file.FileName.ToLowerInvariant();

            if (configuration.IgnoredTypes.Any(t => lowerFileName.EndsWith(t.ToLowerInvariant())))
            {
                return;
            }

            await TrySaveThumbnailImageAsync(file, fileContent, false, false);
        }



        private async Task<bool> TrySaveThumbnailImageAsync(File file, byte[] fileContent, bool disableBlobReuse, bool allowBlobOverriding)
        {
            if (!_fileManager.GetIsImageFile(file))
            {
                return false;
            }

            var fileThumbnail = GetFileThumbnail(file);

            IImageFormat format;


            using (Image thumbnailImage = Image.Load(fileContent, out format))
            {
                thumbnailImage.Mutate(x => x
                     .Resize(256, 256));
                byte[] thumbnailImageData;
                using (var ms = new MemoryStream())
                {
                    thumbnailImage.Save(ms, format);
                    thumbnailImageData = ms.ToArray();
                }
                await _fileManager.TrySaveBlobAsync(fileThumbnail, thumbnailImageData, disableBlobReuse, allowBlobOverriding);

            }

            return true;
        }


        private static File GetFileThumbnail(File file)
        {
            var fileThumbnailBlobName = file.BlobName + "_thumbnail";

            var fileThumbnail = new File(null, file.FileContainerName, file.FileName, MimeTypeNames.ImageJpeg, FileType.RegularFile,
                0, 0, null, fileThumbnailBlobName, file.OwnerUserId, null);
            return fileThumbnail;
        }


    }
}
