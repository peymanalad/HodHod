using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using HodHod.Web.Areas.App.Models.Layout;
using HodHod.Web.Session;
using HodHod.Web.Views;

namespace HodHod.Web.Areas.App.Views.Shared.Themes.Theme11.Components.AppTheme11Footer;

public class AppTheme11FooterViewComponent : HodHodViewComponent
{
    private readonly IPerRequestSessionCache _sessionCache;

    public AppTheme11FooterViewComponent(IPerRequestSessionCache sessionCache)
    {
        _sessionCache = sessionCache;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var footerModel = new FooterViewModel
        {
            LoginInformations = await _sessionCache.GetCurrentLoginInformationsAsync()
        };

        return View(footerModel);
    }
}

