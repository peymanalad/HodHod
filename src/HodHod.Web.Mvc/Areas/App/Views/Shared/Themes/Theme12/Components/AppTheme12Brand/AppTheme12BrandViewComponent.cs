﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using HodHod.Web.Areas.App.Models.Layout;
using HodHod.Web.Session;
using HodHod.Web.Views;

namespace HodHod.Web.Areas.App.Views.Shared.Themes.Theme12.Components.AppTheme12Brand;

public class AppTheme12BrandViewComponent : HodHodViewComponent
{
    private readonly IPerRequestSessionCache _sessionCache;

    public AppTheme12BrandViewComponent(IPerRequestSessionCache sessionCache)
    {
        _sessionCache = sessionCache;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var headerModel = new HeaderViewModel
        {
            LoginInformations = await _sessionCache.GetCurrentLoginInformationsAsync()
        };

        return View(headerModel);
    }
}

