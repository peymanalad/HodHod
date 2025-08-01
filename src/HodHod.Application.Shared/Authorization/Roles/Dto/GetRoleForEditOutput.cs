﻿using System.Collections.Generic;
using HodHod.Authorization.Permissions.Dto;

namespace HodHod.Authorization.Roles.Dto;

public class GetRoleForEditOutput
{
    public RoleEditDto Role { get; set; }

    public List<FlatPermissionDto> Permissions { get; set; }

    public List<string> GrantedPermissionNames { get; set; }
}

