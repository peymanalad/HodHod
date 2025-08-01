﻿(function ($) {
  app.modals.ChangePasswordModal = function () {
    var _profileService = abp.services.app.profile;
    var _passwordComplexityHelper = new app.PasswordComplexityHelper();

    var _modalManager;
    var _$form = null;

    this.init = function (modalManager) {
      _modalManager = modalManager;

      KTPasswordMeter.createInstances();

      _profileService.getPasswordComplexitySetting().done(function (result) {
        _$form = _modalManager.getModal().find('form[name=ChangePasswordModalForm]');
        _$form.validate({
          errorPlacement: function (error, element) {
            const parentEl = element.parent();
            const el = parentEl.hasClass('position-relative') ? parentEl.parent() : parentEl;
            error.appendTo(el);
          },
        });

        _passwordComplexityHelper.setPasswordComplexityRules(
          _$form.find('input[name=NewPassword],input[name=NewPasswordRepeat]'),
          result.setting,
        );
      });
    };

    this.save = function () {
      if (!_$form.valid()) {
        return;
      }

      _modalManager.setBusy(true);
      _profileService
        .changePassword(_$form.serializeFormToObject())
        .done(function () {
          abp.notify.info(app.localize('YourPasswordHasChangedSuccessfully'));
          _modalManager.close();
        })
        .always(function () {
          _modalManager.setBusy(false);
        });
    };
  };
})(jQuery);
