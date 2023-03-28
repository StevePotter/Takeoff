/// <reference path="jquery-1.6.2-vsdoc.js" />
/// <reference path="takeoff-helpers.js" />

//setup for a system of executing javascript functions for specific pages.

$.extend(window.views, {
    Productions_Create: function () {
        var customUrlRegex = /^(?:(?![^a-zA-Z0-9_-]).)*$/i;
        $('#CustomUrl').keyfilter(customUrlRegex);

        $('#SemiAnonymousDecryptedPassword').bind('hastext', function () {
            $('#SemiAnonymousUsersCanComment-Container').css("visibility", "visible");
        });

        $('#SemiAnonymousDecryptedPassword').bind('notext', function () {
            $('#SemiAnonymousUsersCanComment-Container').css("visibility", "hidden");
        });

        if ($.hasChars($('#SemiAnonymousDecryptedPassword').val())) {
            $('#SemiAnonymousUsersCanComment-Container').css("visibility", "visible");            
        }

        $("#main-content form").madValidate({
            rules:
            {
                Title: "required",
                CustomUrl: {
                    required: false,
                    regex: customUrlRegex
                }
            },
            messages:
            {
                CustomUrl: "Only letters, numbers, dashes (-), and underscores (_) are allowed."
            },
            useAjax: false
        });

        $("#main-content form div.buttons a").click(function () {
            history.go(-1);
            return false;
        });
    }
});