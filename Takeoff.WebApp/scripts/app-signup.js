/// <reference path="jquery-1.4.4-vsdoc.js" />
/// <reference path="takeoff-helpers.js" />
/// <reference path="takeoff-forms.js" />
/// <reference path="takeoff-tabs.js" />

// /Signup/Account
$.pageAccount = function () {
    $(document).madTooltips(); //initialize the tooltips
    $("#TimezoneOffset").val(new Date().getTimezoneOffset());

    //the dropdown will not be/ included in the html if we already have their signupsource
    var signupSourceDropdown = $("#SignupSource").change(function () {
        if (signupSourceDropdown.val() === "Other") {
            $("#SignupSourceOther-container").show();
            $("#SignupSourceOther").focus();
        }
        else {
            $("#SignupSourceOther-container").hide();
        }
    }).keypress(function(e){
        if(e.which == 13 && signupSourceDropdown.val() !== "Other"){
            form.submit();
            return false;
        }
    });

    var form = $("#mainForm");
    form.madValidate({
        preventDoubleSubmit: true,
        rules:
        {
            "FirstName": "required",
            "LastName": "required",
            "Email": "required",
            "Password": "required",
            "agreeToTerms": "required",
            "passwordConfirm": {
                required: true,
                equalTo: "#Password"
            },
            "SignupSource": {
                required: {
                    depends: function () {
                        return signupSourceDropdown.hasElements();
                    }
                }
            }
        },
        messages: {
             "SignupSource": "Please let us know how you found us.  It would mean so much :)",
             "agreeToTerms": "Please agree to our agreements :)"
         },
        useAjax: false
    });



};

// /Signup/Subscribe
$.page_subscribe = function () {
    $("#subscribe-form").madValidate({ 
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
};


// /Signup/Guest
$.page_signup_guest = function () {
    $(document).madTooltips(); //initialize the tooltips
    $("#TimezoneOffset").val(new Date().getTimezoneOffset());
    $("#mainForm").madValidate({
        preventDoubleSubmit: true,
        rules:
        {
            "FirstName": "required",
            "LastName": "required",
            "Email": "required",
            "Password": "required",
            "agreeToTerms": "required",
            "passwordConfirm": {
                required: true,
                equalTo: "#Password"
            }
        },
        messages: {
             "agreeToTerms": "Please agree to our agreements :)"
         },
        useAjax: false
    });

};
