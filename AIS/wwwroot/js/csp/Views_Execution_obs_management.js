    var g_obsId = 0;
    var g_newStatusId = 0;
    var g_riskId = 0;
    var g_currentStatus = 0;
    $(document).ready(function () {
        $('#entitySelectField').select2();
        $('#manageObsPanel_parent').hide();
        var entName = $('#manageObsPanel tbody .entity_name_field:first').text();
        $('#entityNameField').val(entName);
        var periodName = $('#manageObsPanel tbody .period_name_field:first').text();
        $('#auditPeriodNameField').val(periodName);

        $('#updateMemoContent').richText({
            imageUpload: false,
            fileUpload: false,
            videoEmbed: false,
            urls: false
        });
    });
    function reloadLocation() {
        
        getEntityObservationSummary();
    }
    function getEntityObservationSummary(){
        $('#manageObsPanelSummary tbody').empty();
        $('#manageObsPanel tbody').empty();
        if ($('#entitySelectField option:selected').val() != 0) {
            $.ajax({
                url: g_asiBaseURL + "/ApiCalls/get_observations_summary_for_selected_entity",
                type: "POST",
                data: {
                    'ENG_ID': $('#entitySelectField option:selected').val()
                },
                cache: false,
                success: function (data) {
                    $.each(data, function (i, v) {
                    
                        $('#manageObsPanelSummary tbody').append(' <tr><td>' + v.e_NAME + '</td><td>' + v.status + '</td><td>' + v.team + '</td><td>' + v.created + '</td><td>' + v.submiT_TO_AUDITEE + '</td><td>' + v.respondeD_BY_AUDITEE + '</td><td>' + v.droP_RESOLVED_BY_TEAM_HEAD + '</td><td>' + v.addeD_TO_DRAFT + '</td><td>' + v.addeD_TO_FINAL + '</td><td>' + v.setteled + '</td><td>' + v.total + '</td></tr>');
                    });
                

                },
                dataType: "json",
            });

        }else{
            $('#manageObsPanel_parent').hide();
            $('#manageObsPanelSummary tbody').empty();
            $('#manageObsPanel tbody').empty();
        }
    }

    function getEntityObservation() {
        $('#manageObsPanel tbody').empty();
       
        if ($('#entitySelectField option:selected').val() != 0) {
            $.ajax({
                url: g_asiBaseURL + "/ApiCalls/get_observations_for_selected_entity",
                type: "POST",
                data: {
                    'ENG_ID': $('#entitySelectField option:selected').val()
                },
                cache: false,
                success: function (data) {
                    if(data.length>0){
                        $('#manageObsPanel_parent').show();
                    }else{
                        $('#manageObsPanel_parent').hide();
                    }
                    $.each(data, function (i, v) {

                        if (v.ind=="")
                            $('#manageObsPanel tbody').append(' <tr id="' + v.obS_ID + '"><td class="text-center">' + v.memo + '</td><td>' + v.drafT_PARA + '</td><td>' + v.finaL_PARA + '</td><td></td><td>' + v.title + '</td><td>' + v.status + '</td><td class="text-center"><a data-click="event.preventDefault();ObservationViewerPanel(' + v.obS_ID + ',' + v.statuS_ID + ', \'' + v.t_IND + '\')" href="#" class="text-hover">View Memo</a></td><td class="text-center"><a data-click="ObservationUpdatePanel(' + v.obS_ID + ')" href="#" class="text-hover">Edit Memo</a></td></tr>');
                        else
                            $('#manageObsPanel tbody').append(' <tr id="' + v.obS_ID + '"><td class="text-center">' + v.memo + '</td><td>' + v.drafT_PARA + '</td><td>' + v.finaL_PARA + '</td><td>' + v.e_NAME + '</td><td>' + v.title + '</td><td>' + v.status + '</td><td class="text-center"><a data-click="event.preventDefault();ObservationViewerPanel(' + v.obS_ID + ',' + v.statuS_ID + ', \'' + v.t_IND + '\')" href="#" class="text-hover">View Memo</a></td><td class="text-center"><a data-click="ObservationUpdatePanel(' + v.obS_ID + ')" href="#" class="text-hover">Edit Memo</a></td></tr>');
                    });
                    setTimeout(function () {
                        if (g_obsId != 0) {
                            var rowpos = $('#manageObsPanel tbody tr#' + g_obsId).position();
                            $('html').scrollTop(rowpos.top);
                        }
                    }, 200)


                },
                dataType: "json",
            });

        }
    }


    function ObservationViewerPanel(obs_id, status_id, indicator) {
        g_obsId = obs_id;
        g_currentStatus = status_id;
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_details_for_manage_observations_text",
            type: "POST",
            data: {
                'OBS_ID': obs_id,
                'INDICATOR': indicator
            },
            cache: false,
            success: function (data) {
                console.log(data);
                g_riskId = data[0].risk;
                if (indicator=="D"){
                    $('#viewMemo_checklistdetail_parent').hide();
                    $('#listofRespPersons_parent').hide();
                    $('#viewMemo_subprocess').html(data[0].cp);
                    $('#viewMemo_violation').html(data[0].psn);
                }else{
                    $('#viewMemo_checklistdetail_parent').show();
                    $('#listofRespPersons_parent').show();
                    $('#viewMemo_process').html(data[0].cp);
                    $('#viewMemo_subprocess').html(data[0].psn);
                    $('#viewMemo_violation').html(data[0].cd); 
                    $('#listofRespPersons tbody').empty();
                    if (data[0].responsiblE_PPs.length > 0) {
                        $.each(data[0].responsiblE_PPs, function (j, pp) {
                            var srNo = $('#listofRespPersons tbody tr').length;
                            srNo++;
                            $('#listofRespPersons tbody').append('<tr id="tr_' + pp.pP_NO + '"><td>' + srNo + '</td><td>' + pp.pP_NO + '</td><td>' + pp.emP_NAME + '</td><td>' + pp.loaN_CASE + '</td><td>' + pp.lC_AMOUNT + '</td><td>' + pp.accounT_NUMBER + '</td><td>' + pp.acC_AMOUNT + '</td></tr>');
                        });
                    }
                }
                $('#viewMemoModel').modal('show');
                $('#viewMemo_memo').html(data[0].text);
                $('#viewMemo_heading').html(data[0].title);
                $('#viewMemo_risk').val(data[0].risK_ID);
                $('#viewMemo_response').html(data[0].obS_REPLY);
              
                
                if (g_currentStatus == 1) {
                    $('#dropButton_memoReply').removeClass('d-none');
                    $('#submitAuditeeButton_memoReply').removeClass('d-none');

                } else if (g_currentStatus == 3) {
                    if (g_riskId == 3) {
                        $('#dropButton_memoReply').addClass('d-none');
                        $('#submitAuditeeButton_memoReply').addClass('d-none');

                    } else {
                        $('#dropButton_memoReply').addClass('d-none');
                        $('#submitAuditeeButton_memoReply').addClass('d-none');

                    }

                } else {
                    $('#dropButton_memoReply').addClass('d-none');
                    $('#submitAuditeeButton_memoReply').addClass('d-none');

                }

                

            },
            dataType: "json",
        });

    }
    function finalCommentsButtonSave() {
        if ($('#commentAreaInCommentsBox').val() == "") {
            alert("Auditor Comments are Mandatory");
            return;
        }
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/update_observation_status",
            type: "POST",
            data: {
                'OBS_ID': g_obsId,
                'NEW_STATUS_ID': g_newStatusId,
                'RISK_ID': g_riskId,
                'AUDITOR_COMMENT': $('#commentAreaInCommentsBox').val()
            },
            cache: false,
            success: function (data) {
                showApiAlert(data);
                onAlertCallback(reloadLocation);
            },
            dataType: "json",
        });
    }
    function updateObservationStatus(obs_id, new_status_id, risk_id) {
        g_obsId = obs_id;
        g_newStatusId = new_status_id;
        g_riskId = risk_id;
        $('#commentsBox').modal('show');
    }
    function dropObservation(obs_id, new_status_id, risk_id) {
        g_obsId = obs_id;
        g_newStatusId = new_status_id;
        g_riskId = risk_id;
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/drop_observation",
            type: "POST",
            data: {
                'OBS_ID': g_obsId,
                'NEW_STATUS_ID': g_newStatusId,
                'RISK_ID': g_riskId
            },
            cache: false,
            success: function (data) {
                showApiAlert(data);
                onAlertCallback(reloadLocation);               
            },
            dataType: "json",
        });
    }
    function submitObservationToAuditee(obs_id, new_status_id, risk_id) {
        $('#' + $('#auditeeEvidences').find('input[type="file"]').attr('id'))


        g_obsId = obs_id;
        g_newStatusId = new_status_id;
        g_riskId = risk_id;
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/submit_observation_to_auditee",
            type: "POST",
            data: {
                'OBS_ID': g_obsId,
                'NEW_STATUS_ID': g_newStatusId,
                'RISK_ID': g_riskId
            },
            cache: false,
            success: function (data) {
                showApiAlert(data);
                onAlertCallback(reloadLocation);
            },
            dataType: "json",
        });
    }

  
    function ObservationUpdatePanel(obs_id) {
        g_obsId = obs_id;
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_dept_observation_text",
            type: "POST",
            data: {
                'OBS_ID': obs_id
            },
            cache: false,
            success: function (data) {
                $('#updateMemoModel').modal('show');
                $('#updateMemoContent').val(data[0].obS_TEXT).trigger('change');
                $('#updateMemo_heading').val(data[0].heading);
                $('#updateMemo_risk').val(data[0].obS_RISK_ID);
            },
            dataType: "json",
        });

    }

    function finalUpdateMemoContent(obs_id) {
        g_obsId = obs_id;
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/update_observation_text",
            type: "POST",
            data: {
                'OBS_ID': g_obsId,
                'OBS_TITLE': $('#updateMemo_heading').val(),
                'RISK_ID': $('#updateMemo_risk').val(),
                'OBS_TEXT': $('.richText-editor').html()              
            },
            cache: false,
            success: function (data) {
                showApiAlert(data);
                onAlertCallback(reloadLocation);
            },
            dataType: "json",
        });

    }
