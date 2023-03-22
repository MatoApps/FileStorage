using Abp.UI;

namespace FileStorage.Exceptions
{
    public class FileIsMovedToSubDirectoryException : UserFriendlyException
    {
        public FileIsMovedToSubDirectoryException() : base(
            message: "A directory cannot be moved from a directory to one of its sub directories.")
        {
        }
    }
}