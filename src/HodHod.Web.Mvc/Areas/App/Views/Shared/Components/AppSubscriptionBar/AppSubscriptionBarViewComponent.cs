using System.Threading.Tasks;
using Abp.Configuration;
using Microsoft.AspNetCore.Mvc;
using HodHod.Configuration;
using HodHod.Web.Areas.App.Models.Layout;
using HodHod.Web.Session;
using HodHod.Web.Views;

namespace HodHod.Web.Areas.App.Views.Shared.Components.AppSubscriptionBar;

public class AppSubscriptionBarViewComponent : HodHodViewComponent
{
    private readonly IPerRequestSessionCache _sessionCache;

    public AppSubscriptionBarViewComponent(
        IPerRequestSessionCache sessionCache)
    {
        _sessionCache = sessionCache;
    }

    public async Task<IViewComponentResult> InvokeAsync(string cssClass = "btn btn-icon btn-custom btn-icon-muted btn-active-light btn-active-color-primary w-35px h-35px w-md-40px h-md-40px position-relative")
    {
        var model = new SubscriptionBarViewModel
        {
            LoginInformations = await _sessionCache.GetCurrentLoginInformationsAsync(),
            SubscriptionExpireNotifyDayCount = SettingManager.GetSettingValue<int>(AppSettings.TenantManagement.SubscriptionExpireNotifyDayCount),
            CssClass = cssClass
        };

        return View(model);
    }

}

