﻿using System.ComponentModel.DataAnnotations;

namespace HodHod.Organizations.Dto;

public class UserToOrganizationUnitInput
{
    [Range(1, long.MaxValue)]
    public long UserId { get; set; }

    [Range(1, long.MaxValue)]
    public long OrganizationUnitId { get; set; }
}

