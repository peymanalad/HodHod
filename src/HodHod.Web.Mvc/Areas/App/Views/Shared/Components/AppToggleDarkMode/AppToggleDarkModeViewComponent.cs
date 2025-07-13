using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using HodHod.Web.Areas.App.Models.Layout;
using HodHod.Web.Views;

namespace HodHod.Web.Areas.App.Views.Shared.Components.AppToggleDarkMode;

public class AppToggleDarkModeViewComponent : HodHodViewComponent
{
    public Task<IViewComponentResult> InvokeAsync(string cssClass, bool isDarkModeActive)
    {
        return Task.FromResult<IViewComponentResult>(View(new ToggleDarkModeViewModel(cssClass, isDarkModeActive)));
    }
}

