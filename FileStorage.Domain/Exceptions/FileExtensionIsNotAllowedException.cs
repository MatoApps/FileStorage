using Abp;
using Abp.UI;

namespace FileStorage.Exceptions
{
    public class FileExtensionIsNotAllowedException : UserFriendlyException
    {
        public FileExtensionIsNotAllowedException(string fileName) : base(
            "FileExtensionIsNotAllowed",
            $"The extension of {fileName} is not allowed.")
        {
        }
    }
}