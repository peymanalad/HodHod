﻿using System.ComponentModel.DataAnnotations;

namespace HodHod.Organizations.Dto;

public class RoleToOrganizationUnitInput
{
    [Range(1, long.MaxValue)]
    public int RoleId { get; set; }

    [Range(1, long.MaxValue)]
    public long OrganizationUnitId { get; set; }
}

