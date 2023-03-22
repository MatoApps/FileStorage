using Abp.AspNetCore.Mvc.Views;

namespace FileStorage.Web.Views
{
    public abstract class FileStorageRazorPage<TModel> : AbpRazorPage<TModel>
    {
        protected FileStorageRazorPage()
        {
            LocalizationSourceName = FileStorageConsts.LocalizationSourceName;
        }
    }
}
