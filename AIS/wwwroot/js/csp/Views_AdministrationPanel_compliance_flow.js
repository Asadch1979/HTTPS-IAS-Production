    var g_entTypeId = 0;
    var g_groupId = 0;
    var g_Id = 0;
    $(document).ready(function () {
        $('#entityTypeSelectionField').select2();
        $('#entityTypeSelectionField').css("width", "100% !important");

        $('#groupSelectionPanel').hide();
        $('#prevNextGroupSelectionPanel').hide();
        entityTypeSelectionChangeEvent();

    });
    function entityTypeSelectionChangeEvent() {
        $('#entity_compliance_flow_grid tbody').empty();
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_compliance_flow_by_entity_type",
            type: "POST",
            data: {
                "ENTITY_TYPE_ID": $('#entityTypeSelectionField').val()
            },
            cache: false,
            success: function (data) {
                $.each(data, function (i, v) {
                    $('#entity_compliance_flow_grid').append('<tr><td>' + ++i + '</td><td>' + v.entitY_TYPE_NAME + '</td><td>' + v.grouP_NAME + '</td><td>' + v.nexT_GROUP_NAME + '</td><td>' + v.preV_GROUP_NAME + '</td><td>' + v.comP_UP_STATUS_DESC + '</td><td>' + v.comP_DOWN_STATUS_DESC + '</td><td><a href="#" data-onclick="event.preventDefault();updateComplianceFlow(' + v.id + ',' + v.entitY_TYPE_ID + ', ' + v.grouP_ID + ');">Edit</a></td></tr>');
                });

            },
            dataType: "json",
        });
    }
    function addComplianceWorkFlow() {
        if ($('#entityTypeSelectionField_modal option:selected').val() == 0) {
            alert("Please select Entity type to proceed");
            return;
        }
        if ($('#groupSelectionBox option:selected').val() == 0) {
            alert("Please select Group/Role State to proceed");
            return;
        }

        if ($('#prevGroupSelectionBox option:selected').val() == 0) {
            alert("Plase select Previous Role State to proceed");
            return;
        }
        if ($('#nextGroupSelectionBox option:selected').val() == 0) {
            alert("Plase select Next Role State to proceed");
            return;
        }
        if ($('#CompUpStatusSelectionBox option:selected').val() == 0) {
            alert("Plase select Compliance Up State to proceed");
            return;
        }
        if ($('#CompDownStatusSelectionBox option:selected').val() == 0) {
            alert("Plase select Compliance Down State to proceed");
            return;
        }
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/add_compliance_flow",
            type: "POST",
            data: {
                'GROUP_ID': $('#groupSelectionBox option:selected').val(),
                'ENTITY_TYPE_ID': $('#entityTypeSelectionField_modal option:selected').val(),
                'PREV_GROUP_ID': $('#prevGroupSelectionBox option:selected').val(),
                'NEXT_GROUP_ID': $('#nextGroupSelectionBox option:selected').val(),
                'COMP_UP_STATUS': $('#CompUpStatusSelectionBox option:selected').val(),
                'COMP_DOWN_STATUS': $('#CompDownStatusSelectionBox option:selected').val(),
                "ID": g_Id
            },
            cache: false,
            success: function (data) {
                showApiAlert(data);
                onAlertCallback(reloadPage)
            },
            dataType: "json",
        });
    }

    function addComplianceFlow() {
        g_entTypeId = 0;
        g_groupId = 0;
        g_Id = 0;
        $('#addComplianceFlowEntityTypeModal').modal('show');
        $('#addComplianceFlowEntityTypeModal .modal-title').html('Add Compliance Flow');
        $('#addComplianceFlowEntityTypeModal .btn-footer').html('Add Compliance Flow');
        $('#entityTypeSelectionField_modal').val(0);
        $('#groupSelectionBox').val(0);
        $('#prevGroupSelectionBox').val(0);
        $('#nextGroupSelectionBox').val(0);
        $('#CompUpStatusSelectionBox').val(0);
        $('#CompDownStatusSelectionBox').val(0);

    }

    function updateComplianceFlow(id,entId,grpId) {
        g_entTypeId = entId;
        g_groupId = grpId;
        g_Id = id;
        $('#addComplianceFlowEntityTypeModal').modal('show');
        $('#addComplianceFlowEntityTypeModal .modal-title').html('Update Compliance Flow');
        $('#addComplianceFlowEntityTypeModal .btn-footer').html('Update Compliance Flow');
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_compliance_flow_by_entity_type",
            type: "POST",
            data: {
                "ENTITY_TYPE_ID": g_entTypeId,
                    "GROUP_ID": g_groupId
            },
            cache: false,
            success: function (data) {
                $.each(data, function (i, v) {
                    $('#entityTypeSelectionField_modal').val(v.entitY_TYPE_ID);
                    $('#groupSelectionBox').val(v.grouP_ID);
                    $('#prevGroupSelectionBox').val(v.preV_GROUP_ID);
                    $('#nextGroupSelectionBox').val(v.nexT_GROUP_ID);
                    $('#CompUpStatusSelectionBox').val(v.comP_UP_STATUS);
                    $('#CompDownStatusSelectionBox').val(v.comP_DOWN_STATUS);
                });

            },
            dataType: "json",
        });
     
    }

    function reloadPage() {
        entityTypeSelectionChangeEvent();
    }
