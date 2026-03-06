    var g_obsId = 0;
    var g_newStatusId = 0;
    var g_riskId = 0;
    var g_obsList = [];
     var g_procId = 0;
    var g_subProcId=0;
    $(document).ready(function () {
        $('#entitySelectField').select2();
        var entName = $('#manageObsPanel tbody .entity_name_field:first').text();
        $('#entityNameField').val(entName);
        var periodName = $('#manageObsPanel tbody .period_name_field:first').text();
        $('#auditPeriodNameField').val(periodName);

           $('#viewMemo_memo_ObSent').richText({
            imageUpload: false,
            fileUpload: false,
            videoEmbed: false,
            urls: false
        });

    });

    function reloadLocation() {
        getEntityObservation();
    }


    function ViewAuditeeAttachedEvidences(id) {
        $('#viewMemoResponseModel').modal('show');
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_responded_obs_evidences",
            type: "POST",
            data: {
                'OBS_ID': id
            },
            cache: false,
            success: function (data) {

                $('#complianceCycleEvidences').empty();
                if (data.length > 0) {
                    $.each(data, function (i, pp) {

                        var extension = pp.imagE_NAME.split('.').pop().toLowerCase();
                        const contentType = getContentType(extension);

                        // Create and append the attachment item
                        const container = document.createElement('div');
                        container.className = 'evidence-link';

                        // Add icon
                        const icon = document.createElement('i');
                        icon.className = getIconClass(extension) + ' evidence-icon mr-1';
                        container.appendChild(icon);

                        // Add label
                        const label = document.createElement('span');
                        label.innerText = pp.imagE_NAME;
                        label.classList.add('text-primary');

                        // Add cursor style to make it look like a link
                        label.style.cursor = 'pointer';
                        container.appendChild(label);

                        // Add click event to download file on selection
                        container.addEventListener('click', function () {
                            downloadFile(pp.filE_ID);
                        });

                        $('#complianceCycleEvidences').append(container);
                    });
                }
                else {
                    $('#complianceCycleEvidences').append("<i>No evidence is attached </i>");
                }

            },
            dataType: "json",
        });
    }
    function getEntityObservation() {

        $('#entitySelectField').attr('disabled', 'disabled');
        $('#manageObsPanel tbody').empty();
        if ($('#entitySelectField option:selected').val() != 0) {
            $.ajax({
                url: g_asiBaseURL + "/ApiCalls/get_finalized_observations_draft",
                type: "POST",
                data: {
                    'ENG_ID': $('#entitySelectField option:selected').val()
                },
                cache: false,
                success: function (data) {
                    $('#entitySelectField').removeAttr('disabled');
                    g_obsList = data;

                    $.each(data, function (i, v) {
                        $('#auditPeriodNameField').val(v.period);
                        if (v.obS_STATUS_ID == 5)
                            $('#manageObsPanel tbody').append('<tr id="' + v.obS_ID + '"><td class="text-center">' + v.memO_NO + '</td><td class="text-center">' + v.drafT_PARA_NO + '</td><td class="text-center">' + v.finaL_PARA_NO + '</td><td class="branchfield">' + v.heading + '</td><td>' + v.obS_RISK + '</td><td>' + v.obS_STATUS + '</td><td><a href="#" data-onclick="viewObservationDetails(' + v.obS_ID + ', '+v.obS_STATUS_ID+');" class="text-hover text-success ml-5px"><small>View Details</small></a></td></tr></tr>');
                        else
                            $('#manageObsPanel tbody').append('<tr id="' + v.obS_ID + '"><td class="text-center">' + v.memO_NO + '</td><td class="text-center">' + v.drafT_PARA_NO + '</td><td class="text-center">' + v.finaL_PARA_NO + '</td><td class="branchfield">' + v.heading + '</td><td>' + v.obS_RISK + '</td><td>' + v.obS_STATUS + '</td><td>-</td></tr></tr>');

                    });


                },
                dataType: "json",
            });
            getReportSummary();
        }
    }

    function getViolationNatureOfProcess() {
        $('#viewMemo_subprocess_ObSent').empty();
        $.ajax({
           url: g_asiBaseURL + "/Execution/sub_voilation",
            type: "POST",
            data: {
                'V_ID': $('#viewMemo_process_ObSent').val()
            },
            cache: false,
            success: function (data) {
                $('#viewMemo_subprocess_ObSent').append('<option value="0">--Select Violation Nature--</option>');
                $.each(data, function (i, v) {
                    $('#viewMemo_subprocess_ObSent').append('<option value="' + v.id + '">' + v.suB_V_NAME + '</option>');
                });
                if(g_subProcId!=0)
                    {
                        $('#viewMemo_subprocess_ObSent').val(g_subProcId);
                    }


            },
            dataType: "json",
        });

    }

     function viewObservationDetails(obsId,status_id){
        g_obsId=obsId;
        $('#viewMemoDetailsModel').modal('show');


        if(status_id !=5){
            $('#update_audit_obs_button').addClass("d-none");
            $('#un_settle_audit_obs_button').addClass("d-none");
            $('#settle_audit_obs_button').addClass("d-none");

        }else{
            $('#update_audit_obs_button').removeClass("d-none");
            $('#un_settle_audit_obs_button').removeClass("d-none");
            $('#settle_audit_obs_button').removeClass("d-none");
        }

         $('#viewMemo_heading_ObSent').val('');
         $('#viewMemo_memo_ObSent').val('').trigger('change');
         $('#viewMemo_response_ObSent').html('');
         $('#viewMemo_aud_reply_ObSent').html('');
         $('#viewMemo_head_reply_ObSent').html('');
         $('#viewMemo_risk_ObSent').val(0);
         $('#viewMemo_process_ObSent').val(0);

          $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_obs_details_by_id_ho",
            type: "POST",
            data: {
                'OBS_ID': g_obsId
            },
            cache: false,
            success: function (data) {
         $('#viewMemo_heading_ObSent').val(data.heading);
         $('#viewMemo_memo_ObSent').val(data.observatioN_TEXT).trigger('change');
         $('#viewMemo_response_ObSent').html(data.auditeE_REPLY);
         ViewAuditeeAttachedEvidences();
         $('#viewMemo_aud_reply_ObSent').html(data.auditoR_RECOM);
         $('#viewMemo_risk_ObSent').val(data.riskmodeL_ID);
         $('#viewMemo_process_ObSent').val(data.procesS_ID);
         g_procId=data.procesS_ID;
         g_subProcId=data.subchecklisT_ID;
         getViolationNatureOfProcess();

               },
            dataType: "json",
        });
    }


    function getReportSummary() {
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/draft_report_summary",
            type: "POST",
            data:
            {
                'ENG_ID': $('#entitySelectField option:selected').val()
            },
            cache: false,
            success: function (data)
            {
                g_obsList = data;
                $('#totalObsLabel').text(data.total);
                $('#highObsLabel').text(data.high);
                $('#mediumObsLabel').text(data.medium);
                $('#lowObsLabel').text(data.low);
                $('#resolvedObsLabel').text(data.settled);
                $('#addToDraftLabel').text(data.addtoDraft);
            },
            dataType: "json",
        });
    }
    function finalCommentsButtonSave() {
        var svpComments = "";

        if ($('#finalNoInCommentsBox').val() == "") {
            alert("Please enter Final Para No to proceed");
            return;
        }



        svpComments = $('#commentAreaInCommentsBox').val();
        if ($('#commentAreaInCommentsBox').val() == "") {
            alert("Please enter the Comments First");
            return;
        }


        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/update_observation_status",
            type: "POST",
            data: {
                'OBS_ID': g_obsId,
                'NEW_STATUS_ID': g_newStatusId,
                'DRAFT_PARA_NO': $('#finalNoInCommentsBox').val(),
                'RISK_ID': g_riskId,
                'AUDITOR_COMMENT': svpComments
            },
            cache: false,
            success: function (data) {
                showApiAlert(data);
                onAlertCallback(reloadLocation);
                $('#commentsBox').modal('hide');
            },
            dataType: "json",
        });

    }
    function updateObservationStatus(obs_id, new_status_id, risk_id, audRem) {
        g_obsId = obs_id;
        g_newStatusId = new_status_id;
        g_riskId = risk_id;
        $('#commentsBox').modal('show');
        if (g_newStatusId == 9) {
            $('#finalNoInCommentsBox').val(0);
            $('#finalNoInCommentsBox').attr("disabled", true);

        }
        else {
            $('#finalNoInCommentsBox').val('');
            $('#finalNoInCommentsBox').attr("disabled", false);
        }

        if (g_newStatusId == 9) {
            $('#commentAreaInCommentsBox').val("Settled on the recommendation of Audit Team and compliance submitted by auditee");
        } else if (g_newStatusId == 8) {
            $('#commentAreaInCommentsBox').val(audRem);
        }

    }
    function downloadFile(id) {
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_auditee_evidence_data",
            type: "POST",
            data: {
                'FILE_ID': id,
            },
            cache: false,
            success: function (data) {
                var extension = data.imagE_NAME.split('.').pop().toLowerCase();
                const contentType = getContentType(extension);

                const blob = base64ToBlob(data.imagE_DATA, contentType);
                const link = document.createElement('a');
                link.href = URL.createObjectURL(blob);
                link.download = data.imagE_NAME;
                link.click(); // Trigger the download

            },
            dataType: "json",
        });


    }
    function getFileExtension(file) {
        var fileName = file.name;
        var extension = fileName.substring(fileName.lastIndexOf('.') + 1).toLowerCase();
        return extension;
    }
    function getIconClass(extension) {
        switch (extension) {
            case 'pdf': return 'fa fa-file-pdf';
            case 'zip': return 'fa fa-file-archive';
            case 'png':
            case 'jpg':
            case 'jpeg':
            case 'bmp': return 'fa fa-file-image';
            case 'doc':
            case 'docx': return 'fa fa-file-word';
            default: return 'fa fa-file';
        }
    }
    function getContentType(extension) {
        switch (extension) {
            case 'pdf': return 'application/pdf';
            case 'zip': return 'application/zip';
            case 'png': return 'image/png';
            case 'doc': return 'application/msword';
            case 'docx': return 'application/vnd.openxmlformats-officedocument.wordprocessingml.document';
            default: return 'application/octet-stream';
        }
    }
    function base64ToBlob(base64, contentType) {
        const byteCharacters = atob(base64);
        const byteArrays = [];

        for (let offset = 0; offset < byteCharacters.length; offset += 512) {
            const slice = byteCharacters.slice(offset, offset + 512);

            const byteNumbers = new Array(slice.length);
            for (let i = 0; i < slice.length; i++) {
                byteNumbers[i] = slice.charCodeAt(i);
            }

            const byteArray = new Uint8Array(byteNumbers);
            byteArrays.push(byteArray);
        }

        const blob = new Blob(byteArrays, { type: contentType });
        return blob;
    }
      function updateObservationDetails(obsId){

        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/update_audit_para_for_finalization_ho",
            type: "POST",
            data: {
                'OBS_ID': obsId,
                'ANNEX_ID': $('#viewMemo_annex_ObSent').val(),
                'VIOLATION_ID': $('#viewMemo_process_ObSent').val(),
                'VIOLATION_NATURE_ID': $('#viewMemo_subprocess_ObSent').val(),
                'RISK_ID': $('#viewMemo_risk_ObSent').val(),
                'GIST_OF_PARA': $('#viewMemo_heading_ObSent').val(),
                'TEXT_PARA': $('#viewMemo_memo_ObSent').val()

            },
            cache: false,
            success: function (data) {
                showApiAlert(data);
                 onAlertCallback(getEntityObservation);
            },
            dataType: "json",
        });
        return;
    }
