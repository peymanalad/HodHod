using System.Collections.Generic;
using Abp.Application.Services.Dto;
using HodHod.Authorization.Permissions.Dto;
using HodHod.Web.Areas.App.Models.Common;

namespace HodHod.Web.Areas.App.Models.Roles;

public class RoleListViewModel : IPermissionsEditViewModel
{
    public List<FlatPermissionDto> Permissions { get; set; }

    public List<string> GrantedPermissionNames { get; set; }
}

