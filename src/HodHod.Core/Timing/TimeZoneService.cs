﻿using Abp.Application.Services.Dto;
using Abp.Configuration;
using Abp.Dependency;
using Abp.Timing;
using Abp.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TimeZoneConverter;

namespace HodHod.Timing;

public class TimeZoneService : ITimeZoneService, ITransientDependency
{
    readonly ISettingManager _settingManager;
    readonly ISettingDefinitionManager _settingDefinitionManager;

    public TimeZoneService(
        ISettingManager settingManager,
        ISettingDefinitionManager settingDefinitionManager)
    {
        _settingManager = settingManager;
        _settingDefinitionManager = settingDefinitionManager;
    }

    public async Task<string> GetDefaultTimezoneAsync(SettingScopes scope, int? tenantId)
    {
        if (scope == SettingScopes.User)
        {
            if (tenantId.HasValue)
            {
                return await _settingManager.GetSettingValueForTenantAsync(TimingSettingNames.TimeZone, tenantId.Value);
            }

            return await _settingManager.GetSettingValueForApplicationAsync(TimingSettingNames.TimeZone);
        }

        if (scope == SettingScopes.Tenant)
        {
            return await _settingManager.GetSettingValueForApplicationAsync(TimingSettingNames.TimeZone);
        }

        if (scope == SettingScopes.Application)
        {
            var timezoneSettingDefinition = _settingDefinitionManager.GetSettingDefinition(TimingSettingNames.TimeZone);
            return timezoneSettingDefinition.DefaultValue;
        }

        throw new Exception("Unknown scope for default timezone setting.");
    }

    public TimeZoneInfo FindTimeZoneById(string timezoneId)
    {
        return TZConvert.GetTimeZoneInfo(timezoneId);
    }

    public List<NameValueDto> GetWindowsTimezones()
    {
        return TZConvert.KnownWindowsTimeZoneIds.OrderBy(tz => tz)
            .Select(tz => new NameValueDto
            {
                Value = tz,
                Name = TZConvert.WindowsToIana(tz) + " (" + GetTimezoneOffset(TZConvert.GetTimeZoneInfo(tz)) + ")"
            }).OrderBy(e => e.Name).ToList();
    }

    private string GetTimezoneOffset(TimeZoneInfo timeZoneInfo)
    {
        if (timeZoneInfo.BaseUtcOffset < TimeSpan.Zero)
        {
            return "-" + timeZoneInfo.BaseUtcOffset.ToString(@"hh\:mm");
        }

        return "+" + timeZoneInfo.BaseUtcOffset.ToString(@"hh\:mm");
    }

    public void ValidateTimezone(string timeZone)
    {
        var timeZones = GetWindowsTimezones();

        if (!timeZones.Select(tz => tz.Value).Contains(timeZone))
        {
            throw new UserFriendlyException("The timezone value provided is not recognized");
        }
    }
}

