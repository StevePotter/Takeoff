/// <reference path="jquery-1.6.2-vsdoc.js" />
/// <reference path="takeoff-helpers.js" />
/// <reference path="takeoff-popup.js" />
/**
* Copyright (c) 2010 Mediascend, Inc.
* http://www.mediascend.com
* flexible tabs:  created after frustration with several tab plugins.  doesn't require any specific kind of markup construction, as opposed to other plugins.  also allows for dynamic adding and deleting of tabs (which others tripped up on)
* version: 1.0
*/

/*
Notes:
1.  It is assumed all tabs are closed with display:none in the beginning


params:

tabSelector -
panesContainer - jquery object that contains all the panes.  Required!
paneSelector - 

*/
(function ($) {
    $ = jQuery;


    function madTabs(tabsContainer, options) {
        tabsContainer.data("madTabs", plugin);
        options = $.extend({},
        {
            tabSelector: 'a',
            paneSelector: '> div',
            initialIndex: 0,
            tabOpenEvent: 'click',
            openTabClass: 'openTab',
            panesContainer: tabsContainer,
            api: false,
            openTabClosable: false//whether the currently open tab can be closed when the user tabOpenEvent fires. 
        }, options);

        var plugin = this, $plugin = $(this), tabs, panesContainer = options.panesContainer, panes, currentIndex = -1;

        $.each(options, function (name, fn) {
            if ($.isFunction(fn))
            { $plugin.bind(name, fn); }
        });


        var onTabOpenEvent = function (event) {
            var tab = $(this);
            var index = plugin.indexOf(tab);

            //this gives user code a chance to cancel the entire deal or check for repeat clicks 
            var e = $.Event();
            e.type = "onTabOpenEvent";
            $plugin.trigger(e, { sourceEvent: event, tabIndex: index, tab: tab });
            if (e.isDefaultPrevented()) {
                return;
            }

            event.preventDefault();
            if (currentIndex === index) {
                if (options.openTabClosable)//close the currently open tab.  otherwise ignore the event
                    plugin.close(false);
            }
            else {
                plugin.open(index, true);
            }
        };

        $.extend(plugin, {

            getOptions: function () {
                return options;
            },

            getOpenIndex: function () {
                return currentIndex;
            },

            getOpenTab: function () {
                return currentIndex >= 0 ? tabs.eq(currentIndex) : null;
            },
            getTabs: function () {
                return tabs;
            },
            getTab: function (index) {
                return tabs.eq(index);
            },
            getCount: function () {
                return tabs.length;
            },
            indexOf: function (tabOrPane) {
                return $(tabOrPane).data("tabIndex");
            },
            isOpen: function () {
                return currentIndex >= 0;
            },
            getOpenPane: function () {
                return currentIndex >= 0 ? panes.eq(currentIndex) : null;
            },
            getPanes: function () {
                return panes;
            },
            getPane: function (index) {
                return panes.eq(index);
            },
            add: function (tab, pane, index) {
                if (_.isUndefined(index) || index < 0)
                    index = tabs.length;
                if (index > tabs.length)
                    throw "invalid index";

                if (tabs.length == 0) {
                    tabsContainer.append(tab);
                    panesContainer.append(pane);
                }
                else if (index == tabs.length) {
                    //insert after last
                    tabs.eq(tabs.length - 1).after(tab);
                    panes.eq(panes.length - 1).after(pane);
                }
                else {
                    //insert before tab currently at that position
                    tabs.eq(index).before(tab);
                    panes.eq(index).before(pane);
                }

                if (currentIndex >= 0 && index <= currentIndex)
                    currentIndex++; //we bumped the current tab up one

                tab.bind(options.tabOpenEvent, onTabOpenEvent);
                plugin._createTabsAndPanes();
                return index;
            },

            remove: function (index) {
                if (_.isUndefined(index) || index < 0 || index >= tabs.length)
                    throw "invalid index";

                tabs.eq(index).remove().unbind(options.tabOpenEvent, onTabOpenEvent);
                panes.eq(index).remove();

                if (currentIndex >= 0 && index <= currentIndex)
                    currentIndex--; //we bumped the current tab down one
                plugin._createTabsAndPanes();
            },

            //opens the tab indicated.  you can pass the tab index, the tab element or the pane element.
            //fromUiEvent is used to distinguish between programatically opening a tab and a user action.
            open: function (indexOrTab, fromUiEvent) {
                if (_.isUndefined(indexOrTab))
                    throw "invalid index";
                if ($.isjQuery(indexOrTab)) {
                    indexOrTab = this.indexOf(indexOrTab);
                }

                if (!$.isNumber(indexOrTab) || indexOrTab < 0 || indexOrTab >= tabs.length) {
                    throw "invalid index";
                }

                var closedIndex;

                if (currentIndex >= 0) {
                    if (currentIndex == indexOrTab)
                        return;
                    closedIndex = currentIndex;
                    plugin.close(true);
                }

                currentIndex = indexOrTab;
                var tabToOpen = tabs.eq(currentIndex);
                var paneToOpen = panes.eq(currentIndex);

                var e = $.Event();
                e.type = "tabOpening";
                var args = { tabToOpen: tabToOpen, paneToOpen: paneToOpen, closedIndex: closedIndex, fromUiEvent: fromUiEvent };
                $plugin.trigger(e, args);
                if (!e.isDefaultPrevented()) {
                    tabToOpen.addClass(options.openTabClass);
                    paneToOpen.show();
                }
                e = $.Event();
                e.type = "tabOpened";
                $plugin.trigger(e, args);

            },

            close: function (openingAnother) {
                var index = currentIndex;
                var tabToHide = tabs.eq(index);
                var paneToHide = panes.eq(index);

                var e = $.Event();
                e.type = "tabClosing";
                var args = { tabToHide: tabToHide, paneToHide: paneToHide, index: index, openingAnother: openingAnother };
                $plugin.trigger(e, args);
                if (!e.isDefaultPrevented()) {
                    paneToHide.hide();
                    tabToHide.removeClass(options.openTabClass);
                }
                e = $.Event();
                e.type = "tabClosed";
                $plugin.trigger(e, args);

                if (true !== openingAnother) {//reset the index if we're not opening another now.  otherwise keep the index so the events can indicate to handlers which tab was just closed
                    currentIndex = -1;
                }

                return index;
            },

            _createTabsAndPanes: function () {
                tabs = $(tabsContainer).find(options.tabSelector);
                panes = $(panesContainer).find(options.paneSelector);
                tabs.each(function (i) {
                    $(this).data("tabIndex", i);
                });
                panes.each(function (i) {
                    $(this).data("tabIndex", i);
                });

                $plugin.trigger("tabsRefreshed", [tabs, panes]); //this event is a good place to do something in the UI when a tab is removed or added

            }
        });

        plugin._createTabsAndPanes();

        tabs.each(function () {
            $(this).bind(options.tabOpenEvent, onTabOpenEvent);
        });

        var initialIndex = options.initialIndex;
        if ($.isFunction(initialIndex)) {
            initialIndex = options.initialIndex(this);
        }
        if (tabs.length && (initialIndex >= 0)) {
            plugin.open(initialIndex);
        }

    };



    // jQuery plugin implementation
    $.fn.madTabs = function (opts) {
        // return existing instance
        var plugin = this.data("madTabs");
        if (plugin) { return plugin; }

        this.each(function (i) {
            plugin = new madTabs($(this), opts);
        });

        return opts.api ? plugin : this;
    };


})(jQuery); 


