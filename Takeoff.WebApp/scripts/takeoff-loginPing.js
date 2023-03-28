/// <reference path="jquery-1.6.2-vsdoc.js" />
/// <reference path="takeoff-helpers.js" />
/// <reference path="takeoff-forms.js" />
/// <reference path="takeoff-helpers.js" />

/*
* Mediascend login pinger
* version: 1.0
* Keeps users logged into our system during periods when they don't contact the server (such as uploading a massive file or eating lunch)
*
* Copyright (c) 2009 Mediascend, Inc.
* 
* Requires: jQuery v1.3+
* 
*/



jQuery.loginPing = function() {
    var minutes = 10; //about half the time that normal cookie will expire
    var interval = 60000 * minutes; //milliseconds
    var intervalId;

    var ping = function() {
        $.madAjax({
            url: (window.urlPrefixes.relativeHttps + "Account/IsUserLoggedIn"),
            data: {},
            error: function() {
                
            },
            success: function(result) {
                if (!result) {
                    var email = window.user.email;
                    var emailStr = $.emptyString(email) ? "" : "email=" + encodeURIComponent(email) + "&";
                    $.goTo(window.urlPrefixes.relativeHttps + "login?" + emailStr + "heading=" + encodeURIComponent("You were inactive for a while so we need you to log back in.") + "&returnUrl=" + encodeURIComponent(top.location.href));
                    window.clearInterval(intervalId); //we redirect so no need to 
                }
            }
        });
    };

    $(document).ajaxComplete(function() {//if an ajax function executes, we can reset the timer because it will update the sliding expiration on the login.  todo: once you start doing ajax requests outside the domain, you need to check and ignore non-native domains
        if ($.isNumber(intervalId)) {
            window.clearInterval(intervalId);
        }
        intervalId = window.setInterval(ping, interval);
    });
    
    intervalId = window.setInterval(ping, interval);
};