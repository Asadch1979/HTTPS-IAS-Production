    var g_procId = 0;
    var g_subProcId = 0;
    var g_procDetailId = 0;
    var g_procDetailList=[];
    $(document).ready(function () {
        $("#searchTableRecord").on("keyup", function () {
            var value = $(this).val().toLowerCase();
            $("#listOfDepartment tbody tr").filter(function () {
                $(this).toggle($(this).text().toLowerCase().indexOf(value) > -1)
            });
        });
    });
    
    function openUpdateSubCheclist(id, sId) {
        g_procDetailId = id;
        g_subProcId = sId;
        $('#updateEntityModal').modal('show');
        $.each(g_procDetailList, function (i, v) {
            if (v.s_ID == g_procDetailId) 
            {
                $('#updatedChecklist').val($('#checklistSelectField').val());
                getSubChecklistOfProcess();           
                $('#updatedProcDetailHeading').val(v.heading);
                $('#updatedViolationlist').val(v.v_ID);
                $('#updatedProcOwnerlist').val(v.procesS_OWNER_ID);
                $('#updatedRoleResplist').val(v.rolE_RESP_ID);
                $('#updatedRisklist').val(v.risK_ID);
                $('#updatedAnnexlist').val(v.anneX_ID);

                if ($('#updatedChecklist').val()!= 0)
                $('#oldProcessLabel').val($('#updatedChecklist option:selected').text());
                else
                    $('#oldProcessLabel').val('');

                if ($('#updatedViolationlist').val() != 0)
                    $('#oldViolationLabel').val($('#updatedViolationlist option:selected').text());
                    else 
                    $('#oldViolationLabel').val('');

                if ($('#updatedProcOwnerlist').val() != 0)
                   $('#oldProcessOwnerLabel').val($('#updatedProcOwnerlist option:selected').text());
                   else
                    $('#oldProcessOwnerLabel').val('');



                if ($('#updatedRoleResplist').val() != 0)
                    $('#oldRoleRespLabel').val($('#updatedRoleResplist option:selected').text());
                    else 
                    $('#oldRoleRespLabel').val('');


                if ($('#updatedRisklist').val() != 0)
                    $('#oldRiskLabel').val($('#updatedRisklist option:selected').text());
                    else 
                    $('#oldRiskLabel').val("");

                if ($('#updatedAnnexlist').val() != 0)
                    $('#oldAnnexureLabel').val($('#updatedAnnexlist option:selected').text());
                    else 
                    $('#oldAnnexureLabel').val("");
                
                    $('#oldProcessDetailLabel').val(v.heading);
            }
        });
        
    }
    function updateSubProcess() {

        if ($('#updatedChecklist').val()==0){
            alert("please select Process to proceed");
            return;
        }
        if ($('#updatedSubChecklist').val() == 0) {
            alert("please select Sub Process to Proceed");
            return;
        }
        if ($('#updatedProcDetailHeading').val() == "") {
            alert("please provide Process Detail Heading to Proceed");
            return;
        }

        if ($('#updatedViolationlist').val() == 0) {
            alert("please select Vilation to Proceed");
            return;
        }

        if ($('#updatedProcOwnerlist').val() == 0) {
            alert("please select Process Owner to Proceed");
            return;
        }


        if ($('#updatedRoleResplist').val() == 0) {
            alert("please select Role Responsible to Proceed");
            return;
        }


        if ($('#updatedRisklist').val() == 0) {
            alert("please select Risk to Proceed");
            return;
        }


        if ($('#updatedAnnexlist').val() == 0) {
            alert("please select Annexure to Proceed");
            return;
        }


        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/update_audit_checklist_detail",
            type: "POST",
            data: {
                'PROCESS_ID': $('#updatedChecklist').val(),
                'SUB_PROCESS_ID': $('#updatedSubChecklist').val(),
                'PROCESS_DETAIL_ID': g_procDetailId,
                'HEADING': $('#updatedProcDetailHeading').val(),
                'V_ID': $('#updatedViolationlist').val(),
                'CONTROL_ID': $('#updatedProcOwnerlist').val(),
                'ROLE_ID': $('#updatedRoleResplist').val(),
                'RISK_ID': $('#updatedRisklist').val(),
                'ANNEX_CODE': $('#updatedAnnexlist').val()
            },
            cache: false,
            success: function (data) {
               showApiAlert(data);
                ShowSubChecklist();
            },
            dataType: "json",
        });
      

    }
    function getAuditSubChecklist() {
        var processId = $('#checklistSelectField').val();
        if (processId != 0) {
            $('#subchecklistSelectField').empty();
            $.ajax({
                url: g_asiBaseURL + "/ApiCalls/get_audit_sub_checklist",
                type: "POST",
                data: {
                    'PROCESS_ID': processId
                },
                cache: false,
                success: function (data) {
                    var sr = 0;
                    $('#subchecklistSelectField').append('<option value="0">--Select Audit Sub-Checklist--</option>');
                    $.each(data, function (i, v) {
                        sr++;
                        $('#subchecklistSelectField').append('<option value="' + v.s_ID + '">'+v.heading+'</option>');

                    });
                },
                dataType: "json",
            });
        } else {
            g_entList = [];
            $('#subchecklistSelectField').empty();
            $('#subchecklistSelectField').append('<option value="0">--Select Audit Sub-Checklist--</option>');
        }

    }
    function getAuditSubChecklistForModal() {
        var processId = $('#newChecklist').val();
        if (processId != 0) {
            $('#newSubChecklist').empty();
            $.ajax({
                url: g_asiBaseURL + "/ApiCalls/get_audit_sub_checklist",
                type: "POST",
                data: {
                    'PROCESS_ID': processId
                },
                cache: false,
                success: function (data) {
                    $('#newSubChecklist').append('<option value="0">--Select Audit Sub-Checklist--</option>');
                    $.each(data, function (i, v) {
                        var className="";
                        if (v.s_ID == g_subProcId)
                            className = "selected=\"selected\"";

                        $('#newSubChecklist').append('<option ' + className + ' value="' + v.s_ID + '">' + v.heading + '</option>');

                    });
                },
                dataType: "json",
            });
        } else {
            g_entList = [];
            $('#newSubChecklist').empty();
            $('#newSubChecklist').append('<option value="0">--Select Audit Sub-Checklist--</option>');
        }

    }
    function ShowSubChecklist() {
        var processId = $('#checklistSelectField').val();
        var subProcessId = $('#subchecklistSelectField').val();
        $('#auditeeEntitiesList tbody').empty();
        if (processId != 0 && subProcessId !=0) {
            $.ajax({
                url: g_asiBaseURL + "/ApiCalls/get_audit_checklist_detail",
                type: "POST",
                data: {
                    'SUB_PROCESS_ID': subProcessId
                },
                cache: false,
                success: function (data) {
                    g_procDetailList = data;
                    var sr = 0;
                    $.each(data, function (i, v) {
                        sr++;
                        $('#auditeeEntitiesList tbody').append('<tr id="tr_' + v.s_ID + '"><td>' + sr + '</td><td>' + v.heading + '</td><td style="cursor:pointer;"><a class="text-danger" data-onclick="event.preventDefault();openUpdateSubCheclist(' + v.s_ID + ', ' + v.id + ')">Update</a></td></tr>');

                    });
                },
                dataType: "json",
            });
        } else {
            g_entList = [];
            $('#auditeeEntitiesList tbody').empty();
        }

    }
    function getSubChecklistForNewEntry() {
        var processId = $('#newChecklist').val();
        if (processId != 0) {
             $('#newSubChecklist').empty();
            $.ajax({
                url: g_asiBaseURL + "/ApiCalls/get_audit_sub_checklist",
                type: "POST",
                data: {
                    'PROCESS_ID': processId
                },
                cache: false,
                success: function (data) {
                    $('#newSubChecklist').append('<option value="0">--Select Sub-Checklist--</option>');
                    $.each(data, function (i, v) {
                        $('#newSubChecklist').append('<option value="' + v.s_ID + '">' + v.heading + '</option>');
                    });
                },
                dataType: "json",
            });
        } 

    }
    function getSubChecklistOfProcess() {
        $('#updatedSubChecklist').empty();
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_audit_sub_checklist",
            type: "POST",
            data: {
                'PROCESS_ID': $('#checklistSelectField').val()
            },
            cache: false,
            success: function (data) {
                $('#updatedSubChecklist').append('<option value="0">--Select Sub-Checklist--</option>');
                $.each(data, function (i, v) {
                    $('#updatedSubChecklist').append('<option value="' + v.s_ID + '">' + v.heading + '</option>');
                });
                $('#updatedSubChecklist').val(g_subProcId);
                $('#oldSubProcessLabel').val($('#updatedSubChecklist option:selected').text());

            },
            dataType: "json",
        });

    }
    function addNewCheckListDetail(){
        g_subProcId = 0;
        g_procDetailId = 0;
        $('#addEntityModal').modal('show');
        $('#newChecklist').val(0);
        $('#newSubChecklist').val(0);
        $('#newViolationlist').val(0);
        $('#newProcOwnerlist').val(0);
        $('#newRoleResplist').val(0);
        $('#newAnnexlist').val(0);
        $('#newRisklist').val(0);
        $('#newProcDetailHeading').val('');
     
    }
    function addProcessDetail(){

        if ($('#newChecklist').val() == 0) {
            alert("please select Process to proceed");
            return;
        }
        if ($('#newSubChecklist').val() == 0) {
            alert("please select Sub Process to Proceed");
            return;
        }
        if ($('#newProcDetailHeading').val() == "") {
            alert("please provide Process Detail Heading to Proceed");
            return;
        }

        if ($('#newViolationlist').val() == 0) {
            alert("please select Vilation to Proceed");
            return;
        }

        if ($('#newProcOwnerlist').val() == 0) {
            alert("please select Process Owner to Proceed");
            return;
        }


        if ($('#newRoleResplist').val() == 0) {
            alert("please select Role Responsible to Proceed");
            return;
        }


        if ($('#newRisklist').val() == 0) {
            alert("please select Risk to Proceed");
            return;
        }


        if ($('#newAnnexlist').val() == 0) {
            alert("please select Annexure to Proceed");
            return;
        }


        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/add_audit_checklist_detail",
            type: "POST",
            data: {
                'PROCESS_ID': $('#newChecklist').val(),
                'SUB_PROCESS_ID': $('#newSubChecklist').val(),
                'HEADING': $('#newProcDetailHeading').val(),
                'V_ID': $('#newViolationlist').val(),
                'CONTROL_ID': $('#newProcOwnerlist').val(),
                'ROLE_ID': $('#newRoleResplist').val(),
                'RISK_ID': $('#newRisklist').val(),
                'ANNEX_CODE': $('#newAnnexlist').val(),
            },
            cache: false,
            success: function (data) {
                showApiAlert(data);
                ShowSubChecklist();
              
            },
            dataType: "json",
        });
    }
