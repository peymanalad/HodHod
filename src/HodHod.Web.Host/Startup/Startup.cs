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
        _hostingEnvironment = env;
        _appConfiguration = env.GetAppConfiguration();
    }

    public IServiceProvider ConfigureServices(IServiceCollection services)
    {
        services.Configure<FormOptions>(options =>
        {
            //options.MultipartBodyLengthLimit = 50 * 1024 * 1024; // 50MB
            options.MultipartBodyLengthLimit = long.MaxValue;
        });

        //MVC
        var mvcBuilder = services.AddControllersWithViews(options =>
        {
            options.Filters.Add(new AbpAutoValidateAntiforgeryTokenAttribute());
            options.AddAbpHtmlSanitizer();
        });
#if DEBUG
        mvcBuilder.AddRazorRuntimeCompilation();
#endif

        services.AddSignalR();

        //Configure CORS for angular2 UI
        services.AddCors(options =>
        {
            options.AddPolicy(DefaultCorsPolicyName, builder =>
            {
                //App:CorsOrigins in appsettings.json can contain more than one address with splitted by comma.
                builder
                    //.WithOrigins(
                    //    // App:CorsOrigins in appsettings.json can contain more than one address separated by comma.
                    //    Environment.GetEnvironmentVariable("App:CorsOrigins"]
                    //        .Split(",", StringSplitOptions.RemoveEmptyEntries)
                    //        .Select(o => o.RemovePostFix("/"))
                    //        .ToArray()
                    //)
                    //.SetIsOriginAllowedToAllowWildcardSubdomains()
                    .SetIsOriginAllowed(_=> true)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });

        if (bool.Parse(Environment.GetEnvironmentVariable("KestrelServer:IsEnabled") ?? "false"))
        {
            ConfigureKestrel(services);
        }

        IdentityRegistrar.Register(services);
        AuthConfigurer.Configure(services, _appConfiguration);

        if (bool.TryParse(Environment.GetEnvironmentVariable("OpenIddict:IsEnabled"), out var isEnabled) && isEnabled)
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
            //Swagger - Enable this line and the related lines in Configure method to enable swagger UI
            ConfigureSwagger(services);
        }

        services.AddPasswordlessLoginRateLimit();
        services.AddReportRateLimit();

        //Recaptcha
        services.AddreCAPTCHAV3(x =>
        {
            x.SiteKey = Environment.GetEnvironmentVariable("Recaptcha:SiteKey");
            x.SiteSecret = Environment.GetEnvironmentVariable("Recaptcha:SecretKey");
        });

        if (WebConsts.HangfireDashboardEnabled)
        {
            //Hangfire(Enable to use Hangfire instead of default job manager)
            services.AddHangfire(config =>
            {
                //config.UseSqlServerStorage(_appConfiguration.GetConnectionString("Default"));
                var dbConn = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING") ?? _appConfiguration.GetConnectionString("Default");
                config.UseSqlServerStorage(dbConn);
            });

            services.AddHangfireServer();
        }

        if (WebConsts.GraphQL.Enabled)
        {
            services.AddAndConfigureGraphQL();
        }

        if (bool.TryParse(Environment.GetEnvironmentVariable("HealthChecks:HealthChecksEnabled"), out var healthChecksEnabled) && healthChecksEnabled)
        {
            ConfigureHealthChecks(services);
        }

        //Configure Abp and Dependency Injection
        return services.AddAbp<HodHodWebHostModule>(options =>
        {
            //Configure Log4Net logging
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
        //Initializes ABP framework.
        app.UseAbp(options =>
        {
            options.UseAbpRequestLocalization = false; //used below: UseAbpRequestLocalization
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

#pragma warning disable CS0162
        if (HodHodConsts.PreventNotExistingTenantSubdomains)
        {
            app.UseMiddleware<DomainTenantCheckMiddleware>();
        }

        app.UseRouting();

        app.UseCors(DefaultCorsPolicyName); //Enable CORS!
        app.UseRateLimiter();

        app.UseAuthentication();
        app.UseJwtTokenMiddleware();

        if (bool.Parse(Environment.GetEnvironmentVariable("OpenIddict_IsEnabled")))
        {
            app.UseAbpOpenIddictValidation();
        }

        app.UseAuthorization();

        using (var scope = app.ApplicationServices.CreateScope())
        {
            if (scope.ServiceProvider.GetService<DatabaseCheckHelper>()
                .Exist(Environment.GetEnvironmentVariable("ConnectionStrings:Default")))
            {
                app.UseAbpRequestLocalization();
            }
        }

        if (WebConsts.HangfireDashboardEnabled)
        {
            //Hangfire dashboard &server(Enable to use Hangfire instead of default job manager)
            app.UseHangfireDashboard(WebConsts.HangfireDashboardEndPoint, new DashboardOptions
            {
                Authorization = new[]
                    {new AbpHangfireAuthorizationFilter(AppPermissions.Pages_Administration_HangfireDashboard)}
            });
        }

        if (bool.Parse(Environment.GetEnvironmentVariable("Payment_Stripe_IsActive")))
        {
            StripeConfiguration.ApiKey = Environment.GetEnvironmentVariable("Payment_Stripe_SecretKey");
        }

        if (WebConsts.GraphQL.Enabled)
        {
            app.UseGraphQL<MainSchema>(WebConsts.GraphQL.EndPoint);
            if (WebConsts.GraphQL.PlaygroundEnabled)
            {
                // to explorer API navigate https://*DOMAIN*/ui/playground
                app.UseGraphQLPlayground(
                    WebConsts.GraphQL.PlaygroundEndPoint,
                    new PlaygroundOptions()
                );
            }
        }

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapHub<AbpCommonHub>("/signalr");
            endpoints.MapHub<ChatHub>("/signalr-chat");
            endpoints.MapHub<QrLoginHub>("signalr-qr-login");

            endpoints.MapControllerRoute("defaultWithArea", "{area}/{controller=Home}/{action=Index}/{id?}");
            endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");

            app.ApplicationServices.GetRequiredService<IAbpAspNetCoreConfiguration>().EndpointConfiguration
                .ConfigureAllEndpoints(endpoints);
        });

        if (bool.Parse(Environment.GetEnvironmentVariable("HealthChecks_HealthChecksEnabled")))
        {
            app.UseHealthChecks("/health", new HealthCheckOptions()
            {
                Predicate = _ => true,
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });

            if (bool.Parse(Environment.GetEnvironmentVariable("HealthChecks_HealthChecksUI_HealthChecksUIEnabled")))
            {
                app.UseHealthChecksUI();
            }
        }

        if (WebConsts.SwaggerUiEnabled)
        {
            // Enable middleware to serve generated Swagger as a JSON endpoint
            app.UseSwagger();
            // Enable middleware to serve swagger-ui assets (HTML, JS, CSS etc.)

            app.UseSwaggerUI(options =>
            {
                var swaggerEndpoint = Environment.GetEnvironmentVariable("App_SwaggerEndPoint");
                var serverRootAddress = Environment.GetEnvironmentVariable("App_ServerRootAddress");

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
            options.Listen(new System.Net.IPEndPoint(System.Net.IPAddress.Any, 443),
                listenOptions =>
                {
                    var certPassword = Environment.GetEnvironmentVariable("Kestrel_Certificates_Default_Password");
                    var certPath = Environment.GetEnvironmentVariable("Kestrel_Certificates_Default_Path");

                    var cert = new System.Security.Cryptography.X509Certificates.X509Certificate2(certPath,
                        certPassword);
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

            //add summaries to swagger
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

        if (bool.Parse(healthCheckUISection["HealthChecksUIEnabled"]))
        {
            services.Configure<HealthChecksUISettings>(settings =>
            {
                healthCheckUISection.Bind(settings, c => c.BindNonPublicProperties = true);
            });
            services.AddHealthChecksUI()
                .AddInMemoryStorage();
        }
    }
}

