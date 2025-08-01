﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using HodHod.Authorization.Users.Dto;
using HodHod.Dto;

namespace HodHod.Authorization.Users;

public class ProxyUserAppService : ProxyAppServiceBase, IUserAppService
{
    public async Task<PagedResultDto<UserListDto>> GetUsers(GetUsersInput input)
    {
        return await ApiClient.PostAsync<PagedResultDto<UserListDto>>(GetEndpoint(nameof(GetUsers)), input);
    }

    public async Task<FileDto> GetUsersToExcel(GetUsersToExcelInput input)
    {
        return await ApiClient.GetAsync<FileDto>(GetEndpoint(nameof(GetUsersToExcel)), input);
    }

    public async Task<GetUserForEditOutput> GetUserForEdit(NullableIdDto<long> input)
    {
        return await ApiClient.GetAsync<GetUserForEditOutput>(GetEndpoint(nameof(GetUserForEdit)), input);
    }

    public async Task<GetUserPermissionsForEditOutput> GetUserPermissionsForEdit(EntityDto<long> input)
    {
        return await ApiClient.GetAsync<GetUserPermissionsForEditOutput>(GetEndpoint(nameof(GetUserPermissionsForEdit)), input);
    }

    public async Task ResetUserSpecificPermissions(EntityDto<long> input)
    {
        await ApiClient.PostAsync(GetEndpoint(nameof(ResetUserSpecificPermissions)), input);
    }

    public async Task UpdateUserPermissions(UpdateUserPermissionsInput input)
    {
        await ApiClient.PutAsync(GetEndpoint(nameof(UpdateUserPermissions)), input);
    }

    public async Task CreateOrUpdateUser(CreateOrUpdateUserInput input)
    {
        await ApiClient.PostAsync(GetEndpoint(nameof(CreateOrUpdateUser)), input);
    }

    public async Task DeleteUser(EntityDto<long> input)
    {
        await ApiClient.DeleteAsync(GetEndpoint(nameof(DeleteUser)), input);
    }

    public async Task UnlockUser(EntityDto<long> input)
    {
        await ApiClient.PostAsync(GetEndpoint(nameof(UnlockUser)), input);
    }

    public async Task<List<string>> GetUserExcelColumnsToExcel()
    {
        return await ApiClient.GetAsync<List<string>>(GetEndpoint(nameof(GetUserExcelColumnsToExcel)));
    }
}

