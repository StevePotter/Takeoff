/// <reference path="jquery-1.6.2-vsdoc.js" />
/// <reference path="swfobject.debug.js" />
/// <reference path="underscore.js" />
/// <reference path="takeoff-helpers.js" />
/// <reference path="mediascend.upload.js" />
/// <reference path="takeoff-tooltip.js" />
/// <reference path="takeoff-tabs.js" />
/// <reference path="ProductionView.js" />
/// <reference path="jquery-ui-1.8.4.custom.min.js" />

/**
* Copyright (c) 2009 Mediascend, Inc.
* http://www.mediascend.com
*/

//*********************** Video Player ********************

var secondsBetweenKeyframes = 0.2;

var videoDetailsData; //the current video being shown in the videoDetails 
var playerArea, playerTransport, playBtn, volumeBtn, downloadProgress, isFlashVideoPlayer;
var playheadTime, playheadProgress, playheadFill, playheadThumb, playheadTooltip, addCommentBtnInPlayhead, playheadTimecode, durationTimecode;
var playheadTooltipTimecode;
var clipDuration, secondsDownloaded, isVideoClipOpen = false, isPlaying = false;
var videoEditForm; //the form that is shown when the user hits Edit on the video
var playheadPosition; //the current position in the video being shown
var markerWithMouseDown; //fixes an issue when clicking on markers.  a marker can actually span more than one second.  and there is no click event because jquery slider intercepts it.  and even then you want to focus on the right spot.  this guy tracks the latest marker who fired a mousedown.
var isPlayheadDragging = false;
var isVideoEnlarged = false;
var enlargeTooltip = "Make Video Bigger";
var preferHtml5Video = false; //if true, html5 will be used before flash

//loads the video with the given id into the player and comment areas.  shows the player immedately
function loadVideo(videoIdOrData, force, onLoaded) {
    var isId = $.isNumber(videoIdOrData);
    var videoId = isId ? videoIdOrData : videoIdOrData.Id;
    if (videoDetailsData && videoDetailsData.Id === videoId && !force) {
        openVideoDetails();
        openVideosTab();
        if ($.isFunction(onLoaded))
            onLoaded();
        return;
    }
    if (isId) {
        $.madAjax({
            url: (window.urlPrefixes.relative + "productions/" + productionData.Id.toFixed(0) + "/videos/" + videoId.toFixed(0)),
            data: function () {
                return {
                    id: videoId,
                    logWatch: true
                };
            },
            success: function (result) {
                //todo: if the video didn't exist, we should provide some kind of error message and drop out of this funciton
                loadVideo(result, force, onLoaded);
            }
        });
    }
    else {
        resetVideoPlayer();
        resetComments();
        videoDetailsData = videoIdOrData;
        initPlayer();
        addComments();
        openVideoDetails();
        openVideosTab();
        mainPanelTabs.open(0);
        if ($.isFunction(onLoaded))
            onLoaded();
    }
}

function initPlayer() {
    playerArea = $("#player-container").empty();
    playerArea.append(createVideoTitleArea(videoDetailsData));

    if (!videoDetailsData.HasVideo) {
        showVideoEncoding();
        return;
    }


    if (preferHtml5Video && tryInitHtml5Player()) {
        return;
    }
    if (swfobject.hasFlashPlayerVersion("9.0.0")) {
        initFlashPlayer(videoDetailsData.WatchUrl);
        initPlayerTransport();
    }
    else if (!tryInitHtml5Player()) {
        showPlayerPluginError();
    }
}

function tryInitHtml5Player() {
    var videoSupport = html5VideoSupport(); //todo: detect flash and use html5 as a fallback because scrubbing sucked in chrome  html5VideoSupport();  swfobject.getFlashPlayerVersion().major
    if (videoSupport && videoSupport.mp4) {
        initHtml5Player(videoDetailsData.WatchUrl);
        initPlayerTransport();
        return true;
    }
    return false;
}

function showVideoEncoding() {
    if (swfobject.hasFlashPlayerVersion("9.0.0")) {
        var msg = $('<div class="alert alert-info">The video is being processed and should be ready soon.  In the meantime, please enjoy some old school Pong.</div>');
        msg.appendTo(playerArea);
        var swfId = "pong" + $.randomBetween(0, 10000);
        var swfUrl = window.urlPrefixes.asset + "Assets/pong.swf";
        $('<div class="pong"><div id="' + swfId + '"></div></div>').appendTo(playerArea);
        swfobject.embedSWF(swfUrl, swfId, "100%", "100%", "9", window.urlPrefixes.asset + "Assets/expressInstall.swf", {}, {
            wmode: "transparent",
            quality: "high",
            allowfullscreen: "false"
        });
    }
    else {
        var msg = $('<div class="alert-message">The video is being processed and should be ready soon.</div>');
        msg.appendTo(playerArea);
    }
}

function html5VideoSupport() {
    var videoEl = document.createElement('video') || false;
    if (_.isUndefined(videoEl) || _.isUndefined(videoEl.canPlayType))
        return undefined;

    return {
        mp4: (videoEl.canPlayType("video/mp4") === "maybe" || videoEl.canPlayType("video/mp4") === "probably"),
        ogg: (videoEl.canPlayType("video/ogg") === "maybe" || videoEl.canPlayType("video/ogg") === "probably")
    };
};


function initHtml5Player(videoUrl) {
    isFlashVideoPlayer = false;
    //autobuffer and preload=auto are basically the same thing but browsers have different implementations of course
    var videoTag = $('<div id="player"><video id="playerPlugin" autobuffer preload="auto"><source src="' + videoUrl + '" type="video/mp4"></video></div>').appendTo(playerArea).find("video");
    videoTag.attr("controls", "true");
    videoTag.hover(function () {
        $(this).attr("controls", "true");
    }, function () {
        $(this).removeAttr("controls");
    });
    videoPlayer = videoTag[0];

    //NOTE: the buffers didn't seem to work perfectly.  like it claimed it wasn't fully buffered but would play to the very end.  so i do some trickery with the buffering

    videoTag.bind('durationchange', function () {
        onPlayerClipOpened();
        //chrome sometimes would cache the video so this allows us to mark it as downloaded and thus we can skip aroudn
        if (videoPlayer.buffered && videoPlayer.buffered.length)
            updateDownloadProgress(videoPlayer.buffered.end(0));
    }).bind('progress', function () {
        if (videoPlayer.buffered && videoPlayer.buffered.length) {//do this to avoid funky buffer changes foudn during testing
            var buffered = videoPlayer.buffered.end(0);
            if (isNaN(secondsDownloaded) || buffered > secondsDownloaded)
                updateDownloadProgress(buffered);
        }
    }).bind('error', function () {

    }).bind('pause', function () {
        updateIsPlaying(false);
    }).bind('play', function () {
        updateIsPlaying(true);
    }).bind('timeupdate', function () {
        updatePlayheadPosition(videoPlayer.currentTime);

        if (isNaN(secondsDownloaded) || videoPlayer.currentTime > secondsDownloaded) {
            updateDownloadProgress(videoPlayer.currentTime);
        }
    });
}

function initFlashPlayer(videoUrl) {
    //player container
    $('<div id="player"><div id="playerPlugin"></div><div id="playerPluginOverlay"></div></div>').appendTo(playerArea);

    isFlashVideoPlayer = true;
    videoPlayer = $.madPlayer({
        pluginId: "playerPlugin",
        api: true,
        pluginScript: "videoPlayer",
        "url": window.urlPrefixes.asset + "Assets/Player.swf",
        "params": {
            autoPlay: false,
            externalData: videoDetailsData.Id,
            clip: {
                _className: "mediascend.media::VideoClip",
                source: {
                    _className: "mediascend.media::ProgressiveDownloadSource",
                    url: videoUrl
                }
            }
        },
        width: "100%",
        height: "100%",
        onFlashEmbedded: function (e, d) {
            if (!d) {
                showPlayerPluginError();
            }
        },
        "onPlayheadPositionChanged": function (e, d) {
            updatePlayheadPosition(d[0]);
        },
        "onDownloadProgress": function (e, d) {
            updateDownloadProgress(d[0]);
        },
        "onClipOpened": onPlayerClipOpened,
        "onClipClosed": function () {
            isVideoClipOpen = false;
        },
        "onIsPlayingChanged": function (e) {
            updateIsPlaying(videoPlayer.isPlaying());
        }
    });

}

//shown when no flash or html5 support is available
function showPlayerPluginError() {
    playerArea.html(
                        '<div class="noFlash">' +
                        '<div>' +
                        '<img src="' + window.urlPrefixes.asset + 'img/warning64.png"></div>' +
                        '<div>' +
                        'To watch this video you need the latest version of Adobe Flash Player. Either you do not have it, it is an old version, or security permissions are preventing Flash from running.' +
                        '</div>' +
                        '<div>' +
                        '<a target="_blank" href="http://get.adobe.com/flashplayer/">Download the free Flash Player now!</a>' +
                        '</div>' +
                        '<div>' +
                        '<a target="_blank" href="http://get.adobe.com/flashplayer/">' +
                        '<img src="' + window.urlPrefixes.asset + 'img/get_flash_player.gif"></a></div>' +
                        '</div>');
}


function initPlayerTransport() {
    //when they click on teh video surface, pause it
    $("#playerPluginOverlay").click(function () {
        if (isPlaying) {
            videoPlayer.pause();
        }
    });
    playerTransport = $('<table class="playerTransport"><tr>' +
                        '        <td><a class="playBtn play disabled" href="#">Play/Pause</a> </td><td class="playheadTime"><div><span class="playheadTimecode">00:00</span>/<span class="durationTimecode">00:00</span></div></td><td class="timeline">' +
                        '        <div>' +
                        '            <div class="downloadProgress">' +
                        '            </div>' +
                        '            <div class="playheadProgress">' +
                        '                <div class="playheadFill">' +
                        '                </div>' +
                        '            </div>' +
                        '        </div>' +
                        '        </td>' +
                        '        <td><a class="volumeBtn disabled mute" href="#">Mute/Unmute</a> </td>' +
                        '</tr></table>').appendTo(playerArea);
    //'<td><a class="enlargeBtn disabled" title="' + enlargeTooltip + '" href="#">Enlarge</a> </td>' +

    playBtn = playerTransport.find(".playBtn");
    volumeBtn = playerTransport.find(".volumeBtn");
    enlargeBtn = playerTransport.find(".enlargeBtn");

    enlargeBtn.click(function (e) {
        onVideoEnlargeBtnClick(enlargeBtn);
        return false;
    });

    playheadTime = playerTransport.find(".playheadTime");
    playheadTimecode = playheadTime.find(".playheadTimecode");
    durationTimecode = playheadTime.find(".durationTimecode");
    var isMouseDownOnAddComment = false, isMouseOverPlayhead = false; //when true, the mouse was pressed down on the "Add Comment" button over the playhead.  this is necessary because it's not a simple click event as the slider will go to where the mouse was clicked (which since the button is 60px wide the mouse could go up a few secs before and after)
    var timeline = playerTransport.find(".timeline");
    downloadProgress = timeline.find(".downloadProgress");
    playheadProgress = timeline.find(".playheadProgress");
    playheadProgress.slider({
        disabled: true,
        step: secondsBetweenKeyframes, //seconds between keyframess
        start: function (event, ui) {
            isPlayheadDragging = true;
        },
        slide: function (event, ui) {
            if (isMouseDownOnAddComment) {
                return false;
            }

            //we only seek when we ar/e playing and the event hasn't been prevented (which timeline markers will do...see addCommentmarker)
            if (!isPlaying && $.exists(markerWithMouseDown)) {
                //we don't seek if we are currently in the marker that was clicked.  this was happening once per marker click which resulted in an unnecessary seek which is expensive
                var markerX = markerWithMouseDown.offset().left;
                var markerRight = markerX + markerWithMouseDown.outerWidth();
                //in testing the slide event happened once during mouse click.  if it happens in a marker, return false.  this will avoid a slight jumping of the playhead as the marker's click handler updates the position
                if (event.originalEvent.pageX >= markerX && event.originalEvent.pageX <= markerRight) {
                    return false;
                }
            }

            var seekTo = ui.value;
            var notifyUserIfCantSeek = isNaN(secondsDownloaded) || (seekTo - secondsDownloaded > 2);
            if (!canSeekTo(seekTo, notifyUserIfCantSeek))
                return false;

            //if we are playing, we don't update teh actual playhead position while sliding.  but we do show applicable comments
            if (!isPlaying) {
                seek(seekTo); //pla
            }
            playheadFill.width(getWidthPercentForTime(seekTo));
        },
        stop: function (event, ui) {
            isPlayheadDragging = false;
            //the mouse went down on the Add Comment button and now it's been released.  the mouseup is captured by the slider.  but if we return false here, the click event on the Add Comment button will fire.
            if (isMouseDownOnAddComment) {
                isMouseDownOnAddComment = false;
                return false;
            }
            if (!isMouseOverPlayhead)
                playheadTooltip.hide();
            if ($.exists(markerWithMouseDown)) {
                //if the slider ended up on the same marker the mouse went down, we cancel the stop event.  this will in turn cause a click on the marker.  the click event of the marker will then seek the player which will update the playhead at the exact spot it should be
                var markerX = markerWithMouseDown.offset().left;
                var markerRight = markerX + markerWithMouseDown.outerWidth();
                if (event.originalEvent.pageX >= markerX && event.originalEvent.pageX <= markerRight) {
                    markerWithMouseDown.trigger("click");
                    markerWithMouseDown = null; //no longer needed
                    return; //return nothing.  don't return false or else teh marker's click might fire twice
                }
                markerWithMouseDown = null; //no longer needed
            }
            if (!seek(ui.value, false, true))
                return false;
        },
        change: function (event, ui) {
            if ($.exists(playheadFill))
                playheadFill.width(getWidthPercentForTime(ui.value));
        }
    });
    //remove the default key press handlers because we have hotkeys that vary according to key combo (ctrl+arrow).  instead of handling keystrokes at the slider, we handle them at the document level.  the slider was preventing the ability to skip to next or prev comments.  this hack is a result of the plugin's inability to shut off key responses
    playheadProgress.find(".ui-slider-handle").unbind("keydown keyup");

    playheadProgress.attr("class", "playheadProgress"); //wipe out the styles from jqueryui because we use our own
    playheadThumb = playheadProgress.find("a"); //created by slider plugin
    playheadThumb.attr("class", "playheadThumb disabled");

    playheadTooltip = $("<div></div>").appendTo(playheadThumb).hide();
    if (productionData.CanAddComment) {
        //have to create this button now and not in markup because it's an A tag and would screw up the slider.
        addCommentBtnInPlayhead = $('<a>Add a Comment Here</a>').appendTo(playheadTooltip).click(function (e) {
            isMouseDownOnAddComment = false;
            onCommentAddBtnClick(e);
        });

        addCommentBtnInPlayhead.mousedown(function () {
            isMouseDownOnAddComment = true;
        });
        playheadThumb.hover(function () {
            isMouseOverPlayhead = true;
            if (!playheadTooltip.is(":visible"))
                playheadTooltip.show();
        }, function () {
            isMouseOverPlayhead = false;
            if (!isPlayheadDragging)
                playheadTooltip.hide();
        });
    }
    playheadTooltipTimecode = $("<span></span>").prependTo(playheadTooltip);
    playheadFill = playheadProgress.find("div.playheadFill");

    playBtn.click(_playBtn_click);
    volumeBtn.click(_volumeBtn_click);

}


function onVideoEnlargeBtnClick(enlargeBtn) {
    var player = $("#player");

    setIsMainAreaFullWidth(!isVideoEnlarged);

    if (!isVideoEnlarged) {
        //when you click the enlarge button, we have to resize the #mainArea, not the player div.  to do that, just compute the new desired player height from the natural video size (taken from player metadata) and add the difference to #mainArea
        var videoWidth = 0, videoHeight = 0;
        if (isFlashVideoPlayer) {
            var size = videoPlayer.getFlashProperty("videoSize");
            if ($.hasChars(size)) {
                var widthAndHeight = size.split('_');
                videoWidth = parseInt(widthAndHeight[0]);
                videoHeight = parseInt(widthAndHeight[1]);
            }
        }
        else {
            videoWidth = videoPlayer.videoWidth;
            videoHeight = videoPlayer.videoHeight;
        }
        if (videoWidth > 0 && videoHeight > 0) {
            var desiredVideoHeight = mainArea.width() * (videoHeight / videoWidth);
            player.height(desiredVideoHeight);
            enlargeBtn.addClass("selected");
            enlargeBtn.attr("title", "Make Video Smaller");
        }
    }
    else {
        player.css({ height: "" });
        enlargeBtn.removeClass("selected");
        enlargeBtn.attr("title", enlargeTooltip);
    }
    isVideoEnlarged = !isVideoEnlarged;
}

//clears the video player only.  doesn't clear comments.  also doesn't change videoDetailsData
function resetVideoPlayer() {
    if (videoPlayer && isFlashVideoPlayer) {
        videoPlayer.dispose();
        videoPlayer = undefined;
    }
    $("#player-container").empty();

    isFlashVideoPlayer = undefined;
    playerArea = undefined;
    playerTransport = undefined;
    playBtn = undefined;
    volumeBtn = undefined;
    downloadProgress = undefined;
    playheadTime = undefined;
    playheadProgress = undefined;
    playheadFill = undefined;
    playheadThumb = undefined;
    playheadTooltip = undefined;
    addCommentBtnInPlayhead = undefined;
    playheadTimecode = undefined;
    durationTimecoded = undefined;
    playheadTooltipTimecoded = undefined;
    clipDuration = undefined;
    secondsDownloaded = undefined;
    isVideoClipOpen = false;
    isPlaying = false;
    playheadPositiond = undefined;
    markerWithMouseDownd = undefined;
    isPlayheadDragging = false;
    videoEditForm = undefined;

}


//called only once...this sets up hot keys for video player
function initHotKeys() {

    $(document).keydown(function (e) {
        //if there is no video shown then don't do a thing
        if (!isVideoClipOpen || !isVideoDetailsOpen || !isVideosTabOpen())
            return;
        var target = $(e.target);
        while (target.hasElements()) {
            if (target.is("form")) {
                return;
            }
            target = target.parent();
        }

        switch (e.which) {
            case 32: //space bar
                if (isPlaying) {
                    videoPlayer.pause();
                }
                else {
                    videoPlayer.play();
                }
                return false;
                //case 38: //up arrow
            case 39: //right arrow
                if (!isPlaying) {
                    if (true === e.ctrlKey || true === e.shiftKey) {
                        var nextMarkerPosition = nextSecondWithComments(playheadPosition);
                        if (!isNaN(nextMarkerPosition)) {
                            seek(getFirstCommentTimeInSecond(nextMarkerPosition), true, true);
                        }
                    }
                    else {
                        seek(playheadPosition + secondsBetweenKeyframes);
                    }
                }

                break;
            case 37: //left arrow
                //case 40: //down arrow
                if (!isPlaying) {
                    if (true === e.ctrlKey || true === e.shiftKey) {
                        var prevMarkerPosition = prevSecondWithComments(playheadPosition);
                        if (!isNaN(prevMarkerPosition)) {
                            seek(getFirstCommentTimeInSecond(prevMarkerPosition), true, true);
                        }
                        else {
                            seek(0); //no marker to the left so we shoot to the beginning.  note we don't do this the other way, where you might skip to the end.  we saw no need for that
                        }
                    }
                    else {
                        seek(playheadPosition - secondsBetweenKeyframes);
                    }
                }
                break;
        }
    });
}


//when the user presses pause, we jump to any current comment if there is one
function updateIsPlaying(playing) {
    isPlaying = playing;
    if (isPlaying) {
        playBtn.removeClass("play").addClass("pause");
    }
    else {
        playBtn.removeClass("pause").addClass("play");
    }
}

function canSeekTo(time, notifyUserIfCantSeek) {
    var can = !isNaN(secondsDownloaded) && time <= secondsDownloaded;
    if (!can && true === notifyUserIfCantSeek) {
        notification.warning("Just a minute please.  You have to wait until the video has loaded up to this point before you can view it.");
    }
    return can;
}

//seeks to a new point in the video if it's possible
function seek(seconds, openCurrCommentsAfterSeek, notifyUserIfCannotSeek) {
    if (_.isUndefined(openCurrCommentsAfterSeek)) {
        openCurrCommentsAfterSeek = false;
    }
    if (canSeekTo(seconds, notifyUserIfCannotSeek) && seconds !== playheadPosition) {
        openCurrCommentsOnNextUpdate = openCurrCommentsAfterSeek;
        if (isFlashVideoPlayer)
            videoPlayer.seek(seconds);
        else
            videoPlayer.currentTime = seconds;
        return true;
    }
    else {
        openCurrCommentsOnNextUpdate = false;
        return false;
    }
}

function updatePlayheadPosition(seconds) {
    var oldPosition = playheadPosition;
    playheadPosition = seconds;
    var timecode = $.timeCode(playheadPosition, false, true, 0);
    playheadTimecode.text(timecode);
    playheadTooltipTimecode.text(timecode);
    if (!isPlayheadDragging) {
        playheadProgress.slider("value", playheadPosition);
    }
    $(document).trigger("playheadUpdated");
}

function updateDownloadProgress(seconds) {
    secondsDownloaded = seconds;
    downloadProgress.width(getWidthPercentForTime(secondsDownloaded));
}



//called by startup script within the markup.  This handles the clipOpening event for the video player.  Here we add the comments to the player from commentsData
function onPlayerClipOpened() {
    isVideoClipOpen = true;
    playheadThumb.removeClass("disabled");
    playBtn.removeClass("disabled");
    volumeBtn.removeClass("disabled");
    enlargeBtn.removeClass("disabled");
    if (addCommentBtnInPlayhead)
        addCommentBtnInPlayhead.removeClass("disabled");
    clipDuration = isFlashVideoPlayer ? videoPlayer.getFlashProperty("duration") : videoPlayer.duration;
    playheadProgress.slider("option", "disabled", false);
    playheadProgress.slider("option", "max", clipDuration);
    durationTimecode.text($.timeCode(clipDuration, false, true, 0));

    //add the comment markers to the timeline
    var comments = getComments();
    if (videoPlayer && comments && comments.length > 0) {
        //comments are already added via addCommentToUi, but the markers had to wait until now.  after this markers get set right away
        $.each(commentsPerSecond, function (key, value) {
            addCommentMarker(getFirstCommentTimeInSecond(parseInt(key)));
        });
    }
    updatePlayheadPosition(0);

    //the timecode switch in the comment form is only shown when teh video clip is open, so if the form was opened before the clip became available (happens on new videos or slow download), show it
    if (isCommentFormOpen && $.isDefined(commentFormIsTimedField)) {
        commentFormIsTimedField.parent().show(); //just in case it was hidden before
    }
}


//adds the given jquery element as a marker at the given time
function addTimelineMarker(element, time) {
    element.css("left", getWidthPercentForTime(time));

    playheadProgress.append(element);

    element.mousedown(function (e) {
        markerWithMouseDown = element;
    });
}

//gets the width from the left of the playhead slider for a given time in the video
function getWidthPercentForTime(time) {
    if (!$.isPositiveNumber(clipDuration) || !$.isPositiveNumber(time))
        return "0%";

    return (100 * (time / clipDuration)).toString() + "%"; //this can generate fractional output like 15.323421% but that is okay
}


function _playBtn_click(e) {
    if (!isVideoClipOpen)
        return false;

    if (isPlaying) {
        videoPlayer.pause();
    }
    else {
        videoPlayer.play();
    }

    return false;
}


function _volumeBtn_click(e) {
    if (!isVideoClipOpen)
        return false;

    var isMuted = videoPlayer.getFlashProperty("muted");
    if (isMuted) {
        volumeBtn.removeClass("unmute").addClass("mute");
    }
    else {
        volumeBtn.removeClass("mute").addClass("unmute");
    }

    videoPlayer.setFlashProperty("muted", !isMuted);

    return false;
}


//creates the title, video info, download, edit, and delete buttons as necessary
function createVideoTitleArea(video) {

    var titleArea = $('<div id="video-details-title"></div>');
    var breadCrumb = $('<ul class="breadcrumb"><li><a id="video-details-title-videos-button" href="#">Videos</a><span class="divider">/</span></li><li class="active"><span id="video-details-title-text"></span><a class="icon-info-sign" id="video-details-title-tooltip-anchor" href="#"></a></li></ul>').appendTo(titleArea);
    breadCrumb.find("#video-details-title-text").text(video.Title);
    breadCrumb.find("#video-details-title-videos-button").click(function () {
        openAllVideos();
        return false;
    });

    var created = new Date(video.CreatedOn);
    breadCrumb.find('#video-details-title-tooltip-anchor').popover(
        {
            title: 'Video Information',
            content: 'Uploaded by ' + ($.exists(video.Creator) ? video.Creator.Name : "N/A") + " on " + moment(video.CreatedOn).format('LLL')
        });

    var options = $('<div class="btn-group" id="video-details-title-options"><button data-toggle="dropdown" class="btn dropdown-toggle"><i class="icon-cog"></i> Options <span class="caret"></span></button><ul class="dropdown-menu"></ul></div>').appendTo(titleArea);

    var optionMenu = options.find('ul');
    if (video.IsSourceDownloadable) {
        var url = window.urlPrefixes.relative + "videos/" + video.Id + "/download"; //for whatever reason, adding productions/{id} didn't work right
        optionMenu.append($('<li><a href="' + url + '" class="download">Download</a></li>'));
    }
    $('<li><a href="#" class="print">Print</a></li>').appendTo(optionMenu).find("a").click(function () {
        printVideo();
        return false;
    });
    
    if (video.CanEdit) {
        $('<li><a href="#" class="edit">Edit</a></li>').appendTo(optionMenu).find("a").click(function () {
            if (_.isUndefined(videoEditForm)) {
                createVideoEditForm();
            }
            if (!videoEditForm.is(":visible")) {
                $("#video-details-title").hide();
                videoEditForm.show();
            }
            options.removeClass('open'); //close dropdown
            return false;
        });
    }
    if (video.CanDelete) {
        $('<li><a href="#" class="delete">Delete</a></li>').appendTo(optionMenu).find("a").click(function () {
            onVideoDeleteBtnClick(video);
            options.removeClass('open'); //close dropdown
            return false;
        });
    }
    return titleArea;
}




//gets the videoEditForm, possibly creating it
function createVideoEditForm() {
    videoEditForm = $('<form id="video-edit-form" class="form-horizontal"></form>');

    var titleInput = controlGroup(
        {
            label: 'Video Title',
            inputClass: 'span4',
            inputName: 'title',
            value: videoDetailsData.Title
        }).appendTo(videoEditForm).find('input');

    var downloadableInput;
    if (videoDetailsData.HasSource) {
        downloadableInput = controlGroup(
            {
                label: 'Downloadable',
                inputName: 'IsDownloadable',
                value: videoDetailsData.IsSourceDownloadable,
                tooltip: 'When turned on, this makes it possible for anyone who watches the video to download the video to their computer.  The file will be the same size and format as the original.'
            }).appendTo(videoEditForm).find('input');
    }

    var customUrlRegex = /^(?:(?![^a-zA-Z0-9_-]).)*$/i;
    var customUrlInput = controlGroup(
    {
        label: 'Custom Url',
        inputClass: 'span2',
        inputName: 'CustomUrl',
        prepend: 'http://takeoffvideo.com/',
        value: videoDetailsData.StandaloneCustomUrl,
        tooltip: 'A custom web address for this video makes it easier to find and share this video, isolated from the entire production.  For example, enter "mymovie" and this video can be found at http://takeoffvideo.com/mymovie.  This is typically used along with the "Guest Password" below.'
    }).appendTo(videoEditForm).find('input').keyfilter(customUrlRegex);

    var guestAccessGroup = controlGroup(
        {
            label: 'Guest Password',
            inputClass: 'span3',
            inputName: 'GuestPassword',
            value: videoDetailsData.GuestPassword,
            tooltip: 'Setting this password lets people access the video, isolated from the entire production, without creating a Takeoff account.  You can optionally allow those people to add comments.  This is typically used along with the "Custom Url" above.'
        }).appendTo(videoEditForm);
    var guestPassword = guestAccessGroup.find("input[name='GuestPassword']");
    var guestsCanCommentContainer = $('<label id="Video-EditForm-GuestsCanComment-Container" class="checkbox"><input type="checkbox" name="GuestAccessAllowCommenting" value="true" />Guest can Comment</label>').appendTo(guestAccessGroup.find("div.controls"));
    var guestsCanComment = guestsCanCommentContainer.find("input");
    if (videoDetailsData.GuestsCanComment === true) {
        guestsCanComment.attr("checked", "checked");
    }
    guestPassword.bind('hastext', function () {
        guestsCanCommentContainer.css("visibility", "visible");
    }).bind('notext', function () {
        guestsCanCommentContainer.css("visibility", "hidden");
    });
    if ($.hasChars(guestPassword.val())) {
        guestsCanCommentContainer.css("visibility", "visible");
    } else {
        guestsCanCommentContainer.css("visibility", "hidden");
    }

    videoEditForm.append('<div class="form-actions"><input type="submit" value="Update" class="btn btn-primary" /><a href="#" class="btn">Cancel</a></div>');

    videoEditForm.madValidate({
        url: function () {
            return window.urlPrefixes.relative + "productions/" + productionData.Id + "/videos/" + videoDetailsData.Id + "/edit";
        },
        rules:
        {
            title: "required"
        },
        data: function () {
            return {
                Title: titleInput.val(),
                IsDownloadable: videoDetailsData.HasSource ? downloadableInput.is(":checked") : null,
                CustomUrl: customUrlInput.val(),
                GuestPassword: guestPassword.val(),
                GuestsCanComment: guestsCanComment.is(":checked")
            };
        },
        submitSuccess: function (result) {
            videoDetailsData.Title = titleInput.val();
            if (videoDetailsData.HasSource) {
                videoDetailsData.IsSourceDownloadable = downloadableInput.is(":checked");
            }
            videoEditForm.hide();
            $("#video-details-title").replaceWith(createVideoTitleArea(videoDetailsData));
            updateVideoTitle(videoDetailsData.Id, videoDetailsData.Title);
        }
    });

    videoEditForm.find("a").click(function () {
        videoEditForm.hide();
        $("#video-details-title").show();
        return false;
    });

    $("#video-details-title").after(videoEditForm);
}

//updates the title of the given video
function updateVideoTitle(videoId, title) {
    //if the video is being watched, update the title
    if ($.isDefined(videoDetailsData) && videoDetailsData.Id === videoId) {
        $("#video-details-title h3").text(title);
    }

    //video in #videoslist
    $("#video" + videoId.toFixed(0) + " strong").text(title);

    //update the video title in the activity panel
    //in activity items, video titles are all clickable with hrefs starting with #video_{id}...ex:  href="#video_1234".  
    var hrefStart = "video_" + videoId.toFixed(0);
    $("#activity-container a").each(function () {
        var $this = $(this);
        var href = $this.attr("href");
        if (($.startsWith(href, "#") || $.startsWith(href, top.location.href + "#")) && $.afterStr(href, "#") === hrefStart) {
            $this.text(title);
        }
    });

    getVideoListItems().equalHeights();
}


function printVideo() {
    if (_.isUndefined(videoDetailsData)) {
        alert('No video loaded.');
        return;
    }

    var commentModels = getCurrentCommentModels();
    if (_.isUndefined(commentModels) || commentModels.length == 0) {
        notification.error("Sorry but there are no comments to print.");
        return;
    }
    
    var rootView = $("<div class='wrapper'></div>");

    $("<h1></h1>").text(videoDetailsData.Title).appendTo(rootView);

    _.each(commentModels, function (commentModel) {
        var commentView = $('<div class="comment"></div>');
        var infoView = $('<div class="info"></div>').appendTo(commentView);
        if (_.isNumber(commentModel.StartTime)) {
            var timecode = $.timeCode(commentModel.StartTime, false, false, 1);
            $('<span class="timecode" />').text(timecode).appendTo(infoView);
        }
        if ($.exists(commentModel.Creator)) {
            $('<span class="creator" />').text(commentModel.Creator.Name).appendTo(infoView);
        }
        $('<span class="created-on" />').text(moment(commentModel.CreatedOn).format('LLL')).appendTo(infoView);

        $('<p class="body"></p>').text(commentModel.Body).appendTo(commentView);

        //todo: you should use the commentmodel.replies, but right now Replies is only for initial data, because if a reply is added or dleeted the model isn't synced.  once you fix that you are good to use model.Replies again
        if ($.isArrayWithItems(commentModel.Replies))
        {
            var repliesView = $('<div class="replies"></div>').appendTo(commentView);
            _.each(commentModel.Replies, function (replyModel)
            {
                var replyView = $('<div class="reply"></div>').appendTo(repliesView);
                var infoView = $('<div class="info"></div>').appendTo(replyView);
                if ($.exists(replyModel.Creator)) {
                    $('<span class="creator" />').text(replyModel.Creator.Name).appendTo(infoView);
                }

                $('<span class="created-on" />').text(moment(replyModel.CreatedOn).format('LLL')).appendTo(infoView);

                $('<p class="body"></p>').text(replyModel.Body).appendTo(replyView);
            });
        }

        commentView.appendTo(rootView);
    });
    rootView.printArea(
        {
            popTitle: videoDetailsData.Title,
            cssUrl: printCssUrl
            //iframeStyle: 'border:0;position:absolute;width:600px;height:600px;left:0px;top:0px;' 
        });
}


