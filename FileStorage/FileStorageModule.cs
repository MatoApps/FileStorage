using Abp.Modules;
using Abp.Reflection.Extensions;
using FileStorage.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System.IO;
using System.Runtime.InteropServices;

namespace FileStorage
{
    public class FileStorageModule : AbpModule
    {
        private readonly IConfigurationRoot _appConfiguration;
        public FileStorageModule(IHostEnvironment env)
        {
            _appConfiguration = AppConfigurations.Get(
typeof(FileStorageModule).GetAssembly().GetDirectoryPathOrNull(), env.EnvironmentName, env.IsDevelopment()
);
        }
        public override void PreInitialize()
        {
            IocManager.Register<IFileContainerConfiguration, FileContainerConfiguration>();


            Configuration.Modules.Configure(container =>
            {
                // private container never be used by non-owner users (except user who has the "File.Manage" permission).
                container.FileContainerType = FileContainerType.Public;
                container.AbpBlobDirectorySeparator = _appConfiguration["FileStorage:DirectorySeparator"];

                container.RetainUnusedBlobs =  _appConfiguration.GetSection("FileStorage:RetainUnusedBlobs").Get<bool>();
                container.EnableAutoRename = _appConfiguration.GetSection("FileStorage:EnableAutoRename").Get<bool>();

                container.MaxByteSizeForEachFile = _appConfiguration.GetSection("FileStorage:MaxByteSizeForEachFile").Get<long>()   * 1024 * 1024;
                container.MaxByteSizeForEachUpload = _appConfiguration.GetSection("FileStorage:MaxByteSizeForEachUpload").Get<long>()  * 1024 * 1024;
                container.MaxFileQuantityForEachUpload = _appConfiguration.GetSection("FileStorage:MaxFileQuantityForEachUpload").Get<int>();

                container.AllowOnlyConfiguredFileExtensions = _appConfiguration.GetSection("FileStorage:AllowOnlyConfiguredFileExtensions").Get<bool>();

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    container.LocalBlobPath=Path.Combine("D:/", "my-files");
                }
                else
                {
                    container.LocalBlobPath=Path.Combine("/", "my-files");
                }


                var fileExtensionsAllowList = _appConfiguration["FileStorage:FileExtensionsConfiguration"].Split(',');

                foreach (var fileExtension in fileExtensionsAllowList)
                {
                    container.FileExtensionsConfiguration.Add(fileExtension, true);
                }

                container.GetDownloadInfoTimesLimitEachUserPerMinute = _appConfiguration.GetSection("FileStorage:GetDownloadInfoTimesLimitEachUserPerMinute").Get<int>();
            });

        }
        public override void Initialize()
        {
            var thisAssembly = typeof(FileStorageModule).GetAssembly();

            IocManager.RegisterAssemblyByConvention(thisAssembly);

        }


    }
}
