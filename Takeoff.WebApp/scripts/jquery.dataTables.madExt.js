/// <reference path="jquery-1.4.4-vsdoc.js" />
/// <reference path="takeoff-helpers.js" />
/// <reference path="takeoff-forms.js" />
/// <reference path="takeoff-tabs.js" />
/// <reference path="jquery.dataTables.js" />


//fixes datatable's crappy sorting.  To use, each tr must have a $(tr).data("data") object that contains a property that coorespond's a column's sName property.  That property must be a date.
//ex column settings: { sType: "fastDate", bUseRendered: false, "sSortDataType": "fastDate", sName: "CreatedOn" }
$.fn.dataTableExt.afnSortData['fastDate'] = function (oSettings, iColumn) {
    var column = oSettings.aoColumns[iColumn];
    var aData = [];
    var sortProp = column.sName;
    $.each(oSettings.aoData, function () {
        var data = $(this.nTr).data("data");
        aData.push(data[sortProp]);
    });
    return aData;
}

jQuery.fn.dataTableExt.oSort['fastDate-asc'] = function (a, b) {
    return a.compareTo(b) * -1; //The addition of the datejs library caused the default to be insanely slow...probabgly because it uses a non-native Parse function.  Anyway since it provides a sweet compareTo, I use that instead. 
};
jQuery.fn.dataTableExt.oSort['fastDate-desc'] = function (a, b) {
    return a.compareTo(b); //The addition of the datejs library caused the default to be insanely slow...probabgly because it uses a non-native Parse function.  Anyway since it provides a sweet compareTo, I use that instead. 
};



$.fn.gridServer = function (options) {
    var table = this;
    var serverData; //stores the raw server data returned from the server, which is then assigned to each row.data() after the rows are filled
    options = $.extend({},
    {
        "bJQueryUI": false,
        "bPaginate": true,
        "sPaginationType": "full_numbers",
        "bLengthChange": true,
        "bSort": true,
        "bInfo": true,
        "bFilter": false,
        "bAutoWidth": false,
        "bServerSide": true,
        "bRetrieve": true,
        "sAjaxSource": options.url,
        "fnServerData": function (sSource, aoData, fnCallback) {
            var data = {};
            var columnNames;
            //set up the request data including page & sort info
            $.each(aoData, function (index) {
                var name = this.name;
                if (name === "sEcho")
                    data.Echo = this.value;
                else if (name === "sColumns")
                    columnNames = this.value.split(',');
                else if (name === "sSearch")
                    data.Search = this.value;
                else if (name === "iDisplayStart")
                    data.DisplayStart = this.value;
                else if (name === "iDisplayLength")
                    data.DisplayLength = this.value;
                else if ($.startsWith(name, "iSortCol")) {
                    var columnIndex = this.value;
                    var columnName = columnNames[columnIndex];
                    var direction = aoData[index + 1].value;
                    //todo: allow for multiple sort fields
                    data.SortBy = [columnName + ' ' + direction];
                }
            });
            $.madAjax({
                url: (options.url),
                data: data,
                success: function (result) {
                    //server must return an array of objects.  now we flatten that into an array of arrays, using column info to do it
                    serverData = result.aaData;

                    if (options.onServerDataReturned) {
                        options.onServerDataReturned(serverData);
                    }

                    var tableData = [];
                    //take the raw data and turn it into arrays of arrays
                    $.each(serverData, function () {
                        var dataRecord = this;
                        var fields = $.map(options.aoColumns, function (col) {
                            var name = col.sName;
                            if ($.hasChars(name))
                                return dataRecord[name];
                            else {
                                return "";
                            }
                        });
                        tableData.push(fields);
                    });
                    result.aaData = tableData;
                    fnCallback(result);
                }
            });
        },
        fnRowCallback: function (nRow, aData, iDisplayIndex) {

            var $row = $(nRow);
            var tds = $('td', $row);
            var data = $row.data("data");
            if (_.isUndefined(data)) {
                data = serverData[iDisplayIndex];
                $row.data("data", data); //will happen on the first draw when server-side processing is used
            }

            if (options.rowCallback) {
                //make an object that has each cell index by column name.  this avoids having to work with indexes
                var cellsPerColumn = {};
                $.each(options.aoColumns, function (index) {
                    var name = this.sName;
                    if ($.hasChars(name))
                        cellsPerColumn[name] = tds.eq(index);
                });

                options.rowCallback(aData, iDisplayIndex, tds, data, cellsPerColumn);
            }
            return nRow;
        }
    }, options);

    return table.dataTable(options);
}