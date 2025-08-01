﻿(function () {
  $(function () {
    var _cachingService = abp.services.app.caching;
    var _webLogService = abp.services.app.webLog;
    var _notificationService = abp.services.app.notification;

    //Caching
    function clearCache(cacheName) {
      _cachingService
        .clearCache({
          id: cacheName,
        })
        .done(function () {
          abp.notify.success(app.localize('CacheSuccessfullyCleared'));
        });
    }

    function clearAllCaches() {
      _cachingService.clearAllCaches().done(function () {
        abp.notify.success(app.localize('AllCachesSuccessfullyCleared'));
      });
    }

    $('.btn-clear-cache').click(function (e) {
      e.preventDefault();
      var cacheName = $(this).attr('data-cache-name');
      clearCache(cacheName);
    });

    $('#ClearAllCachesButton').click(function (e) {
      e.preventDefault();
      clearAllCaches();
    });

    function SendNewVersionAvailableNotification() {
      abp.message.confirm(app.localize('SendNewVersionNotificationWarningMessage'), null, function (isConfirmed) {
        if (isConfirmed) {
          _notificationService.createNewVersionReleasedNotification().done(function () {
            abp.notify.info(app.localize('SuccessfullySentNewVersionNotification'));
          });
        }
      });
    }

    $('#NewVersionAvailableNotification').click(function (e) {
      e.preventDefault();
      SendNewVersionAvailableNotification();
    });

    //Web Logs
    function getWebLogs() {
      _webLogService.getLatestWebLogs({}).done(function (result) {
        var logs = getFormattedLogs(result.latestWebLogLines);
        $('#WebSiteLogsContent').html(logs);
        fixWebLogsPanelHeight();
      });
    }

    function downloadWebLogs() {
      _webLogService.downloadWebLogs({}).done(function (result) {
        app.downloadTempFile(result);
      });
    }

    function getFormattedLogs(logLines) {
      var resultHtml = '';
      $.each(logLines, function (index, logLine) {
        resultHtml +=
          '<span class="log-line">' +
          _.escape(logLine)
            .replace('DEBUG', '<span class="badge badge-dark">DEBUG</span>')
            .replace('INFO', '<span class="badge badge-info">INFO</span>')
            .replace('WARN', '<span class="badge badge-warning">WARN</span>')
            .replace('ERROR', '<span class="badge badge-danger">ERROR</span>')
            .replace('FATAL', '<span class="badge badge-danger">FATAL</span>') +
          '</span>';
      });
      return resultHtml;
    }

    function fixWebLogsPanelHeight() {
      var windowHeight = $(window).height();
      var panelHeight = $('.full-height').height();
      var difference = windowHeight - panelHeight;
      var fixedHeight = panelHeight + difference;
      $('.full-height').css('height', fixedHeight - 350 + 'px');
    }

    $('#DownloadAllLogsbutton').click(function (e) {
      e.preventDefault();
      downloadWebLogs();
    });

    $('#RefreshButton').click(function (e) {
      e.preventDefault();
      getWebLogs();
    });

    $(window).resize(function () {
      fixWebLogsPanelHeight();
    });

    getWebLogs();
  });
})();
