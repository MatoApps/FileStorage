using Abp.UI;

namespace FileStorage.Exceptions
{
    public class FileSizeExceededLimitException : UserFriendlyException
    {
        public FileSizeExceededLimitException(string fileName, long fileByteSize, long maxByteSize) : base(
            "FileSizeExceededLimit",
            $"The size of the file (name: {fileName}, size: {fileByteSize}) exceeded the limit: {maxByteSize}.")
        {
        }
    }
}