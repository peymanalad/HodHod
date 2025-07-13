using Abp.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc;
using HodHod.Authorization;
using HodHod.Caching;
using HodHod.Web.Areas.App.Models.Maintenance;
using HodHod.Web.Controllers;

namespace HodHod.Web.Areas.App.Controllers;

[Area("App")]
[AbpMvcAuthorize(AppPermissions.Pages_Administration_Host_Maintenance)]
public class MaintenanceController : HodHodControllerBase
{
    private readonly ICachingAppService _cachingAppService;

    public MaintenanceController(ICachingAppService cachingAppService)
    {
        _cachingAppService = cachingAppService;
    }

    public ActionResult Index()
    {
        var model = new MaintenanceViewModel
        {
            Caches = _cachingAppService.GetAllCaches().Items,
            CanClearAllCaches = _cachingAppService.CanClearAllCaches()
        };

        return View(model);
    }
}

