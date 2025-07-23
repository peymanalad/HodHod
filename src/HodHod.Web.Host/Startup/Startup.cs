using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Abp.AspNetCore;
using Abp.AspNetCore.Configuration;
using Abp.AspNetCore.Mvc.Antiforgery;
using Abp.AspNetCore.Mvc.Extensions;
using Abp.AspNetCore.SignalR.Hubs;
using Abp.AspNetZeroCore.Web.Authentication.JwtBearer;
using Abp.Castle.Logging.Log4Net;
using Abp.Extensions;
using Abp.Hangfire;
using Abp.PlugIns;
using Castle.Facilities.Logging;
using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using HodHod.Authorization;
using HodHod.Configuration;
using HodHod.EntityFrameworkCore;
using HodHod.Identity;
using HodHod.Web.Chat.SignalR;
using HodHod.Web.Common;
using HodHod.Web.Swagger;
using Stripe;
using ILoggerFactory = Microsoft.Extensions.Logging.ILoggerFactory;
using GraphQL.Server.Ui.Playground;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using HodHod.Configure;
using HodHod.Schemas;
using HodHod.Web.HealthCheck;
using Owl.reCAPTCHA;
using HealthChecksUISettings = HealthChecks.UI.Configuration.Settings;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using HodHod.Web.MultiTenancy;
using Abp.HtmlSanitizer;
using Microsoft.AspNetCore.Authentication.Cookies;
using HodHod.Web.Authentication.PasswordlessLogin;
using HodHod.Web.OpenIddict;
using Abp.AspNetCore.OpenIddict;
using HodHod.Authorization.QrLogin;
using HodHod.Web.Reports;
using Microsoft.AspNetCore.Http.Features;

namespace HodHod.Web.Startup;

public class Startup
{
    private const string DefaultCorsPolicyName = "localhost";

    private readonly IConfigurationRoot _appConfiguration;
    private readonly IWebHostEnvironment _hostingEnvironment;

    public Startup(IWebHostEnvironment env)
    {
        DotNetEnv.Env.TraversePath().Load();
        _hostingEnvironment = env;
        _appConfiguration = env.GetAppConfiguration();
    }

    public IServiceProvider ConfigureServices(IServiceCollection services)
    {
        services.Configure<FormOptions>(options =>
        {
            options.MultipartBodyLengthLimit = long.MaxValue;
        });

        var mvcBuilder = services.AddControllersWithViews(options =>
        {
            options.Filters.Add(new AbpAutoValidateAntiforgeryTokenAttribute());
            options.AddAbpHtmlSanitizer();
        });
#if DEBUG
        mvcBuilder.AddRazorRuntimeCompilation();
#endif

        services.AddSignalR();

        services.AddCors(options =>
        {
            options.AddPolicy(DefaultCorsPolicyName, builder =>
            {
                builder
                    .SetIsOriginAllowed(_ => true)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });

        if (AppSettingProvider.GetBool("KestrelServer__IsEnabled", _appConfiguration))
        {
            ConfigureKestrel(services);
        }

        IdentityRegistrar.Register(services);
        AuthConfigurer.Configure(services, _appConfiguration);

        if (AppSettingProvider.GetBool("OpenIddict__IsEnabled", _appConfiguration))
        {
            OpenIddictRegistrar.Register(services, _appConfiguration);
            services.Configure<CookieAuthenticationOptions>(IdentityConstants.ApplicationScheme,
                options => { options.LoginPath = "/Ui/Login"; });
        }

        services.Configure<SecurityStampValidatorOptions>(opts =>
        {
            opts.OnRefreshingPrincipal = SecurityStampValidatorCallback.UpdatePrincipal;
        });

        if (WebConsts.SwaggerUiEnabled)
        {
            ConfigureSwagger(services);
        }

        services.AddPasswordlessLoginRateLimit();
        services.AddReportRateLimit();

        services.AddreCAPTCHAV3(x =>
        {
            x.SiteKey = AppSettingProvider.Get("Recaptcha__SiteKey", _appConfiguration);
            x.SiteSecret = AppSettingProvider.Get("Recaptcha__SecretKey", _appConfiguration);
        });

        if (WebConsts.HangfireDashboardEnabled)
        {
            services.AddHangfire(config =>
            {
                var dbConn = AppSettingProvider.Get("DB_CONNECTION_STRING", _appConfiguration);
                config.UseSqlServerStorage(dbConn);
            });

            services.AddHangfireServer();
        }

        if (WebConsts.GraphQL.Enabled)
        {
            services.AddAndConfigureGraphQL();
        }

        if (AppSettingProvider.GetBool("HealthChecks__HealthChecksEnabled", _appConfiguration))
        {
            ConfigureHealthChecks(services);
        }

        return services.AddAbp<HodHodWebHostModule>(options =>
        {
            options.IocManager.IocContainer.AddFacility<LoggingFacility>(
                f => f.UseAbpLog4Net().WithConfig(_hostingEnvironment.IsDevelopment()
                    ? "log4net.config"
                    : "log4net.Production.config")
            );

            options.PlugInSources.AddFolder(Path.Combine(_hostingEnvironment.WebRootPath, "Plugins"),
                SearchOption.AllDirectories);
        });
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
    {
        app.UseAbp(options =>
        {
            options.UseAbpRequestLocalization = false;
        });

        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseHsts();
        }
        else
        {
            app.UseStatusCodePagesWithRedirects("~/Error?statusCode={0}");
            app.UseExceptionHandler("/Error");
        }

        app.UseStaticFiles();

        if (HodHodConsts.PreventNotExistingTenantSubdomains)
        {
            app.UseMiddleware<DomainTenantCheckMiddleware>();
        }

        app.UseRouting();

        app.UseCors(DefaultCorsPolicyName);
        app.UseRateLimiter();

        app.UseAuthentication();
        app.UseJwtTokenMiddleware();

        if (AppSettingProvider.GetBool("OpenIddict__IsEnabled", _appConfiguration))
        {
            app.UseAbpOpenIddictValidation();
        }

        app.UseAuthorization();

        using (var scope = app.ApplicationServices.CreateScope())
        {
            var conn = AppSettingProvider.Get("DB_CONNECTION_STRING", _appConfiguration);

            if (scope.ServiceProvider.GetService<DatabaseCheckHelper>().Exist(conn))
            {
                app.UseAbpRequestLocalization();
            }
        }

        if (WebConsts.HangfireDashboardEnabled)
        {
            app.UseHangfireDashboard(WebConsts.HangfireDashboardEndPoint, new DashboardOptions
            {
                Authorization = new[]
                    {new AbpHangfireAuthorizationFilter(AppPermissions.Pages_Administration_HangfireDashboard)}
            });
        }

        if (AppSettingProvider.GetBool("Payment__Stripe__IsActive", _appConfiguration))
        {
            StripeConfiguration.ApiKey = AppSettingProvider.Get("Payment__Stripe__SecretKey", _appConfiguration);
        }

        if (WebConsts.GraphQL.Enabled)
        {
            app.UseGraphQL<MainSchema>(WebConsts.GraphQL.EndPoint);
            if (WebConsts.GraphQL.PlaygroundEnabled)
            {
                app.UseGraphQLPlayground(WebConsts.GraphQL.PlaygroundEndPoint, new PlaygroundOptions());
            }
        }

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapHub<AbpCommonHub>("/signalr");
            endpoints.MapHub<ChatHub>("/signalr-chat");
            endpoints.MapHub<QrLoginHub>("signalr-qr-login");

            endpoints.MapControllerRoute("defaultWithArea", "{area}/{controller=Home}/{action=Index}/{id?}");
            endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
            endpoints.MapControllerRoute("api", "api/{controller}/{action}");

            app.ApplicationServices.GetRequiredService<IAbpAspNetCoreConfiguration>().EndpointConfiguration
                .ConfigureAllEndpoints(endpoints);
            endpoints.MapControllers();

        });

        if (AppSettingProvider.GetBool("HealthChecks__HealthChecksEnabled", _appConfiguration))
        {
            app.UseHealthChecks("/health", new HealthCheckOptions()
            {
                Predicate = _ => true,
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });

            if (AppSettingProvider.GetBool("HealthChecks__HealthChecksUI__HealthChecksUIEnabled", _appConfiguration))
            {
                app.UseHealthChecksUI();
            }
        }

        if (WebConsts.SwaggerUiEnabled)
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                var swaggerEndpoint = AppSettingProvider.Get("App__SwaggerEndPoint", _appConfiguration);
                var serverRootAddress = AppSettingProvider.Get("App__ServerRootAddress", _appConfiguration);

                options.SwaggerEndpoint(swaggerEndpoint, "HodHod API V1");
                options.IndexStream = () => Assembly.GetExecutingAssembly()
                    .GetManifestResourceStream("HodHod.Web.wwwroot.swagger.ui.index.html");
                options.InjectBaseUrl(serverRootAddress);
            });
        }
    }

    private void ConfigureKestrel(IServiceCollection services)
    {
        services.Configure<Microsoft.AspNetCore.Server.Kestrel.Core.KestrelServerOptions>(options =>
        {
            options.Listen(new System.Net.IPEndPoint(System.Net.IPAddress.Any, 443), listenOptions =>
            {
                var certPassword = AppSettingProvider.Get("Kestrel__Certificates__Default__Password", _appConfiguration);
                var certPath = AppSettingProvider.Get("Kestrel__Certificates__Default__Path", _appConfiguration);

                var cert = new System.Security.Cryptography.X509Certificates.X509Certificate2(certPath, certPassword);
                listenOptions.UseHttps(new HttpsConnectionAdapterOptions()
                {
                    ServerCertificate = cert
                });
            });
        });
    }

    private void ConfigureSwagger(IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo() { Title = "HodHod API", Version = "v1" });
            options.DocInclusionPredicate((docName, description) => true);
            options.ParameterFilter<SwaggerEnumParameterFilter>();
            options.ParameterFilter<SwaggerNullableParameterFilter>();
            options.SchemaFilter<SwaggerEnumSchemaFilter>();
            options.OperationFilter<SwaggerOperationIdFilter>();
            options.OperationFilter<SwaggerOperationFilter>();
            options.CustomDefaultSchemaIdSelector();

            bool canShowSummaries = _appConfiguration.GetValue<bool>("Swagger:ShowSummaries");
            if (canShowSummaries)
            {
                var hostXmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var hostXmlPath = Path.Combine(AppContext.BaseDirectory, hostXmlFile);
                options.IncludeXmlComments(hostXmlPath);

                var applicationXml = $"HodHod.Application.xml";
                var applicationXmlPath = Path.Combine(AppContext.BaseDirectory, applicationXml);
                options.IncludeXmlComments(applicationXmlPath);

                var webCoreXmlFile = $"HodHod.Web.Core.xml";
                var webCoreXmlPath = Path.Combine(AppContext.BaseDirectory, webCoreXmlFile);
                options.IncludeXmlComments(webCoreXmlPath);
            }
        });
    }

    private void ConfigureHealthChecks(IServiceCollection services)
    {
        services.AddAbpZeroHealthCheck();

        var healthCheckUISection = _appConfiguration.GetSection("HealthChecks")?.GetSection("HealthChecksUI");

        if (AppSettingProvider.GetBool("HealthChecks__HealthChecksUI__HealthChecksUIEnabled", _appConfiguration))
        {
            services.Configure<HealthChecksUISettings>(settings =>
            {
                healthCheckUISection.Bind(settings, c => c.BindNonPublicProperties = true);
            });
            services.AddHealthChecksUI().AddInMemoryStorage();
        }
    }
}
