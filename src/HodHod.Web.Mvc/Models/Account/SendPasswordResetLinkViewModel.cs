﻿using System.ComponentModel.DataAnnotations;

namespace HodHod.Web.Models.Account;

public class SendPasswordResetLinkViewModel
{
    [Required]
    public string EmailAddress { get; set; }
}

