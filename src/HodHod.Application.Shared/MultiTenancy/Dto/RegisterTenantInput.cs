﻿using System.ComponentModel.DataAnnotations;
using Abp.Auditing;
using Abp.Authorization.Users;
using Abp.MultiTenancy;
using HodHod.MultiTenancy.Payments;
using HodHod.MultiTenancy.Payments.Dto;

namespace HodHod.MultiTenancy.Dto;

public class RegisterTenantInput
{
    [Required]
    [StringLength(AbpTenantBase.MaxTenancyNameLength)]
    public string TenancyName { get; set; }

    [Required]
    [StringLength(AbpTenantBase.MaxNameLength)]
    public string Name { get; set; }

    [Required]
    [EmailAddress]
    [StringLength(AbpUserBase.MaxEmailAddressLength)]
    public string AdminEmailAddress { get; set; }

    [StringLength(AbpUserBase.MaxNameLength)]
    public string AdminName { get; set; }

    [StringLength(AbpUserBase.MaxSurnameLength)]
    public string AdminSurname { get; set; }

    [StringLength(AbpUserBase.MaxPlainPasswordLength)]
    [DisableAuditing]
    public string AdminPassword { get; set; }

    [DisableAuditing]
    public string CaptchaResponse { get; set; }

    public SubscriptionStartType SubscriptionStartType { get; set; }

    public PaymentPeriodType? PaymentPeriodType { get; set; }

    public int? EditionId { get; set; }

    public string SuccessUrl { get; set; }

    public string ErrorUrl { get; set; }
}

