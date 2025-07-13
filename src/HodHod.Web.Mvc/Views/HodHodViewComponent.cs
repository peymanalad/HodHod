using Abp.AspNetCore.Mvc.ViewComponents;

namespace HodHod.Web.Views;

public abstract class HodHodViewComponent : AbpViewComponent
{
    protected HodHodViewComponent()
    {
        LocalizationSourceName = HodHodConsts.LocalizationSourceName;
    }
}

