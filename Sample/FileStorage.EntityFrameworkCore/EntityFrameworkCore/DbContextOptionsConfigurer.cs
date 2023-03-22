using Microsoft.EntityFrameworkCore;

namespace FileStorage.EntityFrameworkCore
{
    public static class DbContextOptionsConfigurer
    {
        public static void Configure(
            DbContextOptionsBuilder<FileStorageDbContext> dbContextOptions, 
            string connectionString
            )
        {
            /* This is the single point to configure DbContextOptions for FileStorageDbContext */
            dbContextOptions.UseSqlServer(connectionString);
        }
    }
}
