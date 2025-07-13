using System.Collections.Generic;
using HodHod.Authorization.Permissions.Dto;

namespace HodHod.Web.Areas.App.Models.Common;

public interface IPermissionsEditViewModel
{
    List<FlatPermissionDto> Permissions { get; set; }

    List<string> GrantedPermissionNames { get; set; }
}

