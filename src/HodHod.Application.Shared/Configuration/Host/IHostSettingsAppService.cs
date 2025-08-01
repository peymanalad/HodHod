﻿using System.Threading.Tasks;
using Abp.Application.Services;
using HodHod.Configuration.Host.Dto;

namespace HodHod.Configuration.Host;

public interface IHostSettingsAppService : IApplicationService
{
    Task<HostSettingsEditDto> GetAllSettings();

    Task UpdateAllSettings(HostSettingsEditDto input);

    Task SendTestEmail(SendTestEmailInput input);
}

