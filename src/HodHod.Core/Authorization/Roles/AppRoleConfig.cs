using Abp.MultiTenancy;
using Abp.Zero.Configuration;

namespace HodHod.Authorization.Roles;

public static class AppRoleConfig
{
    public static void Configure(IRoleManagementConfig roleManagementConfig)
    {
        //Static host roles

        roleManagementConfig.StaticRoles.Add(
            new StaticRoleDefinition(
                StaticRoleNames.Host.Admin,
                MultiTenancySides.Host,
                grantAllPermissionsByDefault: true)
            );

        roleManagementConfig.StaticRoles.Add(
            new StaticRoleDefinition(
                StaticRoleNames.Host.SuperAdmin,
                MultiTenancySides.Host,
                grantAllPermissionsByDefault: true)
        );

        roleManagementConfig.StaticRoles.Add(
            new StaticRoleDefinition(
                StaticRoleNames.Host.ProvinceAdmin,
                MultiTenancySides.Host)
        );

        roleManagementConfig.StaticRoles.Add(
            new StaticRoleDefinition(
                StaticRoleNames.Host.CityAdmin,
                MultiTenancySides.Host)
        );

        //Static tenant roles

        roleManagementConfig.StaticRoles.Add(
            new StaticRoleDefinition(
                StaticRoleNames.Tenants.Admin,
                MultiTenancySides.Tenant,
                grantAllPermissionsByDefault: true)
            );

        roleManagementConfig.StaticRoles.Add(
            new StaticRoleDefinition(
                StaticRoleNames.Tenants.User,
                MultiTenancySides.Tenant)
            );
    }
}

