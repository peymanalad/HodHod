using System;
using System.IO;
using System.Text;
using Abp.AspNetCore;
using Abp.AspNetCore.Configuration;
using Abp.AspNetCore.OpenIddict;
using Abp.AspNetCore.SignalR;
using Abp.AspNetZeroCore.Web;
using Abp.Configuration.Startup;
using Abp.Hangfire;
using Abp.Hangfire.Configuration;
using Abp.Modules;
using Abp.Reflection.Extensions;
using Abp.Runtime.Caching.Redis;
using Abp.Zero.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using HodHod.Configuration;
using HodHod.EntityFrameworkCore;
using HodHod.Startup;
using HodHod.Web.Authentication.JwtBearer;
using HodHod.Web.Common;
using HodHod.Web.Configuration;
using Abp.HtmlSanitizer;
using Abp.HtmlSanitizer.Configuration;
using HodHod.Authorization.Accounts;

namespace HodHod.Web;

[DependsOn(
    typeof(HodHodApplicationModule),
    typeof(HodHodEntityFrameworkCoreModule),
    typeof(AbpAspNetZeroCoreWebModule),
    typeof(AbpAspNetCoreSignalRModule),
    typeof(HodHodGraphQLModule),
    typeof(AbpRedisCacheModule), //AbpRedisCacheModule dependency (and Abp.RedisCache nuget package) can be removed if not using Redis cache
    typeof(AbpHangfireAspNetCoreModule), //AbpHangfireModule dependency (and Abp.Hangfire.AspNetCore nuget package) can be removed if not using Hangfire
    typeof(AbpHtmlSanitizerModule),
    typeof(AbpAspNetCoreOpenIddictModule)
)]
public class HodHodWebCoreModule : AbpModule
{
    private readonly IWebHostEnvironment _env;
    private readonly IConfigurationRoot _appConfiguration;

    public HodHodWebCoreModule(IWebHostEnvironment env)
    {
        _env = env;
        _appConfiguration = env.GetAppConfiguration();
    }

    public override void PreInitialize()
    {
        var envConnection = ConnectionStringProvider.Get(_appConfiguration);
        //Use database for language management
        Configuration.Modules.Zero().LanguageManagement.EnableDbLocalization();

        Configuration.Modules.AbpAspNetCore()
            .CreateControllersForAppServices(
                typeof(HodHodApplicationModule).GetAssembly()
            );

        if (Environment.GetEnvironmentVariable("Authentication:JwtBearer:IsEnabled") != null &&
            bool.Parse(Environment.GetEnvironmentVariable("Authentication:JwtBearer:IsEnabled")))
        {
            ConfigureTokenAuth();
        }

        Configuration.ReplaceService<IAppConfigurationAccessor, AppConfigurationAccessor>();

        Configuration.ReplaceService<IAppConfigurationWriter, AppConfigurationWriter>();

        if (WebConsts.HangfireDashboardEnabled)
        {
            Configuration.BackgroundJobs.UseHangfire();
        }

        // Configure Redis cache if a connection string is provided.
        var redisConnection = Environment.GetEnvironmentVariable("Abp:RedisCache:ConnectionString") ??
                              Environment.GetEnvironmentVariable("Abp__RedisCache__ConnectionString");
        if (!string.IsNullOrWhiteSpace(redisConnection))
        {
            Configuration.Caching.UseRedis(options =>
            {
                options.ConnectionString = redisConnection;

                var dbIdValue = Environment.GetEnvironmentVariable("Abp:RedisCache:DatabaseId") ??
                                Environment.GetEnvironmentVariable("Abp__RedisCache__DatabaseId");
                if (!string.IsNullOrWhiteSpace(dbIdValue) && int.TryParse(dbIdValue, out var dbId))
                {
                    options.DatabaseId = dbId;
                }
                else
                {
                    options.DatabaseId = _appConfiguration.GetValue<int>("Abp:RedisCache:DatabaseId");
                }
            });
        }

        Configuration.Modules.AbpHtmlSanitizer()
            .KeepChildNodes()
            .AddSelector<IAccountAppService>(x => nameof(x.IsTenantAvailable))
            .AddSelector<IAccountAppService>(x => nameof(x.Register));
    }

    private void ConfigureTokenAuth()
    {
        IocManager.Register<TokenAuthConfiguration>();
        var tokenAuthConfig = IocManager.Resolve<TokenAuthConfiguration>();

        tokenAuthConfig.SecurityKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("Authentication:JwtBearer:SecurityKey"))
        );

        tokenAuthConfig.Issuer = Environment.GetEnvironmentVariable("Authentication:JwtBearer:Issuer");
        tokenAuthConfig.Audience = Environment.GetEnvironmentVariable("Authentication:JwtBearer:Audience");
        tokenAuthConfig.SigningCredentials =
            new SigningCredentials(tokenAuthConfig.SecurityKey, SecurityAlgorithms.HmacSha256);
        tokenAuthConfig.AccessTokenExpiration = AppConsts.AccessTokenExpiration;
        tokenAuthConfig.RefreshTokenExpiration = AppConsts.RefreshTokenExpiration;
    }

    public override void Initialize()
    {
        IocManager.RegisterAssemblyByConvention(typeof(HodHodWebCoreModule).GetAssembly());
    }

    public override void PostInitialize()
    {
        SetAppFolders();

        IocManager.Resolve<ApplicationPartManager>()
            .AddApplicationPartsIfNotAddedBefore(typeof(HodHodWebCoreModule).Assembly);
    }

    private void SetAppFolders()
    {
        var appFolders = IocManager.Resolve<AppFolders>();

        var sampleProfileImagesFolder = Environment.GetEnvironmentVariable("AppFolders:SampleProfileImagesFolder") ??
                                        Environment.GetEnvironmentVariable("AppFolders__SampleProfileImagesFolder");
        appFolders.SampleProfileImagesFolder = !string.IsNullOrWhiteSpace(sampleProfileImagesFolder)
            ? sampleProfileImagesFolder
            : Path.Combine(_env.WebRootPath,
                $"Common{Path.DirectorySeparatorChar}Images{Path.DirectorySeparatorChar}SampleProfilePics");

        var webLogsFolder = Environment.GetEnvironmentVariable("AppFolders:WebLogsFolder") ??
                            Environment.GetEnvironmentVariable("AppFolders__WebLogsFolder");
        appFolders.WebLogsFolder = !string.IsNullOrWhiteSpace(webLogsFolder)
            ? webLogsFolder
            : Path.Combine(_env.ContentRootPath, $"App_Data{Path.DirectorySeparatorChar}Logs");

        var reportFilesFolder = Environment.GetEnvironmentVariable("AppFolders:ReportFilesFolder") ??
                                Environment.GetEnvironmentVariable("AppFolders__ReportFilesFolder");
        appFolders.ReportFilesFolder = !string.IsNullOrWhiteSpace(reportFilesFolder)
            ? reportFilesFolder
            : "/var/dockers/HodHodBackend/BinaryObjects";

        Directory.CreateDirectory(appFolders.ReportFilesFolder);
        Directory.CreateDirectory(appFolders.WebLogsFolder);
    }
}

