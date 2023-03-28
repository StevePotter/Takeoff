/// <reference path="jquery-1.6.2-vsdoc.js" />
/// <reference path="swfobject.debug.js" />
/// <reference path="takeoff-helpers.js" />
/// <reference path="mediascend.upload.js" />
/// <reference path="takeoff-tooltip.js" />
/// <reference path="takeoff-tabs.js" />
/// <reference path="ProductionView.js" />

/**
* Copyright (c) 2009 Mediascend, Inc.
* http://www.mediascend.com

this might be a better autocomplete plugin http://docs.jquery.com/Plugins/Autocomplete/autocomplete#url_or_dataoptions

*/

//*********************** Members ********************

//dataTable - plugin for the Files table
var membersUi =
{
    //when true, the autocomplete input plugin for new members has been initialized and ready to rock
    autoCompleteInitialized: false,
    //when true, the autocomplete's source data will be updated when the form is opened or when a search in the autocomplete begins...whichever comes first
    updateAutoCompleteNext: false
};

function initMembers() {
    //create the datatable plugin and fill in with initial data
    membersUi.dataTable = $("#membersTable").dataTable({
        bJQueryUI: false,
        "bPaginate": false,
        "bLengthChange": false,
        "bFilter": false,
        "bSort": true,
        "bInfo": false,
        "bAutoWidth": false,
        aoColumns: [
            { sType: "string", bUseRendered: false, bSortable: false, sWidth: "300px" }, //user name/email link
            {sType: "string", bSearchable: false, bSortable: false }, //status
            {sType: "string", bSearchable: false, bSortable: false}//actions
            ],
        fnRowCallback: function (nRow, aData, iDisplayIndex) {
            var $row = $(nRow);
            var tds = $('td', $row);
            var data = $row.data("data");

            if ($.hasChars(data.Email)) {
                tds.eq(0).html('<a href="mailto:' + encodeURIComponent(data.Email) + '">' + $.htmlEncode(data.Name) + '</a>');
            }
            else {
                tds.eq(0).text(data.Name);
            }
            var statusCell = tds.eq(1);
            var actionsCell = tds.eq(2);
            actionsCell.empty();

            var isInvitation = data.IsInvitation;
            if ($.isDefined(isInvitation)) {
                if (true === isInvitation) {
                    statusCell.html('Pending invite');
                    if (data.CanDelete) {
                        var deleteBtn = $('<a href="#">Uninvite</a>').click(function () {
                            var message = "Are you sure you want to cancel this invitation?";
                            if (confirm(message)) {
                                $.madAjax({
                                    url: (window.urlPrefixes.relative + "MembershipRequests/Cancel"),
                                    data: function () {
                                        return {
                                            id: data.Id
                                        };
                                    },
                                    success: function (result) {
                                        deleteMemberFromUi(data.Id);
                                    }
                                });
                            }
                            return false;
                        });
                        actionsCell.append(deleteBtn);
                        actionsCell.append("&nbsp;");

                        var resendBtn = $('<a href="#">Resend</a>').click(function () {
                            $.madAjax({
                                url: (window.urlPrefixes.relative + "MembershipRequests/Resend"),
                                data: function () {
                                    return {
                                        id: data.Id
                                    };
                                },
                                success: function (result) {
                                    notification.success("The invitation has been emailed.");
                                }
                            });
                            return false;
                        });
                        actionsCell.append(resendBtn);

                    }
                }
                else {
                    statusCell.html('Pending approval');
                    if (data.CanDelete) {
                        actionsCell.html('<a href="' + window.urlPrefixes.relative + 'MembershipRequests/' + data.Id + '">Accept or Ignore</a>');
                    }
                }
            }
            else {
                var createdOn = $('<em class="relativeDate"></em>').text(toRelativeString(data.CreatedOn)).data("date", data.CreatedOn).data("config", relativeDateConfig.sentence);
                statusCell.html('Joined ').append(createdOn);

                if (data.CanDelete) {
                    var deleteBtn = $('<a href="#">Kick out</a>').click(function () {
                        var message = "Are you sure you want to remove '" + data.Name + "' from this production?";
                        if (confirm(message)) {
                            $.madAjax({
                                url: (window.urlPrefixes.relative + "ProductionMembers/Delete"),
                                data: function () {
                                    return {
                                        id: data.Id
                                    };
                                },
                                success: function (result) {
                                    deleteMemberFromUi(data.Id);
                                }
                            });
                        }
                        return false;
                    });
                    actionsCell.append(deleteBtn);
                }
            }

            return nRow;
        }
    });

    var memberData = [];
    if ($.isArrayWithItems(productionData.Members)) {
        $.each(productionData.Members, function () {
            memberData.push(this);
        });
    }
    if ($.isArrayWithItems(productionData.MembershipRequests)) {
        $.each(productionData.MembershipRequests, function () {
            memberData.push(this);
        });
    }
    loadMembersIntoTable(memberData);

    var form = $("#member-add-form");
    if (form.hasElements()) {
        membersUi.form = form;
        _initMemberAdd();
    }
}

//handles crap for adding a new member
function _initMemberAdd() {
    var form = membersUi.form;
    monkeyPatchAutocomplete();
    //below is stuff for adding a new user
    var input = membersUi.input = $("#newMemberNameEmail"), userFromAutoComplete, validator; //the autocomplete-enabled text box for the new user
    var autocompleteOpts =
    {
        minLength: 0,
        delay: 0,
        search: function () {
            if (membersUi.updateAutoCompleteNext) {
                $.madAjax({
                    url: (window.urlPrefixes.relative + "ProductionMembers/GetPotentials"),
                    data: function () {
                        return {
                            productionId: productionData.Id
                        };
                    },
                    async: false,
                    success: function (result) {
                        $.each(result, function () {
                            this.label = this.Email + " " + this.Name;
                            this.value = this.Name;
                        });
                        membersUi.input.autocomplete("option", "source", result);
                    }
                });
                membersUi.updateAutoCompleteNext = false;
            }
        },
        select: function (event, ui) {
            userFromAutoComplete = ui.item;
            $("#newUserRole").hide();
            form.resetValidation(false); //redo validation so if "invalid email" is there, it will be removed
        }
    };

    var resetNewMemberForm = function () {
        userFromAutoComplete = null;
        form.find("input[type='radio']:first").attr("checked", "checked");
        $("#newUserRole").hide();
        form.resetValidation(true);
    };

    //cancel button - note the stopImmediatePropagation, which is necessary on all Cancel buttons in validated forms.  avoids http://dev.jquery.com/ticket/6473.  make sure this goes above the validate() for the form.
    form.find("a.cancelBtn").click(function (e) {
        e.stopImmediatePropagation();
        resetNewMemberForm();
        form.hide();
        $("#newMemberBtn").removeClass("disabled").fadeTo("normal", 1);
        return false;
    });

    $("#newMemberBtn").click(newMember_click);

    //whether the user is entering a brand new user that wasn't taken from the autocomplete.  will return false if no chars are entered
    var isEnteringNewUser = function () {
        if ($.exists(userFromAutoComplete))
            return false;
        var valEntered = input.val();
        return $.hasChars(valEntered);
    };

    input.bind("change keyup", function () {
        //if they selected a person from the dropdown but then edited the entry, clear userFromAutoComplete
        if ($.exists(userFromAutoComplete) && userFromAutoComplete.value !== input.val()) {
            userFromAutoComplete = null;
        }

        //if they are entering a new user and have provided a valid email, show the Role option
        if (!$.exists(userFromAutoComplete) && $.isValidEmail(input.val())) {
            $("#newUserRole").show();
            form.resetValidation(false); //clear the invalid message, as it would normally stay until the user submits the form again.  but it's valid and we don't want to confuse them    
        }
        else {
            $("#newUserRole").hide();
        }
    });

    validator = form.madValidate(
    {
        onfocusout: false,
        onkeyup: false,
        rules:
        {
            newMemberNameEmail:
            {
                required: true,
                email: {//if they selected a user from autocomplete, then their name will be there.  otherwise we want an email
                    depends: function () {
                        return isEnteringNewUser();
                    }
                }
            }

        },
        data: function () {
            return {
                productionId: productionData.Id,
                emailOrUserId: isEnteringNewUser() ? input.val() : userFromAutoComplete.Id,
                role: form.find("input[type='radio']:checked").val(),
                note: $("#newMemberNoteInput").val()
            };
        },
        submitSuccess: function (result) {
            var resultType = $.isDefined(result.Type) ? result.Type : result;
            //the user came from autocomplete so remove it from the autocomplete source
            if ($.exists(userFromAutoComplete)) {
                if (membersUi.autoCompleteInitialized) {
                    var source = input.autocomplete("option", "source");
                    source = $.grep(source, function (option) { return option != userFromAutoComplete; });
                    input.autocomplete("option", "source", source);
                }
            }

            if (resultType === "AlreadyMember") {
                notification.error("Sorry, but that person is already a member.  If you cannot see them below, please hit the Refresh button on your browser.");
                resetNewMemberForm();
            }
            else if (resultType === "AlreadyInvited") {
                notification.error("Sorry, but that person is already invited.  If you cannot see them below, please hit the Refresh button on your browser.");
                resetNewMemberForm();
            }
            else {
                if (resultType === "Invited") {
                    loadMembersIntoTable([result.Request]);
                    notification.success("An invitation has been sent.");
                }
                else if (resultType === "Added") {
                    loadMembersIntoTable([result.Membership]);
                    notification.success("Success!");
                }
                else if (resultType === "Rejected") {
                    notification.error("Sorry but this person isn't accepting invitations.");
                }
                else {
                    throw "Invalid success result.";
                }
                resetNewMemberForm();
            }
        }
    });

    var initAutoComplete = function () {
        if (!membersUi.autoCompleteInitialized) {
            membersUi.autoCompleteInitialized = true;

            $.madAjax({
                url: (window.urlPrefixes.relative + "ProductionMembers/GetPotentials"),
                data: function () {
                    return {
                        productionId: productionData.Id
                    };
                },
                success: function (result) {
                    $.each(result, function () {
                        this.label = this.Email + " " + this.Name;
                        this.value = this.Name;
                    });
                    autocompleteOpts.source = result;
                    input.autocomplete(autocompleteOpts);
                    input.click(function () {
                        //when they initially click on it, show all possible members.  in the future you may want to provide some kind of threshold 
                        if (!$.exists(userFromAutoComplete) && !$.hasChars(input.val())) {
                            input.autocomplete("search", "");
                        }
                    });
                }
            });
        }
    };


    //when the user closes the tab, reset validation to hide any validation errors
    $("#members-tab").bind("tabClosing", function (e, args) {
        form.resetValidation(true);
    }).bind("tabOpening", function () {
        initAutoComplete();
    });
}

function newMember_click(e) {
    var button = $("#newMemberBtn");
    if (button.hasClass("disabled"))
        return false;

    var form = $("#member-add-form");
    form.slideDown("normal", function () {
        $("#newMemberNameEmail").focus();
    });
    button.addClass("disabled").fadeTo("normal", 0.2);
    return false;
}

//adds one or many members into the Team table.  Data should come from MembershipThingView.
function loadMembersIntoTable(membersData, clearTable) {
    if (!$.isArray(membersData)) {
        membersData = [membersData];
    }

    var members = [];
    $.each(membersData, function () {
        members.push([this.Name, this, this]);
    });
    if (clearTable)
        membersUi.dataTable.fnClearTable();

    if (members.length > 0) {
        var aoDataIndices = membersUi.dataTable.fnAddData(members, false);
        var adData = membersUi.dataTable.fnSettings().aoData;
        $.each(aoDataIndices, function (i) {
            $(adData[this].nTr).data("data", membersData[i]);
        });
        membersUi.dataTable.fnDraw();
    }
    checkForEmptyMembersTable();
    membersUi.updateAutoCompleteNext = true;

}

function checkForEmptyMembersTable() {
    if (isMembersEmpty()) {//fnGetNodes worked after filling the table but not after fnDeleteRow, which resulted in the table not hiding once the last row was deleted.  so i had to do this shitty jquery garbage
        $("#membersTable").parent().hide();
        $("#membersTableEmptyMsg").show();
    }
    else {
        $("#membersTable").parent().show();
        $("#membersTableEmptyMsg").hide();
    }
}

function isMembersEmpty() {
    return $("#membersTable tbody tr:first td.dataTables_empty").hasElements();
}

//determines whether a member with the given id is in the data table
function isMemberShown(membershipId) {
    if (isMembersEmpty())
        return false;
    var found = false;
    $("#membersTable tbody tr").eachJ(function () {
        var dataObj = this.data("data");
        if (dataObj.Id === membershipId) {
            found = true;
            return false;
        }
    });
    return found;
}

function deleteMemberFromUi(membershipId) {
    deleteActivityItem(membershipId);

    if (isMembersEmpty())
        return;

    //fnGetNodes and fnGetData are weird because they include deleted rows.  i have no idea why...the explanation is odd and can be found in the source code
    $("#membersTable tbody tr").each(function () {
        var dataObj = $(this).data("data");
        if (dataObj.Id === membershipId) {
            membersUi.dataTable.fnDeleteRow(this);
            return false;
        }
    });
    checkForEmptyMembersTable();
    membersUi.updateAutoCompleteNext = true;
}

//http://stackoverflow.com/questions/2435964/jqueryui-how-can-i-custom-format-the-autocomplete-plug-in-results
//lets me customize display of autocomplete items.  also avoids a bug where a value with html would break shit
function monkeyPatchAutocomplete() {
    $.ui.autocomplete.prototype._renderItem = function (ul, item) {
        var text = item.Name;
        if (item.Name !== item.Email) {
            text += "  <" + item.Email + ">";
        }
        return $("<li></li>")
              .data("item.autocomplete", item)
              .append($("<a></a>").text(text))
              .appendTo(ul);
    };
}
