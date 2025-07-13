using Abp.Application.Services;
using Abp.Application.Services.Dto;
using HodHod.Authorization.Permissions.Dto;

namespace HodHod.Authorization.Permissions;

public interface IPermissionAppService : IApplicationService
{
    ListResultDto<FlatPermissionWithLevelDto> GetAllPermissions();
}

