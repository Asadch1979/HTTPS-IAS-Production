    function createAuditEngagement(planId, name, size, risk, freq, days, auditperiod, periodId, code, zoneId, entity_id, ent_type_id) {
        if (window.planningDashboard && typeof window.planningDashboard.loadSubChildStep === 'function') {
            window.planningDashboard.loadSubChildStep('AUDIT_PLAN', 'ENGAGEMENT_PLAN', 'CREATE', {
                planId: planId,
                name: name,
                size: size,
                risk: risk,
                freq: freq,
                days: days,
                period: auditperiod,
                periodId: periodId,
                code: code,
                zoneId: zoneId,
                entityId: entity_id,
                entityType: ent_type_id
            });
            return;
        }

        var baseUrl = (typeof g_asiBaseURL === 'string' && g_asiBaseURL) ? g_asiBaseURL : '';
        var query = new URLSearchParams({
            planId: planId || 0,
            name: name || '',
            size: size || '',
            risk: risk || '',
            freq: freq || '',
            days: days || '',
            period: auditperiod || '',
            periodId: periodId || 0,
            code: code || '',
            zoneId: zoneId || 0,
            entityId: entity_id || 0,
            entityType: ent_type_id || 0
        });

        window.location.href = baseUrl + '/Planning/tentative_engagement_plan?' + query.toString();
    }
    $(document).ready(function () {
        $("#searchTableRecord").on("keyup", function () {
            var value = $(this).val().toLowerCase();
            $("#tentative_plan_list tbody tr").filter(function () {
                $(this).toggle($(this).text().toLowerCase().indexOf(value) > -1)
            });
        });
    });


// Bind delegated actions once: this script may also be loaded inside the planning dashboard partial.
(function () {
    if (window.__iasPlanDeletionBound) return;
    window.__iasPlanDeletionBound = true;

    document.addEventListener('click', function (event) {
        var createButton = event.target.closest('.js-create-audit-engagement');
        if (createButton) {
            var d = createButton.dataset;
            createAuditEngagement(d.planId, d.name, d.size, d.risk, d.freq, d.days,
                d.period, d.periodId, d.code, d.zoneId, d.entityId, d.entityType);
            return;
        }

        var requestButton = event.target.closest('.js-request-plan-deletion');
        if (requestButton) {
            var dialog = document.getElementById('planDeletionDialog');
            if (!dialog) return;
            var planId = document.getElementById('planDeletionPlanId');
            var summary = document.getElementById('planDeletionSummary');
            var reason = document.getElementById('planDeletionReason');
            var validation = document.getElementById('planDeletionValidation');
            planId.value = requestButton.dataset.planId || '';
            summary.textContent = (requestButton.dataset.entityName || 'Selected audit') +
                ' | ' + (requestButton.dataset.periodName || '');
            reason.value = '';
            validation.textContent = '';
            if (typeof dialog.showModal === 'function') dialog.showModal();
            else dialog.setAttribute('open', '');
            reason.focus();
            return;
        }

        if (event.target.closest('#cancelPlanDeletion')) {
            var dlg = document.getElementById('planDeletionDialog');
            if (dlg) {
                if (typeof dlg.close === 'function') dlg.close();
                else dlg.removeAttribute('open');
            }
        }
    });

    document.addEventListener('submit', function (event) {
        if (!event.target || event.target.id !== 'planDeletionRequestForm') return;
        var reason = document.getElementById('planDeletionReason');
        var validation = document.getElementById('planDeletionValidation');
        var trimmed = (reason.value || '').trim();
        if (trimmed.length < 10 || trimmed.length > 2000) {
            event.preventDefault();
            validation.textContent = 'Please enter a deletion reason between 10 and 2000 characters.';
            reason.focus();
        } else {
            document.getElementById('submitPlanDeletion').disabled = true;
        }
    });
})();
