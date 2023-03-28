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
*/

//*********************** Videos *****************
var videosList, videosTab, videosTabBtn;

//indicates whether a specific video is currently loaded.  otherwise the videos panel will be shown, unless of course the Assets Team or Settings tabs are visible
var isVideoDetailsOpen = false;

function initVideos() {
    videosList = $("#videosList");

    //tracks the file objects as they go back and forth with server (server will add signatures and form post variables)
    videosTab = $("#videos-tab");
    videosTabBtn = $("#videos-tab a");
    videosTabBtn.data("text", videosTabBtn.text());

    videosTab.bind("tabOpening", function (e, args) {
        if (isVideoDetailsOpen) {
            setIsMainAreaFullWidth(isVideoEnlarged);

            $("#videosArea").removeClass("openTabPane");
            $("#video-details-container").addClass("openTabPane");
            $("#commentsArea").show();
        }
        else {
            setIsMainAreaFullWidth(false);

            $("#video-details-container").removeClass("openTabPane");
            $("#videosArea").addClass("openTabPane");
            $("#commentsArea").hide();
        }
    });

    videosTab.bind("tabClosing", function (e, args) {
        if (isPlaying)
            videoPlayer.pause();

        if (isVideoDetailsOpen) {
            $("#video-details-container").removeClass("openTabPane");
            $("#commentsArea").hide();
        }

    });

    addVideosToList(productionData.Videos, false); //load the initial data because it is not in the html

    $("#video-add-button").click(function () {
        createVideoUploadForm();
        return false;
    });
}

function createVideoUploadForm() {
    var form = $('<form class="well upload form-horizontal"></form>');

    //this is used to handle the case where someone uploads a file and leaves.  normally they should fill out the rest of the form and click add, but they might not and in that case they would expect the video to have posted.  this takes care of that, automatically submitting the form after some time of inactivity

    //if the file was uploaded and the user does something in the form, cancel the auto submit
    form.on('click keydown keypress change focus', function (event) {
        var autosubmitTimer = form.data("autosubmitTimer");
        if (autosubmitTimer && autosubmitTimer.isRunning())
            autosubmitTimer.cancel();
    });

    var fileGroup = form.append($.controlGroup(
        {
            tooltip: 'Press the button to upload one or more videos in our upload popup.  You can upload files from your computer or use other sources like Dropbox.',
            label: 'File',
            input: function () {
                return $("<button class='btn upload-btn'>Select File(s)</button>")
            }
        }));
    
    form.append($.controlGroup(
        {
            label: 'Video Title',
            inputClass: 'span4',
            inputName: 'title',
            value: ''
        }));
    form.append($.controlGroup(
        {
            label: 'Downloadable',
            inputName: 'downloadable',
            value: false,
            tooltip: 'When turned on, this makes it possible for anyone who watches the video to download the video to their computer.  The file will be the same size and format as the original.'
        }));

    form.append($.controlGroup(
            {
                label: 'Notes',
                inputClass: 'span4',
                inputName: 'notes',
                type: 'textarea',
                value: '',
                tooltip: 'Video Notes will appear as the first entry in the Comments section.'
            }));

    form.append($('<div class="form-actions"><input type="submit" class="btn btn-primary" value="Add" /> <a href="#" class="cancel btn">Cancel</a></div>'));

    form.find("a.cancel").click(function () {
        form.remove();
        return false;
    });

    form.find('.upload-btn').click(function () {
        filepicker.pickAndStore(
                {
                    services: ['COMPUTER', 'FTP', 'DROPBOX', 'FACEBOOK', 'GOOGLE_DRIVE', 'BOX', 'URL', 'VIDEO'],
                    openTo: 'COMPUTER',
                    maxSize: 20 * 1073741824,
                    multiple: true
                },
                { location: "S3" },
                function (fpfiles) {
                    if (_.isArray(fpfiles) && fpfiles.length > 0) {
                        for (var i = 0; i < fpfiles.length; i++) {
                            var targetForm = i == 0 ? form : createVideoUploadForm(); //create another form for additional files
                            addFileToForm(fpfiles[i], targetForm);
                        }
                    }
                },
                function (fpError) {

                });
        return false;
    });

    form.madValidate({
        url: function () {
            return window.urlPrefixes.relative + "productions/" + productionData.Id + "/videos/create";
        },
        data: function () {
            var data = form.formToObject();
            data.downloadable = form.find("input[name='downloadable']").is(":checked"); //formtoojbect sucks with checkboxes and uses "on" instead of true.  this makes that easier
            return data;
        },
        onfocusout: false,
        onkeyup: false,
        rules:
                {
                    title: "required"
                },
        beforeSubmit: function (result) {
            var autosubmitTimer = form.data("autosubmitTimer");
            if (autosubmitTimer && autosubmitTimer.isRunning()) {
                autosubmitTimer.cancel();
            }
            var fileKey = form.find('input[name="fileKey"]').val();
            if (!_.isString(fileKey) || fileKey.length == 0) {
                $.createOkModal({
                    heading: "Missing File",
                    body: "Please select a file by pressing Select File.",
                    okText: "OK"
                }).modal({
                    show: true,
                    backdrop: true,
                    keyboard: true
                });
                return false;
            }
            return true;
        },
        submitSuccess: function (result) {
            var showDetails = form.data("detailsOnSubmit");
            form.remove();
            var videoData = result.Data;
            addActivityItem(result.ActivityPanelItem, false);
            if ($.exists(result.CommentActivityPanelItem)) {
                addActivityItem(result.CommentActivityPanelItem, false);
            }
            productionData.Videos.push(videoData);
            addVideosToList([videoData], true);
            if (showDetails)
                loadVideo(videoData);
        }
    });

    form.data("detailsOnSubmit", true);
    $("#video-add").append(form);
    return form;
}



function addFileToForm(file, targetForm) {
    var hasFileName = _.isString(file.filename);
    if (_.isString(file.key))
        $("<input type='hidden' name='fileKey' />").val(file.key).appendTo(targetForm);
    if (hasFileName)
        $("<input type='hidden' name='fileName' />").val(file.filename).appendTo(targetForm);
    if (file.size)
        $("<input type='hidden' name='fileSize' />").val(file.size).appendTo(targetForm);

    //show file name and use file name as title if a title hasn't been entered
    targetForm.find('button.upload-btn').replaceWith($('<span class="uneditable-input"></span>').text(hasFileName ? file.filename : 'Done!'));
    var titleInput = targetForm.find('input[name="title"]');
    if (titleInput.val().length == 0 && hasFileName) {
        titleInput.val(file.filename);
    }
    //this is used to handle the case where someone uploads a file and leaves.  normally they should fill out the rest of the form and click add, but they might not and in that case they would expect the video to have posted.  this takes care of that, automatically submitting the form after some time of inactivity
    var autosubmitTimer = new $.timer({
        delay: (1000 * 60 * 5),
        operation: function () {
            targetForm.data("detailsOnSubmit", false);
            targetForm.submit(); 
        }
    });
    targetForm.data("autosubmitTimer", autosubmitTimer)
    autosubmitTimer.start();
}

//called by tab plugin when teh Videos tab gets clicked when the tab is already open.  here we exit the Video Details to All Videos if possible
function onVideoTabReclick() {
    openAllVideos();
}

//opens the tab that contains the videosList and videoDetails
function openVideosTab() {
    if (!isVideosTabOpen())
        mainPanelTabs.open(videosTab);
}


//if the "Videos" tab is open.  This includes the videosList as well as video Details.  See isVideoDetailsOpen for that.
function isVideosTabOpen() {
    return mainPanelTabs.getOpenTab().attr("id") === "videos-tab";
}



//if a video is being shown, this will open op the "videos list".  this does NOT open the tab for the Videos
function openAllVideos() {
    setIsMainAreaFullWidth(false);

    if (isVideoDetailsOpen) {
        if (isPlaying)
            videoPlayer.pause();
        $("#videosArea").addClass("openTabPane");
        $("#video-details-container").removeClass("openTabPane");
        $("#commentsArea").hide();
        isVideoDetailsOpen = false;
    }
}

//opens the video details for teh current video, which includes comments and the video player.  does NOT open the tab for videos
function openVideoDetails() {
    setIsMainAreaFullWidth(isVideoEnlarged);

    if (!isVideoDetailsOpen) {
        isVideoDetailsOpen = true;
        //the videos panel was showing so switch over to the video details
        if (mainPanelTabs.getOpenTab().attr("id") === "videos-tab") {
            $("#videosArea").removeClass("openTabPane");
            $("#video-details-container").addClass("openTabPane");
            $("#commentsArea").show();
        }
    }
}


function onVideoDeleteBtnClick(videoData) {
    var message = "Are you sure you want to delete this video?  This cannot be undone.";
    if (confirm(message)) {
        $.madAjax({
            url: function () {
                return window.urlPrefixes.relative + "productions/" + productionData.Id + "/videos/" + videoData.Id + "/delete";
            },
            data: function () {
                return {
                    id: videoData.Id
                };
            },
            success: function () {
                deleteVideoFromUi(videoData.Id, false);
            }
        });
    }
}

function getVideoData(videoId) {
    return $("#video" + videoId.toFixed(0)).data("data");
}



//adds the given array of videos to #videosList
//each video will have an LI with an id of "video{id}", such as video12311.  The LI also has the video data in data("data")
function addVideosToList(videosData, sortAfterInsert) {
    if ($.isArrayWithItems(videosData)) {
        $.each(videosData, function () {
            videosList.append(createVideoListItem(this));
        });
    }

    var items = getVideoListItems();

    //videos are sorted by the date they were added
    if (sortAfterInsert) {
        items.sortElements(function (a, b) {
            var videoA = $(a).data("data");
            var videoB = $(b).data("data");
            return videoB.CreatedOn - videoA.CreatedOn;
        });
    }
    checkForEmptyVideosList();
    items.equalHeights();
}

function getVideoListItems() {
    return videosList.find("li");
}

function createVideoListItem(video) {
    var newItem = $('<li id="video' + video.Id.toFixed(0) + '"><a href="#"><strong></strong></a></li>').data("data", video);
    var anchor = newItem.find("a").click(function () {
        loadVideo(video.Id);
        return false;
    });
    newItem.find("strong").text(video.Title);
    var thumbs = video.Thumbnails;
    if ($.isArrayWithItems(thumbs)) {
        var firstThumb = thumbs[0];
        var images = $('<div class="thumbs" style="width:' + firstThumb.Width + 'px;"><img src="' + firstThumb.Url + '" style="width:' + firstThumb.Width + 'px;height:' + firstThumb.Height + 'px" /></div>');
        var cycled = false;
        anchor.append(images).hover(function () {
            if (!cycled) {
                //to avoid a ton of image downloads when the page loads, we only get the rest of the thumbnails when the mouse first goes over
                $.each(thumbs, function (index) {
                    if (index > 0) {
                        images.append($('<img src="' + this.Url + '" style="display:none;width:' + this.Width + 'px;height:' + this.Height + 'px" />'));
                    }
                });
                images.cycle({ timeout: 800, pause: 0 });
                cycled = true;
            }
            else {
                images.cycle('resume');
            }
            images.cycle('next'); //always move directly to the next slide on hover because 1) to indicate that hovering cycles.  2) they probably already saw the current thumbnail cuz it was sittin gthere
        }, function () {
            if (cycled)
                images.cycle('pause');
        });
    }
    else {
        anchor.append('<div class="noThumbs"></div>');
    }

    if ($.exists(video.Creator)) {
        anchor.append($("<div></div>").text("by " + video.Creator.Name));
    }

    var created = new Date(video.CreatedOn);
    var date = $('<div><em class="relativeDate"></em></div>').appendTo(anchor).find("em").data("date", created).attr("title", moment(created).format('LLL')).text(toRelativeString(created));
    return newItem;

}


//if the videosList is empty, this shows the "no videos" message
function checkForEmptyVideosList() {
    if (videosList.find("li:first").hasElements()) {
        videosList.show();
        $("#videosListEmptyMsg").hide();
    }
    else {
        videosList.hide();
        $("#videosListEmptyMsg").show();
    }
}

//call when you want to delete a video with the given ID from the activity panel and videosList
//if the video is loaded in the video player, it will be closed out 
function deleteVideoFromUi(videoId, notify) {
    deleteActivityItem(videoId);
    videosList.find("#video" + videoId.toFixed(0)).remove();
    checkForEmptyVideosList();

    if (isVideoDetailsOpen && $.isDefined(videoDetailsData) && videoDetailsData.Id === videoId) {
        resetVideoPlayer();
        resetComments();
        videoDetailsData = undefined;
        openAllVideos();
        if (notify)
            notification.warning("The video you were just watching has been deleted.", 5000);
    }
}