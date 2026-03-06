    var g_procId = 0;
    var g_subProcId = 0;
    var g_subProcList=[];
    $(document).ready(function () {
        $("#searchTableRecord").on("keyup", function () {
            var value = $(this).val().toLowerCase();
            $("#listOfDepartment tbody tr").filter(function () {
                $(this).toggle($(this).text().toLowerCase().indexOf(value) > -1)
            });
        });

        ShowSubChecklist();
    });

    function addNewSubCheckList(id) {
        g_subProcId = 0;
        g_procId = 0;
        $('#updateEntityModal').modal('show');
        $('#newChecklist').val(0);
        $('#entCodeField').val("");
        $('#entSeqField').val("");
        $('#entWeightField').val("");
        $('#checklistDetailsList').empty();
    }
    
    function openUpdateSubCheclist(id) {
        g_subProcId=id;
        $('#updateEntityModal').modal('show');
        $('#checklistDetailsList').empty();
        $.each(g_subProcList, function (i, v) {
            if (v.s_ID == g_subProcId) {
                g_procId=v.t_ID;
                $('#newChecklist').val(v.t_ID);
                $('#entCodeField').val(v.heading);
                $('#entSeqField').val(v.risK_SEQUENCE);
                $('#entWeightField').val(v.risK_WEIGHTAGE);
            }
        });

        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_checklist_details_for_sub_process",
            type: "POST",
            data: {
                'SUB_PROCESS_ID': g_subProcId
            },
            cache: false,
            success: function (data) {
                g_procDetailList = data;
                var sr = 0;
                $.each(data, function (i, v) {
                    $('#checklistDetailsList').append('<li>' + v.heading + '</li>');
                });
            },
            dataType: "json",
        });       
    }
   
    function reloadLocation() {
        window.location.reload();
    }

    function updateSubProcess() {
        if (document.querySelectorAll('input.alnum-only.is-invalid').length > 0) {
            Swal.fire({ icon: "error", title: "Validation error", text: "Please correct highlighted fields." });
            return;
        }
        if ($('#newChecklist').val() == 0) {
            alert("Please select Process");
            return;
        }
        if ($('#entCodeField').val() == "") {
            alert("Please enter Sub Process Title");
            return;
        }

        if ($('#entSeqField').val() == "") {
            alert("Please enter Risk Sequence");
            return;
        }

        if ($('#entWeightField').val() == "") {
            alert("Please enter Risk Weightage");
            return;
        }
        if (g_subProcId != 0)
        {
            $.ajax({
                url: g_asiBaseURL + "/ApiCalls/update_audit_sub_checklist",
                type: "POST",
                data: {
                    'OLD_PROCESS_ID': g_procId,
                    'PROCESS_ID': $('#newChecklist').val(),
                    'SUB_PROCESS_ID': g_subProcId,
                    'HEADING': $('#entCodeField').val(),
                    'RISK_SEQUENCE': $('#entSeqField').val(),
                    'RISK_WEIGHTAGE': $('#entWeightField').val(),
                    'ENTITY_TYPE_ID': 6
                },
                cache: false,
                success: function (data) {
                    showApiAlert(data);
                    ShowSubChecklist();
                },
                dataType: "json",
            });
        }else{

            $.ajax({
                url: g_asiBaseURL + "/ApiCalls/add_audit_sub_checklist",
                type: "POST",
                data: {
                    'PROCESS_ID': $('#newChecklist').val(),
                    'ENTITY_TYPE_ID': 6,
                    'HEADING': $('#entCodeField').val(),
                    'RISK_SEQUENCE': $('#entSeqField').val(),
                    'RISK_WEIGHTAGE': $('#entWeightField').val(),
                },
                cache: false,
                success: function (data) {
                    showApiAlert(data);
                    ShowSubChecklist();
                },
                dataType: "json",
            });
        }
     
      

    }
    function ShowSubChecklist() {
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_audit_sub_checklist",
            type: "POST",
            data: {
            },
            cache: false,
            success: function (data) {
                g_subProcList = data;
                var sr = 0;
                $('#auditeeEntitiesList tbody').empty();
                $.each(data, function (i, v) {
                    sr++;
                    $('#auditeeEntitiesList tbody').append('<tr><td>' + sr + '</td><td>' + v.process + '</td><td>' + v.heading + '</td><td>' + v.risK_SEQUENCE + '</td><td>' + v.risK_WEIGHTAGE + '</td><td>' + v.comments + '</td><td style="cursor:pointer;"><a class="text-danger" data-onclick="event.preventDefault();openUpdateSubCheclist(' + v.s_ID + ')">Update</a></td></tr>');

                });
            },
            dataType: "json",
        });
        

    }
