﻿using Abp.Application.Navigation;

namespace HodHod.Web.Areas.App.Views.Shared.Components.AppMenu;

public class UserMenuItemViewModel
{
    public UserMenuItem MenuItem { get; set; }

    public string CurrentPageName { get; set; }

    public int MenuItemIndex { get; set; }

    public int ItemDepth { get; set; }

    public bool RootLevel { get; set; }

    public bool IsTabMenuUsed { get; set; }

    public bool IconMenu { get; set; }

    public string GetTriggerCssClass()
    {
        return IconMenu ? "hover" : "click";
    }

    public string GetIconSize()
    {
        return IconMenu ? "fs-2x m-auto" : "";
    }
}

