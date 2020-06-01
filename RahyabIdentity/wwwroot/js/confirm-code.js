// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

$(document).ready(function () {
    "use strict";
    //$("#container").addClass("d-flex");
    document.getElementById("container").style.display = "flex";
    document.getElementById("container").style.marginBottom ="2rem";
   // document.getElementById("login-link").style.color = "#fc00ff";

    var sec = 60;
    var myTimer = document.getElementById('timer-resend-confirm-code');
    var myBtn = document.getElementById('active-resend-confirm-code');
    var timerIsEnabled = false;
    var secPrint = "";
    countDown();
    function countDown() {
        timerIsEnabled = true;
        myBtn.innerHTML = " لطفا " + secPrint + " ثانیه صبر کنید  ...";
        myBtn.style.hover = "#666666";
        if (sec < 10 && sec > 0) {
            secPrint = "0" + sec;
        } else {
            secPrint = sec;
        }
        if (sec <= 0) {
            $("#myBtn").removeAttr("disabled");
            $("#myBtn").removeClass().addClass("btnEnable");
            $("#myTimer").fadeTo(2500, 0);
            secPrint = "";
            myBtn.innerHTML = "ارسال مجدد کد فعال سازی";
            //myBtn.style.color = "#666666";
            sec = 60;
            timerIsEnabled = false;
            return;
        }
        sec -= 1;
        window.setTimeout(countDown, 1000);
    }

    $("#active-resend-confirm-code").click(function (e) {
        console.log("clicked");
        console.log(timerIsEnabled);
        document.getElementById("active-resend-confirm-code").disable = true;
        debugger;
        if (!timerIsEnabled) {
            if (sec === 60 || sec <= 0) {
                $.ajax({
                    headers: {
                        'Accept': 'application/json',
                        'Content-Type': 'application/json'
                    },
                    type: "POST",
                    url: "/api/Utilities/Resend",
                    data: JSON.stringify({
                        userId: $("#UserId").val()
                    }),
                    success: function(data) {
                        console.log(data);
                        countDown();
                    },
                    error: function(e) {
                        console.log(e);
                    }
                });
            } else {
                countDown();
            }
        }
    });

});