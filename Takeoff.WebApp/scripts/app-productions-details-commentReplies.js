/// <reference path="jquery-1.6.2-vsdoc.js" />
/// <reference path="swfobject.debug.js" />
/// <reference path="takeoff-helpers.js" />
/// <reference path="mediascend.upload.js" />
/// <reference path="takeoff-tooltip.js" />
/// <reference path="takeoff-tabs.js" />
/// <reference path="ProductionView.js" />
/// <reference path="jquery.domec-1.1.0.js" />


//*********************** DOM & Views ********************

function isReplyInUi(id) {
    return getReplyView(id).hasElements();
}

function getReplyView(id) {
    return $("#reply-" + id.toFixed(0));
}

//creates the HTML view for the given reply and inserts it properly into the dom
function addReplyToUi(data) {
    var commentView = getCommentView(data.ParentId);
    var replies = commentView.find("div.replies");
    var replyView = createReplyView(data);
    replies.append(replyView);
    replies.show(); //if this is the first reply, the comment's div.replies will be hidden
    return replyView;
}

//removes the comment from the UI
function deleteReplyFromUi(id) {
    var view = getReplyView(id);
    if (view.hasElements()) {
        var replies = view.parent();
        view.remove();
        hideRepliesContainerIfEmpty(replies);
    }
    deleteActivityItem(id);
}

//creates DOM element for a comment reply.  hooks up data and events as well.
function createReplyView(data) {
    var view = $('<blockquote class="reply" id="reply-' + data.Id + '"></blockquote>').data("data", data);

    //this hides the parent commment's Actions while the mouse is over this reply
    var parentCommentActions = $.lazy(function () {
        return view.parents(".comment-thread").find("> blockquote.comment-container span.commentAction");
    });

    var body = $('<p class="body"></p>').text(data.Body).appendTo(view);

    var author;
    if (true === data.IsCreator) {
        author = $("<span>You</span>");
    }
    else if ($.exists(data.Creator)) {
        if ($.hasChars(data.Creator.Email)) {
            author = $("<a></a>").attr("href", "mailto:" + data.Creator.Email).text(data.Creator.Name);
        }
        else {
            author = $("<span></span>").text(data.Creator.Name);
        }
    }
    else {
        author = $("<span></span>").text("Author Unknown");
    }

    var createdTooltip = moment(data.CreatedOn).format('LLL');
    var createdOn = $('<em class="relativeDate"></em>').text(toRelativeString(data.CreatedOn)).attr("title", createdTooltip).data("date", data.CreatedOn);

    var info = $('<small class="footer"></small>').append(author).append(" &bull; ").append(createdOn).appendTo(view);

    var actions = $('<span class="actions"></span>').appendTo(info);

    if (data.CanEdit) {
        $('<a href="#" class="smallEdit" title="Edit this reply">Edit</a>').click(function () {
            var closeForm = function () {
                body.empty().text(data.Body);
            };
            var form = $('<form class="reply-edit"></form>');
            var textInput = $('<textarea type="text" name="body"></textarea>').val(data.Body).appendTo(form);
            var buttons = $('<div class="buttons"></div>').appendTo(form);
            $('<input type="submit" value="Update" class="btn btn-primary" />').appendTo(buttons);
            $('<a class="cancelBtn btn" href="#">Cancel</a>').click(function () {
                closeForm();
                return false;
            }).appendTo(buttons);
            body.empty().append(form);
            textInput.focus(); //must initialize after adding to DOM or else plugin will throw errors

            form.madValidate({
                url: function () {
                    return (window.urlPrefixes.relative + "replies/" + data.Id + "/edit");
                },
                onfocusout: false,
                onkeyup: false,
                rules:
                {
                    body: "required"
                },
                data: function () {
                    return {
                        id: data.Id,
                        body: textInput.val()
                    };
                },
                submitSuccess: function (result) {
                    data.Body = textInput.val();
                    closeForm();
                }
            });

            return false;
        }).appendTo(actions);
    }
    if (data.CanDelete) {
        $('<a href="#" class="smallDelete" title="Delete this reply">Delete</a>').click(function () {
            actions.addClass("visible"); //the alert box caused the hover to leave and the actions were hidden.  this made it tough to see which reply you were deleting.  so this fixes that
            var message = "Are you sure you want to delete this reply?  This cannot be undone.";
            if (confirm(message)) {
                $.madAjax({
                    url: function () {
                        return (window.urlPrefixes.relative + 'replies/' + data.Id + '/delete');
                    },
                    data: function () {
                        return {
                            id: data.Id
                        };
                    },
                    error: function () {
                        alert("Uh oh.  There was a problem while deleting your comment.  We'll check it out and get back to you.  In the meantime, we've put your comment back in.");
                        addReplyToUi(data, false);
                    },
                    beforeSend: function (result) {
                        //we delete the reply immediately and restore it if there was a problem.  this makes the UI more responsive
                        deleteReplyFromUi(data.Id);
                    }
                });
            }
            actions.removeClass("visible"); //the alert box caused the hover to leave and the actions were hidden.  this made it tough to see which reply you were deleting.  so this fixes that
            return false;
        }).appendTo(actions);
    }

    return view;
}

//*********************** Input ***********************

//initializes one of the comment reply forms.  There is one form per comment and the forms are created on demand
function createCommentReplyForm(replies, commentId, onFormClosed) {
    var form = $('<form class="reply-add"></form>');
    $('<label for="">In Reply</label>').appendTo(form);
    var textInput = $('<textarea type="text" name="body"></textarea>').appendTo(form);

    var nameInput;
    if (needsToEnterUserNameInCommentForms()) {
        nameInput = $('<div class="field name-container"><label>Your Name</label><input type="text" value="" name="name" /></div>').appendTo(form).find("input");
    }
    var buttons = $('<div class="buttons"></div>').appendTo(form);
    $('<input type="submit" value="Add Reply" class="btn btn-primary" />').appendTo(buttons);

    var closeForm = function () {
        form.hide();
        form.resetValidation(true);
        onFormClosed();
    };

    $('<a class="cancelBtn btn" href="#">Cancel</a>').click(function () {
        closeForm();
        hideRepliesContainerIfEmpty(replies);
        return false;
    }).appendTo(buttons);

    textInput.focus();

    var submittedData;

    form.madValidate({
        url: function () {
            return window.urlPrefixes.relative + "replies";
        },
        rules:
        {
            body: "required",
            'name': {
                required: function () {
                    return needsToEnterUserNameInCommentForms();
                }
            }
        },
        data: function () {
            submittedData = {
                commentId: commentId,
                body: textInput.val()
            };
            if (needsToEnterUserNameInCommentForms()) {
                submittedData["name"] = nameInput.val();
            }
            return submittedData;
        },
        submitSuccess: function (result) {
            if ($.hasChars(submittedData["name"])) {
                productionData.SemiAnonymousUserName = submittedData["name"];
                onSemiAnonymousUserNameChangeSubmitted();
            }
            closeForm();
            var view = addReplyToUi(result.Data, false);
            addActivityItem(result.ActivityPanelItem, false);
            highlightComment(view, false, true);
        }
    });
    return form;
}

function hideRepliesContainerIfEmpty(replies) {
    if (replies.children(".reply:first").length === 0)//no replies, so hide the whole div so the border doesn't show
    {
        replies.hide();
        replies.trigger("mouseleave"); //this was necessary because comments bind to the hover event to make their actions visible again.  when hiding, mouseleave didn't fire, which meant the comment's Actions weren't available
    }
}

function updateCommentReplyBody(commentId, body) {
    var view = getReplyView(commentId);
    if (view.hasElements()) {
        view.find("p.body:first").text(body);
    }
}