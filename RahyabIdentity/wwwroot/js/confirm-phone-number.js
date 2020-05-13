function resend(userId) {
    console.log("script");
    //  $("#btn-resend").click(function() {
    console.log("here");
    $.ajax({
        url: "/api/Utilities/",
        type: 'POST',
        data: userId, //@Model.UserId,
        dataType: 'json',
        success: function (res) {
            console.log(res);
            alert(res);
        }
    });
    //  });
}