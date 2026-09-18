    var g_newParaId = 0;
    var g_comId = 0;
    var g_oldParaId = 0;
    var g_prevRole = "";
    var g_nextRole = "";
    var g_obsList = [];
    var fileInput = null;
    var g_allAttachedImages = [];
    var g_allowedFormats = ["pdf", "zip", "jpg", "jpeg", "png", "doc", "docx", "csv", "xls", "xlsx"]; // allowed file formats


    var btnClick = "own";
    var g_allowLimit = '10'; // Maximum file size in MB
    var g_maxUploadFiles = 100;
    var g_maxEvidenceBytes = 10 * 1024 * 1024;
    var g_totalEvidenceSizeError = "Total evidence size cannot exceed 10 MB. Please remove unnecessary files or compress your documents.";

    function getDisplayParaRisk(value) {
        var risk = (value || '').toString().trim().toLowerCase();
        if (risk === 'high')
            return 'High';
        if (risk === 'medium')
            return 'Medium';
        if (risk === 'low')
            return 'Low';
        return '-';
    }

    $(document).ready(function () {
        getOwnParasForCompliance();
        $('#viewMemo_compliance_sc').richText({
            imageUpload: false,
            fileUpload: false,
            videoEmbed: false,
            urls: false
        });

        $("aks-file-upload").aksFileUpload({
            fileUpload: "#aksfileupload", // With target [input]file or [type]json you can save the data of loaded items
            fileType: ["pdf", "zip", "jpg", "jpeg", "png", "doc", "docx", "csv", "xls", "xlsx"], // Allowed file formats
            dragDrop: false, // Enable drag & drop upload
            maxSize: g_allowLimit + " MB", // Maximum uploaded file size
            multiple: true, // Allow multiple file uploads
            maxFile: g_maxUploadFiles, // Maximum number of uploaded files
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
            formData.append('subfolder', g_comId);
            const maxSizeInBytes = g_maxEvidenceBytes;
            const selectedFiles = Array.from(files);
            const selectedFileNames = selectedFiles.map((file) => file.name);
            const acceptedFiles = [];
            const validationMessages = [];

            if (selectedFiles.length > g_maxUploadFiles) {
                validationMessages.push("A maximum of " + g_maxUploadFiles + " files can be uploaded at once.");
            }

            selectedFiles.forEach((file) => {
                const extension = getFileExtension(file);
                if (!g_allowedFormats.includes(extension)) {
                    validationMessages.push(file.name + ": disallowed file format.");
                    return;
                }

                if (file.size > maxSizeInBytes) {
                    validationMessages.push(g_totalEvidenceSizeError);
                    return;
                }

                acceptedFiles.push(file);
            });

            const aggregateSize = acceptedFiles.reduce((total, file) => total + file.size, 0);
            if (validationMessages.length > 0) {
                removeFailedPreviews(selectedFileNames);
                alert(validationMessages.join("\n"));
                return;
            }

            if (aggregateSize > g_maxEvidenceBytes) {
                removeFailedPreviews(selectedFileNames);
                alert(g_totalEvidenceSizeError);
                return;
            }

            const uploadStatus = await getEvidenceUploadStatus();
            if (!uploadStatus || uploadStatus.success !== true) {
                removeFailedPreviews(selectedFileNames);
                alert(uploadStatus && uploadStatus.message ? uploadStatus.message : "Unable to validate current evidence size.");
                return;
            }

            const existingSizeBytes = parseInt(uploadStatus.existingSizeBytes || 0, 10);
            const maxTotalBytes = parseInt(uploadStatus.maxTotalBytes || g_maxEvidenceBytes, 10);
            if (existingSizeBytes + aggregateSize > maxTotalBytes) {
                removeFailedPreviews(selectedFileNames);
                alert(g_totalEvidenceSizeError);
                return;
            }

            if (acceptedFiles.length > 0) {
                acceptedFiles.forEach((file) => formData.append('files', file));
                const uploaded = await uploadFiles(formData);
                if (!uploaded) {
                    removeFailedPreviews(selectedFileNames);
                }
            }
        }

        function removeFailedPreviews(fileNames) {
            $("aks-file-upload").trigger("aksFileUploadRemove", [fileNames]);
        }


        async function getEvidenceUploadStatus() {
            try {
                return await $.ajax({
                    url: g_asiBaseURL + "/UploadFile/GetComplianceEvidenceUploadStatus",
                    type: 'POST',
                    data: {
                        subfolder: g_comId
                    }
                });
            } catch (error) {
                console.error("Error validating evidence upload size:", error);
                const messages = getUploadErrorMessages(error);
                return messages.length > 0 ? { success: false, message: messages.join("\n") } : null;
            }
        }


        async function uploadFiles(formData) {
            try {
                const response = await $.ajax({
                    url: g_asiBaseURL + "/UploadFile/UploadFiles",
                    type: 'POST',
                    data: formData,
                    processData: false,
                    contentType: false
                });

                if (!response || response.success !== true) {
                    alert(response && response.message ? response.message : "Files could not be uploaded.");
                    return false;
                }

                alert(response.message || "Files uploaded successfully!");

                $(".aks-file-upload .aks-file-upload-delete").on("click", function (e) {
                    var filename = $(this).attr("data-delete");
                    deleteFileFromServer(filename);
                });

                return true;

            } catch (error) {
                console.error("Error uploading files:", error);
                const messages = getUploadErrorMessages(error);
                alert(messages.length > 0 ? messages.join("\n") : "Error uploading files. Please try again.");
                return false;
            }
        }

        function getUploadErrorMessages(error) {
            var response = error && error.responseJSON;
            if (!response && error && error.responseText) {
                try {
                    response = JSON.parse(error.responseText);
                } catch (parseError) {
                    var responseText = error.responseText.trim();
                    return responseText && responseText.indexOf('<') !== 0 ? [responseText] : [];
                }
            }

            var messages = [];
            if (response && response.message) {
                messages.push(response.message);
            }

            if (response && response.errors) {
                Object.keys(response.errors).forEach((field) => {
                    const fieldErrors = Array.isArray(response.errors[field])
                        ? response.errors[field]
                        : [response.errors[field]];
                    fieldErrors.forEach((message) => {
                        if (message) {
                            messages.push(message);
                        }
                    });
                });
            }

            return messages;
        }
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
                url: g_asiBaseURL + "/UploadFile/DeleteFile",
                type: "POST",
                data: {
                    'fileName': fileName,
                    'subFolder': g_comId
                },
                cache: false,
                success: function (data) {


                },

                dataType: "json",
            });
        }




    }


    function getOwnParasForCompliance() {

       
        destroyDatatable("manageObsPanel");
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_paras_for_compliance_by_auditee",
            type: "POST",
            data: {

            },
            cache: false,
            success: function (data) {
                g_obsList = data;
                $.each(data, function (index, child) {

                    $('#manageObsPanel tbody').append('<tr id="div_' + child.id + '"><td>' + ++index + '</td><td><p class="fw-normal mb-1">' + child.audiT_PERIOD + '</p></td><td><p class="fw-normal mb-1">' + child.parA_NO + '</p></td><td><p class="fw-normal mb-1">' + child.audiT_DATE + '</p></td><td><p class="fw-normal mb-1">' + getDisplayParaRisk(child.parA_RISK) + '</p></td><td><p class="fw-normal mb-1">' + child.gisT_OF_PARAS + '</p></td><td>' + child.receiveD_FROM + '</td><td>' + child.auditoR_REMARKS + '</td><td class="text-center"><a href="#" data-onclick="event.preventDefault();viewParaDetails(' + child.neW_PARA_ID + ',' + child.olD_PARA_ID + ',\'' + child.indicator + '\', \'' + child.parA_NO + '\', \'' + child.preV_ROLE + '\', \'' + child.nexT_ROLE + '\', \'' + child.coM_ID + '\'  );" class="text-hover text-danger mr-5px"><small>Compliance</small></a></td></tr>');
                });    
                
                initializeDataTable("manageObsPanel");

            },

            dataType: "json",
        });

    }

    function viewParaDetails(newParaId = 0, oldParaId = 0, indicator = '', memo_no = '', prevRole, nextRole, comID) {

        g_newParaId = newParaId;
        g_oldParaId = oldParaId;
        g_prevRole = prevRole;
        g_nextRole = nextRole;
        g_comId = comID;
        g_indicator = indicator;
        g_memoNo = memo_no;

        $('#viewParaComplianceModel').modal('show');
        $('#manageComplianceHistPanel tbody').empty();
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_compliance_history",
            type: "POST",
            data: {
                'COM_ID': comID,
            },
            cache: false,
            success: function (data) {
                if (data.length == 0) {
                    $('#manageComplianceHistPanel tbody').append('<tr><td class="text-center" colspan="5"><i>No previous compliance submitted yet</i></td><td><a data-onclick="event.preventDefault();getComplianceText(' + comID + ', 0);" href="#" class="text-danger">Submit Compliance</a></td></tr>');
                }

                else {

                    var cycle_count = data.length > 0 ? parseInt(data[0].coM_CYCLE) - 1 : 0;
                    $.each(data, function (i, v) {
                        if (v.coM_CYCLE > cycle_count) {
                            $('#manageComplianceHistPanel tbody').append('<tr><td><div>' + v.coM_CYCLE + '</div></td><td>' + v.pP_NO + '</td><td>' + v.name + '</td><td>' + v.commenT_BY_ROLE + '</td><td>' + v.comments + '</td><td><a data-onclick="event.preventDefault();getComplianceText(' + v.coM_ID + ',' + v.coM_CYCLE + ');" href="#" class="text-danger">View Compliance</a></td></tr>');
                            cycle_count++;
                        }
                        else
                            $('#manageComplianceHistPanel tbody').append('<tr><td></td><td><div>' + v.pP_NO + '</div></td><td><div>' + v.name + '</div></td><td>' + v.commenT_BY_ROLE + '</td><td>' + v.comments + '</td><td></td></tr>');

                    });
                    $('#manageComplianceHistPanel tbody').append('<tr><td class="text-center" colspan="5"><i>Re-submit compliance</i></td><td><a data-onclick="event.preventDefault();getComplianceText(' + comID + ', 0);" href="#" class="text-danger">Submit Compliance</a></td></tr>');
                }

            },

            dataType: "json",
        });
    }

    function PublishCompliance(ind) {

        var complianceRemarks = "";
        var commentsRemarks = "";
        var productImagesArr = [];

        if ($('.richText-editor').html() == "") {
            alert("Please provide Compliance to proceed");
            return;
        }
        complianceRemarks = $('.richText-editor').html();
        $('#nextRoleButtonHandler').attr("disabled", true);

        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/submit_post_audit_compliance",
            type: "POST",
            data: {
                'OLD_PARA_ID': g_oldParaId,
                'NEW_PARA_ID': g_newParaId,
                'INDICATOR': ind,
                'COMPLIANCE': complianceRemarks,
                'EVIDENCE_LIST': productImagesArr,
                'SUBFOLDER': g_comId
            },
            cache: false,
            success: function (data) {
                showApiAlert(data);
                $('#nextRoleButtonHandler').attr("disabled", false);
                onAlertCallback(reloadLocation);
            }, error: function (jqXHR, textStatus, errorThrown) {
                $('#nextRoleButtonHandler').attr("disabled", false);
                alert("An error occurred: " + jqXHR.status + " " + errorThrown);


            },
            dataType: "json",
        });
    }
    function reloadLocation() {
        $('#viewParaComplianceModel').modal('hide');
        $('#viewMemoModel').modal('hide');
        $('#submitComplianceMemoModel').modal('hide');

        window.location.reload();
    }
    function clearEvidencesLog() {
        $('.aks-file-upload-delete').click();
        $('.aks-file-upload-error').remove();
        document.getElementById('aksfileupload').value = '';
    }
    function getComplianceText(comID, cycle) {
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_para_compliance_text",
            type: "POST",
            data: {
                'NEW_PARA_ID': g_newParaId,
                'OLD_PARA_ID': g_oldParaId,
                'INDICATOR': g_indicator
            },
            cache: false,
            success: function (data) {

                if (cycle == "0") {
                    $('#submitComplianceMemoModel').modal('show');
                    
                    $('.aks-file-upload-delete').click();
                    $('.aks-file-upload-error').remove();
                    document.getElementById('aksfileupload').value = '';

                    $('#viewMemo_memoNumber_sc').val(g_memoNo);
                    $('#viewMemo_paraGist_sc').val(data.gisT_OF_PARA);
                    $('#viewMemo_memo_sc').html(data.parA_TEXT);
                    $('#viewMemo_compliance_sc').val('').trigger('change');

                    $('#listofRespPersons_sc tbody').empty();
                    if (data.responsiblE_PPs.length > 0) {
                        $.each(data.responsiblE_PPs, function (j, pp) {
                            var srNo = $('#listofRespPersons_sc tbody tr').length;
                            srNo++;
                            $('#listofRespPersons_sc tbody').append('<tr id="tr_' + pp.pP_NO + '"><td>' + srNo + '</td><td>' + pp.pP_NO + '</td><td>' + pp.emP_NAME + '</td><td>' + pp.loaN_CASE + '</td><td>' + pp.lC_AMOUNT + '</td><td>' + pp.accounT_NUMBER + '</td><td>' + pp.acC_AMOUNT + '</td></tr>');
                        });
                    }


                } else {
                    $('#viewMemoModel').modal('show');
                    $('#viewMemo_memoNumber').val(g_memoNo);
                    $('#viewMemo_paraGist').val(data.gisT_OF_PARA);
                    $('#viewMemo_memo').html(data.parA_TEXT);
                    $('#viewMemo_compliance').val('');

                    $('#listofRespPersons tbody').empty();
                    if (data.responsiblE_PPs.length > 0) {
                        $.each(data.responsiblE_PPs, function (j, pp) {
                            var srNo = $('#listofRespPersons tbody tr').length;
                            srNo++;
                            $('#listofRespPersons tbody').append('<tr id="tr_' + pp.pP_NO + '"><td>' + srNo + '</td><td>' + pp.pP_NO + '</td><td>' + pp.emP_NAME + '</td><td>' + pp.loaN_CASE + '</td><td>' + pp.lC_AMOUNT + '</td><td>' + pp.accounT_NUMBER + '</td><td>' + pp.acC_AMOUNT + '</td></tr>');
                        });
                    }

                    $.ajax({
                        url: g_asiBaseURL + "/ApiCalls/get_old_para_compliance_cycle_text",
                        type: "POST",
                        data: {
                            'COM_ID': comID,
                            'C_CYCLE': cycle
                        },
                        cache: false,
                        success: function (data) {
                            $('#complianceCycleTextPanel').html(data.parA_TEXT);
                            $('#complianceCycleEvidences').empty();
                            if (data.evidences.length > 0) {

                                $.each(data.evidences, function (j, pp) {
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
                            } else {
                                $('#complianceCycleEvidences').append("<i>No evidence is attached </i>");
                            }


                        },

                        dataType: "json",
                    });
                }
            },

            dataType: "json",
        });
    }

    function downloadFile(id) {
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_post_compliance_evidence_data",
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
