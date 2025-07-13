using Abp.Authorization;
using HodHod.Authorization.Roles;
using HodHod.Authorization.Users;

namespace HodHod.Authorization;

public class PermissionChecker : PermissionChecker<Role, User>
{
    public PermissionChecker(UserManager userManager)
        : base(userManager)
    {

    }
}

