    var g_entId = 0;
    var g_entRecord = [];


    $(document).ready(function () {

        getEntityRelations();

    });

    function getEntityRelations() {
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_entity_relations",
            type: "POST",
            data: {

            },
            cache: false,
            success: function (data) {
                g_entRecord = data;
                $('#entityRelationsGrid tbody').empty();
                $.each(data, function (i, v) {
                    $('#entityRelationsGrid tbody').append('<tr><td>' + ++i + '</td><td>' + v.id + '</td><td>' + v.entitY_REALTION_ID + '</td><td>' + v.parenT_ENTITY_TYPEID + '</td><td>' + v.chilD_ENTITY_TYPEID + '</td><td>' + v.parenT_NAME + '</td><td>' + v.chilD_NAME + '</td><td>' + v.status + '</td><td><a href="#" data-click="event.preventDefault();UpdateEntityType(' + v.autid + ');" class="text-danger">Update</a></td></tr>');
                });
            },
            dataType: "json",
        });
    }
    function reloadLocation() {
        $('#updateEntityType').modal('hide');
        getEntityRelations();
    }

    function UpdateEntityType(typeId) {
        g_entId = typeId;
        $('#updateEntityType').modal('show');

        $('#modalEntId').val('');
        $('#modalEntType').val('');
        $('#modalEntTypeDesc').val('');
        $('#modalEntAuitable').val('N');
        $('#modalAuditedCode').val('');
        $('#modalEntAuitedType').val('');
        $('#entAuditbyField').val(0);
        $.each(g_entRecord, function (i, v) {
            if (v.autid == g_entId) {

                $('#modalEntId').val(v.autid);
                $('#modalEntType').val(v.entitycode);
                $('#modalEntTypeDesc').val(v.entitytypedesc);
                $('#modalEntAuitable').val(v.auditable);
                $('#modalAuditedCode').val(v.auditedby);
                $('#modalEntAuitedType').val(v.audiT_TYPE);
                $('#entAuditbyField').val(v.auditeD_BY_ENTITY);

            }
        })

    }
    function saveChangesEntityType() {

        var entityUpdateModel = {
            'AUTID': $('#modalEntId').val(),
            'ENTITYCODE': $('#modalEntType').val(),
            'ENTITYTYPEDESC': $('#modalEntTypeDesc').val(),
            'AUDITABLE': $('#modalEntAuitable').val(),
            'AUDITEDBY': $('#modalAuditedCode').val(),
            'AUDITED_BY_ENTITY': $('#entAuditbyField').val(),
            'AUDIT_TYPE': $('#modalEntAuitedType').val(),
            'E_AUTID': g_entId
        }
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/update_entity_types",
            type: "POST",
            data: {
                ENTITY_MODEL: entityUpdateModel
            },
            cache: false,
            success: function (data) {
                showApiAlert(data);
                onAlertCallback(reloadLocation);

            },
            dataType: "json",
        });
    }
