using Abp.Modules;
using Abp.Reflection.Extensions;

namespace HodHod;

public class HodHodClientModule : AbpModule
{
    public override void Initialize()
    {
        IocManager.RegisterAssemblyByConvention(typeof(HodHodClientModule).GetAssembly());
    }
}

