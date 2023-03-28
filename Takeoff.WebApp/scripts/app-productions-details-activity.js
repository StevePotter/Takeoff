/// <reference path="jquery-1.6.2-vsdoc.js" />
/// <reference path="takeoff-helpers.js" />
/// <reference path="takeoff-forms.js" />
/// <reference path="ProductionView.js" />

//*********************** Change Checking ********************

//the number of milliseconds between checking for activity
var activityInterval;
var activityList;
var maxActivityItems = 10;
var enableChangeSyncing = true;

function initActivity() {
    activityList = $("#activity-container ul");

    if ($.isArrayWithItems(productionData.Activity)) {
        activityList.empty();
        $.each(productionData.Activity, function () {
            addActivityItem(this, true);
        });

    }
    else {
        $("#activityEmpty").show();
    }

    //some of the links go to elements like a comment, but it may be for another video so we use a system of conventions to direct the user to the right place
    $("#activity-container ul, #notification").delegate("a", "click", function () {
        var href = $(this).attr("href");
        if (($.startsWith(href, "#") || $.startsWith(href, top.location.href + "#")) && sendToItem($.afterStr(href, "#"))) {//note: some browsers were putting hte whole url as the href value (sorry I forget which ones..this comment was written after the fact).
            return false;
        }
    });

    if (enableChangeSyncing)
        initChangeChecking();
}

function addActivityItem(activityData, append) {
    if (!$.exists(activityList))
        return;
    $("#activityEmpty").hide();

    while (activityList.children().length >= maxActivityItems - 1) {
        activityList.children("li:last").remove();
    }

    var activityView = $('<li class="' + activityData.CssClass + '" id="activity' + activityData.ThingId + '">' + activityData.Html + '</li>');
    if (append)
        activityView.appendTo(activityList);
    else
        activityView.prependTo(activityList);

    if ($.isDefined(activityData.Date)) {
        activityView.find("em").text(toRelativeString(activityData.Date, relativeDateConfig.sentence)).data("date", activityData.Date).data("config", relativeDateConfig.sentence);
    }
    return activityView;
}

//deletes the action related to the given thing if it exists in the activity panel
function deleteActivityItem(thingId) {
    var item = $("#activity" + thingId.toFixed(0));
    if (item.hasElements()) {
        item.remove();
        if (!activityList.find("li").hasElements())
            $("#activityEmpty").show();
    }
}

function sendToItem(link) {
    var typeAndId = link.split("_");
    var mainType = typeAndId[0];
    var ids = {};
    for (var i = 0; i < typeAndId.length; i += 2) {
        ids[typeAndId[i]] = parseInt(typeAndId[i + 1]);
    }
    switch (mainType) {
        case "comment":
            loadVideo(ids.video, false, function () {
                var view = getCommentView(ids.comment);
                if (view.hasElements()) {
                    var data = view.data("data");
                    //if we can move the video to the comment's time, go for it.  otherwise just open it in the "All" tab
                    var isTimed = $.isNumber(data.StartTime);
                    if (isTimed && canSeekTo(data.StartTime, false)) {
                        seek(data.StartTime);
                        openCurrentComments();
                    }
                    else {
                        openAllComments();
                    }
                    //scroll to the comment (leave a little space cuz it looks better then show a big arrow next to it to highlight for a few seconds
                    highlightComment(view, true, false);
                }
            });
            return true;
        case "reply":
            loadVideo(ids.video, false, function () {
                var view = getReplyView(ids.reply);
                if (view.hasElements()) {

                    openAllComments();
                    highlightComment(view, true, false);

                }
            });
            return true;
        case "video":
            loadVideo(ids.video);
            return true;
    }
    return false;
}

function initChangeChecking() {
    window.setTimeout(function () {
        $.madAjax({
            url: (window.urlPrefixes.relative + "Productions/" + productionData.Id.toFixed(0) + "/Changes"),
            data: {
                //    id: productionData.Id,
                latestChangeId: productionData.LastChangeId
            },
            error: function () {
                //global error handlers will run, but we want to keep trying
                initChangeChecking();
            },
            serverError: function () {
                return true;//this is a background request so don't pop up an error message on errors.  this was a big problem when memcached was occasionally blowing up
            },
            aborted: function () {
                return true; //this is a background request so don't pop up an error message on errors.  this was a big problem when memcached was occasionally blowing up
            },
            success: function (result) {
                //a null result indicates no changes to the production
                if ($.isNullOrUndefined(result)) {
                    initChangeChecking();
                    return;
                }
                //a RedirectUrl property is provided when there is an error like the production was deleted by someone else
                if (result.RedirectUrl) {
                    $.goTo(result.RedirectUrl);
                    return;
                }
                if ("NotLoggedIn" === result.ErrorCode) {
                    var email = window.user.email;
                    var emailStr = $.emptyString(email) ? "" : "email=" + encodeURIComponent(email) + "&";
                    $.goTo(window.urlPrefixes.relativeHttps + "login?" + emailStr + "heading=" + encodeURIComponent("You were inactive for a while so we need you to log back in.") + "&returnUrl=" + encodeURIComponent(top.location.href));
                    return;
                }

                productionData.LastChangeId = result.LatestChange;
                if (result && result.Changes && result.Changes.length > 0) {
                    $.each(result.Changes, function () {
                        var change = this;
                        //add the new entry to the activity panel if necessary
                        var activityPanelItem = change.ActivityPanelItem;
                        if ($.exists(activityPanelItem)) {
                            if (!$("#activity" + activityPanelItem.ThingId.toFixed(0)).hasElements()) {
                                var item = addActivityItem(activityPanelItem, false);
                                //ghetto way to provide activity message
                                var n = $("<span>" + item.html() + "</span>");
                                n.children("em").remove(); //todo: what is this here for?
                                notification.info(n);
                            }
                        }

                        if (change.ThingType === "VideoComment" || change.ThingType === "Comment") {
                            //for now we ignore comments changed on a different video than the one being viewed
                            if ($.isNumber(change.ThingParentId) && $.exists(videoDetailsData) && change.ThingParentId === videoDetailsData.Id) {
                                if (change.Action === "Add") {
                                    if (!isCommentInUi(change.ThingId))
                                        addCommentToUi(change.ViewData, false);
                                }
                                else if (change.Action === "Delete") {
                                    deleteCommentFromUi(change.ThingId);
                                }
                                else if (change.Action === "Update") {
                                    if ($.exists(change.ViewData) && $.exists(change.ViewData.Body)) {
                                        updateCommentBody(change.ThingId, change.ViewData.Body);
                                    }
                                }
                            }
                        }
                        else if (change.ThingType === "CommentReply") {
                            if (change.Action === "Add") {
                                if (!isReplyInUi(change.ThingId))
                                    addReplyToUi(change.ViewData);
                            }
                            else if (change.Action === "Delete") {
                                deleteReplyFromUi(change.ThingId);
                            }
                            else if (change.Action === "Update") {
                                if ($.exists(change.ViewData) && $.exists(change.ViewData.Body)) {
                                    updateCommentReplyBody(change.ThingId, change.ViewData.Body);
                                }
                            }
                        }
                        else if (change.ThingType === "Video") {
                            var changedVideo = getVideoData(change.ThingId);
                            if (change.Action === "Add") {
                                if (!$.exists(changedVideo)) {
                                    productionData.Videos.push(change.ViewData);
                                    addVideosToList([change.ViewData], true);
                                    notification.info("A new video called '" + change.ViewData.Title + "' has been added.");
                                }
                            }
                            else if (change.Action === "Update") {//right now the only update is when the video transcode succeeded
                                if ($.exists(changedVideo)) {
                                    var isCurrentVideo = $.exists(videoDetailsData) && videoDetailsData.Id === changedVideo.Id;
                                    if (change.ViewData && $.isDefined(change.ViewData.HasVideo)) {
                                        var hadVideo = changedVideo.HasVideo;
                                        $.each(change.ViewData, function (key, value) {
                                            changedVideo[key] = value;
                                            if (isCurrentVideo)
                                                videoDetailsData[key] = value;
                                        });
                                        //if the video that changed is being watched, reload the player for the new video
                                        if (isCurrentVideo && $.isDefined(change.ViewData.HasVideo)) {
                                            //this makes an extra ajax call because the video url is not included in ViewData.  
                                            loadVideo(changedVideo.Id, true);
                                        }
                                        else if (!hadVideo && changedVideo.HasVideo) {
                                            notification.info(changedVideo.Title + " is ready to watch.");
                                        }

                                        //will update the thumbnails and title
                                        var videoListItem = videosList.find("#video" + changedVideo.Id.toFixed(0));
                                        if (videoListItem.hasElements()) {
                                            videoListItem.replaceWith(createVideoListItem(changedVideo));
                                            getVideoListItems().equalHeights();
                                        }
                                    }
                                    if (change.ViewData && $.isDefined(change.ViewData.Title)) {
                                        $.each(change.ViewData, function (key, value) {
                                            changedVideo[key] = value;
                                            if (isCurrentVideo)
                                                videoDetailsData[key] = value;
                                        });
                                        updateVideoTitle(changedVideo.Id, change.ViewData.Title);
                                    }

                                }

                            }
                            else if (change.Action === "Delete") {
                                deleteVideoFromUi(change.ThingId, true);
                            }
                        }
                        else if (change.ThingType === "File") {
                            if (change.Action === "Add") {
                                //only add files that fall directly under production
                                if (!isFileInUi(change.ThingId) && $.isNumber(change.ThingParentId) && change.ThingParentId === productionData.Id) {
                                    addFileToUi(change.ViewData);
                                }
                            }
                            else if (change.Action === "Delete") {
                                deleteFileFromUi(change.ThingId);
                            }
                        }
                        else if (change.ThingType === "Membership") {
                            if (change.Action === "Add") {
                                //user id is in the Data field
                                if (!isMemberShown(change.ThingId)) {
                                    loadMembersIntoTable([change.ViewData], false);
                                }
                            }
                            else if (change.Action === "Delete") {
                                deleteMemberFromUi(change.ThingId);
                            }
                        }
                        else if (change.ThingType === "MembershipRequest") {
                            if (change.Action === "Add") {
                                //user id is in the Data field
                                if (!isMemberShown(change.ThingId)) {
                                    loadMembersIntoTable([change.ViewData], false);
                                }
                            }
                            else if (change.Action === "Delete") {
                                deleteMemberFromUi(change.ThingId);
                            }
                        }
                        else if (change.ThingType === "Project") {
                            if (change.Action === "Update") {
                                var fields = change.ViewData;

                                var title = fields.Title;
                                if ($.exists(title)) {
                                    updateTitle(title);
                                }
                            }
                        }
                    });
                }

                initChangeChecking();
            }
        });
    }, activityInterval);
}

