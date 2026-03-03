        function toggleVisibility(inputId, btn){
            const input = document.getElementById(inputId);
            const icon = btn.querySelector('i');
            if (input.type === 'password') {
                input.type = 'text';
                icon.classList.remove('bi-eye');
                icon.classList.add('bi-eye-slash');
            } else {
                input.type = 'password';
                icon.classList.remove('bi-eye-slash');
                icon.classList.add('bi-eye');
            }
        }

        async function submitNewPassword(){
            const pass1 = document.getElementById('newPassword').value;
            const pass2 = document.getElementById('confirmPassword').value;

            if (!pass1 || !pass2) { alert('Please enter both fields.'); return; }
            if (pass1 !== pass2) { alert('Passwords do not match.'); return; }
            if (pass1.length < 8) { alert('Password must be at least 8 characters.'); return; }

            const baseUrl = (typeof g_asiBaseURL !== 'undefined' && g_asiBaseURL)
                ? g_asiBaseURL.replace(/\/+$/, '')
                : '';

            try {
                const endpoint = baseUrl + '/ApiCalls/UpdateSbpObservationPassword';
                let response;

                try {
                    response = await fetchWithPageId(endpoint, {
                        method: 'POST',
                        headers: {
                            'Content-Type': 'application/json',
                            'X-Requested-With': 'XMLHttpRequest'
                        },
                        credentials: 'same-origin',
                        body: JSON.stringify({ newPassword: pass1 })
                    });
                } catch (networkError) {
                    console.error(networkError);
                    throw new Error('Unable to reach the server. Please try again.');
                }

                const contentType = response.headers.get('content-type') || '';
                let payload = null;

                if (contentType.includes('application/json')) {
                    try {
                        payload = await response.clone().json();
                    } catch (_) {
                        payload = null;
                    }
                }

                if (payload == null) {
                    try {
                        const text = await response.text();
                        if (text) {
                            if (contentType.includes('application/json')) {
                                try {
                                    payload = JSON.parse(text);
                                } catch (_) {
                                    payload = { message: text };
                                }
                            } else {
                                payload = { message: text };
                            }
                        }
                    } catch (_) {
                        payload = null;
                    }
                }

                if (response.status === 401) {
                    const message = extractApiMessage(payload, 'Your session has expired. Please sign in again.');
                    alert(message);
                    window.location.href = baseUrl + '/Login/Index';
                    return;
                }

                if (!response.ok) {
                    const message = extractApiMessage(payload, 'Password update failed.');
                    throw new Error(message);
                }

                if (!payload || typeof payload !== 'object' || payload.success !== true) {
                    const message = extractApiMessage(payload, 'Password update failed.');
                    throw new Error(message);
                }

                sessionStorage.removeItem('SBP_AUTH');
                sessionStorage.removeItem('SBP_AUTH_RETAIN');
                const successMessage = extractApiMessage(payload, 'Password updated. You will need to re-authenticate next time.');
                alert(successMessage);
                window.location.href = baseUrl + '/HM/SbpObservationRegister';
            } catch (error) {
                console.error(error);
                alert(extractApiMessage(error, 'Password update failed.'));
            }
        }
