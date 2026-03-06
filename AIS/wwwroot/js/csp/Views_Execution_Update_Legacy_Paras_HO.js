    var g_paraId = 0;
    var g_obsList = [];
    var g_procId = 0;
    var g_subprocId = 0;
    var g_procDetailId = 0;
    var g_entType = "";
    var g_paraRef = "";

    $(document).ready(function () {
        $('#responseAuditee').richText({
            imageUpload: false,
            fileUpload: false,
            videoEmbed: false,
            urls: false
        });

        $('#text_selectBox').richText({
            imageUpload: false,
            fileUpload: false,
            videoEmbed: false,
            urls: false
        });

        //
    });
    function getReportDropdownContents() {
        if ($('#entitySelectField').val() == 0) {
            $('#dropdownReportPanel').addClass('d-none');
        } else {
            $('#dropdownReportPanel').removeClass('d-none');
            $('#entityReportSelectField').empty();
            $('#entityReportSelectField').append('<option value="0">--Select Entity for Report</option>');
            $.ajax({
                url: g_asiBaseURL + "/ApiCalls/get_legacy_report_dropdown_contents",
                type: "POST",
                data: {
                    'ENTITY_ID': $('#entitySelectField').val()
                },
                cache: false,
                success: function (data) {
                    g_obsList = data;
                    $('#entityNameField').html(data.length > 0 ? data[0].name : '');
                    $.each(data, function (index, v) {
                        $('#entityReportSelectField').append('<option value="' + v.id + '"> ' + v.entitY_NAME + '</option>');
                    });
                },
                dataType: "json",
            });
        }

    }
    function getLegacyPara() {
        $('#process_detail').modal('hide');
        $('#manageObsPanel tbody').empty();
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_legacy_paras_for_update_ho",
            type: "POST",
            data: {
                'ENTITY_NAME': $('#entityReportSelectField option:selected').text()
            },
            cache: false,
            success: function (data) {
                g_obsList = data;
                $('#entityNameField').html(data.length > 0 ? data[0].name : '');

                $.each(data, function (index, child) {
                    child.parA_STATUS = child.parA_STATUS == 9 ? 'Settle' : 'Un-Settle';
                    $('#manageObsPanel tbody').append('<tr id="div_' + child.id + '"><td><p class="fw-normal mb-1">' + ++index + '</p></td><td><p class="fw-normal mb-1">' + child.audiT_PERIOD.split(' ')[0] + '</p></td><td><p class="fw-normal mb-1">' + child.parA_NO + '</p></td><td><p class="fw-normal mb-1">' + child.gisT_OF_PARAS + '</p></td><td><p  class="fw-normal mb-1">' + child.parA_STATUS + '</p></td><td class="text-center"><a href="#" data-click="event.preventDefault();updateParaDetails(\'' + child.reF_P + '\', \'' + child.parA_NO + '\', \'' + child.id + '\' );" class="text-hover text-primary mr-5px"><small>Update Observation</small></a></td><td class="text-center"><a href="#" data-click="updateParaStatusToSettle(\'' + child.reF_P + '\', \'' + child.parA_NO + '\', \'' + child.id + '\' );" class="text-hover text-success mr-5px"><small>Settle Observation</small></a></td><td class="text-center"><a href="#" data-click="submitDeletionofLegacyPara(\'' + child.reF_P + '\' );" class="text-hover text-danger mr-5px"><small>Delete Observation</small></a></td></tr>')
                });
            },

            dataType: "json",
        });

    }
    function getViolationNature() {
        $('#nature_selectBox').empty();
        $('#nature_selectBox').append('<option value="0">--Select Nature/Area--</option>');
        if ($('#violation_selectBox option:selected').val() != 0) {

            $.ajax({
                url: g_asiBaseURL + "/Execution/sub_voilation",
                type: "POST",
                data: {
                    'V_ID': $('#violation_selectBox option:selected').val(),
                },
                cache: false,
                success: function (data) {
                    $('#nature_selectBox').append("<option value=\"0\" id=\"0\">--Select Sub Group--</option>");
                    $.each(data, function (index, item) {
                        var className = "";
                        if (g_subprocId != 0 && g_subprocId == item.id) {
                            className = 'selected = "selected"';
                        }
                        $('#nature_selectBox').append("<option " + className + " value=\"" + item.id + "\"> " + item.suB_V_NAME + " </option> ");
                    });
                },
                dataType: "json",
            });
        }
    }
    function updateParaDetails(ref_p, memo_no, paraId) {
        g_paraId = paraId;
        g_paraRef = ref_p;
        $('#process_detail').modal('show');
        $.each(g_obsList, function (i, v) {
            if (v.reF_P == ref_p) {

                g_procId = v.process;
                g_subprocId = v.suB_PROCESS;
                g_procDetailId = v.procesS_DETAIL;
                g_entType = v.enT_TYPE;
                $('#violation_selectBox').val(v.process);
                $('#risk_selectBox').val(v.risK_ID);
                $('#responseAuditee').val(v.parA_TEXT).trigger('change');
                $('#text_selectBox').val(v.parA_TEXT).trigger('change');
                //
                getViolationNature();
            }
        });

    }

    function updateParaStatusToSettle(ref_p, memo_no, paraId) {
        g_paraRef = ref_p;
        $('#settleRemarksModel').modal('show');
        $('#Reason_settlement').val('');
    }
    function submitSettlementofLegacyPara() {
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/settle_legacy_para_HO",
            type: "POST",
            data: {
                'NEW_STATUS': $('#settleUnSettleDropdown').val(),
                'PARA_REF': g_paraRef,
                'SETTLEMENT_NOTES': $('#Reason_settlement').val()
            },
            cache: false,
            success: function (data) {

                showApiAlert(data);
                $('#settleRemarksModel').modal('hide');
                getLegacyPara();
            },

            dataType: "json",
        });

    }
    function submitDeletionofLegacyPara(paraRef) {
        g_paraRef = paraRef;
        confirmAlert("Do you confirm to delete this Legacy Para");
        onconfirmAlertCallback(confirmDeletionofLegacyPara);
    }
    function confirmDeletionofLegacyPara() {

        confirmAlert("Your PP Number will be recorded for this action, Do you confirm to delete this Legacy Para");
        onconfirmAlertCallback(confirm_submitLegacyParaUpdates);


        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/delete_legacy_para_HO",
            type: "POST",
            data: {
                'PARA_REF': g_paraRef
            },
            cache: false,
            success: function (data) {

                showApiAlert(data);
                $('#confirmModel').modal('hide');
                getLegacyPara();
            },

            dataType: "json",
        });

    }

    function reloadLocation() {
        getLegacyPara();
    }
    function submitLegacyParaUpdates() {

        if (g_entType == "B") {


            if ($('#listofRespPersons tbody tr .ibtnAdd').length > 0) {
                alert("There are pending responsibilities, please add all pending responsibilities to proceed");
                return;
            }

            if ($('#listofRespPersons tbody tr .ibtnAdded').length == 0) {
                confirmAlert("Do you confirm this Legacy Para has no responsibility");
                onconfirmAlertCallback(confirm_submitLegacyParaUpdates);
                return;
            } else {
                confirm_submitLegacyParaUpdates();
            }
        } else {
            confirm_submitLegacyParaUpdates();
        }

    }
    function confirm_submitLegacyParaUpdates() {
        var resP = [];
        $('#PublishParaText').attr('disabled', true);
        if (g_entType != "D") {
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

            if ($('#risk_selectBox').val() == 0) {
                alert("Please select Risk");
                $('#PublishParaText').attr('disabled', false);
                return;
            }
        }
        else {
            if ($('#violation_selectBox').val() == 0) {
                alert("Please select Violation");
                $('#PublishParaText').attr('disabled', false);
                return;
            }

            if ($('#nature_selectBox').val() == 0) {
                alert("Please select Nature");
                $('#PublishParaText').attr('disabled', false);
                return;
            }

            if ($('#risk_selectBox').val() == 0) {
                alert("Please select Risk");
                $('#PublishParaText').attr('disabled', false);
                return;
            }

        }

        var add_leg_data = {
            'PARA_TEXT': $('#text_selectBox').val(),
            'ID': g_paraId,
            'PROCESS_ID': g_entType == "B" ? $('#updateMemo_process').val() : $('#violation_selectBox').val(),
            'SUB_PROCESS_ID': g_entType == "B" ? $('#updateMemo_subprocess').val() : $('#nature_selectBox').val(),
            'CHECKLIST_DETAIL_ID': g_entType == "B" ? $('#updateMemo_violation').val() : 0,
            'RISK_ID': $('#risk_selectBox').val(),
            'REF_P': g_paraRef,
            'RESP_PP': resP
        };
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/update_legacy_para_with_responsibilities",
            type: "POST",
            data: add_leg_data,
            cache: false,
            success: function (data) {
                $('#PublishParaText').attr('disabled', false);
                showApiAlert(data);
                getLegacyPara();
            },

            dataType: "json",
        });
    }
    function submitLegacyParaUpdatesWithNoChanges() {
        if (g_entType == "B") {



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
        } else {
            confirm_submitLegacyParaUpdatesWithNoChanges();
        }

    }
    function confirm_submitLegacyParaUpdatesWithNoChanges() {

        var resP = [];

        if (g_entType != "D") {
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

            if ($('#risk_selectBox').val() == 0) {
                alert("Please select Risk");
                $('#PublishParaText').attr('disabled', false);
                return;
            }


        }
        else {
            if ($('#violation_selectBox').val() == 0) {
                alert("Please select Violation");
                $('#PublishParaText').attr('disabled', false);
                return;
            }

            if ($('#nature_selectBox').val() == 0) {
                alert("Please select Nature");
                $('#PublishParaText').attr('disabled', false);
                return;
            }

            if ($('#risk_selectBox').val() == 0) {
                alert("Please select Risk");
                $('#PublishParaText').attr('disabled', false);
                return;
            }

        }


        var add_leg_data = {
            'ID': g_paraId,
            'PROCESS_ID': g_entType == "B" ? $('#updateMemo_process').val() : $('#violation_selectBox').val(),
            'SUB_PROCESS_ID': g_entType == "B" ? $('#updateMemo_subprocess').val() : $('#nature_selectBox').val(),
            'CHECKLIST_DETAIL_ID': g_entType == "B" ? $('#updateMemo_violation').val() : 0,
            'RISK_ID': $('#risk_selectBox').val(),
            'REF_P': g_paraRef,
            'RESP_PP': resP
        };
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/update_legacy_para_with_responsibilities_no_changes_AZ",
            type: "POST",
            data: add_leg_data,
            cache: false,
            success: function (data) {
                showApiAlert(data);
                getLegacyPara();
            },

            dataType: "json",
        });

    }
