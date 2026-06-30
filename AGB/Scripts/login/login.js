$(document).ready(function () {
    $("#loginForm").submit(function (e) {
        e.preventDefault();

        $.ajax({
            url: "/Login/Login",
            type: "POST",
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            data: JSON.stringify({
                Username: $("#Username").val(),
                Password: $("#Password").val()
            }),
            success: function (response) {
                if (response.success) {
                    window.location.href = response.redirectUrl; // Redirect here
                } else {
                    $("#message").text(response.message).css("color", "red");
                }
            },
            error: function () {
                $("#message").text("An error occurred. Please try again.").css("color", "red");
            }
        });
    });
});
