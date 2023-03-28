/* http://www.enaic.nl/i18n.html http://plugins.jquery.com/project/eei18n
Internatioanlisation for jQuery v1.3.
Written by Eugene Bos (ebos{at}eniac.nl) April 2009. 
licensed under the GPL (http://dev.jquery.com/browser/trunk/jquery/GPL-LICENSE.txt) and 
MIT (http://dev.jquery.com/browser/trunk/jquery/MIT-LICENSE.txt) licenses. 
Please attribute the author if you use it. 
The loading part of the function is based on
Localisation assistance for jQuery v1.0.4.
http://keith-wood.name/localisation.html
Written by Keith Wood (kbwood{at}iinet.com.au) June 2007.*/

(function (jQuery) { // Hide scope, no jQuery conflict

    /* Load applicable localisation texts for one or more jQuery packages.
    Assumes that the localisations are named <base>-<lang>.js
    and loads them in order from least to most specific.
    For example, $(document).translate('mytexts.json');
    with the browser set to 'en-US' would attempt to load
    mytexts-en.js and mytexts-en-US.json.
    */
    jQuery.fn.translate = function (packages, languageName) {
        // Save the current setting, we want to direct load the dictionary JSON
        var saveSettings = { async: jQuery.ajaxSettings.async, timeout: jQuery.ajaxSettings.timeout };
        jQuery.ajaxSetup({ async: false, timeout: 500 });
        /* Get the appropriate JSON dictionary */
        var translateImpl = function (dictionary) {
            jQuery.each(dictionary, function (i, item) {
                /* get the tags for this id 
                if  it isn't an textarea 
                and it isn't an input 
                translate text in tag */
                var test = "#" + i + ":not(input):not(textarea)";
                jQuery(test).text(item);
                /* get the label-tags where this id = 'for' 
                translate text in label-tag */
                test = "label[for=" + i + "]";
                jQuery(test).text(item);
            });
        };

        // function to load the languagePackages, with the language and extension 
        var localiseOne = function (languagePackage, extension, lang) {
            try {
                jQuery.getJSON(languagePackage + "." + extension, translateImpl);
                if (lang.length >= 2) {
                    jQuery.getJSON(languagePackage + '-' + lang.substring(0, 2) + "." + extension, translateImpl);
                }
                if (lang.length >= 5) {
                    jQuery.getJSON(languagePackage + '-' + lang.substring(0, 5) + "." + extension, translateImpl);
                }
            } catch (shit) {
            };
        };
        var splitPackage = function (languagePackage) {
            var indx = languagePackage.lastIndexOf('.');
            var extension = "";
            var packageBase = languagePackage;
            if (indx != -1 && indx < languagePackage.length) {
                extension = languagePackage.substr(indx + 1);
                packageBase = languagePackage.substr(0, indx);
            }
            var ret = new Object();
            ret.base = packageBase;
            ret.ext = extension;
            return ret;
        };
        var lang = normaliseLang(languageName || jQuery.fn.translate.defaultLanguage);
        packages = (jQuery.isArray(packages) ? packages : [packages]);
        for (i = 0; i < packages.length; i++) {
            var packageObj = splitPackage(packages[i]);
            localiseOne(packageObj.base, packageObj.ext, lang);
        }
        jQuery.ajaxSetup(saveSettings);
        return this;
    };

    /* Retrieve the default language set for the browser. */
    jQuery.fn.translate.defaultLanguage = normaliseLang(navigator.language /* Mozilla */ ||
	  navigator.userLanguage /* IE */);

    /* Ensure language code is in the format aa-AA. */
    function normaliseLang(lang) {
        lang = lang.replace(/_/, '-').toLowerCase();
        if (lang.length > 3) {
            lang = lang.substring(0, 3) + lang.substring(3).toUpperCase();
        }
        return lang;
    };

})(jQuery);