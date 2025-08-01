﻿var FeaturesTree = (function ($) {
  return function () {
    var $tree;

    function init($treeContainer) {
      $tree = $treeContainer;

      $tree
        .on('ready.jstree', function () {
          customizeTreeNodes();
        })
        .on('redraw.jstree', function () {
          customizeTreeNodes();
        })
        .on('after_open.jstree', function () {
          customizeTreeNodes();
        })
        .on('create_node.jstree', function () {
          customizeTreeNodes();
        })
        .on('changed.jstree', function (e, data) {
          if (!data.node) {
            return;
          }

          var childrenNodes;

          if (data.node.state.selected) {
            selectNodeAndAllParents($tree.jstree('get_parent', data.node));

            childrenNodes = $.makeArray($tree.jstree('get_node', data.node).children);
            $tree.jstree('select_node', childrenNodes);
          } else {
            childrenNodes = $.makeArray($tree.jstree('get_node', data.node).children);
            $tree.jstree('deselect_node', childrenNodes);
          }
        })
        .jstree({
          types: {
            default: {
              icon: 'fa fa-folder text-warning',
            },
            file: {
              icon: 'fa fa-file text-warning',
            },
          },
          checkbox: {
            keep_selected_style: false,
            three_state: false,
            cascade: '',
          },
          plugins: ['checkbox', 'types'],
        });

      function customizeTreeNodes() {
        $tree.find('.jstree-node').each(function () {
          var $nodeLi = $(this);
          var $nodeA = $nodeLi.find('.jstree-anchor');
          var feature = JSON.parse($nodeLi.attr('data-feature'));
          var featureValue = $nodeLi.attr('data-feature-value');
          var attr = feature.inputType.validator.attributes;

          if (!feature || !feature.inputType) {
            return;
          }

          if (feature.inputType.name == 'CHECKBOX') {
            //no change for checkbox
          } else if (feature.inputType.name == 'SINGLE_LINE_STRING') {
            if (!$nodeLi.find('.feature-tree-textbox').length) {
              $nodeA.find('.jstree-checkbox').hide();

              var inputType = 'text';
              if (feature.inputType.validator) {
                if (feature.inputType.validator.name == 'NUMERIC') {
                  inputType = 'number';
                }
              }

              var $textbox = $('<input class="feature-tree-textbox" type="' + inputType + '" />')
                .val(featureValue)
                .on('change', function () {
                  $tree.jstree(true)._model.data[feature.name].li_attr['data-feature-value'] = $textbox.val();
                });

              if (inputType == 'number') {
                $textbox.attr('min', attr.MinValue);
                $textbox.attr('max', attr.MaxValue);
              } else {
                if (feature.inputType.validator && feature.inputType.validator.name == 'STRING') {
                  if (attr.maxLength > 0) {
                    $textbox.attr('maxlength', attr.maxLength);
                  }
                  if (feature.inputType.validator.minLength > 0) {
                    $textbox.attr('required', 'required');
                  }
                  if (feature.inputType.validator.regularExpression) {
                    $textbox.attr('pattern', feature.inputType.validator.regularExpression);
                  }
                }
              }

              $textbox.on('input propertychange paste', function () {
                if (isFeatureValueValid(feature, $textbox.val())) {
                  $nodeLi.attr('data-feature-value', $textbox.val());
                  $textbox.removeClass('feature-tree-textbox-invalid');
                } else {
                  $textbox.addClass('feature-tree-textbox-invalid');
                }
              });

              $textbox.appendTo($nodeLi);
            }
          } else if (feature.inputType.name == 'COMBOBOX') {
            if (!$nodeLi.find('.feature-tree-combobox').length) {
              $nodeA.find('.jstree-checkbox').hide();

              var $combobox = $('<select class="feature-tree-combobox" />');
              _.each(feature.inputType.itemSource.items, function (opt) {
                $('<option></option>').attr('value', opt.value).text(opt.displayText).appendTo($combobox);
              });

              $combobox
                .val(featureValue)
                .on('change', function () {
                  $tree.jstree(true)._model.data[feature.name].li_attr['data-feature-value'] = $combobox.val();
                })
                .appendTo($nodeLi);
            }
          }
        });
      }
    }

    function selectNodeAndAllParents(node) {
      $tree.jstree('select_node', node, true);
      var parent = $tree.jstree('get_parent', node);
      if (parent) {
        selectNodeAndAllParents(parent);
      }
    }

    function isFeatureValueValid(feature, value) {
      if (!feature || !feature.inputType || !feature.inputType.validator) {
        return true;
      }

      var validator = feature.inputType.validator;
      var attr = feature.inputType.validator.attributes;

      if (validator.name == 'STRING') {
        if (value == undefined || value == null) {
          return validator.allowNull;
        }

        if (typeof value != 'string') {
          return false;
        }

        if (validator.minLength > 0 && value.length < validator.minLength) {
          return false;
        }

        if (validator.maxLength > 0 && value.length > validator.maxLength) {
          return false;
        }

        if (validator.regularExpression) {
          return new RegExp(validator.regularExpression).test(value);
        }
      } else if (validator.name == 'NUMERIC') {
        var numValue = parseInt(value);

        if (isNaN(numValue)) {
          return false;
        }

        var minValue = attr.MinValue;
        if (minValue > numValue) {
          return false;
        }

        var maxValue = attr.MaxValue;
        if (maxValue > 0 && numValue > maxValue) {
          return false;
        }
      }

      return true;
    }

    function isValid() {
      return $tree.find('.feature-tree-textbox-invalid').length <= 0;
    }

    function getFeatureValues() {
      var featureValues = [];

      $.each($tree.jstree(true)._model.data, function (key, object) {
        if (object.li_attr) {
          var feature = JSON.parse(object.li_attr['data-feature']);
          if (feature.inputType.name == 'CHECKBOX') {
            featureValues.push({
              name: feature.name,
              value: object.state.selected ? 'true' : 'false',
            });
          } else {
            featureValues.push({
              name: feature.name,
              value: object.li_attr['data-feature-value'],
            });
          }
        }
      });

      return featureValues;
    }

    return {
      init: init,
      getFeatureValues: getFeatureValues,
      isValid: isValid,
    };
  };
})(jQuery);
