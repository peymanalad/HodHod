﻿using System;
using System.Web;
using Abp.Runtime.Security;
using Abp.Runtime.Validation;
using HodHod.Authorization.Accounts.Dto;
using HodHod.Security;

namespace HodHod.Web.Models.Account;

public class ResetPasswordViewModel : ResetPasswordInput
{
    public int? TenantId { get; set; }

    public PasswordComplexitySetting PasswordComplexitySetting { get; set; }

    protected override void ResolveParameters()
    {
        base.ResolveParameters();

        if (!string.IsNullOrEmpty(c))
        {
            var parameters = SimpleStringCipher.Instance.Decrypt(c);
            var query = HttpUtility.ParseQueryString(parameters);

            if (query["tenantId"] != null)
            {
                TenantId = Convert.ToInt32(query["tenantId"]);
            }
        }
    }
}

