if ($.validator) {
    $.extend($.validator.defaults || {}, {
        errorClass: 'invalid'
    });
}

primaryButtonCssClass = 'btn btn-primary';
primaryButtonCssClass = 'btn primary';

$(document).ready(function() {
    $("span.tooltip-anchor").tooltip(); 
});

//creates the element needed for a simple modal with heading, body and yes/no buttons
$.createYesNoModal = function (options) {
    var modal = $('<div class="modal">' +
            '<div class="modal-header">' +
            '<a class="close" data-dismiss="modal">×</a>' +
            '<h3></h3>' +
            '</div>' +
            '<div class="modal-body">' +
            '<p></p>' +
            '</div>' +
            '<div class="modal-footer">' +
            '<a href="#" class="modal-btn-yes ' + primaryButtonCssClass + '"></a>' +
            '<a href="#" data-dismiss="modal" class="modal-btn-no btn"></a>' +
            '</div>' +
            '</div>');
    modal.find("h3").text(options.heading);
    modal.find("p").text(options.body);
    modal.find("a.modal-btn-yes").attr("href", options.yesUrl).text(options.yesText);
    modal.find("a.modal-btn-no").text(options.noText);
    return modal;
};

$.createOkModal = function (options) {
    var modal = $('<div class="modal">' +
            '<div class="modal-header">' +
            '<a class="close" data-dismiss="modal">×</a>' +
            '<h3></h3>' +
            '</div>' +
            '<div class="modal-body">' +
            '<p></p>' +
            '</div>' +
            '<div class="modal-footer">' +
            '<a href="#" data-dismiss="modal" class="modal-btn-ok ' + primaryButtonCssClass + '"></a>' +
            '</div>' +
            '</div>');
    modal.find("h3").text(options.heading);
    modal.find("p").text(options.body);
    modal.find("a.modal-btn-ok").text(options.okText);
    return modal;
};


$.madAjaxOptions.abortedDefault = function (result) {
    //if the request was aborted, they probably lost internet connectivity or the server is down.  in this case we put up a modal and hide it if another successful request goes through.  
    var modalDataKey = "no-connectivity-modal";
    if (!$.exists($(document).data(modalDataKey))) {
        var hideModal = function (e, args) { //if an ajax function executes, we can reset the timer because it will update the sliding expiration on the login.  todo: once you start doing ajax requests outside the domain, you need to check and ignore non-native domains
            $(document).data(modalDataKey).modal('hide');
        };

        var modal = $.createOkModal({
            heading: "Connection Error",
            body: "We couldn't communicate with Takeoff servers.  Most likely your internet connection isn't working or our servers are down.",
            okText: "OK"
        }).modal({
            show: true,
            backdrop: true,
            keyboard: true
        }).on('hidden', function () {
            $(document).unbind("ajaxSuccess.madAjax", hideModal);
            $(document).removeData(modalDataKey);
        });
        $(document).data(modalDataKey, modal);
        $(document).bind("ajaxSuccess.madAjax", hideModal);

    }
    return true;
};

$.madAjaxOptions.forbiddenDefault = function (result) {
    if (!result || !result.ErrorCode)
        return;

    if (result.ErrorCode === "BasicTrialInformationRequired") {
        var signupUrl = result.ResolveUrl;
        signupUrl += signupUrl.indexOf("?") > 0 ? "&" : "?";
        signupUrl += "postSignupUrl=" + encodeURIComponent(top.location.href);

        var modal = $.createYesNoModal({
            heading: "Signup Required",
            body: "In order to do this, we need to know who you are.  For that, we need you to sign up for Takeoff.  No worries, it's free and takes just a few seconds.",
            yesUrl: signupUrl,
            yesText: "Sign Up",
            noText: "Maybe Later"
        }).modal({
            show: true,
            backdrop: true,
            keyboard: true
        });
        return true;
    }

    if (result.ErrorCode === "UnverifiedEmail") {
        var signupUrl = result.ResolveUrl;
        var modal = $.createYesNoModal({
            heading: "Email Validation",
            body: "Before you can do this, we need to verify that you indeed own the email address you signed up with. Verifying is quickly done by clicking a link in the email we sent when you signed up. If you can't find it, check your junk mail, and if it's not there, we'll send you another.",
            yesUrl: result.ResolveUrl,
            yesText: "OK",
            noText: "Cancel"
        }).modal({
            show: true,
            backdrop: true,
            keyboard: true
        });
        return true;
    }

};

//setup for a system of executing javascript functions for specific pages.
window.views = {};



//todo: 
//Takeoff = window.Takeoff = { };
//$.extend(Takeoff, {
//    
//});