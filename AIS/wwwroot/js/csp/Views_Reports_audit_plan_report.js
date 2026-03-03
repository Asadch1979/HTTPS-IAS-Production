    function createAuditEngagement(planId, name, size, risk, freq, days, auditperiod, periodId, code, zoneId, entity_id, ent_type_id) {
        window.location.href = g_asiBaseURL + '/planning/tentative_engagement_plan?planId=' + planId + '&name=' + name + '&size=' + size + '&risk=' + risk + '&freq=' + freq + '&period=' + auditperiod + '&days=' + days + '&periodId=' + periodId + '&code=' + code + '&entityType=' + ent_type_id + '&zoneId=' + zoneId + '&entityId=' + entity_id;
    }
    $(document).ready(function () {
        $("#searchTableRecord").on("keyup", function () {
            var value = $(this).val().toLowerCase();
            $("#tentative_plan_list tbody tr").filter(function () {
                $(this).toggle($(this).text().toLowerCase().indexOf(value) > -1)
            });
        });
    });
