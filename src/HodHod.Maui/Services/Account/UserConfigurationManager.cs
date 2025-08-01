﻿using HodHod.ApiClient;
using HodHod.Configuration;
using HodHod.MultiTenancy;
using System.Globalization;
using HodHod.Maui.Core;
using HodHod.Maui.Core.Helpers;
using HodHod.Maui.Core.Localization;
using HodHod.Maui.Core.Threading;
using HodHod.Maui.Services.UI;

namespace HodHod.Maui.Services.Account;

public static class UserConfigurationManager
{
    private static readonly Lazy<IApplicationContext> AppContext = new Lazy<IApplicationContext>(
        DependencyResolver.Resolve<IApplicationContext>
    );

    private static IAccessTokenManager AccessTokenManager => DependencyResolver.Resolve<IAccessTokenManager>();
    private static UserDialogsService UserDialogsService => DependencyResolver.Resolve<UserDialogsService>();

    public static bool HasConfiguration => AppContext.Value.Configuration != null;

    public static async Task GetAsync(Func<Task> successCallback = null)
    {
        var userConfigurationService = DependencyResolver.Resolve<UserConfigurationService>();
        userConfigurationService.OnAccessTokenRefresh = App.OnAccessTokenRefresh;
        userConfigurationService.OnSessionTimeOut = App.OnSessionTimeout;

        await WebRequestExecuter.Execute(
            async () => await userConfigurationService.GetAsync(AccessTokenManager.IsUserLoggedIn),
            async result =>
            {
                AppContext.Value.Configuration = result;
                SetCurrentCulture();
                if (!result.MultiTenancy.IsEnabled)
                {
                    AppContext.Value.SetAsTenant(TenantConsts.DefaultTenantName, TenantConsts.DefaultTenantId);
                }

                AppContext.Value.CurrentLanguage = result.Localization.CurrentLanguage;
                await WarnIfUserHasNoPermission();
                if (successCallback != null)
                {
                    await successCallback();
                }
            },
            _ =>
            {
                CurrentApplicationCloser.Quit();
                return Task.CompletedTask;
            },
            () => DependencyResolver
                .IocManager
                .Release(userConfigurationService)
        );
    }

    private static async Task WarnIfUserHasNoPermission()
    {
        if (!AccessTokenManager.IsUserLoggedIn)
        {
            return;
        }

        var hasAnyPermission = AppContext.Value.Configuration.Auth.GrantedPermissions != null &&
                               AppContext.Value.Configuration.Auth.GrantedPermissions.Any();

        if (!hasAnyPermission)
        {
            await UserDialogsService.AlertWarn("NoPermission");
        }
    }

    private static void SetCurrentCulture()
    {
        var locale = DependencyResolver.Resolve<ILocale>();
        var userCulture = GetUserCulture(locale);

        locale.SetLocale(userCulture);
    }

    private static CultureInfo GetUserCulture(ILocale locale)
    {
        if (AppContext.Value.Configuration.Localization.CurrentCulture.Name == null)
        {
            return locale.GetCurrentCultureInfo();
        }

        try
        {
            return new CultureInfo(AppContext.Value.Configuration.Localization.CurrentCulture.Name);
        }
        catch (CultureNotFoundException)
        {
            return locale.GetCurrentCultureInfo();
        }
    }

}