﻿using System.Threading.Tasks;
using Abp;
using Abp.Configuration;
using HodHod.Configuration;
using HodHod.Configuration.Dto;
using HodHod.UiCustomization;
using HodHod.UiCustomization.Dto;

namespace HodHod.Web.UiCustomization.Metronic;

public class Theme5UiCustomizer : UiThemeCustomizerBase, IUiCustomizer
{
    public Theme5UiCustomizer(ISettingManager settingManager)
        : base(settingManager, AppConsts.Theme5)
    {
    }

    public async Task<UiCustomizationSettingsDto> GetUiSettings()
    {
        var settings = new UiCustomizationSettingsDto
        {
            BaseSettings = new ThemeSettingsDto
            {
                Layout = new ThemeLayoutSettingsDto
                {
                    LayoutType = await GetSettingValueAsync(AppSettings.UiManagement.LayoutType),
                    DarkMode = await GetSettingValueAsync<bool>(AppSettings.UiManagement.DarkMode)
                },
                Header = new ThemeHeaderSettingsDto
                {
                    MinimizeDesktopHeaderType = await GetSettingValueAsync(AppSettings.UiManagement.Header.MinimizeType)
                },
                Menu = new ThemeMenuSettingsDto()
                {
                    SearchActive = await GetSettingValueAsync<bool>(AppSettings.UiManagement.SearchActive)
                }
            }
        };

        settings.BaseSettings.Theme = ThemeName;
        settings.BaseSettings.Menu.Position = "top";
        settings.BaseSettings.Menu.AsideSkin = "dark";
        settings.BaseSettings.Footer.DesktopFixedFooter = false;

        settings.BaseSettings.SubHeader.TitleStyle = "page-title d-flex flex-column py-1";
        settings.BaseSettings.SubHeader.ContainerStyle = "toolbar d-flex flex-stack flex-wrap mb-5 mb-lg-7";

        settings.IsLeftMenuUsed = false;
        settings.IsTopMenuUsed = true;
        settings.IsTabMenuUsed = false;

        return settings;
    }

    public async Task UpdateUserUiManagementSettingsAsync(UserIdentifier user, ThemeSettingsDto settings)
    {
        await SettingManager.ChangeSettingForUserAsync(user, AppSettings.UiManagement.Theme, ThemeName);

        await ChangeSettingForUserAsync(user, AppSettings.UiManagement.DarkMode, settings.Layout.DarkMode.ToString());
        await ChangeSettingForUserAsync(user, AppSettings.UiManagement.LayoutType, settings.Layout.LayoutType);
        await ChangeSettingForUserAsync(user, AppSettings.UiManagement.Header.DesktopFixedHeader, settings.Header.DesktopFixedHeader.ToString());
        await ChangeSettingForUserAsync(user, AppSettings.UiManagement.Header.MobileFixedHeader, settings.Header.MobileFixedHeader.ToString());
        await ChangeSettingForUserAsync(user, AppSettings.UiManagement.Header.MinimizeType, settings.Header.MinimizeDesktopHeaderType);
        await ChangeSettingForUserAsync(user, AppSettings.UiManagement.SearchActive, settings.Menu.SearchActive.ToString());
    }

    public async Task UpdateTenantUiManagementSettingsAsync(int tenantId, ThemeSettingsDto settings, UserIdentifier changerUser)
    {
        await SettingManager.ChangeSettingForTenantAsync(tenantId, AppSettings.UiManagement.Theme, ThemeName);

        await ChangeSettingForTenantAsync(tenantId, AppSettings.UiManagement.DarkMode, settings.Layout.DarkMode.ToString());
        await ChangeSettingForTenantAsync(tenantId, AppSettings.UiManagement.LayoutType, settings.Layout.LayoutType);
        await ChangeSettingForTenantAsync(tenantId, AppSettings.UiManagement.Header.DesktopFixedHeader, settings.Header.DesktopFixedHeader.ToString());
        await ChangeSettingForTenantAsync(tenantId, AppSettings.UiManagement.Header.MobileFixedHeader, settings.Header.MobileFixedHeader.ToString());
        await ChangeSettingForTenantAsync(tenantId, AppSettings.UiManagement.Header.MinimizeType, settings.Header.MinimizeDesktopHeaderType);
        await ChangeSettingForTenantAsync(tenantId, AppSettings.UiManagement.SearchActive, settings.Menu.SearchActive.ToString());

        await ResetDarkModeSettingsAsync(changerUser);
    }

    public async Task UpdateApplicationUiManagementSettingsAsync(ThemeSettingsDto settings, UserIdentifier changerUser)
    {
        await SettingManager.ChangeSettingForApplicationAsync(AppSettings.UiManagement.Theme, ThemeName);

        await ChangeSettingForApplicationAsync(AppSettings.UiManagement.DarkMode, settings.Layout.DarkMode.ToString());
        await ChangeSettingForApplicationAsync(AppSettings.UiManagement.LayoutType, settings.Layout.LayoutType);
        await ChangeSettingForApplicationAsync(AppSettings.UiManagement.Header.DesktopFixedHeader, settings.Header.DesktopFixedHeader.ToString());
        await ChangeSettingForApplicationAsync(AppSettings.UiManagement.Header.MobileFixedHeader, settings.Header.MobileFixedHeader.ToString());
        await ChangeSettingForApplicationAsync(AppSettings.UiManagement.Header.MinimizeType, settings.Header.MinimizeDesktopHeaderType);
        await ChangeSettingForApplicationAsync(AppSettings.UiManagement.SearchActive, settings.Menu.SearchActive.ToString());

        await ResetDarkModeSettingsAsync(changerUser);
    }

    public async Task<ThemeSettingsDto> GetHostUiManagementSettings()
    {
        var theme = await SettingManager.GetSettingValueForApplicationAsync(AppSettings.UiManagement.Theme);

        return new ThemeSettingsDto
        {
            Theme = theme,
            Layout = new ThemeLayoutSettingsDto
            {
                LayoutType = await GetSettingValueForApplicationAsync(AppSettings.UiManagement.LayoutType),
                DarkMode = await GetSettingValueForApplicationAsync<bool>(AppSettings.UiManagement.DarkMode)
            },
            Header = new ThemeHeaderSettingsDto
            {
                MinimizeDesktopHeaderType = await GetSettingValueForApplicationAsync(AppSettings.UiManagement.Header.MinimizeType)
            },
            Menu = new ThemeMenuSettingsDto
            {
                SearchActive = await GetSettingValueForApplicationAsync<bool>(AppSettings.UiManagement.SearchActive)
            }
        };
    }

    public async Task<ThemeSettingsDto> GetTenantUiCustomizationSettings(int tenantId)
    {
        var theme = await SettingManager.GetSettingValueForTenantAsync(AppSettings.UiManagement.Theme, tenantId);

        return new ThemeSettingsDto
        {
            Theme = theme,
            Layout = new ThemeLayoutSettingsDto
            {
                LayoutType = await GetSettingValueForTenantAsync(AppSettings.UiManagement.LayoutType, tenantId),
                DarkMode = await GetSettingValueForTenantAsync<bool>(AppSettings.UiManagement.DarkMode, tenantId)
            },
            Header = new ThemeHeaderSettingsDto
            {
                MinimizeDesktopHeaderType = await GetSettingValueForTenantAsync(AppSettings.UiManagement.Header.MinimizeType, tenantId)
            },
            Menu = new ThemeMenuSettingsDto()
            {
                SearchActive = await GetSettingValueForTenantAsync<bool>(AppSettings.UiManagement.SearchActive, tenantId)
            }
        };
    }

    public override Task<string> GetBodyClass()
    {
        return Task.FromResult("header-fixed header-tablet-and-mobile-fixed aside-enabled");
    }
}

