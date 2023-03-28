
/*
* Mediascend forms library
* version: 1.0
* Handles form validation and submission to Mediascend web services
*
* Copyright (c) 2009 Mediascend, Inc.
* 
* Requires: takeoff-helpers.js, jQuery v1.3+, jQuery JSON, jQuery Validate
* 
*/
(function (jQuery) {
    $ = jQuery;

    //fixes problems that occured on the server when updating to jquery 1.4.  see http://www.google.com/#hl=en&q=ajaxSettings.traditional+asp.net+mvc&aq=f&aqi=&aql=&oq=&gs_rfai=&pbx=1&fp=5c95fbf2260eab9f
    jQuery.ajaxSettings.traditional = true;

    $.madAjaxOptions = {
        type: "POST",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        url: function (form) {
            return (form && form.length > 0) ? form[0].action : null;
        }
    };

    //Wraps around $.ajax, adding a few extras:
    //1.  Meant for calling an asp.net web service passing JSON data
    //2.  Adds a default error message for the user
    //3.  Hides the "result.d" parameter that asp.net passes back in web services
    //4.  Automatically serializes the data (NOTE: opts.data should often be a function so it gets computed after teh form submits, otherwise form elements will be wrong)
    //5.  Works seamlessly with jQuery validate plugin
    $.madAjax = function (opts, form) {
        if (form)
            form = $(form);
        //if the form is already being submitted (user pressed enter twice or hit the button twice), we ignore this submission
        if (form && form.data("submitting")) {
            return false;
        }

        var ajaxOps = jQuery.extend({}, $.madAjaxOptions, opts); //make a copy of opts because it can be used for multiple form submissions, and overwriting data the first time would screw things up the second time

        if ($.isFunction(ajaxOps.url))//lazy eval of url so it can use dynamic data, like 'video/' + $('#xxx').val()
            ajaxOps.url = ajaxOps.url(form);

        if (!$.exists(ajaxOps.data) && form) {
            var values = form.formToArray();
            ajaxOps.data = {};
            $.each(values, function () {
                ajaxOps.data[this.name] = this.value;
            });
        }
        else if ($.isFunction(ajaxOps.data))//data was a function so we can evaluate it right now.  remember options are json objects often declared in the page load.  since they return input values, a function would be accurate while creating the data object at load time would return all the default values
            ajaxOps.data = ajaxOps.data();

        if (ajaxOps.dataType === "json" && $.isDefined(ajaxOps.data))
            ajaxOps.data = JSON.stringify(ajaxOps.data);

        ajaxOps.error = function (XMLHttpRequest, textStatus, errorThrown) {
            if (form)
                form.data("submitting", false);

            var status = XMLHttpRequest.status;
            var responseResult = null; //an object deserialized from the response body, which often includes a description and app-specific error code.  if the object couldn't be deserialized, this is the response text
            if (status != 0) {
                var contentType = XMLHttpRequest.getResponseHeader("Content-Type");
                if ($.exists(contentType) && contentType.indexOf('json') >= 0 && XMLHttpRequest.responseText.length > 0) {
                    try {
                        responseResult = JSON.parse(XMLHttpRequest.responseText); //ignore poorly formed json                    
                    } catch (e) {
                    }
                }
                if (!$.exists(responseResult)) {
                    responseResult = XMLHttpRequest.responseText;
                }
            }

            //code-specific calls, which return true to avoid default error handling.
            var errorHandled =
                (opts.error && opts.error(status, responseResult, XMLHttpRequest) === true) || //first use a generic error handler.  give it the status, data, and core request object if it's needed.  then fall through to code-specific problems
                (status == 0 && ajaxOps.aborted && ajaxOps.aborted(responseResult) === true) ||
                (status == 0 && ajaxOps.abortedDefault && ajaxOps.abortedDefault(responseResult) === true) ||
                (status == 400 && ajaxOps.badRequest && ajaxOps.badRequest(responseResult) === true) ||
                (status == 400 && ajaxOps.badRequestDefault && ajaxOps.badRequestDefault(responseResult) === true) ||
                (status == 401 && ajaxOps.unauthorized && ajaxOps.unauthorized(responseResult) === true) ||
                (status == 401 && ajaxOps.unauthorizedDefault && ajaxOps.unauthorizedDefault(responseResult) === true) ||
                (status == 403 && ajaxOps.forbidden && ajaxOps.forbidden(responseResult) === true) ||
                (status == 403 && ajaxOps.forbiddenDefault && ajaxOps.forbiddenDefault(responseResult) === true) ||
                (status == 404 && ajaxOps.notFound && ajaxOps.notFound(responseResult) === true) ||
                (status == 404 && ajaxOps.notFoundDefault && ajaxOps.notFoundDefault(responseResult) === true) ||
                (status == 500 && ajaxOps.serverError && ajaxOps.serverError(responseResult) === true) ||
                (status == 500 && ajaxOps.serverErrorDefault && ajaxOps.serverErrorDefault(responseResult) === true);

            //generic error method.  notice use of opts and not ajaxopts, since that would result in infinite recursion.  same goes for success stuff down below
            //            if (!errorHandled && opts.error && opts.error(XMLHttpRequest, textStatus, errorThrown, responseResult) === true) {
            //                errorHandled = true;
            //            }
            var e = $.Event();
            e.type = "ajaxError.madAjax";
            $(document).trigger(e, { result: responseResult });

            if (!errorHandled && !e.isDefaultPrevented()) {
                if (status == 401) {
                    $.goTo(window.urlPrefixes.relativeHttps + "login?heading=" + encodeURIComponent("Please Log In to Continue") + "&returnUrl=" + encodeURIComponent(top.location.href));
                } else {
                    var msg = 'Uh oh!  There was an error while communicating with our server.  ' + XMLHttpRequest.statusText + XMLHttpRequest.responseText;
                    alert(msg);
                }
            }
        };

        ajaxOps.success = function (result) {

            if (form)
                form.data("submitting", false);

            //this allows controllers to give a redirect for an ajax command.  a regular redirect can't happen during ajax
            if ($.exists(result) && $.hasChars(result._RedirectUrl)) {
                $.goTo(result._RedirectUrl);
                return;
            }

            //this gives user code a chance to cancel the entire deal or check for repeat clicks 
            var e = $.Event();
            e.type = "ajaxSuccess.madAjax";
            $(document).trigger(e, { result: result });

            if (e.isDefaultPrevented()) {
                //this allows client code to, say, cancel a file upload plugin
                if (opts.successCancelled) {
                    opts.successCancelled(result);
                }
            }
            else if (opts.success) {
                opts.success(result);
            }
        };


        ajaxOps.beforeSend = function (XMLHttpRequest) {
            if (opts.beforeSend) {
                if (false === opts.beforeSend(XMLHttpRequest)) {
                    if (form)
                        form.data("submitting", false);
                    return false;
                }
            }
        };

        if (form) {
            form.data("submitting", true);
        }
        $.ajax(ajaxOps);
    };


    //preventDoubleSubmit option:  useful for non-ajax forms.  default is only when not using ajax
    $.fn.madValidate = function (options) {
        options = $.extend({ useTooltips: true, useAjax: true }, options);
        //a hacky way to know which validation errors are now gone so we can re-enable tooltips

        var currShowErrors = options.showErrors;
        options.showErrors = function (errorMap, errorList) {
            if (currShowErrors)
                currShowErrors(errorMap, errorList);
            else
                this.defaultShowErrors(errorMap, errorList);

            $.each(this.errorList, function () {
                var tooltip = $(this.element).data("tooltipPopup");
                if (tooltip) {
                    tooltip.disable();
                }
            });
            $.each(this.successList, function () {
                var tooltip = $(this).data("tooltipPopup");
                if (tooltip) {
                    tooltip.enable();
                }
            });
        };

        var currErrorPlacement = options.errorPlacement;
        options.errorPlacement = function (errorElement, element) {
            if (!currErrorPlacement) {
                //sometimes the popup needs to anchor to something besides the input element.  This was made originally for our Flash-based uploader.              
                if (options.errorElementAnchors && _.has(options.errorElementAnchors, element.attr("id"))) {
                    element = options.errorElementAnchors[element.attr("id")](); //errorElementAnchors is a function that shoudl return a jquery object
                }
                if (!options.useTooltips) {
                    errorElement.insertAfter(element); //same as plugin's default
                }
                else {
                    var popup = new $.madPopup({
                        anchor: element,
                        hAnchorAlign: "right",
                        hPopupAlign: "left",
                        vAnchorAlign: "center",
                        vPopupAlign: "center",
                        showOnHover: false,
                        popup: errorElement
                    });
                    element.data("errorPopup", popup);
                    popup.positionPopup();
                    popup.show();
                }
            }
            else
                currErrorPlacement(errorElement, element);
        };

        var originalSubmitHandler = options.submitHandler;
        options.submitHandler = function (form) {
            if (options.beforeSubmit) {//sometimes better than using validation rules.  especially for ajax functions...validation plugin will often call rules many times and you may not want to shut off onfocusout for everything but one field
                if (!options.beforeSubmit()) {
                    return false;
                }
            }
            var $form = $(form);
            var preventDoubleSubmit = options.preventDoubleSubmit;
            if (_.isUndefined(preventDoubleSubmit)) {
                preventDoubleSubmit = !options.useAjax;
            }
            if (!preventDoubleSubmit || !$form.data("submitted")) {
                $form.trigger("submitting");
                $form.data("submitted", true);
                if ($.exists(originalSubmitHandler)) {
                    originalSubmitHandler(form);
                }
                else if (options.useAjax) {
                    var ajaxOps = {};
                    if (options.data)
                        ajaxOps.data = options.data;
                    if (options.url)
                        ajaxOps.url = options.url;
                    if (options.submitSuccess)
                        ajaxOps.success = options.submitSuccess;
                    if (options.unauthorized)
                        ajaxOps.unauthorized = options.unauthorized;
                    if (options.forbidden)
                        ajaxOps.forbidden = options.forbidden;
                    if (options.badRequest)
                        ajaxOps.badRequest = options.badRequest;
                    if (options.serverError)
                        ajaxOps.serverError = options.serverError;
                    if (options.notFound)
                        ajaxOps.notFound = options.notFound;
                    if (options.error)
                        ajaxOps.error = options.error;
                    if (options.beforeSend)
                        ajaxOps.beforeSend = options.beforeSend;
                    $.madAjax(ajaxOps, form);
                }
                else {
                    form.submit();
                }
            }
        };

        var validator = this.validate(options); //will be undefined if the target doesn't exist
        if (validator) {
            if (options.useTooltips)
                validator.errorContext = $("body"); //this line is critical because the labels are added by errorPlacement to the body.  once a "context" is added to teh popup, you can use the form as the context and this can be removed
        }
        return validator;
    };

    if ($.validator) {
        //add a plugin that works with text input that uses the watermark plugin
        $.validator.addMethod(
            "watermarkedRequired",
            function (value, element) {
                var $element = $(element);
                if ($element.hasWatermark())
                    return false;
                return $.trim(value).length > 0;
            },
            'Required'
        );
        $.validator.addMethod(
            "customFn",
            function (value, element, fn) {
                return fn(element);
            },
            'Required'
        );
        $.validator.addMethod(
            "regex",
            function (value, element, regexp) {
                return this.optional(element) || regexp.test(value);
            },
            'Please check your input.'
        );
    }

    //resets the validation for the form and resets its fields
    $.fn.resetValidation = function (clearFields) {
        return this.each(function () {
            var form = $(this);
            var validator = form.data("validator");
            if (validator) {
                if (clearFields) {
                    validator.resetForm();
                    //we use the autogrow plugin a lot and when you clear the value after losing focus, it will not reset its height back to one line.  so we handle it here manually.  this way, when the form is reactivated the textarea won't be really tall
                    form.find("textarea").each(function () {
                        var autogrowPlugin = $(this).data("autogrow");
                        if ($.exists(autogrowPlugin))
                            autogrowPlugin.checkExpand();
                    });
                }
                else {
                    var resetFn = $.fn.resetForm;
                    $.fn.resetForm = undefined; //trick the validation plugin into thinking the forms plugin function isn't available.
                    validator.resetForm();
                    $.fn.resetForm = resetFn;
                }

            }
        });
    };

    //indicates whether the object has a watermark visible from the watermark plugin
    $.fn.hasWatermark = function () {
        return this.hasClass(this.data("watermarkClass")); //"watermarkClass" is a constant from the watermark plugin
    };

    //taken from jQuery validation plugin
    $.isValidEmail = function (value) {
        return $.isString(value) && value.length > 0 && /^((([a-z]|\d|[!#\$%&'\*\+\-\/=\?\^_`{\|}~]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+(\.([a-z]|\d|[!#\$%&'\*\+\-\/=\?\^_`{\|}~]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+)*)|((\x22)((((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(([\x01-\x08\x0b\x0c\x0e-\x1f\x7f]|\x21|[\x23-\x5b]|[\x5d-\x7e]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(\\([\x01-\x09\x0b\x0c\x0d-\x7f]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF]))))*(((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(\x22)))@((([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.)+(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.?$/i.test(value.toLowerCase());
    };

})(jQuery);

//prevents a form from being submitted more than once - adapted from http://henrik.nyh.se/2008/07/jquery-double-submission
jQuery.fn.preventDoubleSubmit = function () {
    var alreadySubmitted = false;
    return jQuery(this).submit(function () {
        if (alreadySubmitted)
            return false;
        else
            alreadySubmitted = true;
    });
};


$.controlGroup = controlGroup = function(options) {
    var controlsContainer, group, input, inputType;
    group = $("<div class=\"control-group\">" + "<label class=\"control-label\"></label>" + "<div class=\"controls\"></div>" + "</div>");
    if (options.id) {
        group.attr('id', options.id);
    }
    controlsContainer = group.find("div.controls");
    if (_.isString(options.label)) {
        group.find("label").text(options.label);
    }
    if (options.tooltip) {
        $("<span class=\"tooltip-anchor label\">?</span>").attr("title", options.tooltip).tooltip().appendTo(group.find("label"));
    }
    inputType = options.type;
    if (_.isUndefined(inputType)) {
        if (options.input) {
            inputType = "custom";
        } else if (_.has(options, "value") && _.isBoolean(options.value)) {
            inputType = "checkbox";
        } else {
            inputType = "text";
        }
    }
    if (_.isString(options.prepend)) {
        controlsContainer = $("<div class=\"input-prepend\"><span class=\"add-on\"></span></div>").appendTo(controlsContainer);
        controlsContainer.find("span").text(options.prepend);
    }
    switch (inputType) {
        case "checkbox":
            input = $("<input type=\"checkbox\" />");
            if (_.has(options, "value") && options.value) {
                input.prop("checked", true);
            }
            break;
        case "textarea":
            input = $("<textarea></textarea>").val((_.has(options, "value") ? options.value : ""));
            if (_.has(options, "rows")) {
                input.prop("rows", options.rows);
            }
            break;
        case "custom":
            input = _.isFunction(options.input) ? options.input(controlsContainer) : options.input;
            break;
        case "none":
            input = null;
            break;
        default:
            input = $("<input type=\"text\" />").val((_.has(options, "value") ? options.value : ""));
    }
    if (input) {
        input.appendTo(controlsContainer);
        if (_.isString(options.inputClass)) {
            input.addClass(options.inputClass);
        }
        if (_.isString(options.inputName)) {
            input.attr("name", options.inputName);
        }
    }
    return group;
};


$.fn.formToObject = function () {
    var values;
    values = {};
    _.each(this.formToArray(), function (item) {
        if (_.has(values, item.name)) {
            if (_.isArray(values[item.name])) {
                return values[item.name].push(item.value);
            } else {
                return values[item.name] = [values[item.name], item.value];
            }
        } else {
            return values[item.name] = item.value;
        }
    });
    return values;
};