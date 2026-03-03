    var g_procId = 0;
    var g_subProcId = 0;
    var g_procDetailId = 0;
    var g_subProcList = [];    
    var g_procDetailList = [];
    $(document).ready(function () {
        $("#searchTableRecord").on("keyup", function () {
            var value = $(this).val().toLowerCase();
            $("#checkListDetailsPanel tbody tr").filter(function () {
                if ($(this).text().toLowerCase().indexOf(value) > -1) {
                    $(this).addClass('matched');
                } else {
                    $(this).removeClass('matched');
                    $(this).find('.chkbox').eq(0).attr('checked', false);
                }
                $(this).toggle($(this).text().toLowerCase().indexOf(value) > -1)
                
            });
        });

        $('#selectAllHeaderCheckbox').on('click',function(){
            if ($('#selectAllHeaderCheckbox').is(':checked')){
                
                $.each($('#checkListDetailsPanel tbody tr.matched'), function (i, tr) {
                    $(tr).find('.chkbox').eq(0).prop('checked',true)
                });
            }else{
                $.each($('#checkListDetailsPanel tbody tr.matched'), function (i, tr) {
                    $(tr).find('.chkbox').eq(0).prop('checked', false);

                });
            }
        })
    });

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
                    g_subProcList = data;
                    $('#subchecklistSelectField').append('<option value="0">--Select Audit Sub-Checklist--</option>');
                    $.each(data, function (i, v) {
                       $('#subchecklistSelectField').append('<option value="' + v.s_ID + '">' + v.heading + '</option>');
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
    function getAuditChecklistDetail() {
        var processId = $('#checklistSelectField').val();
        var subProcessId = $('#subchecklistSelectField').val();
        $('#checklistDetailSelectField').empty();
        $('#checklistDetailSelectField').append('<option value="0">--Select Checklist Detail--</option>');
        if (processId != 0 && subProcessId != 0) {
            $.ajax({
                url: g_asiBaseURL + "/ApiCalls/get_audit_checklist_detail_for_remove_duplicate",
                type: "POST",
                data: {
                    'SUB_PROCESS_ID': subProcessId
                },
                cache: false,
                success: function (data) {
                    g_procDetailList = data;
                    $.each(data, function (i, v) {
                        $('#checklistDetailSelectField').append('<option value="' + v.s_ID + '">' + v.heading + '</option>');

                    });
                },
                dataType: "json",
            });
        }
    }

    function openUpdateProcess(id, text) {
        g_ProcId = $('#checklistSelectField').val();
        $('#updateProcessModal').modal('show');
        $('#processCodeField').val($('#checklistSelectField option:selected').text());

    }
    function updateProcess() {
        if ($('#processCodeField').val() == "") {
            alert("Please enter Checklist Title");
            return;
        }
       
            $.ajax({
                url: g_asiBaseURL + "/ApiCalls/update_audit_checklist",
                type: "POST",
                data: {
                'PROCESS_ID': g_ProcId,
                    'HEADING': $('#processCodeField').val(),
                    'ACTIVE': $('#entActiveField').val()
                },
                cache: false,
                success: function (data) {
                    showApiAlert(data);
                    $('#updateProcessModal').modal('hide');
                },
                dataType: "json",
            });
        

    }
    function openUpdateSubCheclist() {
        g_subProcId = $('#subchecklistSelectField').val();
        $('#updateSubProcessModal').modal('show');
        $('#subchecklist_DetailsList').empty();
        $.each(g_subProcList, function (i, v) {
            if (v.s_ID == g_subProcId) {
                g_procId = v.t_ID;
                $('#newChecklist').val(v.t_ID);
                $('#subprocessCodeField').val(v.heading);
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
                //g_procDetailList = data;
                var sr = 0;
                $.each(data, function (i, v) {
                    $('#subchecklist_DetailsList').append('<li>' + v.heading + '</li>');
                });
            },
            dataType: "json",
        });
    }

    function updateSubProcess() {
        if ($('#newChecklist').val() == 0) {
            alert("Please select Process");
            return;
        }
        if ($('#subprocessCodeField').val() == "") {
            alert("Please enter Sub Process Title");
            return;
        }
      
            $.ajax({
                url: g_asiBaseURL + "/ApiCalls/update_audit_sub_checklist",
                type: "POST",
                data: {
                    'OLD_PROCESS_ID': g_procId,
                    'PROCESS_ID': $('#newChecklist').val(),
                    'SUB_PROCESS_ID': g_subProcId,
                    'HEADING': $('#subprocessCodeField').val(),
                    'ENTITY_TYPE_ID': 6
                },
                cache: false,
                success: function (data) {
                    showApiAlert(data);
                },
                dataType: "json",
            });
        


    }

    function openUpdadateCheclist(id, sId) {
        g_procDetailId = $('#checklistDetailSelectField').val();
        g_subProcId = $('#subchecklistSelectField').val();
        $('#updateProcessDetailModal').modal('show');
        $.each(g_procDetailList, function (i, v) {
            if (v.s_ID == g_procDetailId) {
                $('#updatedChecklist').val($('#checklistSelectField').val());
                getSubChecklistOfProcess();
                $('#updatedProcDetailHeading').val(v.heading);
                $('#updatedViolationlist').val(v.v_ID);
                $('#updatedProcOwnerlist').val(v.procesS_OWNER_ID);
                $('#updatedRoleResplist').val(v.rolE_RESP_ID);
                $('#updatedRisklist').val(v.risK_ID);
                $('#updatedAnnexlist').val(v.anneX_ID);

                if ($('#updatedChecklist').val() != 0)
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
    function updateProcessDetail() {

        if ($('#updatedChecklist').val() == 0) {
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
                $('#updateProcessDetailModal').modal('hide');
            },
            dataType: "json",
        });


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

    function mergeDuplicate(){
        if ($('#checklistDetailSelectField').val()=="0"){
            alert("Select Checlist Detail to proceed");
            return;
        }

        var ids_arr = [];
        $('#checkListDetailsPanel tbody tr.matched').each(function (i, tr) {
            var $checkbox = $(tr).find('.chkbox').eq(0);
            if ($checkbox.is(':checked')) {
                ids_arr.push($checkbox.attr('id'));
            }
        });

        if(ids_arr.length>0){
            $.ajax({
                url: g_asiBaseURL + "/ApiCalls/merge_duplicate_checklists",
                type: "POST",
                data: {
                    'MAIN_CHECKLIST_ID': $('#checklistDetailSelectField').val(),
                    'MERGE_CHECKLIST_IDs': ids_arr
                },
                cache: false,
                success: function (data) {
                    showApiAlert(data);
                    onAlertCallback(reloadLocation);
                },
                dataType: "json",
            });
        }else{
            alert("Please select atleast one checklist detail to merge with above");
            return;
        }
       
    }

    function reloadLocation(){
        window.location.reload();
    }
