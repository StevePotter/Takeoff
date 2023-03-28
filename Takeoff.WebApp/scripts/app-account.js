/// <reference path="jquery-1.6.2-vsdoc.js" />
/// <reference path="takeoff-helpers.js" />

$.extend(window.views, {
    //http://localhost:64853/account/renderview?action=NecessaryInfo
    Account_NecessaryInfo: function () {
        $("#TimezoneOffset").val(new Date().getTimezoneOffset());

        //the dropdown will not be included in the html if we already have their signupsource
        var signupSourceDropdown = $("#SignupSource").change(function () {
            if (signupSourceDropdown.val() === "Other") {
                $("#SignupSourceOther-container").show();
                $("#SignupSourceOther").focus();
            }
            else {
                $("#SignupSourceOther-container").hide();
            }
        });

        //some fields might not be included, but validation plugin ignores them so it's cool.
        $("#main-content form").madValidate({
            rules:
            {
                "FirstName": "required",
                "LastName": "required",
                "Password": {
                    required: true
                },
                passwordConfirm: {
                    required: true,
                    equalTo: "#Password"
                },
                SignupSource: {
                    required: {
                        depends: function () {
                            return signupSourceDropdown.hasElements();
                        }
                    }
                }
            },
            useAjax: false
        });
    },

    // /account/billinginfo
    Account_MainInfo: function () {
        $("#main-content form").madValidate({
            rules:
            {
                FirstName: "required",
                LastName: "required",
                Email: {
                    required: true,
                    email: true
                },
                CurrentPassword: "required"
            },
            useAjax: false
        });
        $("#CurrentPassword").val(''); //sometimes browsers remember passwords so this will clear it
        $(document).madTooltips(); //initialize the tooltips

    },

    // /account/notifications
    Account_Notifications: function () {
        $("#main-content form").submit(function () {
            $(this).ajaxSubmit({ 
                    success: function (result) {
                         $("body").madNotification({ api: true }).success("Your settings have been updated.");
                    } 
            }); 
            return false;
        });

    },

    // /account/privacy
    Account_Privacy: function () {
        $('#mainForm').submit(function () { 
            $(this).ajaxSubmit({
                success: function (result) {
                    $("body").madNotification({ api: true }).success("Your settings have been updated.");
                } 
            });
            return false;
         }); 
    },

        // /account/billinginfo
    Account_BillingInfo: function () {
        $("#mainForm").madValidate({
            rules: {
                "FirstName": "required",
                "LastName": "required",
                "PostalCode": "required",
                "CreditCardNumber": {
                    required: true,
                    creditcard: true
                },
                "CreditCardVerificationCode": {
                    required: true,
                    digits: true
                }
            },
            useAjax: false
        });
    },

    //account/logo
    Account_Logo: function () {
        $("#logo-form").madValidate({
            rules:
            {
                Logo: "required"
            },
            useAjax: false
        });
    }
});