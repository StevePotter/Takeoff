/// <reference path="jquery-1.6.2-vsdoc.js" />
/// <reference path="swfobject.debug.js" />

/*
* Mediascend core library
* version: 1.0
* Shared components used across various Mediascend libraries
*
* Copyright (c) 2009 Mediascend, Inc.
* 
* Requires: jQuery v1.3+
* 
* Some functions borrowed from the great plugin  http://code.google.com/p/jquery-extensions/
*/


/////////////////////////////////////////////////////////////////////////////////
/*
* Util functions - simple jQuery plugins and other basic util methods
*/
/////////////////////////////////////////////////////////////////////////////////


/* Determines whether the object is set to an instance of something (not undefined and not null).
@param The object to compare.
*/
jQuery.exists = function (o) {
    ///	<summary>
    ///	Determines whether the object is set to an instance of something (not undefined and not null).
    ///	</summary>
    ///	<param name="o" type="Object">The object to compare.</param>
    ///	<returns type="Boolean" />
    return !jQuery.isNull(o) && !_.isUndefined(o);
};

/* Determines whether the object is null (declare variables with a value set to null). This will return false for undefined values.
@param The object to compare.
*/
jQuery.isNull = function (o) {
    ///	<summary>
    ///	Determines whether the object is null (declare variables with a value set to null). This will return false for undefined values.
    ///	</summary>
    ///	<param name="o" type="Object">The object to compare.</param>
    ///	<returns type="Boolean" />
    return (o === null);
};



/* Determines whether the object is not undefined, that a value has been set for it. This will return true for variables with null value.
@param The object to compare.
*/
jQuery.isDefined = function (o) {
    ///	<summary>
    ///	Determines whether the object is not undefined, that a value has been set for it. This will return true for variables with null value.
    ///	</summary>
    ///	<param name="o" type="Object">The object to compare.</param>
    ///	<returns type="Boolean" />
    return (typeof o !== "undefined");
};

/* Determines whether the object provided is null, or undefined.
@param The object to compare.
*/
jQuery.isNullOrUndefined = function (o) {
    ///	<summary>
    ///	Determines whether the object provided is null, or undefined.
    ///	</summary>
    ///	<param name="o" type="Object">The object to compare.</param>
    ///	<returns type="Boolean" />
    return jQuery.isNull(o) || _.isUndefined(o);
};


/* If the value passed is undefined, this returns the second parameter.
*/
jQuery.definedOr = function (value, ifUndefined) {
    ///	<summary>
    ///	If the value passed is undefined, this returns the second parameter.
    ///	</summary>
    if (typeof value === "undefined")
        return ifUndefined;
    return value;
};

/* If the object has a length property (array, jQuery, string), this returns whether length is 0.  Otherwise, it returns true if the object has no properties.  Null or undefined objects return true.
*/
jQuery.empty = function (o) {
    ///	<summary>
    ///	If the object has a length property (array, jQuery, string), this returns whether length is 0.  Otherwise, it returns true if the object has no properties.  Null or undefined objects return true.
    ///	</summary>

    if (jQuery.isNullOrUndefined(o))
        return true;
    var isEmpty = true;
    $.each(o, function () {
        isEmpty = false;
        return false;
    });
    return isEmpty;
};


/* Determines whether the object is a Javascript Number object (int or float)
@param The object to compare.
*/
jQuery.isNumber = function (o) {
    ///	<summary>
    ///	Determines whether the object is a Javascript Number object (int or float)
    ///	</summary>
    ///	<param name="o" type="Object">The object to compare.</param>
    ///	<returns type="Boolean" />
    if (typeof o == "object" && o !== null)
        return (typeof o.valueOf() === "number");
    else
        return (typeof o === "number");
};


jQuery.isPositiveNumber = function (o) {
    return jQuery.isNumber(o) && !isNaN(o) && o > 0;
};



/* Determines whether the object is a jquery object. 
@param The object to compare.
*/
jQuery.isjQuery = function (o) {
    ///	<summary>
    ///	Determines whether the object is a jquery object. 
    ///	</summary>

    if (typeof o == "object" && o !== null)
        return o instanceof jQuery;

    return false;
};

/* Returns true if obj is a DOM node of type 1, false otherwise.  Taken from prototype framework - http://www.prototypejs.org/api/object/isElement
@param The object to compare.
*/
jQuery.isElement = function (o) {
    ///	<summary>
    ///	Returns true if obj is a DOM node of type 1, false otherwise.
    ///	</summary>
    return !!(o && o.nodeType == 1);
};

jQuery.isArrayWithItems = function (o) {
    return $.isArray(o) && o.length > 0;
};


/* Removes objects from the array.
*/
Array.prototype.removeItems = function (itemsToRemove) {
    if (!$.isArray(itemsToRemove))
        itemsToRemove = [itemsToRemove];
    var j;
    for (var i = 0; i < itemsToRemove.length; i++) {
        j = 0;
        while (j < this.length) {
            if (this[j] == itemsToRemove[i]) {
                this.splice(j, 1);
            } else {
                j++;
            }
        }
    }
};


/* Determines whether the object is a Javascript string object.
@param The object to compare.
*/
jQuery.isString = function (o) {
    ///	<summary>
    ///	Determines whether the object is a Javascript string object.
    ///	</summary>
    ///	<param name="o" type="Object">The object to compare.</param>
    ///	<returns type="Boolean" />
    return (typeof o === "string");
};

/* Determines whether the object is a Javascript string object, and is empty - that is zero length.
Undefined and null objects return false.
@param The string to compare.
*/
jQuery.emptyString = function (str) {
    ///	<summary>
    ///	Determines whether the object is a Javascript string object, and is empty - that is zero length.
    /// Undefined and null objects return false.
    ///	</summary>
    ///	<param name="str" type="String">The string to compare.</param>
    ///	<returns type="Boolean" />
    if (jQuery.isNullOrUndefined(str))
        return true;
    else if (!jQuery.isString(str))
        throw "isEmpty: the object is not a string";
    else if (str.length === 0)
        return true;

    return false;
};

/* Determines whether the object is a Javascript string object, and it has at least one character.
Undefined and null objects return false.
@param The string to compare.
*/

jQuery.hasChars = function (str) {
    ///	<summary>
    ///	Determines whether the object is a Javascript string object, and it has at least one character.
    /// Undefined and null objects return false.
    ///	</summary>
    ///	<param name="str" type="String">The string to compare.</param>
    ///	<returns type="Boolean" />
    if (jQuery.emptyString(str))
        return false;

    return $.trim(str).length > 0;
};

/* Determines whether a string starts with a particular other string.
@param str The string to search.
@param search The string to search for.
*/
jQuery.startsWith = function (str, search) {
    ///	<summary>
    ///	 Determines whether a string starts with a particular other string.
    ///	</summary>
    ///	<param name="str" type="String">The string to search.</param>
    ///	<param name="search" type="String">The string to search for.</param>
    ///	<returns type="Boolean" />
    if (jQuery.isString(str))
        return (str.indexOf(search) === 0);
    return false;
};

/* Determines whether a string ends with a particular other string.
@param str The string to search.
@param search The string to search for.
*/
jQuery.endsWith = function (str, search) {
    ///	<summary>
    ///	Determines whether a string ends with a particular other string.
    ///	</summary>
    ///	<param name="str" type="String">The string to search.</param>
    ///	<param name="search" type="String">The string to search for.</param>
    ///	<returns type="Boolean" />
    if (!jQuery.isString(str) || !jQuery.isString(search) || jQuery.emptyString(str) || jQuery.emptyString(search))
        return false;
    else if (search.length > str.length)
        return false;
    else if (str.length - search.length === str.lastIndexOf(search))
        return true;

    return false;
};

/* Gets the string that occurs after the first occurance of string to find.
@param str The string to search.
@param search The string to search for.
*/
jQuery.afterStr = function (str, search) {
    var start = str.indexOf(search);
    if (start >= 0) {
        return str.substr(start + search.length);
    }
    return null;
};

/* Navigates to the given url.
*/
jQuery.goTo = function (url) {
    ///	<summary>
    ///	Navigates to the given url.
    ///	</summary>
    top.location.href = url;

};

/* Takes an array and converts it to a object that lets you look elements up by some ID.
@param array The array to convert.
@param keySelector A function that takes in a single array item and returns the key.
*/
jQuery.makeDictionary = function (array, keySelector) {
    ///	<summary>
    ///	Takes an array and converts it to a object that lets you look elements up by some ID.
    ///	</summary>
    ///	<param name="array" type="String">The array to convert.</param>
    ///	<param name="keySelector" type="String">A function that takes in a returns the key.  Array item is 'this'.</param>
    ///	<returns type="Object" />
    var result = {};
    for (var i = 0, l = array.length; i < l; i++) {
        var item = array[i];
        var key = keySelector.apply(item,[]);
        result[key] = item;
    }
    return result;
};

jQuery.formatFileSize = function (bytes) {
    var bytesInKb = 1024, bytesInMb = 1048576, bytesInGb = 1073741824;

    if (bytes < bytesInMb)//less than a mb, use "kb"
    {
        var kb = (bytes / bytesInKb);
        return kb.toFixed(0) + " KB"; //no need to round
    }
    if (bytes < bytesInGb) {
        var mb = (bytes / bytesInMb);
        return mb.toFixed(1) + " MB";
    }

    //use 1 decimal place for mgb
    var gb = (bytes / bytesInGb);
    return gb.toFixed(2) + " GB";
};

//converts the given seconds to video timecode format
jQuery.timeCode = function(seconds, showHoursIf0, padMinutes, millisecDigits) {

    var output = "";
    var h = Math.floor(seconds / 3600);
    if (showHoursIf0 || h > 0) {
        output += h.toFixed(0);
    }

    var m = Math.floor((seconds % 3600) / 60);
    if (output.length > 0)
        output += ":";
    if (padMinutes && m < 10)
        output += "0";
    output += m.toFixed(0) + ":";

    var s = Math.floor((seconds % 3600) % 60);
    if (s < 10)
        output += "0";
    output += s.toFixed(0);

    if (millisecDigits > 0) {
        output += ".";
        var tenToPower = Math.pow(10, millisecDigits);
        var decimals = Math.round((seconds - Math.floor(seconds)) * tenToPower);
        var milliseconds = decimals.toString();

        while (milliseconds.length < millisecDigits)
            milliseconds += "0";


        output += milliseconds;
    }

    return output;
};



//http://www.rockechris.com/jquery/jquery.random.js
jQuery.random = function (max) {
    return Math.floor(max * (Math.random() % 1));
};
jQuery.randomBetween = function (MinV, MaxV) {
    return MinV + jQuery.random(MaxV - MinV + 1);
};

//takes the text (meant to be used as inner text) and gets its raw html equivalent, like turning " into &quot;.  This is useful when generating html to insert in templates and whatnot and you want to set a data string to be the inner text of an element.
jQuery.htmlEncode = function (text) {
    if (!$.exists(text))
        return "";
    return $('<div/>').text(text).html();
};


//indicates whether the jquery object has at least one element
jQuery.fn.hasElements = function () {
    return this.length > 0;
};


/**
* jQuery.fn.sortElements
* --------------
* @param Function compareFn:
*   Exactly the same behaviour as [1,2,3].sort(compareFn)
*   
* @param Function getSortable
*   A function that should return the element that is
*   to be sorted. The compareFn will run on the
*   current collection, but you may want the actual
*   resulting sort to occur on a parent or another
*   associated element.
*   
*   E.g. $('td').sortElements(compareFn, function(){
*      return this.parentNode; 
*   })
*   
*   The <td>'s parent (<tr>) will be sorted instead
*   of the <td> itself.
*   thanks to http://james.padolsey.com/javascript/sorting-elements-with-jquery/
*/
jQuery.fn.sortElements = (function () {
    var sort = [].sort;
    return function (compareFn, getSortable) {
        getSortable = getSortable || function () { return this; };

        var placements = this.map(function () {
            var sortElement = getSortable.call(this), parentNode = sortElement.parentNode,
            // Since the element itself will change position, we have
            // to have some way of storing its original position in
            // the DOM. The easiest way is to have a 'flag' node:
                nextSibling = parentNode.insertBefore(
                    document.createTextNode(''),
                    sortElement.nextSibling
                );
            return function () {
                if (parentNode === this) {
                    throw new Error(
                        "You can't sort elements if any one is a descendant of another."
                    );
                }
                // Insert before flag:
                parentNode.insertBefore(this, nextSibling);
                // Remove flag:
                parentNode.removeChild(nextSibling);
            };
        });

        var sorted = sort.call(this, compareFn);
        return sorted.each(function (i) {
            placements[i].call(getSortable.call(this));
        });

    };

})();

////used for tracking...hits a url blindly.
//$.pingUrl = function(url, params) {
//    if (params) {
//        url += (url.indexOf('?') >= 0 ? '&' : '?');
//        for (var name in params) {
//            url += name + '=' + encodeURIComponent(params[name]) + '&';
//        }
//    }
//    //xmlhttprequest has security issues that this bypasses.  this is the technique that many analytics solutions use
//    (new Image(1, 1)).src = url;
//};


//just like .each() but "this" is a jquery object wrapping the current element
$.fn.eachJ = function (callback, args) {
    return this.each(function (i) {
        var $this = $(this);
        return callback.call($this, i, $this);
    }, args);
};


//a simple plugin that allows for lazy (deferred) retrieval of a value.  this lets you obtain a value on demand and the value function is only executed once
//just pass in a function that will return the given value.  ex:  var value = $.lazy(function() { return 5 * 2; });      alert(value());
jQuery.lazy = function (valueFunction) {
    //underscores help prevent variable overrides in the function due to scope 
    var __func = valueFunction;
    var __value = undefined;

    return function () {
        if (typeof __value === "undefined")
            __value = __func();
        return __value;
    };
};


//sets all the elements to be the same height
$.fn.equalHeights = function () {
    if (this.length < 2)
        return this;
    var els = [];
    var tallest = 0;
    this.eachJ(function () {
        els.push(this);
        var currHeight = this.height();
        if (currHeight > tallest)
            tallest = currHeight;
    });
    $.each(els, function () {
        this.css({ 'min-height': tallest });
    });
    return this;
};




/////////////////////////////////////////////////////////////////////////////////
/*
* Timer - jQuery Plugin
* a nice alternative to using window.setTimeout and window.setInterval
*/
/////////////////////////////////////////////////////////////////////////////////

//a wrapper for an operation that occurs after some timeout or interval
$.timer = function (options) {
    this.settings = $.extend({}, $.timer.defaults, options);
    this._timeoutCount = 0;
    this._timeoutID = -1;

    if (this.settings.startNow === true)
        this.start();
};

$.timer.defaults = {
    delay: 1000,
    operation: null,
    repeatCount: 1,
    startNow: false
};

$.timer.prototype = {

    isRunning: function () {
        return this._timeoutID >= 0;
    },

    start: function () {
        if (!this.isRunning()) {
            var me = this; //needed because "this" is different context when the callback runs
            var timeoutHandler = function () {
                if (me._timeoutID >= 0) {
                    var settings = me.settings;
                    var repeatCount = settings.repeatCount;
                    if (repeatCount == 1) {
                        me._timeoutID = -1;
                    }
                    else if (repeatCount > 1) {
                        if (me._timeoutCount === repeatCount - 1) {
                            window.clearInterval(me._timeoutID);
                            me._timeoutID = -1;
                        }
                        else {
                            me._timeoutCount++;
                        }
                    }
                    me.settings.operation();
                }
            };

            if (this.settings.repeatCount == 1)
                this._timeoutID = window.setTimeout(timeoutHandler, this.settings.delay);
            else
                this._timeoutID = window.setInterval(timeoutHandler, this.settings.delay);
        }
    },

    restart: function () {
        this.cancel();
        this.start();
    },

    cancel: function () {
        if (this._timeoutID >= 0) {
            if (this.settings.repeatCount == 1)
                window.clearTimeout(this._timeoutID);
            else
                window.clearInterval(this._timeoutID);

            this._timeoutID = -1;
        }
    }
};

