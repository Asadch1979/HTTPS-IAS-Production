      



    var g_rowId = 0;
    var g_statusRecord = [];


    $(document).ready(function () {


        $("#searchTableRecord").on("keyup", function () {
            var value = $(this).val().toLowerCase();
            $("#manageEntityAuditDeptGrid tbody tr").filter(function () {
                $(this).toggle($(this).text().toLowerCase().indexOf(value) > -1)
            });
        });
        getManageEntityAuditDeptt();

    });

    function getManageEntityAuditDeptt() {
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_manage_ent_audit_dept",
            type: "POST",
            data: {

            },
            cache: false,
            success: function (data) {
                g_statusRecord = data;
                $('#manageEntityAuditDeptGrid tbody').empty();
                $.each(data, function (i, v) {

                    $('#manageEntityAuditDeptGrid tbody').append('<tr><td>' + ++i + '</td><td>' + v.enT_ID + '</td><td>' + v.d_ID + '</td><td>' + v.d_CODE + '</td><td>' + v.cbaS_CODE + '</td><td>' + v.d_NAME + '</td><td>' + v.auD_ID + '</td><td>' + v.auditor + '</td><td>' + v.status + '</td> <td><a href="#" onclick="event.preventDefault();UpdateManageEntityAuditDept(' + v.r_ID + ');" class="text-danger">Update</a></td></tr>');

                });
            },
            dataType: "json",
        });
    }
    function reloadLocation() {
        $('#updateManageEntityAuditDepartment').modal('hide');
    
        getManageEntityAuditDeptt();
    }

    function addNewEntAuditDept(){
        g_rowId = 0;
        $('#updateManageEntityAuditDepartment').modal('show');
        $('#modalentityid').val('');
        $('#modaldepartmentid').val('');
        $('#modaldepartmentCode').val('');
        $('#modalcbasecode').val('');

        $('#modaldepartmentname').val('');
        $('#modalauditdepartmentid').val('');
        $('#modalauditor').val('Y');
        $('#modalstatus').val('Y');
    }

    function UpdateManageEntityAuditDept(rowId) {
        g_rowId = rowId;
        $('#updateManageEntityAuditDepartment').modal('show');


        $('#modalentityid').val('');
        $('#modaldepartmentid').val('');
        $('#modaldepartmentCode').val('');
        $('#modalcbasecode').val('Y');

        $('#modaldepartmentname').val('');
        $('#modalauditdepartmentid').val('');
        $('#modalauditor').val('');
        $('#modalstatus').val('Y');
     
        $.each(g_statusRecord, function (i, v) {
            if (v.r_ID == g_rowId) {           

                $('#modalentityid').val(v.enT_ID);
                $('#modaldepartmentid').val(v.d_ID);
                $('#modaldepartmentCode').val(v.d_CODE);
                $('#modalcbasecode').val(v.cbaS_CODE);
                $('#modaldepartmentname').val(v.d_NAME);
                $('#modalauditdepartmentid').val(v.auD_ID);
                $('#modalauditor').val(v.auditor);
                $('#modalstatus').val(v.status);
              

            }
        })

    }
    function saveChangesManageEntityAuditDepartment() {
        var manageEntityAuditDepartmentModel = {

            'R_ID': g_rowId,
            'ENT_ID': $('#modalentityid').val(),
            'D_ID': $('#modaldepartmentid').val(),
            'D_CODE': $('#modaldepartmentCode').val(),
            'CBAS_CODE': $('#modalcbasecode').val(),
            'D_NAME': $('#modaldepartmentname').val(),
            'AUD_ID': $('#modalauditdepartmentid').val(),
            'AUDITOR': $('#modalauditor').val(),
            'STATUS': $('#modalstatus').val(),

        }
        if (g_rowId == 0) {
            $.ajax({
                url: g_asiBaseURL + "/ApiCalls/add_manage_entities_audit_department",
                type: "POST",
                data: {
                    ENT_AUD_DEPT_MODEL: manageEntityAuditDepartmentModel
                },
                cache: false,
                success: function (data) {
                    showApiAlert(data);
                    onAlertCallback(reloadLocation);

                },
                dataType: "json",
            });
        } else {
            $.ajax({
                url: g_asiBaseURL + "/ApiCalls/update_manage_entities_audit_department",
                type: "POST",
                data: {
                    ENT_AUD_DEPT_MODEL: manageEntityAuditDepartmentModel
                },
                cache: false,
                success: function (data) {
                    showApiAlert(data);
                    onAlertCallback(reloadLocation);

                },
                dataType: "json",
            });
        }
      
       
    }
