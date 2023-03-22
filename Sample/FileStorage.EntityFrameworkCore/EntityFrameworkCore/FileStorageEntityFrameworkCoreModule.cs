using Abp.EntityFrameworkCore;
using Abp.Modules;
using Abp.Reflection.Extensions;

namespace FileStorage.EntityFrameworkCore
{
    [DependsOn(
        typeof(FileStorageCoreModule),
        typeof(FileStorageDomainModule), 
        typeof(AbpEntityFrameworkCoreModule))]
    public class FileStorageEntityFrameworkCoreModule : AbpModule
    {
        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(FileStorageEntityFrameworkCoreModule).GetAssembly());
        }
    }
}