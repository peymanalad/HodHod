using Abp.Modules;
using Abp.Reflection.Extensions;
using Castle.Windsor.MsDependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using HodHod.Configure;
using HodHod.Startup;
using HodHod.Test.Base;

namespace HodHod.GraphQL.Tests;

[DependsOn(
    typeof(HodHodGraphQLModule),
    typeof(HodHodTestBaseModule))]
public class HodHodGraphQLTestModule : AbpModule
{
    public override void PreInitialize()
    {
        IServiceCollection services = new ServiceCollection();

        services.AddAndConfigureGraphQL();

        WindsorRegistrationHelper.CreateServiceProvider(IocManager.IocContainer, services);
    }

    public override void Initialize()
    {
        IocManager.RegisterAssemblyByConvention(typeof(HodHodGraphQLTestModule).GetAssembly());
    }
}