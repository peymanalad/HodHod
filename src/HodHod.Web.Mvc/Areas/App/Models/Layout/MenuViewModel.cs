﻿using Abp.Application.Navigation;

namespace HodHod.Web.Areas.App.Models.Layout;

public class MenuViewModel
{
    public UserMenu Menu { get; set; }

    public string Height { get; set; }

    public string CurrentPageName { get; set; }

    public bool IconMenu { get; set; }

    public string SideMenuClass { get; set; }

    public string TopMenuClass { get; set; }
}

