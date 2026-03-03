    var g_procId = 0;
    var g_subProcId = 0;
    var g_procDetailId = 0;
    var g_subProcList = [];
    var g_procDetailList = [];
    $(document).ready(function () {
       
        $('#selectAllHeaderCheckbox').on('click', function () {
            if ($('#selectAllHeaderCheckbox').is(':checked')) {

                $.each($('#checkListDetailsPanel tbody tr.matched'), function (i, tr) {
                    $(tr).find('.chkbox').eq(0).prop('checked', true)
                });
            } else {
                $.each($('#checkListDetailsPanel tbody tr.matched'), function (i, tr) {
                    $(tr).find('.chkbox').eq(0).prop('checked', false);

                });
            }
        })
        getAuditSubChecklist();
    });

    function getAuditSubChecklist() {
        $('#subchecklistSelectField').empty();
        $('#checkListDetailsPanel tbody').empty();
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_audit_sub_checklist",
            type: "POST",
            data: {
                'PROCESS_ID': 0
            },
            cache: false,
            success: function (data) {
                g_subProcList = data;
                $('#subchecklistSelectField').append('<option value="0">--Select Audit Sub-Checklist--</option>');
                $.each(data, function (i, v) {
                    $('#subchecklistSelectField').append('<option value="' + v.s_ID + '">' + v.heading + '</option>');
                    $('#checkListDetailsPanel tbody').append('<tr class="matched"><td>' + ++i + '</td><td class="prIdCol" id="' + v.s_ID + '">' + v.heading + '</td><td>' + v.risK_SEQUENCE + '</td><td>' + v.risK_WEIGHTAGE + '</td><td class="text-center"><input id="' + v.s_ID + '" class="chkbox" type="checkbox" /></td></tr>');
                });
            },
            dataType: "json",
        });

    }
    
    function mergeDuplicate() {
        if ($('#checklistSelectField').val() == "0") {
            alert("Select Process and Sub Process to proceed");
            return;
        }

        if ($('#subchecklistSelectField').val() == "0") {
            alert("Select Sub Process to proceed");
            return;
        }

        var ids_arr = [];
        $('#checkListDetailsPanel tbody tr.matched').each(function (i, tr) {
            var $checkbox = $(tr).find('.chkbox').eq(0);
            if ($checkbox.is(':checked')) {
                ids_arr.push($checkbox.attr('id'));
            }
        });

        if (ids_arr.length > 0) {
            $.ajax({
                url: g_asiBaseURL + "/ApiCalls/merge_duplicate_sub_process",
                type: "POST",
                data: {
                    'MAIN_PROCESS_ID': $('#checklistSelectField').val(),
                    'MAIN_SUB_PROCESS_ID': $('#subchecklistSelectField').val(),
                    'MERGE_SUB_PROCESS_IDs': ids_arr
                },
                cache: false,
                success: function (data) {
                    showApiAlert(data);
                    onAlertCallback(reloadLocation);
                },
                dataType: "json",
            });
        } else {
            alert("Please select atleast one Process to merge with above selected process");
            return;
        }

    }

    function reloadLocation() {
        getAuditSubChecklist();
    }

    function removeSelectedProcFromGrid() {
        var selProc = $('#subchecklistSelectField').val();
        $('#checkListDetailsPanel tbody tr td.prIdCol').each(function (i, tr) {

            if ($(tr).attr('id') == selProc) {
                $(tr).parent().hide();
                $(tr).parent().find('.chkbox').eq(0).prop("checked", false);
                $(tr).parent().removeClass("matched");
            } else {
                $(tr).parent().show();
                $(tr).parent().addClass("matched");
            }

        });


    }
