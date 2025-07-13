﻿using Abp.Application.Services;
using Abp.Application.Services.Dto;
using HodHod.EntityChanges.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HodHod.EntityChanges;

public interface IEntityChangeAppService : IApplicationService
{
    Task<ListResultDto<EntityAndPropertyChangeListDto>> GetEntityChangesByEntity(GetEntityChangesByEntityInput input);
}

