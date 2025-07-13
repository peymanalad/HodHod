using Abp.Dependency;
using Abp.Reflection.Extensions;
using Microsoft.Extensions.Configuration;
using HodHod.Configuration;

namespace HodHod.Test.Base.Configuration;

public class TestAppConfigurationAccessor : IAppConfigurationAccessor, ISingletonDependency
{
    public IConfigurationRoot Configuration { get; }

    public TestAppConfigurationAccessor()
    {
        Configuration = AppConfigurations.Get(
            typeof(HodHodTestBaseModule).GetAssembly().GetDirectoryPathOrNull()
        );
    }
}
