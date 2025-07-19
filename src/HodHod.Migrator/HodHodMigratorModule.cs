using System;
using Abp.AspNetZeroCore;
using Abp.Events.Bus;
using Abp.Modules;
using Abp.Reflection.Extensions;
using Castle.MicroKernel.Registration;
using Microsoft.Extensions.Configuration;
using HodHod.Configuration;
using HodHod.EntityFrameworkCore;
using HodHod.Migrator.DependencyInjection;

namespace HodHod.Migrator;

[DependsOn(typeof(HodHodEntityFrameworkCoreModule))]
public class HodHodMigratorModule : AbpModule
{
    private readonly IConfigurationRoot _appConfiguration;

    public HodHodMigratorModule(HodHodEntityFrameworkCoreModule abpZeroTemplateEntityFrameworkCoreModule)
    {
        abpZeroTemplateEntityFrameworkCoreModule.SkipDbSeed = true;

        _appConfiguration = AppConfigurations.Get(
            typeof(HodHodMigratorModule).GetAssembly().GetDirectoryPathOrNull(),
            addUserSecrets: true
        );
    }

    public override void PreInitialize()
    {
        //Configuration.DefaultNameOrConnectionString = _appConfiguration.GetConnectionString(
        //    HodHodConsts.ConnectionStringName
        //    );
        //var envConnection = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");
        //if (string.IsNullOrEmpty(envConnection))
        //{
        //    envConnection = _appConfiguration.GetConnectionString(
        //        HodHodConsts.ConnectionStringName);
        //}
        //Configuration.DefaultNameOrConnectionString = envConnection;
        //Configuration.Modules.AspNetZero().LicenseCode = Environment.GetEnvironmentVariable("AbpZeroLicenseCode"];
        var envConnection = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING")
                            ?? Environment.GetEnvironmentVariable($"ConnectionStrings__{HodHodConsts.ConnectionStringName}");

        Configuration.DefaultNameOrConnectionString = !string.IsNullOrEmpty(envConnection)
            ? envConnection
            : _appConfiguration.GetConnectionString(HodHodConsts.ConnectionStringName);

        Configuration.Modules.AspNetZero().LicenseCode = Environment.GetEnvironmentVariable("ABP_LICENSE_CODE")
                                                         ?? Environment.GetEnvironmentVariable("AbpZeroLicenseCode");
        Configuration.BackgroundJobs.IsJobExecutionEnabled = false;
        Configuration.ReplaceService(typeof(IEventBus), () =>
        {
            IocManager.IocContainer.Register(
                Component.For<IEventBus>().Instance(NullEventBus.Instance)
            );
        });
    }

    public override void Initialize()
    {
        IocManager.RegisterAssemblyByConvention(typeof(HodHodMigratorModule).GetAssembly());
        ServiceCollectionRegistrar.Register(IocManager);
    }
}

