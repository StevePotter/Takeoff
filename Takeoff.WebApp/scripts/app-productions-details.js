/// <reference path="jquery-1.6.2-vsdoc.js" />
/// <reference path="swfobject.debug.js" />
/// <reference path="takeoff-helpers.js" />
/// <reference path="takeoff-tooltip.js" />
/// <reference path="takeoff-forms.js" />
/// <reference path="takeoff-tabs.js" />
/// <reference path="date.Relative.js" />
/// <reference path="ProductionView.VideoDetails.js" />
/// <reference path="ProductionView.Videos.js" />
/// <reference path="ProductionView.Files.js" />
/// <reference path="ProductionView.Comments.js" />
/// <reference path="ProductionView.Members.js" />
/// <reference path="ProductionView.Settings.js" />

/**
* Copyright (c) 2009 Mediascend, Inc.
* http://www.mediascend.com
*/
var tooltips, mainAreaPanels;
var productionData; //initial data for loading the page
var initialFocus; //used when a link to an element like comment or somethign that will be highlighted is passed in 
var videoPlayer;
var mainPanelTabs;
var notification;
var localNav;
var mainArea;

function initProductionUi() {
    mainArea = $("#mainArea");
    notification = $("body").madNotification({ api: true });
    mainAreaPanels = $("#mainArea > div");
    localNav = $("#divLocalNav");
    tooltips = $(document).madTooltips({ api: true });

    $("span.tooltip-anchor").tooltip();

    initSettings(); //before tabs so when focus is "settings", the tab open events will fire and init this on demand

    initTabs();

    initComments();

    initMembers();

    initVideos();

    initFiles();

    initHotKeys();

    initActivity();

    updateRelativeDates();

    if ($.exists(productionData.CurrentVideo)) {
        loadVideo(productionData.CurrentVideo, true, function () {
            if ($.hasChars(initialFocus)) {
                sendToItem(initialFocus);
            }
        });
    }
    if ($.exists(window.message)) {
        notification[window.message.type](window.message.text);
    }


};

//sets up the Videos, Assets, etc panes, which are controlled by the mainPanelTabs tabs plugin
//you should consider adding init functions for various tabs here so document load time could be shortened
function initTabs() {
    var initialIndex = 0;
    //crappy hack for the initial tab
    if ($.hasChars(initialFocus) && $.startsWith(initialFocus, "settings")) {
        initialIndex = 3;
    }


    //the madTabs plugin that manages the show/hide of the Files/Users/Versions panes.
    mainPanelTabs = $("#mainTabs").madTabs({
        tabSelector: "li",
        panesContainer: "#mainArea",
        paneSelector: "div.tabPane:lt(4)", //don't include the last pane, which holds the video player.  this allows us to do a little trickery with the videos tab (which has two different panes)
        initialIndex: initialIndex,
        api: true,
        openTabClosable: false,
        tabOpening: function (e, args) {
            setIsMainAreaFullWidth(false);
            e.preventDefault();
            args.tabToOpen.addClass("active");
            args.paneToOpen.addClass("openTabPane");
            args.tabToOpen.trigger(e, args);
        },
        tabClosing: function (e, args) {
            e.preventDefault();
            args.tabToHide.removeClass("active");
            args.paneToHide.removeClass("openTabPane");
            args.tabToHide.trigger(e, args);
        },
        onTabOpenEvent: function (e, args) {
            if (args.tab.attr("id") == "videos-tab" && isVideosTabOpen()) {
                onVideoTabReclick();
            }
        }
    });
}


//*********************** Window unload handler to warn users when they close a page with an upload in progress ********************
//this could become a useful plugin one day

var uploadsInProgress = {};

function isUploadInProgress() {
    var hasProps = false;
    $.each(uploadsInProgress, function () {
        hasProps = true;
        return false;
    });
    return hasProps;
}

function setUploadStart(pluginName) {
    uploadsInProgress[pluginName] = true;

    if (!$.exists(window.onbeforeunload)) {
        //no jquery support for onbeforeunload...no big deal
        window.onbeforeunload = function () { return 'Leaving or refreshing this page right now will cancel your upload.'; };
    }
}
function setUploadStop(pluginName) {
    delete uploadsInProgress[pluginName];
    if (!isUploadInProgress())
        window.onbeforeunload = null;
}

//*********************** Util ********************

function isSemiAnonymousUserWhoCanComment() {
    return productionData.CanAddComment === true && !productionData.IsMember;
}

//when true, #mainArea is all teh way across the main container, where the side bar is on the right side.  when true, the side bar gets pushed down
var isMainAreaFullWidth = false;

function setIsMainAreaFullWidth(val) {
    if (isMainAreaFullWidth === val)
        return;

    isMainAreaFullWidth = val;
    if (val) {
        mainArea.removeClass("grid_8").addClass("grid_12");
    }
    else {
        mainArea.removeClass("grid_12").addClass("grid_8");
    }
}

