//object for all methods on the search results page
var task = function () {

    getNotices = function (callback) {
        $.ajax({
            url: "/UserTask/GetUpdateNotificatons",
            cache: false,
            traditional: true,
            type: "POST",
            success: function (data) {
                callback(data);
            }
        })
    },

    validEmail = function (email, callback) {
         $.ajax({
             url: "/UserTask/ValidEmail",
             data: { email: email },
             cache: false,
             success: function (data) {
                 callback(data);
             }
         })
     },

    getStreamUpdateCount = function (plyIDs, times, callback) {
        $.ajax({
            url: "/UserTask/GetStreamUpdateCount",
            data: { ids: plyIDs, tms: times },
            cache: false,
            traditional: true,
            type: "POST",
            success: function (data) {
                callback(data);
            }
        })
    },

    getStream = function (callback) {
        $.ajax({
            url: "/UserTask/GetStream",
            cache: false,
            success: function (data) {
                callback(data);
            }
        })
    },

    postReplyMessage = function (message, plyID, prnt, showReply, callback) {
        $.ajax({
            url: "/UserTask/SaveMessageReply",
            data: { msg: message, plyID: plyID, parent: prnt, showReply: showReply },
            cache: false,
            success: function (data) {
                callback(data);
            }
        })
    },

    postUserMessage = function (message, plyID, inline, prnt, callback) {
        $.ajax({
            url: "/UserTask/SaveMessage",
            data: { msg: message, plyID: plyID, inline: inline, prnt: prnt },
            cache: false,
            success: function (data) {
                callback(data);
            }
        })
    },

    setStreamMatchupChoice = function (plyID, mtchID, callback) {
        $.ajax({
            url: "/UserTask/SetStreamMatchupChoice",
            data: { playerID: plyID, matchupID: mtchID },
            cache: false,
            success: function (data) {
                callback(data);
            }
        })
    },

    getTrending = function (num, pos, callback) {
        $.ajax({
            url: "/UserTask/GetTrending",
            data: { number: num, pos: pos },
            cache: false,
            success: function (data) {
                callback(data);
            }
        })
    },

    getConversation = function (msg, callback) {
        $.ajax({
            url: "/UserTask/GetConversation",
            data: { msgID: msg },
            cache: false,
            success: function (data) {
                callback(data);
            }
        })
    }

    followAccount = function (accountID, typeName, callback) {
        $.getJSON('/UserTask/Follow', { accountID: accountID, type: typeName }, function (data) {
            if ($("#follow-player-count").exists()) {
                $("#follow-player-count").text(parseInt($("#follow-player-count").text()) + 1);
            }
            callback(data);
        });
    },

    unfollowAccount = function (accountID, typeName, callback) {
        $.getJSON('/UserTask/Unfollow', { accountID: accountID, type: typeName }, function (data) {
            if ($("#follow-player-count").exists()) {
                $("#follow-player-count").text(parseInt($("#follow-player-count").text()) - 1);
            }
            callback(data);
        });
    },

    sendPassword = function (email, callback) {
        $.getJSON('/Account/SendPassword', { email: email }, function (data) {
            callback(data);
        });
    },

    sendInvite = function (email, msg, callback) {
        $.getJSON('/Account/Invite', { email: email, msg: msg }, function (data) {
            callback(data);
        });
    },

    sendInviteRequest = function (email, callback) {
        $.getJSON('/Account/InviteRequest', { email: email }, function (data) {
            callback(data);
        });
    },

    sendContactUs = function (email, message, callback) {
        $.getJSON('/Home/ContactUs', { email: email, message: message }, function (data) {
            callback(data);
        });
    },

    getStarters = function (accountID, callback) {
        $.ajax({
            url: "/UserTask/GetWeeklyStarters",
            data: { accountID: accountID },
            cache: false,
            success: function (data) {
                callback(data);
            }
        })
    },

    getUserMatchups = function (accountID, callback) {
        $.ajax({
            url: "/UserTask/GetUserMatchups",
            data: { accountID: accountID },
            cache: false,
            success: function (data) {
                callback(data);
            }
        })
    },

     getUserMatchup = function (matchupID, callback) {
         $.ajax({
             url: "/UserTask/GetUserMatchup",
             data: { matchupID: matchupID },
             cache: false,
             success: function (data) {
                 callback(data);
             }
         })
     },

    inviteAnswer = function (userID, matchupID, callback) {
        $.ajax({
            url: "/UserTask/InviteAnswer",
            data: { userID: userID, matchupID: matchupID },
            cache: false,
            success: function (data) {
                callback(data);
            }
        })
    },

    addMatchupChoice = function (playerID, matchupID, callback) {
        $.ajax({
            url: "/UserTask/SetMatchupChoice",
            data: { playerID: playerID, matchupID: matchupID },
            cache: false,
            success: function (data) {
                callback(data);
            }
        })
    },

    addMatchupItem = function (player1, player2, scoringType, callback) {
        $.ajax({
            url: "/UserTask/AddMatchupItem",
            data: { player1: player1, player2: player2, scoringTypeID: scoringType },
            cache: false,
            success: function (data) {
                callback(data);
            }
        });
    },

    addMatchup = function (player1, player2, scoringType, callback) {
        $.ajax({
            url: "/UserTask/AddMatchup",
            data: { player1: player1, player2: player2, scoringTypeID: scoringType },
            cache: false,
            success: function (data) {
                callback(data);
            }
        });
    };

    return {
        getNotices: getNotices,
        getStreamUpdateCount: getStreamUpdateCount,
        getStream: getStream,
        postUserMessage: postUserMessage,
        postReplyMessage: postReplyMessage,
        setStreamMatchupChoice: setStreamMatchupChoice,
        follow: followAccount,
        unfollow: unfollowAccount,
        sendPassword: sendPassword,
        sendInvite: sendInvite,
        sendInviteRequest: sendInviteRequest,
        sendContactUs: sendContactUs,
        getStarters: getStarters,
        addMatchup: addMatchup,
        addMatchupItem: addMatchupItem,
        getUserMatchups: getUserMatchups,
        getUserMatchup: getUserMatchup,
        inviteAnswer: inviteAnswer,
        addMatchupChoice: addMatchupChoice,
        getConversation: getConversation,
        getTrending: getTrending,
        validEmail: validEmail
    };
} ();