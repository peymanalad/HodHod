﻿using System.Threading.Tasks;
using Abp.AspNetCore.Mvc.Authorization;
using Abp.MultiTenancy;
using Microsoft.AspNetCore.Mvc;
using HodHod.Authorization;
using HodHod.Web.Controllers;

namespace HodHod.Web.Areas.App.Controllers;

[Area("App")]
[AbpMvcAuthorize]
public class HomeController : HodHodControllerBase
{
    public async Task<ActionResult> Index()
    {
        if (AbpSession.MultiTenancySide == MultiTenancySides.Host)
        {
            if (await IsGrantedAsync(AppPermissions.Pages_Administration_Host_Dashboard))
            {
                return RedirectToAction("Index", "HostDashboard");
            }

            if (await IsGrantedAsync(AppPermissions.Pages_Tenants))
            {
                return RedirectToAction("Index", "Tenants");
            }
        }
        else
        {
            if (await IsGrantedAsync(AppPermissions.Pages_Tenant_Dashboard))
            {
                return RedirectToAction("Index", "TenantDashboard");
            }
        }

        //Default page if no permission to the pages above
        return RedirectToAction("Index", "Welcome");
    }
}

