    var g_entities = [];

    function getVal(obj, prop) {
        return obj[prop] ?? obj[prop.toLowerCase()] ?? obj[prop.toUpperCase()] ?? obj[prop.replace(/_/g, '')];
    }
    function loadEntities() {
        $('#entitiesGrid tbody').empty();
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/GetAuditeeEntitiesByTypeId",
            type: "POST",
            data: {
                'ENTITY_TYPE_ID': $('#entTypeField option:selected').val()
            },

            cache: false,
            success: function (data) {
                g_entities = data;
                $.each(data, function (i, v) {
                    var id = getVal(v, 'entitY_ID');
                    $('#entitiesGrid tbody').append('<tr><td>' + id + '</td><td>' + getVal(v, 'code') + '</td><td>' + getVal(v, 'name') + '</td><td>' + getVal(v, 'active') + '</td><td>' + getVal(v, 'auditbY_NAME') + '</td><td>' + getVal(v, 'auditable') + '</td><td>' + getVal(v, 'address') + '</td><td>' + getVal(v, 'telephone') + '</td><td>' + getVal(v, 'emaiL_ADDRESS') + '</td><td>' + getVal(v, 'erisk') + '</td><td>' + getVal(v, 'esize') + '</td><td><a href="#" onclick="event.preventDefault();editEntity(' + id + ');" class="text-danger">Update</a></td></tr>');
                });
            },
            dataType: "json",
        });
    }
    function editEntity(id) {
        var ent = g_entities.find(e => getVal(e, 'entitY_ID') == id);
        if (!ent) return;
        $('#modalEntityId').val(getVal(ent, 'entitY_ID'));
        $('#modalCode').val(getVal(ent, 'code'));
        $('#modalName').val(getVal(ent, 'name'));
        $('#modalActive').val(getVal(ent, 'active'));
        $('#modalAuditBy').val(getVal(ent, 'auditbY_NAME'));
        $('#modalAuditable').val(getVal(ent, 'auditable'));
        $('#modalAddress').val(getVal(ent, 'address'));
        $('#modalTelephone').val(getVal(ent, 'telephone'));
        $('#modalEmail').val(getVal(ent, 'emaiL_ADDRESS'));
        $('#modalRisk').val(String(getVal(ent, 'risK_ID')));
        $('#modalSize').val(String(getVal(ent, 'sizE_ID')));
        $('#updateEntityModal').modal('show');
    }
    function saveEntity() {
        var model = {
            'ENTITY_ID': $('#modalEntityId').val(),
            'CODE': $('#modalCode').val(),
            'NAME': $('#modalName').val(),
            'ACTIVE': $('#modalActive').val(),
            'AUDITABLE': $('#modalAuditable').val(),
           // 'AUDITBY_NAME' $('#modalAuditBy').val(),
            'ADDRESS': $('#modalAddress').val(),
            'TELEPHONE': $('#modalTelephone').val(),
            'EMAIL_ADDRESS': $('#modalEmail').val(),
            'RISK_ID': $('#modalRisk').val(),
            'SIZE_ID': $('#modalSize').val(),
            'UP_STATUS': 'U'
        };
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/UpdateAuditeeEntity",
            type: "POST",
            data: { ENTITY_MODEL: model, IND: 'U' },
            cache: false,
            success: function (resp) {
                showApiAlert(resp);
                loadEntities();
            },
            dataType: "json",
        });
    }
