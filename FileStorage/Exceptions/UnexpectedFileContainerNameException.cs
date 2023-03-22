using Abp.UI;

namespace FileStorage.Exceptions
{
    public class UnexpectedFileContainerNameException : UserFriendlyException
    {
        public UnexpectedFileContainerNameException(string fileContainerName) : base(
            message: $"The FileContainerName ({fileContainerName}) is unexpected.")
        {
        }

        public UnexpectedFileContainerNameException(string fileContainerName, string expectedFileContainerName) : base(
            message: $"The FileContainerName ({fileContainerName}) is unexpected, it should be {expectedFileContainerName}.")
        {
        }
    }
}