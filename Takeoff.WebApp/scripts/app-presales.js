/// <reference path="jquery-1.6.2-vsdoc.js" />
/// <reference path="takeoff-helpers.js" />
/// <reference path="takeoff-forms.js" />
/// <reference path="takeoff-tabs.js" />

$.extend(window.views, {
    //js for / (home)
    home: function (options) {
        $("#quotes").quote_rotator({ rotation_speed: 5000 }); //tried to use cycle plugin but it didn't work well.  quote rotator works fine

        var notification = $("body").madNotification({ api: true });
        if ($.hasChars(options.message)) {
            notification.success(options.message);
        }

        $("#highlights a.galleryIcon").click(function () {
            if (!$("#divModal").hasElements()) {
                $("body").append('<div id="divModal"><div id="divModalActions"><a href="#" title="Close"></a></div><div id="divModalBg"></div><div class="content"></div></div>');
            }

            var url = $(this).attr("href");
            var frame = $('<iframe src="' + url + '" frameborder="0" border="0" marginWidth="0" marginHeight="0" scrolling="no" vspace="0" hspace="0"></iframe>');
            $("#divModal").madDialog({ api: true }).show(frame);
            return false;
        });
    }
});