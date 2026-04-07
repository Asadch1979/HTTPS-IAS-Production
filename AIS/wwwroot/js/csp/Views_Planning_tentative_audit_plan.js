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
