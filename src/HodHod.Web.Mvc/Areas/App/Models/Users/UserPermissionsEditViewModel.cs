using Abp.AutoMapper;
using HodHod.Authorization.Users;
using HodHod.Authorization.Users.Dto;
using HodHod.Web.Areas.App.Models.Common;

namespace HodHod.Web.Areas.App.Models.Users;

[AutoMapFrom(typeof(GetUserPermissionsForEditOutput))]
public class UserPermissionsEditViewModel : GetUserPermissionsForEditOutput, IPermissionsEditViewModel
{
    public User User { get; set; }
}

