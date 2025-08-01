﻿(function ($) {
  $(function () {
    var _hostSettingsService = abp.services.app.hostSettings;
    var _initialTimeZone = $('#GeneralSettingsForm [name=Timezone]').val();
    var _usingDefaultTimeZone =
      $('#GeneralSettingsForm [name=TimezoneForComparison]').val() === abp.setting.values['Abp.Timing.TimeZone'];
    var _openIdConnectClaimsManager = new KeyValueListManager();
    var _wsFederationClaimsManager = new KeyValueListManager();
    var _$tabPanel = $('#SettingsTabPanel');
    var _initialEmailSettings = $('#EmailSmtpSettingsForm').serializeFormToObject();

    var _$smtpCredentialFormGroups = _$tabPanel
      .find('input[name=SmtpDomain],input[name=SmtpUserName],input[name=SmtpPassword]')
      .closest('.form-group');

    var _$tenantSettingsCheckboxes = _$tabPanel
      .find('input[name=IsNewRegisteredTenantActiveByDefault],input[name=UseCaptchaOnRegistration]')
      .closest('.form-group');

    var _$userLockOutSettingsFormItems = $('#UserLockOutSettingsForm')
      .find('input')
      .not('#Setting_UserLockOut_IsEnabled')
      .closest('.form-group');

    var _$twoFactorLoginSettingsFormItems = $('#TwoFactorLoginSettingsForm')
      .find('input')
      .not('#Setting_TwoFactorLogin_IsEnabled')
      .closest('.form-group');

    var _$SessionTimeOutItems = _$tabPanel.find('.divSessionTimeOut');

    function toggleSmtpCredentialFormGroups() {
      if ($('#Settings_SmtpUseAuthentication').is(':checked')) {
        _$smtpCredentialFormGroups.slideUp('fast');
      } else {
        _$smtpCredentialFormGroups.slideDown('fast');
      }
    }

    function toggleTenantManagementFormGroups() {
      if (!$('#Setting_AllowSelfRegistration').is(':checked')) {
        _$tenantSettingsCheckboxes.slideUp('fast');
      } else {
        _$tenantSettingsCheckboxes.slideDown('fast');
      }
    }

    function toggleUserLockOutSettingsFormItems() {
      if ($('#Setting_UserLockOut_IsEnabled').is(':checked')) {
        _$userLockOutSettingsFormItems.slideDown('fast');
      } else {
        _$userLockOutSettingsFormItems.slideUp('fast');
      }
    }

    function toggleTwoFactorLoginSettingsFormItems() {
      if ($('#Setting_TwoFactorLogin_IsEnabled').is(':checked')) {
        _$twoFactorLoginSettingsFormItems.slideDown('fast');
      } else {
        _$twoFactorLoginSettingsFormItems.slideUp('fast');
      }
    }

    function toggleSessionTimeOutItems() {
      if ($('#Setting_IsSessionTimeOutEnabled').is(':checked')) {
        _$SessionTimeOutItems.slideDown('fast');
      } else {
        _$SessionTimeOutItems.slideUp('fast');
      }
    }

    toggleSmtpCredentialFormGroups();
    toggleTenantManagementFormGroups();
    toggleUserLockOutSettingsFormItems();
    toggleTwoFactorLoginSettingsFormItems();
    toggleSessionTimeOutItems();

    $('#Settings_SmtpUseAuthentication').change(function () {
      toggleSmtpCredentialFormGroups();
    });

    $('#Setting_AllowSelfRegistration').change(function () {
      toggleTenantManagementFormGroups();
    });

    $('#Setting_UserLockOut_IsEnabled').change(function () {
      toggleUserLockOutSettingsFormItems();
    });

    $('#Setting_TwoFactorLogin_IsEnabled').change(function () {
      toggleTwoFactorLoginSettingsFormItems();
    });

    $('#Setting_IsSessionTimeOutEnabled').change(function () {
      toggleSessionTimeOutItems();
    });

    $('#Setting_PasswordComplexity_UseDefaultSettings').change(function (val) {
      if ($('#Setting_PasswordComplexity_UseDefaultSettings').is(':checked')) {
        $('#PasswordComplexitySettingsForm').hide('fast', function () {
          $('#DefaultPasswordComplexitySettingsForm').show('fast');
        });
      } else {
        $('#DefaultPasswordComplexitySettingsForm').hide('fast', function () {
          $('#PasswordComplexitySettingsForm').show('fast');
        });
      }
    });

    function getDefaultPasswordComplexitySettings() {
      //note: this is a fix for '$('#DefaultPasswordComplexitySettingsForm').serializeFormToObject()' always returns true for checkboxes if they are disabled.
      var $disabledDefaultPasswordInputs = $('#DefaultPasswordComplexitySettingsForm input:disabled');
      $disabledDefaultPasswordInputs.removeAttr('disabled');
      var defaultPasswordComplexitySettings = $('#DefaultPasswordComplexitySettingsForm').serializeFormToObject();
      $disabledDefaultPasswordInputs.attr('disabled', 'disabled');
      return defaultPasswordComplexitySettings;
    }

    function getSelectedOpenIdResponseTypes() {
      var openIdResponseTypes = '';
      if ($('#Setting_OpenIdConnect_ResponseType_Token').prop('checked')) {
        openIdResponseTypes += 'token';
      }
      if ($('#Setting_OpenIdConnect_ResponseType_IdToken').prop('checked')) {
        if (openIdResponseTypes.length > 0) {
          openIdResponseTypes += ',';
        }
        openIdResponseTypes += 'id_token';
      }
      if ($('#Setting_OpenIdConnect_ResponseType_Code').prop('checked')) {
        if (openIdResponseTypes.length > 0) {
          openIdResponseTypes += ',';
        }
        openIdResponseTypes += 'code';
      }

      return openIdResponseTypes;
    }

    $('#SaveAllSettingsButton').click(function () {
      if (!IsSmtpSettingsFormValid()) {
        return;
      }

      if (!IsUserManagementSettingsFormValid()) {
        return;
      }

      if (!IsUserLockOutSettingsFormValid()) {
        return;
      }

      var userManagement = $('#UserManagementSettingsForm').serializeFormToObject();
      userManagement.sessionTimeOutSettings = {
        isEnabled: $('#Setting_IsSessionTimeOutEnabled').is(':checked'),
        timeOutSecond: $('#Setting_SessionTimeOutSecond').val(),
        showTimeOutNotificationSecond: $('#Setting_ShowTimeOutNotificationSecond').val(),
        showLockScreenWhenTimedOut: $('#Setting_ShowLockScreenWhenTimedOut').is(':checked'),
      };

      userManagement.passwordlessLogin = {
        isEmailProviderEnabledForApplication: $('#Setting_IsEmailPasswordlessLoginEnabled').is(':checked'),
        isSmsProviderEnabledForApplication: $('#Setting_IsSmsPasswordlessLoginEnabled').is(':checked'),
      };

      _hostSettingsService
        .updateAllSettings({
          general: $('#GeneralSettingsForm').serializeFormToObject(),
          tenantManagement: {
            allowSelfRegistration: $('#Setting_AllowSelfRegistration').is(':checked'),
            isNewRegisteredTenantActiveByDefault: $('#Setting_IsNewRegisteredTenantActiveByDefault').is(':checked'),
            defaultEditionId: $('#DefaultEditionId').val(),
            isRestrictedEmailDomainEnabled: $('#Settings_IsRestrictedEmailDomainEnabled').is(':checked'),
            captchaSettings: {
              useCaptchaOnRegistration: $('#Setting_UseCaptchaOnRegistration').is(':checked'),
              useCaptchaOnResetPassword: $('#Setting_UseCaptchaOnResetPassword').is(':checked'),
              useCaptchaOnEmailActivation: $('#Setting_UseCaptchaOnEmailActivation').is(':checked'),
            },
          },
          userManagement: userManagement,
          email: $('#EmailSmtpSettingsForm').serializeFormToObject(),
          chat: $('#ChatSettingsForm').serializeFormToObject(),
          billing: $('#BillingSettingsForm').serializeFormToObject(),
          otherSettings: $('#OtherSettingsForm').serializeFormToObject(),
          security: {
            useDefaultPasswordComplexitySettings: $('#Setting_PasswordComplexity_UseDefaultSettings').is(':checked'),
            passwordComplexity: $('#PasswordComplexitySettingsForm').serializeFormToObject(),
            defaultPasswordComplexity: getDefaultPasswordComplexitySettings(),
            userLockOut: $('#UserLockOutSettingsForm').serializeFormToObject(),
            twoFactorLogin: $('#TwoFactorLoginSettingsForm').serializeFormToObject(),
            AllowOneConcurrentLoginPerUser: $('#Setting_AllowOneConcurrentLoginPerUser').is(':checked'),
            userPasswordSettings: {
              enableCheckingLastXPasswordWhenPasswordChange: $(
                '#Setting_EnableCheckingLastXPasswordWhenPasswordChange',
              ).is(':checked'),
              checkingLastXPasswordCount: $('#Setting_CheckingLastXPasswordCount').val(),
              enablePasswordExpiration: $('#Setting_EnablePasswordExpiration').is(':checked'),
              passwordExpirationDayCount: $('#Setting_PasswordExpirationDayCount').val(),
              passwordResetCodeExpirationHours: $('#Setting_PasswordResetCodeExpirationHours').val(),
            },
          },
          externalLoginProviderSettings: {
            facebook: {
              appId: $('#Setting_Facebook_AppId').val(),
              appSecret: $('#Setting_Facebook_AppSecret').val(),
            },
            google: {
              clientId: $('#Setting_Google_ClientId').val(),
              clientSecret: $('#Setting_Google_ClientSecret').val(),
              userInfoEndpoint: $('#Setting_Google_UserInfoEndpoint').val(),
            },
            twitter: {
              consumerKey: $('#Setting_Twitter_ConsumerKey').val(),
              consumerSecret: $('#Setting_Twitter_ConsumerSecret').val(),
            },
            microsoft: {
              clientId: $('#Setting_Microsoft_ClientId').val(),
              clientSecret: $('#Setting_Microsoft_ClientSecret').val(),
            },
            openIdConnect: {
              clientId: $('#Setting_OpenIdConnect_ClientId').val(),
              clientSecret: $('#Setting_OpenIdConnect_ClientSecret').val(),
              authority: $('#Setting_OpenIdConnect_Authority').val(),
              validateIssuer: $('#Setting_OpenIdConnect_ValidateIssuer').prop('checked'),
              responseType: getSelectedOpenIdResponseTypes(),
            },
            openIdConnectClaimsMapping: _openIdConnectClaimsManager.getValues().map((x) => {
              return {
                key: x.key,
                claim: x.value,
              };
            }),
            wsFederation: {
              clientId: $('#Setting_WsFederation_ClientId').val(),
              tenant: $('#Setting_WsFederation_Tenant').val(),
              metaDataAddress: $('#Setting_WsFederation_MetaDataAddress').val(),
              wtrealm: $('#Setting_WsFederation_Wtrealm').val(),
              authority: $('#Setting_WsFederation_Authority').val(),
            },
            wsFederationClaimsMapping: _wsFederationClaimsManager.getValues().map((x) => {
              return {
                key: x.key,
                claim: x.value,
              };
            }),
          },
        })
        .done(function () {
          abp.notify.info(app.localize('SavedSuccessfully'));

          var newTimezone = $('#GeneralSettingsForm [name=Timezone]').val();
          if (
            abp.clock.provider.supportsMultipleTimezone &&
            _usingDefaultTimeZone &&
            _initialTimeZone !== newTimezone
          ) {
            abp.message.info(app.localize('TimeZoneSettingChangedRefreshPageNotification')).done(function () {
              window.location.reload();
            });
          }
          _initialEmailSettings = $('#EmailSmtpSettingsForm').serializeFormToObject();
        });
    });

    $('#Settings_SmtpUseAuthentication').change(function () {
      $('#SmtpAuthenticationDiv').toggle();
    });

    $('#SendTestEmailButton').click(function () {
      if (!$('#EmailSmtpSettingsTestForm').valid()) {
        return;
      }

      var currentEmailSettings = $('#EmailSmtpSettingsForm').serializeFormToObject();

      if (JSON.stringify(_initialEmailSettings) !== JSON.stringify(currentEmailSettings)) {
        abp.message.confirm(
          app.localize('SendEmailWithSavedSettingsWarning'),
          app.localize('AreYouSure'),
          function (isConfirmed) {
            if (isConfirmed) {
              _hostSettingsService
                .sendTestEmail({
                  emailAddress: $('#TestEmailAddressInput').val(),
                })
                .done(function () {
                  abp.notify.info(app.localize('TestEmailSentSuccessfully'));
                });
            }
          },
        );
      } else {
        _hostSettingsService
          .sendTestEmail({
            emailAddress: $('#TestEmailAddressInput').val(),
          })
          .done(function () {
            abp.notify.info(app.localize('TestEmailSentSuccessfully'));
          });
      }
    });

    $('.passwordShowButton').click(function () {
      var itemId = $(this).data('id');
      var item = $('#' + itemId);
      if (item) {
        if (item[0].type === 'password') {
          item[0].type = 'text';

          $(this).find('i').removeClass('fa-eye');
          $(this).find('i').addClass('fa-eye-slash');
          $(this).find('span').text(app.localize('Hide'));
        } else {
          item[0].type = 'password';

          $(this).find('i').removeClass('fa-eye-slash');
          $(this).find('i').addClass('fa-eye');
          $(this).find('span').text(app.localize('Show'));
        }
      }
    });

    function initializeOpenIdConnectClaimsMappings() {
      _openIdConnectClaimsManager.init({
        containerId: 'claimsMappingsContainer',
        name: 'openIdConnectClaimsMappings',
        keyName: 'ClaimKey',
        valueName: 'ClaimValue',
        items: openIdConnectClaimsMappings.map((x) => {
          return {
            key: x.Key,
            value: x.Claim,
          };
        }),
      });
    }
    initializeOpenIdConnectClaimsMappings();

    function initializeWsFederationClaimsMappings() {
      _wsFederationClaimsManager.init({
        containerId: 'Setting_WsFederation_ClaimsMappingsContainer',
        name: 'wsFederationClaimsMappings',
        keyName: 'ClaimKey',
        valueName: 'ClaimValue',
        items: wsFederationClaimsMappings.map((x) => {
          return {
            key: x.Key,
            value: x.Claim,
          };
        }),
      });
    }
    initializeWsFederationClaimsMappings();

    function initializeFormValidations() {
      $('#EmailSmtpSettingsForm').validate();
      $('#EmailSmtpSettingsTestForm').validate();
      $('#UserManagementSettingsForm').validate();
      $('#UserLockOutSettingsForm').validate();
    }

    initializeFormValidations();

    function IsSmtpSettingsFormValid() {
      return $('#EmailSmtpSettingsForm').valid();
    }

    function IsUserManagementSettingsFormValid() {
      return $('#UserManagementSettingsForm').valid();
    }

    function IsUserLockOutSettingsFormValid() {
      return $('#UserLockOutSettingsForm').valid();
    }

    function toggleEnableCheckingLastXPasswordWhenPasswordChange() {
      if ($('#Setting_EnableCheckingLastXPasswordWhenPasswordChange').is(':checked')) {
        $('#divCheckingLastXPasswordCount').slideDown('fast');
      } else {
        $('#divCheckingLastXPasswordCount').slideUp('fast');
      }
    }

    $('#Setting_EnableCheckingLastXPasswordWhenPasswordChange').change(function () {
      toggleEnableCheckingLastXPasswordWhenPasswordChange();
    });

    toggleEnableCheckingLastXPasswordWhenPasswordChange();

    function toggleEnablePasswordExpiration() {
      if ($('#Setting_EnablePasswordExpiration').is(':checked')) {
        $('#divPasswordExpirationDayCount').slideDown('fast');
        $('#divPasswordResetCodeExpirationHours').slideDown('fast');
      } else {
        $('#divPasswordExpirationDayCount').slideUp('fast');
        $('#divPasswordResetCodeExpirationHours').slideUp('fast');
      }
    }

    $('#Setting_EnablePasswordExpiration').change(function () {
      toggleEnablePasswordExpiration();
    });

    toggleEnablePasswordExpiration();

    $('#ExternalLoginSettingsTab .tab-pane:first').addClass('show active');
    $('#ExternalLoginSettingsTab ul.nav-tabs li:first a.nav-link').addClass('active');
  });
})(jQuery);
