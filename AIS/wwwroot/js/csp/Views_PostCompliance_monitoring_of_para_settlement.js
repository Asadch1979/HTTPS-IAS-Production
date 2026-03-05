var g_paraId = 0;
var refp = 0;
var g_obsList = [];
var g_obsId = 0;
var g_paraRefP = 0;

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
            g_obsList = data;

            $.each(data, function (index, child) {
                $('#manageObsPanel tbody').append('<tr><td><p class="fw-normal mb-1">' + child.reportinG_OFFICE + '</p><td><p class="fw-normal mb-1">' + child.entitY_NAME + '</p></td><td><p class="fw-normal mb-1">' + child.audiT_PERIOD + '</p></td><td><p class="fw-normal mb-1">' + child.parA_NO + '</p></td><td><p class="fw-normal mb-1">' + child.settleD_BY + '</p></td><td><p class="fw-normal mb-1">' + child.settleD_ON + '</p></td><td><p class="fw-normal mb-1">' + child.risk + '</p></td><td><a href="#" class="js-view-para-text" data-com-id="' + child.coM_ID + '">View Para Text</a></td><td><a href="#" class="js-view-para-compliance" data-ref-p="' + child.reF_P + '" data-obs-id="' + child.aU_OBS_ID + '">View Compliance</a></td><td><a href="#" class="js-mark-comments" data-ref-p="' + child.reF_P + '" data-obs-id="' + child.aU_OBS_ID + '">Comments</a></td></tr>');
            });
            initializeDataTable("manageObsPanel");
        },

        dataType: "json",
    });
}

function getParaText(comId) {
    if (!comId || comId === "0") return;
    $('#paraTextViewerModel').modal("show");
    $('#paraTextDivField').empty();

    $.ajax({
        url: g_asiBaseURL + "/ApiCalls/get_all_para_text",
        type: "POST",
        data: {
            'COM_ID': comId
        },
        cache: false,
        success: function (data) {
            $('#paraTextDivField').html(data);
        }
    });
}

function viewParaCompliance(paraREF, obsID) {
    g_paraRefP = paraREF;
    g_obsId = obsID;
    $('#viewParaComplianceModel').modal('show');
    $('#manageComplianceHistPanel tbody').empty();

    $.ajax({
        url: g_asiBaseURL + "/ApiCalls/get_settled_para_compliance_history",
        type: "POST",
        data: {
            'REF_P': paraREF,
            'OBS_ID': obsID
        },
        cache: false,
        success: function (data) {
            var rowSpan = data.length;
            $.each(data, function (i, v) {
                var lastCol = '';

                if (i == 0) {
                    lastCol = '<td rowspan="' + rowSpan + '"><a href="#" class="js-view-compliance-text" data-com-seq="' + v.coM_SEQ_NO + '">View Compliance</a></td>';
                }

                $('#manageComplianceHistPanel tbody').append('<tr><td><div>' + v.attendeD_BY + '</div></td><td><div>' + v.name + '</div></td><td><div>' + v.designation + '</div></td><td>' + v.remarks + '</td>' + lastCol + '</tr>');

            });

        },

        dataType: "json",
    });
}

function getComplianceText(comSeq) {
    $('#viewParaComplianceTextModel').modal('show');
    $.ajax({
        url: g_asiBaseURL + "/ApiCalls/get_old_para_compliance_cycle_text",
        type: "POST",
        data: {
            'REF_P': g_paraRefP,
            'OBS_ID': g_obsId,
            'COM_SEQ': comSeq

        },
        cache: false,
        success: function (data) {
            $('#complianceCycleTextPanel').html(data.parA_TEXT);
        },

        dataType: "json",
    });
}

function markActionComments(refP, obsId) {

    g_obsId = obsId;
    g_paraRefP = refP;
    $('#viewParaComplianceCommentsModel').modal('show');
    $('#commentsOnCompliance').val('');
}

function saveCommentsOnSettledCompliance() {

    $.ajax({
        url: g_asiBaseURL + "/ApiCalls/submit_settled_para_compliance_comments",
        type: "POST",
        data: {
            'REF_P': g_paraRefP,
            'OBS_ID': g_obsId,
            'COMMENTS': $('#commentsOnCompliance').val()

        },
        cache: false,
        success: function (data) {
            showApiAlert(data);
            onAlertCallback(reloadLocation);
        },

        dataType: "json",
    });
}

function reloadLocation() {
    getSettledParasForMonitoring();
}

$(document).on('change', '#entitySelectField', function () {
    getSettledParasForMonitoring();
});

$(document).on('click', '.js-view-para-text', function (event) {
    event.preventDefault();
    getParaText($(this).data('com-id'));
});

$(document).on('click', '.js-view-para-compliance', function (event) {
    event.preventDefault();
    viewParaCompliance($(this).data('ref-p'), $(this).data('obs-id'));
});

$(document).on('click', '.js-mark-comments', function (event) {
    event.preventDefault();
    markActionComments($(this).data('ref-p'), $(this).data('obs-id'));
});

$(document).on('click', '.js-view-compliance-text', function (event) {
    event.preventDefault();
    getComplianceText($(this).data('com-seq'));
});


$(document).on('click', '#saveCommentsOnSettledComplianceBtn', function () {
    saveCommentsOnSettledCompliance();
});
