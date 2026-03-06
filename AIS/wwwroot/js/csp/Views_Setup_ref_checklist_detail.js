    var g_procId = 0;
    var g_subProcId = 0;
    var g_procDetailId = 0;
    var g_procDetailList=[];
    $(document).ready(function () {
        ShowSubChecklist();
       
    });
    
    function openUpdateSubCheclist(id, sId) {
        g_procDetailId = id;
        g_subProcId = sId;
        $('#updateEntityModal').modal('show');      

        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_checklist_detail_comparison_by_Id_for_referredBack",
            type: "POST",
            data: {
                'CHECKLIST_DETAIL_ID': id
            },
            cache: false,
            success: function (data) {
                var tr = data[0];
                g_procId=tr.procesS_ID;
                $('#updatedChecklist').val(tr.procesS_ID);
                getSubChecklistOfProcess();
                $('#updatedSubChecklist').val(tr.neW_SUB_PROCESS);
                $('#updatedProcDetailHeading').val(tr.neW_PROCESS_DETAIL);
                $('#updatedViolationlist').val(tr.neW_VIOLATION);
                $('#updatedProcOwnerlist').val(tr.neW_FUNCTIONAL_RESP);
                $('#updatedRoleResplist').val(tr.neW_ROLE_RESP);
                $('#updatedRisklist').val(tr.neW_RISK);
                $('#updatedAnnexlist').val(tr.neW_ANNEXURE);

                $('#oldProcessLabel').val(tr.process);
                $('#oldViolationLabel').val(tr.violation);
                $('#oldProcessOwnerLabel').val(tr.functionaL_RESP);
                $('#oldRoleRespLabel').val(tr.rolE_RESP);
                $('#oldRiskLabel').val(tr.risk);
                $('#oldAnnexureLabel').val(tr.annexure);
                $('#oldProcessDetailLabel').val(tr.procesS_DETAIL);
                $('#oldSubProcessLabel').val(tr.suB_PROCESS);

            },
            dataType: "json",
        });
        
    }
    function updateSubProcess() {
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
          $('#auditeeEntitiesList tbody').empty();
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_ref_audit_checklist_detail",
            type: "POST",
            data: {

            },
            cache: false,
            success: function (data) {
                g_procDetailList = data;
                var sr = 0;
                $.each(data, function (i, v) {
                    sr++;
                    $('#auditeeEntitiesList tbody').append('<tr id="tr_' + v.s_ID + '"><td>' + sr + '</td><td>' + v.heading + '</td><td>' + v.comments + '</td><td style="cursor:pointer;"><a class="text-danger" data-click="event.preventDefault();openUpdateSubCheclist(' + v.s_ID + ', ' + v.id + ')">Update</a></td></tr>');

                });
            },
            dataType: "json",
        });
       

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
                'PROCESS_ID': g_procId
            },
            cache: false,
            success: function (data) {
                $('#updatedSubChecklist').append('<option value="0">--Select Sub-Checklist--</option>');
                $.each(data, function (i, v) {
                    $('#updatedSubChecklist').append('<option value="' + v.s_ID + '">' + v.heading + '</option>');
                });
                $('#updatedSubChecklist').val(g_subProcId); },
            dataType: "json",
        });

    }
