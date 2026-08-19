    var g_obsId = 0;
    var g_obsTextId = 0;
    var g_obsList = [];
    var g_imgFiles = null;
    var g_imgLoader = null;
    var g_entityIdSF =0;
    var g_allAttachedImages = [];
    var respSection = null;
    var g_allowedFormats = ["pdf", "jpg", "jpeg", "png", "doc", "docx", "jpg", "csv", "xls", "xlsx"]; // allowed file formats
    var g_allowLimit = '12'; // Maximum file size in MB

    function resetAuditeeObservationReference() {
        var $section = $('#auditeeObservationReferenceSection');
        if (!$section.length || typeof window.initObservationReference !== 'function') {
            return;
        }

        $section.find('#observationReferenceId').val('');
        window.initObservationReference('#auditeeObservationReferenceSection', {
            readOnly: true,
            forceReload: true,
            currentReferenceLabel: 'Saved Reference',
            emptyCurrentText: 'No reference attached with this observation.',
            initialRefId: null
        });
    }

    function initAuditeeObservationReference(referenceId) {
        var $section = $('#auditeeObservationReferenceSection');
        if (!$section.length || typeof window.initObservationReference !== 'function') {
            return;
        }

        $section.find('#observationReferenceId').val(referenceId || '');
        window.initObservationReference('#auditeeObservationReferenceSection', {
            readOnly: true,
            forceReload: true,
            currentReferenceLabel: 'Saved Reference',
            emptyCurrentText: 'No reference attached with this observation.',
            initialRefId: referenceId || null
        });
    }

    $(document).ready(function () {

        $('#viewMemo_reply').richText({
            imageUpload: false,
            fileUpload: false,
            videoEmbed: false,
            urls: false
        });

        respSection = initResponsibilitySection({
            tableSelector: '#viewMemo_respPP_ObSent',
            readOnly: true,
            status: 2,
            directSaveMode: false
        });

        $("aks-file-upload").aksFileUpload({
            fileUpload: "#aksfileupload", // With target [input]file or [type]json you can save the data of loaded items
            fileType: ["pdf", "zip", "jpg", "jpeg", "png", "doc", "docx", "csv", "xls", "xlsx"], // Allowed file formats
            dragDrop: false, // Enable drag & drop upload
            maxSize: g_allowLimit + " MB", // Maximum uploaded file size
            multiple: true, // Allow multiple file uploads
            maxFile: 100, // Maximum number of uploaded files
            maxFileError: "File exceeds upload limit. - Max limit:", // Error message for exceeding file count
            maxSizeError: "File exceeds size. - Max limit:", // Error message for exceeding file size
            fileTypeError: "Disallowed file format.", // Error message for disallowed file format
            label: "Select your files and wait until they appear below", // Label text for the file input


        });
        var fileInput = document.getElementById('aksfileupload');

         fileInput.addEventListener('change', function () {
            if (this.files.length > 0) {
                $("#wait").css("display", "block"); // Show loader
                processFiles(this.files).finally(() => {
                    $("#wait").css("display", "none"); // Hide loader
                });
            } else {
                $("#wait").css("display", "none"); // Hide loader if no files
            }
        });


        async function processFiles(files) {
            const formData = new FormData();
            formData.append('subfolder', g_entityIdSF);

            // Convert MB to bytes
            const maxSizeInBytes = parseInt(g_allowLimit) * 1024 * 1024;
            let hasFiles = false; // Flag to check if any valid files were added

            Array.from(files).forEach((file, index) => {
                if (file.size <= maxSizeInBytes) {
                    formData.append('files', file);
                    hasFiles = true; // Set flag to true if a file is valid
                }
            });

            // Call uploadFiles only if there are valid files
            if (hasFiles) {
                uploadFiles(formData);
            }
        }
        async function uploadFiles(formData) {
            try {
                const response = await $.ajax({
                    url: g_asiBaseURL + "/UploadFile/UploadAuditeeEvideces",
                    type: 'POST',
                    data: formData,
                    processData: false,
                    contentType: false
                });
                alert("Files uploaded successfully!");

                $(".aks-file-upload .aks-file-upload-delete").on("click", function (e) {
                    var filename = $(this).attr("data-delete");
                    deleteFileFromServer(filename);
                });

            } catch (error) {
                console.error("Error uploading files:", error);
                alert("Error uploading files. Please try again."); // Use custom alert
            }
        }

        $('#viewMemoModel').on('hidden.bs.modal', resetAuditeeObservationReference);
        resetAuditeeObservationReference();
        loadAllAssignedObservations();
    });

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
    function generateUniqueTimestamp() {
        var date = new Date();
        var year = date.getFullYear();
        var month = ('0' + (date.getMonth() + 1)).slice(-2);
        var day = ('0' + date.getDate()).slice(-2);
        var hours = ('0' + date.getHours()).slice(-2);
        var minutes = ('0' + date.getMinutes()).slice(-2);
        var seconds = ('0' + date.getSeconds()).slice(-2);
        var milliseconds = ('00' + date.getMilliseconds()).slice(-3);

        var timestamp = year + month + day + '_' + hours + minutes + seconds + milliseconds;
        return timestamp;
    }
    function deleteImageObjectByFileName(fileName) {
        const index = g_allAttachedImages.findIndex(image => image.FILE_NAME === fileName);
        if (index !== -1) {
            g_allAttachedImages.splice(index, 1);
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
    function deleteFileFromServer(fileName) {

        if (fileName != null || fileName != "" || typeof fileName != "undefined") {
            $.ajax({
                url: g_asiBaseURL + "/UploadFile/DeleteAuditeeEvidences",
                type: "POST",
                data: {
                    'fileName': fileName,
                    'subFolder': g_entityIdSF
                },
                cache: false,
                success: function (data) {


                },

                dataType: "json",
            });
        }




    }
    function appendAssignedObservationRow(v) {
        var memoDate = (v.memO_DATE || '').split(' ')[0];
        var replyDate = (v.memO_REPLY_DATE || '').split(' ')[0];
        var actionText = 'View';

        if (v.caN_REPLY == 2) {
            actionText = 'Update Reply';
        } else if (v.caN_REPLY == 1 || v.editable == 1) {
            actionText = 'Reply';
        }

        $('#manageObsPanel tbody').append(
            '<tr id="assignedObRow_' + v.obS_ID + '">' +
            '<td>' + (v.memO_NUMBER || '') + '</td>' +
            '<td>' + (v.audiT_YEAR || '') + '</td>' +
            '<td>' + (v.entitY_NAME || '') + '</td>' +
            '<td class="text-center">' + memoDate + '</td>' +
            '<td class="text-center">' + (v.gist || '') + '</td>' +
            '<td class="text-center">' + replyDate + '</td>' +
            '<td class="text-center">' + (v.status || '') + '</td>' +
            '<td class="text-center"><a href="#" data-onclick="event.preventDefault();showMemo(' +
            v.obS_ID + ',' + v.obS_TEXT_ID + ',' + v.responsE_ID +
            ');" class="text-hover font-weight-bold text-success">' + actionText + '</a></td>' +
            '</tr>');
    }

    function loadAllAssignedObservations() {
        $('#manageObsPanel tbody').empty();
        g_obsList = [];

        var engagementIds = $('#entitySelectField option').map(function () {
            return parseInt($(this).val() || 0, 10);
        }).get().filter(function (engId, index, values) {
            return engId > 0 && values.indexOf(engId) === index;
        });

        if (engagementIds.length === 0) {
            $('#manageObsPanel tbody').append('<tr><td colspan="8" class="text-center text-muted">No assigned audit entity was found.</td></tr>');
            return;
        }

        var requests = engagementIds.map(function (engId) {
            return $.ajax({
                url: g_asiBaseURL + "/ApiCalls/get_assigned_observation",
                type: "POST",
                data: {
                    'ENG_ID': engId
                },
                cache: false,
                success: function (data) {
                    $.each(data || [], function (i, v) {
                        v.assignedEngId = engId;
                        g_obsList.push(v);
                    });
                },
                dataType: "json"
            });
        });

        $.when.apply($, requests).always(function () {
            $('#manageObsPanel tbody').empty();
            if (g_obsList.length === 0) {
                $('#manageObsPanel tbody').append('<tr><td colspan="8" class="text-center text-muted">No assigned observation was found.</td></tr>');
                return;
            }

            $.each(g_obsList, function (i, v) {
                appendAssignedObservationRow(v);
            });

            if (g_obsId != 0) {
                var rowpos = $('#assignedObRow_' + g_obsId).position();
                if (rowpos) {
                    $('html').scrollTop(rowpos.top);
                }
            }
        });
    }

    function getEntityObservation() {
        loadAllAssignedObservations();
    }

    function reloadLocation() {
        document.getElementById('aksfileupload').value = '';
        loadAllAssignedObservations();
    }
    function showMemo(obs_id, obs_text_id, resp_id) {

        var memo_number, canReply, gist, editable;

        g_obsId = obs_id;
        g_obsTextId = obs_text_id;
        g_respId = resp_id;
        $.each(g_obsList, function (i, v) {
            if (v.obS_ID == obs_id) {
                g_entityIdSF = v.assignedEngId || 0;
                gist = v.gist;
                memo_number = v.memO_NUMBER;
                canReply = v.caN_REPLY;
                editable = v.editable;
            }
        });

        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_observation_text",
            type: "POST",
            data: {
                'OBS_ID': g_obsId,
                'RESP_ID': g_respId
            },
            cache: false,
            success: function (data) {

                $('#viewMemoModel').modal('show');
                $('.aks-file-upload-delete').click();
                $('.aks-file-upload-error').remove();
                document.getElementById('aksfileupload').value = '';

                $('#viewMemo_memo').html(data[0]);
                $('#viewMemo_memoNumber').val(memo_number);
                $('#viewMemo_memoGist').val(gist);
                initAuditeeObservationReference(data.length > 3 ? data[3] : null);
                if (canReply == 2) {

                    $('#replyButton_memoReply').removeClass('d-none');
                    $('#viewMemo_responded').parent().addClass('d-none');
                    $('#replyrichTextWrapper').removeClass('d-none');
                    $('#viewMemo_reply').val(data[1]).trigger('change');
                    $('#evidenceViewer').removeClass('d-none');
                    $('#evidenceUploader').removeClass('d-none');
                }
                else if (canReply == 1) {
                    $('#replyButton_memoReply').removeClass('d-none');
                    $('#viewMemo_responded').parent().addClass('d-none');
                    $('#replyrichTextWrapper').removeClass('d-none');
                    $('#viewMemo_reply').val(data[1]).trigger('change');
                    $('#evidenceViewer').addClass('d-none');
                    $('#evidenceUploader').removeClass('d-none');
                }
                else {
                    $('#replyButton_memoReply').addClass('d-none');
                    $('#viewMemo_responded').parent().removeClass('d-none');
                    $('#replyrichTextWrapper').addClass('d-none');
                    $('#viewMemo_responded').html(data[1]);
                    $('#evidenceViewer').removeClass('d-none');
                    $('#evidenceUploader').addClass('d-none');

                }
                var auctionImages = [];
                $('#complianceCycleEvidences').empty();
                $('#complianceCycleEvidences').removeClass('d-none');
                if (data[2].length == 0 && editable > 0 && canReply > 0) {

                    $('#complianceCycleEvidences').append("<i>No evidence is attached </i>");
                }
                $.each(data[2], function (i, pp) {

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
                respSection.updateContext({ newParaId: g_obsId });
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
    function replyMemo() {
        g_allAttachedImages = [];
        var replyTxt = ($('#viewMemo_reply').val()).length;
        if (replyTxt > 0) {
            $.ajax({
                url: g_asiBaseURL + "/ApiCalls/reply_observation",
                type: "POST",
                data: {
                    'AU_OBS_ID': g_obsId,
                    'OBS_TEXT_ID': g_obsTextId,
                    'REPLY': $('#viewMemo_reply').val(),
                    'EVIDENCE_LIST': g_allAttachedImages,
                    'SUBFOLDER': g_entityIdSF
                },
                cache: false,
                success: function (data) {
                    alert("Reply sent successfuly");
                    onAlertCallback(reloadLocation);

                },
                dataType: "json",
            });
        } else {
            alert("Provide your comments to proceed");
            return false;
        }

    }

    function clearEvidencesLog() {
        $('.aks-file-upload-delete').click();
        $('.aks-file-upload-error').remove();
        document.getElementById('aksfileupload').value = '';
    }
