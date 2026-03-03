    var g_paraId = 0;
    var g_obsList = [];
    var g_procId = 0;
    var g_subprocId = 0;
    var g_procDetailId = 0;
    var g_paraRef = "";
    var g_event="";

    function normalizeRequiredInt(value) {
        var trimmed = $.trim(value);
        if (!trimmed) {
            return 0;
        }
        var number = parseInt(trimmed, 10);
        return Number.isNaN(number) ? 0 : number;
    }

    function normalizeNullableInt(value) {
        var trimmed = $.trim(value);
        if (!trimmed) {
            return null;
        }
        var number = parseInt(trimmed, 10);
        return Number.isNaN(number) ? null : number;
    }

    $(document).ready(function () {
        $('#responseAuditee').richText({
            imageUpload: false,
            fileUpload: false,
            videoEmbed: false,
            urls: false
        });

    });

    function AddRecord(e) {
        var respRow = $(e).closest("tr");
        if ($(respRow).find('input').eq(0).val() == '') {
            alert("Please enter responsible PP No to proceed");
            return;
        }
        var resP = [];
        var _error = false;
        $.each($(respRow), function (i, v) {

            if ($(v).find('td').eq(0).find('input').eq(0).val() != "" && ($(v).find('td').eq(2).find('input').eq(0).val() == "" || $(v).find('td').eq(3).find('input').eq(0).val() == "") && $(v).find('td').eq(4).find('input').eq(0).val() == "") {

                _error = true;
            }
            resP.push({
                'PP_NO': normalizeRequiredInt($(v).find('td').eq(0).find('input').eq(0).val()),
                'EMP_NAME': $(v).find('td').eq(1).find('input').eq(0).val(),
                'LOAN_CASE': normalizeNullableInt($(v).find('td').eq(2).find('input').eq(0).val()),
                'LC_AMOUNT': normalizeNullableInt($(v).find('td').eq(3).find('input').eq(0).val()),
                'ACCOUNT_NUMBER': normalizeRequiredInt($(v).find('td').eq(4).find('input').eq(0).val()),
                'ACC_AMOUNT': normalizeRequiredInt($(v).find('td').eq(5).find('input').eq(0).val())
            });
        });

        if (_error) {
            alert("Please enter either Loan Case and Loan Case Amount or Account Number against responsible");
            return false;
        }


        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/add_responsibility_to_legacy_para_fad",
            type: "POST",
            data: {
                'RESP_PP': resP[0],
                'REF_P': g_paraRef,
                'P_ID': g_paraId
            },
            cache: false,
            success: function (data) {
                showApiAlert(data);
                $($(e).closest("tr").find('.ibtnAdd').eq(0)).attr('disabled', true);
                $($(e).closest("tr").find('.ibtnAdd').eq(0)).val('Added');
                $($(e).closest("tr").find('.ibtnAdd').eq(0)).addClass('ibtnAdded');
                $($(e).closest("tr").find('.ibtnAdded').eq(0)).removeClass('ibtnAdd');
                $($(e).closest("tr").find('.ibtnDel').eq(0)).removeClass('d-none');
               

            },

            dataType: "json",
        });

    }

    function DeleteRecord(e) {

           
        var respRow = $(e).closest("tr");
        if ($($(e).closest("tr").find('.ibtnAdd').eq(0)).length > 0) {
            $(e).closest("tr").remove();
        } else {
            DeleteRecordFromDB(e);
        }

    }

    function DeleteRecordFromDB(e){
        g_event=e;
        confirmAlert("Do you really want to delete this record");
        onconfirmAlertCallback(DeleteConfirmAlertBox)
    }

    function DeleteConfirmAlertBox(){
       
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/delete_legacy_para_responsibility",
            type: "POST",
            data: {
                'PARA_REF': g_paraRef,
                'PARA_ID': g_paraId,
                'PP_NO': normalizeRequiredInt($($($(g_event).parent().parent()).find('td').eq(0).find('input').eq(0)).val()),
            },
            cache: false,
            success: function (data) {
                showApiAlert(data);
                $(g_event).closest("tr").remove();              
            },

            dataType: "json",
        });


    }
    function AddNewRespRecord(pp, emp_name, lc, lc_amount, acc, acc_amount, mode, checkClass) {
        var dis_attr=''
        if(mode=="disabled")
            dis_attr = 'disabled="disabled";'

        var newRow = $("<tr \"new_row\">");
        if (mode == "disabled")
        newRow = $("<tr class=\"disabled_row\">");
        var cols = "";
        var counter = $('#listofRespPersons tbody tr').length + 1;
        cols += '<td><input type="text" ' + dis_attr + ' onfocusout="getEmployeeName(this,' + counter + ');" class="form-control" id="ppNo' + counter + '" name="ppNo' + counter + '" value="' + pp + '" /></td>';
        cols += '<td><input type="text" ' + dis_attr + ' class="form-control" disabled="disabled" id="empName' + counter + '" value="' + emp_name + '" name="empName' + counter + '" value="' + emp_name + '"  /></td>';
        cols += '<td><input type="text" ' + dis_attr + ' class="form-control" name="loanCase' + counter + '" id="loanCase' + counter + '" value ="' + lc + '" /></td>';
        cols += '<td><input type="text" ' + dis_attr + ' class="form-control" name="lcAMount' + counter + '" id="lcAMount' + counter + '" value="' + lc_amount + '"  /></td>';
        cols += '<td><input type="text" ' + dis_attr + ' class="form-control" name="account' + counter + '"  id="account' + counter + '" value="' + acc + '" /></td>';
        cols += '<td><input type="text" ' + dis_attr + ' class="form-control" name="accountAmount' + counter + '" id="accountAmount' + counter + '" value="' + acc_amount + '" /></td>';
        if (checkClass == "checked")
            cols += '<td><input onclick="event.preventDefault();AddRecord(this);" disabled="disabled" type="button" class="ibtnAdded btn btn-md btn-success" value="Added"></td>';
        else
            cols += '<td><input onclick="event.preventDefault();AddRecord(this);" type="button" class="ibtnAdd btn btn-md btn-success" value="Add"></td>';

        if (mode == "disabled")
            cols += '<td><input onclick="event.preventDefault();DeleteRecordFromDB(this);" type="button" class="ibtnDel btn btn-md btn-danger " value="Delete"></td>';
        else
            cols += '<td><input onclick="event.preventDefault();DeleteRecord(this);" type="button" class="ibtnDel btn btn-md btn-danger " value="Delete"></td>';
        newRow.append(cols);
        $('#listofRespPersons tbody').append(newRow);
    }

    function calculateRow(row) {
        var price = +row.find('input[name^="price"]').val();

    }

    function calculateGrandTotal() {
        var grandTotal = 0;
        $("table.order-list").find('input[name^="price"]').each(function () {
            grandTotal += +$(this).val();
        });
        $("#grandtotal").text(grandTotal.toFixed(2));
    }
    function getLegacyPara() {
        $('#process_detail').modal('hide');
        $('#manageObsPanel tbody').empty();
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_legacy_paras_for_update_FAD",
            type: "POST",
            data: {
                'ENTITY_ID': $('#entitySelectField').val()
            },
            cache: false,
            success: function (data) {
                g_obsList = data;
                $('#entityNameField').html(data.length > 0 ? data[0].name : '');
                $.each(data, function (index, child) {
                    $('#manageObsPanel tbody').append('<tr id="div_' + child.id + '"><td><p class="fw-normal mb-1">' + child.audiT_PERIOD + '</p></td><td><p class="fw-normal mb-1">' + child.parA_NO + '</p></td><td><p class="fw-normal mb-1">' + child.gisT_OF_PARAS + '</p></td><td><p  class="fw-normal mb-1">' + child.amounT_INVOLVED + '</p></td><td><p  class="fw-normal mb-1">' + child.voL_I_II + '</p></td><td class="text-center"><a href="#" onclick="event.preventDefault();updateParaDetails(\'' + child.reF_P + '\', \'' + child.parA_NO + '\', \'' + child.id + '\' );" class="text-hover text-danger mr-5px"><small>Update Observation</small></a></td></tr>')
                });
            },

            dataType: "json",
        });

    }
    function updateParaDetails(ref_p, memo_no, paraId) {
        

        g_paraId = paraId;
        g_paraRef = ref_p;
        $('#process_detail').modal('show');
        $('#listofRespPersons tbody').empty();

        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_legacy_paras_for_update_FAD",
            type: "POST",
            data: {
                'ENTITY_ID': $('#entitySelectField').val(),
                'PARA_REF': ref_p,
                'PARA_ID': paraId
            },
            cache: false,
            success: function (data) {

                if (data.length > 0) {
                    var v = data[0];
                    g_procId = v.process;
                    g_subprocId = v.suB_PROCESS;
                    g_procDetailId = v.procesS_DETAIL;

                    $('#updateMemo_process').val(v.process);
                    getSubProcessList();
                    $('#observation').html(v.gisT_OF_PARAS);
                    $('#responseAuditee').val(v.parA_TEXT).trigger('change');
                    $('#auditCriteriaRiskField').val(v.risK_ID);                  

                    $.each(v.parA_RESP, function (i, res) {

                        AddNewRespRecord(res.pP_NO, res.emP_NAME, res.loaN_CASE, res.lC_AMOUNT, res.accounT_NUMBER, res.acC_AMOUNT,'disabled','checked')

                    });
                }
            },

            dataType: "json",
        });


    }

    function getSubProcessList() {
        if ($('#updateMemo_process option:selected').val() == 0) {
            $('#updateMemo_subprocess').empty();
            $('#updateMemo_violation').empty();
        }
        else {

            $('#updateMemo_subprocess').empty();
            $('#updateMemo_violation').empty();
            $.ajax({
                url: g_asiBaseURL + "/ApiCalls/sub_checklist",
                type: "POST",
                data: {
                    'T_ID': $('#updateMemo_process option:selected').val(),
                },
                cache: false,
                success: function (data) {
                    $('#updateMemo_subprocess').append("<option value=\"0\" id=\"0\">--Select Sub Group--</option>");
                    $.each(data, function (index, item) {
                        var className = "";
                        if (g_subprocId == item.s_ID) {
                            className = 'selected = "selected"';
                        }
                        $('#updateMemo_subprocess').append("<option " + className + " value=\"" + item.s_ID + "\"> " + item.heading + " </option > ");
                    });
                    if (g_subprocId != 0) {
                        getSubProcessViolationList();
                    }
                },
                dataType: "json",
            });
        }
    }
    function getSubProcessViolationList() {
        if ($('#updateMemo_subprocess option:selected').val() == 0)
            $('#updateMemo_violation').empty();
        else {
            $('#updateMemo_violation').empty();
            $.ajax({
                url: g_asiBaseURL + "/ApiCalls/checklist_details",
                type: "POST",
                data: {
                    'S_ID': $('#updateMemo_subprocess option:selected').val(),
                },
                cache: false,
                timeout: 300000,
                success: function (data) {
                    $('#updateMemo_violation').append("<option value=\"0\" id=\"0\">--Select Sub Group--</option>");
                    $.each(data, function (index, item) {

                        var className = "";
                        if (g_procDetailId == item.id) {
                            className = 'selected = "selected"';
                        }

                        $('#updateMemo_violation').append("<option " + className + " value=\"" + item.id + "\"> " + item.heading + "</option>");
                    });
                },
                error: function (xhr, textStatus) {
                    if (textStatus === "timeout") {
                        alert('Request taking longer than usual, please wait or refine search.');
                        return;
                    }
                    alert('Request failed. Please try again.');
                },
                dataType: "json",
            });
        }


    }

    function getEmployeeName(e, counter) {
        if (e.value != "" && e.value.length > 4 && e.value != "000000") {
            $.ajax({
                url: g_asiBaseURL + "/ApiCalls/get_employee_name_from_pp",
                type: "POST",
                data: {
                    'PP_NO': normalizeRequiredInt(e.value)
                },
                cache: false,
                success: function (data) {
                    console.log($(e));
                    $('#empName' + counter).val(data.name);
                },

                dataType: "json",
            });
        }
    }

    function reloadLocation() {
        getLegacyPara();
    }

    function submitLegacyParaUpdates() {

         if ($('#listofRespPersons tbody tr .ibtnAdd').length > 0) {
            alert("There are pending responsibilities, please add all pending responsibilities to proceed");
            return;
        } 

        if ($('#listofRespPersons tbody tr .ibtnAdded').length==0){
            confirmAlert("Do you confirm this Legacy Para has no responsibility");
            onconfirmAlertCallback(confirm_submitLegacyParaUpdates);
            return;
        }else{
            confirm_submitLegacyParaUpdates();
        } 
    }

    function confirm_submitLegacyParaUpdates(){
        $('#PublishParaText').attr('disabled', true);
        var resP = [];

        if ($('#updateMemo_process').val() == 0) {
            alert("Please select Process");
            $('#PublishParaText').attr('disabled', false);
            return;
        }


        if ($('#updateMemo_subprocess').val() == 0) {
            alert("Please select Sub Process");
            $('#PublishParaText').attr('disabled', false);
            return;
        }

        if ($('#updateMemo_violation').val() == 0) {
            alert("Please select Checklist Detail");
            $('#PublishParaText').attr('disabled', false);
            return;
        }
        if ($('#auditCriteriaRiskField').val() == 0) {
            alert("Please select Risk");
            $('#PublishParaText').attr('disabled', false);
            return;
        }


        var add_leg_data = {
            'PARA_TEXT': $('.richText-editor').html(),
            'ID': g_paraId,
            'PROCESS_ID': $('#updateMemo_process').val(),
            'SUB_PROCESS_ID': $('#updateMemo_subprocess').val(),
            'CHECKLIST_DETAIL_ID': $('#updateMemo_violation').val(),
            'RISK_ID': $('#auditCriteriaRiskField').val(),
            'REF_P': g_paraRef,
            'RESP_PP': resP
        };
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/update_legacy_para_with_responsibilities_FAD",
            type: "POST",
            data: add_leg_data,
            cache: false,
            success: function (data) {
                showApiAlert(data);
                $('#PublishParaText').attr('disabled', false);
                getLegacyPara();
            },

            dataType: "json",
        });

    }

    function submitLegacyParaUpdatesWithNoChanges() {

        if ($('#listofRespPersons tbody tr .ibtnAdd').length > 0) {
            alert("There are pending responsibilities, please add all pending responsibilities to proceed");
            return;
        }

        if ($('#listofRespPersons tbody tr .ibtnAdded').length == 0) {
            confirmAlert("Do you confirm this Legacy Para has no responsibility");
            onconfirmAlertCallback(confirm_submitLegacyParaUpdatesWithNoChanges);
            return;
        } else {
            confirm_submitLegacyParaUpdatesWithNoChanges();
        }
    }

    function confirm_submitLegacyParaUpdatesWithNoChanges() {

        $('#PublishParaTextWithNoChanges').attr('disabled', true);
        var resP = [];       

        if ($('#updateMemo_process').val() == 0) {
            alert("Please select Process");
            $('#PublishParaTextWithNoChanges').attr('disabled', false);
            return;
        }

        if ($('#updateMemo_subprocess').val() == 0) {
            alert("Please select Sub Process");
            $('#PublishParaTextWithNoChanges').attr('disabled', false);
            return;
        }

        if ($('#updateMemo_violation').val() == 0) {
            alert("Please select Checklist Detail");
            $('#PublishParaTextWithNoChanges').attr('disabled', false);
            return;
        }

        if ($('#auditCriteriaRiskField').val() == 0) {
            alert("Please select Risk");
            $('#PublishParaTextWithNoChanges').attr('disabled', false);
            return;
        }

        var add_leg_data = {
            'PARA_TEXT': $('.richText-editor').html(),
            'ID': g_paraId,
            'PROCESS_ID': $('#updateMemo_process').val(),
            'SUB_PROCESS_ID': $('#updateMemo_subprocess').val(),
            'CHECKLIST_DETAIL_ID': $('#updateMemo_violation').val(),
            'RISK_ID': $('#auditCriteriaRiskField').val(),
            'REF_P': g_paraRef,
            'RESP_PP': resP
        };
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/update_legacy_para_with_responsibilities_no_changes",
            type: "POST",
            data: add_leg_data,
            cache: false,
            success: function (data) {
                showApiAlert(data);
                getLegacyPara();
                $('#PublishParaTextWithNoChanges').attr('disabled', false);
            },

            dataType: "json",
        });

    }

    function referBackLegacyParaUpdates(){
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/refer_back_legacy_para_to_az",
            type: "POST",
            data: {
                'PARA_REF':g_paraRef,
                'PARA_ID':g_paraId
            },
            cache: false,
            success: function (data) {
                showApiAlert(data);
                getLegacyPara();
                $('#PublishParaTextWithNoChanges').attr('disabled', false);
            },

            dataType: "json",
        });

    }
