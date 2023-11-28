﻿(function (window, $) {
    window.pim = {
        config: {
            serverUrl: "",
        },
        features: {},
        pages: {},
        init: function (config) {
            $.extend(this.config, config);
            ///////////////////////////////////////////////////////Features
            //Feature: footer 
            (function (ftr, conf, $, bootstrapDialog) {
                ftr.setUpdateLanguageAjax = function () {
                    $("#languageRadioSection :radio[name=language]").change(function () {
                        $.ajax({
                            url: conf.serverUrl + "/Common/UpdateLanguage",
                            type: "POST",
                            data:
                            {
                                language: $("#languageRadioSection :radio[name='language']:checked").val()
                            },
                            error: function (e) {
                                bootstrapDialog.alert("Unable to connect to the server. The error is: " + e);
                            }
                        });
                    });
                };
            }(this.features.ftr = this.features.ftr || {}, this.config, $, window.BootstrapDialog));

            // general limit client rule
            (function (gLimit, $) {
                gLimit.setGeneralLimitRule = function () {

                    $.validator.unobtrusive.adapters.add("generallimit", ["limit"],
                        function (options) {
                            options.rules["generallimit"] = options.params;
                            if (options.message != null) {
                                $.validator.messages.generallimit = options.message;
                            }
                        }
                    );

                    $.validator.addMethod("generallimit", function (value, element, params) {

                        const currentValue = parseInt(value);
                        if (isNaN(currentValue)) {
                            return true;
                        }

                        const limitName = element.name.split(".").splice(0, 2).join(".") + "." + params.limit;
                        const maxValid = parseInt($("input[name='" + limitName + "." + "MaxValid']").val());
                        const minValid = parseInt($("input[name='" + limitName + "." + "MinValid']").val());

                        if (isNaN(minValid) || isNaN(maxValid)) {
                            return true;
                        }
                        if (isNaN(currentValue) || minValid > currentValue || currentValue > maxValid) {
                            const message = $(element).attr("data-val-generallimit");
                            const displayName = $("label[for='" + element.id + "']").html();
                            $.validator.messages.generallimit = $.validator.format(message, displayName, minValid, maxValid);
                            return false;
                        }
                        return true;
                    }, "");
                };
            }(this.features.gLimit = this.features.gLimit || {}, $));

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
            // when initialized, sets up truncation of all text inputs (having type='text' attribute) and uppercasing
            // all text inputs with 'barcode' class; also sets up focus preserver if any form submission is configured.
            // If you don't want submitting form but only to update text inputs, pass empty array to init().
            this.features.autoAjax = (function ($) {
                const defaultPostbackPredicate = function (event) {
                    const target = $(event.target || event.srcElement);

                    // previous value will be undefined before first ajax post - after GET
                    const previousValue = target.attr("previous-value") || "";
                    const currentValue = pim.features.elementHelper.getInputValue(target);

                    return previousValue !== currentValue;
                };
                const defaultConf = {
                    // url that form is posted to
                    url: "",
                    triggers: [{
                        selector: "[data-cause-postback=True]:not([tt-hint]):not([tt-input]):not(.select2-hidden):not([type=number])",
                        eventName: "change"
                    }, {
                        // accepting twitter typeahead suggestion does not cause change event in IE
                        // input:number raises change event without focusout with every increment
                        selector: "input[data-cause-postback=True][tt-input],input[data-cause-postback=True][type=number]",
                        eventName: "focusout",
                        // optional function returning falsy value to cancel postback
                        predicate: defaultPostbackPredicate
                    }
                    ],
                    // element contains in progress message
                    inProgressSelector: "#divInProgress",
                    // form to postback
                    formSelector: "#mainForm",
                    validateBeforePosting: false,
                    // whether to disable the input which triggers the ajax call (see 'selector') if the event is 'click'
                    disableClickTriggerInputForCallDuration: true,
                    // selector of element whose content to replace with post result; default is the formSelector
                    replacementTargetSelector: "",
                    // selector of element in the post result which is to replace the target; default is 'form'
                    replacementSourceSelector: "",
                    // element to show messages of postback
                    messageSelector: "#formMessages",
                    // always on ajax call (in addition to default behavior)
                    always: function () {
                    },
                    // fail on ajax call (in addition to default behavior)
                    fail: function () {
                    },
                    success: function () {
                    },
                    // function to call before posting; accepts event if returns false, cancel posting
                    confirmFunction: function (e) {
                        return true;
                    },
                    getPostData: function (event) {
                        return $(this.formSelector).serialize();
                    },
                    // function grabbing additional data to post
                    getAdditionalPostData: function () {
                        return {};
                    }
                };
                return {
                    config: [],
                    init: function (conf) {
                        const that = this;
                        $.each(conf, function (ind, val) {
                            const confElem = {};
                            $.extend(confElem, defaultConf, val);
                            that.config.push(confElem);
                        });

                        const normalizeInputs = function () {
                            const updateFunction =
                                pim.features.elementHelper.createTextInputNormalizationFunction(true, false);
                            $.each($("input:text.form-control:not(.barcode),textarea.form-control"),
                                function (int, element) {
                                    updateFunction(element);
                                });
                        };

                        const postback = function (event) {
                            event.preventDefault();
                            const postbackConf = event.data.conf;

                            if (postbackConf.confirmFunction && !postbackConf.confirmFunction(event))
                                return null;

                            const runtimeData = event.data.runtimeData;

                            if (!postbackConf.validateBeforePosting || $(postbackConf.formSelector).valid()) {
                                $(postbackConf.inProgressSelector).show();
                                if (postbackConf.event === "click" && postbackConf.disableClickTriggerInputForCallDuration) {
                                    $(postbackConf.selector).prop("disabled", true);
                                    runtimeData.triggerHasBeenDisabled = true;
                                }
                                return postbackFunction(event).always(function () {
                                    postbackConf.always && postbackConf.always();
                                    // client 'always' and 'fail' handlers may cause focus loss such as when setting up autocomplete,
                                    // hence restoring focus after client handler invocation
                                    if (that.config.length)
                                        pim.features.elementHelper.focusPreserver.preserve();

                                    if (runtimeData.triggerHasBeenDisabled)
                                        $(postbackConf.selector).prop("disabled", false);
                                })
                                    .fail(postbackConf.fail);
                            }
                        };

                        function postbackFunction(event) {
                            normalizeInputs();

                            const eventData = event.data;
                            const postbackConf = eventData.conf;
                            const runtimeData = eventData.runtimeData;

                            pim.features.waitingDialog.showPleaseWait(postbackConf.modal === true);
                            const postData = postbackConf.getPostData(event);
                            if (postbackConf.getAdditionalPostData)
                                $.extend(postData, postbackConf.getAdditionalPostData());

                            return $.post(postbackConf.url, postData, function (result) {
                                const $result = $(result);
                                const sourceSelector = postbackConf.replacementSourceSelector ? postbackConf.replacementSourceSelector : "form";
                                const targetSelector = postbackConf.replacementTargetSelector ? postbackConf.replacementTargetSelector : postbackConf.formSelector;

                                const source = $result.find(sourceSelector).add($result.filter(sourceSelector));
                                $(targetSelector).html(source.html());

                                $.each($("[data-cause-postback=True]:not([tt-hint]):not(.select2-hidden)"),
                                    // to manually track changes where change event does not work (in IE with twitter typeahead)
                                    function (ind, element) {
                                        const $element = $(element);
                                        const value = pim.features.elementHelper.getInputValue($element);
                                        // validated value
                                        $element.attr("previous-value", value);
                                    });


                                // having <script> in html result interferes with jquery validation. 
                                // need to run manually (not using browser default behavior)
                                $result.filter("script").each(function (ind, val) {
                                    eval($(val).html());
                                });
                                if (postbackConf.success) {
                                    postbackConf.success();
                                }
                            }).fail(function (data) {
                                const formMessageElem = $(postbackConf.messageSelector);
                                formMessageElem.append($("<p>").addClass("field-validation-error").text("Error: " + data));
                            }).always(function (data) {
                                pim.features.elementHelper.loadTooltipWithValidation();
                                $(postbackConf.inProgressSelector).hide();
                            });
                        };
                        $(document).ready(function () {
                            const $doc = $(document);
                            // massaging form text inputs
                            // NOTE if change event is handled, focusout does not fire
                            $doc.on("change", ".form-control.auto-trim:not([tt-input]):not(.select2-hidden)"
                                , pim.features.elementHelper.onTextInputChange(true, false));

                            if (that.config.length)
                                pim.features.elementHelper.focusPreserver.init();

                            $.each(that.config, function (ind, postbackConfig) {
                                $.each(postbackConfig.triggers,
                                    function (triggerIndex, triggerObject) {
                                        $doc.on(triggerObject.eventName,
                                            triggerObject.selector,
                                            {
                                                conf: postbackConfig,
                                                runtimeData: {}
                                            },
                                            function (event) {
                                                if (!triggerObject.predicate || triggerObject.predicate(event))
                                                    postback(event);
                                            });
                                    }
                                    );
                            });
                        });
                    }
                };
            })($);

            ////////////////////////PleaseWait dialog
            this.features.waitingDialog = (function (conf, $) {

                const pleaseWaitDiv = $('<div class="modal " id="pleaseWaitDialog" data-backdrop="static" data-keyboard="false">' +
                    '<div class="modal-body"><span>Processing</span><img id="progressIndicator" src="' +
                    conf.serverUrl +
                    '/image/progress.gif" alt="img progress" /> </div></div>');
                return {
                    showPleaseWait: function (modal) {
                        try {
                            if (modal)
                                pleaseWaitDiv.modal();
                            else
                                $(conf.inProgressSelector).show();
                        } catch (exception) {
                        }
                    },
                    hidePleaseWait: function (modal) {
                        if (modal)
                            pleaseWaitDiv.modal("hide");
                        else
                            $(conf.inProgressSelector).hide();
                    }
                };

            })(this.config, $);

            ///////////////////////element Helper
            // functionalities related to http elements
            //
            this.features.elementHelper = (function (conf, $) {

                return {
                    trimInput: function (input) {
                        const elem = $(input);
                        elem.val(elem.val().trim());
                    },
                    createTextNormalizationFunction: function (trim, upper) {
                        return function (value) {
                            value = value || "";
                            if (trim)
                                value = value.trim();
                            if (upper)
                                value = value.toUpperCase();
                            return value;
                        };
                    },
                    createTextInputNormalizationFunction: function (trim, upper) {
                        const valueFunction = this.createTextNormalizationFunction(trim, upper);
                        return function (input) {
                            const $input = $(input);
                            let value = $input.val();
                            value = valueFunction(value);
                            $input.val(value);
                            $input.attr("value", value);
                            $input.attr("defaultValue", value);
                        };
                    },
                    onTextInputChange: function (trim, upper) {
                        const inputValueFunction = this.createTextInputNormalizationFunction(trim, upper);
                        return function (event) {
                            event = event || window.event;
                            const source = event.target || event.srcElement;
                            const $source = $(source);
                            inputValueFunction($source);
                        }
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
                        const allValidatedElements = $("[data-validation-result-type]");
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
                    // get value of an input element, supporting those whose val() method is not working well
                    getInputValue: function (elem) {
                        const $elem = $(elem);
                        const inputType = $elem.attr("type");
                        if (inputType === "radio" || inputType === "checkbox")
                            return $elem.is(":checked");

                        return $elem.val();
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
                        // if stayFocusedSelector has elements, clear their input
                        ifClearStayFocused: false,
                        //all inputs in formSelector excluding excludeSelectors
                        getInputSelector: function () {
                            return this.formSelector + " :input:not(" + this.excludeSelectors.toString() + ")";
                        },
                        init: function () {
                            const that = this;
                            $(document).on(this.eventToCapture, this.getInputSelector(), function (e) {
                                const id = $(e.target).prop("id");
                                if (id)
                                    that.lastChangedSelector = "#" + id;
                            });
                            $(document).on("focus", ":input.form-control,textarea.form-control", function (e) {
                                const id = $(e.target).prop("id");
                                if (id)
                                    that.lastFocusedSelector = "#" + id;
                            });
                        },
                        preserve: function () {
                            const inputs = $(this.getInputSelector());
                            const lastFocused = $(this.lastFocusedSelector);
                            const lastChanged = $(this.lastChangedSelector);
                            const lastChangedInd = lastChanged.length && inputs.index(lastChanged);
                            const stayFocused = inputs.filter(this.stayFocusedSelector);

                            if (this.ifClearStayFocused) {
                                stayFocused.val("");
                                stayFocused.typeahead && stayFocused.typeahead("val", "");
                            }
                            else if (stayFocused.length && stayFocused[0].setSelectionRange) {
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
                    },
                    // for use with chosen plugin selector replacements and bootstrap validation classes
                    applyValidationClassesToAllChosenControls: function (formSelector) {
                        const chosenContainers = $(formSelector + " div.chosen-container");

                        for (let i = 0; i < chosenContainers.length; ++i) {
                            const chosenContainer = $(chosenContainers[i]);
                            const chosenControl = chosenContainer.children().first();

                            const id = chosenContainer.attr("id");
                            const pos = id.indexOf("_chosen");
                            if (pos > 0) {
                                const targetControlId = id.substring(0, pos);
                                const control = $('#' + targetControlId);

                                const classesString = control.attr("class");

                                if (classesString) {
                                    const classes = classesString.split(" ");

                                    let validationClasses = "";

                                    for (let j = 0; j < classes.length; ++j) {
                                        if (classes[j].indexOf('validation') >= 0) {
                                            validationClasses += (classes[j] + " ");
                                        }
                                    }

                                    chosenControl.addClass(validationClasses);
                                }
                            }
                        }
                    }
                };

            })(this.config, $);
        }
    };
}(window, jQuery));
