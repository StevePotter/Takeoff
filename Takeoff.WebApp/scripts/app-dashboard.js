/// <reference path="jquery-1.6.2-vsdoc.js" />
/// <reference path="swfobject.debug.js" />
/// <reference path="takeoff-helpers.js" />
/// <reference path="takeoff-forms.js" />
/// <reference path="date.Relative.js" />

$.extend(window.views, {
    Dashboard: function (data) {
        var activityList;
        var maxActivityItems = 10;

        function initActivity() {
            activityList = $("#activity ul");

            if ($.isArrayWithItems(data.Activity)) {
                activityList.empty();
                $.each(data.Activity, function () {
                    addActivityItem(this, true);
                });
            }
            else {
                $("#activity div.empty-data-msg").show();
            }
            updateActivityPanel();
        }

        function updateActivityPanel() {
            window.setTimeout(function () {
                $.madAjax({
                    url: (window.urlPrefixes.relative + "Dashboard"),
                    data: {
                    },
                    error: function() {
                        updateActivityPanel(); //keep trying to update until connectivity resumes
                    },
                    serverError: function () {
                        return true; //this is a background request so don't pop up an error message on errors.  this was a big problem when memcached was occasionally blowing up
                    },
                    aborted: function () {
                        return true; //this is a background request so don't pop up an error message on errors.  this was a big problem when memcached was occasionally blowing up
                    },                                 
                    success: function (result) {
                        if (result && $.isArray(result.Activity)) {
                            activityList.empty();
                            $.each(result.Activity, function () {
                                addActivityItem(this, true);
                            });
                        }
                        updateActivityPanel();
                    }
                });
            }, 5000);
        }

        function addActivityItem(activityData, append) {
            $("#activity div.empty-data-msg").hide();

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

        }

        //deletes the action related to the given thing if it exists in the activity panel
        function deleteActivityItem(thingId) {
            var item = $("#activity" + thingId.toFixed(0));
            if (item.hasElements()) {
                item.remove();
                if (!activityList.find("li").hasElements())
                    $("#activity div.empty-data-msg").show();
            }
        }

        initActivity();

        updateRelativeDates();

        var prods = [], prodsById = {};
        $.each(data.Productions, function () {
            prods.push([this.Title.toString(), this.CreatedOn, this.LastChangeDate, this.OwnerName]);
            prodsById[this.Id.toString()] = this;
        });

        var table = $("#productions-data");
        var productionsDataTable = table.dataTable({
            bJQueryUI: false,
            "bPaginate": false,
            "bLengthChange": false,
            "bFilter": false,
            "bSort": true,
            "bInfo": false,
            "bAutoWidth": false,
            aoColumns: [
                    { sType: "string", bUseRendered: false },
                    { bUseRendered: false, sName: "CreatedOn" },
                    { bUseRendered: false, sName: "LastChangeDate" },
                    { sType: "string"} //creator
                ],
            fnRowCallback: function (nRow, aData, iDisplayIndex) {
                var $row = $(nRow);
                var tds = $('td', $row);
                var data = $row.data("data");
                tds.eq(0).html('<a href="' + window.urlPrefixes.relative + 'productions/' + data.Id + '"></a>').find("a").text(data.Title);
                tds.eq(1).addClass("relativeDate").data("date", data.CreatedOn).attr("title", moment(data.CreatedOn).format('LLL')).text(toRelativeString(new Date(data.CreatedOn)));
                tds.eq(2).addClass("relativeDate").data("date", data.LastChangeDate).attr("title", moment(data.LastChangeDate).format('LLL')).text(toRelativeString(new Date(data.LastChangeDate)));
                return nRow;
            }
        });

        if (prods.length > 0) {
            var aoDataIndices = productionsDataTable.fnAddData(prods, false);
            var adData = productionsDataTable.fnSettings().aoData;
            $.each(aoDataIndices, function (i) {
                $(adData[this].nTr).data("data", data.Productions[i]);
            });
            productionsDataTable.fnDraw();
        }
        else {
            table.parent().hide();
            $("#productions div.empty-data-msg").show();
        }
    }
});