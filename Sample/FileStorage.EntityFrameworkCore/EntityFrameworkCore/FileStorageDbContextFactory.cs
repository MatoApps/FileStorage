using FileStorage.Configuration;
using FileStorage.Web;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace FileStorage.EntityFrameworkCore
{
    /* This class is needed to run EF Core PMC commands. Not used anywhere else */
    public class FileStorageDbContextFactory : IDesignTimeDbContextFactory<FileStorageDbContext>
    {
        public FileStorageDbContext CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<FileStorageDbContext>();
            var configuration = AppConfigurations.Get(WebContentDirectoryFinder.CalculateContentRootFolder());

            DbContextOptionsConfigurer.Configure(
                builder,
                configuration.GetConnectionString("Default")
            );

            return new FileStorageDbContext(builder.Options);
        }
    }
}