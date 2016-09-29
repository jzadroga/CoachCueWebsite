//spinners
var spinnerOpts = {
    lines: 13, // The number of lines to draw
    length: 7, // The length of each line
    width: 4, // The line thickness
    radius: 10, // The radius of the inner circle
    rotate: 41, // The rotation offset
    color: '#000', // #rgb or #rrggbb
    speed: 1, // Rounds per second
    trail: 60, // Afterglow percentage
    shadow: false, // Whether to render a shadow
    hwaccel: false, // Whether to use hardware acceleration
    className: 'spinner', // The CSS class to assign to the spinner
    zIndex: 2e9, // The z-index (defaults to 2000000000)
    top: 'auto', // Top position relative to parent in px
    left: 'auto' // Left position relative to parent in px
};

var largeSpinnerOpts = {
    lines: 13, // The number of lines to draw
    length: 7, // The length of each line
    width: 4, // The line thickness
    radius: 14, // The radius of the inner circle
    rotate: 41, // The rotation offset
    color: '#000', // #rgb or #rrggbb
    speed: 1, // Rounds per second
    trail: 60, // Afterglow percentage
    shadow: false, // Whether to render a shadow
    hwaccel: false, // Whether to use hardware acceleration
    className: 'spinner-starters', // The CSS class to assign to the spinner
    zIndex: 2e9, // The z-index (defaults to 2000000000)
    top: 'auto', // Top position relative to parent in px
    left: 'auto' // Left position relative to parent in px
};

var smalSpinnerBlackOpts = {
    lines: 10, // The number of lines to draw
    length: 7, // The length of each line
    width: 2, // The line thickness
    radius: 2, // The radius of the inner circle
    rotate: 41, // The rotation offset
    color: '#000', // #rgb or #rrggbb
    speed: 1, // Rounds per second
    trail: 60, // Afterglow percentage
    shadow: false, // Whether to render a shadow
    hwaccel: false, // Whether to use hardware acceleration
    className: 'small-spinner', // The CSS class to assign to the spinner
    zIndex: 2e9, // The z-index (defaults to 2000000000)
    top: 'auto', // Top position relative to parent in px
    left: 'auto' // Left position relative to parent in px
};

var smalSpinnerOpts = {
    lines: 10, // The number of lines to draw
    length: 7, // The length of each line
    width: 2, // The line thickness
    radius: 2, // The radius of the inner circle
    rotate: 41, // The rotation offset
    color: '#fff', // #rgb or #rrggbb
    speed: 1, // Rounds per second
    trail: 60, // Afterglow percentage
    shadow: false, // Whether to render a shadow
    hwaccel: false, // Whether to use hardware acceleration
    className: 'small-spinner', // The CSS class to assign to the spinner
    zIndex: 2e9, // The z-index (defaults to 2000000000)
    top: 'auto', // Top position relative to parent in px
    left: 'auto' // Left position relative to parent in px
};

jQuery.fn.exists = function () { return this.length > 0; }

$.fn.spin = function (opts) {
    this.each(function () {
        var $this = $(this),
          data = $this.data();

        if (data.spinner) {
            data.spinner.stop();
            delete data.spinner;
        }
        if (opts !== false) {
            data.spinner = new Spinner($.extend({ color: $this.css('color') }, opts)).spin(this);
        }
    });
    return this;
};

//watermarks
function setWaterMarks() {
    $('input:[data-watermark]').each(function () {
        if ($(this).val() == "") {
            $(this).val($(this).attr("data-watermark"));
        }
    });

    $("input:[data-watermark]").focus(function () {
        if ($(this).val() == $(this).attr("data-watermark")) {
            $(this).val("");
            $(this).addClass("watermarks-active");
        }
    });

    $("input:[data-watermark]").blur(function () {
        if ($(this).val() == "") {
            $(this).val($(this).attr("data-watermark"));
            $(this).removeClass("watermarks-active");
        }
    });
}

//validation
function setValidation() {
    $("form.validate").submit(function () {
        return validateForm(this);
    });
}

function validateForm(form) {
    $(form).find($('span.help-inline.validation-message')).each(function () {
        $(this).hide();
    });

    var valid = true;
    $(form).find($('input[data-required="true"]')).each(function () {
        if ($(this).val() == $(this).attr("data-watermark") || $(this).val() == "") {
            $(this).parents("div.form-group").addClass("has-error");
            $(this).siblings(".validation-message").css('display', 'inline-block');
            valid = false;
        }
    });

    if ($(this).hasClass("data-validate-email")) {
        if (!validEmail($(this).val().toLowerCase())) {
            $(this).parents("div.form-group").addClass("has-error");
            $(this).next(".validation-message").css('display', 'inline-block');
            valid = false;
        }
    }

    return valid;
}

function validEmail(email) {
    var re = "^[a-zA-Z0-9_\\+-]+(\\.[a-zA-Z0-9_\\+-]+)*@[a-zA-Z0-9-]+(\\.[a-zA-Z0-9-]+)*\\.([a-zA-Z]{2,4})$";
    return email.match(re)
}

//general functions
function urldecode(str) {
    return decodeURIComponent((str + '').replace(/\+/g, '%20'));
}

//twitter parsing
function twitterList(twitters) {
    var statusHTML = [];
    statusHTML.push('<div class="child-list">');
    if (twitters.length == 0) {
        statusHTML.push('<div class="item clear child-bottom empty-news"><div class="content"><div class="content-header"></div><span>No Recent News</span></div></div>');
    } 
    else {
        for (var i = 0; i < twitters.length; i++) {
            var user = twitters[i].from_user;
            var username = twitters[i].from_user_name;
            var status = twitters[i].text.replace(/((https?|s?ftp|ssh)\:\/\/[^"\s\<\>]*[^.,;'">\:\s\<\>\)\]\!])/g, function (url) {
                return '<a href="' + url + '">' + url + '</a>';
            }).replace(/\B@([_a-z0-9]+)/ig, function (reply) {
                return reply.charAt(0) + '<a href="http://twitter.com/' + reply.substring(1) + '">' + reply.substring(1) + '</a>';
            });
            if (i == 5) {
                statusHTML.push('<div class="more-items">');
            } //only show the top five, rest get hidden

            var childClass = (i < 5 && i == twitters.length - 1) ? "child-bottom" : "child";
            statusHTML.push('<div class="item clear ' + childClass + '"><div class="content"><div class="content-header"><a rel="tooltip" title="first tooltip" class="username" target="_blank" href="http://twitter.com/' + user + '">' + username + '</a><span class="user">@' + user + '</span></div><img class="avatar" src="' + twitters[i].profile_image_url + '" /><span>' + status + '</span> <a class="time" target="_blank" href="http://twitter.com/' + user + '/statuses/' + twitters[i].id_str + '">' + relative_time(twitters[i].created_at, false) + '</a></div></div>');
            if (i > 4 && i == twitters.length - 1) {
                statusHTML.push('</div>');
            }
        }

        if (twitters.length > 5) {
            statusHTML.push("<div class='more-less-tweets-bar'><button id='more-less' type='button' class='btn big btn-inverse'>More</button></div>");
        }
    }

    statusHTML.push('</div>');
    statusHTML.push("<div class='sep-space'></div>");

    document.getElementById('twitter_update_list').innerHTML += statusHTML.join('');
}

function twitterPlayer(twitters, element) {
    var statusHTML = [];
    statusHTML.push('<div class="">');
    for (var i = 0; i < twitters.length; i++) {
        var user = twitters[i].user.name;
        var username = twitters[i].user.screen_name;
        var status = twitters[i].text.replace(/((https?|s?ftp|ssh)\:\/\/[^"\s\<\>]*[^.,;'">\:\s\<\>\)\]\!])/g, function (url) {
            return '<a href="' + url + '">' + url + '</a>';
        }).replace(/\B@([_a-z0-9]+)/ig, function (reply) {
            return reply.charAt(0) + '<a href="http://twitter.com/' + reply.substring(1) + '">' + reply.substring(1) + '</a>';
        });
        statusHTML.push('<div class="sideitem clear"><div class="content"><div class="content-header"><a class="username" target="_blank" href="http://twitter.com/' + user + '">' + username + '</a><span class="user">@' + user + '</span><a class="time" target="_blank" href="http://twitter.com/' + user + '/statuses/' + twitters[i].id_str + '">' + relative_time(twitters[i].created_at, true) + '</a></div><img class="avatar" src="' + twitters[i].user.profile_image_url + '" /><span>' + status + '</span> </div></div>');
    }
    statusHTML.push('</div>');
    return statusHTML.join('');
}

function relative_time(time_value, userTweet) {
    var values = time_value.split(" ");
    time_value = (!userTweet) ? values[2] + " " + values[1] + ", " + values[3] + " " + values[4] : values[1] + " " + values[2] + ", " + values[5] + " " + values[3];
    var parsed_date = Date.parse(time_value);
    var relative_to = new Date(); //(arguments.length > 1) ? arguments[1] : new Date();
    var delta = parseInt((relative_to.getTime() - parsed_date) / 1000);
    delta = delta + (relative_to.getTimezoneOffset() * 60);

    if (delta < 60) {
        return (userTweet) ? delta + " secs" : 'less than a minute ago';
    } else if (delta < 120) {
        return (userTweet) ? '1 min' : 'about a minute ago';
    } else if (delta < (60 * 60)) {
        return (userTweet) ? (parseInt(delta / 60)).toString() + ' mins' : (parseInt(delta / 60)).toString() + ' minutes ago';
    } else if (delta < (120 * 60)) {
        return (userTweet) ? '1 hr' : 'about an hour ago';
    } else if (delta < (24 * 60 * 60)) {
        return (userTweet) ? (parseInt(delta / 3600)).toString() + ' hrs' : 'about ' + (parseInt(delta / 3600)).toString() + ' hours ago';
    } else if (delta < (48 * 60 * 60)) {
        return (userTweet) ? '1 day' : '1 day ago';
    } else {
        return (userTweet) ? (parseInt(delta / 86400)).toString() + ' days' : (parseInt(delta / 86400)).toString() + ' days ago';
    }
}