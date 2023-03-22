namespace FileStorage.Configuration
{
    public interface IFileContainerConfigurationProvider
    {
        IFileContainerConfiguration Get(string fileContainerName);
    }
}