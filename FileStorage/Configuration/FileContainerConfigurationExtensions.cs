using System;
using Abp.Configuration.Startup;

namespace FileStorage.Configuration
{
    public static class FileContainerConfigurationExtensions
    {
        /// <summary>
        ///     Used to configure ABP FileContainer module.
        /// </summary>
        public static IFileContainerConfiguration FileContainer(this IModuleConfigurations configurations)
        {
            return configurations.AbpConfiguration.Get<IFileContainerConfiguration>();
        }


        public static void Configure(this IModuleConfigurations configurations, Action<IFileContainerConfiguration> p)
        {
            p?.Invoke(configurations.AbpConfiguration.Get<IFileContainerConfiguration>());
        }
    }
}
