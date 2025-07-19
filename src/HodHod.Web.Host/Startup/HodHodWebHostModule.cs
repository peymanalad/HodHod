using System;
using System.Collections.Generic;
using Abp.AspNetZeroCore;
using Abp.AspNetZeroCore.Web.Authentication.External;
using Abp.AspNetZeroCore.Web.Authentication.External.Facebook;
using Abp.AspNetZeroCore.Web.Authentication.External.Google;
using Abp.AspNetZeroCore.Web.Authentication.External.Microsoft;
using Abp.AspNetZeroCore.Web.Authentication.External.OpenIdConnect;
using Abp.AspNetZeroCore.Web.Authentication.External.Twitter;
using Abp.AspNetZeroCore.Web.Authentication.External.WsFederation;
using Abp.Configuration.Startup;
using Abp.Dependency;
using Abp.Extensions;
using Abp.Modules;
using Abp.Reflection.Extensions;
using Abp.Threading.BackgroundWorkers;
using Abp.Timing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using HodHod.Auditing;
using HodHod.Authorization.Users.Password;
using HodHod.Configuration;
using HodHod.EntityFrameworkCore;
using HodHod.MultiTenancy;
using HodHod.MultiTenancy.Subscription;
using HodHod.Web.Startup.ExternalLoginInfoProviders;

namespace HodHod.Web.Startup;

[DependsOn(
    typeof(HodHodWebCoreModule)
)]
public class HodHodWebHostModule : AbpModule
{
    private readonly IWebHostEnvironment _env;
    private readonly IConfigurationRoot _appConfiguration;

    public HodHodWebHostModule(
        IWebHostEnvironment env)
    {
        _env = env;
        _appConfiguration = env.GetAppConfiguration();
    }

    public override void PreInitialize()
    {
        //Configuration.Modules.AbpWebCommon().MultiTenancy.DomainFormat =
        //    Environment.GetEnvironmentVariable("App:ServerRootAddress"] ?? "https://localhost:44301/";
        //Configuration.Modules.AspNetZero().LicenseCode = Environment.GetEnvironmentVariable("AbpZeroLicenseCode"];
        Configuration.Modules.AbpWebCommon().MultiTenancy.DomainFormat =
            Environment.GetEnvironmentVariable("App_ServerRootAddress") ?? "https://localhost:44301/";

        Configuration.Modules.AspNetZero().LicenseCode =
            Environment.GetEnvironmentVariable("AbpZeroLicenseCode");
    }

    public override void Initialize()
    {
        IocManager.RegisterAssemblyByConvention(typeof(HodHodWebHostModule).GetAssembly());
    }

    public override void PostInitialize()
    {
        using (var scope = IocManager.CreateScope())
        {
            var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING")
                                   ?? Environment.GetEnvironmentVariable($"ConnectionStrings__{HodHodConsts.ConnectionStringName}")
                                   ?? _appConfiguration.GetConnectionString("Default");

            if (!scope.Resolve<DatabaseCheckHelper>().Exist(connectionString))
            {
                return;
            }

        }

        var workManager = IocManager.Resolve<IBackgroundWorkerManager>();
        if (IocManager.Resolve<IMultiTenancyConfig>().IsEnabled)
        {
            workManager.Add(IocManager.Resolve<SubscriptionExpirationCheckWorker>());
            workManager.Add(IocManager.Resolve<SubscriptionExpireEmailNotifierWorker>());
            workManager.Add(IocManager.Resolve<SubscriptionPaymentNotCompletedEmailNotifierWorker>());
        }

        var expiredAuditLogDeleterWorker = IocManager.Resolve<ExpiredAuditLogDeleterWorker>();
        if (Configuration.Auditing.IsEnabled && expiredAuditLogDeleterWorker.IsEnabled)
        {
            workManager.Add(expiredAuditLogDeleterWorker);
        }

        workManager.Add(IocManager.Resolve<PasswordExpirationBackgroundWorker>());

        ConfigureExternalAuthProviders();
    }

    private void ConfigureExternalAuthProviders()
    {
        var externalAuthConfiguration = IocManager.Resolve<ExternalAuthConfiguration>();

        bool.TryParse(Environment.GetEnvironmentVariable("Authentication:OpenId:IsEnabled"), out var openIdEnabled);
        if (openIdEnabled)
        {
            bool.TryParse(Environment.GetEnvironmentVariable("Authentication:AllowSocialLoginSettingsPerTenant"), out var perTenant);
            if (perTenant)
            {
                externalAuthConfiguration.ExternalLoginInfoProviders.Add(
                    IocManager.Resolve<TenantBasedOpenIdConnectExternalLoginInfoProvider>());
            }
            else
            {
                var jsonClaimMappings = new List<JsonClaimMap>();
                _appConfiguration.GetSection("Authentication:OpenId:ClaimsMapping").Bind(jsonClaimMappings);

                externalAuthConfiguration.ExternalLoginInfoProviders.Add(
                    new OpenIdConnectExternalLoginInfoProvider(
                        Environment.GetEnvironmentVariable("Authentication:OpenId:ClientId"),
                        Environment.GetEnvironmentVariable("Authentication:OpenId:ClientSecret"),
                        Environment.GetEnvironmentVariable("Authentication:OpenId:Authority"),
                        Environment.GetEnvironmentVariable("Authentication:OpenId:LoginUrl"),
                        bool.TryParse(Environment.GetEnvironmentVariable("Authentication:OpenId:ValidateIssuer"), out var validateIssuer) && validateIssuer,
                        Environment.GetEnvironmentVariable("Authentication:OpenId:ResponseType"),
                        jsonClaimMappings
                    )
                );
            }
        }

        bool.TryParse(Environment.GetEnvironmentVariable("Authentication:WsFederation:IsEnabled"), out var wsFedEnabled);
        if (wsFedEnabled)
        {
            bool.TryParse(Environment.GetEnvironmentVariable("Authentication:AllowSocialLoginSettingsPerTenant"), out var perTenant);
            if (perTenant)
            {
                externalAuthConfiguration.ExternalLoginInfoProviders.Add(
                    IocManager.Resolve<TenantBasedWsFederationExternalLoginInfoProvider>());
            }
            else
            {
                var jsonClaimMappings = new List<JsonClaimMap>();
                _appConfiguration.GetSection("Authentication:WsFederation:ClaimsMapping").Bind(jsonClaimMappings);

                externalAuthConfiguration.ExternalLoginInfoProviders.Add(
                    new WsFederationExternalLoginInfoProvider(
                        Environment.GetEnvironmentVariable("Authentication:WsFederation:ClientId"),
                        Environment.GetEnvironmentVariable("Authentication:WsFederation:Tenant"),
                        Environment.GetEnvironmentVariable("Authentication:WsFederation:MetaDataAddress"),
                        Environment.GetEnvironmentVariable("Authentication:WsFederation:Authority"),
                        jsonClaimMappings)
                );
            }
        }

        bool.TryParse(Environment.GetEnvironmentVariable("Authentication:Facebook:IsEnabled"), out var fbEnabled);
        if (fbEnabled)
        {
            bool.TryParse(Environment.GetEnvironmentVariable("Authentication:AllowSocialLoginSettingsPerTenant"), out var perTenant);
            if (perTenant)
            {
                externalAuthConfiguration.ExternalLoginInfoProviders.Add(
                    IocManager.Resolve<TenantBasedFacebookExternalLoginInfoProvider>());
            }
            else
            {
                externalAuthConfiguration.ExternalLoginInfoProviders.Add(new FacebookExternalLoginInfoProvider(
                    Environment.GetEnvironmentVariable("Authentication:Facebook:AppId"),
                    Environment.GetEnvironmentVariable("Authentication:Facebook:AppSecret")
                ));
            }
        }

        bool.TryParse(Environment.GetEnvironmentVariable("Authentication:Twitter:IsEnabled"), out var twitterEnabled);
        if (twitterEnabled)
        {
            bool.TryParse(Environment.GetEnvironmentVariable("Authentication:AllowSocialLoginSettingsPerTenant"), out var perTenant);
            if (perTenant)
            {
                externalAuthConfiguration.ExternalLoginInfoProviders.Add(
                    IocManager.Resolve<TenantBasedTwitterExternalLoginInfoProvider>());
            }
            else
            {
                var twitterExternalLoginInfoProvider = new TwitterExternalLoginInfoProvider(
                    Environment.GetEnvironmentVariable("Authentication:Twitter:ConsumerKey"),
                    Environment.GetEnvironmentVariable("Authentication:Twitter:ConsumerSecret"),
                    Environment.GetEnvironmentVariable("App:ClientRootAddress").EnsureEndsWith('/') + "account/login"
                );

                externalAuthConfiguration.ExternalLoginInfoProviders.Add(twitterExternalLoginInfoProvider);
            }
        }

        bool.TryParse(Environment.GetEnvironmentVariable("Authentication:Google:IsEnabled"), out var googleEnabled);
        if (googleEnabled)
        {
            bool.TryParse(Environment.GetEnvironmentVariable("Authentication:AllowSocialLoginSettingsPerTenant"), out var perTenant);
            if (perTenant)
            {
                externalAuthConfiguration.ExternalLoginInfoProviders.Add(
                    IocManager.Resolve<TenantBasedGoogleExternalLoginInfoProvider>());
            }
            else
            {
                externalAuthConfiguration.ExternalLoginInfoProviders.Add(
                    new GoogleExternalLoginInfoProvider(
                        Environment.GetEnvironmentVariable("Authentication:Google:ClientId"),
                        Environment.GetEnvironmentVariable("Authentication:Google:ClientSecret"),
                        Environment.GetEnvironmentVariable("Authentication:Google:UserInfoEndpoint")
                    )
                );
            }
        }

        bool.TryParse(Environment.GetEnvironmentVariable("Authentication:Microsoft:IsEnabled"), out var msEnabled);
        if (msEnabled)
        {
            bool.TryParse(Environment.GetEnvironmentVariable("Authentication:AllowSocialLoginSettingsPerTenant"), out var perTenant);
            if (perTenant)
            {
                externalAuthConfiguration.ExternalLoginInfoProviders.Add(
                    IocManager.Resolve<TenantBasedMicrosoftExternalLoginInfoProvider>());
            }
            else
            {
                externalAuthConfiguration.ExternalLoginInfoProviders.Add(
                    new MicrosoftExternalLoginInfoProvider(
                        Environment.GetEnvironmentVariable("Authentication:Microsoft:ConsumerKey"),
                        Environment.GetEnvironmentVariable("Authentication:Microsoft:ConsumerSecret")
                    )
                );
            }
        }
    }

}

