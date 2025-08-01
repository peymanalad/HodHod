﻿using System;
using System.Collections.Generic;
using System.Linq;
using Abp.Configuration;
using Abp.Dependency;
using Abp.Extensions;
using Microsoft.AspNetCore.Mvc.Razor;
using HodHod.Configuration;
using HodHod.EntityFrameworkCore;

namespace HodHod.Web.Startup;

/// <summary>
/// That class is generated so that new areas that use default layout can use default components.
/// </summary>
public class RazorViewLocationExpander : IViewLocationExpander
{
    public RazorViewLocationExpander()
    {
    }

    public void PopulateValues(ViewLocationExpanderContext context)
    {
    }

    public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context,
        IEnumerable<string> viewLocations)
    {
        var currentThemeName = GetCurrentTheme();

        var locations = viewLocations.ToList();

        //{0} is like "Components/{componentname}/{viewname}"
        locations.Add("~/Areas/App/Views/Shared/{0}.cshtml");
        locations.Add("~/Areas/App/Views/Shared/Themes/" + currentThemeName.ToPascalCase() +
                      "/{0}.cshtml");

        return locations;
    }

    private string GetCurrentTheme()
    {
        try
        {
            return IocManager.Instance.Resolve<ISettingManager>()
                .GetSettingValue(AppSettings.UiManagement.Theme);
        }
        catch (Exception e)
        {
            return AppConsts.ThemeDefault;
        }
    }
}