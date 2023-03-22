using Abp.UI;

namespace FileStorage.Exceptions
{
    public class LocalFileDownloadInvalidTokenException : UserFriendlyException
    {
        public LocalFileDownloadInvalidTokenException() : base("LocalFileDownloadInvalidToken",
            "The file download token is not invalid.")
        {
        }
    }
}