﻿using System.ComponentModel.DataAnnotations;

namespace HodHod.TokenAuth.Dto;

public class SendTwoFactorAuthCodeModel
{
    [Range(1, long.MaxValue)]
    public long UserId { get; set; }

    [Required]
    public string Provider { get; set; }
}