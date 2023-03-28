/// <reference path="jquery-1.6.2-vsdoc.js" />
/// <reference path="swfobject.debug.js" />
/// <reference path="takeoff-helpers.js" />
/// <reference path="mediascend.upload.js" />
/// <reference path="takeoff-tooltip.js" />
/// <reference path="takeoff-tabs.js" />
/// <reference path="ProductionView.js" />
/// <reference path="jquery.domec-1.1.0.js" />

/**
* Copyright (c) 2009 Mediascend, Inc.
* http://www.mediascend.com
*/

var currCommentsTime = NaN; //the second that contains the "current" comments, which are comments in this second or in the previous second that had current comments.
var currCommentIds; //contains element ids (like 'comment1234') (value is true for each) for the "current" comment ids

var commentForm, isCommentFormOpen = false, commentFormIsTimedField, commentAddForm_isTimed_timecode;
var comment_add_form_name_container; //for semi-anonymous users, this prompts them to enter a name the first time they type a comment.  after that, the original name is used and this field is hidden
var comment_add_form_isTimed_container;
var commentsPerSecond = {}; //stores a comments grouped by the second (integer) they are tied to.  
var commentsList;  //the list of comments
var commentsAllBtnQuantity;  //SPAN that shows the number of comments in commentsAllBtn

var selectedMarker; //the marker and group header that contains the current comments and is highlighted when current comments are open

var openCurrCommentsOnNextUpdate = false;
//when true, the user explicitly changed the value of commentFormIsTimedField while the form was open.  this is used to prevent switching of the value automatically if the user scrubs from 0.0 to somewhere else in the video
var wasCommentFormTimedFieldClicked = false;

//*********************** Main ********************


//initializes all the comments-related stuff
function initComments() {
    commentsAllBtnQuantity = $("#commentsQuantity");

    commentsList = $("#comments");
    commentForm = $("#comment-add-form");
    if (commentForm.hasElements()) {
        initNewCommentForm();
    }

    $(document).bind("playheadUpdated", function () {
        updateCurrComments(playheadPosition);
        if (isCommentFormOpen) {
            updateIsTimedFieldForCurrPosition();
        }
    });
}

function getComments() {
    if (videoDetailsData)
        return videoDetailsData.Comments;
    return undefined;
}

function getCommentsInSecond(time) {
    return commentsPerSecond[Math.floor(time).toFixed(0)];
}

//*********************** DOM & Views ********************

//gets the elements that contain this reply in the two comment lists
function getCommentView(id) {
    return $("#comment-" + id.toFixed(0));
}

function getAllCommentViews() {
    return commentsList.children(".comment-thread");
}

function getCommentViewsAndHeaders() {
    return commentsList.children();
}

//indicates whether a comment has a DOM element in the UI
function isCommentInUi(commentId) {
    return getCommentView(commentId).hasElements();
}

//clears all the comments and wipes out the necssary variables
function resetComments() {
    getCommentViewsAndHeaders().remove();
    clearCurrentComments(true, true);
    if (isCommentFormOpen)
        closeCommentForm();

    currCommentsTime = NaN; //the second that contains the "current" comments, which are comments in this second or in the previous second that had current comments.
    currCommentIds = undefined; //contains element ids (like 'comment1234') (value is true for each) for the "current" comment ids
    commentsPerSecond = {}; //stores a comments grouped by the second (integer) they are tied to.  
    selectedMarker = undefined; //the marker and group header that contains the current comments and is highlighted when current comments are open
    openCurrCommentsOnNextUpdate = false;
}


function updateCommentListButtonCounts() {
    //parenthesis are added here instead of in the original html because when loading the page, it shows "()" which is lame
    commentsAllBtnQuantity.text("(" + getAllCommentViews().length + ")"); //at some point you need to fix the data to be in sync.  that way you can use the data and not jquery elements (getComments().length.toFixed(0));
}

//adds all the comments to the UI for the current video's comments data.  assumes the comments have been cleared out
function addComments() {
    var comments = getComments();
    if (comments && $.isArrayWithItems(comments)) {
        checkForEmptyComments(true);
        //add the pre-existing comments
        $.each(comments, function () {
            addCommentToUi(this, true);
        });
    }
    else {
        checkForEmptyComments(false);
    }

}

function checkForEmptyComments(hasComments) {
    if (_.isUndefined(hasComments))
        hasComments = getCommentViewsAndHeaders().hasElements();
    if (hasComments) {
        commentsList.show();
        $("#commentsEmpty").hide();
        commentsAllBtnQuantity.show();
    }
    else {
        commentsList.hide();
        $("#commentsEmpty").show();
        commentsAllBtnQuantity.hide();
    }
}



//creates the HTML view for the given comment and inserts it properly into the dom
//append - if true the comment will be added to the end.  this is reserved for the initial page load, where the comments are pre-sorted.  if false, it will be inserted in the proper position
//note: this shithole function needs refactoring badly
function addCommentToUi(commentData, append) {

    //will hide the empty comments msg if this is the first comment
    if (!append) {
        checkForEmptyComments(true);
    }

    var commentView = createCommentView(commentData);
    var isTimed = $.isNumber(commentData.StartTime);
    if (!isTimed) {
        if (!append) {
            var added = false;
            getCommentViewsAndHeaders().each(function (index) {
                var curr = $(this);
                var currCommentData = curr.data("data");
                //if we encounter a header or a timed comment, insert before it
                if (_.isUndefined(currCommentData) || $.isDefined(currCommentData.StartTime)) {
                    curr.before(commentView);
                    added = true;
                    return false;
                }
            });
            if (!added)
                append = true;
        }
        if (append) {
            commentView.appendTo(commentsList);
        }
    }
    else {
        var isOnlyCommentInSec = false;
        //add it into commentsPerSecond
        var commentSecond = Math.floor(commentData.StartTime);
        var commentsInSec = commentsPerSecond[commentSecond.toFixed(0)];
        if ($.isArray(commentsInSec)) {
            commentsInSec.push(commentData);
        }
        else {
            commentsInSec = [commentData];
            commentsPerSecond[commentSecond.toFixed(0)] = commentsInSec;
            isOnlyCommentInSec = true;
        }

        var insertBefore;
        if (!append) {
            //all we do is look for a comment header whose second is greater than the current second
            getCommentViewsAndHeaders().each(function () {
                var curr = $(this);
                var currSecond = curr.data("second"); //only headers have the 'second' data
                if ($.isDefined(currSecond) && currSecond > commentSecond) {
                    insertBefore = curr;
                    return false;
                }
            });
        }

        if ($.isDefined(insertBefore)) {
            if (isOnlyCommentInSec) {
                insertBefore.before(createSecondHeader(commentData));
            }
            insertBefore.before(commentView);
        }
        else {
            if (isOnlyCommentInSec) {
                createSecondHeader(commentData).appendTo(commentsList);
            }
            commentView.appendTo(commentsList);
        }

        if (isVideoClipOpen) {
            if (isOnlyCommentInSec) {//the comment is first in this second so add teh marker if the video is open               
                addCommentMarker(commentData.StartTime);
                updateSelectedMarker();
            }
            if (sameSecond(currCommentsTime, commentSecond)) {
                updateCurrComments(currCommentsTime, true);
            }
            else if (isCurrentCommentsOpen()) {//this avoids having a new comment from ajax sync showing up during "current" mode
                commentView.hide();
                commentView.prev().hide(); //hide the header as well
            }
        }
    }

    updateCommentListButtonCounts();

    return commentView;
}

//creates the button that acts as a group header for all the comments in a particular second
function createSecondHeader(commentData) {
    var currSecText = $.timeCode(commentData.StartTime, false, false, 0);
    var second = Math.floor(commentData.StartTime);
    var id = 'header-for-second-' + second.toFixed(0);
    var header = $('<div id="' + id + '" class="timestamp"></div>').data("second", second);
    $('<a class="secondHeaderAll" title="Jump to ' + currSecText + ' and show only comments in the same second." href="#">' + currSecText + '</a>').appendTo(header).click(function (e) {
        onMarkerClick(commentData.StartTime);
        return false;
    });
    $('<span class="secondHeaderCurr">' + currSecText + ' - <a title="Show all comments instead of only the ones in this second." href="#">Show All</a></span>').appendTo(header).find("a").click(function (e) {
        openAllComments();
        return false;
    });
    return header;
}

//removes the comment from the UI
function deleteCommentFromUi(commentId) {
    deleteActivityItem(commentId);
    var views = getCommentView(commentId);
    if (!views.hasElements()) {
        return;
    }
    var commentData = views.data("data");
    var isTimed = $.isNumber(commentData.StartTime);
    if (isTimed) {
        var commentSecond = Math.floor(commentData.StartTime);
        var commentsInSec = commentsPerSecond[commentSecond.toFixed(0)];
        commentsInSec.removeItems(commentData);
        var wasOnlyCommentInSecond = commentsInSec.length === 0;
        if (wasOnlyCommentInSecond) {
            delete commentsPerSecond[commentSecond.toFixed(0)];
        }

        var marker = $('#commentMarker' + commentSecond.toFixed(0));
        if (marker.hasElements()) {
            if (wasOnlyCommentInSecond)
                marker.remove();
            else {
                //the markers sit on the first comment.  we might have remove that comment now so we adjsut the marker's position
                marker.css("left", getWidthPercentForTime(getFirstCommentTimeInSecond(commentSecond)));
            }
        }
        if (wasOnlyCommentInSecond) {
            $('#header-for-second-' + commentSecond.toFixed(0)).remove();
        }

    }

    //this will delete any replies from the activity window 
    views.find("div.replies > .reply").each(function () {
        var replyId = $(this).data("data").Id;
        deleteActivityItem(replyId);
    });

    views.remove();

    checkForEmptyComments();

    updateCommentListButtonCounts();

    if (isTimed) {
        if (sameSecond(currCommentsTime, commentSecond)) {
            updateCurrComments(currCommentsTime, true);
        }
    }

//    updateCommentsModelFromViews();//really, you should just delete the comment from 
}


//creates DOM element for a comment.  hooks up data and events as well.
function createCommentView(data) {
    var view = $('<div class="comment-thread" id="comment-' + data.Id + '"></div>').data("data", data);
    var commentContents = $('<blockquote class="comment-contents"></blockquote>').appendTo(view);

    var body = $('<p class="body"></p>').text(data.Body).appendTo(commentContents); //whitespace will be preserved via css

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

    var info = $('<small class="footer"></small>').append(author).append(" &bull; ");

    var isTimed = $.isNumber(data.StartTime);
    if (isTimed) {
        var timeCode = $('<span class="timeCode"></span>').appendTo(info);
        timeCode.append($('<a class="timeCode" href="#" title="Move the video to this spot."></a>').text($.timeCode(data.StartTime, false, false, 1)).click(function (e) {
            if (isVideoClipOpen) {
                if (isPlaying)
                    videoPlayer.pause();
                if (canSeekTo(data.StartTime, true)) {
                    seek(data.StartTime, true);
                    $.scrollTo("#player");
                }
            }
            return false;
        })).append(" &bull; ");
    }


    var createdTooltip = moment(data.CreatedOn).format('LLL');
    var createdOn = $('<em class="relativeDate"></em>').text(toRelativeString(data.CreatedOn)).attr("title", createdTooltip).data("date", data.CreatedOn);

    info.append(createdOn).appendTo(commentContents);

    var actions = $('<span class="actions"></span>').appendTo(info);
    var replyBtn, replies, isReplyFormOpen = false, isEditing = false, isHoveringOverReplies = false;
    if (productionData.CanAddCommentReply) {
        replyBtn = $('<a href="#" class="commentReply" title="Write a response to this comment">Reply</a>');
        replyBtn.click(function () {
            isReplyFormOpen = true;
            var replyForm = createCommentReplyForm(replies, data.Id, function () {
                isReplyFormOpen = false;
                replyForm.remove();
                if (!isHoveringOverReplies && !isEditing)
                    actions.removeClass("hidden");
            });
            replyForm.prependTo(replies);
            replies.show(); //form is contained within replies div, and if no replies have been written it will be hidden 
            replyForm.show();
            replyForm.find("textarea").focus();
            actions.addClass("hidden");
            return false;
        }).appendTo(actions);
    }
    if (data.CanEdit) {
        $('<a href="#" class="smallEdit" title="Edit this comment">Edit</a>').click(function () {
            var form = $('<form class="comment-edit"></form>');
            var closeForm = function () {
                form.resetValidation(false);
                body.empty().text(data.Body);
                if (!isReplyFormOpen && !isHoveringOverReplies)
                    actions.removeClass("hidden");
            };
            var textInput = $('<textarea type="text" name="body"></textarea>').val(data.Body).appendTo(form);
            var buttons = $('<div class="buttons"></div>').appendTo(form);
            $('<input type="submit" value="Update" class="btn btn-primary" />').appendTo(buttons);
            $('<a class="cancelBtn btn" href="#">Cancel</a>').click(function () {
                closeForm();
                return false;
            }).appendTo(buttons);
            body.empty().append(form);
            textInput.focus(); //must do this after added to DOM
            actions.addClass("hidden");
            var oldBody = data.Body;
            form.madValidate({
                url: (window.urlPrefixes.relative + "comments/edit"),
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
                error: function () {
                    alert("Uh oh.  There was a problem while updating your comment.  We'll check it out and get back to you.  In the meantime, we've put the original text back in.");
                    data.Body = oldBody;
                    body.empty().text(data.Body);
                },
                beforeSend: function (result) {
                    //we update the reply immediately and restore it if there was a problem.  this makes the UI faster.  
                    data.Body = textInput.val();
                    closeForm();
                }
            });

            return false;
        }).appendTo(actions);
    }

    if (data.CanDelete) {
        $('<a href="#" class="smallDelete" title="Delete this comment">Delete</a>').click(function () {
            onCommentDeleteBtnClick(data);
            return false;
        }).appendTo(actions);
    }

    //while the user is focused on a reply, we hide the actions for this comment.  otherwise it would get pretty busy
    var replies = $('<div class="replies"></div>').appendTo(view).hover(function () {
        isHoveringOverReplies = true;
        actions.addClass("hidden");
    }, function () {
        isHoveringOverReplies = false;
        if (!isReplyFormOpen && !isEditing)
            actions.removeClass("hidden");
    });

    if ($.isArrayWithItems(data.Replies)) {
        $.each(data.Replies, function () {
            replies.append(createReplyView(this));
        });
    }
    else {
        replies.hide();
    }

    return view;
}

//given a comment wrapper, this finds the element containing the comment body text
function findCommentBody(view) {
    return view.find("p.body:first");
}

//*********************** Current Comments pane ***********************

//removes all the data associated with Current comments and hides the tab
function clearCurrentComments(openAllComments1, hideTabBtn) {
    currCommentIds = null;
    currCommentsTime = NaN;
    if (openAllComments1)
        openAllComments();

    updateCommentListButtonCounts();
}

//given the time passed in, this returns the first prior second that had comments 
function prevSecondWithComments(time) {
    var second = Math.floor(time) - 1; //ignore the current second
    //we could definitely use an optimization here.  maybe we can store an array of sorted seconds that each contain comments.  then all that we do here is an easy binary search to find the index of the original second then just decrement down
    while (second >= 0) {
        if ($.isDefined(commentsPerSecond[second.toFixed(0)])) {
            return second;
        }
        second--;
    }
    return NaN;
}

//given the time passed in, this returns the first prior second that had comments 
function nextSecondWithComments(time) {
    var second = Math.floor(time) + 1; //ignore the current second
    var lastSecond = Math.floor(clipDuration);
    //we could definitely use an optimization here.  maybe we can store an array of sorted seconds that each contain comments.  then all that we do here is an easy binary search to find the index of the original second then just decrement down
    while (second <= clipDuration) {
        if ($.isDefined(commentsPerSecond[second.toFixed(0)])) {
            return second;
        }
        second++;
    }
    return NaN;
}


//gives focus to the comments that fall within the second passed.  note the comments will not be highlighted if comments are being hovered....however once the hover is cleared, they will be shown
function updateCurrComments(time, refreshIfSameSecond) {
    if (isNaN(time))
        time = currCommentsTime;
    var second = Math.floor(time);

    //if the comment form is open, we keep focus even if there are no comments in this second
    var comments = commentsPerSecond[second.toFixed(0)];
    var hasCommentsInSecond = $.isArrayWithItems(comments);

    var updateComments = false;
    var openCurrTab = false;
    if (hasCommentsInSecond) {
        //no need to do any more work
        if (refreshIfSameSecond || !sameSecond(currCommentsTime, time)) {
            updateComments = true;
            //make sure the tab is open.  the NaN check is for when the video initially loads and there is a marker at the 0 second.  in that case we don't want to open the current comments tab so the user can initially see all comments
            if (!isPlaying && !(isNaN(currCommentsTime) && second === 0))
                openCurrTab = true;
            currCommentsTime = time;
        }
        else if (sameSecond(currCommentsTime, time) && !isPlaying) {
            openCurrTab = true;
        }
    }
    else {
        var showEmpty = isCommentFormOpen && isNewCommentTimed();
        //if the form is open and they are commenting on the current position, we show the current comment pane even if there are no comments in that second
        if (showEmpty) {
            clearCurrentComments(false, false);
            currCommentsTime = time;
            comments = [];
            updateComments = true;
        } else {
            //if the playhead seeks, it might jump over a marker.  in that case, we aren't focused on the exact second but the current comments need to be replaced with the most recent ones
            var prevComments = prevSecondWithComments(time);
            //this happens if they seek back to a position before the first marker.  in this case we hide the current comments completely
            if (isNaN(prevComments)) {
                clearCurrentComments(true, true);
                return;
            }
            else {
                //the current comments are from the prior second that also has comments, or the user skipped right passed the first marker 
                if (refreshIfSameSecond || (isNaN(currCommentsTime) || !sameSecond(currCommentsTime, prevComments))) {
                    updateComments = true;
                    currCommentsTime = prevComments;
                    comments = commentsPerSecond[prevComments.toFixed(0)];
                }
            }
        }
    }

    if (updateComments) {
        currCommentIds = {};
        var isFirst = true;
        $.each(comments, function () {
            if (isFirst) {
                currCommentIds['header-for-second-' + Math.floor(this.StartTime).toFixed(0)] = true;
                isFirst = false;
            }
            currCommentIds['comment-' + this.Id] = true;
        });

        var currSecText = $.timeCode(currCommentsTime, false, false, 0);
        if (commentAddForm_isTimed_timecode)
            commentAddForm_isTimed_timecode.text(currSecText);
        updateSelectedMarker();
        if (isCurrentCommentsOpen()) {
            updateCurrentCommentViews();
        }
    }

    if (openCurrCommentsOnNextUpdate) {
        openCurrentComments();
    }
}

function openAllComments() {
    $("#commentsArea").removeClass("curr");
    getCommentViewsAndHeaders().show();
    updateSelectedMarker();

}

//indicates whether the Current comments tab is open
function isCurrentCommentsOpen() {
    return $("#commentsArea").hasClass("curr");
}

//opens the tab that contains the Current comments.  
function openCurrentComments() {
    if (isCurrentCommentsOpen())
        return;
    $("#commentsArea").addClass("curr");
    updateCurrentCommentViews();
    updateSelectedMarker();

}

//isolates the current comment views by showing and hiding the appropriate elements
function updateCurrentCommentViews() {
    getCommentViewsAndHeaders().each(function () {
        var curr = $(this);
        var id = curr.attr("id");
        if ($.exists(currCommentIds) && true === currCommentIds[id]) {
            curr.show();
        }
        else {
            curr.hide();
        }

    });
}

function sameSecond(time1, time2) {
    return Math.floor(time1) === Math.floor(time2);
}

//*********************** Markers ***********************

//adds a comment marker.  be sure you call this correctly because it could result in multiple markers for teh same second
function addCommentMarker(time) {
    var id = 'commentMarker' + Math.floor(time).toFixed(0);
    var marker = $('<div id="' + id + '" class="marker"></div>');
    var popup, timeCode, author, body;

    var popupPlugin = new $.madPopup({
        anchor: marker,
        hAnchorAlign: "center",
        hPopupAlign: "center",
        vAnchorAlign: "top",
        vPopupAlign: "bottom",
        showOnHover: true,
        hoverDelay: 0,
        popup: function () {
            popup = $('<div class="commentMarkerTooltip"></div>');
            var detailsDiv = $('<div class="details"></div>').appendTo(popup);
            author = $('<div class="author"></div>').appendTo(detailsDiv);
            timeCode = $('<span class="time"></span>').text($.timeCode(time, false, true, 0)).appendTo(detailsDiv);
            body = $('<div class="body"></div>').appendTo(popup);
            return popup;
        },
        beforeShow: function () {
            //update comments each time tooltip is shown so we don't have to worry about incorrect info if the comments change
            var comments = getCommentsInSecond(time);
            //the comment we will be showing
            var commentData;
            if (comments.length > 1) {
                commentData = getMostRecentCreatedComment(comments);
                body.text(commentData.Body);
                body.prepend($("<div>" + comments.length.toFixed(0) + " comments.  Latest one:</div>"));
            }
            else {
                commentData = comments[0];
                body.text(commentData.Body);
            }
            if ($.exists(commentData.Creator)) {
                author.text(commentData.Creator.Name);
            }
            else {
                author.text("Unknown Author");
            }
        }
    });

    marker.data("second", Math.floor(time))
        .click(function (e) {
            onMarkerClick(time);
            return false;
        }).mouseenter(function (e) {
            if (isNaN(secondsDownloaded) || secondsDownloaded < time) {
                marker.css("cursor", "default");
            }
            else {
                marker.css("cursor", "pointer");
            }
        }).mouseleave(function (e) {

        });

    addTimelineMarker(marker, time);
}

//fired when a marker on the timeline or a group header in the list is clicked
function onMarkerClick(time) {
    if (isPlaying)
        videoPlayer.pause();

    var seekTo = getFirstCommentTimeInSecond(time);
    if (!canSeekTo(seekTo, true)) {
        return;
    }

    openCurrCommentsOnNextUpdate = true;
    seek(seekTo, true);
}

function getMarkers() {
    return playheadProgress.find("div.marker");
}

function findMarkerAtSecond(second) {
    var sec = Math.floor(second).toFixed(0);
    return $('#commentMarker' + sec + ', #header-for-second-' + sec);
}


//ensures the selectedMarker is correct.  will deselect if necessary
function updateSelectedMarker() {
    //marker will be selected when the current comments are open

    if (!selectedMarker) {
        selectedMarker = findMarkerAtSecond(Math.floor(currCommentsTime));
        selectedMarker.data("second", Math.floor(currCommentsTime));
        selectedMarker.addClass("selected");
    }
    else if (selectedMarker.data("second") !== Math.floor(currCommentsTime)) {
        selectedMarker.removeClass("selected");
        selectedMarker = findMarkerAtSecond(Math.floor(currCommentsTime));
        selectedMarker.data("second", Math.floor(currCommentsTime));
        selectedMarker.addClass("selected");
    }
}



//gets the exact position, including milliseconds, of the first comment within the second passed
function getFirstCommentTimeInSecond(second) {
    second = Math.floor(second);
    var commentsInSec = commentsPerSecond[second.toString()];
    var time = NaN;
    $.each(commentsInSec, function () {
        if (isNaN(time) || this.StartTime < time)
            time = this.StartTime;
    });
    return time;
}


//gets the comment with the most recent CreatedOn date
function getMostRecentCreatedComment(comments) {
    var latestDate = NaN, latestComment;
    $.each(comments, function () {
        if (isNaN(latestDate) || this.CreatedOn < latestDate) {
            latestDate = this.CreatedOn;
            latestComment = this;
        }
    });
    return latestComment;
}


//scrolls to the comment or reply if it's not already in the viewport.  When scrolling, it scrolls the minimal amount possible, so the comment will be on the top or bottom of the viewport, depending on the scroll position when this was called.
//also places a big arrow next to it to highlight the comment
function highlightComment(view, showArrow, colorEffect) {
    var viewport = $(window);
    var popup = viewport.data("cHighlight");
    if (popup)
        popup.hide(); //so there's only one popup at a time
    var viewportTop = viewport.scrollTop(), viewportHeight = viewport.height();
    var viewportBottom = viewportHeight + viewportTop;
    var elementTop = view.offset().top;
    var elementHeight = view.outerHeight();
    var elementBottom = elementTop + elementHeight;

    if (viewportTop > elementTop) {
        var scrollTop = (elementTop) - 30; //add 30 for a bit extra padding
        $.scrollTo({ top: scrollTop, left: 0 }, 500);
    }
    else if (viewportBottom < elementBottom) {
        var scrollTop = (elementBottom - viewportHeight) + 30; //add 30 for a bit extra padding
        $.scrollTo({ top: scrollTop, left: 0 }, 500);
    }

    if (showArrow) {
        var popup = $("<label class='arrowRight'></label>").appendTo("body");
        viewport.data("cHighlight", popup);
        var popupOffset = popup.offset();
        var commentText = findCommentBody(view);
        var left = view.offset().left - popup.width(); //align horizontally with the comment container
        var top = commentText.offset().top + (commentText.outerHeight() / 2) - (popup.height() / 2); //align vertically with the
        popup.css({ "left": left.toFixed(0) + "px", "top": top.toFixed(0) + "px" });
        window.setTimeout(function () {
            popup.fadeOut(500, function () {
                popup.remove();
                if (popup === viewport.data("cHighlight")) {
                    viewport.removeData("cHighlight");
                }
            });
        }, 2500);
    }

    if (colorEffect) {
        view.effect("highlight", { color: "#ffc" }, 10000);
    }
}



function updateCommentBody(commentId, body) {
    var view = getCommentView(commentId);
    if (view.hasElements()) {
        findCommentBody(view).text(body);
    }
}



//*********************** Add, Edit, Delete ***********************

//indicates whether the radio button in the Add Comment form is for a timed comment or the whole video
function isNewCommentTimed() {
    return isVideoClipOpen && commentFormIsTimedField.filter(":checked").val() === "true";
}

//call this after setting productionData.SemiAnonymousUserName after the server returned a successful comment or reply result
function onSemiAnonymousUserNameChangeSubmitted() {
    if ($.exists(comment_add_form_name_container)) {
        comment_add_form_name_container.remove();
        comment_add_form_name_container = undefined;
    }
}

function needsToEnterUserNameInCommentForms() {
    return isSemiAnonymousUserWhoCanComment() && !$.hasChars(productionData.SemiAnonymousUserName);
}


//does setup for the comment input form
function initNewCommentForm() {
    $("#addCommentBtnInHeader, #commentsEmpty a").click(function (e) {
        onCommentAddBtnClick(e);
        return false;
    });

    $("#comment-add-form-body-input").val(""); //this is necessary if the user goes to another page and hits the back button.  at that point the browser will have the old text in there
    //note that I originallt wanted to use .cancel but used .cancel-button because jquery validation plugin has an error with .cancel elements
    commentForm.find("a.cancel-button").click(function (e) {
        closeCommentForm();
        checkForEmptyComments(); //the commentsEmpty will be hidden while this form is active, so if they click cancel then we reshow it
        updateCurrComments(playheadPosition); //will in turn close the details pane if it's empty
        return false;
    });

    if (needsToEnterUserNameInCommentForms()) {
        comment_add_form_name_container = $("#comment-add-form-name");
    }

    comment_add_form_isTimed_container = $("#comment-add-form-isTimed-container");
    commentFormIsTimedField = comment_add_form_isTimed_container.find("input[type='radio']").click(function (e) {
        wasCommentFormTimedFieldClicked = true;
        var isTimed = isNewCommentTimed();
        if (isTimed) {
            updateCurrComments(playheadPosition);
            openCurrentComments();
        }
        else {
            updateCurrComments(playheadPosition);
            openAllComments();
        }
    });

    commentAddForm_isTimed_timecode = $("#comment-add-form-isTimed-timecode");

    var submittedData;
    commentForm.madValidate({
        //        onfocusout: true,
        //        onkeyup: true,
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
            var startTime;
            if (isNewCommentTimed()) {
                startTime = playheadPosition;
            }
            else {
                startTime = null;
            }
            submittedData = {
                VideoId: videoDetailsData.Id,
                Body: $('#comment-add-form-body-input').val(),
                StartTime: startTime
            };
            if (needsToEnterUserNameInCommentForms() && $.exists(comment_add_form_name_container) && comment_add_form_name_container.length > 0) {
                submittedData["UserName"] = comment_add_form_name_container.find("input").val();
            }
            return submittedData;
        },
        submitSuccess: function (result) {
            closeCommentForm();
            var view = addCommentToUi(result.Data, false);
            addActivityItem(result.ActivityPanelItem, false);
            highlightComment(view, false, true);
            //updateCommentsModelFromViews();
            if ($.hasChars(submittedData["UserName"])) {
                productionData.SemiAnonymousUserName = submittedData["UserName"];
                onSemiAnonymousUserNameChangeSubmitted();
            }

        }
    });

}

//this works but is not needed
////updates current video model based on comment views.  not ideal and eventually you gotta replcae with some type of model framework like bootstrap or something
//function updateCommentsModelFromViews() {
//    videoDetailsData.Comments = [];
//    getAllCommentViews().eachJ(function () {
//        var commentModel = this.data("data");
//        videoDetailsData.Comments.push(commentModel);
//    }
//    );
//}


//in lieu of comment models not being in sync with the UI, this temporary function gets the current models from views
function getCurrentCommentModels() {
    if (_.isUndefined(videoDetailsData)) {
        return;
    }

    var commentModels = [];
    getAllCommentViews().eachJ(function () {
        var commentModel = this.data("data");
        commentModels.push(commentModel);
        commentModels.Replies = [];
        this.find("blockquote.reply").eachJ(function () {
            commentModels.Replies.push(this.data("data"));
        });
    });
    return commentModels;
}




//occurs when the Add Comment button on the player is clicked
function onCommentAddBtnClick(e) {

    e.preventDefault();
    if (!productionData.CanAddComment)
        return;
    if (isCommentFormOpen) {
        commentForm.submit();
        return;
    }
    isCommentFormOpen = true;
    wasCommentFormTimedFieldClicked = false;
    if (isPlaying)
        videoPlayer.pause();

    $("#commentsEmpty").hide();

    if (isVideoClipOpen) {
        comment_add_form_isTimed_container.show(); //just in case it was hidden before
        updateIsTimedFieldForCurrPosition();
    }
    else {
        //if the video clip isn't open, either because they clicked real quickly before it downloaded or because it's transcoding, we still allow them to comment for the whole video.
        comment_add_form_isTimed_container.hide();
    }
    commentForm.show();
    $.scrollTo("#comment-add-form", 100);
    $("#comment-add-form-body-input").focus();
}


//automatically sets the "is timed" comment radio button based on playhead position.  If playhead is at beginning, we assume the comment is for the whole video. 
function updateIsTimedFieldForCurrPosition() {
    //the user clicked the radio button so don't change the value 
    if (wasCommentFormTimedFieldClicked)
        return;
    //if they playhead is at the start, assume they want to add a comment for a whole video
    if (playheadPosition === 0) {
        commentFormIsTimedField.filter(":last").attr("checked", "checked");
    }
    else {
        commentFormIsTimedField.filter(":first").attr("checked", "checked");
        updateCurrComments(playheadPosition, false);
        openCurrentComments();
    }
}

//occurs when the user clicks the Delete button for a comment
function onCommentDeleteBtnClick(commentData) {
    var commentId = commentData.Id;
    var message = "Are you sure you want to delete this comment?  This cannot be undone.";
    if (confirm(message)) {
        $.madAjax({
            url: (window.urlPrefixes.relative + "comments/delete"),
            data: function () {
                return {
                    id: commentId
                };
            },
            error: function () {
                alert("Uh oh.  There was a problem while deleting your comment.  We'll check it out and get back to you.  In the meantime, we've put your comment back in.");
                addCommentToUi(commentData, false);
                return true; //cancel other error handlers
            },
            beforeSend: function (result) {
                //we delete the comment immediately and restore it if there was a problem.  this makes the UI more responsive
                deleteCommentFromUi(commentId);
            }
        });
    }
}

//clears the form contents for Add Comment form and hides it
function closeCommentForm() {
    isCommentFormOpen = false;
    commentForm.hide().resetValidation(true);
    commentForm.find("input[type='radio']:first").attr("checked", "checked");
}



//*********************** Sorting ***********************


//how you would sort
//    getAllCommentViews().sortElements(function (a, b) {
//        return sortByDateCreated($(a).data("data"), $(b).data("data"));
//    }, false);
//return -1 is a < b, 1 is b > a
//sorts by StartTime then by CreatedOn
function sortByTime(commentA, commentB) {
    var isATimed = $.isNumber(commentA.StartTime);
    var isBTimed = $.isNumber(commentB.StartTime);
    if (!isATimed && !isBTimed) {
        return commentA.CreatedOn - commentB.CreatedOn;
    }
    else if (!isATimed || !isBTimed) {
        if (!isATimed)
            return -1;
        return 1;
    }
    else {
        var startDiff = commentA.StartTime - commentB.StartTime;
        if (startDiff === 0) {
            return commentA.CreatedOn - commentB.CreatedOn;
        } else {
            return startDiff;
        }
    }
}

function sortByDateCreated(commentA, commentB) {
    return commentA.CreatedOn - commentB.CreatedOn
}
