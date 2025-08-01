﻿(function () {
  $(function () {
    var _$languagesTable = $('#LanguagesTable');
    var _languageService = abp.services.app.language;
    var _defaultLanguageName = null;

    var _permissions = {
      create: abp.auth.hasPermission('Pages.Administration.Languages.Create'),
      edit: abp.auth.hasPermission('Pages.Administration.Languages.Edit'),
      changeTexts: abp.auth.hasPermission('Pages.Administration.Languages.ChangeTexts'),
      changeDefaultLanguage: abp.auth.hasPermission('Pages.Administration.Languages.ChangeDefaultLanguage'),
      delete: abp.auth.hasPermission('Pages.Administration.Languages.Delete'),
    };

    var _createOrEditModal = new app.ModalManager({
      viewUrl: abp.appPath + 'App/Languages/CreateOrEditModal',
      scriptUrl: abp.appPath + 'view-resources/Areas/App/Views/Languages/_CreateOrEditModal.js',
      modalClass: 'CreateOrEditLanguageModal',
    });

    var dataTable = _$languagesTable.DataTable({
      paging: false,
      serverSide: false,
      processing: true,
      listAction: {
        ajaxFunction: _languageService.getLanguages,
      },
      columnDefs: [
        {
          className: 'dtr-control responsive',
          orderable: false,
          render: function () {
            return '';
          },
          targets: 0,
        },
        {
          targets: 1,
          data: null,
          orderable: false,
          autoWidth: false,
          type: 'html',
          defaultContent: '',
          rowAction: {
            text:
              '<i class="fa fa-cog"></i> <span class="d-none d-md-inline-block d-lg-inline-block d-xl-inline-block">' +
              app.localize('Actions') +
              '</span> <span class="caret"></span>',
            items: [
              {
                text: app.localize('Edit'),
                visible: function (data) {
                  return _permissions.edit && data.record.tenantId === abp.session.tenantId;
                },
                action: function (data) {
                  _createOrEditModal.open({ id: data.record.id });
                },
              },
              {
                text: app.localize('ChangeTexts'),
                visible: function () {
                  return _permissions.changeTexts;
                },
                action: function (data) {
                  document.location.href = abp.appPath + 'App/Languages/Texts?languageName=' + data.record.name;
                },
              },
              {
                text: app.localize('SetAsDefaultLanguage'),
                visible: function () {
                  return _permissions.changeDefaultLanguage;
                },
                action: function (data) {
                  setAsDefaultLanguage(data.record);
                },
              },
              {
                text: app.localize('Delete'),
                visible: function (data) {
                  return _permissions.delete && data.record.tenantId === abp.session.tenantId;
                },
                action: function (data) {
                  deleteLanguage(data.record);
                },
              },
            ],
          },
        },
        {
          targets: 2,
          data: 'displayName',
          render: function (displayName, type, row, meta) {
            var $span = $('<span/>')
              .append($('<i/>').addClass(row.icon).css('margin-right', '5px'))
              .append($('<span/>').attr('data-language-name', row.name).text(row.displayName));

            if (meta.settings.rawServerResponse.defaultLanguageName === row.name) {
              $span.addClass('text-bold').append(' (' + app.localize('Default') + ')');
            }

            return $span[0].outerHTML;
          },
        },
        {
          targets: 3,
          data: 'name',
        },
        {
          targets: 4,
          data: 'tenantId',
          visible: abp.session.tenantId ? true : false, //this field is visible only for tenants
          render: function (tenantId, type, row, meta) {
            var $span = $('<span/>').addClass('label');

            if (meta.settings.rawServerResponse.defaultLanguageName === row.name) {
              $span.addClass('badge badge-success').text(app.localize('Yes'));
            } else {
              $span.addClass('badge badge-dark').text(app.localize('No'));
            }

            return $span[0].outerHTML;
          },
        },
        {
          targets: 5,
          data: 'isDisabled',
          render: function (isDisabled) {
            var isEnabled = !isDisabled;
            var $span = $('<span/>').addClass('label');
            if (isEnabled) {
              $span.addClass('badge badge-success').text(app.localize('Yes'));
            } else {
              $span.addClass('badge badge-dark').text(app.localize('No'));
            }

            return $span[0].outerHTML;
          },
        },
        {
          targets: 6,
          data: 'creationTime',
          render: function (creationTime) {
            return moment(creationTime).format('L');
          },
        },
      ],
    });

    function setAsDefaultLanguage(language) {
      _languageService
        .setDefaultLanguage({
          name: language.name,
        })
        .done(function () {
          getLanguages();
          abp.notify.success(app.localize('SuccessfullySaved'));
        });
    }

    function deleteLanguage(language) {
      abp.message.confirm(
        app.localize('LanguageDeleteWarningMessage', language.displayName),
        app.localize('AreYouSure'),
        function (isConfirmed) {
          if (isConfirmed) {
            _languageService
              .deleteLanguage({
                id: language.id,
              })
              .done(function () {
                getLanguages();
                abp.notify.success(app.localize('SuccessfullyDeleted'));
              });
          }
        },
      );
    }

    $('#CreateNewLanguageButton').click(function () {
      _createOrEditModal.open();
    });

    function getLanguages() {
      dataTable.ajax.reload();
    }

    abp.event.on('app.createOrEditLanguageModalSaved', function () {
      getLanguages();
    });
  });
})();
