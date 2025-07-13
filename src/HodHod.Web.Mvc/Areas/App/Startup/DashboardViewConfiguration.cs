using System.Collections.Generic;
using HodHod.Web.DashboardCustomization;


namespace HodHod.Web.Areas.App.Startup;

public class DashboardViewConfiguration
{
    public Dictionary<string, WidgetViewDefinition> WidgetViewDefinitions { get; } = new Dictionary<string, WidgetViewDefinition>();

    public Dictionary<string, WidgetFilterViewDefinition> WidgetFilterViewDefinitions { get; } = new Dictionary<string, WidgetFilterViewDefinition>();

    public DashboardViewConfiguration()
    {
        var jsAndCssFileRoot = "/Areas/App/Views/CustomizableDashboard/Widgets/";
        var viewFileRoot = "App/Widgets/";

        #region FilterViewDefinitions

        WidgetFilterViewDefinitions.Add(HodHodDashboardCustomizationConsts.Filters.FilterDateRangePicker,
            new WidgetFilterViewDefinition(
                HodHodDashboardCustomizationConsts.Filters.FilterDateRangePicker,
                "~/Areas/App/Views/Shared/Components/CustomizableDashboard/Widgets/DateRangeFilter.cshtml",
                jsAndCssFileRoot + "DateRangeFilter/DateRangeFilter.min.js",
                jsAndCssFileRoot + "DateRangeFilter/DateRangeFilter.min.css")
        );

        //add your filters iew definitions here
        #endregion

        #region WidgetViewDefinitions

        #region TenantWidgets

        WidgetViewDefinitions.Add(HodHodDashboardCustomizationConsts.Widgets.Tenant.DailySales,
            new WidgetViewDefinition(
                HodHodDashboardCustomizationConsts.Widgets.Tenant.DailySales,
                viewFileRoot + "DailySales",
                jsAndCssFileRoot + "DailySales/DailySales.min.js",
                jsAndCssFileRoot + "DailySales/DailySales.min.css"));

        WidgetViewDefinitions.Add(HodHodDashboardCustomizationConsts.Widgets.Tenant.GeneralStats,
            new WidgetViewDefinition(
                HodHodDashboardCustomizationConsts.Widgets.Tenant.GeneralStats,
                viewFileRoot + "GeneralStats",
                jsAndCssFileRoot + "GeneralStats/GeneralStats.min.js",
                jsAndCssFileRoot + "GeneralStats/GeneralStats.min.css"));

        WidgetViewDefinitions.Add(HodHodDashboardCustomizationConsts.Widgets.Tenant.ProfitShare,
            new WidgetViewDefinition(
                HodHodDashboardCustomizationConsts.Widgets.Tenant.ProfitShare,
                viewFileRoot + "ProfitShare",
                jsAndCssFileRoot + "ProfitShare/ProfitShare.min.js",
                jsAndCssFileRoot + "ProfitShare/ProfitShare.min.css"));

        WidgetViewDefinitions.Add(HodHodDashboardCustomizationConsts.Widgets.Tenant.MemberActivity,
            new WidgetViewDefinition(
                HodHodDashboardCustomizationConsts.Widgets.Tenant.MemberActivity,
                viewFileRoot + "MemberActivity",
                jsAndCssFileRoot + "MemberActivity/MemberActivity.min.js",
                jsAndCssFileRoot + "MemberActivity/MemberActivity.min.css"));

        WidgetViewDefinitions.Add(HodHodDashboardCustomizationConsts.Widgets.Tenant.RegionalStats,
            new WidgetViewDefinition(
                HodHodDashboardCustomizationConsts.Widgets.Tenant.RegionalStats,
                viewFileRoot + "RegionalStats",
                jsAndCssFileRoot + "RegionalStats/RegionalStats.min.js",
                jsAndCssFileRoot + "RegionalStats/RegionalStats.min.css",
                12,
                10));

        WidgetViewDefinitions.Add(HodHodDashboardCustomizationConsts.Widgets.Tenant.SalesSummary,
            new WidgetViewDefinition(
                HodHodDashboardCustomizationConsts.Widgets.Tenant.SalesSummary,
                viewFileRoot + "SalesSummary",
                jsAndCssFileRoot + "SalesSummary/SalesSummary.min.js",
                jsAndCssFileRoot + "SalesSummary/SalesSummary.min.css",
                6,
                10));

        WidgetViewDefinitions.Add(HodHodDashboardCustomizationConsts.Widgets.Tenant.TopStats,
            new WidgetViewDefinition(
                HodHodDashboardCustomizationConsts.Widgets.Tenant.TopStats,
                viewFileRoot + "TopStats",
                jsAndCssFileRoot + "TopStats/TopStats.min.js",
                jsAndCssFileRoot + "TopStats/TopStats.min.css",
                12,
                10));

        //add your tenant side widget definitions here
        #endregion

        #region HostWidgets

        WidgetViewDefinitions.Add(HodHodDashboardCustomizationConsts.Widgets.Host.IncomeStatistics,
            new WidgetViewDefinition(
                HodHodDashboardCustomizationConsts.Widgets.Host.IncomeStatistics,
                viewFileRoot + "IncomeStatistics",
                jsAndCssFileRoot + "IncomeStatistics/IncomeStatistics.min.js",
                jsAndCssFileRoot + "IncomeStatistics/IncomeStatistics.min.css"));

        WidgetViewDefinitions.Add(HodHodDashboardCustomizationConsts.Widgets.Host.TopStats,
            new WidgetViewDefinition(
                HodHodDashboardCustomizationConsts.Widgets.Host.TopStats,
                viewFileRoot + "HostTopStats",
                jsAndCssFileRoot + "HostTopStats/HostTopStats.min.js",
                jsAndCssFileRoot + "HostTopStats/HostTopStats.min.css"));

        WidgetViewDefinitions.Add(HodHodDashboardCustomizationConsts.Widgets.Host.EditionStatistics,
            new WidgetViewDefinition(
                HodHodDashboardCustomizationConsts.Widgets.Host.EditionStatistics,
                viewFileRoot + "EditionStatistics",
                jsAndCssFileRoot + "EditionStatistics/EditionStatistics.min.js",
                jsAndCssFileRoot + "EditionStatistics/EditionStatistics.min.css"));

        WidgetViewDefinitions.Add(HodHodDashboardCustomizationConsts.Widgets.Host.SubscriptionExpiringTenants,
            new WidgetViewDefinition(
                HodHodDashboardCustomizationConsts.Widgets.Host.SubscriptionExpiringTenants,
                viewFileRoot + "SubscriptionExpiringTenants",
                jsAndCssFileRoot + "SubscriptionExpiringTenants/SubscriptionExpiringTenants.min.js",
                jsAndCssFileRoot + "SubscriptionExpiringTenants/SubscriptionExpiringTenants.min.css",
                6,
                10));

        WidgetViewDefinitions.Add(HodHodDashboardCustomizationConsts.Widgets.Host.RecentTenants,
            new WidgetViewDefinition(
                HodHodDashboardCustomizationConsts.Widgets.Host.RecentTenants,
                viewFileRoot + "RecentTenants",
                jsAndCssFileRoot + "RecentTenants/RecentTenants.min.js",
                jsAndCssFileRoot + "RecentTenants/RecentTenants.min.css"));

        //add your host side widgets definitions here
        #endregion

        #endregion
    }
}

