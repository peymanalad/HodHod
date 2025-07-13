using Abp.AspNetCore.Mvc.Views;

namespace HodHod.Web.Views;

public abstract class HodHodRazorPage<TModel> : AbpRazorPage<TModel>
{
    protected HodHodRazorPage()
    {
        LocalizationSourceName = HodHodConsts.LocalizationSourceName;
    }
}

