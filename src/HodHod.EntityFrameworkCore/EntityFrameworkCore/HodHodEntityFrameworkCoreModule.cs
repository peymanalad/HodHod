using Abp;
using Abp.Dependency;
using Abp.EntityFrameworkCore.Configuration;
using Abp.Modules;
using Abp.OpenIddict.EntityFrameworkCore;
using Abp.Reflection.Extensions;
using Abp.Zero.EntityFrameworkCore;
using HodHod.Configuration;
using HodHod.EntityHistory;
using HodHod.Migrations.Seed;

namespace HodHod.EntityFrameworkCore;

[DependsOn(
    typeof(AbpZeroCoreEntityFrameworkCoreModule),
    typeof(HodHodCoreModule),
    typeof(AbpZeroCoreOpenIddictEntityFrameworkCoreModule)
)]
public class HodHodEntityFrameworkCoreModule : AbpModule
{
    /* Used it tests to skip DbContext registration, in order to use in-memory database of EF Core */
    public bool SkipDbContextRegistration { get; set; }

    public bool SkipDbSeed { get; set; }

    public override void PreInitialize()
    {
        if (!SkipDbContextRegistration)
        {
            Configuration.Modules.AbpEfCore().AddDbContext<HodHodDbContext>(options =>
            {
                if (options.ExistingConnection != null)
                {
                    HodHodDbContextConfigurer.Configure(options.DbContextOptions,
                        options.ExistingConnection);
                }
                else
                {
                    HodHodDbContextConfigurer.Configure(options.DbContextOptions,
                        options.ConnectionString);
                }
            });
        }

        // Set this setting to true for enabling entity history.
        Configuration.EntityHistory.IsEnabled = false;

        // Uncomment below line to write change logs for the entities below:
        //Configuration.EntityHistory.Selectors.Add("HodHodEntities", EntityHistoryHelper.TrackedTypes);
        //Configuration.CustomConfigProviders.Add(new EntityHistoryConfigProvider(Configuration));
    }

    public override void Initialize()
    {
        IocManager.RegisterAssemblyByConvention(typeof(HodHodEntityFrameworkCoreModule).GetAssembly());
    }

    public override void PostInitialize()
    {
        var configurationAccessor = IocManager.Resolve<IAppConfigurationAccessor>();

        using (var scope = IocManager.CreateScope())
        {
            if (!SkipDbSeed && scope.Resolve<DatabaseCheckHelper>()
                    .Exist(configurationAccessor.Configuration["ConnectionStrings:Default"]))
            {
                SeedHelper.SeedHostDb(IocManager);
            }
        }
    }
}

