using Abp.UI;

namespace FileStorage.Exceptions
{
    public class UploadQuantityExceededLimitException : UserFriendlyException
    {
        public UploadQuantityExceededLimitException(long uploadQuantity, long maxQuantity) : base(
            "UploadQuantityExceededLimit",
            $"The quantity of the files ({uploadQuantity}) exceeded the limit: {maxQuantity}.")
        {
        }
    }
}