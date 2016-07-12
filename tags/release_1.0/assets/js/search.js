//object for all methods on the search results page
var search = function () {

    this.init = function () {

        $(".follow").live('click', function (e) {
            task.follow($(this).attr("data-account"), "players");
            $(this).removeClass("btn-success follow").addClass("btn-danger unfollow").html("<i class='icon-minus-sign icon-white'></i>Unfollow");
            return false;
        });

        $(".unfollow").live('click', function (e) {
            task.unfollow( $(this).attr("data-account"));
            $(this).removeClass("btn-danger unfollow").addClass("btn-success follow").html("<i class='icon-plus-sign icon-white'></i>Follow");
            return false;
        });
    };

    return {
        init: init
    };
} ();