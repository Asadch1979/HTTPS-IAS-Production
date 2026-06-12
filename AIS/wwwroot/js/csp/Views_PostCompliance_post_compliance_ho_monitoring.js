    var g_newParaId = 0;
    var g_oldParaId = 0;
    var g_prevRole = "";
    var g_nextRole = "";
    var g_obsList = [];
    var g_allAttachedImages = [];
    var g_allowedFormats = ["pdf", "jpg", "jpeg", "png", "doc", "docx", "jpg", "csv", "xls", "xlsx"]; // allowed file formats

    var btnClick = "review";

    $(document).ready(function () {
        $('#viewMemo_compliance').richText({
            imageUpload: false,
            fileUpload: false,
            videoEmbed: false,
            urls: false
        });


          const monthSelect = document.getElementById('month-select');
    const months = [
        { short: 'Jan', full: 'January' },
        { short: 'Feb', full: 'February' },
        { short: 'Mar', full: 'March' },
        { short: 'Apr', full: 'April' },
        { short: 'May', full: 'May' },
        { short: 'Jun', full: 'June' },
        { short: 'Jul', full: 'July' },
        { short: 'Aug', full: 'August' },
        { short: 'Sep', full: 'September' },
        { short: 'Oct', full: 'October' },
        { short: 'Nov', full: 'November' },
        { short: 'Dec', full: 'December' }
    ];

    months.forEach(month => {
        const option = document.createElement('option');
        option.value = month.full;
        option.textContent = month.short;
        monthSelect.appendChild(option);
    });

    // Populate the Year Dropdown
    const yearSelect = document.getElementById('year-select');
    const currentYear = new Date().getFullYear();
    const startYear = 2024;

    for (let year = startYear; year <= currentYear; year++) {
        const option = document.createElement('option');
        option.value = year;
        option.textContent = year;
        yearSelect.appendChild(option);
    }

    });

    function getParaDetailValue(source, names) {
        if (!source || typeof source !== 'object') {
            return '';
        }

        for (var i = 0; i < names.length; i++) {
            var value = source[names[i]];
            if (value !== undefined && value !== null && String(value).trim() !== '') {
                return value;
            }
        }

        return '';
    }

    function setParaDetailHtml(selector, value) {
        $(selector).html(value && String(value).trim() !== '' ? value : '<span class="text-muted">N/A</span>');
    }

    function resolveAuditeeResponse(source) {
        return getParaDetailValue(source, ['AUDITEE_RESPONSE', 'auditeE_RESPONSE', 'auditeeResponse', 'BRANCH_REPLY', 'branchReply']);
    }

    function resolveAuditRecommendation(source) {
        return getParaDetailValue(source, ['AUDIT_RECOMMENDATION', 'auditRecommendation', 'AUDITOR_RECOMMENDATION', 'auditoR_RECOMMENDATION', 'RECOMMENDATION', 'recommendation', 'CAU_INSTRUCTION', 'cauInstruction']);
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

          $('#wait').show();
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_settled_post_compliances_for_monitoring",
            type: "POST",
            data: {
                "MONTH_NAME":$('#month-select').val(),
                "YEAR":$('#year-select').val()
            },
            cache: false,
            success: function (data) {
                $('#wait').hide();
                g_obsList = data;
                $.each(data, function (index, child) {
                    $('#manageObsPanel2 tbody').append('<tr id="div_' + child.id + '"><td>' + ++index + '</td><td>' + child.compliancE_UNIT + '</td><td>' + child.compliancE_SETTLEMENT_OFFICER + '</td><td>' + child.compliancE_UNIT_INCHARGE + '</td><td>' + child.entitY_NAME + '</td><td><p class="fw-normal mb-1">' + child.audiT_PERIOD + '</p></td><td><p class="fw-normal mb-1">' + child.parA_NO + '</p></td><td><p class="fw-normal mb-1">' + child.parA_RISK + '</p></td><td><p class="fw-normal mb-1">' + child.gisT_OF_PARAS + '</p></td><td>' + child.settleD_ON + '</td><td class="text-center"><a href="#" data-onclick="event.preventDefault();viewParaDetails(' + child.neW_PARA_ID + ',' + child.olD_PARA_ID + ',\'' + child.indicator + '\', \'' + child.parA_NO + '\', \'' + child.preV_ROLE + '\', \'' + child.nexT_ROLE + '\', \'' + child.coM_ID + '\'  );" class="text-hover text-danger mr-5px"><small>Compliance</small></a></td></tr>');
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


                $('#viewMemoReportingModel').modal('show');
                $('#viewMemo_memoNumber_rep').val(g_memoNo);
                $('#viewMemo_paraGist_rep').val(data.gisT_OF_PARA);
                $('#viewMemo_memo_rep').html(data.parA_TEXT);
                setParaDetailHtml('#viewMemo_auditeeResponse_rep', resolveAuditeeResponse(data));
                setParaDetailHtml('#viewMemo_auditRecommendation_rep', resolveAuditRecommendation(data));
                $('#viewMemo_compliance_rep').val('');
                if (g_prevRole == "")
                    $('#prevRoleButtonHandler_rep').remove();
                else
                    $('#prevRoleButtonHandler_rep').html(g_prevRole);

                if (g_nextRole == "")
                    $('#nextRoleButtonHandler_rep').remove();
                else
                    $('#nextRoleButtonHandler_rep').html(g_nextRole);

                $('#listofRespPersons_rep tbody').empty();
                if (data.responsiblE_PPs.length > 0) {
                    $.each(data.responsiblE_PPs, function (j, pp) {
                        var srNo = $('#listofRespPersons_rep tbody tr').length;
                        srNo++;
                        $('#listofRespPersons_rep tbody').append('<tr id="tr_' + pp.pP_NO + '"><td>' + srNo + '</td><td>' + pp.pP_NO + '</td><td>' + pp.emP_NAME + '</td><td>' + pp.loaN_CASE + '</td><td>' + pp.lC_AMOUNT + '</td><td>' + pp.accounT_NUMBER + '</td><td>' + pp.acC_AMOUNT + '</td></tr>');
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
