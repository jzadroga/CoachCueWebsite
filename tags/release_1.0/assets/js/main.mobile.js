function removePlayerStream(data) {
    if (data.Result) {
        $('.player-container[data-player="' + data.ID + '"]').remove();
    }
}

function updateStream(data) {
    if (data.Type == "user") {
    }
    else if (data.Type == "list") {
        //reload
        $("#message-stream").empty().append(data.StreamData);
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

            $("#message-stream .over-limit").hide();
        }
    }
}

$(function () {
   
});

//make global settings
$(document).bind("mobileinit", function () {
    $.mobile.defaultPageTransition = 'slide';
});

$(document).on("pageinit", function () {
    $("#message-stream .over-limit").hide();

    //filter the stream view
    $(".filter-view").on('click', function (event, ui) {
        var listItem = $(this).parent();
        $(listItem).parent().children().removeClass("active");
        $(listItem).addClass("active");

        var playerID = $(this).attr("data-player");

        var selectedView = $(this).attr("data-filter");
        var $playerPod = $("#" + playerID);
        switch (selectedView) {
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

        return false;
    });

    //follow
    $(".follow").on('click', function (event, ui) {
        var playerDetail = $(this).hasClass("playerdetail");
        task.follow($(this).attr("data-account"), $(this).attr("data-account-type"), function (data) {
            if (!playerDetail) {
                updateStream(data);
            }
        });

        $(this).removeClass("btn-success follow").addClass("btn-inverse unfollow").html("Unfollow");
        return false;
    });

    //unfollow player or user
    $(".unfollow").on('click', function (event, ui) {
        task.unfollow($(this).attr("data-account"), $(this).attr("data-account-type"), removePlayerStream);
        $(this).removeClass("btn-inverse unfollow").addClass("btn-success follow").html("Follow");
        return false;
    });

    //add player matchup inline
    $("form.new-matchup-post").submit(function () {
        var $form = $(this);
        task.addMatchup($(this).attr("data-plyid"), $(this).find(".new-matchup-input").attr("data-plyid"), 1, function (data) {
            updateStream(data);
            var empty = $form.parent().find("div.empty.stream-matchup");
            if (empty.length > 0) {
                $(empty).remove();
            }
        });
        $(".new-matchup-input").val("");
        $(".new-matchup-input").blur();
        return false;
    });

    //add player message inline
    $("form.new-message-post").submit(function () {
        if ($(this).find(".new-message-input-textarea").val().length > 0) {
            task.postUserMessage($(this).find(".new-message-input-textarea").val(), $(this).attr("data-plyid"), true, updateStream);
            $(".new-message-input-textarea").val("");
            $(".new-message-input-textarea").blur();
        }
        return false;
    });

    //select a matchup from the stream
    $("a.stream-select-starter").on("click", function () {
        $(this).button('loading');
        var current = $(this).parents("div.matchup-item");

        task.setStreamMatchupChoice($(this).attr("data-player-id"), $(current).attr("data-matchup-id"), updateStream);
        return false;
    });

    $(".new-message-input").focus(function () {
        $(this).parent().hide();
        var parentForm = $(this).parent().parent();
        parentForm.find("div.new-message-text").show();
        var textArea = parentForm.find(".new-message-input-textarea")
        textArea.focus();
        textArea.limiter(140, parentForm.find(".message-charNum"));
    });

    $('.new-message-input-textarea').keydown(function (event) {
        if (event.keyCode == 13) {
            $(this.form).submit();
            return false;
        }
    }).blur(function () {
        var parentForm = $(this).parent().parent();
        parentForm.find("div.new-message-text").hide();
        parentForm.find(".new-message-input").parent().show();
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
                    $("#user-message").focus();
                    $("#user-message").limiter(140, $("#charNum"));
                });
            } else if (id == "player2") {
                $("#home-post-matchup, #player-post-matchup").removeClass("disabled");
            }
            return playerData[0];
        }
    });
});

$(document).bind("pagebeforechange", function (e, data) {
});