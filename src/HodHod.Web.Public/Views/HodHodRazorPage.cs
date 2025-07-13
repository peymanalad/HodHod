using Abp.AspNetCore.Mvc.Views;
using Abp.Runtime.Session;
using Microsoft.AspNetCore.Mvc.Razor.Internal;

namespace HodHod.Web.Public.Views;

public abstract class HodHodRazorPage<TModel> : AbpRazorPage<TModel>
{
    [RazorInject]
    public IAbpSession AbpSession { get; set; }

    protected HodHodRazorPage()
    {
        LocalizationSourceName = HodHodConsts.LocalizationSourceName;
    }
}

