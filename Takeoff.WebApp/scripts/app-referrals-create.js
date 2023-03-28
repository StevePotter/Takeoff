/// <reference path="jquery-1.6.2-vsdoc.js" />
/// <reference path="takeoff-helpers.js" />

//setup for a system of executing javascript functions for specific pages.

$.extend(window.views, {
    Referrals_Create: function () {
        var isValidEmail = function (value) {
            return /^((([a-z]|\d|[!#\$%&'\*\+\-\/=\?\^_`{\|}~]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+(\.([a-z]|\d|[!#\$%&'\*\+\-\/=\?\^_`{\|}~]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+)*)|((\x22)((((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(([\x01-\x08\x0b\x0c\x0e-\x1f\x7f]|\x21|[\x23-\x5b]|[\x5d-\x7e]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(\\([\x01-\x09\x0b\x0c\x0d-\x7f]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF]))))*(((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(\x22)))@((([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.)+(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))$/i.test(value);
        };

        $("#people").tokenizer({
            placeholderText: "Enter Email Addresses",
            parseInput: function (text, keyPressed) {
                var tokens = [];
                var delimiters = new RegExp("[,|;| |\\r\\n|\\r|\\n|\\t]+")
                _.each(text.split(delimiters), function (value) {
                    value = $.trim(value);
                    if (value.length > 0)
                        tokens.push({ text: value, value: value, valid: isValidEmail(value) });
                });
                return {
                    tokens: tokens,
                    remainingText: ''
                };
            }
        });

        $("#main-content form").madValidate({
            rules:
            {
                "people": "required"
            },
            ignore: '', //tokenized input box is hidden and by default jquery validator ignores hidden elements
            errorElementAnchors:
            {
                "people": function () {
                    return $("#main-content form div.tokenizer-container");//place next to "fake" input box
                }
            },
            useAjax: false
        });
    }
});