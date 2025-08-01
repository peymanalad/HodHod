﻿using Microsoft.AspNetCore.Mvc;
using HodHod.DashboardCustomization;
using HodHod.DashboardCustomization.Dto;
using HodHod.Web.Areas.App.Models.CustomizableDashboard;
using HodHod.Web.Controllers;
using System.Linq;
using System.Threading.Tasks;
using HodHod.Web.Areas.App.Startup;

namespace HodHod.Web.Areas.App.Controllers;

public abstract class CustomizableDashboardControllerBase : HodHodControllerBase
{
    protected readonly IDashboardCustomizationAppService DashboardCustomizationAppService;
    protected readonly DashboardViewConfiguration DashboardViewConfiguration;

    protected CustomizableDashboardControllerBase(
        DashboardViewConfiguration dashboardViewConfiguration,
        IDashboardCustomizationAppService dashboardCustomizationAppService)
    {
        DashboardViewConfiguration = dashboardViewConfiguration;
        DashboardCustomizationAppService = dashboardCustomizationAppService;
    }

    public async Task<PartialViewResult> AddWidgetModal(string dashboardName, string pageId)
    {
        var availableWidgets = await DashboardCustomizationAppService.GetAllAvailableWidgetDefinitionsForPage(
            new GetAvailableWidgetDefinitionsForPageInput()
            {
                Application = HodHodDashboardCustomizationConsts.Applications.Mvc,
                PageId = pageId,
                DashboardName = dashboardName
            });

        var viewModel = new AddWidgetViewModel
        {
            Widgets = availableWidgets,
            DashboardName = dashboardName,
            PageId = pageId
        };

        return PartialView(
            "~/Areas/App/Views/Shared/Components/CustomizableDashboard/_AddWidgetModal.cshtml",
            viewModel
        );
    }

    protected async Task<ActionResult> GetView(string dashboardName)
    {
        var dashboardDefinition = DashboardCustomizationAppService.GetDashboardDefinition(
            new GetDashboardInput
            {
                DashboardName = dashboardName,
                Application = HodHodDashboardCustomizationConsts.Applications.Mvc
            }
        );

        var userDashboard = await DashboardCustomizationAppService.GetUserDashboard(new GetDashboardInput
        {
            DashboardName = dashboardName,
            Application = HodHodDashboardCustomizationConsts.Applications.Mvc
        }
        );

        // Show only view defined widgets
        foreach (var userDashboardPage in userDashboard.Pages)
        {
            userDashboardPage.Widgets = userDashboardPage.Widgets
                .Where(w => DashboardViewConfiguration.WidgetViewDefinitions.ContainsKey(w.WidgetId)).ToList();
        }

        dashboardDefinition.Widgets = dashboardDefinition.Widgets.Where(dw =>
            userDashboard.Pages.Any(p => p.Widgets.Select(w => w.WidgetId).Contains(dw.Id))).ToList();

        return View("~/Areas/App/Views/Shared/Components/CustomizableDashboard/Index.cshtml",
            new CustomizableDashboardViewModel(
                dashboardDefinition,
                userDashboard)
        );
    }
}

