using Abp.AspNetCore.Mvc.Controllers;
using Abp.Auditing;
using Abp.Domain.Uow;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using HodHod.Configuration;
using HodHod.Configuration.Dto;
using HodHod.Configuration.Host.Dto;
using HodHod.EntityFrameworkCore;
using HodHod.Install;
using HodHod.Install.Dto;
using HodHod.Migrations.Seed.Host;
using HodHod.Web.Models.Install;
using System.Threading.Tasks;

namespace HodHod.Web.Controllers;

[DisableAuditing]
public class InstallController : AbpController
{
    private readonly IInstallAppService _installAppService;
    private readonly IHostApplicationLifetime _applicationLifetime;
    private readonly DatabaseCheckHelper _databaseCheckHelper;
    private readonly IConfigurationRoot _appConfiguration;

    public InstallController(
        IInstallAppService installAppService,
        IHostApplicationLifetime applicationLifetime,
        DatabaseCheckHelper databaseCheckHelper,
        IAppConfigurationAccessor appConfigurationAccessor)
    {
        _installAppService = installAppService;
        _applicationLifetime = applicationLifetime;
        _databaseCheckHelper = databaseCheckHelper;
        _appConfiguration = appConfigurationAccessor.Configuration;
    }

    [UnitOfWork(IsDisabled = true)]
    public ActionResult Index()
    {
        var appSettings = _installAppService.GetAppSettingsJson();
        var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING")
                               ?? Environment.GetEnvironmentVariable($"ConnectionStrings__{HodHodConsts.ConnectionStringName}")
                               ?? _appConfiguration[$"ConnectionStrings:{HodHodConsts.ConnectionStringName}"];
        if (_databaseCheckHelper.Exist(connectionString))
        {
            return RedirectToAction("Index", "Home");
        }

        var model = new InstallSetupViewModel
        {
            Languages = DefaultLanguagesCreator.InitialLanguages,
            AppSettingsJson = appSettings
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(InstallSetupViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.Languages = DefaultLanguagesCreator.InitialLanguages;
            return View(model);
        }

        await _installAppService.Setup(new InstallDto
        {
            ConnectionString = model.ConnectionString,
            AdminPassword = model.AdminPassword,
            WebSiteUrl = model.WebSiteUrl,
            DefaultLanguage = model.DefaultLanguage,
            SmtpSettings = new EmailSettingsEditDto
            {
                DefaultFromAddress = model.DefaultFromAddress,
                DefaultFromDisplayName = model.DefaultFromDisplayName,
                SmtpHost = model.SmtpHost,
                SmtpPort = model.SmtpPort ?? 0,
                SmtpEnableSsl = model.SmtpEnableSsl,
                SmtpUseAuthentication = model.SmtpUseAuthentication,
                SmtpDomain = model.SmtpDomain,
                SmtpUserName = model.SmtpUserName,
                SmtpPassword = model.SmtpPassword
            },
            BillInfo = new HostBillingSettingsEditDto
            {
                LegalName = model.LegalName,
                Address = model.BillAddress
            },
        });

        return RedirectToAction("Restart");
    }


    public ActionResult Restart()
    {
        _applicationLifetime.StopApplication();
        return View();
    }
}