using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using HodHod.Authentication.TwoFactor.Google;
using HodHod.Authorization;
using HodHod.Authorization.Roles;
using HodHod.Authorization.Users;
using HodHod.Editions;
using HodHod.MultiTenancy;

namespace HodHod.Identity;

public static class IdentityRegistrar
{
    public static IdentityBuilder Register(IServiceCollection services)
    {
        services.AddLogging();

        return services.AddAbpIdentity<Tenant, User, Role>(options =>
            {
                options.Tokens.ProviderMap[GoogleAuthenticatorProvider.Name] = new TokenProviderDescriptor(typeof(GoogleAuthenticatorProvider));
            })
            .AddAbpTenantManager<TenantManager>()
            .AddAbpUserManager<UserManager>()
            .AddAbpRoleManager<RoleManager>()
            .AddAbpEditionManager<EditionManager>()
            .AddAbpUserStore<UserStore>()
            .AddAbpRoleStore<RoleStore>()
            .AddAbpSignInManager<SignInManager>()
            .AddAbpUserClaimsPrincipalFactory<UserClaimsPrincipalFactory>()
            .AddAbpSecurityStampValidator<SecurityStampValidator>()
            .AddPermissionChecker<PermissionChecker>()
            .AddDefaultTokenProviders();
    }
}

