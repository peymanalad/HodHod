﻿using System.ComponentModel.DataAnnotations;
using Abp.Auditing;

namespace HodHod.Authorization.Users.Profile.Dto;

public class ChangePasswordInput
{
    [Required]
    [DisableAuditing]
    public string CurrentPassword { get; set; }

    [Required]
    [DisableAuditing]
    public string NewPassword { get; set; }
}

