﻿using System.Linq;
using Abp.Configuration;
using Abp.Localization;
using Abp.MultiTenancy;
using Abp.Net.Mail;
using Abp.Timing;
using Microsoft.EntityFrameworkCore;
using HodHod.EntityFrameworkCore;

namespace HodHod.Migrations.Seed.Host;

public class DefaultSettingsCreator
{
    private readonly HodHodDbContext _context;

    public DefaultSettingsCreator(HodHodDbContext context)
    {
        _context = context;
    }

    public void Create()
    {
        int? tenantId = null;

        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
        if (!HodHodConsts.MultiTenancyEnabled)
#pragma warning disable 162
        {
            tenantId = MultiTenancyConsts.DefaultTenantId;
        }
#pragma warning restore 162

        //Emailing
        AddSettingIfNotExists(EmailSettingNames.DefaultFromAddress, "admin@mydomain.com", tenantId);
        AddSettingIfNotExists(EmailSettingNames.DefaultFromDisplayName, "mydomain.com mailer", tenantId);

        //Languages
        AddSettingIfNotExists(LocalizationSettingNames.DefaultLanguage, "en", tenantId);
        AddSettingIfNotExists(TimingSettingNames.TimeZone, "Iran Standard Time", tenantId);
    }

    private void AddSettingIfNotExists(string name, string value, int? tenantId = null)
    {
        if (_context.Settings.IgnoreQueryFilters().Any(s => s.Name == name && s.TenantId == tenantId && s.UserId == null))
        {
            return;
        }

        _context.Settings.Add(new Setting(tenantId, null, name, value));
        _context.SaveChanges();
    }
}

