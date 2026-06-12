    var g_entId = 0;
    $(document).ready(function () {
        var url_string = window.location;
        var url = new URL(url_string);
        g_entId = url.searchParams.get("entId");
        getObservations();
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
    function getObservations() {
       
        destroyDatatable('entitywise_panel');
        if (g_entId == 0)
            return;       

        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_entity_wise_observation_detail",
            type: "POST",
            data: {
                'ENTITY_ID': g_entId
            },
            cache: false,
            success: function (data) {
                var sr = 1;

                $.each(data, function (index, item) {
                    var $row = $('<tr />');
                    $row.append('<td>' + sr + '</td>');
                    $row.append('<td>' + item.name + '</td>');
                    $row.append('<td>' + item.audiT_PERIOD + '</td>');
                    $row.append('<td class="text-right">' + item.parA_NO + '</td>');
                    $row.append('<td class="text-left">' + item.parA_GIST + '</td>');
                    $row.append('<td class="text-left">' + item.p_RISK + '</td>');

                    var comId = item.coM_ID || item.com_ID || item.COM_ID || '';
                    var $link = $('<a />', {
                        href: '#',
                        text: 'View Para Text',
                        class: 'view-para-text',
                        'data-com-id': comId
                    });

                    $row.append($('<td />').append($link));
                    $('#entitywise_panel tbody').append($row);
                    sr++;
                });
                initializeDataTable('entitywise_panel');
            },
            dataType: "json",
        });
    }

    $(document).on('click', '.view-para-text', function (event) {
        event.preventDefault();
        var $link = $(this);
        var comID = $link.data('comId');

        if (!comID) {
            $('#paraTextDivField').html('<p class="text-center">Para information is unavailable.</p>');
            $('#paraTextViewerModel').modal("show");
            return;
        }

        getParaText(comID);
    });

    function getParaText(comID) {
        $('#paraTextViewerModel').modal("show");
        $('#paraTextDivField').empty();
        setParaDetailHtml('#auditeeResponseDivField', '');
        setParaDetailHtml('#auditRecommendationDivField', '');

        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_all_para_text",
            type: "POST",
            data: {
                'COM_ID': comID
            },
            cache: false,
            success: function (data) {
                if (data) {
                    var paraText = typeof data === 'object' ? getParaDetailValue(data, ['PARA_TEXT', 'parA_TEXT', 'ParaText', 'paraText', 'Text', 'text']) : data;
                    $('#paraTextDivField').html(paraText || '<p class="text-center">No para text available.</p>');
                    setParaDetailHtml('#auditeeResponseDivField', resolveAuditeeResponse(data));
                    setParaDetailHtml('#auditRecommendationDivField', resolveAuditRecommendation(data));
                } else {
                    $('#paraTextDivField').html('<p class="text-center">No para text available.</p>');
                    setParaDetailHtml('#auditeeResponseDivField', '');
                    setParaDetailHtml('#auditRecommendationDivField', '');
                }
            }
        });
    }
