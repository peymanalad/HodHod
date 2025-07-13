using System.Collections.Generic;

namespace HodHod.Authorization.Roles.Dto;

public class GetRolesInput
{
    public List<string> Permissions { get; set; }
}

