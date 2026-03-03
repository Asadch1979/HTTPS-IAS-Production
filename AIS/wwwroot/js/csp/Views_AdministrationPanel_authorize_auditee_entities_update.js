    var g_pending = [];
    var g_current = null;
    function getVal(obj, prop) {
        return obj[prop] ?? obj[prop.toLowerCase()] ?? obj[prop.toUpperCase()] ?? obj[prop.replace(/_/g, '')];
    }
    function loadPending() {
        $('#pendingGrid tbody').empty();
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/GetAuditeeEntitiesPendingAuthorization",
            type: "POST",
            cache: false,
            success: function (data) {
                var pendingList = Array.isArray(data) ? data : (data ? [data] : []);
                g_pending = pendingList;
                if (!pendingList.length) {
                    $('#pendingGrid tbody').append('<tr><td colspan="12" class="text-center">No pending updates found.</td></tr>');
                    return;
                }
                $.each(pendingList, function (i, v) {
                    var id = getVal(v, 'entitY_ID');
                    $('#pendingGrid tbody').append(
                        '<tr><td>' + id + '</td>' +
                        '<td>' + getVal(v, 'code') + '</td>' +
                        '<td>' + getVal(v, 'name') + '</td>' +
                        '<td>' + getVal(v, 'active') + '</td>' +
                        '<td>' + getVal(v, 'auditbY_NAME') + '</td>' +
                        '<td>' + getVal(v, 'auditable') + '</td>' +
                        '<td>' + getVal(v, 'address') + '</td>' +
                        '<td>' + getVal(v, 'telephone') + '</td>' +
                        '<td>' + getVal(v, 'emaiL_ADDRESS') + '</td>' +
                        '<td>' + getVal(v, 'erisk') + '</td>' +
                        '<td>' + getVal(v, 'esize') + '</td>' +
                        '<td>' +
                        '<a href="#" onclick="event.preventDefault();openCompare(' + id + ');" class="text-danger me-2">View</a>' +
                        '</td></tr>');
                });
            },
            dataType: "json",
        });
    }
    function openCompare(id) {
        var ent = g_pending.find(e => getVal(e, 'entitY_ID') == id);
        if (!ent) return;
        g_current = ent;
        $('#compareEntityId').val(id);
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/GetAuditeeEntityByIdforAuthorization",
            type: "POST",
            data: { ENTITY_ID: id },
            cache: false,
            success: function (orig) {
                if (!orig) return;

                $('#oldCode').val(getVal(orig, 'codE_OLD'));
                $('#oldName').val(getVal(orig, 'namE_OLD'));
                $('#oldActive').val(getVal(orig, 'activE_OLD'));
                $('#oldAuditBy').val(getVal(orig, 'auditbY_NAME_OLD'));
                $('#oldAuditable').val(getVal(orig, 'auditablE_OLD'));
                $('#oldAddress').val(getVal(orig, 'addresS_OLD'));
                $('#oldTelephone').val(getVal(orig, 'telephonE_OLD'));
                $('#oldEmail').val(getVal(orig, 'emaiL_ADDRESS_OLD'));
                $('#oldRiskId').val(getVal(orig, 'risK_ID_OLD'));
                $('#oldSizeId').val(getVal(orig, 'sizE_ID_OLD'));
                $('#oldRisk').val(getVal(orig, 'erisK_OLD'));
                $('#oldSize').val(getVal(orig, 'esizE_OLD'));

                $('#newCode').val(getVal(ent, 'code'));
                $('#newName').val(getVal(ent, 'name'));
                $('#newActive').val(getVal(ent, 'active'));
                $('#newAuditBy').val(getVal(ent, 'auditbY_NAME'));
                $('#newAuditable').val(getVal(ent, 'auditable'));
                $('#newAddress').val(getVal(ent, 'address'));
                $('#newTelephone').val(getVal(ent, 'telephone'));
                $('#newEmail').val(getVal(ent, 'emaiL_ADDRESS'));
                $('#newRiskId').val(getVal(ent, 'risk_ID'));
                $('#newSizeId').val(getVal(ent, 'size_ID'));
                $('#newRisk').val(getVal(ent, 'erisk'));
                $('#newSize').val(getVal(ent, 'esize'));

                $('#compareModal').modal('show');
            },
            dataType: "json",
        });
    }
    function authorizeCurrent() {
        if (!g_current) return;
        g_current.UP_STATUS = 'A';
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/UpdateAuditeeEntity",
            type: "POST",
            data: { ENTITY_MODEL: g_current, IND: 'A' },
            cache: false,
            success: function (resp) {
                showApiAlert(resp);
                $('#compareModal').modal('hide');
                loadPending();
            },
            dataType: "json",
        });
    }
    function rejectCurrent() {
        if (!g_current) return;
        g_current.UP_STATUS = 'R';
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/UpdateAuditeeEntity",
            type: "POST",
            data: { ENTITY_MODEL: g_current, IND: 'R' },
            cache: false,
            success: function (resp) {
                showApiAlert(resp);
                $('#compareModal').modal('hide');
                loadPending();
            },
            dataType: "json",
        });
    }
    function authorizeEntry(id) {
        var ent = g_pending.find(e => getVal(e, 'entitY_ID') == id);
        if (!ent) return;
        g_current = ent;
        authorizeCurrent();
    }
    function rejectEntry(id) {
        var ent = g_pending.find(e => getVal(e, 'entitY_ID') == id);
        if (!ent) return;
        g_current = ent;
        rejectCurrent();
    }
    $(document).ready(function(){
        loadPending();
    });
