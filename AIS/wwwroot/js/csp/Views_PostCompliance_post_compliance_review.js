    var g_newParaId = 0;
    var g_comId = 0;
    var g_oldParaId = 0;
    var g_prevRole = "";
    var g_nextRole = "";
    var g_indicator = "";
    var g_memoNo = "";
    var g_obsList = [];
    var g_allAttachedImages = [];
    var g_allowedFormats = ["pdf", "jpg", "jpeg", "png", "doc", "docx", "jpg", "csv", "xls", "xlsx"]; // allowed file formats
    var g_maxCycle = 0;
    var g_complianceRemarksMaxLength = 1000;


    var btnClick = "review";

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
        getReivewParasForCompliance();
        $('#viewMemo_compliance').richText({
            imageUpload: false,
            fileUpload: false,
            videoEmbed: false,
            urls: false
        });

        $("aks-file-upload").aksFileUpload({
            fileUpload: "#aksfileupload", // With target [input]file or [type]json you can save the data of loaded items
            fileType: ["pdf", "zip", "jpg", "jpeg", "png", "doc", "docx", "jpg", "csv", "xls", "xlsx"], // allowed file formats
            dragDrop: true, // drag & drop upload
            maxSize: "2.5 MB", // maximum uploaded file size
            multiple: true, // multiple file upload
            maxFile: 100, // maximum number of uploaded files
            maxFileError: "File exceeds upload limit. - Max limit:", // error text
            maxSizeError: "File exceeds size. - Max limit:", // error text
            fileTypeError: "Disallowed file format.", // error text
            label: "Drag & Drop your files or Browse" // label text

        });

        document.getElementById('aksfileupload').addEventListener('change', function () {

            var g_imgFiles = this.files;

            function readFileAsDataURL(file) {
                return new Promise((resolve, reject) => {
                    const reader = new FileReader();
                    reader.onload = () => resolve(reader.result.split(',')[1]); // get Base64 string without prefix
                    reader.onerror = reject;
                    reader.readAsDataURL(file);
                });
            }

            async function processFiles(files) {
                const productImagesArr = [];
                for (let i = 0; i < files.length; i++) {
                    const file = files[i];
                    if (file.size > 2621440)
                        continue;
                    var ext = getFileExtension(file);
                    if (!g_allowedFormats.includes(ext))
                        continue;
                    try {
                        const base64Data = await readFileAsDataURL(file);
                        var fileNameGen = generateUniqueTimestamp() + "_" + i;
                        var extension = getFileExtension(file);
                        const ProductObject = {
                            'TEXT_ID': g_comId,
                            'IMAGE_NAME': file.name,
                            'FILE_NAME': fileNameGen + "." + extension,
                            'IMAGE_DATA': base64Data,
                            'IMAGE_TYPE': extension,
                            'LENGTH': file.size,
                            'SEQUENCE': i
                        };
                        productImagesArr.push(ProductObject);
                        g_allAttachedImages.push(ProductObject);
                    } catch (error) {

                    }
                }
                uploadFiles(productImagesArr);
            }

            function uploadFiles(productImagesArr) {

                $.each($(".aks-file-upload .aks-file-upload-delete"), function (i, v) {
                    var filename = $(v).attr("data-delete");
                    for (var i = 0; i < g_allAttachedImages.length; i++) {
                        if (g_allAttachedImages[i].IMAGE_NAME == filename) {
                            $(v).attr("file-name", g_allAttachedImages[i].FILE_NAME);
                        }
                    }
                });
                $(".aks-file-upload .aks-file-upload-delete").on("click", function (e) {
                    var filename = $(this).attr("file-name");
                    deleteFileFromServer(filename);
                });
                /*
                $.ajax({
                    url: g_asiBaseURL + "/UploadFile/UploadFiles",
                    type: 'POST',
                    dataType: "json",
                    data: {
                        "files": productImagesArr
                    },
                    success: function (response) {


                    },
                    error: function (error) {

                    }
                });   */
            }
            processFiles(g_imgFiles);
        });


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
        deleteImageObjectByFileName(fileName);
    }

    function getReivewParasForCompliance() {
        
        destroyDatatable('manageObsPanel2');
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_paras_for_review_compliance_by_auditee",
            type: "POST",
            data: {

            },
            cache: false,
            success: function (data) {
                g_obsList = data;
                $.each(data, function (index, child) {
                    $('#manageObsPanel2 tbody').append('<tr id="div_' + child.id + '"><td>' + ++index + '</td><td><p class="fw-normal mb-1">' + child.audiT_PERIOD + '</p></td><td><p class="fw-normal mb-1">' + child.parA_NO + '</p></td><td><p class="fw-normal mb-1">' + getDisplayParaRisk(child.parA_RISK) + '</p></td><td><p class="fw-normal mb-1">' + child.gisT_OF_PARAS + '</p></td><td>' + child.receiveD_FROM + '</td><td>' + child.auditoR_REMARKS + '</td><td class="text-center"><a href="#" data-onclick="event.preventDefault();viewParaDetails(' + child.neW_PARA_ID + ',' + child.olD_PARA_ID + ',\'' + child.indicator + '\', \'' + child.parA_NO + '\', \'' + child.preV_ROLE + '\', \'' + child.nexT_ROLE + '\', \'' + child.coM_ID + '\'  );" class="text-hover text-danger mr-5px"><small>Compliance</small></a></td></tr>');
                });
                  initializeDataTable('manageObsPanel2');             

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

                var cycle_count = data.length > 0 ? parseInt(data[0].coM_CYCLE) - 1 : 0;
                $.each(data, function (i, v) {
                    g_maxCycle = parseInt(v.coM_CYCLE);
                    if (v.coM_CYCLE > cycle_count) {
                        $('#manageComplianceHistPanel tbody').append('<tr><td><div>' + v.coM_CYCLE + '</div></td><td>' + v.pP_NO + '</td><td>' + v.name + '</td><td>' + v.commenT_BY_ROLE + '</td><td>' + v.comments + '</td><td><a data-onclick="event.preventDefault();getComplianceText(' + v.coM_ID + ',' + v.coM_CYCLE + ');" href="#" class="text-danger">View Compliance</a></td></tr>');
                        cycle_count++;
                    }
                    else
                        $('#manageComplianceHistPanel tbody').append('<tr><td></td><td><div>' + v.pP_NO + '</div></td><td><div>' + v.name + '</div></td><td>' + v.commenT_BY_ROLE + '</td><td>' + v.comments + '</td><td></td></tr>');

                });

            },

            dataType: "json",
        });
    }

    function PublishCompliance(ind) {


        var commentsRemarks = "";
        var evidenceList = [];
        var requestData = {};
        if ($('#viewMemoModel').hasClass('show')) {
            commentsRemarks = $('#viewMemoModel .richText-editor').html() || $('#viewMemo_compliance').val();
            evidenceList = g_allAttachedImages;
        } else {
            commentsRemarks = $('#viewMemo_compliance_rep').val();
        }

        if ($.trim(commentsRemarks) == "") {
            alert("Please provide Remarks to proceed");
            return;
        }

        if (!validateComplianceRemarksLength(true, commentsRemarks)) {
            return;
        }

        requestData = {
            'OLD_PARA_ID': g_oldParaId,
            'NEW_PARA_ID': g_newParaId,
            'INDICATOR': ind,
            'COMMENTS': commentsRemarks,
        };

        $.each(evidenceList, function (index, item) {
            requestData['EVIDENCE_LIST[' + index + '].TEXT_ID'] = item.TEXT_ID;
            requestData['EVIDENCE_LIST[' + index + '].IMAGE_NAME'] = item.IMAGE_NAME;
            requestData['EVIDENCE_LIST[' + index + '].FILE_NAME'] = item.FILE_NAME;
            requestData['EVIDENCE_LIST[' + index + '].IMAGE_DATA'] = item.IMAGE_DATA;
            requestData['EVIDENCE_LIST[' + index + '].IMAGE_TYPE'] = item.IMAGE_TYPE;
            requestData['EVIDENCE_LIST[' + index + '].LENGTH'] = item.LENGTH;
            requestData['EVIDENCE_LIST[' + index + '].SEQUENCE'] = item.SEQUENCE;
        });


        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/submit_post_audit_compliance_review",
            type: "POST",
            data: requestData,
            cache: false,
            success: function (data) {
                showApiAlert(data);
                onAlertCallback(reloadLocation);
            },
            dataType: "json",
        });
    }

    function getComplianceRemarksText(remarks) {
        return $('<div>').html(remarks || '').text().replace(/\u00a0/g, ' ');
    }

    function validateComplianceRemarksLength(showAlert, remarks) {
        var isReportingModal = $('#viewMemoReportingModel').hasClass('show');
        var currentRemarks = remarks;
        if (typeof currentRemarks === 'undefined') {
            currentRemarks = isReportingModal
                ? $('#viewMemo_compliance_rep').val()
                : ($('#viewMemoModel .richText-editor').html() || $('#viewMemo_compliance').val());
        }

        var isValid = getComplianceRemarksText(currentRemarks).length <= g_complianceRemarksMaxLength;
        var errorSelector = isReportingModal ? '#viewMemo_compliance_rep_error' : '#viewMemo_compliance_error';
        $(errorSelector).toggleClass('d-none', isValid);

        if (!isValid && showAlert) {
            alert('Only 1000 characters are allowed in Remarks.');
        }
        return isValid;
    }
    function reloadLocation() {

        $('#viewParaComplianceModel').modal('hide');
        $('#viewMemoModel').modal('hide');
        $('#viewMemoReportingModel').modal('hide');


      


        getReivewParasForCompliance();
    }
    function clearEvidencesLog() {
        $('.aks-file-upload-delete').click();
        $('.aks-file-upload-error').remove();
        g_allAttachedImages = [];
        var fileInput = document.getElementById('aksfileupload');
        if (fileInput) {
            fileInput.value = '';
        }
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
                    $('#viewMemoModel').modal('show');
                  
                    $('.aks-file-upload-delete').click();
                    $('.aks-file-upload-error').remove();
                    g_allAttachedImages = [];
                    var fileInput = document.getElementById('aksfileupload');
                    if (fileInput) {
                        fileInput.value = '';
                    }

                    $('#viewMemo_memoNumber').val(g_memoNo);
                    $('#viewMemo_paraGist').val(data.gisT_OF_PARA);
                    $('#viewMemo_memo').html(data.parA_TEXT);
                    $('#viewMemo_compliance').val('').trigger('change');
                    if (g_prevRole == "")
                        $('#prevRoleButtonHandler').remove();
                    else
                        $('#prevRoleButtonHandler').html(g_prevRole);

                    if (g_nextRole == "")
                        $('#nextRoleButtonHandler').remove();
                    else
                        $('#nextRoleButtonHandler').html(g_nextRole);

                    $('#listofRespPersons tbody').empty();
                    if (data.responsiblE_PPs.length > 0) {
                        $.each(data.responsiblE_PPs, function (j, pp) {
                            var srNo = $('#listofRespPersons tbody tr').length;
                            srNo++;
                            $('#listofRespPersons tbody').append('<tr id="tr_' + pp.pP_NO + '"><td>' + srNo + '</td><td>' + pp.pP_NO + '</td><td>' + pp.emP_NAME + '</td><td>' + pp.loaN_CASE + '</td><td>' + pp.lC_AMOUNT + '</td><td>' + pp.accounT_NUMBER + '</td><td>' + pp.acC_AMOUNT + '</td></tr>');
                        });
                    }
                    $('#complianceCycleEvidences').empty();

                    $("#wait").css("display", "block");
                    $.ajax({
                        url: g_asiBaseURL + "/ApiCalls/get_old_para_compliance_cycle_text",
                        type: "POST",
                        data: {
                            'COM_ID': comID,
                            'C_CYCLE': cycle
                        },
                        cache: false,
                        success: function (data) {
                            $("#wait").css("display", "none");
                            $('#complianceCycleTextPanel').html(data.parA_TEXT);
                            $('#complianceCycleEvidences').empty();
                            if (data.evidences && data.evidences.length > 0) {

                                $.each(data.evidences, function (j, pp) {
                                    var extension = pp.imagE_NAME.split('.').pop().toLowerCase();
                                    const blob = base64ToBlob(pp.imagE_DATA, getContentType(extension));
                                    const link = document.createElement('a');
                                    link.href = URL.createObjectURL(blob);
                                    link.download = pp.imagE_NAME;
                                    link.innerText = "Download " + pp.imagE_NAME; // Set the text inside the hyperlink
                                    const icon = document.createElement('i');
                                    icon.className = getIconClass(extension) + ' evidence-icon mr-1';

                                    const container = document.createElement('div');
                                    container.className = 'evidence-link';
                                    container.appendChild(icon);
                                    container.appendChild(link);

                                    $('#complianceCycleEvidences').append(container);
                                });
                            } else {
                                $('#complianceCycleEvidences').append("<i>No evidence is attached </i>");
                            }
                        },

                        dataType: "json",
                    });

                } else {
                    $('#viewMemoReportingModel').modal('show');
                    $('#viewMemo_memoNumber_rep').val(g_memoNo);
                    $('#viewMemo_paraGist_rep').val(data.gisT_OF_PARA);
                    $('#viewMemo_memo_rep').html(data.parA_TEXT);
                    $('#viewMemo_compliance_rep').val('');
                    if (g_prevRole == "")
                        $('#prevRoleButtonHandler_rep').remove();
                    else
                        $('#prevRoleButtonHandler_rep').html(g_prevRole);

                    if (g_nextRole == "")
                        $('#nextRoleButtonHandler_rep').remove();
                    else
                        $('#nextRoleButtonHandler_rep').html(g_nextRole);


                    if (g_maxCycle != parseInt(cycle)) {

                        $('#prevRoleButtonHandler_rep').addClass('d-none');
                        $('#nextRoleButtonHandler_rep').addClass('d-none');
                        $('#viewMemo_compliance_rep').parent().addClass('d-none');

                    } else {
                        $('#prevRoleButtonHandler_rep').removeClass('d-none');
                        $('#nextRoleButtonHandler_rep').removeClass('d-none');
                        $('#viewMemo_compliance_rep').parent().removeClass('d-none');
                    }

                    $('#listofRespPersons_rep tbody').empty();

                    if (data.responsiblE_PPs.length > 0) {
                        $.each(data.responsiblE_PPs, function (j, pp) {
                            var srNo = $('#listofRespPersons_rep tbody tr').length;
                            srNo++;
                            $('#listofRespPersons_rep tbody').append('<tr id="tr_' + pp.pP_NO + '"><td>' + srNo + '</td><td>' + pp.pP_NO + '</td><td>' + pp.emP_NAME + '</td><td>' + pp.loaN_CASE + '</td><td>' + pp.lC_AMOUNT + '</td><td>' + pp.accounT_NUMBER + '</td><td>' + pp.acC_AMOUNT + '</td></tr>');
                        });
                    }
                    $("#wait").css("display", "block");
                    $('#complianceCycleEvidences_rep').empty();
                    $.ajax({
                        url: g_asiBaseURL + "/ApiCalls/get_old_para_compliance_cycle_text",
                        type: "POST",
                        data: {
                            'COM_ID': comID,
                            'C_CYCLE': cycle
                        },
                        cache: false,
                        success: function (data) {
                            $('#complianceCycleTextPanel_rep').html(data.parA_TEXT);
                            $('#complianceCycleEvidences_rep').empty();
                            $("#wait").css("display", "none");
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

                                    $('#complianceCycleEvidences_rep').append(container);
                                });
                            } else {
                                $('#complianceCycleEvidences_rep').append("<i>No evidence is attached </i>");
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
