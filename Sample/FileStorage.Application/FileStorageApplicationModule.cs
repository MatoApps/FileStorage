using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Abp.AutoMapper;
using Abp.Modules;
using Abp.Reflection.Extensions;
using FileStorage;

namespace FileStorage.Application
{
    [DependsOn(
        typeof(CAHFileStorageModule)
    )]
    public class FileStorageApplicationModule : AbpModule
    {
        public override void Initialize()
        {
            var thisAssembly = typeof(FileStorageApplicationModule).GetAssembly();

            IocManager.RegisterAssemblyByConvention(thisAssembly);

            Configuration.Modules.AbpAutoMapper().Configurators.Add(
                // Scan the assembly for classes which inherit from AutoMapper.Profile
                cfg => cfg.AddMaps(thisAssembly)
            );
        }
    }
}
