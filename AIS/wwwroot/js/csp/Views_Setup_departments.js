    $(document).ready(function () {
        var g_deptId = 0;
        $('#sidebar_policy').hide();
        $("#searchTableRecord").on("keyup", function () {
            var value = $(this).val().toLowerCase();
            $("#listOfDepartment tbody tr").filter(function () {
                $(this).toggle($(this).text().toLowerCase().indexOf(value) > -1)
            });
        });
    });

    function newDeptSetup() {
        g_deptId = 0;
        $('#setupDeptModal').modal('show');
        $('#deptCodeModalField').val('');
        $('#deptNameModalField').val('');
        $('#deptdivNameModalField').val(0);
        $('#deptdivNameModalField').val(0);
    }

    function setupDepartment(code, name, div_name, status, id, div_id,audited_id) {
        g_deptId = id;
        $('#deptCodeModalField').val(code);
        $('#deptNameModalField').val(name);
        //audited_id

        $('#deptdivNameModalField').val(div_id);
        $('#deptAuditByNameModalField').val(audited_id);
        if (status == "Active")
            $('#deptActiveModalField').click();
        else if (status == "InActive")
            $('#deptInactiveModalField').click();

        $('#setupDeptModal').modal('show');
    }

    function publishDepartmentChanges() {

        var code = $('#deptCodeModalField').val();
        var name = $('#deptNameModalField').val();
        var div_id = $('#deptdivNameModalField').val();
        var auditby_id = $('#deptAuditByNameModalField option:selected').val();
        var div_name = $('#deptdivNameModalField option:selected').text();
        var status;
        var badge;
        if ($('#deptActiveModalField').is(':checked')) {
            status = 'Active';
            badge = 'text-bg-success ';
        }
        else {
            status = 'InActive';
            badge = 'text-bg-danger ';
        }
        if (auditby_id == 0) {
            alert("Select Audit By Department");
            return;
        }
       $('#setupDeptModal').modal('hide');
        $.ajax({
            url: g_asiBaseURL + "/Setup/department_add",
            type: "POST",
            data: {
                'id': g_deptId,
                'code': code,
                'name': name,
                'div_id': div_id,
                'AUDITED_BY_DEPID': auditby_id,
                'status': status
            },
            cache: false,
            success: function (data) {
                //console.log(data);
                window.location = window.location.pathname;
            },
            dataType: "json",
        });
    }
