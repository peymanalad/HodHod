﻿using System;
using Abp.Auditing;
using Abp.Authorization;
using Abp.Authorization.Users;
using Abp.Configuration.Startup;
using Abp.UI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using HodHod.Authorization;
using HodHod.Authorization.Accounts;
using HodHod.Authorization.Accounts.Dto;
using HodHod.Authorization.Users;
using HodHod.Configuration;
using HodHod.EntityFrameworkCore;
using HodHod.Identity;
using HodHod.MultiTenancy;
using HodHod.Sessions.Dto;
using HodHod.Web.Models.Ui;
using HodHod.Web.Session;
using System.Threading.Tasks;

namespace HodHod.Web.Controllers;

public class UiController : HodHodControllerBase
{
    private readonly IPerRequestSessionCache _sessionCache;
    private readonly IMultiTenancyConfig _multiTenancyConfig;
    private readonly IAccountAppService _accountAppService;
    private readonly LogInManager _logInManager;
    private readonly SignInManager _signInManager;
    private readonly AbpLoginResultTypeHelper _abpLoginResultTypeHelper;
    private DatabaseCheckHelper _databaseCheckHelper;
    private readonly IConfigurationRoot _appConfiguration;

    public UiController(
        IPerRequestSessionCache sessionCache,
        IMultiTenancyConfig multiTenancyConfig,
        IAccountAppService accountAppService,
        LogInManager logInManager,
        SignInManager signInManager,
        AbpLoginResultTypeHelper abpLoginResultTypeHelper,
        DatabaseCheckHelper databaseCheckHelper,
        IAppConfigurationAccessor appConfigurationAccessor)
    {
        _sessionCache = sessionCache;
        _multiTenancyConfig = multiTenancyConfig;
        _accountAppService = accountAppService;
        _logInManager = logInManager;
        _signInManager = signInManager;
        _abpLoginResultTypeHelper = abpLoginResultTypeHelper;
        _databaseCheckHelper = databaseCheckHelper;
        _appConfiguration = appConfigurationAccessor.Configuration;
    }

    [DisableAuditing]
    public async Task<IActionResult> Index()
    {
        var model = new HomePageModel();

        var conn = ConnectionStringProvider.Get(_appConfiguration);

        if (_databaseCheckHelper.Exist(conn))
        {
            model.LoginInformation = await _sessionCache.GetCurrentLoginInformationsAsync();
            model.IsMultiTenancyEnabled = _multiTenancyConfig.IsEnabled;
            model.DatabaseExists = true;

            if (model.LoginInformation?.User == null)
            {
                return RedirectToAction("Login");
            }
        }

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Login(string returnUrl = "")
    {
        if (!string.IsNullOrEmpty(returnUrl))
        {
            ViewBag.ReturnUrl = returnUrl;
        }

        await _signInManager.SignOutAsync();

        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginModel model, string returnUrl = "")
    {
        if (model.TenancyName != null)
        {
            var isTenantAvailable = await _accountAppService.IsTenantAvailable(new IsTenantAvailableInput
            {
                TenancyName = model.TenancyName
            });

            switch (isTenantAvailable.State)
            {
                case TenantAvailabilityState.InActive:
                    throw new UserFriendlyException(L("TenantIsNotActive", model.TenancyName));
                case TenantAvailabilityState.NotFound:
                    throw new UserFriendlyException(L("ThereIsNoTenantDefinedWithName{0}", model.TenancyName));
            }
        }

        var loginResult = await GetLoginResultAsync(model.UserNameOrEmailAddress, model.Password, model.TenancyName);

        if (loginResult.User.ShouldChangePasswordOnNextLogin)
        {
            throw new UserFriendlyException(L("RequiresPasswordChange"));
        }

        var signInResult = await _signInManager.SignInOrTwoFactorAsync(loginResult, model.RememberMe);

        if (signInResult.RequiresTwoFactor)
        {
            throw new UserFriendlyException(L("RequiresTwoFactorAuth"));
        }

        if (!string.IsNullOrEmpty(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction("Index");
    }

    public async Task<ActionResult> Logout()
    {
        await _signInManager.SignOutAsync();

        return RedirectToAction("Index");
    }

    private async Task<AbpLoginResult<Tenant, User>> GetLoginResultAsync(string usernameOrEmailAddress, string password, string tenancyName)
    {
        var loginResult = await _logInManager.LoginAsync(usernameOrEmailAddress, password, tenancyName);

        switch (loginResult.Result)
        {
            case AbpLoginResultType.Success:
                return loginResult;
            default:
                throw _abpLoginResultTypeHelper.CreateExceptionForFailedLoginAttempt(loginResult.Result, usernameOrEmailAddress, tenancyName);
        }
    }
}

