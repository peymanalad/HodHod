using Abp.AutoMapper;
using HodHod.Authorization.Roles.Dto;
using HodHod.Web.Areas.App.Models.Common;

namespace HodHod.Web.Areas.App.Models.Roles;

[AutoMapFrom(typeof(GetRoleForEditOutput))]
public class CreateOrEditRoleModalViewModel : GetRoleForEditOutput, IPermissionsEditViewModel
{
    public bool IsEditMode => Role.Id.HasValue;
}

