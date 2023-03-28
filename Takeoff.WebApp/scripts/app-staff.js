/// <reference path="jquery-1.4.4-vsdoc.js" />
/// <reference path="swfobject.debug.js" />
/// <reference path="takeoff-helpers.js" />
/// <reference path="takeoff-forms.js" />
/// <reference path="date.Relative.js" />

function initStaff() {
    $(document).madTooltips(); //initialize the tooltips
    $("#pageDropdown").change(function () {
        var item = $("#pageDropdown option:selected");
        $.goTo(item.attr("href"));
    });

}

//accounts page
function pageAccounts() {

    //create the datatable plugin and fill in with initial data
    var table = $("#dataTable").gridServer({
        "bFilter": true,
        url: window.urlPrefixes.relative + "staff/accounts",
        onServerDataReturned: function (dataRecords) {
            $.each(dataRecords, function () {
                this.CreatedOn = new Date(this.CreatedOn);
            });
        },
        aoColumns: [
                { sType: "numeric", sName: "Id", sTitle: "Account ID", bUseRendered: false },
                { sType: "numeric", sName: "OwnerId", sTitle: "Owner Id", bUseRendered: false },
                { sType: "fastDate", "sSortDataType": "fastDate", sTitle: "Created On", sName: "CreatedOn", bUseRendered: false },
                { sType: "string", sName: "Name", sTitle: "Owner Name" },
                { sType: "string", sName: "Email", sTitle: "Owner Email", bUseRendered: false },
                { sType: "boolean", sName: "IsVerified", sTitle: "Verified" },
                { sType: "string", sName: "Plan", sTitle: "Plan" },
                { sType: "string", sName: "Status", sTitle: "Status" },
                { sType: "numeric", sName: "ProjectCount", sTitle: "Productions #" },
                { sType: "numeric", sName: "VideoCount", sTitle: "Videos #" },
                { sType: "numeric", sName: "VideoDuration", sTitle: "Videos Duration", bUseRendered: false },
                { sType: "numeric", sName: "VideoStreamCount", sTitle: "Video Files #" },
                { sType: "numeric", sName: "VideoStreamSize", sTitle: "Video Files Size", bUseRendered: false },
                { sType: "numeric", sName: "FileCount", sTitle: "Files #" },
                { sType: "numeric", sName: "FileSize", sTitle: "Files Size", bUseRendered: false },
                { sType: "numeric", sName: "UploadCount", sTitle: "Uploads #" },
                { sType: "numeric", sName: "UploadSize", sTitle: "Uploads Size", bUseRendered: false },
                { sType: "numeric", sName: "DownloadCount", sTitle: "Downloads #" },
                { sType: "numeric", sName: "DownloadSize", sTitle: "Downloads Size", bUseRendered: false },
                { sType: "numeric", sName: "MemberCount", sTitle: "Members" }
                ],
        rowCallback: function (aData, iDisplayIndex, tds, data, cellsPerColumn) {
            cellsPerColumn.CreatedOn.attr("title", moment(data.CreatedOn).format('LLL')).text(moment(data.CreatedOn).format('L'));
            cellsPerColumn.VideoStreamSize.text(jQuery.formatFileSize(data.VideoStreamSize));
            cellsPerColumn.FileSize.text(jQuery.formatFileSize(data.FileSize));
            cellsPerColumn.UploadSize.text(jQuery.formatFileSize(data.UploadSize));
            cellsPerColumn.DownloadSize.text(jQuery.formatFileSize(data.DownloadSize));
            cellsPerColumn.VideoDuration.text(jQuery.timeCode(data.VideoDuration, true, true, 0));

            cellsPerColumn.Id.html('<a href="' + window.urlPrefixes.relative + 'staff/accounts/' + data.Id + '">' + data.Id + '</a>');
            cellsPerColumn.OwnerId.html('<a href="' + window.urlPrefixes.relative + 'staff/users/' + data.OwnerId + '">' + data.OwnerId + '</a>');
            cellsPerColumn.Email.html('<a href="mailto:' + data.Email + '">' + data.Email + '</a>');
        }
    });

}

//staff/users page
function pageUsers() {

    //create the datatable plugin and fill in with initial data
    var table = $("#dataTable").gridServer({
        "bFilter": true,
        url: window.urlPrefixes.relative + "staff/users",
        onServerDataReturned: function (dataRecords) {
            $.each(dataRecords, function () {
                this.CreatedOn = new Date(this.CreatedOn);
            });
        },
        aoColumns: [
                { sType: "numeric", sName: "Id", sTitle: "User ID", bUseRendered: false },
                { sType: "fastDate", "sSortDataType": "fastDate", sTitle: "Created On", sName: "CreatedOn", bUseRendered: false },
                { sType: "string", sName: "Name", sTitle: "Owner Name" },
                { sType: "string", sName: "Email", sTitle: "Owner Email", bUseRendered: false },
                { sType: "boolean", sName: "IsVerified", sTitle: "Verified" },
                { sType: "boolean", sName: "HasAccount", sTitle: "Owns an Account" }
                ],
        rowCallback: function (aData, iDisplayIndex, tds, data, cellsPerColumn) {
            cellsPerColumn.CreatedOn.attr("title", moment(data.CreatedOn).format('LLL')).text(moment(data.Created).format('L'));
            cellsPerColumn.Id.html('<a href="' + window.urlPrefixes.relative + 'staff/users/' + data.Id + '">' + data.Id + '</a>');
            cellsPerColumn.Email.html('<a href="mailto:' + data.Email + '">' + data.Email + '</a>');
        }
    });

}

//staff/encodes
function pageStaffEncodes() {

    //create the datatable plugin and fill in with initial data
    var table = $("#dataTable").gridServer({
        "bFilter": false,
        "aaSorting": [[0,'desc']],
        url: window.urlPrefixes.relative + "staff/encodes",
        onServerDataReturned: function (dataRecords) {
            $.each(dataRecords, function () {
                this.UploadCompleted = new Date(this.UploadCompleted);
                this.EncodingCompleted = new Date(this.EncodingCompleted);
                this.JobCompleted = new Date(this.JobCompleted);
                this.JodId = this.VideoId;
            });
        },
        aoColumns: [
                { sType: "numeric", sName: "VideoId", sTitle: "ID", bUseRendered: false },
                { sType: "numeric", sName: "AccountId", sTitle: "Account ID", bUseRendered: false },
                { sType: "numeric", sName: "UserId", sTitle: "User ID", bUseRendered: false },
                { sType: "string", sName: "Email", sTitle: "Email", bUseRendered: false },
                { sType: "numeric", sName: "ProductionId", sTitle: "Production", bUseRendered: false },
                { sType: "string", sName: "InputOriginalFileName", sTitle: "File Name" },
                { sType: "fastDate", "sSortDataType": "fastDate", sTitle: "Uploaded On", sName: "UploadCompleted" },
                { sType: "fastDate", "sSortDataType": "fastDate", sTitle: "Encoded On", sName: "EncodingCompleted" },
                { sType: "fastDate", "sSortDataType": "fastDate", sTitle: "Completed On", sName: "JobCompleted" },
                { sType: "boolean", sName: "Error", sTitle: "Error" }
                ],
        rowCallback: function (aData, iDisplayIndex, tds, data, cellsPerColumn) {
            cellsPerColumn.VideoId.html(data.VideoId + ' <a href="' + data.LogUrl + '">Log</a>');
            cellsPerColumn.AccountId.html('<a href="' + window.urlPrefixes.relative + 'staff/accounts/' + data.AccountId + '">' + data.AccountId + '</a>');
            cellsPerColumn.UserId.html('<a href="' + window.urlPrefixes.relative + 'staff/users/' + data.UserId + '">' + data.UserId + '</a>');
            cellsPerColumn.UploadCompleted.attr("title", moment(data.UploadCompleted).format('LLL')).text(moment(data.UploadCompleted).format('LLL'));
            cellsPerColumn.EncodingCompleted.attr("title", moment(data.EncodingCompleted).format('LLL')).text(moment(data.EncodingCompleted).format('LLL'));
            cellsPerColumn.JobCompleted.attr("title", moment(data.JobCompleted).format('LLL')).text(moment(data.JobCompleted).format('LLL'));
            cellsPerColumn.ProductionId.html('<a href="' + window.urlPrefixes.relative + 'productions/' + data.ProductionId + '">' + data.ProductionTitle + ' (' + data.ProductionId + ')</a>');
            cellsPerColumn.Email.html('<a href="mailto:' + data.Email + '">' + data.Email + '</a>');
            //cellsPerColumn.VideoId.text(data.VideoTitle + ' (' + data.VideoId + ')');
        }
    });

}



function pageStaffOutgoingMail() {

    //create the datatable plugin and fill in with initial data
    var table = $("#dataTable").gridServer({
        "bFilter": false,
        url: window.urlPrefixes.relative + "staff/outgoingmail",
        onServerDataReturned: function (dataRecords) {
            $.each(dataRecords, function () {
                this.SentOn = new Date(this.SentOn);
            });
        },
        aoColumns: [
                { sType: "fastDate", "sSortDataType": "fastDate", sTitle: "Sent On", sName: "SentOn" },
                { sType: "string", sName: "ToAddress", sTitle: "To" },
                { sType: "string", sName: "Template", sTitle: "Template" },
                { sType: "string", sName: "TimesOpened", sTitle: "Times Opened", bUseRendered: false },
                { sType: "string", sName: "Id", sTitle: "Details", bUseRendered: false }
                ],
        rowCallback: function (aData, iDisplayIndex, tds, data, cellsPerColumn) {
            if (data.IncludedTrackingImage === true) {
                cellsPerColumn.TimesOpened.text(data.TimesOpened);
            }
            else {
                cellsPerColumn.TimesOpened.text('N/A');
            }
            
            cellsPerColumn.Id.html('<a target="_blank" href="' + window.urlPrefixes.relative + 'staff/outgoingmail/' + data.Id + '?what=html">Html</a><br />' +
            '<a target="_blank" href="' + window.urlPrefixes.relative + 'staff/outgoingmail/' + data.Id + '?what=txt">Plain</a><br />' +
            '<a target="_blank" href="' + window.urlPrefixes.relative + 'staff/outgoingmail/' + data.Id + '">Data</a><br />');
        }
    });

}

