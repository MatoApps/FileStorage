using Abp.Domain.Repositories;
using Abp.EntityFrameworkCore;
using FileStorage.EntityFrameworkCore.Repositories;
using FileStorage.Files;
using Microsoft.EntityFrameworkCore;

namespace FileStorage.EntityFrameworkCore
{
    [AutoRepositoryTypes(
               typeof(IRepository<>),
               typeof(IRepository<,>),
               typeof(FileStorageRepositoryBase<>),
               typeof(FileStorageRepositoryBase<,>))]
    public class FileStorageDbContext : AbpDbContext
    {
        //Add DbSet properties for your entities...
        public DbSet<File> File { get; set; }
        public FileStorageDbContext(DbContextOptions<FileStorageDbContext> options) 
            : base(options)
        {

        }

    }
}
