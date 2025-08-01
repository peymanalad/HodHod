﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Application.Navigation;
using Abp.Runtime.Session;
using Microsoft.AspNetCore.Mvc;
using HodHod.MultiTenancy;
using HodHod.Web.Areas.App.Models.Layout;
using HodHod.Web.Areas.App.Startup;
using HodHod.Web.Views;

namespace HodHod.Web.Areas.App.Views.Shared.Components.AppMenu;

public class AppMenuViewComponent : HodHodViewComponent
{
    private readonly IUserNavigationManager _userNavigationManager;
    private readonly IAbpSession _abpSession;
    private readonly TenantManager _tenantManager;

    public AppMenuViewComponent(
        IUserNavigationManager userNavigationManager,
        IAbpSession abpSession,
        TenantManager tenantManager)
    {
        _userNavigationManager = userNavigationManager;
        _abpSession = abpSession;
        _tenantManager = tenantManager;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="isLeftMenuUsed">set to true for rendering left aside menu</param>
    /// <param name="iconMenu">set to render main menu items as icons. Only valid for left menu</param>
    /// <param name="currentPageName">Name of the current pagae</param>
    /// <param name="height">height of the menu</param>
    /// <param name="sideMenuClass">Css class of side menu</param>
    /// <param name="topMenuClass">Css class of top menu</param>
    /// <returns></returns>
    public async Task<IViewComponentResult> InvokeAsync(
        bool isLeftMenuUsed,
        bool iconMenu = false,
        string currentPageName = null,
        string height = "auto",
        string sideMenuClass = "menu menu-column menu-rounded menu-sub-indention px-3",
        string topMenuClass = "menu menu-rounded menu-column menu-lg-row menu-active-bg menu-title-gray-700 menu-state-primary menu-arrow-gray-400 fw-semibold my-5 my-lg-0 align-items-stretch px-2 px-lg-0")
    {
        var model = new MenuViewModel
        {
            Menu = await _userNavigationManager.GetMenuAsync(AppNavigationProvider.MenuName, _abpSession.ToUserIdentifier()),
            Height = height,
            CurrentPageName = currentPageName,
            IconMenu = iconMenu,
            SideMenuClass = sideMenuClass,
            TopMenuClass = topMenuClass
        };

        if (AbpSession.TenantId == null)
        {
            return GetView(model, isLeftMenuUsed);
        }

        var tenant = await _tenantManager.GetByIdAsync(AbpSession.TenantId.Value);
        if (tenant.EditionId.HasValue)
        {
            return GetView(model, isLeftMenuUsed);
        }

        var subscriptionManagement = FindMenuItemOrNull(model.Menu.Items, AppPageNames.Tenant.SubscriptionManagement);
        if (subscriptionManagement != null)
        {
            subscriptionManagement.IsVisible = false;
        }

        return GetView(model, isLeftMenuUsed);
    }

    public UserMenuItem FindMenuItemOrNull(IList<UserMenuItem> userMenuItems, string name)
    {
        if (userMenuItems == null)
        {
            return null;
        }

        foreach (var menuItem in userMenuItems)
        {
            if (menuItem.Name == name)
            {
                return menuItem;
            }

            var found = FindMenuItemOrNull(menuItem.Items, name);
            if (found != null)
            {
                return found;
            }
        }

        return null;
    }

    private IViewComponentResult GetView(MenuViewModel model, bool isLeftMenuUsed)
    {
        return View(isLeftMenuUsed ? "Default" : "Top", model);
    }
}

