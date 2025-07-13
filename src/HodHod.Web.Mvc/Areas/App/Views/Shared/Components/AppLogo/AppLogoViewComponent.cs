using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using HodHod.Web.Areas.App.Models.Layout;
using HodHod.Web.Session;
using HodHod.Web.Views;

namespace HodHod.Web.Areas.App.Views.Shared.Components.AppLogo;

public class AppLogoViewComponent : HodHodViewComponent
{
    private readonly IPerRequestSessionCache _sessionCache;

    public AppLogoViewComponent(
        IPerRequestSessionCache sessionCache
    )
    {
        _sessionCache = sessionCache;
    }

    public async Task<IViewComponentResult> InvokeAsync(string logoSkin = null, string logoClass = "")
    {
        var headerModel = new LogoViewModel
        {
            LoginInformations = await _sessionCache.GetCurrentLoginInformationsAsync(),
            LogoSkinOverride = logoSkin,
            LogoClassOverride = logoClass
        };

        return View(headerModel);
    }
}

