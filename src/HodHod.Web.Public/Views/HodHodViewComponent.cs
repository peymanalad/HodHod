using Abp.AspNetCore.Mvc.ViewComponents;

namespace HodHod.Web.Public.Views;

public abstract class HodHodViewComponent : AbpViewComponent
{
    protected HodHodViewComponent()
    {
        LocalizationSourceName = HodHodConsts.LocalizationSourceName;
    }
}

