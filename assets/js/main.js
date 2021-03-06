﻿//object for the home page
var pollRate = 40000;
var streamTimer;

$(document).ready(function () {

    $.ajaxSetup({
        cache: false
    });

    loadSearchTypeahead();
    loadPlayersTypeahead();
    loadMatchupPlayersTypeahead();
    loadMatchupInviteFilter();

    //tooltips
    $("[rel='tooltip']").tooltip();

    //fitler matchups
    $('ul.stream-filter a.stream-filter-item').click(function () {
        $('ul.nav.stream-filter li').removeClass('active');
        $(this).parent().addClass('active');
        $("#filter-matchup-spinner").spin(matchupFilterSpin);

        loadMatchupStream($(this).attr('data-position'));

        //send analytics event
        ga('send', {
            hitType: 'event',
            eventCategory: 'Filter',
            eventAction: 'click',
            eventLabel: 'Matchup Filter'
        });

        return false;
    });

    //filter player page
    $('ul.player-filter a.stream-filter-item').click(function () {
        $('ul.nav.player-filter li').removeClass('active');
        $(this).parent().addClass('active');
        $("#filter-matchup-spinner").spin(matchupFilterSpin);

        loadPlayerStream($(this).attr('data-player'), $(this).attr('data-view'));

        //send analytics event
        ga('send', {
            hitType: 'event',
            eventCategory: 'Filter',
            eventAction: 'click',
            eventLabel: 'Player Filter'
        });

        return false;
    });

    //full page modal for message and matchup
    $(document).on('click', '.reply-message-panel', function () {
        var id = $(this).attr('data-msg');
        $('#prnt-id').val(id);
        $('#msgType').val($(this).attr('data-msg-type'));

        $('#players').tagsinput('removeAll');
        $('#players').tagsinput('input').attr('readonly', false).val('');

        //get any prepopulated players for the data attribute
        $('div#match-' + id).find('input.player-tags').each(function (index) {
            var playerName = $(this).attr("data-name");
            $('#players').tagsinput('add', { name: playerName, value: playerName, username: "", shortName: playerName, id: $(this).val(), profileImage: $(this).attr("data-img") });
            if (index == 0) {
                $('#players').tagsinput('input').removeAttr('placeholder').attr('readonly', true);
            }
        });
    });

    $(".modal-fullscreen").on('show.bs.modal', function () {
        setTimeout(function () {
            $(".modal-backdrop").addClass("modal-backdrop-fullscreen");
        }, 0);
        loadUserTypeahead();
        loadMatchupPlayersTypeahead();
    });

    //close message modal
    $("#modal-message").on('hidden.bs.modal', function () {
        $(".modal-backdrop").addClass("modal-backdrop-fullscreen");
        $(".users-typeahead").val("");
        $('.image-preview').popover('hide');
        $('.image-preview').attr("data-content", "");
        $('#players').tagsinput('removeAll');
        $('#input-file-preview').val('');
        $('#prnt-id').val('');
        $(".bootstrap-tagsinput input.tt-query").attr("placeholder", "+ Add Player(s) included in the message");
    });

    //close matchup modal - reset everything
    $("#modal-matchup").on('hidden.bs.modal', function () {
        $(".modal-backdrop").addClass("modal-backdrop-fullscreen");
        $('#matchup-invite').hide();
        $('#matchup-select').show();
        $('#askFilterSearch').val('');
        $('#matchup-select').find('div.selected-player').hide();
        $('#matchup-select').find('input.matchup-player-select').val('').show();
        $('#player3-id').parents('li.list-group-item').hide();
        $('#player4-id').parents('li.list-group-item').hide();
        $('#add-matchup-player').parent().show();
        $('#invite-user-list').empty();
        syncCheckmarks();
        $('input.player-id').val('');
        $('#matchup-exists-alert').hide();
    });

    //image preview
    $(document).on('click', '#close-preview', function () {
        $('.image-preview').popover('hide');
        $('.image-preview').attr("data-content", "");
    });

    $(function () {
        // Create the close button
        var closebtn = $('<button/>', {
            type: "button",
            text: 'x',
            id: 'close-preview',
            style: 'font-size: initial; display: block;',
        });

        closebtn.attr("class", "close pull-right");
        // Set the popover default content
        $('.image-preview').popover({
            trigger: 'manual',
            html: true,
            title: "<strong>Preview</strong>" + $(closebtn)[0].outerHTML,
            content: "There's no image",
            placement: 'top'
        });
       
        // Create the preview image
        $("#input-file-preview").change(function () {
            var img = $('<img/>', {
                id: 'dynamic',
                width: 250,
                height: 200
            });
            var file = this.files[0];
            var reader = new FileReader();
            // Set preview image into the popover data-content
            reader.onload = function (e) {
                img.attr('src', e.target.result);
                $(".image-preview").attr("data-content", $(img)[0].outerHTML).popover("show");
            }
            reader.readAsDataURL(file);
        });
    });

    //submit the new message
    $("#modal-message").on("click", '#share-post', function (e) {
        e.preventDefault();

        var message = $("#share-message").val();
        
        if ($(this).hasClass("disabled") || message.length <= 0) {
            return false;
        }

        var parentID = $("#prnt-id").val();
        var msgType = $("#msgType").val();
        var $parentMsg = (msgType == "general") ? $("#msg-" + parentID) : $("#match-" + parentID);
        var inline = ($parentMsg.attr("data-parent") === undefined) ? false : true;

        var file = $('#input-file-preview')[0].files[0];

        task.postUserMessage(message, $("#players").val(), parentID, msgType, inline, file, function (data) {
            if (msgType == "general" || msgType == "matchup") {
    
                if (parentID == 0) {
                    $(".stream-messages").prepend(data.StreamData);
                } else {
                    $("div.message-post-" + parentID).removeClass("empty-message-block");
                    $("div." + parentID + "-message-block").find("div.show-message-block").append(data.StreamData);
                }
         
                //send out email notifications
                task.sendNotificationEmail(data.ID, msgType, function (emailSent) {
                });                
            }

            showNotice("Message Posted", "Thanks for sharing. Your message has been posted.");
        });
        
        $("#modal-message").modal('hide');
        return false;
    });

    //show matchup votes
    $("body").on("click", '.ms-action-details', function (e) {     
        var $parent = $(this).parents("div.stats");
        $parent.find("div.vote-details").slideDown();
        $(this).find("i.glyphicon").removeClass("glyphicon-chevron-down").addClass("glyphicon-chevron-up");
        $(this).addClass("ms-action-hide-details");
        $(this).removeClass("ms-action-details");
        
        return false;
    });

    //hide matchup votes
    $("body").on("click", '.ms-action-hide-details', function (e) {  
        var $parent = $(this).parents("div.stats");
        $parent.find("div.vote-details").slideUp();
        $(this).find("i.glyphicon").removeClass("glyphicon-chevron-up").addClass("glyphicon-chevron-down");
        $(this).removeClass("ms-action-hide-details");
        $(this).addClass("ms-action-details");

        return false;
    });

    //show the conversation
    $(".message-list-stream").on("click", '.ms-action-conversation', function (e) {
        var matchupID = $(this).attr("data-mtch");
        var src = $(this).attr("data-src");

        var $parent = $(this).parents("div.matchup-message-block");
        $parent.find("div.show-message-block").hide("fast", function() {
            $parent.find("div.hidden-message-block").slideDown("slow");
        });
        $(this).addClass("ms-action-hideconversation").removeClass("ms-action-conversation").html("<i class='glyphicon glyphicon-chevron-up'></i> Hide comments");

        return false;
    });

    //hide the conversation
    $(".message-list-stream").on("click", '.ms-action-hideconversation', function (e) {

        var $parent = $(this).parents("div.matchup-message-block");
        
        $parent.find("div.hidden-message-block").slideUp(function () {
            $parent.find("div.show-message-block").show();
        });

        $(this).removeClass("ms-action-hideconversation").addClass("ms-action-conversation").html("<i class='glyphicon glyphicon-chevron-down'></i> " + $(this).attr("data-count") + " comments");

        return false;
    });

    //filter trending views
    $(".trending-view").on('click', function (e) {
        var listItem = $(this).parent();
        $(listItem).parent().children().removeClass("active");
        $(listItem).addClass("active");

        var pos = $(this).attr("data-position");
        task.getTrending(5, pos, function (data) {
            $("#trending-filter").empty();
            $("#trending-filter").html(data.Results);
        });
        return false;
    });

    //add a matchup ask invite
    $("#modal-matchup").on("click", ".user-invite", function (e) {

        //if user button already exists than remove
        var $inviteButton = $('#invite-user-list').find('button.invite-user-add[data-id="' + $(this).attr('data-id') + '"]');
        if ($inviteButton.length > 0) {
            $inviteButton.remove();
        } else {
            //add a checkmark and a button to the top with a delete x (if that is clicked it is removed)
            $('#invite-user-list').append('<button data-id="' + $(this).attr('data-id') + '" class="btn btn-default invite-user-add">' + $(this).attr('data-name') + '&nbsp;<span class="glyphicon glyphicon-remove" aria-hidden="true"></span></button>');
        }

        syncCheckmarks();

        return false;
    });

    $("#modal-matchup").on("click", ".invite-user-add", function (e) {
        $(this).remove();
        syncCheckmarks();
    });

    //send out matchup ask invites
    $("#modal-matchup").on("click", '#send-invites', function (e) {
        var inviteUsers = $("#invite-user-list button.invite-user-add")
             .map(function () { return $(this).attr("data-id"); }).get();

        showNotice("Invites Sent", "Your Ask requests have been sent.");
        var matchupID = $("#invite-user-list").attr("data-matchup");

        //send out invites if any exist
        if (inviteUsers.length > 0) {
            task.inviteAnswer(inviteUsers, matchupID, function () { });          
        }

        $("#modal-matchup").modal('hide');

        ga('send', {
            hitType: 'event',
            eventCategory: 'Invite',
            eventAction: 'click',
            eventLabel: 'Invites sent'
        });

        return false;
    });

    //add player to matchup
    $("#modal-matchup").on("click", '#add-matchup-player', function (e) {
        var $listItem = $(this).parent();
        $listItem.siblings("li.list-group-item").each(function (index) {
            if (!$(this).is(':visible')) {
                $(this).show();

                if (index == 3) {
                    $listItem.hide();
                }
                return false;
            }
        });
    });

    //add new matchup
    $("#modal-matchup").on("click", '#share-matchup', function (e) {
        var $btn = $(this).button('loading');
        var player1 = $("#player1-id").val();
        var player2 = $("#player2-id").val();
        var player3 = $("#player3-id").val();
        var player4 = $("#player4-id").val();

        if ($(this).hasClass("disabled") || player1.length <= 0 || player2.length <= 0) {
            return false;
        }

        task.addMatchupItem(player1, player2, player3, player4, $('#matchup-type').val(), function (data) {
            $btn.button('reset');
            if (data.Existing) {
                $('#matchup-exists-alert').show();
                $("#matchup-exists-link").attr('href', data.Link);
            } else {
                //add to stream
                $(".stream-matchups").prepend(data.MatchupData);

                showNotice("Matchup Posted", "Thanks for sharing. Your matchup has been posted.");

                //show the invite page
                $('#invite-user-list').attr('data-matchup', data.MatchupID)
                $('#searchlist').append(data.InviteData);

                $('#matchup-select').hide();
                $('#matchup-invite').show();
                
                $('.fb-share-button').attr('data-href', data.FacebookLink);
                $('a.ask-twitter-button').attr('href', data.TwitterLink);
                $.getScript("http://platform.twitter.com/widgets.js");
            }
        });

        return false;
    });

    //toggle register/login view
    $('#modal-register').on('show.bs.modal', function (event) {
        var link = $(event.relatedTarget)
        var showRegister = link.data('register')

        if (!showRegister) {
            $('#frmRegisterLogin').hide();
            $('#frmLoginReg').show();
        }
    });

    //close - login/register
    $("#modal-register").on('hidden.bs.modal', function () {
        $('#frmRegisterLogin').show();
        $('#frmLoginReg').hide();

        $("#login-error").hide();
        $("#forgot-password-controls").hide();
        $("#forgot-password-message").hide();
    });

    //register modal
    $("body").on("click", '#already-member-link', function (e) {
        $('#frmRegisterLogin').hide();
        $('#frmLoginReg').show();

        return false;
    });

    //forgot password
    $("body").on("click", '#forgot-password-link', function (e) {
        $("#forgot-password-message").hide();
        $("#forgot-password-error").hide();
        $("#forgot-password-controls").slideDown();
        return false;
    });

    $("body").on("click", '#btnGetPassword', function (e) {
        if ($("#forgot-username").val() == "") {
            $("#forgot-password-error").show();
        }
        else {
            task.sendPassword($("#forgot-username").val(), passwordSent);
        }
        return false;
    });

    //create new account
    $("body").on("click", '#btnCreateAccount', function (e) {
        var valid = validateForm("#frmRegisterLogin");
        if (valid) {
            task.validEmail($("#regemail").val(), function (data) {
                if (data.Valid) {
                    $("#frmRegisterLogin").submit();
                }
                else {
                    $(".email-validation-message").css('display', 'inline-block');
                }
            });
        }

        return false;
    });

    //login
    $("body").on("click", '#btnlogin', function (e) {
        $("#login-error").hide();
        var valid = validateForm("#frmLoginReg");
        if (valid) {
            task.validLogin($('#username').val(), $('#password').val(), function (data){
                if (data.Success) {
                    location.reload(true);
                }
                else {
                    $("#login-error").show();
                }
            });
        }
        
        return false;
    });

    $("form#frmSendInvite").submit(function () {
        var valid = validateForm("#frmSendInvite");
        if (valid) {
            $(".sending-spinner").show();
            $(".sending-spinner").spin(smalSpinnerBlackOpts);
            task.sendInvite($("#inviteEmail").val(), $("#inviteMsg").val(), inviteSent);
        }

        return false;
    });

    $("form#frmSendInviteRequest").submit(function () {
        var valid = validateForm("#frmSendInviteRequest");
        if (valid) {
            $(".sending-spinner").show();
            $(".sending-spinner").spin(smalSpinnerBlackOpts);
            task.sendInviteRequest($("#inviteRequestEmail").val(), function () {
                $(".sending-spinner").hide();
                $('#invite-request-sent').show();
                $("#btnSendInviteRequest").hide();
            });
        }

        return false;
    });

    //select a matchup starter in stream
    $("body").on("click", 'a.stream-select-starter', function (e) {
        if ($(this).hasClass('disabled'))
            return false;

        var $current = $(this).parents("div.matchup-item");
        $(this).addClass('active');
        $current.find('a.stream-select-starter').addClass('disabled');

        task.setStreamMatchupChoice($(this).attr("data-player-id"), $(this).attr("data-player-name"), $current.attr("data-matchup-id"), function (data) {
            task.sendMatchupVoteEmail(data.ID, data.UserVotedID);

            $current.find('.vote-count').show();
            var $total = $current.find('.vote-count-total');
            $total.text(parseInt($total.text()) + 1);

            $('#vote-' + data.ID + data.PlayerID).fadeOut(500, function () {
                var count = parseInt($(this).text()) + 1;
                $(this).text(count).fadeIn(500);
            });            
        });

        //send analytics event
        ga('send', {
            hitType: 'event',
            eventCategory: 'Vote',
            eventAction: 'click',
            eventLabel: 'Matchup Vote'
        });

        return false;
    });

    $("#invite-btn").click(function () {
        $("#invite-sent").hide();
        $("#inviteEmail").val("");
        $("#btnSendInvite").show();

        ga('send', {
            hitType: 'event',
            eventCategory: 'Invite',
            eventAction: 'click',
            eventLabel: 'Invites sent'
        });
    });

    $("#regemail").change(function () {
        var elem = $(this);
        $.when(elem.focusout()).then(function () {
            //now check if it exists already
            $.ajax({
                type: "POST",
                url: "/Account/EmailExists",
                data: "email=" + elem.val(),
                success: function (result) {
                    if (result.EmailExists) {
                        $(elem).parents("div.control-group").addClass("error");
                        $(elem).siblings(".validation-message").html("This email address is already registered.");
                        $(elem).siblings(".validation-message").css('display', 'inline-block');
                    }
                    else {
                        $(elem).siblings(".validation-message").hide();
                        $(elem).parents("div.control-group").removeClass("error");
                    }
                }
            });
        });
    });

    $(".vote-list").click(function () {
        var icon = $(this).find("i");
        if ($(icon).hasClass("icon-chevron-down")) {
            $(icon).removeClass("icon-chevron-down").addClass("icon-chevron-up");
        }
        else {
            $(icon).removeClass("icon-chevron-up").addClass("icon-chevron-down");
        }
    });
});

function syncCheckmarks() {
    $('#searchlist').find("i.badge").hide();
    $("#invite-user-list button.invite-user-add").each(function (index) {
        var userId = $(this).attr("data-id");
        $('a.user-invite[data-id="' + userId + '"]').find("i.badge").show();
    });
}

function loadMatchupStream(position) { 
    task.getMatchupStream(position, function (data) {
        //$("#matchup-list").attr("data-loaded", "true");
        $("#message-stream").html(data.StreamData);
        $("#filter-matchup-spinner").empty();
    });
}

function loadPlayerStream(id, view) {
    task.getPlayerStream(id, view, function (data) {
        $("div.stream-messages").html(data.StreamData);
        $("#filter-matchup-spinner").empty();
    });
}

function updateStreamCount(data) {
    if (data.UpdatesFound) {
        $("#updateBar").text(data.UpdateCount + " New Messages");
        $("#updateBar").show();
    }
}

function removePlayerStream(data) {
    if (data.Result) {
        $('.player-container[data-player="' + data.ID + '"]').remove();
    }
}

function updatePlayerStream(data) {
    var playerHeader = $('#'+ data.ID);
    //update the stream html and last time so it is current
    $(playerHeader).prepend(data.StreamData);
    $(playerHeader).attr("data-last", data.LastUpdateTicks);
}

function updateStream(data) {
   
    //add new messages
    $("#message-stream").prepend(data.StreamData);
    $("#updateBar").button("reset");
    $("#updateBar").hide();

    $("#message-stream").attr("data-ticks", data.LastDate);
    $("[rel='tooltip']").tooltip();
}

function passwordSent(data) {
    $("#forgot-password-message").show();
    if (data.Sent) {
        $("#forgot-password-message").html("Password has been sent!");
    }
    else {
        $("#forgot-password-message").html("Unable to send password.");
    }
}

function processInvite(data) {
    $("#main-message").html("<span class='success-message'>" + data.Message + "</span>");
    $("div.login-actions").hide();
    $('#invite-modal').modal('hide');
}

function inviteSent(data) {
    $(".sending-spinner").hide();
    $('#invite-sent').show();
    $("#btnSendInvite").hide();
}

function setSelectionRange(input, selectionStart, selectionEnd) {
    if (input.setSelectionRange) {
        input.focus();
        input.setSelectionRange(selectionStart, selectionEnd);
    }
    else if (input.createTextRange) {
        var range = input.createTextRange();
        range.collapse(true);
        range.moveEnd('character', selectionEnd);
        range.moveStart('character', selectionStart);
        range.select();
    }
}

function setCaretToPos(input, pos) {
    setSelectionRange(input, pos, pos);
}

function getUrlVars() {
    var vars = [], hash;
    var hashes = window.location.href.slice(window.location.href.indexOf('?') + 1).split('&');
    for (var i = 0; i < hashes.length; i++) {
        hash = hashes[i].split('=');
        vars.push(hash[0]);
        vars[hash[0]] = hash[1];
    }
    return vars;
}

//notices
function showNotice(header, msg) {
    var $notice = $('#alertmsg');
    $notice.find("h1").html(header);
    $notice.find("p").html(msg);

    if (!$notice.is('.in')) {
        $notice.addClass('in');

        setTimeout(function () {
            $('#alertmsg').removeClass('in');
        }, 3200);
    }
}

//typeaheads
function loadSearchTypeahead() {
    //top search
    var playerTemplate = '<a href="/player/{{team.slug}}/{{link}}"><img class="typeahead-avatar" src="{{profileImage}}" alt=""><span class="typeahead-bio-player">{{name}} | {{position}} | <span>{{team.slug}}</span></span></a>';
    var nflPlayers = Hogan.compile(playerTemplate);

    var userTemplate = '<a href="/coach/{{link}}"><img class="typeahead-avatar" src="{{image}}" alt=""><span class="typeahead-bio">{{name}} | @{{username}}</span></a>';
    var users = Hogan.compile(userTemplate);

    $('#main-search').typeahead('destroy');  //destroy first to refresh data
    $('#main-search').typeahead([
        {
            header: '<h4>Players</h4>',
            template: nflPlayers.render.bind(nflPlayers),
            highlight: false,
            prefetch: '/assets/data/players.json'
        },
        {
            header: '<h4>Users</h4>',
            highlight: false,
            template: users.render.bind(users),
            prefetch: '/assets/data/users.json'
        }
    ]);

    $('#main-search').bind('typeahead:closed', function (obj, datum, name) {
        $(obj.currentTarget).val("");
    });

    $('#main-search').bind('typeahead:selected', function (obj, datum, name) {
        if (name == 0) { //players
            window.location.href = "/player/" + datum.team.slug + "/" + datum.link;
        }
        else if (name == 1) { //users
            window.location.href = "/coach/" + datum.link;
        }
    });
}

function loadMatchupInviteFilter() {
    $('#searchlist').btsListFilter('#askFilterSearch', { itemChild: 'span' });
    var filter = "";

    $.getJSON("/assets/data/users.json", function (data) {
        $('#searchlist').btsListFilter('#askFilterSearch', {
            loadingClass: 'loading',
            sourceData: function (text, callback) {
       
                filter = text;
                var filterData = [];
                $.each(data, function (i, v) {
                    if (v.name.toUpperCase().includes(text.toUpperCase())) {
                        filterData.push(v);
                    }
                });
         
                callback(filterData);
                syncCheckmarks();
            },
            sourceNode: function (data) {
                return $('<a class="list-group-item user-invite" data-name="' + data.name + '" data-id="' + data.userID + '" href="#"><img class="typeahead-avatar" src="' + data.image + '" alt=""><span class="typeahead-bio">' + data.name + ' | @' + data.username + '</span><i style="display: none; padding-left: 18px" class="badge glyphicon glyphicon-ok">&nbsp;</i></a>')
                    .on('click', function (e) {
                        e.preventDefault();
                        $('#askFilterSearch').val(filter);    
                    });
            },
            emptyNode: function (data) {
                return $('<a class="list-group-item well" href="#"><span>Not found <b>"' + data + '"</b></a>');
            }
        });
    });
}

function loadUserTypeahead() {
    $.ajaxSetup({
        cache: false
    });

    var $textArea = $(".users-typeahead");
    if ($textArea.length > 0) {
        setCaretToPos($textArea.get(0), ($textArea.val().length));
        $textArea.limiter(500, $(".message-charNum"));
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

function loadPlayersTypeahead() {
    $.ajaxSetup({
        cache: false
    });

    //load the typeahead template
    // construct template string
    var template = '<img class="typeahead-avatar" src="{{profileImage}}" alt=""><span class="typeahead-bio-player">{{name}} | {{position}} | <span>{{team.slug}}</span></span>';
    var nflPlayers = Hogan.compile(template);

    //load the player typeahead
    $('#players').tagsinput({
        itemValue: 'id',
        itemText: function (item) {
            var item = '<img class="user-avatar-small" src="' + item.profileImage + '" /><span>' + item.shortName + '</span>';
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
}

function loadMatchupPlayersTypeahead() {
    var template = '<img class="typeahead-avatar" src="{{profileImage}}" alt=""><span class="typeahead-bio-player">{{name}} | {{position}} | <span>{{team.slug}}</span></span>';
    var nflPlayers = Hogan.compile(template);

    $('#player1, #player2, #player3, #player4').typeahead({
        prefetch: '/assets/data/players.json',
        template: nflPlayers.render.bind(nflPlayers),
        limit: 10,
        engine: Hogan
    });

    $('#player1, #player2, #player3, #player4').bind('typeahead:selected', function (obj, datum, name) {
        var $selected = $(obj.target).parents('li.list-group-item');
        $selected.find(".player-selected-name").text(datum.name);
        $selected.find(".player-selected-img").attr("src", datum.profileImage);
        $selected.find(".player-selected-bio").text(datum.position + " " + datum.team.name);
        $selected.find(".player-id").val(datum.id);
        $('#matchup-exists-alert').hide();

        $(obj.target).typeahead('destroy');
        $(obj.target).hide('fast', function () { $selected.find('div.selected-player').show(); });

        if ($("#player1-id").val().length > 0 && $("#player2-id").val().length > 0) {
            $("#share-matchup").removeClass("disabled");
        }
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