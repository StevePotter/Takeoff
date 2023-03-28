/// <reference path="Libs/jquery-1.4.1-vsdoc.js" />
/// <reference path="mediascend.js" />
/*
A simple tooltip library, built primarily using the madTooltips plugin

options:

*/
(function ($) {
    $.madTooltips = function (options) {
        var plugin = this, $plugin = $(this);
        this.options = options = $.extend({}, $.madTooltips.defaults, options);
        this.$this = $plugin;

        this.init();

        $.each(options, function (name, fn) {
            if ($.isFunction(fn))
            { $plugin.bind(name, fn); }
        });
    };

    $.madTooltips.defaults = {
        hAnchorAlign: "right",
        hPopupAlign: "left",
        vAnchorAlign: "center",
        vPopupAlign: "center",
        hoverDelay: 200,
        isPopupInteractive: false,
        showOnFocus: true,
        showOnHoverSelector: ":input", //a selector passed to the tooltip source element's is function
        showOnHoverCondition: "isNot", //if "is", us jquery's is().  otherwise use !is()
        tooltipsSelector: "label.tooltip",
        hideOnBlur: true,
        tooltipForAttr: "for",
        interactiveTooltipSelector: "input, textarea, select, button, a"
    };


    $.madTooltips.prototype = {
        popups: [],
        enabled: false,
        shownPopup: null,

        init: function () {
            var popups = this.popups, plugin = this, options = plugin.options;
            this.enabled = true;
            plugin.addTooltips($(plugin.options.tooltipsSelector));
        },

        addTooltips: function (tooltips) {
            var popups = this.popups, plugin = this, options = plugin.options;
            tooltips.each(function () {
                var popup = $(this);
                var ttFor = popup.attr(options.tooltipForAttr);
                if ($.hasChars(ttFor)) {
                    //if the attribute starts with a letter, assume it's for an element's ID.  otherwise it could be something else
                    var source;
                    var reLetter = /^[a-zA-Z]$/;
                    if (reLetter.test(ttFor.charAt(0))) {
                        source = $("#" + ttFor);
                    }
                    else {
                        source = $(ttFor);
                    }

                    var showOnHover = source.is(options.showOnHoverSelector);
                    if (options.showOnHoverCondition === "isNot")
                        showOnHover = !showOnHover;

                    plugin.add(source, popup,
                    {
                        showOnHover: showOnHover,
                        isPopupInteractive: popup.find(options.interactiveTooltipSelector).hasElements()
                    });
                }
            });
        },

        add: function (source, tooltip, popupConfig) {
            source = $(source);
            var popup = source.madPopup($.extend({}, this.options, { api: true, popup: $(tooltip) }, popupConfig));
            source.data("tooltip", this); //used by mediascend.forms
            source.data("tooltipPopup", popup); //used by mediascend.forms 
            $(popup).bind('beforeShow', this, this.onPopupShowing);
            this.popups.push(popup);
            return popup;
        },

        //occurs when one of the popups shows.  this will hide the other current shown popup
        onPopupShowing: function (e) {
            var plugin = e.data;
            var popupShowing = e.target;
            if ($.exists(plugin.shownPopup))
                plugin.shownPopup.hide();
            plugin.shownPopup = popupShowing;
        },


        enable: function () {
            if (this.enabled)
                return;
            this.enabled = true;
            $.each(this.popups, function () {
                this.enable();
            });
        },

        disable: function () {
            if (!this.enabled)
                return;
            this.enabled = false;

            if ($.exists(this.shownPopup))
                this.shownPopup.hide();

            $.each(this.popups, function () {
                this.disable();
            });
        },

        //updates the popup plugin options for given source element's tooltip
        updateConfig: function (source, popupOpts) {
            var popup = $(source).data("tooltipPopup");
            if (popup) {
                var opts = popup.options;
                $.each(popupOpts, function (name, val) {
                    opts[name] = val;
                });
            }
        }

    };

    // jQuery plugin implementation
    jQuery.fn.madTooltips = function (opts) {

        // return existing instance
        var plugin = this.data("madTooltips");
        if (plugin)
        { return plugin; }

        this.each(function (i) {
            var curr = $(this);
            plugin = new $.madTooltips(opts);
            curr.data("madTooltips", plugin);
        });

        return (opts && opts.api) ? plugin : this;
    };

})(jQuery);
