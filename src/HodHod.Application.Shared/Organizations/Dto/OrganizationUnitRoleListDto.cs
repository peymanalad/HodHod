﻿using System;
using Abp.Application.Services.Dto;

namespace HodHod.Organizations.Dto;

public class OrganizationUnitRoleListDto : EntityDto<long>
{
    public string DisplayName { get; set; }

    public string Name { get; set; }

    public DateTime AddedTime { get; set; }
}

