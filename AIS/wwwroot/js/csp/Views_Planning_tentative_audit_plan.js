    function createAuditEngagement(planId, name, size, risk, freq, days, auditperiod, periodId, code, zoneId, entity_id, ent_type_id) {
        if (window.planningDashboard && typeof window.planningDashboard.loadNestedView === 'function') {
            window.planningDashboard.loadNestedView('TENTATIVE_ENGAGEMENT_PLAN', {
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
        }
    }
    $(document).ready(function () {
        $("#searchTableRecord").on("keyup", function () {
            var value = $(this).val().toLowerCase();
            $("#tentative_plan_list tbody tr").filter(function () {
                $(this).toggle($(this).text().toLowerCase().indexOf(value) > -1)
            });
        });
    });
