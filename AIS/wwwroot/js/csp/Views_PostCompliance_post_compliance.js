    var g_newParaId = 0;
    var g_comId = 0;
    var g_oldParaId = 0;
    var g_prevRole = "";
    var g_nextRole = "";
    var g_obsList = [];
    var fileInput = null;
    var g_allAttachedImages = [];
    var g_allowedFormats = ["pdf", "jpg", "jpeg", "png", "doc", "docx", "jpg", "csv", "xls", "xlsx"]; // allowed file formats


    var btnClick = "own";
    var g_allowLimit = '12'; // Maximum file size in MB

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
            formData.append('subfolder', g_comId);

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
                    url: g_asiBaseURL + "/UploadFile/UploadFiles",
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

                    $('#manageObsPanel tbody').append('<tr id="div_' + child.id + '"><td>' + ++index + '</td><td><p class="fw-normal mb-1">' + child.audiT_PERIOD + '</p></td><td><p class="fw-normal mb-1">' + child.parA_NO + '</p></td><td><p class="fw-normal mb-1">' + child.audiT_DATE + '</p></td><td><p class="fw-normal mb-1">' + child.gisT_OF_PARAS + '</p></td><td>' + child.receiveD_FROM + '</td><td>' + child.auditoR_REMARKS + '</td><td class="text-center"><a href="#" data-click="event.preventDefault();viewParaDetails(' + child.neW_PARA_ID + ',' + child.olD_PARA_ID + ',\'' + child.indicator + '\', \'' + child.parA_NO + '\', \'' + child.preV_ROLE + '\', \'' + child.nexT_ROLE + '\', \'' + child.coM_ID + '\'  );" class="text-hover text-danger mr-5px"><small>Compliance</small></a></td></tr>');
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
                    $('#manageComplianceHistPanel tbody').append('<tr><td class="text-center" colspan="5"><i>No previous compliance submitted yet</i></td><td><a data-click="event.preventDefault();getComplianceText(' + comID + ', 0);" href="#" class="text-danger">Submit Compliance</a></td></tr>');
                }

                else {

                    var cycle_count = data.length > 0 ? parseInt(data[0].coM_CYCLE) - 1 : 0;
                    $.each(data, function (i, v) {
                        if (v.coM_CYCLE > cycle_count) {
                            $('#manageComplianceHistPanel tbody').append('<tr><td><div>' + v.coM_CYCLE + '</div></td><td>' + v.pP_NO + '</td><td>' + v.name + '</td><td>' + v.commenT_BY_ROLE + '</td><td>' + v.comments + '</td><td><a data-click="event.preventDefault();getComplianceText(' + v.coM_ID + ',' + v.coM_CYCLE + ');" href="#" class="text-danger">View Compliance</a></td></tr>');
                            cycle_count++;
                        }
                        else
                            $('#manageComplianceHistPanel tbody').append('<tr><td></td><td><div>' + v.pP_NO + '</div></td><td><div>' + v.name + '</div></td><td>' + v.commenT_BY_ROLE + '</td><td>' + v.comments + '</td><td></td></tr>');

                    });
                    $('#manageComplianceHistPanel tbody').append('<tr><td class="text-center" colspan="5"><i>Re-submit compliance</i></td><td><a data-click="event.preventDefault();getComplianceText(' + comID + ', 0);" href="#" class="text-danger">Submit Compliance</a></td></tr>');
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
