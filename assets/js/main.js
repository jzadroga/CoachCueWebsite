//object for the home page
var pollRate = 40000;
var streamTimer;

$(document).ready(function () {

    $.ajaxSetup({
        cache: false
    });

    loadPlayersTypeahead();
    loadMatchupPlayersTypeahead();
    loadMatchupInviteTypeahead();

    //tooltips
    $("[rel='tooltip']").tooltip();

    //fitler matchups
    $('a.stream-filter-item').click(function () {
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

        $('#matchup-select').find('div.selected-player').hide();
        $('#matchup-select').find('input.matchup-player-select').val('').show();
        $('#player3-id').parents('li.list-group-item').hide();
        $('#player4-id').parents('li.list-group-item').hide();
        $('#add-matchup-player').parent().show();
        $('#invite-user-list').remove();

        loadMatchupPlayersTypeahead();
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

    //sidepanel action - login/register
    $(".login-register").slidepanel({
        orientation: 'left',
        mode: 'overlay'
    });

    //register modal
    $("body").on("click", '#already-member-link', function (e) {
        $('#frmRegisterLogin').hide();
        $('#frmLoginReg').show();

        return false;
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
                task.sendNotificationEmail(data.ID, function (emailSent) {
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
    $("#modal-matchup").on("click", '#send-invites', function (e) {
        var inviteUsers = $("#invite-user-list input:checkbox:checked")
             .map(function () { return $(this).attr("data-user"); }).get();

        showNotice("Invites Sent", "Your Ask requests have been sent.");
        var matchupID = $("#invite-user-list").attr("data-matchup");

        //send out invites if any exist
        if (inviteUsers.length > 0) {
            task.inviteAnswer(inviteUsers, matchupID, function () { });          
        }

        $("#modal-matchup").modal('hide');

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
        var player1 = $("#player1-id").val();
        var player2 = $("#player2-id").val();
        var player3 = $("#player3-id").val();
        var player4 = $("#player4-id").val();

        if ($(this).hasClass("disabled") || player1.length <= 0 || player2.length <= 0) {
            return false;
        }

        task.addMatchupItem(player1, player2, player3, player4, $('#matchup-type').val(), function (data) {
           
            if (data.Existing) {
                window.location = "/Matchup?mt=" + data.MatchupID;
            } else {
                //add to stream
                $(".stream-matchups").prepend(data.MatchupData);

                showNotice("Matchup Posted", "Thanks for sharing. Your matchup has been posted.");

                //show the invite page
                $('#invite-body').append(data.InviteData);
                $('.user-invite').bootstrapToggle();

                $('#matchup-select').hide();
                $('#matchup-invite').show();
                
                $.getScript("http://platform.twitter.com/widgets.js");
            }
        });

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
        var $current = $(this).parents("div.matchup-item");

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

    //images
    loadImages();

    //show latest news from twitter
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

function loadMatchupStream(position) { 
    task.getMatchupStream(position, function (data) {
        //$("#matchup-list").attr("data-loaded", "true");
        $("#message-stream").html(data.StreamData);
        $("#filter-matchup-spinner").empty();
    });
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
        //add the new toggle with unique id 
        var selected = "<li class='list-group-item'>";         
        selected += "<input data-user='" + datum.userID + "' checked id='" + datum.userID + "' data-style='invite-select' class='user-invite' type='checkbox' data-toggle='toggle' data-on='<i class=\"glyphicon glyphicon-ok\"></i> Invite will be sent<br/>to " + datum.name + "' data-off='<img class=\"typeahead-avatar\" src=\"" + datum.image + "\" /> Invite " + datum.name + "<br/>to answer'>";
        selected += "</li>";

        $("#invite-user-list").find("ul.list-group").prepend(selected);

        //initialize it 
        $('#' + datum.userID).bootstrapToggle();
        //clear the typeahead
        $(this).val("");
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

//typeaheads
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

function loadPlayersTypeahead() {
    $.ajaxSetup({
        cache: false
    });

    //load the typeahead template
    // construct template string
    var template = '<img class="typeahead-avatar" src="{{profileImage}}" alt=""><span class="typeahead-bio">{{name}} | {{position}} | {{team.name}}</span>';
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
        console.log(obj);
        this.tagsinput('add', datum);
        this.tagsinput('input').typeahead('setQuery', '');
    }, $('#players')));
}

function loadMatchupPlayersTypeahead() {
    var template = '<img class="typeahead-avatar" src="{{profileImage}}" alt=""><span class="typeahead-bio">{{name}} | {{position}} | {{team.name}}</span>';
    var nflPlayers = Hogan.compile(template);

    $('#player1, #player2, #player3, #player4').typeahead({
        prefetch: '/assets/data/players.json',
        template: nflPlayers.render.bind(nflPlayers),
        limit: 10,
        engine: Hogan
    });

    $('#player1, #player2, #player3, #player4').bind('typeahead:selected', function (obj, datum, name) {
        var $selected = $(obj.target).parent().prev();
        $selected.find(".player-selected-name").text(datum.name);
        $selected.find(".player-selected-img").attr("src", datum.profileImage);
        $selected.find(".player-selected-bio").text(datum.number + " " + datum.position + " " + datum.team.name);
        $selected.find(".player-id").val(datum.id);

        $(obj.target).typeahead('destroy');
        $(obj.target).hide('fast', function () { $selected.show(); });

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