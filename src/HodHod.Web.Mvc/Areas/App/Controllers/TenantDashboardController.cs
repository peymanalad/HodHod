using Abp.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc;
using HodHod.Authorization;
using HodHod.DashboardCustomization;
using System.Threading.Tasks;
using HodHod.Web.Areas.App.Startup;

namespace HodHod.Web.Areas.App.Controllers;

[Area("App")]
[AbpMvcAuthorize(AppPermissions.Pages_Tenant_Dashboard)]
public class TenantDashboardController : CustomizableDashboardControllerBase
{
    public TenantDashboardController(DashboardViewConfiguration dashboardViewConfiguration,
        IDashboardCustomizationAppService dashboardCustomizationAppService)
        : base(dashboardViewConfiguration, dashboardCustomizationAppService)
    {

    }

    public async Task<ActionResult> Index()
    {
        return await GetView(HodHodDashboardCustomizationConsts.DashboardNames.DefaultTenantDashboard);
    }
}

