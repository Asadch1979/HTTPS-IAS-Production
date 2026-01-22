(function () {
    function checkPassword(inputText) {
        return typeof inputText === 'string' && inputText.length >= 8;
    }

    function reloadLocation() {
        window.location.href = g_asiBaseURL + "/login/logout";
    }

    function onSubmitChangePassword() {
        const newPassword = $('#inputNewPassword').val();
        const confirmPassword = $('#inputConfirmPassword').val();

        if (confirmPassword !== newPassword) {
            alert('New Passowrd and Confirm Password does not match');
            return;
        }

        if (!checkPassword(newPassword)) {
            alert('Password does not meet security requirements.');
            return false;
        }

        $.ajax({
            url: g_asiBaseURL + "/Home/DoChangePassword",
            type: "POST",
            data: {
                'Password': encryptPassword($('#inputPassword').val()),
                'NewPassword': encryptPassword(newPassword),
            },
            cache: false,
            success: function (data) {
                if (data && data.success) {
                    alert(data.message || "Your Password has been changed Successfully");
                    onAlertCallback(reloadLocation);
                } else {
                    alert((data && data.message) || "Unable to change password. Please try again.");
                }
            },
            error: function () {
                alert("Unable to change password. Please try again.");
            },
            dataType: "json",
        });
    }

    const changePasswordButton = document.getElementById('changePasswordSubmit');
    if (changePasswordButton) {
        changePasswordButton.addEventListener('click', onSubmitChangePassword);
    }

    window.CheckPassword = checkPassword;
    window.onSubmitChangePassword = onSubmitChangePassword;
})();
