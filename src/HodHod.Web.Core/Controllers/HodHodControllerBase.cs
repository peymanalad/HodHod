﻿using System;
using Abp.AspNetCore.Mvc.Controllers;
using Abp.Configuration.Startup;
using Abp.IdentityFramework;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace HodHod.Web.Controllers;

public abstract class HodHodControllerBase : AbpController
{
    protected HodHodControllerBase()
    {
        LocalizationSourceName = HodHodConsts.LocalizationSourceName;
    }

    protected void CheckErrors(IdentityResult identityResult)
    {
        identityResult.CheckErrors(LocalizationManager);
    }

    protected void SetTenantIdCookie(int? tenantId)
    {
        var multiTenancyConfig = HttpContext.RequestServices.GetRequiredService<IMultiTenancyConfig>();
        Response.Cookies.Append(
            multiTenancyConfig.TenantIdResolveKey,
            tenantId?.ToString() ?? string.Empty,
            new CookieOptions
            {
                Expires = DateTimeOffset.Now.AddYears(5),
                Path = "/"
            }
        );
    }
}

