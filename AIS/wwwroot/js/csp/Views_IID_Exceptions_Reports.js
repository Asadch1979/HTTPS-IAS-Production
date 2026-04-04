var g_iidEngID = 0;

$(document).ready(function () {
    var pageRoot = document.getElementById("iidExceptionReportsRoot");
    if (!pageRoot) {
        return;
    }

    var url = new URL(window.location.href);
    var engIdFromDom = parseInt(pageRoot.getAttribute("data-eng-id") || "0", 10) || 0;
    var engIdFromQuery = parseInt(url.searchParams.get("engId") || url.searchParams.get("complaintId") || "0", 10) || 0;
    g_iidEngID = engIdFromDom || engIdFromQuery;

    $(document).off("click.iidExceptionReports", ".js-iid-view-report").on("click.iidExceptionReports", ".js-iid-view-report", function () {
        var reportId = $(this).data("report-id");
        var loanStatus = $(this).data("loan-status");
        var indicator = ($(this).data("indicator") || "").toString();
        var title = ($(this).data("title") || "").toString();
        var desc = ($(this).data("desc") || "").toString();
        viewSample(reportId, loanStatus, indicator, title, desc);
    });

    listSamples();
});

function getReportField(item, keys) {
    for (var i = 0; i < keys.length; i++) {
        var key = keys[i];
        if (item[key] !== undefined && item[key] !== null) {
            return item[key];
        }

        var normalizedKey = key.toLowerCase();
        var itemKeys = Object.keys(item || {});
        for (var j = 0; j < itemKeys.length; j++) {
            if (itemKeys[j].toLowerCase() === normalizedKey) {
                return item[itemKeys[j]];
            }
        }
    }

    return "";
}

function htmlEncode(value) {
    return $("<div/>").text(value == null ? "" : value).html();
}

function listSamples() {
    if (!g_iidEngID) {
        destroyDatatable("listOfSamples");
        $("#listOfSamples tbody").html('<tr><td colspan="5" class="text-center text-muted">Select a complaint to view exception reports.</td></tr>');
        return;
    }

    $.ajax({
        url: g_asiBaseURL + "/ApiCalls/get_iid_list_of_reports",
        type: "POST",
        cache: false,
        data: {
            ENG_ID: g_iidEngID
        },
        success: function (data) {
            if (Array.isArray(data) && data.length > 0) {
                populateTable(data);
            } else {
                destroyDatatable("listOfSamples");
                $("#listOfSamples tbody").html('<tr><td colspan="5" class="text-center">No data found.</td></tr>');
            }
        },
        error: function () {
            destroyDatatable("listOfSamples");
            $("#listOfSamples tbody").html('<tr><td colspan="5" class="text-center text-danger">Failed to load reports.</td></tr>');
        },
        dataType: "json"
    });
}

function populateTable(data) {
    $("#wait").show();
    destroyDatatable("listOfSamples");
    var tableBody = $("#listOfSamples tbody");
    tableBody.empty();

    data.forEach(function (item, index) {
        var reportId = getReportField(item, ["REPORT_ID", "reportId", "reporT_ID", "R_ID"]);
        var loanStatus = getReportField(item, ["LOAN_STATUS", "loanStatus", "loaN_STATUS"]);
        var indicator = getReportField(item, ["REPORT_INDICATOR", "reportIndicator", "reporT_INDICATOR", "IND"]);
        var reportTitle = getReportField(item, ["REPORT_TITLE", "reportTitle", "reporT_TITLE"]);
        var description = getReportField(item, ["DISCRIPTION", "discription", "DESCRIPTION", "description"]);
        var reportingPeriod = getReportField(item, ["ReportingPeriod", "REPORTING_PERIOD", "reportingPeriod"]);

        var row = '<tr>' +
            '<td>' + (index + 1) + '</td>' +
            '<td>' + htmlEncode(reportTitle) + '</td>' +
            '<td class="text-center">' + htmlEncode(description) + '</td>' +
            '<td>' + htmlEncode(reportingPeriod) + '</td>' +
            '<td class="text-center">' +
                '<button type="button" class="btn btn-danger btn-sm js-iid-view-report"' +
                    ' data-report-id="' + htmlEncode(reportId) + '"' +
                    ' data-loan-status="' + htmlEncode(loanStatus) + '"' +
                    ' data-indicator="' + htmlEncode(indicator) + '"' +
                    ' data-title="' + htmlEncode(reportTitle) + '"' +
                    ' data-desc="' + htmlEncode(description) + '">' +
                    'View' +
                '</button>' +
            '</td>' +
            '</tr>';

        tableBody.append(row);
    });

    initializeDataTable("listOfSamples");
    $("#wait").hide();
}

function viewSample(reportId, loanStatus, indicator, reportTitle, reportDescription) {
    if ((indicator || "").toString().toUpperCase() === "A") {
        redirectToAccount(reportId, loanStatus, reportTitle, reportDescription);
    } else if ((indicator || "").toString().toUpperCase() === "L") {
        redirectToLoan(reportId, loanStatus, reportTitle, reportDescription);
    }
}

function redirectToAccount(reportId, loanStatus, title, desc) {
    window.location.href = g_asiBaseURL + "/IID/Account_exception?engId=" + encodeURIComponent(g_iidEngID) + "&report_id=" + encodeURIComponent(reportId || 0) + "&loan_status=" + encodeURIComponent(loanStatus || 0) + "&title=" + encodeURIComponent(title || "") + "&desc=" + encodeURIComponent(desc || "");
}

function redirectToLoan(reportId, loanStatus, title, desc) {
    window.location.href = g_asiBaseURL + "/IID/loans_exception?engId=" + encodeURIComponent(g_iidEngID) + "&report_id=" + encodeURIComponent(reportId || 0) + "&loan_status=" + encodeURIComponent(loanStatus || 0) + "&title=" + encodeURIComponent(title || "") + "&desc=" + encodeURIComponent(desc || "");
}

