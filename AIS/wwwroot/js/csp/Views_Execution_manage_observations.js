    var g_obsId = 0;
    var g_newStatusId = 0;
    var g_riskId = 0;
    var g_currentStatus = 0;
    function getSelectedEngagementId() {
        var entityField = document.getElementById('entitySelectField');
        if (entityField && entityField.value) {
            return entityField.value;
        }

        var hiddenEngagement = document.querySelector('.ma-engagement-id');
        return hiddenEngagement && hiddenEngagement.value ? hiddenEngagement.value : 0;
    }

    function isCurrentEngagementTeamLead() {
        var teamLeadField = document.getElementById('maIsTeamLeadField');
        var value = teamLeadField && teamLeadField.value ? teamLeadField.value.toString().trim().toUpperCase() : '';
        return value === 'Y' || value === 'YES' || value === 'TRUE' || value === '1';
    }

    $(document).ready(function () {
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
        getEntityObservation();
    }

    function ObservationViewerPanel(obs_id, status_id, risk_id) {
        g_obsId = obs_id;
        g_riskId = risk_id;
        g_currentStatus = status_id;
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_dept_observation_text",
            type: "POST",
            data: {
                'OBS_ID': obs_id
            },
            cache: false,
            success: function (data) {
                console.log(data);
                $('#viewMemoModel').modal('show');
                $('#viewMemo_memo').html(data[0].obS_TEXT);
                $('#viewMemo_heading').html(data[0].heading);
                $('#viewMemo_risk').val(data[0].obS_RISK_ID);
                $('#viewMemo_response').html(data[0].obS_REPLY);
                $('#viewMemo_subprocess').html(data[0].nature);
                $('#viewMemo_violation').html(data[0].violation);
                $('#dropButton_memoReply').addClass('d-none');
                $('#submitAuditeeButton_memoReply').addClass('d-none');

                if (!isCurrentEngagementTeamLead()) {
                    $('#complianceCycleEvidences').empty();
                    if (data[0].attacheD_EVIDENCES.length>0){
                        $.each(data[0].attacheD_EVIDENCES, function (i, pp) {

                            var extension = pp.imagE_NAME.split('.').pop().toLowerCase();
                            const contentType = getContentType(extension);

                            const container = document.createElement('div');
                            container.className = 'evidence-link';

                            const icon = document.createElement('i');
                            icon.className = getIconClass(extension) + ' evidence-icon mr-1';
                            container.appendChild(icon);

                            const label = document.createElement('span');
                            label.innerText = pp.imagE_NAME;
                            label.classList.add('text-primary');
                            label.style.cursor = 'pointer';
                            container.appendChild(label);

                            container.addEventListener('click', function () {
                                downloadFile(pp.filE_ID);
                            });

                            $('#complianceCycleEvidences').append(container);
                        });
                    }
                    else{
                        $('#complianceCycleEvidences').append("<i>No evidence is attached </i>");
                    }

                    return;
                }

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
                $('#complianceCycleEvidences').empty();
                if (data[0].attacheD_EVIDENCES.length>0){
                    $.each(data[0].attacheD_EVIDENCES, function (i, pp) {

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
                else{
                    $('#complianceCycleEvidences').append("<i>No evidence is attached </i>");
                }
            },
            dataType: "json",
        });

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
        if (!isCurrentEngagementTeamLead()) {
            return;
        }

        g_obsId = obs_id;
        g_newStatusId = new_status_id;
        g_riskId = risk_id;
        $('#commentsBox').modal('show');
        $('#commentAreaInCommentsBox').val('');
    }
    function dropObservation(obs_id, new_status_id, risk_id) {
        if (!isCurrentEngagementTeamLead()) {
            return;
        }

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
        if (!isCurrentEngagementTeamLead()) {
            return;
        }

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

    function getEntityObservation() {
        $('#manageObsPanel tbody').empty();
        var selectedEngagementId = getSelectedEngagementId();
        if (selectedEngagementId && parseInt(selectedEngagementId, 10) !== 0) {
            $.ajax({
                url: g_asiBaseURL + "/ApiCalls/get_observations",
                type: "POST",
                data: {
                    'ENG_ID': selectedEngagementId
                },
                cache: false,
                success: function (data) {
                    var canManageObservation = isCurrentEngagementTeamLead();
                    $.each(data, function (i, v) {
                        $('#auditPeriodNameField').val(v.period);
                        var editAction = canManageObservation ? '<a data-onclick="ObservationUpdatePanel(' + v.obS_ID + ')" href="#" class="text-hover">Edit Memo</a>' : '';
                        $('#manageObsPanel tbody').append(' <tr id="' + v.obS_ID + '"><td class="text-center">' + v.memO_NO + '</td><td>' + v.heading + '</td><td>' + v.violation + '</td><td>' + v.obS_RISK + '</td><td>' + v.obS_STATUS + '</td><td class="text-center"><a data-onclick="event.preventDefault();ObservationViewerPanel(' + v.obS_ID + ',' + v.obS_STATUS_ID + ', ' + v.obS_RISK_ID + ')" href="#" class="text-hover">View Memo</a></td><td class="text-center">' + editAction + '</td></tr>');
                    });
                    setTimeout(function () {
                        if (g_obsId != 0) {
                            var rowpos = $('#manageObsPanel tbody tr#' + g_obsId).position();
                            $('html').scrollTop(rowpos.top);
                        }
                    },200)
                   

                },
                dataType: "json",
            });

        }
    }

    function ObservationUpdatePanel(obs_id) {
        if (!isCurrentEngagementTeamLead()) {
            return;
        }

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
                $('#updateMemo_instances').val(data[0].nO_OF_INSTANCES);
            },
            dataType: "json",
        });

    }

    function finalUpdateMemoContent(obs_id) {
        if (!isCurrentEngagementTeamLead()) {
            return;
        }

        g_obsId = obs_id;
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/update_observation_text",
            type: "POST",
            data: {
                'OBS_ID': g_obsId,
                'OBS_TITLE': $('#updateMemo_heading').val(),
                'RISK_ID': $('#updateMemo_risk').val(),
                'OBS_TEXT': $('.richText-editor').html(),
                'NO_INSTANCES': $('#updateMemo_instances').val()
            },
            cache: false,
            success: function (data) {
                showApiAlert(data);
                onAlertCallback(reloadLocation);
            },
            dataType: "json",
        });

    }
