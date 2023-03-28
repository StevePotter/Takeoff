/// <reference path="jquery-1.6.2-vsdoc.js" />
/// <reference path="swfobject.debug.js" />
/// <reference path="mediascend.core-1.1.js" />

/*
* Mediascend player library
* version: 1.1
* Provides everything needed to run the Mediascend video player
*
* Copyright (c) 2009 Mediascend, Inc.
* 
* Requires: mediascend.core-1.1.js, jQuery v1.3+, SWFObject v2.2
* 
*/

/////////////////////////////////////////////////////////////////////////////////
/*
* Player - controller for Mediascend's Flash-based video player
*/
/////////////////////////////////////////////////////////////////////////////////

; (function (jQuery) {
    $ = jQuery;

    function madPlayer(options) {
        var plugin = this, $plugin = $(this);
        this.options = options = $.extend({}, madPlayer.defaults, options);
        this.$this = $plugin;

        $.each(options, function (name, fn) {
            if ($.isFunction(fn))
            { $plugin.bind(name, fn); }
        });

        //the custom name attribute fixed the ExternalInterface.objectId from being null in FF and Chrome.
        var attributes = {
            name: options.pluginId
        };

        if (!options.flashVars.callbackScript)
            options.flashVars.callbackScript = options.pluginScript + ".flash_";

        swfobject.embedSWF(options.url, options.pluginId, options.width, options.height, options.flashVersion, options.expressInstallUrl, options.flashVars, options.flashParams, attributes,
                    function (e) {
                        if (e.success) {
                            plugin.swfElement = e.ref;
                        }
                        $plugin.trigger("onFlashEmbedded", [e.success]);
                    });
    };



    madPlayer.defaults = {
        width: "100%",
        height: "100%",
        flashVersion: "9.0.0",
        pluginScript: "", //IN JQUERY 1.4 this is required!!!
        expressInstallUrl: window.urlPrefixes.asset + "Assets/expressInstall.swf",
        flashVars: {},
        flashParams: {
            wmode: "transparent",
            quality: "high",
            allowScriptAccess: "always",
            allowfullscreen: "true"
        }
    };


    madPlayer.prototype =
    {
        //gets the object element for the flash player
        getFlashElement: function () {
            return this.swfElement;
        },

        //        //invoked by Flash, this occurs when the total number of file bytes selected by the user changes.
        //        flash_onInterfaceReady: function() {
        //            if ($.isDefined(this.options.state)) {
        //                this.loadState(this.options.state);
        //                var options = this.options;
        //                if (options.onInterfaceReady)
        //                    options.onInterfaceReady();
        //            }
        //        },

        loadState: function (state) {
            this.getFlashElement().mp_loadState(state);
        },

        play: function () {
            this.getFlashElement().mp_invokeAction0("play");
        },

        seek: function (newPosition) {
            this.getFlashElement().mp_invokeAction1("seek", newPosition);
        },

        dispose: function () {
//            this.getFlashElement().mp_invokeAction0("stop");
        },

        pause: function () {
            this.getFlashElement().mp_pause();
        },

        isPlaying: function () {
            return this.getFlashElement().mp_getIsPlaying();
        },

        getPlayheadPosition: function () {
            return this.getFlashProperty("playheadPosition");
        },

        getFlashProperty: function (propertyName) {
            return this.getFlashElement().mp_getPropertyValue(propertyName);
        },

        setFlashProperty: function (propertyName, propertyValue) {
            return this.getFlashElement().mp_setPropertyValue(propertyName, propertyValue);
        },

        //occurs when the flash player raises some external event
        flash_eventDispatched: function (params) {
            var eventType = params[0]; //first param is always present
            if (eventType == "interfaceReady" && $.isDefined(this.options.params))
                this.loadState(this.options.params);

            var handlerFunctionName = "on" + eventType.substr(0, 1).toUpperCase() + eventType.substr(1); //the eventType is someing like 'playCompleted'.  To avoid any conflicts with property names in the options, we add 'on' to create a function name like onPlayCompleted
            if (params.length === 1)
                this.$this.trigger(handlerFunctionName);
            else 
                this.$this.trigger(handlerFunctionName, params.slice(1));
        }
    };


    $.madPlayer = function (options) {
        return new madPlayer(options);
    };


    // jQuery plugin implementation
    jQuery.fn.madPlayer = function (options) {

        this.each(function (i) {
            plugin = new madPlayer($.extend({}, options, { id: $(this).attr("id") }));
        });

        return (options && options.api) ? plugin : this;
    };

})(jQuery); 