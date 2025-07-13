using Abp.AutoMapper;
using Abp.Configuration.Startup;
using Abp.Modules;
using Abp.Reflection.Extensions;
using HodHod.ApiClient;
using HodHod.Maui.Core;

namespace HodHod.Maui;

[DependsOn(typeof(HodHodClientModule), typeof(AbpAutoMapperModule))]
public class HodHodMauiModule : AbpModule
{
    public override void PreInitialize()
    {
        Configuration.Localization.IsEnabled = false;
        Configuration.BackgroundJobs.IsJobExecutionEnabled = false;

        Configuration.ReplaceService<IApplicationContext, MauiApplicationContext>();
    }

    public override void Initialize()
    {
        IocManager.RegisterAssemblyByConvention(typeof(HodHodMauiModule).GetAssembly());
    }
}