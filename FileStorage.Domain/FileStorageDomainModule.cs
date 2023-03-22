using Abp.Modules;
using Abp.Reflection.Extensions;
using FileStorage.Configuration;
using FileStorage.Domain.FileHandler;
using FileStorage.Uow;

namespace FileStorage
{
    [DependsOn(typeof(FileStorageModule))]
    public class FileStorageDomainModule : AbpModule
    {
        private IFileHandlerConfiguration fileHandlerConfiguration;

        public override void Initialize()
        {
            var thisAssembly = typeof(FileStorageDomainModule).GetAssembly();

            IocManager.RegisterAssemblyByConvention(thisAssembly);
            IocManager.Register<IFileHandlerConfiguration, FileHandlerConfiguration>();
        }


        public override void PostInitialize()
        {
            fileHandlerConfiguration = IocManager.Resolve<IFileHandlerConfiguration>();
            fileHandlerConfiguration.Handlers.Add(typeof(ImageFileHandler));
        }
    }
}
