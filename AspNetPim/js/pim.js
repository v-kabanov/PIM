(function (window, $) {
    window.pim = {
        config: {
            serverUrl: "",

        },
        features: {},
        pages: {},
        init: function (config) {
            $.extend(this.config, config);
            //RequiredListValidationAttribute
            (function (requiredList) {
                requiredList.init = function () {
                    $.validator.addMethod("requiredlist", function (value, element) {
                        console.write(value);
                        return true;
                    });

                    $.validator.unobtrusive.adapters.addBool("requiredlist");

                };
            })(this.features.requiredList = this.features.requiredList || {}, $);
            //Auto ajax call
            // By initialising this features, config.event on html elements in config.selector causes trigger an ajax call
            // different ajax scenarios can be implemented for elements in config 
            this.features.autoAjax = (function ($) {
                var defaultConf = {
                    // url that form is posted to
                    url: "",
                    // elements which cause postback
                    selector: "[data-cause-postback=True]",
                    //event causes the ajax call
                    event: "change",
                    // element contains in progress message
                    inProgressSelector: "",
                    // form to postback
                    formSelector: "",
                    // element to show messages of postback
                    messageSelector: "",
                    // always on ajax call (in addition to default behavior)
                    always: function () { },
                    // fail on ajax call (in addition to default behavior)
                    fail: function () { }
                };
                return {
                    config: [],
                    init: function (conf) {
                        var that = this;
                        $.each(conf, function (ind, val) {
                            var confElem = {};
                            $.extend(confElem, defaultConf, val);
                            that.config.push(confElem);
                        });

                        var postback = function (event) {
                            var postbackConf = event.data.conf;
                            $(postbackConf.inProgressSelector).show();
                            return postbackFunction(postbackConf).always(postbackConf.always).fail(postbackConf.fail);
                        };
                        function postbackFunction(postbackConf) {
                            return $.post(postbackConf.url, $(postbackConf.formSelector).serialize(), function (result) {
                                var $result = $(result);
                                ///having <script> in html result interferes with jquery validation. 
                                /// need to run manually (not using browser default behavior)
                                $result.filter("script").each(function (ind, val) {
                                    eval($(val).html());
                                });
                                var form = $result.filter("form");
                                $(postbackConf.formSelector).html(form.html());

                            }).fail(function (data) {
                                var formMessageElem = $(postbackConf.messageSelector);
                                formMessageElem.append($("<p>").addClass("field-validation-error").text("Error: " + data));
                            }).always(function (data) {
                                MESMODULE.features.elementHelper.loadTooltipWithValidation();
                                $(postbackConf.inProgressSelector).hide();
                            });
                        };
                        $(document).ready(function () {
                            $.each(that.config, function (ind, val) {
                                $(document).on(val.event, val.selector,
                                    {
                                        conf: val
                                    },
                                    postback);
                            });
                        });
                    }
                };
            })($);

            ////////////////////////PleaseWait dialog
            this.features.waitingDialog = (function (conf, $) {

                var pleaseWaitDiv = $('<div class="modal " id="pleaseWaitDialog" data-backdrop="static" data-keyboard="false">' +
                    '<div class="modal-body"><span>Processing</span><img id="progressIndicator" src="' +
                    conf.serverUrl +
                    '/Content/Images/progress.gif" alt="img progress" /> </div></div>');
                return {
                    showPleaseWait: function () {
                        try {
                            //pleaseWaitDiv.modal();
                        } catch (exception) {
                        }
                    },
                    hidePleaseWait: function () {
                        //pleaseWaitDiv.modal("hide");
                    }
                };

            })(this.config, $);

            ////////////////////////element Helper
            /// functionalities related to http elements
            ///
            this.features.elementHelper = (function (conf, $) {

                return {
                    trimInput: function (input) {
                        var elem = $(input);
                        elem.val(elem.val().trim());
                    },
                    //target element (element with tooltip) must have data-toggle="tooltip"
                    // in case of validation, based on data-validation-result-type, which can be Warning or Info, suitable
                    // css class will be assigned to target element.
                    loadTooltipWithValidation: function () {
                        $('[data-toggle="tooltip"]').tooltip();
                        $("[data-validation-result-type=\"Warning\"").addClass("validation-result-type-warning");
                        $("[data-validation-result-type=\"Info\"").addClass("validation-result-type-info");
                        $("[data-validation-result-type=\"Error\"").addClass("validation-result-type-error");
                    },
                    applyValidationAttributes: function (replaceErrors) {
                        $('[data-toggle="tooltip"]').tooltip();
                        var allValidatedElements = $("[data-validation-result-type]");
                        allValidatedElements.removeClass("validation-result-type-warning validation-result-type-info validation-result-type-success validation-result-type-error");
                        if (replaceErrors) {
                            allValidatedElements.removeClass("validation-result-type-error");
                            $("[data-validation-result-type=\"Error\"]").addClass("validation-result-type-error");
                        }
                        $("[data-validation-result-type=\"Warning\"]").addClass("validation-result-type-warning");
                        $("[data-validation-result-type=\"Info\"]").addClass("validation-result-type-info");
                        $("[data-validation-result-type=\"Success\"]").addClass("validation-result-type-success");
                    },
                    flashing: function (elem) {
                        setInterval(function () {
                            $(elem).fadeIn(500);
                            $(elem).fadeOut(500);
                        }, 2000);
                    },
                    //converting arr (array of json with Name, Code) to jquery autocomplete source (array of json with label, value)
                    convertToJqUiAutocompleteSource: function (arr) {
                        return $.map(arr, function (item) {
                            return {
                                label: item.Name,
                                value: item.Code
                            }
                        });
                    },
                    // After whole form ajax call, works as the default browser behavior. 
                    // In case of error: * it focuses on the first element with error otherwise next element (circular) 
                    //                   * clear the text (ifClearStayFocused) (compatible with typeahead)
                    //                      
                    // Note: * there is no way to memorize the element but id (or name) which persists after ajax call
                    //       * no id, random focus
                    // init: must call on the ready of the document
                    // preserve: must call after any ajax call
                    focusPreserver: {
                        //inputs on this form will preserve
                        formSelector: "body",
                        //last focused input
                        lastFocusedSelector: "",
                        //last changed input
                        lastChangedSelector: "",
                        //input event which changes the lastFocusedSelector
                        // Note: input change does not work on Edge/IE, so, typeahead:change added
                        eventToCapture: "change typeahead:change",
                        //will not be in any search for inputs
                        //further excluding filtering on the form
                        //Default: Extra elemets by select2 and typeahead are removed
                        excludeSelectors: [".tt-hint", "[class*=select2-hidden]"],
                        //focus on element (not using lastFocusedSelector)
                        //common use: when element has an error, that element must be focused
                        stayFocusedSelector: ".input-validation-error",
                        //all inputs in formSelector excluding excludeSelectors
                        getInputSelector: function () {
                            return this.formSelector + " :input:not(" + this.excludeSelectors.toString() + ")";
                        },
                        init: function () {
                            var that = this;
                            $(document).on(this.eventToCapture, this.getInputSelector(), function (e) {
                                that.lastChangedSelector = "#" + $(e.target).prop("id");
                            });
                            $(document).on("focus", ":input", function (e) {
                                that.lastFocusedSelector = "#" + $(e.target).prop("id");
                            });
                        },
                        preserve: function () {
                            var inputs = $(this.getInputSelector());
                            var lastFocused = $(this.lastFocusedSelector);
                            var lastChanged = $(this.lastChangedSelector);
                            var lastChangedInd = inputs.index(lastChanged);
                            var stayFocused = inputs.filter(this.stayFocusedSelector);

                            if (stayFocused.length && stayFocused[0].setSelectionRange) {
                                stayFocused[0].setSelectionRange(0, stayFocused[0].value.length);
                                stayFocused[0].focus();
                                return;
                            }

                            if (!stayFocused.length && lastFocused.length) {
                                lastFocused.focus();
                            }
                            else {
                                stayFocused.length ? stayFocused[0].focus() : inputs.eq(lastChangedInd + 1).focus();
                            }
                        }
                    }
                };

            })(this.config, $);
        }
    };
}(window, jQuery));
