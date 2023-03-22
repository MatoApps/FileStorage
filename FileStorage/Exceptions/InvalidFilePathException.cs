using Abp.UI;

namespace FileStorage.Exceptions
{
    public class InvalidFilePathException : UserFriendlyException
    {
        public InvalidFilePathException(string filePath) : base("InvalidFilePath",
            $"The file path {filePath} is invalid.")
        {
        }
    }
}