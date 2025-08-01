﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using HodHod.Web.Areas.App.Models.Layout;
using HodHod.Web.Views;

namespace HodHod.Web.Areas.App.Views.Shared.Components.AppRecentNotifications;

public class AppRecentNotificationsViewComponent : HodHodViewComponent
{
    public Task<IViewComponentResult> InvokeAsync(string cssClass, string iconClass = "flaticon-alert-2 unread-notification fs-2")
    {
        var model = new RecentNotificationsViewModel
        {
            CssClass = cssClass,
            IconClass = iconClass
        };

        return Task.FromResult<IViewComponentResult>(View(model));
    }
}

