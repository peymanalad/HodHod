using Abp.Modules;
using Abp.Reflection.Extensions;

namespace HodHod;

public class HodHodCoreSharedModule : AbpModule
{
    public override void Initialize()
    {
        IocManager.RegisterAssemblyByConvention(typeof(HodHodCoreSharedModule).GetAssembly());
    }
}

