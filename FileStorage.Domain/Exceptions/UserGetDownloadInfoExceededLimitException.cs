using Abp.UI;

namespace FileStorage.Exceptions
{
    public class UserGetDownloadInfoExceededLimitException : UserFriendlyException
    {
        public UserGetDownloadInfoExceededLimitException() : base("UserGetDownloadInfoExceededLimit",
            "The number of times you get download information exceeds the limit.")
        {
        }
    }
}