using Abp.AutoMapper;
using Abp.Modules;
using Abp.Reflection.Extensions;
using HodHod.Authorization;

namespace HodHod;

/// <summary>
/// Application layer module of the application.
/// </summary>
[DependsOn(
    typeof(HodHodApplicationSharedModule),
    typeof(HodHodCoreModule)
    )]
public class HodHodApplicationModule : AbpModule
{
    public override void PreInitialize()
    {
        //Adding authorization providers
        Configuration.Authorization.Providers.Add<AppAuthorizationProvider>();

        //Adding custom AutoMapper configuration
        Configuration.Modules.AbpAutoMapper().Configurators.Add(CustomDtoMapper.CreateMappings);
    }

    public override void Initialize()
    {
        IocManager.RegisterAssemblyByConvention(typeof(HodHodApplicationModule).GetAssembly());
    }
}
