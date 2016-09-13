//object for the home page
var pollRate = 40000;
var streamTimer;

//popover for players
var originalLeave = $.fn.popover.Constructor.prototype.leave;
$.fn.popover.Constructor.prototype.leave = function (obj) {
    var self = obj instanceof this.constructor ?
        obj : $(obj.currentTarget)[this.type](this.getDelegateOptions()).data('bs.' + this.type)
    var container, timeout;

    originalLeave.call(this, obj);

    if (obj.currentTarget) {
        container = $(obj.currentTarget).siblings('.popover')
        timeout = self.timeout;
        container.one('mouseenter', function () {
            //We entered the actual popover – call off the dogs
            clearTimeout(timeout);
            //Let's monitor popover content instead
            container.one('mouseleave', function () {
                $.fn.popover.Constructor.prototype.leave.call(self, self);
            });
        })
    }
};

$(document).ready(function () {

    $.ajaxSetup({
        cache: false
    });

    //infinite scrolling
    $(window).scroll(function()
    {
        if($(window).scrollTop() == $(document).height() - $(window).height())
        {
            var $tabSelected = $("ul#main-stream-tabs li.active");
            if( $tabSelected.length > 0 ){
                if ($tabSelected.attr("id") == "all-tab") {

                    if ($("#get-more-messages").val() == "false") {
                        $("#get-more-messages").val("true");
                        var $lastTime = $('#message-stream div[data-ticks]').last();
                        $("#loading-messages-spinner").spin(smalSpinnerBlackOpts);
                        $("#all-stream-loader").show();
                        task.getStream($lastTime.attr("data-ticks"), false, updatePastStream);
                    }
                }
            }
        }
    });

    //tooltips
    $("[rel='tooltip']").tooltip();

    //polls the db for new messages
    /*streamTimer = setInterval(function () {
        var lastTime = $("#message-stream").attr("data-ticks");
        if ($("#get-more-messages").val() == "false") {
            task.getStreamUpdateCount(lastTime, updateStreamCount)
        }
    }, pollRate);*/

    $("#updateBar").click(function () {
        $(this).button('loading');
        var lastTime = $("#message-stream").attr("data-ticks");
        task.getStream(lastTime, true, updateStream);
        return false;
    });

    //top search
    var playerTemplate = '<a href="{{link}}"><img class="typeahead-avatar" src="{{profileImg}}" alt=""><span class="typeahead-bio">{{name}} | {{position}} | {{team}}</span></a>';
    var nflPlayers = Hogan.compile(playerTemplate);

    var userTemplate = '<a href="{{link}}"><img class="typeahead-avatar" src="{{image}}" alt=""><span class="typeahead-bio">{{name}} | @{{username}}</span></a>';
    var users = Hogan.compile(userTemplate);

    $('#main-search').typeahead('destroy');  //destroy first to refresh data
    $('#main-search').typeahead([
        {
            header: '<h4>Players</h4>',
            template: nflPlayers.render.bind(nflPlayers),
            prefetch: '/assets/data/players.json'
        },
        {
            header: '<h4>Users</h4>',
            template: users.render.bind(users),
            prefetch: '/assets/data/users.json'
        }
    ]);

    //stream tabs
    var tb = getUrlVars()["tb"];
    if (tb != undefined) {
        loadMatchupStream();
    }

    $('a.home-stream[data-toggle="tab"]').on('shown.bs.tab', function (e) {
        if (e.target.href.indexOf("#matchupstream") != -1) {
            loadMatchupStream();
        }
    })

    //sidepanel action - new message
    $("#new-message-panel").slidepanel({
        orientation: 'left',
        mode: 'overlay'
    });

    //sidepanel action - reply message
    $(".reply-message-panel").slidepanel({
        orientation: 'left',
        mode: 'overlay'
    });

    //sidepanel action - matchups
    $("#select-new-matchup").slidepanel({
        orientation: 'left',
        mode: 'overlay'
    });

    //slidepanel action - invites
    $(".invite-matchup-panel").slidepanel({
        orientation: 'left',
        mode: 'overlay'
    });

    $("#select-new-matchup").click(function () {
        var dt = new Date();
        var href = $(this).attr("href");
        if (href.indexOf("?daz=") >= 0) {
            $(this).attr("href", href.replace(/(daz=)[^\&]+/, '$1' + dt.getMilliseconds()));
        } else {
            $(this).attr("href", href + "?daz=" + dt.getMilliseconds());
        }
        return false;
    });

    $(".social-login").click(function () {
        $("#provider").val($(this).attr("data-provider"));

        $(this).parent().submit();
        return false;
    });

    //register modal
    $("#already-member-link").click(function () {

        $('#register-modal').modal('hide');
        $('#login-modal').modal('show');

        return false;
    });

    //replies need a unique href to make sure they don't cache
    $("body").on("click", '.reply-message-panel', function (e) {
        var dt = new Date();
        var href = $(this).attr("href");
        if (href.indexOf("&daz=") >= 0) {
            $(this).attr("href", href.replace(/(daz=)[^\&]+/, '$1' + dt.getMilliseconds()));
        } else {
            $(this).attr("href", href + "&daz=" + dt.getMilliseconds());
        }
        return false;
    });

    //submit the new message
    $("#slidepanel").on("click", '#share-post', function (e) {

        //$msgPanel.data("plugin_slidepanel").collapse();
        var message = $("#share-message").val();
        
        if ($(this).hasClass("disabled") || message.length <= 0) {
            return false;
        }

        var parentID = $("#prnt-id").val();
        var msgType = $("#msgType").val();
        var $parentMsg = (msgType == "general") ? $("#msg-" + parentID) : $("#match-" + parentID);
        var inline = ($parentMsg.attr("data-parent") === undefined) ? false : true;

        task.postUserMessage(message, $("#players").val(), parentID, msgType, inline, function (data) {
            if (msgType == "general" || msgType == "matchup") {
    
                if (parentID == 0) {
                    $(".stream-messages").prepend(data.StreamData);
                } else {
                    $("div.message-post-" + parentID).removeClass("empty-message-block");
                    $("div." + parentID + "-message-block").find("div.show-message-block").append(data.StreamData);
                    $("div." + parentID + "-message-block").find("div.hidden-message-block").append(data.StreamData);
                }

                $(".reply-message-panel").slidepanel({
                    orientation: 'left',
                    mode: 'overlay'
                });

                loadImages();

                //send out email notifications
                if (data.Type == "matchup") {
                    $.getScript("http://platform.twitter.com/widgets.js");
                    task.sendMatchupMessageEmail(data.MentionNotices, function (emailSent) {
                    });
                } else {
                    task.sendMentionEmail(data.MentionNotices, function (emailSent) {
                    });
                }
            }

            showNotice("Message Posted", "Thanks for sharing. Your message has been posted.");
        });
        
        return false;
    });

    //show matchup votes
    $("body").on("click", '.ms-action-details', function (e) {
        var matchupID = $(this).attr("data-mtch");
        var src = $(this).attr("data-src");
        var $this = $(this);
        var wait = $(this).next(".get-votes-wait");
        $(wait).spin(smalSpinnerBlackOpts);
        $(wait).show();
        task.getDetails(matchupID, function (data) {
            var $parent = $this.parents("div.stats");
            $parent.find("div.vote-details").html(data.DetailsData).slideDown();
            $this.find("i.glyphicon").removeClass("glyphicon-chevron-down").addClass("glyphicon-chevron-up");
            $this.addClass("ms-action-hide-details");
            $this.removeClass("ms-action-details");
            $(wait).hide();
        });

        return false;
    });

    //hide matchup votes
    $("body").on("click", '.ms-action-hide-details', function (e) {
        var matchupID = $(this).attr("data-mtch");
        var src = $(this).attr("data-src");
        var $this = $(this);
        
        var $parent = $this.parents("div.stats");
        $parent.find("div.vote-details").slideUp();
        $this.find("i.glyphicon").removeClass("glyphicon-chevron-up").addClass("glyphicon-chevron-down");
        $this.removeClass("ms-action-hide-details");
        $this.addClass("ms-action-details");

        return false;
    });

    //show the conversation
    $(".message-list-stream").on("click", '.ms-action-conversation', function (e) {
        var matchupID = $(this).attr("data-mtch");
        var src = $(this).attr("data-src");

        var $parent = $(this).parents("div.matchup-message-block");
        $parent.find("div.show-message-block").hide("fast", function() {
            $parent.find("div.hidden-message-block").slideDown("slow");
            loadImages();
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

    //user notifications
    $('#user-notifications').popover({
        trigger: 'click',
        placement: 'bottom',
        html: true
    });
  
    $('#user-notifications').on('shown.bs.popover', function () {
        $("#notice-list").before("<div id='notice-loading'><div class='notice-spinner'></div><span>Loading Notifications</span</div>");
        $("#notice-loading").show();
        $(".notice-spinner").spin(smalSpinnerBlackOpts);
        task.getNotices(function (data) {
            $("#notice-list").append(data.Notices);
            $(".notice-number").text("0");
            $("#notice-loading").hide();
        });
    })

    //filter trending views
    $(".trending-view").on('click', function (e) {
        var listItem = $(this).parent();
        $(listItem).parent().children().removeClass("active");
        $(listItem).addClass("active");

        var pos = $(this).attr("data-position");
        task.getTrending(5, pos, function (data) {
            $("#trending-filter").empty();
            $("#trending-filter").html(data.Results);

            loadImages();
        });
        return false;
    });

    //send out matchup ask invites
    $("#slidepanel").on("click", '#send-invites', function (e) {
        var inviteUsers = $("div.new-invite-sent.sent")
             .map(function () { return $(this).attr("data-user"); }).get();

        showNotice("Invites Sent", "Your Ask requests have been sent.");
        var matchupID = $(".invite-coach-matchup").attr("data-matchup");
        //send out invites if any exist
        if (inviteUsers.length > 0) {
            $.each(inviteUsers, function (i) {
                task.inviteAnswer(inviteUsers[i], matchupID, function () { });
            });
        }
    });

    //add new matchup
    $("#slidepanel").on("click", '#share-matchup', function (e) {
        var player1 = $("#player1-id").val();
        var player2 = $("#player2-id").val();

        if ($(this).hasClass("disabled") || player1.length <= 0 || player2.length <= 0) {
            return false;
        }

        var inviteUsers = $("div.new-invite-sent.sent")
              .map(function () { return $(this).attr("data-user"); }).get();

        var scoringType = $("input:radio[name='scoringOptions']:checked").val();
        task.addMatchupItem(player1, player2, scoringType, function (data) {
           
            if (data.Existing) {
                window.location = "/Matchup?mt=" + data.MatchupID;
            } else {
                //add to stream
                $(".stream-matchups").prepend(data.MatchupData);

                showNotice("Matchup Posted", "Thanks for sharing. Your matchup has been posted.");

                //send out invites if any exist
                if (inviteUsers.length > 0) {
                    $.each(inviteUsers, function (i) {
                        task.inviteAnswer(inviteUsers[i], data.MatchupID, function () { });
                    });
                }

                $.getScript("http://platform.twitter.com/widgets.js");
                loadImages();

                $(".invite-matchup-panel").slidepanel({
                    orientation: 'left',
                    mode: 'overlay'
                });

                $(".reply-message-panel").slidepanel({
                    orientation: 'left',
                    mode: 'overlay'
                });
            }
        });
        return false;
    });

    //follow a player or user
    $("body").on("click", '.follow', function (e) {
        var id = $(this).attr("data-account");
        var type = $(this).attr("data-account-type");
        var $btn = $(this);

        updateFollowCount(type, id, true);
        $btn.removeClass("btn-success follow").addClass("btn-danger unfollow").html("Unfollow");

        task.follow(id, type, function (data) {
            if (type == "users") {
                task.sendFollowNoticeEmail(id);
            }
        });

        return false;
    });

    //unfollow player or user
    $("body").on("click", '.unfollow', function (e) {
        var id = $(this).attr("data-account");
        var type = $(this).attr("data-account-type");
        var $btn = $(this);

        updateFollowCount(type, id, false);
        $btn.removeClass("btn-danger unfollow").addClass("btn-success follow").html("Follow");

        task.unfollow(id, type, function (data) {
        });
        return false;
    });

    //forgot password
    $("#forgot-password-link").click(function () {
        $("#forgot-password-message").hide();
        $("#forgot-password-error").hide();
        $("#forgot-password-controls").slideDown();
        return false;
    });

    $("#btnGetPassword").click(function () {
        if ($("#forgot-username").val() == "") {
            $("#forgot-password-error").show();
        }
        else {
            task.sendPassword($("#forgot-username").val(), passwordSent);
        }
        return false;
    });

    $("#btnCreateAccount").click(function () {
        var valid = validateForm("#frmRegister");
        if (valid) {
            task.validEmail($("#regemail").val(), function (data) {
                if (data.Valid) {
                    $("#frmRegister").submit();
                }
                else {
                    $(".email-validation-message").css('display', 'inline-block');
                }
            });

        }

        return false;
    });

    $("form#frmContactUs").submit(function () {
        var valid = validateForm("#frmContactUs");
        if (valid) {
            $(".sending-spinner").spin(smalSpinnerBlackOpts);
            task.sendContactUs($("#cntEmail").val(), $("#message").val(), processContactUs);
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

    //select a matchup starter - from matchup page
    $("a.select-starter-from-matchup").on("click", function () {
        $(this).button('loading');
          
        task.addMatchupChoice($(this).attr("id"), $(this).attr("data-matchup"), matchupChoiceAdded);
        return false;
    });

    //select a matchup starter in stream
    $("body").on("click", 'a.stream-select-starter', function (e) {
        $(this).button('loading');
        var current = $(this).parents("div.matchup-item");

        task.setStreamMatchupChoice($(this).attr("data-player-id"), $(current).attr("data-matchup-id"), function (data) {
            $('.matchup-item[data-matchup-id="' + data.ID + '"]').replaceWith(data.StreamData);
            task.sendMatchupVoteEmail(data.ID, data.UserVotedID);

            loadImages();
            $.getScript("http://platform.twitter.com/widgets.js");

            $(".reply-message-panel").slidepanel({
                orientation: 'left',
                mode: 'overlay'
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
        });

    $("#btn-add-matchup").click(function () {
        $("#matchup-pick").show();
        $("#matchup-selected").hide();

        $("#invite-answer").hide();
        $("#close-add-matchup").hide();
    });

    $("#cancel-user-matchup").click(function () {
        $("#add-matchup").slideUp(function () {
            $("#matchup-list").show();
        });

        return false;
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

    //matchup inviting
    loadMatchupInviteTypeahead();

    //invite any user
    $("#slidepanel").on("click", '.invite-coach-matchup', function (e) {
        if ($(this).hasClass("disabled")) {
            return false;
        }

        $('.ask-a-coach').typeahead('destroy');
        $(this).button('loading');
        var userID = $(this).parent().find("input.ask-a-coach").attr("data-user");

        task.getUserData(userID, matchupInviteAdded);
        return false;
    });

    //invite suggested users
    $("#slidepanel").on("click", '.invite-coach-new-matchup', function (e) {
        $(this).text('Sending');

        var parent = $(this).parent();
        $(parent).hide();
        $(parent).next().show().attr("data-user", $(this).attr("data-user")).addClass("sent");
        $(this).hide();

        return false;
    });

    //images
    loadImages();

    $("body").on("click", '.player-card-news', function (e) {
        $(".player-latest-news").css("overflow", "hidden");

        var $btn = $(this).button('loading');

        var playerID = $(this).attr("data-player-id");
        var matchupID = $(this).attr("data-matchup-id")

        var $popoverObj = $(this);
        var $newsObj = $("#" + matchupID + "-latest-news");

        $newsObj.find(".player-data-spinner").spin(smalSpinnerBlackOpts);

        //set the body text
        task.getPlayerTwitterStream(playerID, function (data) {
            $btn.button('reset');

            $newsObj.find(".player-pop-details").html(data.StreamData);
            $newsObj.slideDown();

        });

        //send analytics event
        ga('send', {
            hitType: 'event',
            eventCategory: 'News',
            eventAction: 'click',
            eventLabel: 'Latest News'
        });

        return false;
    });

});

function loadMatchupStream() {
    //add the matchup content
    var lastTime = $("#matchup-list").attr("data-ticks");

    if ($("#matchup-list").attr("data-loaded") == "false") {
        $("#loading-mathups-spinner").spin(smalSpinnerBlackOpts);
        $("#matchup-loader").show();

        task.getMatchupStream(lastTime, false, function (data) {
            $("#matchup-list").attr("data-loaded", "true");
            $("#matchup-list").append(data.StreamData);
            $("#matchup-loader").hide();

            $(".reply-message-panel").slidepanel({
                orientation: 'left',
                mode: 'overlay'
            });

            $(".invite-matchup-panel").slidepanel({
                orientation: 'left',
                mode: 'overlay'
            });

            loadImages();
            $.getScript("http://platform.twitter.com/widgets.js");
        });
    }
}

function loadPlayerPopovers() {
   
}

function loadMatchupInviteTypeahead() {
    var userTemplateNoLink = '<img class="typeahead-avatar" src="{{image}}" alt=""><span class="typeahead-bio">{{name}} | @{{username}}</span>';
    var usersNoLink = Hogan.compile(userTemplateNoLink);

    $('.ask-a-coach').typeahead({
        prefetch: '/assets/data/users.json',
        limit: 5,
        template: usersNoLink.render.bind(usersNoLink)
    });

    $('.ask-a-coach').bind('typeahead:selected', function (obj, datum, name) {
        $(this).attr("data-user", datum.userID);
        $(this).parent().parent().find(".invite-coach-matchup").removeClass("disabled");
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
    $(".reply-message-panel").slidepanel({
        orientation: 'left',
        mode: 'overlay'
    });
}

function updatePastStream(data) {
    //add older messages
    $("#all-stream-loader").before(data.StreamData);
    $("#all-stream-loader").hide();
    $("#get-more-messages").val("false");
    $("[rel='tooltip']").tooltip();
    $(".reply-message-panel").slidepanel({
        orientation: 'left',
        mode: 'overlay'
    });
    loadImages();
}

function updateFollowCount(type, id, add) {
    var $playerFollow = $("#following-count");
    if ($playerFollow.length > 0) {
        var followCount = (add == true) ? parseInt($playerFollow.text()) + 1 : parseInt($playerFollow.text()) - 1;
        $playerFollow.text(followCount);
    }
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

function processContactUs(data) {
    $("#sending").hide();
    $('#contact-sent').show();
}

function inviteSent(data) {
    $(".sending-spinner").hide();
    $('#invite-sent').show();
    $("#btnSendInvite").hide();
}

function showUserMatchups(data) {
    var weeks = 1;
    $.each(data.WeeklyMatchup, function (i, matchup) {
        if (matchup.Matchups.length > 0) {
            $("#weekly-page").append("<li class='paging-items' id='page-week-" + matchup.WeekNumber + "'><a class='page-number' data-week='" + matchup.WeekNumber + "' data-page-id='week-votes" + matchup.WeekNumber + "' href='#'>" + matchup.WeekNumber + "</a></li>")
            $("#my-matchup-item").append("<div class='my-matchup-week' id='week-votes" + matchup.WeekNumber + "'></div>");
            buildUserMatchup(matchup);
            
            weeks = matchup.WeekNumber;
        }
    });
    $(".my-matchup-week").hide();
    $("div#week-votes" + weeks).show();

    $("div#week-votes" + weeks).find("li.matchup-tab").first().addClass("active");
    $("div#week-votes" + weeks).find("div.votes-panel").first().addClass("active");

    $("li#page-week-" + weeks).addClass("active");
    $("#my-matchups-loader").hide();
    $("#weekly-title").show();
}

function showStarters(data) {
    var weeks = 1;
    $.each(data.WeeklyMatchup, function (i, matchup) {
        if (matchup.Matchups.length > 0) {
            $("#weekly-tabs").append("<li id='nav-week-" + matchup.WeekNumber + "'><a href='#week" + matchup.WeekNumber + "' data-toggle='tab'>" + matchup.WeekNumber + "</a></li>")
            $("#weekly-content").append("<div class='tab-pane' id='week" + matchup.WeekNumber + "'></div>");
            buildMatchup(matchup);
            weeks = matchup.WeekNumber;
        }
    });
    $("div#week" + weeks).addClass("active");
    $("li#nav-week-" + weeks).addClass("active");
    $("#starters-loader").hide();
}

function buildUserMatchup(matchup) {

    var template = "<div class='small-item tabbable tabs-left'>" +
                        "<ul class='nav nav-tabs matchup-data'>" +
                            "{{#Matchups}}<li class='matchup-tab'>" +
                            "<a href='#votes{{MatchupID}}' data-toggle='tab'>" +
                                "<div class='relative-style' style='padding-bottom: 4px'>" +
                                    "<div class='content-matchup'>" +
                                        "<div class='content-header'><span class='badge'>{{Player1.TotalVotes}}</span><span class='username'>{{Player1.PlayerName}}</span></div>" +
                                        "<img alt='' src='{{Player1.Image}}' class='matchup-avatar' />" +
                                        "<span class='game-text'>{{Player1.GameInfo}}</span>" +
                                    "</div>" +
                                "</div>" +
                                "<div class='relative-style'>" +
                                    "<div class='content-matchup'>" +
                                        "<div class='content-header'><span class='badge'>{{Player2.TotalVotes}}</span><span class='username'>{{Player2.PlayerName}}</span></div>" +
                                        "<img alt='' src='{{Player2.Image}}' class='matchup-avatar' />" +
                                        "<span class='game-text'>{{Player2.GameInfo}}</span>" +
                                    "</div>" +
                                "</div>" +
                            "</a>" +
                            "</li>{{/Matchups}}" +
                        "</ul>" +
                    "<div class='tab-content'>" +
                        "{{#Matchups}}<div class='tab-pane votes-panel' id='votes{{MatchupID}}'>" +
                            "<ul class='nav nav-tabs nav-stacked'>" +
                                "{{#Coaches}}<li class='relative-style {{#CorrectMatchup}}coach-correct{{/CorrectMatchup}}'>" +
                                    "<img class='matchup-user-vote-avatar' src='{{profileImg}}' />" +
                                    "<a class='matchup-user-vote' href='#'>{{fullName}} <span class='matchup-vote'><i class='icon-small icon-thumbs-up icon-white'></i>{{SelectedPlayer}}</span></a>" +
                                "</li>{{/Coaches}}" +
                                "{{#NoVotes}}<li><a class='matchup-user-vote' href='#'>No Starters Selected</a></li>{{/NoVotes}}" +
                            "</ul>" +
                        "</div>{{/Matchups}}" +
                    "</div>" +
                "</div>";

    var html = Mustache.to_html(template, matchup);
    $("#week-votes" + matchup.WeekNumber).append(html);
}


function buildMatchup(matchup) {
    var template = "{{#Matchups}}<div class='item' {{#MatchUpCorrect}}style='background-color: #468847;'{{/MatchUpCorrect}}><div class='content mystarter-item'>" +
                "{{#Player1.Selected}}<div class='mystarter-selected-icon'><i class='icon-large icon-thumbs-up icon-white'></i></div>{{/Player1.Selected}}" +
                "<div class='content-header'>" +
                    "<span style='color: #333' class='username'>{{Player1.PlayerName}}</span>" +
                "</div>" +
                "<img alt='{{Player1.PlayerName}}' src='{{Player1.Image}}' class='avatar mystarter-item-image'>" +
                "<p class='bio mystarter-item-bio'>{{Player1.PlayerDescription}}</p>" +
                "<span class='gamedetails mystarter-item-gamedetails'>{{Player1.GameInfo}}</span>" +
            "</div>" +
            "<div class='content mystarter-item'>" +
                "{{#Player2.Selected}}<div class='mystarter-selected-icon'><i class='icon-large icon-thumbs-up icon-white'></i></div>{{/Player2.Selected}}" +
                "<div class='content-header'>" +
                    "<span style='color: #333' class='username'>{{Player2.PlayerName}}</span>" +
                "</div>" +
                "<img alt='{{Player2.PlayerName}}' src='{{Player2.Image}}' class='avatar mystarter-item-image'>" +
                "<p class='bio mystarter-item-bio'>{{Player2.PlayerDescription}}</p>" +
                "<span class='gamedetails mystarter-item-gamedetails'>{{Player2.GameInfo}}</span>" +
            "</div></div>{{/Matchups}}";

    var html = Mustache.to_html(template, matchup);
    $("#week" + matchup.WeekNumber).append(html);
}

function inviteAdded(data) {
    $(".matchup-invite").button('reset');

    var inviteBtn = $("button#coach-" + data.User.userID);
    if ($(inviteBtn).length > 0) {
        $(inviteBtn).text("Invite Sent").removeClass("ask-coach-matchup").addClass("disabled").attr("href", "#");
    } else {
        //new item so add to the list
        var inviteSent = "<li class='relative-style'><img class='invite-answer-avatar' src='" + data.User.profileImg + "' />" +
            "<a class='invite-answer-coach' href='#'>" + data.User.fullName + " <span> " + data.User.correctPercentage + "</span>" +
            "<button id='coach-" + data.User.userID + "' data-id='" + data.User.userID + "' class='btn btn-mini disabled'>Invite Sent</button></a></li>";

        $("#ask-coaches-list").append(inviteSent)
    }
}

function matchupInviteAdded(data) {
    $(".invite-coach-matchup").button('reset');
    $(".ask-a-coach").val("");

    var matchupParent = $("div#invite-users div.matchup-invite");
    var count = $(matchupParent).length;

    var invited = "<div class='item clear child asked-to-amswer'><div class='content'>" +
        "<div class='content-header'><a class='username' href='#'>" + data.User.fullName + "</a><span class='user'>" + data.User.correctPercentage + "</span></div>" +
        "<img class='avatar profile-avatar' src='" + data.User.profileImg + "' />" +
        "<div data-user='" + data.User.userID + "' class='new-invite-sent sent'><span>Asked to Answer<span></div>" +
        "</div></div>";

    $(invited).insertAfter($('.asked-to-amswer').last());
    loadMatchupInviteTypeahead();
    $("#matchup-ask-" + data.Matchup).addClass('disabled');
}

function matchupChoiceAdded(voteData) {
    if (voteData.MatchupID != 0) {
        var $matchupItem = $('.mymatchup[data-matchup="' + voteData.MatchupID + '"]');
        $matchupItem.find("a.select-starter-from-matchup").remove();
        $matchupItem.find("div.matchup-player1 div.player-matchup-details").append('<div class="total-votes">' + voteData.Player1TotalVotes + '</div>');
        $matchupItem.find("div.matchup-player2 div.player-matchup-details").append('<div class="total-votes">' + voteData.Player2TotalVotes + '</div>');

        var vote = "<div class='item clear child'>" +
            "<div class='content'>" +
                "<div class='content-header'><a rel='tooltip' title='first tooltip' class='username' href='#'>" + voteData.fullName + "</a><span class='user'></span></div>" +
                "<img class='avatar profile-avatar' src='" + voteData.profileImg + "'>" +
            "<span><i class='icon-small icon-thumbs-up'></i>" + voteData.SelectedPlayer + "</span></div></div>";

        var noVotes = $("div#" + voteData.MatchupID).find("div.no-votes");
        if (noVotes.length > 0) {
            $(noVotes).remove();
        }
        $("div#" + voteData.MatchupID).prepend(vote);
        loadImages();
        task.sendMatchupVoteEmail(voteData.MatchupID, voteData.userVoteID);
        $(".reply-message-panel").slidepanel({
            orientation: 'left',
            mode: 'overlay'
        });
    }
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

//lazy load images
function loadImages() {
    $("img.lazy-load").unveil(300);
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