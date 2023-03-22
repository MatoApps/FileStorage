using Abp.UI;

namespace FileStorage.Exceptions
{
    public class UploadSizeExceededLimitException : UserFriendlyException
    {
        public UploadSizeExceededLimitException(long uploadByteSize, long maxByteSize) : base(
            "UploadSizeExceededLimit",
            $"The total size of the files ({uploadByteSize}) exceeded the limit: {maxByteSize}.")
        {
        }
    }
}