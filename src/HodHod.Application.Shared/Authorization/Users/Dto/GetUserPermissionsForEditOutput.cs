using System.Collections.Generic;
using HodHod.Authorization.Permissions.Dto;

namespace HodHod.Authorization.Users.Dto;

public class GetUserPermissionsForEditOutput
{
    public List<FlatPermissionDto> Permissions { get; set; }

    public List<string> GrantedPermissionNames { get; set; }
}

