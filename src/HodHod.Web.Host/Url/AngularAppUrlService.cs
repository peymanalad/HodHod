﻿using Abp.MultiTenancy;
using HodHod.Url;

namespace HodHod.Web.Url;

public class AngularAppUrlService : AppUrlServiceBase
{
    public override string EmailActivationRoute => "account/confirm-email";

    public override string EmailChangeRequestRoute => "account/change-email";

    public override string PasswordResetRoute => "account/reset-password";

    public AngularAppUrlService(
            IWebUrlService webUrlService,
            ITenantCache tenantCache
        ) : base(
            webUrlService,
            tenantCache
        )
    {

    }
}

