using Abp.AspNetZeroCore;
using Abp.Configuration.Startup;
using Abp.Dependency;
using Abp.Modules;
using Abp.Reflection.Extensions;
using Abp.Threading.BackgroundWorkers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using HodHod.Auditing;
using HodHod.Authorization.Users.Password;
using HodHod.Configuration;
using HodHod.EntityFrameworkCore;
using HodHod.MultiTenancy;
using HodHod.MultiTenancy.Subscription;
using HodHod.Web.Areas.App.Startup;

namespace HodHod.Web.Startup;

[DependsOn(
    typeof(HodHodWebCoreModule)
)]
public class HodHodWebMvcModule : AbpModule
{
    private readonly IConfigurationRoot _appConfiguration;

    public HodHodWebMvcModule(IWebHostEnvironment env)
    {
        _appConfiguration = env.GetAppConfiguration();
    }

    public override void PreInitialize()
    {
        Configuration.Modules.AbpWebCommon().MultiTenancy.DomainFormat = _appConfiguration["App:WebSiteRootAddress"] ?? "https://localhost:44302/";
        Configuration.Modules.AspNetZero().LicenseCode = _appConfiguration["AbpZeroLicenseCode"];
        Configuration.Navigation.Providers.Add<AppNavigationProvider>();

        IocManager.Register<DashboardViewConfiguration>();
    }

    public override void Initialize()
    {
        IocManager.RegisterAssemblyByConvention(typeof(HodHodWebMvcModule).GetAssembly());
    }

    public override void PostInitialize()
    {
        if (!IocManager.Resolve<IMultiTenancyConfig>().IsEnabled)
        {
            return;
        }

        using (var scope = IocManager.CreateScope())
        {
            if (!scope.Resolve<DatabaseCheckHelper>().Exist(_appConfiguration["ConnectionStrings:Default"]))
            {
                return;
            }
        }

        var workManager = IocManager.Resolve<IBackgroundWorkerManager>();
        workManager.Add(IocManager.Resolve<SubscriptionExpirationCheckWorker>());
        workManager.Add(IocManager.Resolve<SubscriptionExpireEmailNotifierWorker>());
        workManager.Add(IocManager.Resolve<SubscriptionPaymentNotCompletedEmailNotifierWorker>());

        var expiredAuditLogDeleterWorker = IocManager.Resolve<ExpiredAuditLogDeleterWorker>();
        if (Configuration.Auditing.IsEnabled && expiredAuditLogDeleterWorker.IsEnabled)
        {
            workManager.Add(expiredAuditLogDeleterWorker);
        }

        workManager.Add(IocManager.Resolve<PasswordExpirationBackgroundWorker>());
    }
}

