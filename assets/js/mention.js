﻿/* including the old bootstrap typeahead because its used by mention.js */
!function($){

    "use strict"; // jshint ;_;


    /* TYPEAHEAD PUBLIC CLASS DEFINITION
     * ================================= */

    var userTypeahead = function (element, options) {
        this.$element = $(element)
        this.options = $.extend({}, $.fn.usertypeahead.defaults, options)
        this.matcher = this.options.matcher || this.matcher
        this.sorter = this.options.sorter || this.sorter
        this.highlighter = this.options.highlighter || this.highlighter
        this.updater = this.options.updater || this.updater
        this.source = this.options.source
        this.$menu = $(this.options.menu)
        this.shown = false
        this.listen()
    }

    userTypeahead.prototype = {

        constructor: userTypeahead

    , select: function () {
        var val = this.$menu.find('.active').attr('data-value')

        //remove after last space
        var curVal = "";
        var curWords = this.$element.val().split(' ');
        if (curWords.length > 1) {
            var msg = this.$element.val();
            curVal = this.$element.val().substr(0, msg.lastIndexOf(' ')) + ' ';
        }

        this.$element
          .val(curVal + this.updater(val))
          .change()
        return this.hide()
    }

    , updater: function (item) {
        return item
    }

    , show: function () {
        var pos = $.extend({}, this.$element.position(), {
            height: this.$element[0].offsetHeight
        })

        this.$menu
          .insertAfter(this.$element)
          .css({
              top: pos.top + pos.height
          , left: pos.left
          })
          .show()

        this.shown = true
        return this
    }

    , hide: function () {
        this.$menu.hide()
        this.shown = false
        return this
    }

    , lookup: function (event) {
        var items

        this.query = this.$element.val();

        //update the query to only be the current word
        var curText = this.$element.val().split(" ");
        if (curText.length > 0) {
            this.query = curText[curText.length - 1];
        }

        if (!this.query || this.query.length < this.options.minLength) {
            return this.shown ? this.hide() : this
        }

        items = $.isFunction(this.source) ? this.source(this.query, $.proxy(this.process, this)) : this.source

        return items ? this.process(items) : this
    }

    , process: function (items) {
        var that = this

        items = $.grep(items, function (item) {
            return that.matcher(item)
        })

        items = this.sorter(items)

        if (!items.length) {
            return this.shown ? this.hide() : this
        }

        return this.render(items.slice(0, this.options.items)).show()
    }

    , matcher: function (item) {
        return ~item.toLowerCase().indexOf(this.query.toLowerCase())
    }

    , sorter: function (items) {
        var beginswith = []
          , caseSensitive = []
          , caseInsensitive = []
          , item

        while (item = items.shift()) {
            if (!item.toLowerCase().indexOf(this.query.toLowerCase())) beginswith.push(item)
            else if (~item.indexOf(this.query)) caseSensitive.push(item)
            else caseInsensitive.push(item)
        }

        return beginswith.concat(caseSensitive, caseInsensitive)
    }

    , highlighter: function (item) {
        var query = this.query.replace(/[\-\[\]{}()*+?.,\\\^$|#\s]/g, '\\$&')
        return item.replace(new RegExp('(' + query + ')', 'ig'), function ($1, match) {
            return '<strong>' + match + '</strong>'
        })
    }

    , render: function (items) {
        var that = this

        items = $(items).map(function (i, item) {
            i = $(that.options.item).attr('data-value', item)
            i.find('div').html(that.highlighter(item))
            return i[0]
        })

        items.first().addClass('active')
        this.$menu.html(items)
        return this
    }

    , next: function (event) {
        var active = this.$menu.find('.active').removeClass('active')
          , next = active.next()

        if (!next.length) {
            next = $(this.$menu.find('li')[0])
        }

        next.addClass('active')
    }

    , prev: function (event) {
        var active = this.$menu.find('.active').removeClass('active')
          , prev = active.prev()

        if (!prev.length) {
            prev = this.$menu.find('li').last()
        }

        prev.addClass('active')
    }

    , listen: function () {
        this.$element
          .on('focus',    $.proxy(this.focus, this))
          .on('blur',     $.proxy(this.blur, this))
          .on('keypress', $.proxy(this.keypress, this))
          .on('keyup',    $.proxy(this.keyup, this))

        if (this.eventSupported('keydown')) {
            this.$element.on('keydown', $.proxy(this.keydown, this))
        }

        this.$menu
          .on('click', $.proxy(this.click, this))
          .on('mouseenter', 'li', $.proxy(this.mouseenter, this))
          .on('mouseleave', 'li', $.proxy(this.mouseleave, this))
    }

    , eventSupported: function(eventName) {
        var isSupported = eventName in this.$element
        if (!isSupported) {
            this.$element.setAttribute(eventName, 'return;')
            isSupported = typeof this.$element[eventName] === 'function'
        }
        return isSupported
    }

    , move: function (e) {
        if (!this.shown) return

        switch(e.keyCode) {
            case 9: // tab
            case 13: // enter
            case 27: // escape
                e.preventDefault()
                break

            case 38: // up arrow
                e.preventDefault()
                this.prev()
                break

            case 40: // down arrow
                e.preventDefault()
                this.next()
                break
        }

        e.stopPropagation()
    }

    , keydown: function (e) {
        this.suppressKeyPressRepeat = ~$.inArray(e.keyCode, [40,38,9,13,27])
        this.move(e)
    }

    , keypress: function (e) {
        if (this.suppressKeyPressRepeat) return
        this.move(e)
    }

    , keyup: function (e) {
        switch(e.keyCode) {
            case 40: // down arrow
            case 38: // up arrow
            case 16: // shift
            case 17: // ctrl
            case 18: // alt
                break

            case 9: // tab
            case 13: // enter
                if (!this.shown) return
                this.select()
                break

            case 27: // escape
                if (!this.shown) return
                this.hide()
                break

            default:
                this.lookup()
        }

        e.stopPropagation()
        e.preventDefault()
    }

    , focus: function (e) {
        this.focused = true
    }

    , blur: function (e) {
        this.focused = false
        if (!this.mousedover && this.shown) this.hide()
    }

    , click: function (e) {
        e.stopPropagation()
        e.preventDefault()
        this.select()
        this.$element.focus()
    }

    , mouseenter: function (e) {
        this.mousedover = true
        this.$menu.find('.active').removeClass('active')
        $(e.currentTarget).addClass('active')
    }

    , mouseleave: function (e) {
        this.mousedover = false
        if (!this.focused && this.shown) this.hide()
    }

    }


    /* TYPEAHEAD PLUGIN DEFINITION
     * =========================== */

    var old = $.fn.usertypeahead

    $.fn.usertypeahead = function (option) {
        return this.each(function () {
            var $this = $(this)
              , data = $this.data('usertypeahead')
              , options = typeof option == 'object' && option
            if (!data) $this.data('usertypeahead', (data = new userTypeahead(this, options)))
            if (typeof option == 'string') data[option]()
        })
    }

    $.fn.usertypeahead.defaults = {
        source: []
    , items: 8
    , menu: '<ul class="tt-dropdown-menu dropdown-menu"></ul>'
    , item: '<li class="tt-suggestion"><div></div></li>'
    , minLength: 1
    }

    $.fn.usertypeahead.Constructor = userTypeahead


    /* TYPEAHEAD NO CONFLICT
     * =================== */

    $.fn.usertypeahead.noConflict = function () {
        $.fn.usertypeahead = old
        return this
    }


    /* TYPEAHEAD DATA-API
     * ================== */

    $(document).on('focus.usertypeahead.data-api', '[data-provide="usertypeahead"]', function (e) {
        var $this = $(this)
        if ($this.data('usertypeahead')) return
        $this.usertypeahead($this.data())
    })

}(window.jQuery);

//end old typeahead - renamed to usertypeahead

//start of mention
;(function($) {
    $.fn.extend({
        mention: function(options) {
            this.opts = {
                users: [],
                delimiter: '@',
                sensitive: true,
                emptyQuery: false,
                queryBy: ['name', 'username'],
                typeaheadOpts: {}
            };

            var settings = $.extend({}, this.opts, options),
                _checkDependencies = function() {
                    if (typeof $ == 'undefined') {
                        throw new Error("jQuery is Required");
                    }
                    else {
                        if (typeof $.fn.usertypeahead == 'undefined') {
                            throw new Error("Typeahead is Required");
                        }
                    }
                    return true;
                },
                _extractCurrentQuery = function(query, caratPos) {
                    var i;
                    for (i = caratPos; i >= 0; i--) {
                        if (query[i] == settings.delimiter) {
                            break;
                        }
                    }
                    return query.substring(i, caratPos);
                },
                _matcher = function(itemProps) {
                    var i;
                    
                    if(settings.emptyQuery){
                        var q = (this.query.toLowerCase()),
                                caratPos = this.$element[0].selectionStart,
                                lastChar = q.slice(caratPos-1,caratPos);
                        if(lastChar==settings.delimiter){
                            return true;
                        }
                    }
                    
                    for (i in settings.queryBy) {
                        if (itemProps[settings.queryBy[i]]) {
                            var item = itemProps[settings.queryBy[i]].toLowerCase(),
                                usernames = (this.query.toLowerCase()).match(new RegExp(settings.delimiter + '\\w+', "g")),
                                j;
                            if ( !! usernames) {
                                for (j = 0; j < usernames.length; j++) {
                                    var username = (usernames[j].substring(1)).toLowerCase(),
                                        re = new RegExp(settings.delimiter + item, "g"),
                                        used = ((this.query.toLowerCase()).match(re));

                                    if (item.indexOf(username) != -1 && used === null) {
                                        return true;
                                    }
                                }
                            }
                        }
                    }
                },
                _updater = function(item) {
                    var data = this.query,
                        caratPos = this.$element[0].selectionStart,
                        i;
                    
                    for (i = caratPos; i >= 0; i--) {
                        if (data[i] == settings.delimiter) {
                            break;
                        }
                    }
                    var replace = data.substring(i, caratPos),
                            textBefore = data.substring(0, i),
                            textAfter = data.substring(caratPos),
                            data = textBefore + settings.delimiter + item + textAfter;
                            
                    this.tempQuery = data;

                    return data;
                },
                _sorter = function(items) {
                    if (items.length && settings.sensitive) {
                        var currentUser = _extractCurrentQuery(this.query, this.$element[0].selectionStart).substring(1),
                            i, len = items.length,
                            priorities = {
                                highest: [],
                                high: [],
                                med: [],
                                low: []
                            }, finals = [];
                        if (currentUser.length == 1) {
                            for (i = 0; i < len; i++) {
                                var currentRes = items[i];

                                if ((currentRes.username[0] == currentUser)) {
                                    priorities.highest.push(currentRes);
                                }
                                else if ((currentRes.username[0].toLowerCase() == currentUser.toLowerCase())) {
                                    priorities.high.push(currentRes);
                                }
                                else if (currentRes.username.indexOf(currentUser) != -1) {
                                    priorities.med.push(currentRes);
                                }
                                else {
                                    priorities.low.push(currentRes);
                                }
                            }
                            for (i in priorities) {
                                var j;
                                for (j in priorities[i]) {
                                    finals.push(priorities[i][j]);
                                }
                            }
                            return finals;
                        }
                    }
                    return items;
                },
                _render = function(items) {
                    var that = this;
                    items = $(items).map(function(i, item) {

                        i = $(that.options.item).attr('data-value', item.username);

                        var _linkHtml = $('<div />');

                        if (item.image) {
                            _linkHtml.append('<img class="mention_image" src="' + item.image + '">');
                        }
                        if (item.name) {
                            _linkHtml.append('<b class="mention_name">' + item.name + '</b>');
                        }
                        if (item.username) {
                            _linkHtml.append('<span class="mention_username"> ' + settings.delimiter + item.username + '</span>');
                        }

                        i.find('div').html(that.highlighter(_linkHtml.html()));
                        return i[0];
                    });

                    items.first().addClass('active');
                    this.$menu.html(items);
                    return this;
                };

            $.fn.usertypeahead.Constructor.prototype.render = _render;

            return this.each(function() {
                var _this = $(this);
                if (_checkDependencies()) {
                    _this.usertypeahead($.extend({
                        source: settings.users,
                        matcher: _matcher,
                        updater: _updater,
                        sorter: _sorter
                    }, settings.typeaheadOpts));
                }
            });
        }
    });
})(jQuery);