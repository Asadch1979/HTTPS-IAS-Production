var g_obsList = [];
var g_selectedComId = "";

function reportText(value) {
    return $('<div>').text(value || '').html();
}

function initializeReportTable(id, options) {
    if ($.fn.DataTable.isDataTable('#' + id)) {
        $('#' + id).DataTable().clear().destroy();
    }

    return $('#' + id).DataTable($.extend({
        dom: '<"top"lfB>rt<"bottom"ip><"clear">',
        autoWidth: true,
        ordering: true,
        buttons: [
            getPdfExportButtonConfig(),
            getExcelExportButtonConfig('Export to Excel'),
            getCsvExportButtonConfig('Export to CSV'),
            {
                extend: 'copyHtml5',
                text: 'Copy to Clipboard'
            }
        ],
        lengthMenu: [
            [10, 50, 100, -1],
            [10, 50, 100, "All"]
        ]
    }, options || {}));
}

function getSettledParasForMonitoring() {
    destroyDatatable("manageObsPanel");
    $('#manageObsPanel tbody').empty();

    $.ajax({
        url: g_asiBaseURL + "/ApiCalls/get_settled_paras_for_monitoring",
        type: "POST",
        data: {
            'ENTITY_ID': $('#entitySelectField').val()
        },
        cache: false,
        success: function (data) {
            g_obsList = data || [];

            $.each(g_obsList, function (index, child) {
                var comId = child.coM_ID || '';
                var complianceCycle = child.compliancE_CYCLE || '';
                $('#manageObsPanel tbody').append(
                    '<tr>' +
                    '<td><p class="fw-normal mb-1">' + reportText(child.reportinG_OFFICE) + '</p></td>' +
                    '<td><p class="fw-normal mb-1">' + reportText(child.entitY_NAME) + '</p></td>' +
                    '<td><p class="fw-normal mb-1">' + reportText(child.audiT_PERIOD) + '</p></td>' +
                    '<td><p class="fw-normal mb-1">' + reportText(child.parA_NO) + '</p></td>' +
                    '<td><p class="fw-normal mb-1">' + reportText(child.settleD_BY) + '</p></td>' +
                    '<td><p class="fw-normal mb-1">' + reportText(child.settleD_ON) + '</p></td>' +
                    '<td><p class="fw-normal mb-1">' + reportText(child.risk) + '</p></td>' +                   
                    '<td><a href="#" class="js-view-para-compliance" data-com-id="' + reportText(comId) + '">View Compliance</a></td>' +
                    '</tr>');
            });

            initializeReportTable("manageObsPanel", {
                columnDefs: [
                    { orderable: false, targets: [7] }
                ]
            });
        },
        dataType: "json"
    });
}

function viewParaCompliance(comId) {
    if (!comId || comId === "0") return;
    g_selectedComId = comId;
    $('#viewParaComplianceModel').modal('show');
    destroyDatatable("manageComplianceHistPanel");
    $('#manageComplianceHistPanel tbody').empty();

    $.ajax({
        url: g_asiBaseURL + "/ApiCalls/get_settled_para_compliance_history",
        type: "POST",
        data: {
            'COM_ID': comId
        },
        cache: false,
        success: function (data) {
            var cycleCount = data && data.length > 0 ? parseInt(data[0].coM_CYCLE, 10) - 1 : 0;
            $.each(data || [], function (i, v) {
                var cycle = parseInt(v.coM_CYCLE, 10);
                if (cycle > cycleCount) {
                    $('#manageComplianceHistPanel tbody').append(
                        '<tr>' +
                        '<td><div>' + reportText(v.coM_CYCLE) + '</div></td>' +
                        '<td>' + reportText(v.pP_NO) + '</td>' +
                        '<td>' + reportText(v.name) + '</td>' +
                        '<td>' + reportText(v.commenT_BY_ROLE) + '</td>' +
                        '<td>' + reportText(v.comments) + '</td>' +
                        '<td><a href="#" class="js-view-compliance-text" data-com-id="' + reportText(v.coM_ID) + '" data-com-cycle="' + reportText(v.coM_CYCLE) + '">View Compliance</a></td>' +
                        '</tr>');
                    cycleCount++;
                } else {
                    $('#manageComplianceHistPanel tbody').append(
                        '<tr>' +
                        '<td></td>' +
                        '<td><div>' + reportText(v.pP_NO) + '</div></td>' +
                        '<td><div>' + reportText(v.name) + '</div></td>' +
                        '<td>' + reportText(v.commenT_BY_ROLE) + '</td>' +
                        '<td>' + reportText(v.comments) + '</td>' +
                        '<td></td>' +
                        '</tr>');
                }
            });

            initializeReportTable("manageComplianceHistPanel", {
                ordering: false,
                columnDefs: [
                    { orderable: false, targets: [5] }
                ]
            });
        },
        dataType: "json"
    });
}

function getParaDetails(comId, cycle) {
    if (!comId || !cycle) return;
    $('#viewParaComplianceTextModel').modal('show');
    $('#viewParaDetails_paraNo').val('');
    $('#viewParaDetails_gist').val('');
    $('#viewParaDetails_paraText').empty();
    $('#viewParaDetails_complianceReply').empty();

    $.ajax({
        url: g_asiBaseURL + "/ApiCalls/get_para_compliance_text",
        type: "POST",
        data: {
            'COM_ID': comId,
            'C_CYCLE': cycle
        },
        cache: false,
        success: function (data) {
            $('#viewParaDetails_paraNo').val(data.parA_NO || '');
            $('#viewParaDetails_gist').val(data.gisT_OF_PARA || '');
            $('#viewParaDetails_paraText').html(data.obS_TEXT || '');
            $('#viewParaDetails_complianceReply').html(data.parA_TEXT || '');
        },
        dataType: "json"
    });
}

$(document).on('change', '#entitySelectField', function () {
    getSettledParasForMonitoring();
});

$(document).on('click', '.js-view-para-text', function (event) {
    event.preventDefault();
    getParaDetails($(this).data('com-id'), $(this).data('com-cycle'));
});

$(document).on('click', '.js-view-para-compliance', function (event) {
    event.preventDefault();
    viewParaCompliance($(this).data('com-id'));
});

$(document).on('click', '.js-view-compliance-text', function (event) {
    event.preventDefault();
    getParaDetails($(this).data('com-id'), $(this).data('com-cycle'));
});
