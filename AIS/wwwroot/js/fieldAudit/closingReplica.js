(function () {
    var host = document.getElementById('fieldAuditClosingReplica');
    if (!host) {
        return;
    }

    var engId = parseInt(host.getAttribute('data-eng-id') || document.getElementById('fieldAuditClosingEngId')?.value || '0', 10);
    var entityNameField = document.getElementById('fieldAuditClosingEntityName');
    var joiningTBody = document.querySelector('#fieldAuditJoiningAuditorDetailsTable tbody');
    var progressTBody = document.querySelector('#fieldAuditAuditorWiseProgessTable tbody');
    var closeButton = document.getElementById('fieldAuditCloseDraftAuditBtn');

    function safeDate(value) {
        if (!value) {
            return '';
        }

        return value.toString().split('T')[0];
    }

    function loadClosingData() {
        if (!engId || engId <= 0) {
            return;
        }

        if (joiningTBody) {
            joiningTBody.innerHTML = '';
        }

        if (progressTBody) {
            progressTBody.innerHTML = '';
        }

        fetch((window.g_asiBaseURL || '') + '/ApiCalls/closing_draft_report_status', {
            method: 'POST',
            headers: { 'Content-Type': 'application/x-www-form-urlencoded; charset=UTF-8' },
            body: 'ENG_ID=' + encodeURIComponent(engId),
            credentials: 'same-origin'
        })
            .then(function (res) { return res.json(); })
            .then(function (data) {
                if (!data || !data.length) {
                    return;
                }

                if (entityNameField) {
                    entityNameField.textContent = data[0].entitY_NAME || '';
                }

                data.forEach(function (item) {
                    if (joiningTBody) {
                        var joinRow = document.createElement('tr');
                        joinRow.innerHTML = '<td>' + (item.teaM_MEM_PPNO || '') + '</td>'
                            + '<td>' + (item.membeR_NAME || '') + '</td>'
                            + '<td>' + safeDate(item.joininG_DATE) + '</td>'
                            + '<td>' + safeDate(item.completioN_DATE) + '</td>';
                        joiningTBody.appendChild(joinRow);
                    }

                    if (progressTBody) {
                        var progressRow = document.createElement('tr');
                        progressRow.innerHTML = '<td class="text-center">' + (item.membeR_NAME || '') + '</td>'
                            + '<td class="text-center">' + (item.isteamlead || '') + '</td>'
                            + '<td class="text-center">' + (item.totaL_NO_OB || 0) + '</td>'
                            + '<td class="text-center">' + (item.dropped || 0) + '</td>'
                            + '<td class="text-center">' + (item.submitteD_TO_AUDITEE || 0) + '</td>'
                            + '<td class="text-center">' + (item.responded || 0) + '</td>'
                            + '<td class="text-center">' + (item.resolved || 0) + '</td>'
                            + '<td class="text-center">' + (item.addeD_TO_DRAFT || 0) + '</td>';
                        progressTBody.appendChild(progressRow);
                    }
                });
            });
    }

    function closeDraftAudit() {
        if (!engId || engId <= 0) {
            return;
        }

        fetch((window.g_asiBaseURL || '') + '/ApiCalls/close_draft_audit', {
            method: 'POST',
            headers: { 'Content-Type': 'application/x-www-form-urlencoded; charset=UTF-8' },
            body: 'ENG_ID=' + encodeURIComponent(engId),
            credentials: 'same-origin'
        })
            .then(function (res) { return res.text(); })
            .then(function (data) {
                if (window.showApiAlert) {
                    showApiAlert(data);
                }
            });
    }

    if (closeButton) {
        closeButton.addEventListener('click', closeDraftAudit);
    }

    loadClosingData();
})();
