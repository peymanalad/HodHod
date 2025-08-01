﻿using HodHod.DashboardCustomization;
using HodHod.DashboardCustomization.Dto;

namespace HodHod.Web.Areas.App.Models.CustomizableDashboard;

public class CustomizableDashboardViewModel
{
    public DashboardOutput DashboardOutput { get; }

    public Dashboard UserDashboard { get; }

    public CustomizableDashboardViewModel(
        DashboardOutput dashboardOutput,
        Dashboard userDashboard)
    {
        DashboardOutput = dashboardOutput;
        UserDashboard = userDashboard;
    }
}

