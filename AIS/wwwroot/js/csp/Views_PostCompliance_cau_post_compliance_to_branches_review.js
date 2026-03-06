    var g_newParaId = 0;
    var g_comId = 0;
    var g_oldParaId = 0;
    var g_brEntId = 0;

    $(document).ready(function () {
        getOwnParasForCompliance();
    });
    function getOwnParasForCompliance() {
       destroyDatatable("manageObsPanel");
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_cau_paras_for_compliance_for_review",
            type: "POST",
            data: {

            },
            cache: false,
            success: function (data) {
                g_obsList = data;

                $.each(data, function (index, child) {

                    $('#manageObsPanel tbody').append('<tr id="div_' + child.id + '"><td>' + ++index + '</td><td><p class="fw-normal mb-1">' + child.audiT_PERIOD + '</p></td><td><p class="fw-normal mb-1">' + child.parA_NO + '</p></td><td><p class="fw-normal mb-1">' + child.gisT_OF_PARAS + '</p></td><td class="text-center"><a href="#" data-onclick="event.preventDefault();viewParaDetails(' + child.neW_PARA_ID + ',' + child.olD_PARA_ID + ',\'' + child.coM_ID + '\',\'' + child.indicator + '\',\'' + child.parA_NO + '\',\'' + child.caU_ASSIGNED_ENT_ID + '\', );" class="text-hover text-danger mr-5px"><small>Review</small></a></td></tr>');
                });

                 initializeDataTable("manageObsPanel");

            },

            dataType: "json",
        });
    }
    function viewParaDetails(newParaId = 0, oldParaId = 0, comID, indicator, paraNo, brId) {

        g_newParaId = newParaId;
        g_oldParaId = oldParaId;
        g_comId = comID;
        g_indicator = indicator;
        g_brEntId = brId;
        $('#viewParaComplianceModel').modal('show');
        $('#viewParaNoModelField').val(paraNo);
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_cau_para_to_branch_para_text",
            type: "POST",
            data: {
                'COM_ID': g_comId,
                'INDICATOR': g_indicator
            },
            cache: false,
            success: function (data) {
                $('#viewParaTextModelField').html(data.memO_TXT);
                $('#branchReplyAgainstCAUIns').html(data.brancH_REPLY);
                $('#cauInstructionToBranch').val(data.caU_INSTRUCTION);
                $('#complianceCycleEvidences').empty();
                $.ajax({
                    url: g_asiBaseURL + "/ApiCalls/get_cau_paras_evidences_for_compliance_for_review",
                    type: "POST",
                    data: {
                        'TEXT_ID': data.texT_ID
                    },
                    cache: false,
                    success: function (data) {
                        if (data.length > 0) {

                            $.each(data, function (j, pp) {
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

            },
            dataType: "json",
        });

    }
    function reloadLocation() {
        $('#viewParaComplianceModel').modal('hide');
        window.location.reload();
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
    function downloadFile(id) {
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_cau_paras_post_compliance_evidence_data",
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

    function CAUSubmitParaToBranch() {



        if ($('#cauFurtherInstruction').val() == "") {
            alert("Please provide instructions to proceed");
            return;
        }
        $('#submitCAUParaResponseHandler').attr("disabled", true);
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/submit_cau_para_to_branch",
            type: "POST",
            data: {
                'COM_ID': g_comId,
                'BR_ENT_ID': g_brEntId,
                'CAU_COMMENTS': $('#cauFurtherInstruction').val()
            },
            cache: false,
            success: function (data) {
                showApiAlert(data);
                $('#submitCAUParaResponseHandler').attr("disabled", false);
                onAlertCallback(reloadLocation);
            }, error: function (jqXHR, textStatus, errorThrown) {
                $('#submitCAUParaResponseHandler').attr("disabled", false);
                alert("An error occurred: " + jqXHR.status + " " + errorThrown);


            },
            dataType: "json",
        });
    }
