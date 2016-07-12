;(function ( $, window, document, undefined ) {
    
    var defaults = {
        orientation: 'left',
        mode: 'push',
        static: false
    };

    // The actual plugin constructor
    function Slidepanel( $element, options ) {
        this.$element = $element;
        this.options = $.extend( {}, defaults, options) ;
        this._defaults = defaults;
        this.init();
    }

    Slidepanel.prototype.init = function () {
        
        var base = this;

        if($('#slidepanel').length == 0){
            var panel_html = '<div id="slidepanel" class="cb_slide_panel"><div class="wrapper"><a href="#" class="close close-panel">Close</a><div class="inner"><div class="wrapper"></div></div></div></div>';
            $(panel_html).hide().appendTo($('body'));    
        }

        this.$panel = $('#slidepanel');
        this.$body = $('body');
        this.$body_position = this.$body.css('position');

        //hide the panel and set orientation class for display
        this.$panel.hide().addClass('panel_' + this.options.orientation);
        
        //set current trigger link to false for the current panel
        this.$panel.data('slidepanel-current', false);
        this.$panel.data('slidepanel-loaded', false);
        
        //reset any defined a positions
        this.$panel.css('left', '').css('right', '').css('top', '').css('bottom', '');

        //set a default top value for left and right orientations
        //and set the starting position based on element width
        if(this.options.orientation == 'left' || this.options.orientation == 'right') {
            var options = {};
            options['top'] = 0;
            options[this.options.orientation] = -this.$panel.width();
            this.$panel.css(options);
        }

        //set a default left value for top and bottom orientations
        //and set the starting position based on element height
        if(this.options.orientation == 'top' || this.options.orientation == 'bottom') {
            var options = {};
            options['left'] = 0;
            options[this.options.orientation] = -this.$panel.height();
            this.$panel.css(options);
        }

        //bind click event to trigger ajax load of html content
        //and panel display to any elements that have the attribute rel="panel"
        $(this.$element).on('click', function(e) {
            e.preventDefault();
             
            //if the request mode is static
            if(base.options.static) { 
                //show the panel
                base.expand();
            }
            // if the reques mode is ajax 
            else {
                //load the external html
                base.load();
            };
        });

        //listen for a click on the close buttons for this panel
        $("#slidepanel").on("click", '.close-panel', function (e) {
            e.preventDefault();
            base.collapse();
        });
    };

    Slidepanel.prototype.load = function() {
            var base = this;
            //if the current trigger element is the element that just triggered a load
            if(this.$panel.data('slidepanel-current') == this.$element) {
                //collapse the current panel
                this.collapse();
                return;
            } else {
                //show the slide panel
                this.expand();
                //get the target url
                var href = $(this.$element).attr('href');

                //prevent an ajax request if the current URL is the the target URL
                if(this.$panel.data('slidepanel-loaded') !== href){
                    //load the content from the target url, and update the panel html
                    $('.inner .wrapper', this.$panel).html('').load(href, function() {
                        //remove the loading indicator
                        base.$panel.removeClass('loading');
                        //set the current loaded URL to the target URL
                        base.$panel.data('slidepanel-loaded', href);

                        var pstType = $("#pst-type").val();
                        switch( pstType )
                        {
                            case "msg":
                                loadPlayerTypeahead();
                                break;
                            case "match":
                                loadMatchupPlayerTypeahead();
                                loadMatchupInviteTypeahead();
                                break;
                        }

                        loadUserTypeahead();
                    });
                //  the current URL is already loaded
                } else {
                    //remove the loading indicator
                    this.$panel.removeClass('loading');
                }
            }
            //set the current source element to this element that triggered the load
            this.$panel.data('slidepanel-current', this.$element);
    };


    Slidepanel.prototype.expand = function() {
        var base = this;
                //set the css properties to animatate

        var panel_options = {};
        var body_options = {};
        panel_options.visible = 'show';
        panel_options[this.options.orientation] = 0;
        body_options[this.options.orientation] = (this.options.orientation == 'top' || this.options.orientation == 'bottom') ? this.$panel.height() : this.$panel.width();
        
        //if the animation mode is set to push, we move the body in relation to the panel
        //else the panel is overlayed on top of the body
        if(this.options.mode == 'push'){
            //animate the body position in relation to the panel dimensions
            this.$body.css('position', 'absolute').animate(body_options, 50);
        }

        //animate the panel into view
        this.$panel.addClass('loading').animate(panel_options, 250, function() {
            //show the panel's close button
            $('.close', base.$panel).fadeIn(250);
        });

        $("#share-message").val("");
        $('#players').tagsinput('removeAll');
        $(".message-charNum").html("140").removeClass("warn");
        $(".bootstrap-tagsinput input.tt-query").attr("placeholder", "+ Add a player");
        $('#hidden-modal').modal('show');
    };

    Slidepanel.prototype.collapse = function() {
        //hide the close button for this panel
        $('.close', this.$panel).hide();
        $('#hidden-modal').modal('hide');

        //set the css properties to animatate
        var panel_options = {};
        var body_options = {};
        panel_options.visible = 'hide';
        panel_options[this.options.orientation] = -(this.$panel.width() + 40);
        body_options[this.options.orientation] = 0;
        
        //if the animation mode is push, move the document body back to it's original position
        if(this.options.mode == 'push'){
            this.$body.css('position', this.$body_position).animate(body_options, 50);
        }
        //animate the panel out of view
        this.$panel.animate(panel_options, 50).data('slidepanel-current', false);
    };

    $.fn['slidepanel'] = function (options) {
           return this.each(function () {
            if (!$.data(this, 'plugin_slidepanel')) {
                $.data(this, 'plugin_slidepanel', new Slidepanel( this, options ));
            }
        });
    }

})(jQuery, window);

function loadUserTypeahead() {
    $.ajaxSetup({
        cache: false
    });

    var $textArea = $(".users-typeahead");
    if ($textArea.length > 0) {
        setCaretToPos($textArea.get(0), ($textArea.val().length));
        $textArea.limiter(140, $(".message-charNum"));
    }

    //load the user typeahead
    $.getJSON("/assets/data/users.json", function (data) {
        $(".users-typeahead").mention({
            delimiter: '@',
            sensitive: false,
            users: data
        });
    });
}

function loadMatchupPlayerTypeahead() {
    var template = '<img class="typeahead-avatar" src="{{profileImg}}" alt=""><span class="typeahead-bio">{{name}} | {{position}} | {{team}}</span>';
    var nflPlayers = Hogan.compile(template);

    $('#player1, #player2').typeahead({
        prefetch: '/assets/data/players.json',
        template: nflPlayers.render.bind(nflPlayers),
        limit: 10,
        engine: Hogan
    });

    $('#player1').bind('typeahead:selected', function (obj, datum, name) {
        $("#player1-selected-name").text(datum.fullName);
        $("#player1-selected-img").attr("src", datum.profileImgLarge);
        $("#player1-selected-bio").text(datum.number + " " + datum.position + " " + datum.team);
        $("#player1-id").val(datum.accountID);

        $('#player1').typeahead('destroy');
        $(obj.target).hide('fast', function () { $("#player1-selected").show(); });
    });

    $('#player2').bind('typeahead:selected', function (obj, datum, name) {
        $("#player2-selected-name").text(datum.fullName);
        $("#player2-selected-img").attr("src", datum.profileImgLarge);
        $("#player2-selected-bio").text(datum.number + " " + datum.position + " " + datum.team);
        $("#player2-id").val(datum.accountID);

        $('#player2').typeahead('destroy');
        $(obj.target).hide('fast', function () { $("#player2-selected").show(); });

        if ($("#player1-id").val().length > 0) {
            $("#share-matchup").removeClass("disabled");
            $(".matchup-invite").slideDown("slow");
        }
    });
}

function loadPlayerTypeahead() {
    $.ajaxSetup({
        cache: false
    });

    //load the typeahead template
    // construct template string
    var template = '<img class="typeahead-avatar" src="{{profileImg}}" alt=""><span class="typeahead-bio">{{name}} | {{position}} | {{team}}</span>';
    var nflPlayers = Hogan.compile(template);

    //load the player typeahead
    $('#players').tagsinput({
        itemValue: 'accountID',
        allowRemove: $("#enablePlayerDelete").val(),
        itemText: function (item) {
            var item = '<img class="user-avatar-small" src="' + item.profileImg + '" /><span>' + item.shortName + '</span>';
            return item;
        }
    });
    $('#players').tagsinput('input').typeahead({
        prefetch: '/assets/data/players.json',
        template: nflPlayers.render.bind(nflPlayers),
        limit: 10,
        engine: Hogan
    }).bind('typeahead:selected', $.proxy(function (obj, datum) {
        this.tagsinput('add', datum);
        this.tagsinput('input').typeahead('setQuery', '');
    }, $('#players')));

    //get any prepopulated players for the data attribute
    $("input.player-tags").each(function (index) {
        var playerName = $(this).attr("data-name");
        console.log(playerName);
        $('#players').tagsinput('add', { name: playerName, value: playerName, username: "", shortName: playerName, accountID: $(this).val(), profileImg: $(this).attr("data-img") });
    });
}

//text limiter
$.fn.extend({
    limiter: function (limit, elem) {
        $(this).on("input propertychange", function () {
            setCount(this, elem);
        });
        function getTextLength(chars) {
            var count = chars.length;
            var domains = [".com", ".org", ".net", ".mil", ".edu"];
            var msg = new String(chars);
            
            var words = msg.split(' ');
            words.forEach(function (word) {
                for (var i = 0; i < domains.length; i++) {
                    if (word.indexOf(domains[i]) != -1) {
                        //update the count of the message to account for a long url
                        if (word.length > 20) {
                            count = (count - word.length) + 20;
                        }
                    }
                }
            });

            return count;
        }
        function setCount(src, elem) {
            var chars = getTextLength(src.value);
            if (chars > limit) {
                $(elem).addClass("warn");
                $("#share-post").addClass("disabled");
            } else {
                $(elem).removeClass("warn");
                if (chars == 0) {
                    $("#share-post").addClass("disabled");
                } else {
                    $("#share-post").removeClass("disabled");
                }
            }
            elem.html(limit - chars);
        }
        setCount($(this)[0], elem);
    }
});