$(document).ready(function () {

    $("#team").change(function () {
        $("#frmRoster").submit();
    });

    $("#week").change(function () {
        window.location = "/controlpanel/Matchups?week=" + $("#week option:selected").text();
    });

    $(".edit-points").click(function () {
        var matchupID = $(this).attr("data-matchup");
        $.ajax({
            url: "/ControlPanel/SetMatchupPoints",
            data: { pts1: $("#pts1-" + matchupID).val(), pts2: $("#pts2-" + matchupID).val(), id: matchupID },
            cache: false,
            success: function () {
                //maybe highlight the row
            }
        });
        return false;
    });

});