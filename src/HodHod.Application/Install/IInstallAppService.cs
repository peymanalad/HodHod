﻿using System.Threading.Tasks;
using Abp.Application.Services;
using HodHod.Install.Dto;

namespace HodHod.Install;

public interface IInstallAppService : IApplicationService
{
    Task Setup(InstallDto input);

    AppSettingsJsonDto GetAppSettingsJson();

    CheckDatabaseOutput CheckDatabase();
}
