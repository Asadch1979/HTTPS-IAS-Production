    var g_desigId = 0;
    var g_groupId = 0;
    var g_assignmentId = 0;
    var g_entType = '';
    $(document).ready(function () {
        $("#searchTableRecord").on("keyup", function () {
            var value = $(this).val().toLowerCase();
            $("#hr_desig_wise_role_grid tbody tr").filter(function () {
                $(this).toggle($(this).text().toLowerCase().indexOf(value) > -1)
            });
        });

        getHRDesignationWiseRole();

    });
    function getHRDesignationWiseRole() {
        $('#hr_desig_wise_role_grid tbody').empty();
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_hr_designation_wise_roles",
            type: "POST",
            data: {
            },
            cache: false,
            success: function (data) {
                $.each(data, function (i, v) {
                    $('#hr_desig_wise_role_grid').append('<tr><td>' + ++i + '</td><td>' + v.designatioN_CODE + '</td><td>' + v.description + '</td><td>' + v.rolE_ID + '</td><td>' + v.role + '</td><td>' + v.entitY_TYPE + '</td><td><a href="#" data-onclick="event.preventDefault();updateHRDesignationCode(' + v.id + ',' + v.designatioN_CODE + ',' + v.rolE_ID + ', \'' + v.entitY_TYPE + '\' );">Update</a></td></tr>');
                });

            },
            dataType: "json",
        });
    }
    function addHRDesignationCode(assignId, desigId, grpId, entType) {
        g_assignmentId = assignId;
        g_desigId = desigId;
        g_groupId = grpId;
        g_entType = entType;
        $('#hRDesignationWiseRoleModal').modal("show");
        $('#hRDesignationWiseRoleModal .modal-title').html("New HR Designation Wise Role Assignment");
        $('#HRDesigSelectionField').val(g_desigId);
        $('#RoleSelectionBox').val(g_groupId);
        $('#EntTypeSelectionField').val(g_entTypeId);
    }



    function updateHRDesignationCode(assignId, desigId, grpId, entType) {
        g_assignmentId = assignId;
        g_desigId = desigId;
        g_groupId = grpId;
        g_entType = entType;
        $('#hRDesignationWiseRoleModal').modal("show");
        $('#hRDesignationWiseRoleModal .modal-title').html("Update HR Designation Wise Role Assignment");
        $('#HRDesigSelectionField').val(g_desigId);
        $('#RoleSelectionBox').val(g_groupId);
        $('#EntTypeSelectionField').val(g_entType);
    }

    function submitAddNewHRDesignationWiseRoleAssignment() {

        if (g_assignmentId == 0) {
            $.ajax({
                url: g_asiBaseURL + "/ApiCalls/add_hr_designation_wise_role_assignment",
                type: "POST",
                data: {
                    'ASSIGNMENT_ID': g_assignmentId,
                    'DESIGNATION_ID': $('#HRDesigSelectionField option:selected').val(),
                    'GROUP_ID': $('#RoleSelectionBox option:selected').val(),
                    'SUB_ENTITY_NAME': $('#EntTypeSelectionField option:selected').text()
                },
                cache: false,
                success: function (data) {
                    showApiAlert(data);
                    onAlertCallback(reloadPage)
                },
                dataType: "json",
            });
        } else {
            $.ajax({
                url: g_asiBaseURL + "/ApiCalls/update_hr_designation_wise_role_assignment",
                type: "POST",
                data: {
                    'ASSIGNMENT_ID': g_assignmentId,
                    'DESIGNATION_ID': $('#HRDesigSelectionField option:selected').val(),
                    'GROUP_ID': $('#RoleSelectionBox option:selected').val(),
                    'SUB_ENTITY_NAME': $('#EntTypeSelectionField option:selected').text()
                },
                cache: false,
                success: function (data) {
                    showApiAlert(data);
                    onAlertCallback(reloadPage)
                },
                dataType: "json",
            });
        }

    }


    function reloadPage() {
        window.location.reload();
    }
