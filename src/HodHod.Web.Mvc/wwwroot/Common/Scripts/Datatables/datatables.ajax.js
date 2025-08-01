﻿/************************************************************************
 * Ajax extension for datatables                                         *
 *************************************************************************/
(function ($) {
  if (!$.fn.dataTableExt) {
    return;
  }

  var doAjax = function (listAction, requestData, callbackFunction, settings) {
    var inputFilter = {};

    //set table defined filters
    if (listAction.inputFilter) {
      inputFilter = $.extend(inputFilter, listAction.inputFilter());
    }

    //set paging filters
    if (settings.oInit.paging) {
      inputFilter = $.extend(inputFilter, {
        maxResultCount: requestData.length,
        skipCount: requestData.start,
      });
    }

    //set sorting filter
    if (requestData.order && requestData.order.length > 0) {
      if (!inputFilter.sorting) {
        inputFilter.sorting = '';
      }

      for (var i = 0; i < requestData.order.length; i++) {
        var orderingField = requestData.order[i];
        if (i > 0) {
          inputFilter.sorting += ',';
        }
        if (requestData.columns[orderingField.column].name) {
          inputFilter.sorting += requestData.columns[orderingField.column].name + ' ' + orderingField.dir;
        } else if (requestData.columns[orderingField.column].data) {
          inputFilter.sorting += requestData.columns[orderingField.column].data + ' ' + orderingField.dir;
        }
      }
    }

    //execute ajax function with filter
    if (listAction.ajaxFunction) {
      listAction
        .ajaxFunction(inputFilter)
        .done(function (result) {
          //store raw server response for custom rendering.
          settings.rawServerResponse = result;

          //html encoding can be disabled by adding "disableResponseHtmlEncoding: true" to "listAction" field
          var dataItems;
          if (listAction.disableResponseHtmlEncoding) {
            dataItems = result.items;
          } else {
            //HTML encodes the response items for XSS protection.
            dataItems = app.htmlUtils.htmlEncodeJson(result.items);
          }

          //invoke callback
          callbackFunction({
            recordsTotal: result.totalCount,
            recordsFiltered: result.totalCount,
            data: dataItems,
          });
        })
        .always(function () {
          abp.ui.clearBusy(settings.nTable);
        });
    }
  };

  if (!$.fn.dataTable) {
    return;
  }

  $.extend(true, $.fn.dataTable.defaults, {
    ajax: function (requestData, callbackFunction, settings) {
      if (!settings) {
        return;
      }

      if (!settings.oInit) {
        return;
      }

      if (!settings.oInit.listAction) {
        return;
      }

      doAjax(settings.oInit.listAction, requestData, callbackFunction, settings);
    },
  });

  $.fn.dataTable.Api.register('ajax.reloadPage()', function () {
    // user paging is not reset on reload. https://datatables.net/reference/api/ajax.reload()
    this.ajax.reload(null, false);
  });
})(jQuery);
