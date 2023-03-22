using Microsoft.Extensions.Options;
using Abp.Dependency;

namespace FileStorage.Configuration
{
    public class FileContainerConfigurationProvider : IFileContainerConfigurationProvider, ITransientDependency
    {
        private readonly IFileContainerConfiguration fileContainerConfiguration;

        public FileContainerConfigurationProvider(IFileContainerConfiguration fileContainerConfiguration)
        {
            this.fileContainerConfiguration = fileContainerConfiguration;
        }
        public IFileContainerConfiguration Get(string fileContainerName)
        {
            return fileContainerConfiguration;
        }
    }
}