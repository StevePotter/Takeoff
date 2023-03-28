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

//*********************** Files *******************

var filesDataTable; //dataTable plugin for the Files table

function initFiles() {
    var fileInfos = {};
    var tab = $("#files-tab");
    var tabBtn = $("#files-tab a");
    tabBtn.data("text", tabBtn.text());

//    //set up the upload plugin
//    uploadPlugin = $("#fileUpload").madUpload({
//        api: true,
//        queueList: $("#fileUploadQueue"),
//        file_types: "*.*",
//        removeCompletedFromPartialQueue: false,
//        container_css_position: "", //one is relative, one is absolute.  we set those in markup
//        file_types_description: "Files",

//        fileArea: function (file) {
//            var container = $('<li class="queued"><form class="well upload form-horizontal"></form></li>');
//            var form = container.find('form');

//            var fileArea = form.append('<div class="status-file-cancel">' +
//                '<span class="status">Waiting to upload</span> <span class="file-name"></span> - <span class="progressPercent"></span><span class="size-left">(<span class="remainingSize">5.8 MB</span> left)</span> <a href="#" class="cancel" title="Cancel Upload">Cancel</a>' +
//                '<div class="progress progress-striped"><div class="bar"></div></div>' +
//                '</div>');
//            fileArea.find('.file-name').text(file.name);

//            return container;
//        },
//        fileQueued: function (event, file) {
//            var fileId = file.id;
//            $.madAjax({
//                url: window.urlPrefixes.relative + "ProductionFiles/Allocate",
//                data: function () {
//                    return {
//                        productionId: productionData.Id,
//                        fileToUpload: {
//                            name: file.name,
//                            index: file.index,
//                            bytes: file.size
//                        }
//                    };
//                },
//                successCancelled: function (result) {
//                    uploadPlugin.cancel();
//                },
//                success: function (result) {

//                    fileInfos[fileId] = result;

//                    $.each(result.variables, function (key, value) {
//                        uploadPlugin.addFileParam(file.id, key, value);
//                    });
//                    uploadPlugin.setUploadURL(result.targetUrl);

//                    // start the upload since it's queued
//                    uploadPlugin.startUpload(file.id);
//                    setUploadStart("file");

//                }
//            });

//            mainPanelTabs.open(mainPanelTabs.indexOf(tab));
//        },
//        uploadStart: function (event, file, fileArea) {
//            fileArea.data("progressbar").addClass('active');
//            fileInfos[file.id]._startedOn = (new Date()).getTime();
//        },
//        fileUploaded: function (event, file, fileArea) {
//            //adjust the finished progress bar css classes
//            fileArea.data("progressbar").removeClass('active').removeClass('progress-striped');

//            var startTime = fileInfos[file.id]._startedOn;
//            var uploadDuration = ((new Date()).getTime() - startTime) / 1000;

//            $.madAjax({
//                url: window.urlPrefixes.relative + "ProductionFiles",
//                data: function () {
//                    return {
//                        productionId: productionData.Id,
//                        fileUploaded: fileInfos[file.id],
//                        uploadDuration: uploadDuration
//                    };
//                },
//                success: function (result) {
//                    var fileData = result.Data;
//                    addActivityItem(result.ActivityPanelItem, false);
//                    loadFilesIntoTable([fileData], false);
//                }
//            });
//        },
//        queueComplete: function () {
//            setUploadStop("file");
//            hideBackgroundDownload();
//            //            $("#fileUploadTip").hide();
//        },
//        uploadProgress: function () {
//            if (showingBackgroundDownload) {
//                //compare the text because when it was constantly updating the text, the click event wouldn't fire
//                var newText = uploadPlugin.totalProgressPercent() + "%";
//                if (tabBtn.text() !== newText)
//                    tabBtn.text(uploadPlugin.totalProgressPercent() + "%");
//            }
//        }
//    });

    //create the datatable plugin and fill in with initial data
    filesDataTable = $("#filesTable").dataTable({
        bJQueryUI: false,
        "bPaginate": false,
        "bLengthChange": false,
        "bFilter": false,
        "bSort": true,
        "bInfo": false,
        "bAutoWidth": false,
        aoColumns: [
            { sType: "string", bUseRendered: false, sWidth: "240px" }, //file name
            {sType: "numeric", bUseRendered: false, sWidth: "70px" }, //size
            {sType: "string", sWidth: "100px" }, //creator
            {sType: "fastDate", bUseRendered: false, "sSortDataType": "fastDate", sName: "CreatedOn", sWidth: "120px" }, //date added
            {sType: "string", bSearchable: false, bSortable: false}//delete button
            ],
        fnRowCallback: function (nRow, aData, iDisplayIndex) {
            var $row = $(nRow);
            var tds = $('td', $row);
            var data = $row.data("data");

            tds.eq(0).html('<a href="' + window.urlPrefixes.relative + 'ProductionFiles/Download/' + data.Id + '">' + $.htmlEncode(data.FileName) + '</a>');
            tds.eq(1).text(data.SizeFormatted);

            tds.eq(3).data("date", data.CreatedOn);
            tds.eq(3).addClass("relativeDate").attr("title", moment(data.CreatedOn).format('LLL')).text(toRelativeString(data.CreatedOn));

            tds.eq(4).html('');
            if (data.CanDelete) {
                var deleteBtn = $('<a href="#">Delete</a>').click(function () {
                    var message = "Are you sure you want to remove '" + data.FileName + "'?";
                    if (confirm(message)) {
                        $.madAjax({
                            url: (window.urlPrefixes.relative + "ProductionFiles/Delete"),
                            data: function () {
                                return {
                                    id: data.Id
                                };
                            },
                            error: function () {
                                alert("Uh oh.  There was a problem while deleting your comment.  We'll check it out and get back to you.  In the meantime, we've put your comment back in.");
                                addFileToUi(data);
                                return true; //cancel other error handlers
                            },
                            beforeSend: function (result) {
                                //we delete the reply immediately and restore it if there was a problem.  this makes the UI more responsive
                                deleteFileFromUi(data.Id);
                            }

                        });
                    }
                    return false;
                });
                tds.eq(4).append(deleteBtn);
            }
            return nRow;
        }
    });

    loadFilesIntoTable(productionData.Files, false); //load the initial data because it is not in the html

    $("#file-add-button").click(function () {
        filepicker.pickAndStore(
                {
                    services: ['COMPUTER', 'FTP', 'DROPBOX', 'FACEBOOK', 'GOOGLE_DRIVE', 'BOX', 'URL', 'VIDEO'],
                    openTo: 'COMPUTER',
                    maxSize: 20 * 1073741824,
                    multiple: true
                },
                { location: "S3" },
                function (fpfiles) {
                    _.each(fpfiles, function (file) {                        
                        var data = { productionId: productionData.Id, fileKey: file.key };
                        if (file.size)
                            data.fileSize = file.size;
                        if (file.filename)
                            data.fileName = file.filename;

                        $.madAjax({
                            url: window.urlPrefixes.relative + "ProductionFiles",
                            data: data,
                            success: function (result) {
                                addActivityItem(result.ActivityPanelItem, false);
                                loadFilesIntoTable([result.Data], false);
                            }
                        });

                    });
                },
                function (fpError) {

                });
        return false;
    });
}


function addFileToUi(data) {
    loadFilesIntoTable([data], false);
}

function loadFilesIntoTable(filesData, clearTable, checkForDuplicates) {
    if (!$.isArray(filesData)) {
        filesData = [filesData];
    }
    var files = [];
    $.each(filesData, function () {
        this.CreatedOn = new Date(this.CreatedOn);
        files.push([this.FileName, this.SizeBytes, ($.exists(this.Creator) ? this.Creator.Name : ""), this.CreatedOn, this]);
    });
    if (clearTable)
        filesDataTable.fnClearTable();

    if (files.length > 0) {
        var aoDataIndices = filesDataTable.fnAddData(files, false);
        var adData = filesDataTable.fnSettings().aoData;
        $.each(aoDataIndices, function (i) {
            $(adData[this].nTr).data("data", filesData[i]);
        });
        filesDataTable.fnDraw();
    }
    checkForEmptyFilesTable();
}

function checkForEmptyFilesTable() {
    if (isFilesEmpty()) {//fnGetNodes worked after filling the table but not after fnDeleteRow, which resulted in the table not hiding once the last row was deleted.  so i had to do this shitty jquery garbage
        $("#filesTable").parent().hide();
        $("#filesTableEmptyMsg").show();
    }
    else {
        $("#filesTable").parent().show();
        $("#filesTableEmptyMsg").hide();
    }
}

function isFilesEmpty() {
    return $("#filesTable tbody tr:first td.dataTables_empty").hasElements();
}

//determines whether a file with the given id is in the data table 
function isFileInUi(fileId) {
    if (isFilesEmpty())
        return false;
    var found = false;
    $("#filesTable tbody tr").eachJ(function () {
        var dataObj = this.data("data");
        if (dataObj.Id === fileId) {
            found = true;
            return false;
        }
    });
    return found;
}


//call when you want to delete a file with the given ID from the filesDataTable
function deleteFileFromUi(fileId) {
    deleteActivityItem(fileId);
    if (isFilesEmpty())
        return;

    $("#filesTable tbody tr").each(function () {
        var dataObj = $(this).data("data");
        if (dataObj.Id === fileId) {
            filesDataTable.fnDeleteRow(this);
            return false;
        }
    });

    checkForEmptyFilesTable();
}
