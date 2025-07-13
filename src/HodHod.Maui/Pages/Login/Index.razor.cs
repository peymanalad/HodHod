﻿using System.ComponentModel.DataAnnotations;
using HodHod.ApiClient;
using HodHod.Authorization.Accounts;
using HodHod.Authorization.Accounts.Dto;
using HodHod.Maui.Core.Components;
using HodHod.Maui.Core.Threading;
using HodHod.Maui.Services.Account;

namespace HodHod.Maui.Pages.Login;

public partial class Index : HodHodComponentBase
{
    [Required]
    public string UserName
    {
        get => _accountService.AbpAuthenticateModel.UserNameOrEmailAddress;
        set
        {
            _accountService.AbpAuthenticateModel.UserNameOrEmailAddress = value;
        }
    }

    [Required]
    public string Password
    {
        get => _accountService.AbpAuthenticateModel.Password;
        set
        {
            _accountService.AbpAuthenticateModel.Password = value;
        }
    }

    private readonly IAccountService _accountService = Resolve<IAccountService>();
    private readonly IAccountAppService _accountAppService = Resolve<IAccountAppService>();
    private readonly IApplicationContext _applicationContext = Resolve<IApplicationContext>();

    SwitchTenantModal switchTenantModal;
    EmailActivationModal emailActivationModal;
    ForgotPasswordModal forgotPasswordModal;

    public string CurrentTenancyNameOrDefault => _applicationContext.CurrentTenant != null
        ? _applicationContext.CurrentTenant.TenancyName
        : L("NotSelected");


    protected override async Task OnInitializedAsync()
    {
        await _accountService.LogoutAsync();
        ClearInputs();
    }

    private async Task LoginUser()
    {
        await _accountService.LoginUserAsync();
    }

    private void ClearInputs()
    {
        UserName = "";
        Password = "";
        _accountService.AbpAuthenticateModel.TwoFactorVerificationCode = "";
    }

    private async Task SwitchTenantButton()
    {
        await switchTenantModal.Show();
    }

    private async Task EmailActivationButton()
    {
        await emailActivationModal.Show();
    }

    private async Task ForgotPasswordButton()
    {
        await forgotPasswordModal.Show();
    }

    public async Task OnSwitchTenantSave(string tenantName)
    {
        if (string.IsNullOrEmpty(tenantName))
        {
            _applicationContext.SetAsHost();
            ApiUrlConfig.ResetBaseUrl();
        }
        else
        {
            await SetTenantAsync(tenantName);
        }
    }

    private async Task SetTenantAsync(string tenancyName)
    {
        await SetBusyAsync(async () =>
        {
            await WebRequestExecuter.Execute(
                async () => await _accountAppService.IsTenantAvailable(
                    new IsTenantAvailableInput { TenancyName = tenancyName }),
                result => IsTenantAvailableExecuted(result, tenancyName)
            );
        });
    }

    private async Task IsTenantAvailableExecuted(IsTenantAvailableOutput result, string tenancyName)
    {
        var tenantAvailableResult = result;

        switch (tenantAvailableResult.State)
        {
            case TenantAvailabilityState.Available:
                _applicationContext.SetAsTenant(tenancyName, tenantAvailableResult.TenantId.Value);
                ApiUrlConfig.ChangeBaseUrl(tenantAvailableResult.ServerRootAddress);
                break;
            case TenantAvailabilityState.InActive:
                await UserDialogsService.UnBlock();
                await UserDialogsService.AlertError(L("TenantIsNotActive", tenancyName));
                break;
            case TenantAvailabilityState.NotFound:
                await UserDialogsService.UnBlock();
                await UserDialogsService.AlertError(L("ThereIsNoTenantDefinedWithName{0}", tenancyName));
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    public async Task OnEmailActivation()
    {
        await emailActivationModal.Hide();
        await UserDialogsService.AlertSuccess(L("SendEmailActivationLink_Information"));
    }

    public async Task OnForgotPassword()
    {
        await forgotPasswordModal.Hide();
        await UserDialogsService.AlertSuccess(L("PasswordResetMailSentMessage"));
    }

    private void OnLanguageSwitchAsync()
    {
        StateHasChanged();
    }
}