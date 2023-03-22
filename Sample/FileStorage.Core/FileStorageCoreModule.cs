using Abp.Modules;
using Abp.Reflection.Extensions;
using FileStorage.Localization;

namespace FileStorage
{
    public class FileStorageCoreModule : AbpModule
    {
        public override void PreInitialize()
        {
            Configuration.Auditing.IsEnabledForAnonymousUsers = true;

            FileStorageLocalizationConfigurer.Configure(Configuration.Localization);
            
            Configuration.Settings.SettingEncryptionConfiguration.DefaultPassPhrase = FileStorageConsts.DefaultPassPhrase;
        }

        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(FileStorageCoreModule).GetAssembly());
        }
    }
}