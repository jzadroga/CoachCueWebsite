//object for the home page
var pollRate = 20000;
var streamTimer;

$(document).ready(function () {

    //tooltips
    $("[rel='tooltip']").tooltip();

    //sidepanel action
    $('[data-slidepanel]').slidepanel({
        orientation: 'left',
        mode: 'overlay'
    });

    $("#slidepanel a.close").live('click', function (e) {
        $('#hidden-modal').modal('hide');
        $('[data-slidepanel]').slidepanel({
            orientation: 'left',
            mode: 'overlay'
        });
    });

    $("#sidepanel-msg-post").live('focus', function (e) {
        $("#conversation-placeholder").hide();
        $("#conversation-post").show();
        var textArea = $("#conversation-post").find(".reply-message-input-textarea");
        textArea.focus();
        textArea.limiter(140, $("#conversation-post").find(".message-charNum"));
    });

    $(".cancel-sidepanel-reply").live('click', function (e) {
        $("#conversation-post").hide();
        $("#conversation-post").find(".reply-message-input-textarea").val("");
        $("#conversation-placeholder").show();
    });

    $(".sidepanel-action-reply").live('click', function (e) {
        $("#conversation-placeholder").hide();
        $("#conversation-post").show();
        var $textArea = $("#conversation-post").find(".reply-message-input-textarea");

        $textArea.val("@" + $(this).attr("data-user") + " ");
        $textArea.focus();
        setCaretToPos($textArea.get(0), ($textArea.val().length));
        $textArea.limiter(140, $("#conversation-post").find(".message-charNum"));

        return false;
    });

    $("form.reply-message-post-form").live('submit', function () {
        var $textarea = $(this).find(".reply-message-input-textarea");
        if ($textarea.val().length > 0) {
            task.postReplyMessage($textarea.val(), $(this).attr("data-plyid"), $(this).attr("data-msg"), true, function (data) {
                $textarea.val("");
                $textarea.blur();

                $("#conversation-post").hide();
                $("#conversation-placeholder").show();

                $("#message-replies-sidepanel").prepend(data.StreamData);
            });
        }
        return false;
    });

    $(".ms-action-conversation").live('click', function (e) {
       
        /*var element = $('.scroll-pane').jScrollPane();

        var api = element.data('jsp');
        api.reinitialise();
        var throttleTimeout;
        $(window).bind(
            'resize',
            function () {
                // IE fires multiple resize events while you are dragging the browser window which
                // causes it to crash if you try to update the scrollpane on every one. So we need
                // to throttle it to fire a maximum of once every 50 milliseconds...
                if (!throttleTimeout) {
                    throttleTimeout = setTimeout(
                        function () {
                            alert("tet");
                            //api.reinitialise();
                            throttleTimeout = null;
                        },
                        50
                    );
                }
            }
        );
     */
        $('#hidden-modal').modal('show');
        return false;
    });

    $(".ms-action-reply").live('click', function (e) {
        $('#hidden-modal').modal('show');

        var dt = new Date();
        var href = $(this).attr("href");
        if (href.indexOf("&daz=") >= 0) {
            $(this).attr("href", href.replace(/(daz=)[^\&]+/, '$1' + dt.getMilliseconds()));
        } else {
            $(this).attr("href", href + "&daz=" + dt.getMilliseconds());
        }
        return false;
    });

    $(".social-login").click(function () {
        $("#provider").val($(this).attr("data-provider"));

        $(this).parent().submit();
        return false;
    });

    $("form.reply-post-form").live('submit', function () {
        var $textarea = $(this).find(".reply-message-input-textarea");
        var msgID =  $(this).attr("data-msg");
        if ($textarea.val().length > 0) {
            task.postReplyMessage($textarea.val(), $(this).attr("data-plyid"), msgID, false, function (data) {
                $textarea.val("");
                $textarea.blur();

                $("#reply-post").hide();
                $("#reply-posted").prepend(data.StreamData);
                $("#reply-posted").show();

                if ($("#msg-" + msgID).find(".message-conversation").length <= 0) {
                    var convLink = '<div class="message-conversation"><a id="view-conv-' + msgID + '" data-slidepanel="panel" data-msg="' + msgID + '" role="button" class="with-icn ms-action-conversation" href="/Player/Message?msgID=' + msgID + '"><i class="icon-comment"></i>View conversation</a></div>';
                    $("#msg-" + msgID).find(".message-tools").prepend(convLink);
                }
           });
        }
        return false;
    });

    //matchups
    $("#matchup-list").masonry({
        itemSelector: '.weekly-matchup-item',
        isAnimated: true,
        isFitWidth: true
    });

    //player listing modal
    $("#player-find-list").masonry({
        itemSelector: '.player-item',
        isAnimated: true,
        isFitWidth: true
    });

    $('#player-list-modal').on('shown', function () {
        $("#player-find-list").masonry('reload');
    })

    //notifications
    $("#user-notifications").click(function () {
        $(this).find("span.notice-number").text("0");
        task.getNotices(function (data) {
            $("#notice-list").prepend(data.Notices);
        });
    });

    //add player matchup inline
    $("form.new-matchup-post").submit(function () {
        var $form = $(this);
        task.addMatchup($(this).attr("data-plyid"), $(this).find(".new-matchup-input").attr("data-plyid"), 1, function (data) {
            if (data.Existing) {
                window.location = "Matchup?mt=" + data.MatchupID;
            } else {
                updateStream(data);
                var empty = $form.parent().find("div.empty.stream-matchup");
                if (empty.length > 0) {
                    $(empty).remove();
                }
            }
        });
        $(".new-matchup-input").val("");
        $(".new-matchup-input").blur();
        return false;
    });

    //add player message inline
    $("form.new-message-post").submit(function () {
        if ($(this).find(".new-message-input-textarea").val().length > 0) {
            task.postUserMessage($(this).find(".new-message-input-textarea").val(), $(this).attr("data-plyid"), true, 0, updateStream);
            $(".new-message-input-textarea").val("");
            $(".new-message-input-textarea").blur();
        }
        return false;
    });

    $(".new-message-input").focus(function () {
        $(this).hide();
        var parentForm = $(this).parent();
        parentForm.find("div.new-message-text").show();
        var textArea = parentForm.find(".new-message-input-textarea")
        textArea.focus();
        textArea.limiter(140, parentForm.find(".message-charNum"));

        $("#message-stream").masonry('reload');
    });

    $('.new-message-input-textarea').keydown(function (event) {
        if (event.keyCode == 13) {
            $(this.form).submit();
            return false;
        }
    }).blur(function () {
        $(this).parent().hide();
        $(this).parent().parent().find(".new-message-input").show();
        $("#message-stream").masonry('reload');
    });

    $("#message-stream .over-limit").hide(function () {
        $("#message-stream").masonry({
            itemSelector: '.player-container',
            isAnimated: true,
            isFitWidth: true
        });
    });

    $(".player-item").live({
        mouseenter: function () {
            $(this).find(".item-hover-menu").show();
        },
        mouseleave: function () {
            $(this).find(".item-hover-menu").hide();
        }
    });

    //polls the db for new messages
    /*streamTimer = setInterval(function () {
        var plyIDs = [];
        var times = [];
        $(".player-header").each(function(index) {
            plyIDs.push($(this).attr("data-player"));
            times.push($(this).attr("data-last"));
        });

        task.getStreamUpdateCount(plyIDs, times, updateStreamCount)
    }, pollRate);*/

    //filter trending views
    $(".trending-view").live('click', function (e) {
        var listItem = $(this).parent();
        $(listItem).parent().children().removeClass("active");
        $(listItem).addClass("active");

        var pos = $(this).attr("data-position");
        task.getTrending(10, pos, function (data) {
            $("#trending-filter").empty();
            $("#trending-filter").html(data.Results);
        });
        return false;
    });

    //filters the view of news, matchups and all
    $(".filter-view").live('click', function (e) {
        var listItem = $(this).parent();
        $(listItem).parent().children().removeClass("active");
        $(listItem).addClass("active");

        var playerID = $(this).attr("data-player");

        var selectedView = $(this).attr("data-filter");
        var $playerPod = $("#" + playerID);
        switch(selectedView)
        {
            case "stream-news":
                $playerPod.find(".stream-news").show();
                $playerPod.find(".stream-matchup").hide();
                $playerPod.parent().find(".new-message-post").show();
                $playerPod.parent().find(".new-matchup-post").hide();
                break;
            case "stream-matchup":
                $playerPod.find(".stream-news").hide();
                $playerPod.find(".stream-matchup").show();
                $playerPod.parent().find(".new-message-post").hide();
                $playerPod.parent().find(".new-matchup-post").show();
                break;
            case "none":
                $playerPod.find(".stream-news").show();
                $playerPod.find(".stream-matchup").show();
                $playerPod.find(".over-limit").hide();
                $playerPod.parent().find(".new-message-post").show();
                $playerPod.parent().find(".new-matchup-post").hide();
                break;
        }

        $("#message-stream").masonry('reload');
        return false;
    });

    //post actions from home page top
    $("#close-player-select, #home-cancel-post, #matchup-cancel-post").click(function () {
        showPlayerSelect();
    });

    $("#home-post-message").click(function () {
        var playerID = $("#player1").attr("data-plyid");
        task.postUserMessage($("#user-message").val(), playerID, false, 0, function () {
            $("#user-message").val("");
            showPlayerSelect();
            window.location = "Player/" + playerID + "/" + $("#player-selected-name").text();
        });
        return false;
    });

    $("#home-post-matchup").click(function () {
        //clearInterval(streamTimer);
        var player1 = $("#player1").attr("data-plyid");
        var player2 = $("#player2").attr("data-plyid");
        if (player1 == "" || player2 == "") {
            return false;
        }

        var scoringType = ($('#scoringStandard').is(':checked')) ? 1 : 2;
        task.addMatchup(player1, player2, scoringType, function (data) {
            $("#user-message").val("");
            showPlayerSelect();
            if (data.Existing) {
                window.location = "Matchup?mt=" + data.MatchupID;
            } else {
                window.location = "Player/" + player1 + "/" + $("#player-selected-name").text();
            }
        });
        return false;
    });

    $("#matchup-post-matchup").click(function () {
        var player1 = $("#player1").attr("data-plyid");
        var player2 = $("#player2").attr("data-plyid");
        if (player1 == "" || player2 == "") {
            return false;
        }

        var scoringType = ($('#scoringStandard').is(':checked')) ? 1 : 2;
        task.addMatchupItem(player1, player2, scoringType, function (data) {
            $("#user-message").val("");
            showPlayerSelect();
            if (data.Existing) {
                window.location = "/Matchup?mt=" + data.MatchupID;
            } else {
                //add to carousel
                $("#matchup-carousel-inner").find(".carousel-matchup-item").removeClass("active");
                $("#matchup-carousel-inner").prepend("<div class='carousel-matchup-item active item' style='width: 400px'>" + data.CarouselData + "</div>");

                $('#myCarousel').carousel({
                    interval: false
                })

                //add to list
                $("#matchup-list").prepend(data.MatchupData);
                $("#matchup-list").masonry('reload');
            }
        });
        return false;
    });

    $("#select-new-message").click(function () {
        $(this).addClass("selected");
        $("#select-new-matchup").removeClass("selected");
        $("#message-controls").show();
        $("#user-message").show();     
        $("#matchup-controls").hide();
        $("#player2").hide();
        $("#post-message").addClass("disabled");
        $("#user-message").val("");
        $("#scoring-format").hide();
    });

    $("#select-new-matchup").click(function () {
        $(this).addClass("selected");
        $("#select-new-message").removeClass("selected");
        $("#message-controls").hide();
        $("#user-message").hide();
        $("#player2").show();
        $("#matchup-controls").show();
        $("#post-matchup").addClass("disabled");
        $("#player2").val("");
        $("#scoring-format").show();
    });

    $("#user-message").focus(function () {
        $("#home-post-message, #player-post-message").removeClass("disabled");
    });

    //player details messaging
    $("#user-message").focus(function () {
        $(".new-message-options").show();
        $("#select-new-message").addClass("selected");
        $("#select-new-matchup").removeClass("selected");
        $(this).attr("rows", "4");
        $(this).limiter(140, $("#charNum"));
        if ($(this).val() == "") {
            $(this).attr("placeholder", "");
        }
    });

    $("#player-post-message").click(function () {
        task.postUserMessage($("#user-message").val(), $(".player-header").attr("data-player"), false, 0, updatePlayerStream);
        showPlayerDetailSelect();
        return false;
    });

    $("#player-post-matchup").click(function () {
        var player1 = $(".player-header").attr("data-player")
        var player2 = $("#player2").attr("data-plyid");
        if (player1 == "" || player2 == "") {
            return false;
        }

        var scoringType = ($('#scoringStandard').is(':checked')) ? 1 : 2;
        task.addMatchup(player1, player2, scoringType, function (data) {
            if (data.Existing) {
                window.location = "Matchup?mt=" + data.MatchupID;
            } else {
                updatePlayerStream(data);
                showPlayerDetailSelect();
            }
        });
        return false;
    });

    $("#player-cancel-post").click(function () {
        showPlayerDetailSelect();
    });

    $("#updateBar").click(function () {
        $(this).button('loading');
        task.getStream(updateStream);
        return false;
    });

    //follow a player or user
    $(".follow").live('click', function (e) {
        var id = $(this).attr("data-account");
        var type = $(this).attr("data-account-type");
        var playerDetail = $(this).hasClass("playerdetail");
        var $btn = $(this);

        updateFollowCount(type, id, true);

        task.follow(id, type, function (data) {
            if ($btn.attr("data-refresh") === undefined) {
                if (!playerDetail) {
                    updateStream(data);
                }
            } 
        });

        $(this).removeClass("btn-success follow").addClass("btn-inverse unfollow").html("Unfollow");
        return false;
    });

    //unfollow player or user
    $(".unfollow").live('click', function (e) {
        var id = $(this).attr("data-account");
        var type = $(this).attr("data-account-type");

        updateFollowCount(type, id, false);
        task.unfollow(id, type, removePlayerStream);
        $(this).removeClass("btn-inverse unfollow").addClass("btn-success follow").html("Follow");
        return false;
    });

    $("a.close-item").live("click", function () {
        $(this).parents(".parent").next().slideUp();
        $(this).removeClass("close-item").addClass("open-item").html("<i class='icon-chevron-down icon-white'></i>");
        return false;
    });

    $("a.open-item").live("click", function () {
        $(this).parents(".parent").next().slideDown();
        $(this).removeClass("open-item").addClass("close-item").html("<i class='icon-chevron-up icon-white'></i>");
        return false;
    });

    $("#more-less").live("click", function () {
        if ($(this).text() == "More") {
            $(this).parent().siblings(".more-items").slideDown();
            $(this).text("Less");
        } else {
            $(this).parent().siblings(".more-items").slideUp();
            $(this).text("More");
        }
        return false;
    });

    $("a.close-all-parents").click(function () {
        $(".child-list").slideUp();
        $("a.close-item").removeClass("close-item").addClass("open-item").html("<i class='icon-chevron-down icon-white'></i>");
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
        var valid = validateForm();
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

    //select a matchup starter
    $("a.select-starter-from-matchup").live("click", function () {
        $(this).button('loading');
          
        task.addMatchupChoice($(this).attr("id"), $(this).attr("data-matchup"), matchupChoiceAdded);
        return false;
    });

    $("a.select-starter-from-carousel").live("click", function () {
        $(this).button('loading');

        task.addMatchupChoice($(this).attr("id"), $(this).attr("data-matchup"), matchupCarouselChoiceAdded);
        return false;
    });

    $("a.stream-select-starter").live("click", function () {
        $(this).button('loading');
        var current = $(this).parents("div.matchup-item");

        task.setStreamMatchupChoice($(this).attr("data-player-id"), $(current).attr("data-matchup-id"), updateStream);
        return false;
    });

    $("a.select-starter").live("click", function () {
        $(this).button('loading');
        var current = $(this).parents("div.matchup-item");

        $.getJSON('/UserTask/SetMatchupChoice', { playerID: $(this).attr("id"), matchupID: $(current).attr("id") }, function (data) {
            $(current).fadeOut(function () { $(current).next().fadeIn('slow'); });
            if ($("#matchup-select-count").exists()) {
                $("#matchup-select-count").text(parseInt($("#matchup-select-count").text()) + 1);
            }
        });

        return false;
    });

    $('input').placeholder();

    $("a.page-number").live("click", function () {
        $(".my-matchup-week").hide();
        $("div#" + $(this).attr("data-page-id")).show();

        $("li.paging-items").removeClass("active");
        $(this).parent("li").addClass("active");

        var weeks = $(this).attr("data-week");
        $("li.matchup-tab").removeClass("active");
        $("div#week-votes" + weeks).find("li.matchup-tab").first().addClass("active");

        $("div.votes-panel").removeClass("active");
        $("div#week-votes" + weeks).find("div.votes-panel").first().addClass("active");

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

    $("a.show-my-matchups").click(function () {
        $("#my-matchup-item").empty();
        $("#weekly-page").empty();
        $("#weekly-title").hide();

        $("#my-matchups-loader").spin(largeSpinnerOpts);
        $("#my-matchups-loader").show();

        //decide to show one or all matchups
        if ($(this).attr("data-action-id")) {
            task.getUserMatchup($(this).attr("data-action-id"), showUserMatchups);
        } else {
            task.getUserMatchups($(this).attr("data-account-id"), showUserMatchups);
        }
        return false;
    });

    $("a.show-weekly-starters").click(function () {
        $("#weekly-content").empty();
        $("#weekly-tabs").empty();

        $("#starters-loader").spin(largeSpinnerOpts);
        $("#starters-loader").show();
        task.getStarters($(this).attr("data-account-id"), showStarters);
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

    $('.toggle-votes').on('shown', function () {
        $("#matchup-list").masonry('reload');
    })

    $('.toggle-votes').on('hidden', function () {
        $("#matchup-list").masonry('reload');
    })

    $(".vote-list").click(function () {
        var icon = $(this).find("i");
        if ($(icon).hasClass("icon-chevron-down")) {
            $(icon).removeClass("icon-chevron-down").addClass("icon-chevron-up");
        }
        else {
            $(icon).removeClass("icon-chevron-up").addClass("icon-chevron-down");
        }
    });

    $(".invite-coach-matchup").click(function () {
        $(this).button('loading');
        task.inviteAnswer($("#find-coachID").val(), $(this).attr("data-matchup"), matchupInviteAdded);
        return false;
    });

    $("#ask-coach-matchup-find").click(function () {
        $(this).button('loading');
        task.inviteAnswer($("#find-coachID").val(), $("#invite-coach-macthup-id").val(), inviteAdded);
        return false;
    });

    $(".ask-coach-matchup").click(function () {
        $(this).button('loading');
        task.inviteAnswer($(this).attr("data-id"), $("#invite-coach-macthup-id").val(), inviteAdded);
        return false;
    });

    $.fn.extend({
        limiter: function (limit, elem) {
            $(this).on("keyup focus", function () {
                setCount(this, elem);
            });
            function setCount(src, elem) {
                var chars = src.value.length;
                if (chars > limit) {
                    src.value = src.value.substr(0, limit);
                    chars = limit;
                }
                elem.html(limit - chars);
            }
            setCount($(this)[0], elem);
        }
    });

    //dropdowns for player searching
    var searchlabels, searchmapped;
    $('#player-search').typeahead({
        minLength: 1,
        items: 10,
        menu: '<ul class="typeahead player-search"></ul>',
        item: '<li><div class="player-search-item"></div></li>',
        source: function (query, process) {
            $.get('/UserTask/SearchAll', { query: query }, function (data) {

                searchlabels = [];
                searchmapped = {};

                $.each(data, function (i, item) {
                    var query_label = item;
                    searchlabels.push(query_label);
                });

                process(searchlabels);
            })
        },
        updater: function (item) {               
            return true;
        },
        highlighter: function (item) {
            return item;
        },
        render: function (items) {
            var that = this

            items = $(items).map(function (i, item) {
                i = $(that.options.item).attr('data-value', item)
                i.find('div').html(that.highlighter(item))
                return i[0]
            })

            items.first().addClass('active')
            this.$menu.html(items)
            return this
        },
        click: function (e) {
            return true;
        },
        hide: function () {
            this.$menu.hide()
            this.shown = false
            $("#player-listing").show();
            $("#coach-listing").show();
            return this
        },
        show: function () {
            $("#player-listing").hide();
            $("div.right-side").find("#coach-listing").hide();
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
    });

    //dropdowns for player listings
    var labels, mapped;
    $('#player1, #player2, .new-matchup-input').typeahead({
        minLength: 1,
        items: 8,
        source: function (query, process) {
            $.get('/UserTask/SearchPlayers', { query: query }, function (data) {

                labels = [];
                mapped = {};

                $.each(data, function (i, item) {
                    var query_label = item.fullName + " | " + item.position + " | " + item.team;

                    // mapping item object
                    var playerData = (item.number.length > 0) ? item.number + " " + item.position + " " + item.team : item.position + " " + item.team;
                    mapped[query_label] = item.fullName + "|" + item.accountID + "|" + item.profileImgLarge + "|" + playerData + "|" + item.profileImg;
                    labels.push(query_label);
                });

                process(labels);
            })
        },
        render: function (items) {
            var that = this
            items = $(items).map(function (i, item) {
                var playerData = mapped[item].split("|");
                i = $(that.options.item).attr('data-value', item)
                i.find('a').html('<img class="player-item-avatar typeahead-avatar" src="' + playerData[4] + '" alt=""><span class="typeahead-bio">' + that.highlighter(item) + '</span>');
                return i[0]
            })

            items.first().addClass('active')
            this.$menu.html(items)
            return this
        },
        updater: function (item) {
            var id = $(this)[0].$element.context.id;
            var playerData = mapped[item].split("|");
            $("#" + id).attr("data-plyid", playerData[1]);
            if (id == "player1") {
                $("#player-selected-name").text(playerData[0]);
                $("#player-selected-bio").text(playerData[3]);
                $("#player-selected-img").attr("src", playerData[2]);
                    
                $("#select-player-controls").hide('fast', function () {
                    $("#share-controls").fadeIn();
                    if ($("#user-message").length > 0) {
                        $("#user-message").focus();
                        $("#user-message").limiter(140, $("#charNum"));
                    }
                    else {
                        showMatchupOnly();
                    }
                });
            } else if (id == "player2") {
                $("#home-post-matchup, #player-post-matchup, #matchup-post-matchup").removeClass("disabled");
            }
            return playerData[0];
        }
    });

    //dropdowns for user listings
    var usrlabels, usrmapped;
    $('.find-coach').typeahead({
        minLength: 1,
        items: 8,
        source: function (query, process) {
            $.get('/UserTask/SearchUsers', { query: query }, function (data) {

                usrlabels = [];
                usrmapped = {};

                $.each(data, function (i, item) {
                    //var query_label = '<img style="height: 16px; width:16px; position: absolute; top: 5px; right: 0;" class="profile-avatar" alt="" src="' + item.profileImg + '" />';
                    var query_label = item.fullName + " | " + item.username;

                    // mapping item object
                    usrmapped[query_label] = item.fullName + "|" + item.userID + "|" + item.username;
                    usrlabels.push(query_label);
                });

                process(usrlabels);
            })
        },
        matcher: function(item){
            return true;
        },
        updater: function (item) {
            var id = $(this)[0].$element.context.id;
            var userData = usrmapped[item].split("|");
            $("#find-coachID").val(userData[1]);
            return "@" + userData[2];
        }
    });
});

function showMatchupOnly() {
    $(".new-message-options").show();
    $("#select-new-matchup").addClass("selected");
    $("#player2").show();
    $("#matchup-controls").show();
    $("#player2").val("");
    $("#scoring-format").show();
}

function showPlayerDetailSelect() {
    $("#player2").hide();
    $("#scoring-format").hide();
    $(".new-message-options").hide();
    $("#user-message").show().attr("rows", "1");
    $("#user-message").attr("placeholder", "Click to Add a Message or Matchup").val("");
    $("#user-message").blur();
}

function showPlayerSelect() {
    $("#share-controls").hide('fast', function () {
        $("#select-player-controls").fadeIn();
        $("#scoring-format").hide();
        $("#player1").val('');
        $("#player2").val('');
        $("#user-message").show();
        $("#player2").hide();
    });
}

function updateStreamCount(data) {
    if (data.UpdatesFound) {
        $("#updateBar").show();
    }
}

function removePlayerStream(data) {
    if (data.Result) {
        $('.player-container[data-player="' + data.ID + '"]').remove();
        $("#message-stream").masonry('reload');
    }
}

function updatePlayerStream(data) {
    var playerHeader = $('#'+ data.ID);
    //update the stream html and last time so it is current
    $(playerHeader).prepend(data.StreamData);
    $(playerHeader).attr("data-last", data.LastUpdateTicks);
}

function updateStream(data) {
    if (data.Type == "user") {
    }
    else if (data.Type == "list") {
        //reload
        $("#message-stream").empty().append(data.StreamData);
        $("#updateBar").hide();
        $("#updateBar").button("reset");
    } else {
        if (data.Type == "matchupSelected") {
            $('.matchup-item[data-matchup-id="' + data.ID + '"]').replaceWith(data.StreamData);
        } else {
            if (data.AddPlayerHeader) {
                $("#message-stream").prepend(data.StreamData);
            } else {
                if (!data.Inline) {
                    $('.player-container[data-player="' + data.ID + '"]').prependTo("#message-stream");
                }
                var playerHeader = $('.player-header[data-player="' + data.ID + '"]');
                //update the stream html and last time so it is current
                $(playerHeader).parent().find(".player-stream").prepend(data.StreamData);
                $(playerHeader).attr("data-last", data.LastUpdateTicks);
            }
        }
    }

    $("#message-stream .over-limit").hide();
    $("#message-stream").masonry('reload');
    $("[rel='tooltip']").tooltip();
}

function updateFollowCount(type, id, add) {
    if (type == "players") {
        var $playerFollow = $("#playerfollow-count-" + id);
        if ($playerFollow.length > 0) {
            var followCount = (add == true) ? parseInt($playerFollow.text()) + 1 : parseInt($playerFollow.text()) - 1;
            $playerFollow.text(followCount + " Followers");
        }
    }
    else {
        var $coachFollow = $("#coachfollow-count-" + id);
        if ($coachFollow.length > 0) {
            var coachFollowCount = (add == true) ? parseInt($coachFollow.text()) + 1 : parseInt($coachFollow.text()) - 1;
            $coachFollow.text(coachFollowCount + " Followers");
        }
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
    $(".matchup-invite").button('reset');
    $(".ask-a-coach").val("");

    var matchupParent = $("div#" + data.Matchup + " div.item");
    var count = $(matchupParent).length;

    var invited = "<div class='item clear child asked-to-amswer'><div class='content'>" +
        "<div class='content-header'><a class='username' href='#'>" + data.User.fullName + "</a><span class='user'>" + data.User.correctPercentage + "</span></div>" +
        "<img class='avatar profile-avatar' src='" + data.User.profileImg + "' />" +
        "<span>Asked to Answer</span>" +
        "</div></div>";

    $(matchupParent).eq(count - 1).before($(invited));
}

function matchupCarouselChoiceAdded(voteData) {
    matchupChoiceAdded(voteData);

    if (voteData.MatchupID != 0) {
        var $matchupItem = $('.carousel-matchup[data-matchup="' + voteData.MatchupID + '"]');
        $matchupItem.find("a.select-starter-from-carousel").remove();
        $matchupItem.find("div.matchup-player1").append('<div class="votes">' + voteData.Player1TotalVotes + '</div>');
        $matchupItem.find("div.matchup-player2").append('<div class="votes">' + voteData.Player2TotalVotes + '</div>');
    }

    setTimeout(
    function () {
        $("#myCarousel").carousel('next');
        $("#myCarousel").carousel('pause');
    }, 1000);
    
}

function matchupChoiceAdded(voteData) {
    if (voteData.MatchupID != 0) {
        var $matchupItem = $('.mymatchup[data-matchup="' + voteData.MatchupID + '"]');
        $matchupItem.find("a.select-starter-from-matchup").remove();
        $matchupItem.find("div.matchup-player1").append('<div class="votes">' + voteData.Player1TotalVotes + '</div>');
        $matchupItem.find("div.matchup-player2").append('<div class="votes">' + voteData.Player2TotalVotes + '</div>');

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
        $("#matchup-list").masonry('reload');
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