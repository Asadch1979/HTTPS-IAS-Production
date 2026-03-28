(function () {
    var host = document.getElementById('fieldAuditJoinReplica');
    if (!host) {
        return;
    }

    var engId = parseInt(host.getAttribute('data-eng-id') || '0', 10);
    var completionDate = host.getAttribute('data-completion-date') || '';
    var submitButton = document.getElementById('joinReplicaSubmitBtn');
    var statusHost = document.getElementById('joinReplicaStatus');

    function showInlineMessage(message, success) {
        if (!statusHost) {
            return;
        }

        var css = success ? 'alert alert-success mb-0' : 'alert alert-danger mb-0';
        statusHost.innerHTML = '<div class="' + css + '">' + message + '</div>';
    }

    function disableSubmit() {
        if (submitButton) {
            submitButton.disabled = true;
        }
    }

    if (!submitButton) {
        return;
    }

    submitButton.addEventListener('click', function () {
        if (submitButton.disabled || !engId) {
            return;
        }

        var formData = new FormData();
        formData.append('ID', '0');
        formData.append('ENG_PLAN_ID', engId.toString());
        formData.append('COMPLETION_DATE', completionDate);

        fetch((window.g_asiBaseURL || '') + '/FieldAudit/SubmitJoin', {
            method: 'POST',
            body: formData,
            credentials: 'same-origin'
        })
            .then(function (response) { return response.json(); })
            .then(function (data) {
                if (window.showApiAlert) {
                    showApiAlert(data);
                }

                if (data && data.status) {
                    showInlineMessage(data.message || 'Joining submitted successfully.', true);
                    if (data.isSubmitted) {
                        disableSubmit();
                        if (window.fieldAuditDashboard && typeof window.fieldAuditDashboard.refreshEngagementState === 'function') {
                            window.fieldAuditDashboard.refreshEngagementState();
                        }
                    }
                    return;
                }

                showInlineMessage((data && data.message) || 'Unable to submit joining report.', false);
            })
            .catch(function () {
                showInlineMessage('Unable to submit joining report.', false);
            });
    });
})();
