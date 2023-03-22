using Abp.UI;

namespace FileStorage.Exceptions
{
    public class NoUploadedFileException : UserFriendlyException
    {
        public NoUploadedFileException() : base("NoUploadedFile")
        {

        }
    }
}