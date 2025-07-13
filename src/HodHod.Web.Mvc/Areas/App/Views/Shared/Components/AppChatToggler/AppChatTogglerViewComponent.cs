using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using HodHod.Web.Areas.App.Models.Layout;
using HodHod.Web.Views;

namespace HodHod.Web.Areas.App.Views.Shared.Components.AppChatToggler;

public class AppChatTogglerViewComponent : HodHodViewComponent
{
    public Task<IViewComponentResult> InvokeAsync(string cssClass, string iconClass = "flaticon-chat-2 fs-4")
    {
        return Task.FromResult<IViewComponentResult>(View(new ChatTogglerViewModel
        {
            CssClass = cssClass,
            IconClass = iconClass
        }));
    }
}

