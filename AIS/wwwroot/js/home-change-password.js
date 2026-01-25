(function () {
    function checkPassword(inputText) {
        return typeof inputText === 'string' && inputText.length >= 8;
    }

    function reloadLocation() {
        window.location.href = g_asiBaseURL + "/Home/Index";
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
                'ConfirmPassword': encryptPassword(confirmPassword),
            },
            cache: false,
            success: function (data) {
                if (data && data.Status) {
                    var target = (data && data.RedirectUrl) ? data.RedirectUrl : (g_asiBaseURL + "/Login/Index");
                    alert('Password Changed');
                    onAlertCallback(function () {
                        window.location.href = target;
                    });
                } else {
                    showApiAlert(data, "Unable to change password. Please try again.");
                }
            },
            error: function (xhr) {
                if (xhr && xhr.status === 401) {
                    showApiAlertFromXhr(xhr, xhr.status, getErrorReferenceIdFromXhr(xhr), 'Password change session expired, login again.');
                    window.location.href = g_asiBaseURL + "/Login/Index";
                    return;
                }

                showApiAlertFromXhr(xhr, xhr ? xhr.status : null, getErrorReferenceIdFromXhr(xhr), "Unable to change password. Please try again.");
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
