
//some elements have relative dates, like "Seconds ago", "4 minutes ago", "2 hours ago", etc.  This bit of code runs every 30 seconds and updates those elements
function updateRelativeDates() {
    //update the table every 30 seconds so dates can be updated. you could be smarter about this, like using jquery to format the date, but this is a good start
    window.setInterval(function () {
        //td is for datatables, em is for comments.  i could have used just .relativeDate but for browsers like IE that don't support getElementByClassName natively, it'll be a long long query
        $("td.relativeDate, em.relativeDate").each(function () {
            var $this = $(this);
            var data = $this.data("date");
            $this.text(toRelativeString(data, $this.data("config")));
        });
    }, 30000);
}



//*********************** Util ********************

var relativeDateConfig =
{
    field: {
        secondsOld: "Seconds ago",
        minuteOld: "A minute ago",
        minutesOldPre: "",
        minutesOldPost: " minutes ago",
        hourOld: "1 hour ago",
        todayPre: "Today at ",
        yesterday: "Yesterday at ",
        olderDatePattern: "shortDate",
        olderPre: "",
        olderTimeEnabled: false,
        olderTimePre: " at "
    },
    sentence: {
        secondsOld: "just seconds ago",
        minuteOld: "a minute ago",
        minutesOldPre: "",
        minutesOldPost: " minutes ago",
        hourOld: "about an hour ago",
        todayPre: "today at ",
        yesterday: "yesterday at ",
        olderDatePattern: "shortDate",
        olderPre: "on ",
        olderTimeEnabled: true,
        olderTimePre: " at "
    }
}


//takes the date passed and converts it into a nicer value like "2 minutes ago"
function toRelativeString(date, config) {
    //assumes dates are in the past
    //the rules are
    //1.  If it is less than a minute old, we return "Less than a minute ago"
    //2.  If it is less than 1 hour old, we return the number of minutes:  "19 minutes ago"
    //3.  If it is <= than 6 hours old, or was written today, we return number of hours: "2 hours ago"
    //4.  If it was written yesterday, we return "Yesterday"
    //5.  Otherwise just put month and day (March 24).  If it took place last year, add the year
    if (_.isUndefined(config))
        config = relativeDateConfig.field;

    if ($.isNumber(date))
        date = new Date(date);

    var now = new Date();
    var today = Date.today(); //12am on this day
    var yesterday = Date.today().addDays(-1);

    if (date.compareTo(yesterday) == 1) {//date happened at some point yesterday or today
        var minutesOld = Math.floor((now.getTime() - date.getTime()) / 60000);
        if (minutesOld < 1)
            return config.secondsOld;
        if (minutesOld < 60) {
            if (minutesOld < 2)
                return config.minuteOld;
            else
                return config.minutesOldPre + minutesOld.toFixed(0) + config.minutesOldPost;
        }
        var hoursOld = Math.floor(minutesOld / 60);
        if (date.compareTo(today) == 1 || hoursOld <= config.hoursThreshold) {
            if (hoursOld < 2) {
                return config.hourOld;
            }
            else {
                return config.todayPre + date.toString(Date.CultureInfo.formatPatterns.shortTime);
            }
        }
        return config.yesterday + date.toString(Date.CultureInfo.formatPatterns.shortTime);
    }

    var olderDate = config.olderPre + date.toString(Date.CultureInfo.formatPatterns[config.olderDatePattern]);
    if (config.olderTimeEnabled)
        olderDate += config.olderTimePre + date.toString(Date.CultureInfo.formatPatterns.shortTime);

    return olderDate;
}
