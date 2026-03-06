    var g_entId = 0;
    var g_entRecord = [];


    $(document).ready(function () {

    });

    function getEntityMapping() {
        var indicator = "";
        if ($('#entityIDDropBox').val() != 0) {
            indicator = "Y";
        }
        else if ($('#parentTypeID').val() != 0) {
            indicator = "Y";
        }
        else if ($('#childTypeID').val() != 0) {
            indicator = "Y";
        }
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_entities_mapping",
            type: "POST",
            data: {
                "ENT_ID": $('#entityIDDropBox').val(),
                "P_TYPE": $('#parentTypeID').val(),
                "C_TYPE": $('#childTypeID').val(),
                "IND":indicator
            },
            cache: false,
            success: function (data) {
                g_entRecord = data;
                $('#entityMappingGrid tbody').empty();
                $.each(data, function (i, v) {
                    $('#entityMappingGrid tbody').append('<tr><td>' + ++i + '</td><td>' + v.relatioN_TYPE_ID + '</td><td>' + v.p_TYPE_ID + '</td><td>' + v.c_TYPE_ID + '</td><td>' + v.parenT_CODE + '</td><td>' + v.chilD_CODE + '</td><td>' + v.parenT_ID + '</td><td>' + v.entitY_ID + '</td><td>' + v.p_NAME + '</td><td>' + v.c_NAME + '</td><td>' + v.status + '</td><td><a href="#" data-click="event.preventDefault();UpdateEntityType(' + v.autid + ');" class="text-danger">Update</a></td></tr>');
                });
            },
            dataType: "json",
        });
    }
    function getEntityMappingReporting() {
        var indicator = "";
        if ($('#entityIDDropBox').val() != 0) {
            indicator = "Y";
        }
        else if ($('#parentTypeID').val() != 0) {
            indicator = "Y";
        }
        else if ($('#childTypeID').val() != 0) {
            indicator = "Y";
        }
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_entities_mapping_reporting",
            type: "POST",
            data: {
                "ENT_ID": $('#entityIDDropBox').val(),
                "P_TYPE": $('#parentTypeID').val(),
                "C_TYPE": $('#childTypeID').val(),
                "IND": indicator
            },
            cache: false,
            success: function (data) {
                g_entRecord = data;
                $('#entityMappingGrid tbody').empty();
                $.each(data, function (i, v) {
                    $('#entityMappingGrid tbody').append('<tr><td>' + ++i + '</td><td>' + v.relatioN_TYPE_ID + '</td><td>' + v.p_TYPE_ID + '</td><td>' + v.c_TYPE_ID + '</td><td>' + v.parenT_CODE + '</td><td>' + v.chilD_CODE + '</td><td>' + v.parenT_ID + '</td><td>' + v.entitY_ID + '</td><td>' + v.p_NAME + '</td><td>' + v.c_NAME + '</td><td>' + v.status + '</td><td><a href="#" data-click="event.preventDefault();UpdateEntityType(' + v.autid + ');" class="text-danger">Update</a></td></tr>');
                });
            },
            dataType: "json",
        });
    }
    function reloadLocation() {
        $('#updateEntityType').modal('hide');
        getEntitiesMapping();
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
    function getEntitiesMapping() {
        if ($('#datasource').val() == 0) {
            $('#entityMappingGrid tbody').empty();
        } else {
            $('#entityMappingGrid tbody').empty();
            if ($('#datasource').val() == "mapping") {
                getEntityMapping();
            } else {
                getEntityMappingReporting();
            }

        }
    }
    function getEntityIDDropBox() {
        $('#entityIDDropBox').empty();
        $('#entityIDDropBox').append('<option value=0>--Select Entity--</option>');
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_entities_of_parent_child",
            type: "POST",
            data: {
                'P_TYPE_ID': $('#parentTypeID').val(),
                'C_TYPE_ID': $('#childTypeID').val(),
            },
            cache: false,
            success: function (data) {
                $.each(data, function (i, v) {
                    $('#entityIDDropBox').append('<option value=' + v.parenT_ID + '>' + v.p_NAME + '</option>');
                });
             
            },
            dataType: "json",
        });
    }
