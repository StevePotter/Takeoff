/// <reference path="jquery-1.6.2-vsdoc.js" />
/// <reference path="swfobject.debug.js" />
/// <reference path="takeoff-helpers.js" />

$(document).ready(function () {

//    plupload.buildUrl = function (url, items) {
//        return url;
//    };

//    $("#s3Flash").pluploadQueue({
//        // General settings
//        runtimes: 'flash',
//        url: '/root/uploadtest',
//        max_file_size: '5gb',
//        //chunk_size : '500mb',

//        // Flash settings
//        flash_swf_url: window.urlPrefixes.asset + 'Assets/plupload.flash.swf',
//        preinit: onS3UploaderPreInit
//    });
    $("#scaleUpHtml5").pluploadQueue({
        // General settings
        runtimes: 'flash',
        flash_swf_url: '/Assets/plupload.flash.swf',
//        flash_swf_url: window.urlPrefixes.asset + 'Assets/plupload.flash.swf',
        url: 'http://174.129.243.76/',
        max_file_size: '5gb'
    });

});



function onS3UploaderPreInit(uploader) {
    uploader.bind("UploadFile", function (up, file) {
        $.madAjax({
            url: window.urlPrefixes.relative + "UploadTests/AllocateS3",
            data: function () {
                return {
                    fileToUpload: {
                        chunk: 0,
                        name: file.name,
                        bytes: file.size
                    }
                };
            },
            success: function (result) {
                //file.key = result.variables.key;

                uploader.settings.url = result.targetUrl;
                //                            uploader.settings.headers = result.variables;
                uploader.settings.file_data_name = "file";
                uploader.settings.multipart = true;
                result.variables["Filename"] = result.name; //CRUCIAL for uploading to s3.  or else it fails the Filename built-in condition.  this was added automatically when using swfupload 
                uploader.settings.multipart_params = result.variables;
            },
            async: false
        });
    });

    uploader.bind('FileUploaded', function (up, file) {

    });

}