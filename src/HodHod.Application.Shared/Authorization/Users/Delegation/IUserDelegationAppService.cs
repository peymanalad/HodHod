﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using HodHod.Authorization.Users.Delegation.Dto;

namespace HodHod.Authorization.Users.Delegation;

public interface IUserDelegationAppService : IApplicationService
{
    Task<PagedResultDto<UserDelegationDto>> GetDelegatedUsers(GetUserDelegationsInput input);

    Task DelegateNewUser(CreateUserDelegationDto input);

    Task RemoveDelegation(EntityDto<long> input);

    Task<List<UserDelegationDto>> GetActiveUserDelegations();
}

