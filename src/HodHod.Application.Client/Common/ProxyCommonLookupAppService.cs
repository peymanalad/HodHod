﻿using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Threading;
using HodHod.Common.Dto;
using HodHod.Editions.Dto;

namespace HodHod.Common;

public class ProxyCommonLookupAppService : ProxyAppServiceBase, ICommonLookupAppService
{
    public async Task<ListResultDto<SubscribableEditionComboboxItemDto>> GetEditionsForCombobox(bool onlyFreeItems = false)
    {
        return await ApiClient.GetAsync<ListResultDto<SubscribableEditionComboboxItemDto>>(GetEndpoint(nameof(GetEditionsForCombobox)));
    }

    public async Task<PagedResultDto<FindUsersOutputDto>> FindUsers(FindUsersInput input)
    {
        return await ApiClient.PostAsync<PagedResultDto<FindUsersOutputDto>>(GetEndpoint(nameof(FindUsers)), input);
    }

    public GetDefaultEditionNameOutput GetDefaultEditionName()
    {
        return AsyncHelper.RunSync(() =>
            ApiClient.GetAsync<GetDefaultEditionNameOutput>(GetEndpoint(nameof(GetDefaultEditionName))));
    }
}

