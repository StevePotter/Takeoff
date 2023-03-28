/// <reference path="jquery-1.6.2-vsdoc.js" />
/// <reference path="takeoff-helpers.js" />
/**
Copyright (c) 2010 Mediascend, Inc.
http://www.mediascend.com
Notification plugin - shows warnings, errors, and information messages in a cool little popup that rolls down from up top

If the mouse is over notification message, it will not hide until the mouse is out.

Options:

*/
(function ($) {
    $ = jQuery;

    function madNotification(container, options) {
        options = $.extend({},
        {
            infoClass: "info",
            infoDuration: 3000,
            warningClass: "warning",
            warningDuration: 3000,
            successClass: "success",
            successDuration: 3000,
            errorClass: "error",
            errorDuration: 5000,
            showDuration: 200,
            hideDuration: 100,
            api: false
        }, options);

        var plugin = this, $plugin = $(this), messageContainer = container.find("span.message"), currMessageTimerId = NaN, handlingHover = false, isMouseOver = false, hideOnMouseOut = false, isVisible = false;

        $.extend(plugin, {

            warning: function (content, duration) {
                this.show(content, options.warningClass, duration || options.warningDuration);
            },

            error: function (content, duration) {
                this.show(content, options.errorClass, duration || options.errorDuration);
            },

            success: function (content, duration) {
                this.show(content, options.successClass, duration || options.successDuration);
            },

            info: function (content, duration) {
                this.show(content, options.infoClass, duration || options.infoDuration);
            },

            show: function (content, cssClass, showDuration) {

                hideOnMouseOut = false; //reset it
                var wasAlreadyVisible = isVisible;
                isVisible = true;

                if (!handlingHover) {
                    container.mouseenter(function () {
                        isMouseOver = true;
                    }).mouseleave(function () {
                        isMouseOver = false;
                        if (hideOnMouseOut) {
                            plugin.hide();
                            hideOnMouseOut = false;
                        }
                    });
                    handlingHover = true;
                }

                plugin.cancelTimer();

                if ($.isString(content))
                    messageContainer.empty().text(content);
                else if ($.isjQuery(content))
                    messageContainer.empty().append(content);

                container.removeClass().addClass(cssClass);
                if (wasAlreadyVisible) {
                    plugin.startTimer(showDuration);
                }
                else {
                    container.slideDown(options.showDuration, function () {
                        plugin.startTimer(showDuration);
                    });
                }
            },

            hide: function () {
                if (!isVisible)
                    return;
                plugin.cancelTimer();
                container.slideUp(options.hideDuration);
                currMessageTimerId = NaN;
                isVisible = false;
            },

            //sets the timer to hide the notification window
            startTimer: function (timeout) {
                currMessageTimerId = setTimeout(function () {
                    if (isMouseOver) {
                        hideOnMouseOut = true;
                    }
                    else {
                        plugin.hide();
                    }
                }, timeout);
            },

            cancelTimer: function () {
                if ($.isPositiveNumber(currMessageTimerId)) {
                    clearTimeout(currMessageTimerId);
                    currMessageTimerId = NaN;
                }
            },

            visible: function () {
                return isVisible;
            }

        });


    };


    // jQuery plugin implementation
    $.fn.madNotification = function (opts) {
        // return existing instance
        var plugin = this.data("madNotification");
        if (plugin) {
            return opts.api ? plugin : this;
        }

        var notificationContainer = $("#notification", this);
        if (!notificationContainer.hasElements()) {
            $(this).append('<div id="notification" class="warning"><span class="icon"></span><span class="message"></span></div>');
            notificationContainer = $("#notification", this);
        }

        plugin = new madNotification(notificationContainer, opts);
        notificationContainer.data("madNotification", plugin);

        return opts.api ? plugin : this;
    };


})(jQuery); 


