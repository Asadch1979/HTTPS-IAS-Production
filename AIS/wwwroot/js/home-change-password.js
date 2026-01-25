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

    function setInlineMessage(message, type) {
        var messageContainer = document.getElementById('changePasswordMessage');
        if (!messageContainer) {
            return;
        }

        var trimmedMessage = (message || '').toString().trim();
        if (!trimmedMessage) {
            messageContainer.classList.add('d-none');
            return;
        }

        var alertType = type || 'info';
        messageContainer.className = 'alert alert-' + alertType;
        messageContainer.textContent = trimmedMessage;
        messageContainer.classList.remove('d-none');
    }

    function parseTruthy(value) {
        if (typeof value === 'boolean') {
            return value;
        }

        if (typeof value === 'number') {
            return value === 1;
        }

        if (typeof value === 'string') {
            var normalized = value.toLowerCase().trim();
            return normalized === 'true' || normalized === '1' || normalized === 'y' || normalized === 'yes' || normalized === 'success';
        }

        return false;
    }

    function isSuccessResponse(data) {
        if (!data || typeof data !== 'object') {
            return false;
        }

        var directSuccess = parseTruthy(data.success) ||
            parseTruthy(data.Success) ||
            parseTruthy(data.status) ||
            parseTruthy(data.Status) ||
            parseTruthy(data.isSuccess) ||
            parseTruthy(data.IsSuccess);

        if (directSuccess) {
            return true;
        }

        if (data.data && typeof data.data === 'object') {
            return isSuccessResponse(data.data);
        }

        return false;
    }

    function resolveMessage(payload, fallbackMessage) {
        if (typeof extractApiMessage === 'function') {
            var extracted = extractApiMessage(payload, fallbackMessage);
            if (extracted && extracted.toString().trim()) {
                return extracted.toString().trim();
            }
        }

        return (fallbackMessage || '').toString().trim();
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

    function onSubmitChangePassword() {
        var submitButton = document.getElementById('changePasswordSubmit');
        if (submitButton && submitButton.disabled) {
            return;
        }

        setInlineMessage('', 'info');
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
                var isSuccess = isSuccessResponse(data);
                if (isSuccess) {
                    var successMessage = resolveMessage(data, 'Password changed successfully.');
                    setInlineMessage(successMessage, 'success');
                    setProcessingState(true, false);
                    setTimeout(function () {
                        window.location.href = getLoginUrl();
                    }, 150);
                } else {
                    var failureMessage = resolveMessage(data, 'Password change failed. Please try again.');
                    setProcessingState(false, true);
                    alert(failureMessage);
                }
            },
            error: function (xhr, textStatus) {
                setProcessingState(false, true);
                var errorMessage = resolveMessage(xhr, 'Password change failed. Please try again.');
                if (typeof extractApiMessageFromXhr === 'function') {
                    var extractedMessage = extractApiMessageFromXhr(xhr, '');
                    if (extractedMessage && extractedMessage.toString().trim()) {
                        errorMessage = extractedMessage.toString().trim();
                    }
                }

                alert(errorMessage);
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
