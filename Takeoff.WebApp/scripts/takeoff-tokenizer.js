/* ============================================================
* takeoff-tokenizer.js v0.0.1
* http://github.com/stevepotter
*
* Makes it possible to create inputs that generate 'tokens' as input occurs.  Use to create UIs like dropbox's invitation.
* Inspiration from DropBox, jQuery tokeninput, etc.  
* This plugin was built to work with boostrap and is meant to be simple and highly extensible.
*
* Requires: underscore, jquery, jquery UI (or a way to set outerWidth), zurb textchanged event
* ============================================================
* Copyright 2012 Takeoff, Inc.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
* ============================================================ */

!function ($) {

    "use strict"; // jshint ;_;

    // Keys "enum"
    var controlKeys = {
        "backspace": 8,
        "delete": 46,
        "tab": 9,
        "enter": 13,
        "escape": 27,
        "space": 32,
        "pageUp": 33,
        "pageDown": 34,
        "end": 35,
        "home": 36,
        "left": 37,
        "up": 38,
        "right": 39,
        "down": 40,
        "numpadEnter": 108,
        "comma": 188
    };

    /* TOKENIZER CLASS DEFINITION
    * ========================= */

    var Tokenizer = function (element, options) {
        var plugin = this;
        this.$element = $(element)
        this.options = options

        this.keysCausingParse = {};
        //take the strings in options.keysCausingParse and convert them to an object that can be used as a quick set to look up whether a key was pressed
        _.each(options.keysCausingParse, function (keyName) {
            this.keysCausingParse[controlKeys[keyName]] = {};
        }, this);

        this.container = $(element).wrap("<div class='tokenizer-container'></div>").parent().width($(element).outerWidth())
        this.container.click(function (event) {
            //if the event originated from the container, someone clicked outside a token but not in the input. 
            if (event.target === this) {
                plugin.input.focus();
            }
        })
        this.input = $("<input type='text' spellcheck='0' />").appendTo(this.container).on('textchange', function (event, previousValue) {
            //textchange is a nice plugin that handles keyup as well as pasting.  provides a simple new event and i love it
            plugin.textchanged(event, previousValue);
        }).on('keydown', function (event) {
            plugin.inputKeyPressed(event);
        }).on('focus', function (event) {
            plugin.deselectTokens();
        });

        this.$element.hide();
        this.resizeInput();
        this.checkPlaceholderText();
    }

    Tokenizer.prototype = {
        hasInput: function () {
            return $.trim(this.input.val()).length > 0;
        }
        , checkPlaceholderText: function () {
            if (this.tokens().length == 0)
                this.input.attr("placeholder", this.options.placeholderText);
            else
                this.input.attr("placeholder", "");
        }
        , textchanged: function (event, previousValue) {
            if (this.input.val().length - (previousValue || '').length > 10)//a paste situation.  the number was chosen because typing fast resulted often in multiple key differences
            {
                if (this.hasInput()) {
                    this.parseInputIntoTokens();
                }
            }
            this.resizeInput();
            this.updateElementValue();
        }
        , inputKeyPressed: function (event) {
            //here we don't check for text changes.  instead we look for control keys, such as navigating around
            if ((event.which == controlKeys.left || event.which == controlKeys["delete"] || event.which == controlKeys.backspace) && !this.hasInput()) {
                var tokens = this.tokens();
                if (tokens.length) {
                    event.preventDefault(); //so browser doesn't go back in history (delete button did that in FF at least)
                    this.selectToken(tokens.last());
                }
            }
            if (_.has(this.keysCausingParse, event.which)) {
                if (this.hasInput()) {
                    event.preventDefault(); //so forms don't get submitted or whatever
                    this.parseInputIntoTokens(event.which);
                }
            }
        }
        , parseInputIntoTokens: function (keyCode) {
            if (this.hasInput()) {
                //if they pressed enter, parse the entire input.  otherwise just parse what you can
                var parse = this.options.parseInput(this.input.val(), keyCode);
                this.input.val(parse.remainingText);
                this.addTokens(parse.tokens);
            }
        }
        //updates the original text box's value by joining all the valid token values with a separator.  this is called automatically by the plugin
        , updateElementValue: function () {
            var values = this.validValues();
            this.$element.val(values.length ? values.join(this.options.tokenValueSeparator) : '');
        }
        , tokens: function () {
            return this.container.find('a');
        }
        //returns all valid token values.  useful for submitting forms
        , validValues: function () {
            var validData = [];
            this.tokens().each(function () {
                var token = $(this).data("token");
                if (token && token.valid) {
                    validData.push(token.value);
                }
            });
            if (this.hasInput()) {
                //if they pressed enter, parse the entire input.  otherwise just parse what you can
                var parse = this.options.parseInput(this.input.val(), undefined);
                if (parse.tokens && parse.tokens.length > 0) {
                    _.each(parse.tokens, function (token) {
                        if (token.valid)
                            validData.push(token.value);
                    }, this);
                }
            }
            return validData;
        }
        , resizeInput: function (text, data) {
            var tokens = this.tokens();
            if (!tokens.length) {
                this.input.width(this.container.width());
                return;
            }

            //get space between right edge of the last token and the right edge of the container.  that's the input width
            var lastToken = this.tokens().last();
            //available container width on this line is the width minus the right side of the right-most token.  position().left will include the padding which doesn't count toward container width, so we subtract the container left padding
            var width = this.container.width() - (lastToken.position().left - parseInt(this.container.css("paddingLeft")) + lastToken.outerWidth(true));

            //check for the situation where the text is bigger than the remaining space on the current line.  in this case we make it full size on its own line
            if (this.input.val().length > 0) {
                //borrowed from http://jsbin.com/ahaxe
                var inputSizeChecker = $("<tester/>")
                    .insertAfter(this.input)
                    .css({
                        position: "absolute",
                        top: -9999,
                        left: -9999,
                        width: "auto",
                        fontSize: this.input.css("fontSize"),
                        fontFamily: this.input.css("fontFamily"),
                        fontWeight: this.input.css("fontWeight"),
                        letterSpacing: this.input.css("letterSpacing"),
                        whiteSpace: "nowrap"
                    }).text(this.input.val());
                var inputWidth = inputSizeChecker.width();
                inputSizeChecker.remove();
                var comfortZone = 20; //without this, you could easily exceed the width if you type fast
                if (inputWidth + comfortZone > width) {
                    this.input.width(this.container.width());
                    return;
                }
            }

            var wiggleRoom = 2;
            width -= wiggleRoom; //a few pixels often made the difference between the input starting on a new line and being cut off or not.  obviously this is not ideal but it worked great
            if (width < this.options.minInputWidth)
                width = this.container.width();

            this.input.outerWidth(width); //this requires jquery UI or you'll have to find some plugin
        }
        , addTokens: function (tokens) {
            if (tokens && tokens.length > 0) {
                _.each(tokens, function (token) {
                    this.input.before(this.createToken(token))
                }, this);
            }
            this.resizeInput();
            this.checkPlaceholderText();
            this.updateElementValue();
        }
        , deleteToken: function (token) {
            token.remove();
            this.resizeInput();
            this.checkPlaceholderText();
        }
        , selectToken: function (token) {
            //deselect any existing tokens
            this.deselectTokens();
            token.addClass(this.options.tokenSelectedClass);
            this.resizeInput();
            token.focus();
        }
        , deselectTokens: function () {
            this.container.find('a.' + this.options.tokenSelectedClass).removeClass(this.options.tokenSelectedClass);
        }
        , isTokenSelected: function (token) {
            return token.hasClass(this.options.tokenSelectedClass);
        }
        , createToken: function (token) {
            var plugin = this;
            var tokenElement = $('<a class="tokenizer-token" tabindex="-1" href="#"></a>').data("token", token)//Error, Success, hover, selected
            if (!token.valid)
                tokenElement.addClass(plugin.options.tokenInvalidClass)
            $('<span class="tokenizer-token-title"></span>').text(token.text).appendTo(tokenElement)
            $('<span class="' + plugin.options.tokenCloseButtonClass + '"></span>').text('x').click(function () {
                plugin.deselectTokens();
                plugin.deleteToken(tokenElement);
                if (plugin.tokens().length == 0)
                    plugin.input.focus();
                return false;
            }).appendTo(tokenElement)

            tokenElement.click(function (event) {
                plugin.selectToken(tokenElement);
                return false;
            }).on('keydown', function (event) {
                switch (event.which) {
                    case controlKeys["backspace"]:
                        event.preventDefault(); //avoid the browser thinking we want to navigate Back
                        //backspace deletes current token and then selects the previous one
                        if (plugin.isTokenSelected(tokenElement)) {
                            var prev = plugin.previousToken(tokenElement);
                            if (prev.length)
                                plugin.selectToken(prev);
                            else {
                                var next = plugin.nextToken(tokenElement);
                                if (next.length)
                                    plugin.selectToken(next);
                            }
                        }
                        plugin.deleteToken(tokenElement);
                        if (plugin.tokens().length == 0)
                            plugin.input.focus();
                        break;
                    case controlKeys["delete"]:
                        //when deleting a selected token, the token that fills its spot (first on right or on left), should be selected.  this is basically the opposite of backspace
                        if (plugin.isTokenSelected(tokenElement)) {
                            var next = plugin.nextToken(tokenElement);
                            if (next.length)
                                plugin.selectToken(next);
                            else {
                                var prev = plugin.previousToken(tokenElement);
                                if (prev.length)
                                    plugin.selectToken(prev);
                            }
                        }
                        plugin.deleteToken(tokenElement);
                        if (plugin.tokens().length == 0)
                            plugin.input.focus();
                        break;
                    case controlKeys.left:
                        var prev = plugin.previousToken(tokenElement);
                        if (prev.length)
                            plugin.selectToken(prev);
                        break;
                    case controlKeys.right:
                    case controlKeys.tab:
                        event.preventDefault(); //necessary for tab
                        var next = plugin.nextToken(tokenElement);
                        if (next.length)
                            plugin.selectToken(next);
                        else
                            plugin.input.focus();
                        break;
                    case controlKeys["escape"]:
                        if (plugin.isTokenSelected(tokenElement)) {
                            plugin.deselectTokens();
                        }
                        break;
                }
            });

            return tokenElement
        }
        , previousToken: function (token) {
            return token.prev('a');
        }
        , nextToken: function (token) {
            return token.next('a');
        }
    }


    /* TOKENIZER PLUGIN DEFINITION
    * ========================== */

    $.fn.tokenizer = function (option) {
        return this.each(function () {
            var $this = $(this), data = $this.data('tokenizer'), options = $.extend({}, $.fn.tokenizer.defaults, typeof option == 'object' && option)
            if (!data)
                $this.data('tokenizer', (data = new Tokenizer(this, options)))
        })
    }

    $.fn.tokenizer.defaults = {
        minInputWidth: 30,
        tokenSelectedClass: 'tokenizer-token-selected',
        tokenInvalidClass: 'tokenizer-token-invalid',
        tokenCloseButtonClass: 'tokenizer-token-close',
        keysCausingParse: ["tab", "space", "enter", "numpadEnter"],
        tokenValueSeparator: ";",
        placeholderText: "",
        //takes input and parses the necessary tokens out.  the default function is pretty dumb
        parseInput: function (text, keyPressed) {
            return {
                tokens: [{
                    text: text,
                    value: text,
                    valid: true
                }],
                remainingText: ''
            };
        }
    }

    $.fn.tokenizer.Constructor = Tokenizer

    //add a custom validator method for the popular jquery validation plugin
    if ($.validator) {
        //validToken method checks for at least one valid token in the plugin
        $.validator.addMethod(
            "validToken",
            function (value, element) {
                return $(element).data('tokenizer').validValues().length > 0;
            },
            'Required'
        );
    }

} (window.jQuery);


