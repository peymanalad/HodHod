﻿using Abp.AspNetCore.OpenIddict.Claims;
using Abp.AspNetCore.OpenIddict.Controllers;
using Abp.Authorization;
using Abp.Authorization.Users;
using HodHod.Authorization.Roles;
using HodHod.Authorization.Users;
using HodHod.MultiTenancy;
using OpenIddict.Abstractions;

namespace HodHod.Web.OpenIddict.Controllers;

public partial class TokenController : TokenController<Tenant, Role, User>
{
    public TokenController(AbpSignInManager<Tenant, Role, User> signInManager,
        AbpUserManager<Role, User> userManager, IOpenIddictApplicationManager applicationManager,
        IOpenIddictAuthorizationManager authorizationManager, IOpenIddictScopeManager scopeManager,
        IOpenIddictTokenManager tokenManager,
        AbpOpenIddictClaimsPrincipalManager openIddictClaimsPrincipalManager) : base(signInManager, userManager,
        applicationManager, authorizationManager, scopeManager, tokenManager, openIddictClaimsPrincipalManager)
    {
    }
}

