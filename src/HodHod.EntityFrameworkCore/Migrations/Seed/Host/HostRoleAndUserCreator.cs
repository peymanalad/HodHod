using System.Linq;
using Abp;
using Abp.Authorization;
using Abp.Authorization.Roles;
using Abp.Authorization.Users;
using Abp.MultiTenancy;
using Abp.Notifications;
using Microsoft.EntityFrameworkCore;
using HodHod.Authorization;
using HodHod.Authorization.Roles;
using HodHod.Authorization.Users;
using HodHod.EntityFrameworkCore;
using HodHod.Notifications;

namespace HodHod.Migrations.Seed.Host;

public class HostRoleAndUserCreator
{
    private readonly HodHodDbContext _context;

    private static readonly string[] CategoryPermissions =
    {
        AppPermissions.Pages_Administration_Categories,
        AppPermissions.Pages_Administration_Categories_Create,
        AppPermissions.Pages_Administration_Categories_Edit,
        AppPermissions.Pages_Administration_Categories_Delete,
        AppPermissions.Pages_Administration_SubCategories,
        AppPermissions.Pages_Administration_SubCategories_Create,
        AppPermissions.Pages_Administration_SubCategories_Edit,
        AppPermissions.Pages_Administration_SubCategories_Delete
    };

    public HostRoleAndUserCreator(HodHodDbContext context)
    {
        _context = context;
    }

    public void Create()
    {
        CreateHostRoleAndUsers();
    }

    private void GrantPermissionIfNotExists(Role role, string permissionName)
    {
        var exists = _context.Permissions
            .OfType<RolePermissionSetting>()
            .IgnoreQueryFilters()
            .Any(p => p.RoleId == role.Id && p.Name == permissionName);

        if (!exists)
        {
            _context.Permissions.Add(new RolePermissionSetting
            {
                TenantId = role.TenantId,
                Name = permissionName,
                IsGranted = true,
                RoleId = role.Id
            });
        }
    }

    private void RemovePermissions(Role role, string[] permissionNames)
    {
        foreach (var name in permissionNames)
        {
            var permission = _context.Permissions
                .OfType<RolePermissionSetting>()
                .IgnoreQueryFilters()
                .FirstOrDefault(p => p.RoleId == role.Id && p.Name == name);

            if (permission != null)
            {
                _context.Permissions.Remove(permission);
            }
        }
    }

    private void CreateHostRoleAndUsers()
    {
        //Admin role for host

        var adminRoleForHost = _context.Roles.IgnoreQueryFilters().FirstOrDefault(r => r.TenantId == null && r.Name == StaticRoleNames.Host.Admin);
        if (adminRoleForHost == null)
        {
            adminRoleForHost = _context.Roles.Add(new Role(null, StaticRoleNames.Host.Admin, StaticRoleNames.Host.Admin) { IsStatic = true, IsDefault = true }).Entity;
            _context.SaveChanges();
        }


        foreach (var permission in CategoryPermissions)
        {
            GrantPermissionIfNotExists(adminRoleForHost, permission);
        }

        var provinceAdminRole = _context.Roles.IgnoreQueryFilters().FirstOrDefault(r => r.TenantId == null && r.Name == "ProvinceAdmin");
        if (provinceAdminRole != null)
        {
            RemovePermissions(provinceAdminRole, CategoryPermissions);
        }

        var cityAdminRole = _context.Roles.IgnoreQueryFilters().FirstOrDefault(r => r.TenantId == null && r.Name == "CityAdmin");
        if (cityAdminRole != null)
        {
            RemovePermissions(cityAdminRole, CategoryPermissions);
        }

        _context.SaveChanges();


        var superAdminRoleForHost = _context.Roles.IgnoreQueryFilters().FirstOrDefault(r => r.TenantId == null && r.Name == StaticRoleNames.Host.SuperAdmin);
        if (superAdminRoleForHost == null)
        {
            superAdminRoleForHost = _context.Roles.Add(new Role(null, StaticRoleNames.Host.SuperAdmin, StaticRoleNames.Host.SuperAdmin) { IsStatic = true }).Entity;
            _context.SaveChanges();
        }

        var provinceAdminRoleForHost = _context.Roles.IgnoreQueryFilters().FirstOrDefault(r => r.TenantId == null && r.Name == StaticRoleNames.Host.ProvinceAdmin);
        if (provinceAdminRoleForHost == null)
        {
            provinceAdminRoleForHost = _context.Roles.Add(new Role(null, StaticRoleNames.Host.ProvinceAdmin, StaticRoleNames.Host.ProvinceAdmin) { IsStatic = true }).Entity;
            _context.SaveChanges();
        }

        GrantPermissionIfNotExists(provinceAdminRoleForHost, AppPermissions.Pages_Administration_Users_Create);

        var cityAdminRoleForHost = _context.Roles.IgnoreQueryFilters().FirstOrDefault(r => r.TenantId == null && r.Name == StaticRoleNames.Host.CityAdmin);
        if (cityAdminRoleForHost == null)
        {
            cityAdminRoleForHost = _context.Roles.Add(new Role(null, StaticRoleNames.Host.CityAdmin, StaticRoleNames.Host.CityAdmin) { IsStatic = true }).Entity;
            _context.SaveChanges();
        }

        //admin user for host

        var adminUserForHost = _context.Users.IgnoreQueryFilters().FirstOrDefault(u => u.TenantId == null && u.UserName == AbpUserBase.AdminUserName);
        if (adminUserForHost == null)
        {
            var user = new User
            {
                TenantId = null,
                UserName = AbpUserBase.AdminUserName,
                Name = "admin",
                Surname = "admin",
                EmailAddress = "admin@aspnetzero.com",
                IsEmailConfirmed = true,
                ShouldChangePasswordOnNextLogin = false,
                IsActive = true,
                Password = "AM4OLBpptxBYmM79lGOX9egzZk3vIQU3d/gFCJzaBjAPXzYIK3tQ2N7X4fcrHtElTw=="
            };

            user.SetNormalizedNames();

            adminUserForHost = _context.Users.Add(user).Entity;
            _context.SaveChanges();

            //Assign Admin role to admin user
            _context.UserRoles.Add(new UserRole(null, adminUserForHost.Id, adminRoleForHost.Id));
            _context.SaveChanges();

            //User account of admin user
            _context.UserAccounts.Add(new UserAccount
            {
                TenantId = null,
                UserId = adminUserForHost.Id,
                UserName = AbpUserBase.AdminUserName,
                EmailAddress = adminUserForHost.EmailAddress
            });

            _context.SaveChanges();

            //Notification subscriptions
            _context.NotificationSubscriptions.Add(new NotificationSubscriptionInfo(SequentialGuidGenerator.Instance.Create(), null, adminUserForHost.Id, AppNotificationNames.NewTenantRegistered));
            _context.NotificationSubscriptions.Add(new NotificationSubscriptionInfo(SequentialGuidGenerator.Instance.Create(), null, adminUserForHost.Id, AppNotificationNames.NewUserRegistered));

            _context.SaveChanges();
        }

        var superAdminUserForHost = _context.Users.IgnoreQueryFilters().FirstOrDefault(u => u.TenantId == null && u.UserName == "superadmin");
        if (superAdminUserForHost == null)
        {
            var user = new User
            {
                TenantId = null,
                UserName = "superadmin",
                Name = "Super",
                Surname = "Admin",
                EmailAddress = "superadmin@aspnetzero.com",
                IsEmailConfirmed = true,
                ShouldChangePasswordOnNextLogin = false,
                IsActive = true,
                Password = "AM4OLBpptxBYmM79lGOX9egzZk3vIQU3d/gFCJzaBjAPXzYIK3tQ2N7X4fcrHtElTw=="
            };

            user.SetNormalizedNames();

            superAdminUserForHost = _context.Users.Add(user).Entity;
            _context.SaveChanges();

            _context.UserRoles.Add(new UserRole(null, superAdminUserForHost.Id, superAdminRoleForHost.Id));
            _context.SaveChanges();

            _context.UserAccounts.Add(new UserAccount
            {
                TenantId = null,
                UserId = superAdminUserForHost.Id,
                UserName = "superadmin",
                EmailAddress = superAdminUserForHost.EmailAddress
            });

            _context.SaveChanges();
        }
    }
}
