using Abp.Modules;
using Abp.Reflection.Extensions;

namespace HodHod;

[DependsOn(typeof(HodHodCoreSharedModule))]
public class HodHodApplicationSharedModule : AbpModule
{
    public override void Initialize()
    {
        IocManager.RegisterAssemblyByConvention(typeof(HodHodApplicationSharedModule).GetAssembly());
    }
}

