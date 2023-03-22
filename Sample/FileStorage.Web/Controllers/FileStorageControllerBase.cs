using Abp.AspNetCore.Mvc.Controllers;

namespace FileStorage.Web.Controllers
{
    public abstract class FileStorageControllerBase: AbpController
    {
        protected FileStorageControllerBase()
        {
            LocalizationSourceName = FileStorageConsts.LocalizationSourceName;
        }
    }
}