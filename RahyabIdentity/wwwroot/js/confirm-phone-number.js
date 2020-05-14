//function resend(userId) {
$("#btn-resend").click(function () {
    var userId = $("#UserId").val();
    console.log("here");
    $.ajax({
        url: "/api/Utilities/resend",
        type: 'POST',
        data: userId, 
        dataType: 'json',
        success: function (res) {
            console.log(res);
            alert(res);
        }
    });
});
//}