/// <reference path="jquery-1.6.2-vsdoc.js" />
/// <reference path="takeoff-helpers.js" />
/// <reference path="takeoff-forms.js" />

/*
* Takeoff login page
*
* Copyright (c) 2010 Mediascend, Inc.
* 
*/

$.extend(window.views, {
    login: function(options) {
        options = $.extend({ useAjax: false }, options);
        $("#TimezoneOffset").val(new Date().getTimezoneOffset());

        $("#forgotPasswordBtn").click(function(e) {
            $(this).attr("href", $(this).attr("href") + "?email=steve");
        });

        $("#loginForm").madValidate({
            rules:
                {
                    "Email": "required",
                    "Password": "required"
                },
            useAjax: false// options.useAjax
        });

        $("#production-password-form").madValidate(
            {
                rules:
                    {
                        "Password": "required"
                    },
                useAjax: false// options.useAjax            
            }
        );
    }
});
