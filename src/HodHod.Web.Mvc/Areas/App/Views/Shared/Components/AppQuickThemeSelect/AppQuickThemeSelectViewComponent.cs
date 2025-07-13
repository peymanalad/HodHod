using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using HodHod.Web.Areas.App.Models.Layout;
using HodHod.Web.Views;

namespace HodHod.Web.Areas.App.Views.Shared.Components.AppQuickThemeSelect;

public class AppQuickThemeSelectViewComponent : HodHodViewComponent
{
    public Task<IViewComponentResult> InvokeAsync(string cssClass, string iconClass = "flaticon-interface-7 fs-2")
    {
        return Task.FromResult<IViewComponentResult>(View(new QuickThemeSelectionViewModel
        {
            CssClass = cssClass,
            IconClass = iconClass
        }));
    }
}

