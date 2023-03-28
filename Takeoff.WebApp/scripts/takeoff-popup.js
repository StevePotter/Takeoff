/// <reference path="jquery-1.6.2-vsdoc.js" />
/// <reference path="takeoff-helpers.js" />
/*


* A bag of tricks for placing dom elements ("popup") around some other element ("anchor").  madPopup provides web apps with highly customizable abilities to:
*   - show the popup on hover, click, and focus event.
*   - place the popup at any forseeable position around the anchor.  
* 
* This is useful for things like:
* - tooltips
* - menus
* - alerts & confirmations
*
* Lots of events and almost everything can be messed with.  But the API is still a bit easy to use.
*  
* version: 1.0
* Basic popup functionality.  Takes one element (popup) and positions it relative to another (anchor).  Responds to various triggers.
* This is used to create tooltips, 'ghost bars', and other UI goodies.
* Main features:
- Can position popups in many different ways
- Show/hide popups on mouse hover and/or focus, with adjustable delays
- Allows for "interactive" popups, which means the popop won't hide when it has focus and/or mouse capture.  This is probably the biggest differentiator between this library and others, because most don't allow for this.

* 
*
* Copyright (c) 2009 Mediascend, Inc.
 

options:
- Anchor (horizontal and vertical can be different)
- popup - the jQuery object that represents the popup.  Can also be a function, in which case it's called the first time to create the popup on demand
- anchor – the jQuery object that the popup gets positioned around

- hAnchorAlign – Type: String Default: "center", Values: 'left', 'center', 'right'.  
- hPopupAlign – Type: String, Default: "center".  Values are left, center, or right.  Aligns a side of the poup with the anchor align.  
- hOffset - int.  Number of pixels to adjust the popup relative to its normal position.

- vAnchorAlign – Type: String, Default: "top".  Values are top, center, bottom, or cursor
- vPopupAlign – Type: String, Default: "bottom".  Values are top, center, or bottom.  Aligns a side of the poup with the anchor align.  
- vOffset - Type: Number, Default: 0.  Number of pixels to adjust the popup relative to its normal position.

- showOnHover - Type: Boolean, Default: true. Indicates whether the popup will show once the mouse is over the anchor for [hoverShowDelay] milliseconds.
- hideOnHoverLost - Indicates whether the popup and will hide once the mouse leaves for [hoverHideDelay] milliseconds.  Meant only if showOnHover is true
- hoverDelay.  Type: Number, Default: 300. Fallback value for hoverShowDelay and hoverHideDelay.
- hoverShowDelay.  Type: Number, Default: [hoverDelay].  The milliseconds the mouse needs to be over the anchor in order for the popup to show.
- hoverHideDelay.  Type: Number, Default: [hoverDelay].  The milliseconds the mouse needs to be awway from the anchor in order for the popup to hide.

- showOnFocus - Type: Boolean, Default: false.  Indicates whether the popup will show when the user focuses on the element, which is especially useful with input elements.
- hideOnBlur - Type: Boolean, Default: showOnFocus.  Indicates whether clicking outside the popup and/or anchor once focus has been established will hide the popup.  Set this to true for modal popups with their own close button.

- isPopupInteractive.  Type: Boolean, Default: false.  If true, the user can mouse over and focus in on the popup without it hiding.  Otherwise, the popup will hide.  Note that if showHideOnHover is false, mouse position will not play a role here.

- hideOnFocusedAnchorClick: Type: Boolean.  Default: false.  When true, clicking on the anchor once focus has already been established will hide the popup.  This is used to toggle the popup when you click on the anchor.  NOTE: this should only be used when showOnFocus is true

- autoPosition.  Type: Boolean, Default: false.  If true, popup will be positioned using absolute position according to alignments and offsets.  Set this to false when you have the popup positioned already using css.  Note that all the alignment and offset properties are ignored when this is true.
- repositionCheckInterval - Type: Number: Default: 500.  Only applies when autoPosition is true.  When the popup is visible, it will check for a change in necessary popup position.  This happens when window resizes or elements change.  This defines that polling interval.

known issues:
- interactive popups sometimes don't recognice focusing of inner form elements (textbox) and when teh mouse goes out, the popup hides

todo:
toggle popup on anchor click
better popup opening transitions  - see if you can borrow from another plugin or something
allow for 'alternate' alignments for when the popup doesn't fit in the current viewport
positioning "context" element, including maybe the possibility to make a div position relative with absolute positioning
ability to place multiple targets...allowing for something like "edit bar"
consider some kind of system for "exclusivity" or popup groups, which would make only one popup visible at a time, despite any hide settings.  good for tooltips
more events
simplify the alignment properties.  consider consolidating the 'anchor' and 'popup' align properties into jsut hAlignment and vAlignment.  also consider combining all into a single property, alignment
for cases like a swf over a popup (file uploading in dropdown), you must check for mouse position via mouse events, not just mouseout of anchor and popup
use hoverintent for hovering
show/hide effects in options
allow functions for resolving options


//mouse position: http://docs.jquery.com/Tutorials:Mouse_Position
//info on relative positioning: http://plugins.jquery.com/node/6580
*/

(function ($) {
    $ = jQuery; //todo: remove for production...this is just for vs 2008 intellisense

    // constructor for popup. 
    $.madPopup = function (options) {
        var plugin = this, $plugin = $(this);
        this.options = options = $.extend({}, $.madPopup.defaults, options);
        this.$this = $plugin;

        this.init();

        $.each(options, function (name, fn) {
            if ($.isFunction(fn))
            { $plugin.bind(name, fn); }
        });

    };

    $.madPopup.defaults = {
        hAnchorAlign: "center",
        hPopupAlign: "center",
        hOffset: 0,
        vAnchorAlign: "top",
        vPopupAlign: "bottom",
        vOffset: 0,

        showOnHover: true,
        hideOnHoverLost: undefined,


        hoverDelay: 300,
        repositionCheckInterval: 500,

        showOnFocus: false,
        hideOnBlur: undefined, //set to showOnFocus during init

        isPopupInteractive: false,
        hideOnFocusedAnchorClick: false,
        autoPosition: true
    };


    $.madPopup.prototype = {
        popupState: "hidden",

        //the $.timer for when the mouse is over the anchor.
        mouseOverTimer: null,
        mouseOutTimer: null,

        //the $.timer used to make sure the popup's position is current.  the popup uses absolute positioning, so if the page elements change, like javascript collapsing something, the popup may need to be repositioned
        repositionTimer: null,


        enabled: true,

        //indicates whether the anchor or popup has input focus.  this is only reliable if showOnFocus is true
        hasFocus: false,

        //indicates whether the anchor or popup is hovered over.  this is only reliable if showOnHover is true
        hasHover: false,

        //for the hideOnFocusedAnchorClick feature
        hideOnNextClick: false,
        hideOnNextClickTimerId: -1,
        hideOnNextClickTimerDuration: 100,

        init: function () {
            var me = this;
            var options = me.options;
            var anchor = me.anchor = jQuery(this.getjQueryOpt(options.anchor));
            if (!anchor)
                throw "anchor not supplied";

            if (_.isUndefined(options.hideOnHoverLost)) {
                options.hideOnHoverLost = options.showOnHover;
            }
            if (_.isUndefined(options.hideOnBlur)) {
                options.hideOnBlur = options.showOnFocus;
            }

            //set up the mouseOver show/hide stuff
            if (options.showOnHover || options.hideOnHoverLost) {
                this.mouseOverTimer = new $.timer({
                    delay: $.definedOr(options.hoverShowDelay, options.hoverDelay),
                    operation: function () {
                        me.hasHover = true;
                        if (me.enabled && options.showOnHover)
                            me.show();
                    }
                });
                anchor.mouseenter(function (e) { me.processMouseOver(e); });

                this.mouseOutTimer = new $.timer({
                    delay: $.definedOr(options.hoverHideDelay, options.hoverDelay),
                    operation: function () {
                        me.hasHover = false;
                        if (me.enabled && options.hideOnHoverLost)
                            me.hide();
                    }
                });
                anchor.mouseleave(function (e) { me.processMouseOut(e); });
            }

            if (options.showOnFocus || options.hideOnBlur) {
                anchor.bind("focus keydown click", function (e) { me.processFocus(e); }); //had to include click because Chrome wasn't recognizing focus when I clicked a button or anchor
                anchor.blur(function (e) { me.processBlur(e); });

                if (options.hideOnFocusedAnchorClick) {
                    anchor.click(function (e) { me.anchorClick(e); });
                }
            }

        },

        processFocus: function (e) {
            this.setHasFocus(true);
        },

        processBlur: function (e) {
            if (!this.options.isPopupInteractive) {
                this.setHasFocus(false);
            }
        },

        setHasFocus: function (hasFocus, ignoreShowOrHideFocusOpt) {
            if (this.hasFocus === hasFocus)
                return;

            this.hasFocus = hasFocus;

            var options = this.options;
            var plugin = this;
            if (hasFocus)//now we have focus
            {
                if (options.isPopupInteractive || options.hideOnBlur) {
                    //originally i tried to handle the focus event.  but that ended up being a mess.  first, FF and others required a tabindex in order to even fire the focus event on a div.  without that, FF will set the document as the target for a focus/blur event, which would cause us to close.    Then input elements wouldn't bubble their focus/blur events.  Then Safari just refused to fire the focus event on a checkbox, which hid the tooltip when a checkbox was in it.  To make matters worse, blur events in FF and Chrome didn't have pagex/y set, nor did they have the correct srcElement.  So I gave up and did it the cheap way.
                    $(document).bind("mousedown keydown", this, this.processFocusChangingEvent);
                }


                if (this.options.hideOnFocusedAnchorClick) {//may not seem like the best idea, but handling the event after setting focus is the best way to avoid race conditions where the focus is triggered then a click, which caused the popup to show then immediately hide
                    if (this.hideOnNextClickTimerId > 0)
                        window.clearTimeout(this.hideOnNextClickTimerId);
                    this.hideOnNextClickTimerId = window.setTimeout(function () {
                        plugin.hideOnNextClick = true;
                    }, plugin.hideOnNextClickTimerDuration); //need to wait just a bit for the click event to pass if it's about to fire
                }

                if (this.enabled && (options.showOnFocus || true == ignoreShowOrHideFocusOpt)) {
                    this.show();
                }
            }
            else {
                if (plugin.options.hideOnFocusedAnchorClick) {//may not seem like the best place, but handling the event after setting focus is the best way to avoid race conditions where the focus is triggered then a click, which caused the popup to show then immediately hide
                    this.hideOnNextClick = false;
                    if (this.hideOnNextClickTimerId > 0) {
                        window.clearTimeout(plugin.hideOnNextClickTimerId);
                        plugin.hideOnNextClickTimerId = -1;
                    }
                }

                if (options.isPopupInteractive || options.hideOnBlur) {
                    $(document).unbind("mousedown keydown", this.processFocusChangingEvent); //remove event handler from show()
                }
                if (this.enabled && (options.hideOnBlur || true == ignoreShowOrHideFocusOpt)) {
                    this.hide();
                }
            }
        },

        //gets the final resolved popup.  call this when you gotta do anything interesting with the popup. 
        getPopup: function () {
            var popup = this._popup;
            if (popup)
                return popup;

            //the first time it is requested
            var options = this.options;
            popup = this._popup = this.getjQueryOpt(options.popup);
            if (!popup)
                throw "popup not defined";

            if (options.autoPosition) {
                //this is the first time we've gotten the popup so let's set up a few things and possibly add it to
                popup.css({ display: "", visibility: "hidden", position: "absolute" });
                popup.appendTo("body"); //todo: see if it's not added to the document yet and avoid unnecessarily removing/adding
            }

            var me = this;

            if (options.isPopupInteractive) {
                if (options.showOnFocus || options.hideOnBlur) {
                    popup.bind("focus keydown click", function (e) { me.processFocus(e); }); //had to include click/keydown because Chrome wasn't recognizing focus when I clicked a button or anchor
                    popup.blur(function (e) { me.processBlur(e); });
                }
                //note: this code block could be moved to show(), but just make sure you don't add handlers multiple times
                if (options.showOnHover || options.hideOnHoverLost) {
                    popup.mouseenter(function (e) { me.processMouseOver(e); });
                    popup.mouseleave(function (e) { me.processMouseOut(e); });
                }
            }
            return popup;
        },

        //for the hideOnFocusedAnchorClick feature
        anchorClick: function (e) {
            if (this.hideOnNextClick) {
                e.preventDefault();
                this.setHasFocus(false, true);
            }
        },

        getjQueryOpt: function (value) {
            if ($.isNullOrUndefined(value))
                return null;
            else if ($.isjQuery(value))
                return value;
            else if ($.isString(value))
                return $(value);
            else if ($.isFunction(value))
                return this.getjQueryOpt(value.apply(this));
            else if ($.isElement(value))
                return $(value);
            throw "Invalid type passed.  Valid types are string, jQuery object, function, or dom element.";
        },

        //indicates whether the two elements passed are the same or the "element" is a descendant of "ancestor"
        isMatchOrDescendant: function (ancestor, element) {
            if (ancestor === element)
                return true;
            var climber = element.parentNode;
            while (climber) {
                if (climber === ancestor) {
                    return true;
                }
                climber = climber.parentNode;
            }
            return false;
        },

        //called when hasFocus is true
        processFocusChangingEvent: function (e) {
            var plugin = e.data;

            var onAnchor = plugin.isMatchOrDescendant(plugin.anchor[0], e.target);
            if (plugin.options.isPopupInteractive) {
                plugin.setHasFocus(onAnchor);
            }
            else {
                plugin.setHasFocus(onAnchor || plugin.isMatchOrDescendant(plugin.anchor[0], e.target));
            }
        },

        //        //helper function to indicate whether the x and y coordinates are within the given element's bounds.  x and y should be relative to the document.
        //        //took out but it's correct...maybe you could use it later
        //        isLocationOverElement: function(x, y, element) {
        //            var elementOffset = element.offset();
        //            return x >= elementOffset.left && x <= (elementOffset.left + element.outerWidth()) && y >= elementOffset.top && y <= (elementOffset.top + element.outerHeight());
        //        },

        processMouseOver: function () {
            if (this.enabled && !this.hasFocus)//if we are focused on it, ignore mouse hovers
            {
                this.mouseOutTimer.cancel();
                if (!this.mouseOverTimer.isRunning())
                    this.mouseOverTimer.start();
            }

        },

        processMouseOut: function (event) {
            if (this.enabled && !this.hasFocus) {
                this.mouseOverTimer.cancel();
                if (!this.mouseOutTimer.isRunning())
                    this.mouseOutTimer.start();
            }
        },

        //enable is really more of a "pause" on showing/hiding because anchor hover/focus still remain (for validation scenarios)
        enable: function () {
            if (this.enabled)
                return;
            this.enabled = true;
            if ((this.hasFocus && this.options.showOnFocus) || (this.hasHover && this.options.showOnHover)) {//it was reenabled while the focus or hover remained.  this happens often with tooltips and validation scenarios
                this.show();
            }
        },

        disable: function () {
            if (!this.enabled)
                return;
            this.enabled = false;
            if (this.popupState !== "hidden")
                this.hide();
        },


        //public, shows the popup.
        show: function () {
            if (this.popupState === "hidden") {
                var options = this.options;
                var popup = jQuery(this.getPopup());//get popup before beforeShow so in that event popup will be guaranteed

                //raise the beforeShow event
                var $this = $(this);
                var e = $.Event();
                e.type = "beforeShow";
                $this.trigger(e);

                if (options.autoPosition) {
                    this.positionPopup();
                }

                e = $.Event();
                e.type = "showPopup";
                $this.trigger(e, [popup]);
                if (!e.isDefaultPrevented()) {
                    //the visibility will be 'hidden' (which is needed by positionPopup()), which doesn't work with jquery show.  so we change it to display none
                    popup.css({ visibility: "visible", display: "none" });
                    popup.show();
                }

                this.popupState = "visible";

                if (options.autoPosition) {
                    if (!$.exists(this.repositionTimer)) {
                        var me = this;
                        this.repositionTimer = new $.timer({
                            delay: options.repositionCheckInterval,
                            repeatCount: -1,
                            operation: function () {
                                me.positionPopup();
                            }
                        });
                    }
                    this.repositionTimer.start();
                }

                //raise the afterShow event
                e = $.Event();
                e.type = "afterShow";
                $this.trigger(e);

            }
        },

        hide: function () {
            if (this.popupState === "visible") {
                //raise the beforeHide event
                var options = this.options;
                var $this = $(this);
                var e = $.Event();
                e.type = "beforeHide";
                $this.trigger(e);

                var popup = $(this.getPopup());

                e = $.Event();
                e.type = "hidePopup";
                $this.trigger(e, [popup]);
                if (!e.isDefaultPrevented()) {
                    popup.hide();
                }

                if (options.autoPosition) {
                    popup.css({ visibility: "hidden" }); //this is needed so we can keep the popup invisible but take measurements for when we show it next
                    this.repositionTimer.cancel();
                }

                this.popupState = "hidden";

                this.setHasFocus(false);

                //raise the afterHide event
                e = $.Event();
                e.type = "afterHide";
                $this.trigger(e);
            }
        },

        getPosition: function () {
            var anchor = jQuery(this.anchor);
            var popup = jQuery(this.getPopup());
            var anchorOffset = anchor.offset();
            var options = this.options;

            var left;
            switch (options.hAnchorAlign) {
                case "left":
                    left = anchorOffset.left;
                    break;
                case "center":
                    left = anchorOffset.left + (anchor.outerWidth() / 2);
                    break;
                case "right":
                    left = anchorOffset.left + anchor.outerWidth();
                    break;
                default:
                    throw "hAnchorAlign not correct";
            }
            switch (options.hPopupAlign) {
                case "center":
                    left -= (popup.outerWidth() / 2);
                    break;
                case "right":
                    left -= popup.outerWidth();
                    break;
            };
            left += options.hOffset;

            var top;
            switch (options.vAnchorAlign) {
                case "top":
                    top = anchorOffset.top;
                    break;
                case "center":
                    top = anchorOffset.top + (anchor.outerHeight() / 2);
                    break;
                case "bottom":
                    top = anchorOffset.top + anchor.outerHeight();
                    break;
                default:
                    throw "vAnchorAlign not correct";
            }
            switch (options.vPopupAlign) {
                case "center":
                    top -= (popup.outerHeight() / 2);
                    break;
                case "bottom":
                    top -= popup.outerHeight();
                    break;
            };
            top += options.vOffset;

            return { top: top + "px", left: left + "px" };
        },

        positionPopup: function () {
            jQuery(this.getPopup()).css(this.getPosition());
        }

    };

    // jQuery plugin implementation
    jQuery.fn.madPopup = function (opts) {

        // return existing instance
        var plugin = this.data("madPopup");
        if (plugin)
        { return plugin; }

        this.each(function (i) {
            var curr = $(this);
            plugin = new $.madPopup($.extend({ anchor: curr }, opts));
            curr.data("madPopup", plugin);
        });

        return (opts && opts.api) ? plugin : this;
    };

})(jQuery);