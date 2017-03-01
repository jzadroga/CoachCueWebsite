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

    validLogin = function (usr, pass, callback) {
        $.ajax({
            url: "/Account/LogOnAjax",
            data: { username: usr, password: pass, rememberMe: "true" },
            cache: false,
            traditional: true,
            type: "POST",
            success: function (data) {
                callback(data);
            }
        })
    },

    getStreamUpdateCount = function (lastTime, callback) {
        $.ajax({
            url: "/UserTask/GetStreamUpdateCount",
            data: { tms: lastTime },
            cache: false,
            traditional: true,
            type: "POST",
            success: function (data) {
                callback(data);
            }
        })
    },

    getStream = function (lastTime, ftr, callback) {
        $.ajax({
            url: "/UserTask/GetStream",
            data: { tms: lastTime, ftr: ftr },
            cache: false,
            success: function (data) {
                callback(data);
            }
        })
    },

    getPlayerTwitterStream = function (id, callback) {
        $.ajax({
            url: "/UserTask/GetTwitterStream",
            data: { playerID: id },
            cache: true,
            success: function (data) {
                callback(data);
            }
        })
    },

    getMatchupStream = function (position, callback) {
         $.ajax({
             url: "/UserTask/GetMatchupStream",
             data: { pos: position },
             cache: false,
             success: function (data) {
                 callback(data);
             }
         })
     },

    postUserMessage = function (message, plyID, prnt, type, inline, img, callback) {
        var postData = new FormData();

        postData.append('msg', message);
        postData.append('plyID', plyID);
        postData.append('prnt', prnt);
        postData.append('type', type);
        postData.append('inline', inline);
        postData.append('img', img);
        
        $.ajax({
            contentType: false,
            processData: false,
            type: "POST",
            url: "/UserTask/SaveMessage",
            data: postData,
            cache: false,
            success: function (data) {
                callback(data);
            }
        })
    },

    sendNotificationEmail = function (msgId, callback) {
        var postData = new FormData();
        postData.append('messageId', msgId);

        $.ajax({
            type: "POST",
            contentType: false,
            processData: false,
            url: "/UserTask/SendMessageNotifications",
            data: postData,
            cache: false,
            success: function (data) {
                callback(data);
            }
        })
    },

     sendMatchupMessageEmail = function (mentions, callback) {
         $.ajax({
             type: "POST",
             contentType: 'application/json',
             dataType: 'json',
             url: "/UserTask/SendMatchupMessageEmail",
             data: JSON.stringify(mentions),
             cache: false,
             success: function (data) {
                 callback(data);
             }
         })
     },

    sendMatchupVoteEmail = function (mtchid, voterid) {
        $.ajax({
            type: "POST",
            url: "/UserTask/SendMatchupVoteEmail",
            data: { mtchid: mtchid, voterid: voterid },
            cache: false
        })
    },

    sendFollowNoticeEmail = function (flw) {
        $.ajax({
            type: "POST",
            url: "/UserTask/SendFollowEmail",
            data: { follow: flw },
            cache: false
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

    getConversation = function (id, src, type, callback) {
        $.ajax({
            url: "/UserTask/GetConversation",
            data: { objID: id, srcID: src, type: type },
            cache: false,
            success: function (data) {
                callback(data);
            }
        })
    },

    getDetails = function (id, callback) {
        $.ajax({
            url: "/UserTask/GetDetails",
            data: { objID: id },
            cache: false,
            success: function (data) {
                callback(data);
            }
        })
    },

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

    getUserData = function (userID, callback) {
        $.ajax({
            url: "/UserTask/GetUserData",
            data: { userID: userID },
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

    addMatchupItem = function (player1, player2, scoringType, callback) {
        $.ajax({
            url: "/UserTask/AddMatchupItem",
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
        getMatchupStream : getMatchupStream,
        postUserMessage: postUserMessage,
        follow: followAccount,
        unfollow: unfollowAccount,
        sendPassword: sendPassword,
        sendInvite: sendInvite,
        sendInviteRequest: sendInviteRequest,
        getUserData: getUserData,
        sendContactUs: sendContactUs,
        getStarters: getStarters,
        addMatchupItem: addMatchupItem,
        getUserMatchups: getUserMatchups,
        getPlayerTwitterStream: getPlayerTwitterStream,
        getUserMatchup: getUserMatchup,
        inviteAnswer: inviteAnswer,
        addMatchupChoice: addMatchupChoice,
        setStreamMatchupChoice : setStreamMatchupChoice,
        getConversation: getConversation,
        getDetails: getDetails,
        getTrending: getTrending,
        sendNotificationEmail: sendNotificationEmail,
        sendMatchupMessageEmail: sendMatchupMessageEmail,
        sendMatchupVoteEmail: sendMatchupVoteEmail,
        sendFollowNoticeEmail: sendFollowNoticeEmail,
        validEmail: validEmail,
        validLogin: validLogin
    };
} ();