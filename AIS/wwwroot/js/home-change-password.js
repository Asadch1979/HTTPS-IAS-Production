(function () {
    function checkPassword(inputText) {
        return typeof inputText === 'string' && inputText.length >= 8;
    }

    function reloadLocation() {
        window.location.href = g_asiBaseURL + "/Home/Index";
    }

    function getLoginUrl() {
        return g_asiBaseURL + "/Login/Index";
    }

    function setProcessingState(isProcessing, shouldEnableOnStop) {
        var submitButton = document.getElementById('changePasswordSubmit');
        var spinner = document.getElementById('changePasswordSpinner');
        var submitText = document.getElementById('changePasswordSubmitText');

        if (spinner) {
            spinner.classList.toggle('d-none', !isProcessing);
        }

        if (submitText) {
            submitText.textContent = isProcessing ? 'Processing...' : 'Submit';
        }

        if (submitButton) {
            if (isProcessing) {
                submitButton.disabled = true;
            } else {
                submitButton.disabled = !shouldEnableOnStop;
            }
        }
    }

    function isAuthFailureResponse(xhr, textStatus) {
        if (!xhr) {
            return false;
        }

        if (xhr.status === 401 || xhr.status === 403) {
            return true;
        }

        if (textStatus === 'parsererror') {
            return true;
        }

        var responseText = xhr.responseText || '';
        return responseText.indexOf('Login') !== -1 || responseText.indexOf('login') !== -1;
    }

    function onSubmitChangePassword() {
        var submitButton = document.getElementById('changePasswordSubmit');
        if (submitButton && submitButton.disabled) {
            return;
        }

        setProcessingState(true);

        const newPassword = $('#inputNewPassword').val();
        const confirmPassword = $('#inputConfirmPassword').val();

        if (confirmPassword !== newPassword) {
            alert('New Passowrd and Confirm Password does not match');
            setProcessingState(false, true);
            return;
        }

        if (!checkPassword(newPassword)) {
            alert('Password does not meet security requirements.');
            setProcessingState(false, true);
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
                var isSuccess = data && (data.Success === true || data.Status === true);
                if (isSuccess) {
                    var target = (data && data.RedirectUrl) ? data.RedirectUrl : getLoginUrl();
                    setProcessingState(false, false);
                    onAlertCallback(function () {
                        window.location.href = target;
                    });
                    showApiAlert(data, "Password Changed");
                } else {
                    setProcessingState(false, true);
                    showApiAlert(data, "Unable to change password. Please try again.");
                }
            },
            error: function (xhr, textStatus) {
                if (isAuthFailureResponse(xhr, textStatus)) {
                    setProcessingState(false, true);
                    onAlertCallback(function () {
                        window.location.href = getLoginUrl();
                    });
                    showApiAlertFromXhr(xhr, xhr && xhr.status ? xhr.status : null, getErrorReferenceIdFromXhr(xhr), 'Session expired, please login again.');
                    return;
                }

                setProcessingState(false, true);
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
