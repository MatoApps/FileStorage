using Abp.AspNetCore;
using Abp.AspNetCore.Configuration;
using Abp.Modules;
using Abp.Reflection.Extensions;
using FileStorage.Application;
using FileStorage.Configuration;
using FileStorage.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.Configuration;

namespace FileStorage.Web.Startup
{
    [DependsOn(
        typeof(FileStorageApplicationModule), 
        typeof(FileStorageEntityFrameworkCoreModule), 
        typeof(AbpAspNetCoreModule))]
    public class FileStorageWebModule : AbpModule
    {
        private readonly IConfigurationRoot _appConfiguration;

        public FileStorageWebModule(IWebHostEnvironment env)
        {
            _appConfiguration = AppConfigurations.Get(env.ContentRootPath, env.EnvironmentName);
        }

        public override void PreInitialize()
        {
            Configuration.DefaultNameOrConnectionString = _appConfiguration.GetConnectionString("Default");

            Configuration.Navigation.Providers.Add<FileStorageNavigationProvider>();

            Configuration.Modules.AbpAspNetCore()
                .CreateControllersForAppServices(
                    typeof(FileStorageApplicationModule).GetAssembly()
                );
        }

        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(FileStorageWebModule).GetAssembly());
        }

        public override void PostInitialize()
        {
            IocManager.Resolve<ApplicationPartManager>()
                .AddApplicationPartsIfNotAddedBefore(typeof(FileStorageWebModule).Assembly);
        }
    }
}
