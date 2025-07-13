using Abp.AutoMapper;
using Abp.Modules;
using Abp.Reflection.Extensions;

namespace HodHod.Startup;

[DependsOn(typeof(HodHodCoreModule))]
public class HodHodGraphQLModule : AbpModule
{
    public override void Initialize()
    {
        IocManager.RegisterAssemblyByConvention(typeof(HodHodGraphQLModule).GetAssembly());
    }

    public override void PreInitialize()
    {
        base.PreInitialize();

        //Adding custom AutoMapper configuration
        Configuration.Modules.AbpAutoMapper().Configurators.Add(CustomDtoMapper.CreateMappings);
    }
}

