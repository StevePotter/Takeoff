/// <reference path="jquery-1.6.2-vsdoc.js" />
/// <reference path="swfobject.debug.js" />
/// <reference path="takeoff-helpers.js" />
/// <reference path="takeoff-tooltip.js" />
/// <reference path="takeoff-tabs.js" />
/// <reference path="ProductionView.js" />

/**
* Copyright (c) 2009 Mediascend, Inc.
* http://www.mediascend.com
*/

//*********************** Settings ********************

var settingsForm;

function initSettings() {


    var tabBtn = $("#settings-tab"), initialized = false;

    tabBtn.bind("tabOpening", function (e, args) {
        if (!initialized) {
            //            $("#settingsArea").madTabs({
            //                tabSelector: "a.pill",
            //                paneSelector: "#settingsTabPanes > div",
            //                openTabClass: "selected",
            //                tabClosing: function (e, args) {
            //                    //if there was a validation error and then they go to another tab within settings, the error message would remain.  this avoids that odd situation
            //                    if (settingsForm && settingsForm.is(":visible")) {
            //                        settingsForm.resetValidation();
            //                    }
            //                }
            //            });

            initSettingsForm();

            initialized = true;
        }

        $("#settingsFormTitle").val(productionData.Title);
    });

    tabBtn.bind("tabClosing", function (e, args) {
        //if there was a validation error and then they close the settings tab, the error message would remain.  this avoids that
        if (settingsForm) {
            settingsForm.resetValidation();
        }
    });

}


function initSettingsForm() {
    var settingsForm = $("#settingsForm").preventDoubleSubmit();
    if (!settingsForm.hasElements()) {
        settingsForm = undefined;
        return;
    }

    $('#QuickAccessDecryptedPassword').bind('hastext', function () {
        $('#SemiAnonymousUsersCanComment-Container').css("visibility", "visible");
    });

    $('#QuickAccessDecryptedPassword').bind('notext', function () {
        $('#SemiAnonymousUsersCanComment-Container').css("visibility", "hidden");
    });

    if ($.hasChars($('#QuickAccessDecryptedPassword').val())) {
        $('#SemiAnonymousUsersCanComment-Container').css("visibility", "visible");
    }


    var customUrlRegex = /^(?:(?![^a-zA-Z0-9_-]).)*$/i;
    $('#CustomUrl').keyfilter(customUrlRegex);

    var file = settingsForm.find("input[type='file']");
    var hasLogo = $.exists(productionData.Logo);
    if (hasLogo) {
        var deleteBtn = $('<div class="input"><a class="btn error" href="#">Delete Current Logo</a></div>').click(function (e) {
            var msg;
            if (productionData.HasSpecificLogo && productionData.HasStandardLogo) {
                msg = "The current logo is for this production specifically.  But there is a standard logo that will take its place.  So don't be surprised when you see another logo after this.  Cool?";
            }
            else if (productionData.HasSpecificLogo) {
                msg = "Are you sure you want to delete this logo?  It won't affect any other productions.";
            }
            else if (productionData.HasStandardLogo) {
                msg = "This logo is a standard logo that is used by any other productions that don't have their own specific logo.  So this might also remove the logo for other productions.  Is that okay?";
            }
            if (confirm(msg)) {
                $.madAjax({
                    url: window.urlPrefixes.relative + 'Productions/' + productionData.Id + '/Delete?what=logo',
                    success: function (result) {
                        $.goTo(window.urlPrefixes.relative + 'Productions/' + productionData.Id);
                    }
                });
            }
        });
        file.after(deleteBtn);
    }

    settingsForm.madValidate({
        useAjax: false,
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
            }
    });

    //            submitHandler: function (form) {
    //            $(form).ajaxSubmit({
    //                iframe: "true",
    //                success: function (response) {
    //                    alert($(response).val());
    ////                    var msg = $.parseJSON($(response).val());
    ////                    if (msg.status == "valid") {
    ////                        fadeInPageMessage("success", msg.message);
    ////                    } else if (msg.status == "invalid") {
    ////                        fadeInPageMessage("error", msg.message);
    ////                    }
    //                }

    //            });
    //        },

    //    //HasStandardLogo
    //    //no logo at all
    //    if (!productionData.HasSpecificLogo && !productionData.HasStandardLogo) {
    //        file.after('This logo is for <input type="radio" name="scope" value="specific" checked="checked" /><label>this production</label><input type="radio" name="scope" value="specific" /><label>any production without its own logo</label>');
    //    }
    //    if (productionData.HasSpecificLogo && !productionData.HasStandardLogo) {
    //        file.after('This logo is for <input type="radio" name="scope" value="specific" checked="checked" /><label>this production</label><input type="radio" name="scope" value="specific" /><label>any production without its own logo</label>');
    //    }
    //    if (productionData.HasSpecificLogo && productionData.HasStandardLogo) {
    //        file.after('<a href="#">Use the standard logo instead</a>');
    ////        file.after('This logo is for <input type="radio" name="scope" value="specific" checked="checked" /><label>this production</label><input type="radio" name="scope" value="specific" /><label>any production without its own logo</label>');
    //    }

    //    form.ajaxForm({
    //        dataType: "json",
    //        success: function (a, b) {
    //            alert(5);
    //        }
    //    });
    //    form.ajaxForm(function (e) {
    //        $.madAjax({
    //            async: false,
    //            url: form.attr("action"), //(window.urlPrefixes.relative + "ProductionSettings/AllocateLogo"),
    //            data: function () {
    //                return {
    //                    productionId: productionData.Id,
    //                    fileName: form.find("input[type='file']").val()
    //                };
    //            },
    //            success: function (result) {
    //                form.find("input[type='hidden']").remove();
    //                form.attr("action", result.Url);
    //                $.each(result.Variables, function (key, value) {
    //                    form.prepend('<input type="hidden" name="' + key + '" value="' + value + '" />');
    //                });
    //            },
    //            error: function (result) {
    //                e.preventDefault();
    //            }
    //        });
    //    });
}

//updates the project's title
function updateTitle() {
    var title = productionData.Title;
    localNav.find("h2:first").text(title);
}

function updateLogo() {
    localNav.find("img").remove(); //remove any existing logo
    var logo = productionData.Logo;
    if ($.exists(logo)) {
        localNav.addClass("logo");
        localNav.prepend('<img src="' + logo.Url + '" alt="Logo" style="width:' + logo.Width.toFixed(0) + 'px;height:' + logo.Height.toFixed(0) + 'px;" />');
    }
}

