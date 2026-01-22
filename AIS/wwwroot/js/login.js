(function ($) {
    function showLoginModal(title, message, type) {
        var headerType = type === 'session' ? 'session' : 'error';
        var $modal = $('#loginerrormodal');
        var $header = $modal.find('.modal-header');
        var $title = $modal.find('.modal-title');

        $header.removeClass('bg-danger bg-info text-white');

        if (headerType === 'session') {
            $header.addClass('bg-info text-white');
        } else {
            $header.addClass('bg-danger text-white');
        }

        $title.text(title);
        $('#errorDescField')
  .text(message)
  .css('white-space', 'pre-line');

        $modal.modal('show');
    }

    function resolveLoginError(res) {
        var isLockout = res && res.errorCode === 'LOCKED_OUT';
        var title = (res && res.errorTitle) || (isLockout ? 'Too many attempts' : 'Sign in failed');
        var message = (res && res.errorMsg) || 'Unable to sign in.';

        if (isLockout) {
            if (res && res.retryAfterSeconds) {
                var mins = Math.ceil(res.retryAfterSeconds / 60);
                message = 'For security, sign-in is temporarily blocked. Please try again in ' + mins + ' minute(s).';
            } else if (!res.errorMsg) {
                message = 'For security, sign-in is temporarily blocked. Please try again later.';
            }
        }

        return { title: title, message: message };
    }

    function enforceMaxLength($input) {
        var maxLength = parseInt($input.attr('maxlength'), 8);
        if (!maxLength) {
            return;
        }

        var value = ($input.val() || '').toString();
        if (value.length > maxLength) {
            $input.val(value.slice(0, maxLength));
        }
    }

    function submitKillSessionForm() {
        var ppNumber = ($('#inputPPNoField').val() || '').trim();
        var password = $('#inputPassword').val() || '';

        if (ppNumber === '' || password === '') {
            showLoginModal('Sign in failed', 'Please enter both P.P Number and Password to clear the session.');
            return;
        }

        $('#killSessionPPNumber').val(ppNumber);
        $('#killSessionPassword').val(encryptPassword(password));
        $('#killSessionForm').trigger('submit');
    }

    function submitPasswordResetRequest() {
        $('#resetPasswordModel').modal('show');
        $('#inputPPNoField_reset').val('');
        $('#inputCnicField_reset').val('');
    }

    function submitPasswordResetRequestHandler() {
        if ($('#inputPPNoField_reset').val() === '') {
            alert('Please enter PP Number to proceed');
            return;
        }
        if ($('#inputCnicField_reset').val() === '') {
            alert('Please enter CNIC Number to proceed');
            return;
        }
        $.ajax({
            url: g_asiBaseURL + '/Login/ResetPassword',
            type: 'POST',
            data: {
                'PPNumber': $('#inputPPNoField_reset').val(),
                'CNICNumber': $('#inputCnicField_reset').val(),
            },
            cache: false,
            success: function (data) {
                $('#resetPasswordModel').modal('hide');
                alert(data.message);
            },
            dataType: 'json',
        });
    }

    function getAntiForgeryToken() {
        var tokenInput = document.querySelector("input[name='__RequestVerificationToken']");
        return tokenInput ? tokenInput.value : null;
    }

    function resolveBaseUrl() {
        var pageBase = ($('#login-page').data('base-url') || '').toString().trim();
        var metaBase = document.querySelector('meta[name="base-url"]');
        var metaValue = metaBase ? metaBase.getAttribute('content') : '';
        var globalBase = (window.g_asiBaseURL || '').toString().trim();
        var pathName = window.location && window.location.pathname ? window.location.pathname.toLowerCase() : '';
        var loginIndex = pathName.indexOf('/login');
        var derivedBase = '';

        if (loginIndex > 0) {
            derivedBase = window.location.pathname.substring(0, loginIndex);
        }

        var resolvedBaseUrl = pageBase || metaValue || globalBase || derivedBase || '';

        if (resolvedBaseUrl && resolvedBaseUrl !== '/') {
            resolvedBaseUrl = resolvedBaseUrl.replace(/\/$/, '');
        }

        window.g_asiBaseURL = resolvedBaseUrl;
        return resolvedBaseUrl;
    }

    function handleLoginResponse(data) {
        if (!data.isAuthenticate) {
            $('#submitKillSessionButton').addClass('d-none');
            $('#submitLoginButton').removeClass('d-none');
            var err = resolveLoginError(data);
            showLoginModal(err.title, err.message);
            return;
        }

        if (data.isAlreadyLoggedIn) {
            $('#submitKillSessionButton').removeClass('d-none');
            $('#submitLoginButton').addClass('d-none');
            var title = data.errorTitle || 'Session Details';
            showLoginModal(title, data.errorMsg, 'session');
            $('#inputPPNoField').attr('disabled', true);
            $('#inputPassword').attr('disabled', true);
            return;
        }

        if (data.forcePwdChange || data.passwordChangeRequired || data.changePassword === 'Y') {
            var targetUrl = data.redirectUrl || (g_asiBaseURL + '/Home/Change_Password');
            window.location.href = targetUrl;
            return;
        }

        var baseUrl = resolveBaseUrl();
        window.location.href = baseUrl + '/Home/Index';
    }

    function executeLoginRequest() {
        var token = getAntiForgeryToken();
        if (!token) {
            $('#submitLoginButton').attr('disabled', false);
            showLoginModal('Sign in failed', 'Unable to validate the request. Please refresh the page and try again.');
            return;
        }

        $.ajax({
            url: g_asiBaseURL + '/Login/DoLogin',
            type: 'POST',
            data: {
                '__RequestVerificationToken': token,
                'login.PPNumber': $('#inputPPNoField').val(),
                'login.Password': encryptPassword($('#inputPassword').val())
            },
            headers: {
                'RequestVerificationToken': token
            },
            cache: false,
            success: function (data) {
                handleLoginResponse(data);
                $('#submitLoginButton').attr('disabled', false);
            },
            dataType: 'json',
            error: function (xhr) {
                $('#submitLoginButton').attr('disabled', false);
                if (xhr && xhr.status === 503) {
                    window.location.href = g_asiBaseURL + '/Login/Maintenance';
                    return;
                }

                $('#submitKillSessionButton').addClass('d-none');
                $('#submitLoginButton').removeClass('d-none');
                showLoginModal('Sign in failed', 'Unable to process your request. Please try again later or \nPress Ctrl + F5');

            }
        });
    }

    function doLoginSubmit() {
        $('#submitLoginButton').attr('disabled', true);
        executeLoginRequest();
    }

    function dispatchSessionStatus() {
        var $statusInput = $('#sessionStatusValue');
        if (!$statusInput.length) {
            return;
        }

        var statusMessage = ($statusInput.val() || '').trim();
        if (!statusMessage) {
            return;
        }

        document.dispatchEvent(new CustomEvent('login:sessionStatus', { detail: statusMessage }));
    }

    function bindEventHandlers() {
        $('.pp-limited').on('input', function () {
            enforceMaxLength($(this));
        });

        $('#submitKillSessionButton').on('click', submitKillSessionForm);
        $('#submitLoginButton').on('click', doLoginSubmit);
        $('#forgotPasswordLink').on('click', function (event) {
            event.preventDefault();
            submitPasswordResetRequest();
        });
        $('#submitPasswordResetButton').on('click', submitPasswordResetRequestHandler);

        $(document).on('keydown', function (event) {
            var id = event.key || event.which || event.keyCode || 0;
            if (id === 'Enter' || id === 13) {
                doLoginSubmit();
            }
        });
    }

    document.addEventListener('login:sessionStatus', function (event) {
        var statusMessage = (event && event.detail) ? event.detail.toString().trim() : '';
        if (!statusMessage || !$('#login-page').length) {
            return;
        }

        showLoginModal('Session Details', statusMessage, 'session');
    });

    $(document).ready(function () {
        var $loginPage = $('#login-page');
        if (!$loginPage.length) {
            return;
        }

        bindEventHandlers();
        resolveBaseUrl();
        dispatchSessionStatus();
    });
})(jQuery);
