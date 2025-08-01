﻿using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Abp;
using Abp.AspNetZeroCore;
using Abp.AspNetZeroCore.Timing;
using Abp.AutoMapper;
using Abp.BackgroundJobs;
using Abp.Dependency;
using Abp.Modules;
using Abp.Net.Mail;
using Abp.Reflection.Extensions;
using Abp.Timing;
using Abp.Configuration.Startup;
using Abp.Domain.Uow;
using Abp.Events.Bus;
using Abp.Events.Bus.Exceptions;
using Abp.Json;
using Abp.Localization.Dictionaries.Xml;
using Abp.Localization.Sources;
using Abp.MailKit;
using Abp.Net.Mail.Smtp;
using Abp.OpenIddict;
using Abp.Threading;
using Abp.Threading.BackgroundWorkers;
using Abp.Threading.Timers;
using Abp.Zero;
using Abp.Zero.Configuration;
using Abp.Zero.Ldap;
using Abp.Zero.Ldap.Configuration;
using Castle.MicroKernel.Registration;
using MailKit.Security;
using HodHod.Authentication.TwoFactor;
using HodHod.Authorization.Delegation;
using HodHod.Authorization.Impersonation;
using HodHod.Authorization.Ldap;
using HodHod.Authorization.PasswordlessLogin;
using HodHod.Authorization.QrLogin;
using HodHod.Authorization.Roles;
using HodHod.Authorization.Users;
using HodHod.Chat;
using HodHod.Configuration;
using HodHod.DashboardCustomization.Definitions;
using HodHod.Debugging;
using HodHod.DynamicEntityProperties;
using HodHod.Features;
using HodHod.Friendships;
using HodHod.Friendships.Cache;
using HodHod.Localization;
using HodHod.MultiTenancy;
using HodHod.Net.Emailing;
using HodHod.Net.Sms;
using HodHod.Notifications;
using HodHod.WebHooks;

namespace HodHod;

[DependsOn(
    typeof(HodHodCoreSharedModule),
    typeof(AbpZeroCoreModule),
    typeof(AbpZeroLdapModule),
    typeof(AbpAutoMapperModule),
    typeof(AbpAspNetZeroCoreModule),
    typeof(AbpMailKitModule),
    typeof(AbpZeroCoreOpenIddictModule))]
public class HodHodCoreModule : AbpModule
{
    public override void PreInitialize()
    {
        //workaround for issue: https://github.com/aspnet/EntityFrameworkCore/issues/9825
        //related github issue: https://github.com/aspnet/EntityFrameworkCore/issues/10407
        AppContext.SetSwitch("Microsoft.EntityFrameworkCore.Issue9825", true);

        Configuration.Auditing.IsEnabledForAnonymousUsers = true;

        //Declare entity types
        Configuration.Modules.Zero().EntityTypes.Tenant = typeof(Tenant);
        Configuration.Modules.Zero().EntityTypes.Role = typeof(Role);
        Configuration.Modules.Zero().EntityTypes.User = typeof(User);

        HodHodLocalizationConfigurer.Configure(Configuration.Localization);

        //Adding feature providers
        Configuration.Features.Providers.Add<AppFeatureProvider>();

        //Adding setting providers
        Configuration.Settings.Providers.Add<AppSettingProvider>();

        //Adding notification providers
        Configuration.Notifications.Providers.Add<AppNotificationProvider>();

        //Adding webhook definition providers
        Configuration.Webhooks.Providers.Add<AppWebhookDefinitionProvider>();
        Configuration.Webhooks.TimeoutDuration = TimeSpan.FromMinutes(2);
        Configuration.Webhooks.IsAutomaticSubscriptionDeactivationEnabled = false;

        //Enable this line to create a multi-tenant application.
        Configuration.MultiTenancy.IsEnabled = HodHodConsts.MultiTenancyEnabled;

        //Enable LDAP authentication
        //Configuration.Modules.ZeroLdap().Enable(typeof(AppLdapAuthenticationSource));

        //Twilio - Enable this line to activate Twilio SMS integration
        //Configuration.ReplaceService<ISmsSender,TwilioSmsSender>();

        //Kavenegar SMS integration
        Configuration.ReplaceService<ISmsSender, KavenegarSmsSender>(DependencyLifeStyle.Transient);

        //Adding DynamicEntityParameters definition providers
        Configuration.DynamicEntityProperties.Providers.Add<AppDynamicEntityPropertyDefinitionProvider>();

        // MailKit configuration
        Configuration.Modules.AbpMailKit().SecureSocketOption = SecureSocketOptions.Auto;
        Configuration.ReplaceService<IMailKitSmtpBuilder, HodHodMailKitSmtpBuilder>(DependencyLifeStyle.Transient);

        //Configure roles
        AppRoleConfig.Configure(Configuration.Modules.Zero().RoleManagement);

        if (DebugHelper.IsDebug)
        {
            //Disabling email sending in debug mode
            Configuration.ReplaceService<IEmailSender, NullEmailSender>(DependencyLifeStyle.Transient);
        }

        Configuration.ReplaceService(typeof(IEmailSenderConfiguration), () =>
        {
            Configuration.IocManager.IocContainer.Register(
                Component.For<IEmailSenderConfiguration, ISmtpEmailSenderConfiguration>()
                         .ImplementedBy<HodHodSmtpEmailSenderConfiguration>()
                         .LifestyleTransient()
            );
        });

        // Configures caching with sliding expiration times for  cache items.
        ConfigureCaching();

        IocManager.Register<DashboardConfiguration>();

        Configuration.Notifications.Notifiers.Add<SmsRealTimeNotifier>();
        Configuration.Notifications.Notifiers.Add<EmailRealTimeNotifier>();
    }

    public override void Initialize()
    {
        IocManager.RegisterAssemblyByConvention(typeof(HodHodCoreModule).GetAssembly());
    }

    public override void PostInitialize()
    {
        IocManager.RegisterIfNot<IChatCommunicator, NullChatCommunicator>();
        IocManager.Register<IUserDelegationConfiguration, UserDelegationConfiguration>();

        IocManager.Resolve<ChatUserStateWatcher>().Initialize();
        IocManager.Resolve<AppTimes>().StartupTime = Clock.Now;
    }

    private void ConfigureCaching()
    {
        Configuration.Caching.Configure(FriendCacheItem.CacheName, cache =>
        {
            cache.DefaultSlidingExpireTime = FriendCacheItem.DefaultSlidingExpireTime;
        });

        Configuration.Caching.Configure(TwoFactorCodeCacheItem.CacheName, cache =>
        {
            cache.DefaultSlidingExpireTime = TwoFactorCodeCacheItem.DefaultSlidingExpireTime;
        });

        Configuration.Caching.Configure(PasswordlessLoginCodeCacheItem.CacheName, cache =>
        {
            cache.DefaultSlidingExpireTime = PasswordlessLoginCodeCacheItem.DefaultSlidingExpireTime;
        });

        Configuration.Caching.Configure(ImpersonationCacheItem.CacheName, cache =>
        {
            cache.DefaultSlidingExpireTime = ImpersonationCacheItem.DefaultSlidingExpireTime;
        });

        Configuration.Caching.Configure(SwitchToLinkedAccountCacheItem.CacheName, cache =>
        {
            cache.DefaultSlidingExpireTime = SwitchToLinkedAccountCacheItem.DefaultSlidingExpireTime;
        });

        Configuration.Caching.Configure(QrLoginSessionIdCacheItem.CacheName, cache =>
        {
            cache.DefaultSlidingExpireTime = QrLoginSessionIdCacheItem.DefaultSlidingExpireTime;
        });

        Configuration.Caching.Configure(OtpSendLimitCacheItem.CacheName, cache =>
        {
            cache.DefaultSlidingExpireTime = OtpSendLimitCacheItem.DefaultSlidingExpireTime;
        });
    }
}

