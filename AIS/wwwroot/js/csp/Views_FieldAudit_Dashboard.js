(function () {
    const state = window.fieldAuditDashboard || { engId: 0, stepCode: '' };

    function getJson(url, data) {
        return $.ajax({ url: g_asiBaseURL + url, type: 'GET', data: data || {}, dataType: 'json' });
    }

    function postJson(url, data) {
        return $.ajax({ url: g_asiBaseURL + url, type: 'POST', data: JSON.stringify(data || {}), contentType: 'application/json', dataType: 'json' });
    }

    function renderDynamicTable(tableId, rows) {
        const $table = $(tableId);
        const $thead = $table.find('thead');
        const $tbody = $table.find('tbody');
        $thead.empty();
        $tbody.empty();
        if (!rows || !rows.length) {
            $thead.append('<tr><th>Info</th></tr>');
            $tbody.append('<tr><td>No records found.</td></tr>');
            return;
        }
        const keys = Object.keys(rows[0]).slice(0, 8);
        $thead.append('<tr>' + keys.map(k => `<th>${k}</th>`).join('') + '</tr>');
        rows.forEach(r => {
            $tbody.append('<tr>' + keys.map(k => `<td>${r[k] == null ? '' : r[k]}</td>`).join('') + '</tr>');
        });
    }

    function initJoining() {
        getJson('/Engagement/get_joining_details', { engId: state.engId }).done(data => {
            if (!data) return;
            const meta = [
                ['Entity', data.entitY_NAME],
                ['Risk', data.risk],
                ['Size', data.size],
                ['Start', (data.starT_DATE || '').split('T')[0]],
                ['End', (data.enD_DATE || '').split('T')[0]],
                ['Team', data.teaM_NAME],
                ['Audit Period', data.audiT_PERIOD]
            ];
            meta.forEach(x => $('#joiningMetaBody').append(`<tr><th>${x[0]}</th><td>${x[1] || ''}</td></tr>`));
            const endDate = (data.enD_DATE || '').split('T')[0];
            (data.teaM_DETAILS || []).forEach(t => {
                $('#joiningTeamTable tbody').append(`<tr><td>${t.emP_NAME}</td><td>${t.pP_NO}</td><td>${new Date().toISOString().split('T')[0]}</td><td>${endDate}</td><td>${t.iS_TEAM_LEAD}</td></tr>`);
            });
            $('#saveJoiningBtn').off('click').on('click', function () {
                const first = (data.teaM_DETAILS || [])[0];
                if (!first) return;
                postJson('/FieldAudit/SaveJoining', {
                    ID: 0,
                    ENG_PLAN_ID: data.enG_PLAN_ID,
                    TEAM_MEM_PPNO: first.pP_NO,
                    JOINING_DATE: new Date().toISOString().split('T')[0],
                    COMPLETION_DATE: endDate
                }).done(showApiAlert);
            });
        });
    }

    function initByStep() {
        switch (state.stepCode) {
            case 'SAMPLING':
                getJson('/ApiCalls/get_list_of_samples', { ENG_ID: state.engId }).done(d => renderDynamicTable('#samplingTable', d));
                break;
            case 'EXCEPTION_REPORT':
                getJson('/ApiCalls/get_list_of_reports', { ENG_ID: state.engId }).done(d => renderDynamicTable('#exceptionTable', d));
                break;
            case 'WORKING_PAPER':
                getJson('/ApiCalls/Get_Working_Paper_Loan_Cases', { ENGID: state.engId }).done(d => renderDynamicTable('#workingPaperTable', d));
                break;
            case 'MEMO_CREATION':
                getJson('/ApiCalls/get_observation_branches', { ENG_ID: state.engId }).done(d => renderDynamicTable('#memoTable', d));
                break;
            case 'SUBMIT_TO_AUDITEE':
                getJson('/ApiCalls/get_observations', { ENG_ID: state.engId }).done(d => renderDynamicTable('#submitAuditeeTable', d));
                break;
            case 'EXIT_AUDIT':
                getJson('/ApiCalls/closing_draft_report_status', { ENG_ID: state.engId }).done(d => renderDynamicTable('#exitAuditTable', d));
                $('#closeAuditBtn').off('click').on('click', function () {
                    $.post(g_asiBaseURL + '/ApiCalls/close_draft_audit', { ENG_ID: state.engId }).done(showApiAlert);
                });
                break;
            case 'DRAFT_REPORT':
                getJson('/ApiCalls/get_observations_draft_branch', { ENG_ID: state.engId }).done(d => renderDynamicTable('#draftReportTable', d));
                break;
            case 'JOINING':
                initJoining();
                break;
        }
    }

    $(document).ready(function () {
        $('#faEngagementSelect').on('change', function () {
            const engId = $(this).val();
            const url = new URL(window.location.href);
            if (engId) {
                url.searchParams.set('engId', engId);
            } else {
                url.searchParams.delete('engId');
            }
            window.location.href = url.toString();
        });

        if (state.engId > 0) {
            initByStep();
        }
    });
})();
