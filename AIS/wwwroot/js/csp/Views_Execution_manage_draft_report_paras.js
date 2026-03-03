    var g_obsId = 0;
    var g_newStatusId = 0;
    var g_riskId = 0;
    var g_obsList = [];
    $(document).ready(function () {
        $('#entitySelectField').select2();
        var entName = $('#manageObsPanel tbody .entity_name_field:first').text();
        $('#entityNameField').val(entName);
        var periodName = $('#manageObsPanel tbody .period_name_field:first').text();
        $('#auditPeriodNameField').val(periodName);


    });
    function reloadLocation() {
        getEntityObservation();
    }

    function ViewObservation(id) {
        $('#viewMemoModel').modal('show');
        $('#viewMemo_panel').html("");
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_observations_draft_text",
            type: "POST",
            data: {
                'OBS_ID': id
            },
            cache: false,
            success: function (data) {
                $('#viewMemo_panel').html(data[0].obS_TEXT);
            },
            dataType: "json",
        });
    }
    function ViewObservationResponse(id) {
        $('#viewMemoResponseModel').modal('show');
        $('#viewMemoRes_panel').html("");
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_observations_draft_auditee_reply",
            type: "POST",
            data: {
                'OBS_ID': id
            },
            cache: false,
            success: function (data) {

                $('#viewMemoRes_panel').html(data[0].obS_REPLY);
                ViewAuditeeAttachedEvidences(id);
            },
            dataType: "json",
        });
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
    function finalCommentsButtonSave() {
        var svpComments = "";
        svpComments = $('#commentAreaInCommentsBox').val();
        //
        if ($('#draftNoInCommentsBox').val() == "") {
            alert("Please enter Draft Para No to proceed");
            return;
        }

        if ($('#commentAreaInCommentsBox').val() == "") {
            alert("Please enter the Comments to proceed");
            return;
        }


        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/update_observation_status",
            type: "POST",
            data: {
                'OBS_ID': g_obsId,
                'NEW_STATUS_ID': g_newStatusId,
                'DRAFT_PARA_NO': $('#draftNoInCommentsBox').val(),
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
    function updateObservationStatus(obs_id, new_status_id, risk_id) {
        g_obsId = obs_id;
        g_newStatusId = new_status_id;
        g_riskId = risk_id;
        $('#commentsBox').modal('show');
        $('#commentAreaInCommentsBox').val('');

        if (g_newStatusId == 4) {
            $('#draftNoInCommentsBox').val(0);
            $('#draftNoInCommentsBox').attr("disabled", true);

        } else {
            $('#draftNoInCommentsBox').val('');
            $('#draftNoInCommentsBox').attr("disabled", false);
        }
    }
    function getEntityObservation() {
        $('#manageObsPanel tbody').empty();
        if ($('#entitySelectField option:selected').val() != 0) {
            $.ajax({
                url: g_asiBaseURL + "/ApiCalls/get_observations_draft",
                type: "POST",
                data: {
                    'ENG_ID': $('#entitySelectField option:selected').val()
                },
                cache: false,
                success: function (data) {
                    g_obsList = data;
                    var isbranch = false;
                    $.each(data, function (i, v) {
                        $('#auditPeriodNameField').val(v.period);
                        if (v.violation == null && v.nature == null) {
                            isbranch = true;
                        }
                        if (v.obS_STATUS_ID == 3 && v.obS_RISK_ID == 3)
                            $('#manageObsPanel tbody').append('<tr id="' + v.obS_ID + '"><td class="text-center">' + v.memO_NO + '</td><td class="text-center">' + v.drafT_PARA_NO + '</td><td>' + v.violation + '</td><td>' + v.nature + '</td><td class="text-center"><a onclick="event.preventDefault();ViewObservation(' + v.obS_ID + ');" href="#" class="text-primary">View Observation</a></td><td class="obs_reply"><a onclick="ViewObservationResponse(' + v.obS_ID + ');" href="#" class="text-primary">View Response</a></td><td>' + v.auD_REPLY + '</td><td>' + v.heaD_REPLY + '</td><td>' + v.obS_RISK + '</td><td>' + v.obS_STATUS + '</td><td class="d-none entity_name_field">' + v.entitY_NAME + '</td><td class="d-none period_name_field">' + v.period + '</td><td class="text-center"><a href="#" onclick="updateObservationStatus(' + v.obS_ID + ', 4,' + v.obS_RISK_ID + ');" class="text-hover text-danger mr-5px"><small>Resolved at Memo Level</small></a></td><td><a href="#" onclick="updateObservationStatus(' + v.obS_ID + ',5,' + v.obS_RISK_ID + ');" class="text-hover text-primary ml-5px"><small>Add to Draft Report</small></a></td></tr>');
                        else if (v.obS_STATUS_ID == 3 && v.obS_RISK_ID != 3)
                            $('#manageObsPanel tbody').append('<tr id="' + v.obS_ID + '"><td class="text-center">' + v.memO_NO + '</td><td class="text-center">' + v.drafT_PARA_NO + '</td><td>' + v.violation + '</td><td>' + v.nature + '</td><td class="text-center"><a onclick="event.preventDefault();ViewObservation(' + v.obS_ID + ');" href="#" class="text-primary">View Observation</a></td><td class="obs_reply"><a onclick="ViewObservationResponse(' + v.obS_ID + ');" href="#" class="text-primary">View Response</a></td><td>' + v.auD_REPLY + '</td><td>' + v.heaD_REPLY + '</td><td>' + v.obS_RISK + '</td><td>' + v.obS_STATUS + '</td><td class="d-none entity_name_field">' + v.entitY_NAME + '</td><td class="d-none period_name_field">' + v.period + '</td><td class="text-center">-</td><td class="text-center"><a href="#" onclick="updateObservationStatus(' + v.obS_ID + ',5,' + v.obS_RISK_ID + ');" class="text-hover text-primary ml-5px"><small>Add to Draft Report</small></a></td></tr>');

                        else
                            $('#manageObsPanel tbody').append('<tr id="' + v.obS_ID + '"><td class="text-center">' + v.memO_NO + '</td><td class="text-center">' + v.drafT_PARA_NO + '</td><td>' + v.violation + '</td><td>' + v.nature + '</td><td class="text-center"><a onclick="event.preventDefault();ViewObservation(' + v.obS_ID + ');" href="#" class="text-primary">View Observation</a></td><td class="obs_reply"><a onclick="ViewObservationResponse(' + v.obS_ID + ');" href="#" class="text-primary">View Response</a></td><td>' + v.auD_REPLY + '</td><td>' + v.heaD_REPLY + '</td><td>' + v.obS_RISK + '</td><td>' + v.obS_STATUS + '</td><td class="d-none entity_name_field">' + v.entitY_NAME + '</td><td class="d-none period_name_field">' + v.period + '</td><td class="text-center">-</td><td class="text-center">-</td></tr>');

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
