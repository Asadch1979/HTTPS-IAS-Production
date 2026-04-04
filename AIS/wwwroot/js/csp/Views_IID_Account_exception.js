var g_iidAccountEngId = 0;
var g_iidAccountReportId = 0;

$(document).ready(function () {
    var url = new URL(window.location.href);
    g_iidAccountEngId = url.searchParams.get("engId") || 0;
    g_iidAccountReportId = url.searchParams.get("report_id") || url.searchParams.get("reporT_ID") || 0;
    loadexceptionReport();
});

function loadexceptionReport() {
    $("#wait").show();
    $.ajax({
        url: g_asiBaseURL + "/ApiCalls/get_iid_exception_account_report",
        cache: false,
        type: "POST",
        data: {
            ENG_ID: g_iidAccountEngId,
            RPT_ID: g_iidAccountReportId
        },
        success: function (data) {
            populateDynamicReport(data);
        },
        dataType: "json",
        complete: function () {
            $("#wait").hide();
        }
    });
}

function populateDynamicReport(data) {
    if (!data || !Array.isArray(data.columns) || data.columns.length === 0) {
        $("#dynamicHeader").html("");
        $("#dynamicBody").html("<tr><td colspan='10'>No data found</td></tr>");
        destroyDatatable("biomet_sample_list");
        return;
    }

    buildDynamicTable(data);
}

function buildDynamicTable(result) {
    destroyDatatable("biomet_sample_list");

    var columns = result.columns || [];
    var rows = result.rows || [];
    var hasAccountColumn = columns.some(function (column) {
        return normalizeColumnName(column).toUpperCase() === "ACCOUNT_NO";
    });
    var actionColumnCount = hasAccountColumn ? 2 : 0;

    var headerHtml = "<tr><th>Sr No</th>";
    columns.forEach(function (column) {
        headerHtml += "<th>" + normalizeColumnHeader(column) + "</th>";
    });

    if (hasAccountColumn) {
        headerHtml += "<th>Documents</th><th>Transactions</th>";
    }

    headerHtml += "</tr>";
    $("#dynamicHeader").html(headerHtml);

    if (!rows.length) {
        var emptyColspan = columns.length + 1 + actionColumnCount;
        $("#dynamicBody").html("<tr><td colspan='" + emptyColspan + "' class='text-center text-muted'>No data found for this report.</td></tr>");
        return;
    }

    var bodyHtml = "";
    rows.forEach(function (row, index) {
        bodyHtml += "<tr><td>" + (index + 1) + "</td>";

        columns.forEach(function (column) {
            var colName = normalizeColumnName(column);
            var rowKey = colName.toUpperCase();
            var value = row[rowKey];
            if (value === undefined) {
                value = row[colName];
            }
            var formattedValue = formatCellValue(colName, value == null ? "" : value);
            var numericClass = typeof value === "number" ? " class='text-end'" : "";
            bodyHtml += "<td" + numericClass + ">" + formattedValue + "</td>";
        });

        if (hasAccountColumn) {
            var accountNo = row.ACCOUNT_NO || row.account_no || row.accountNo || "";
            bodyHtml += "<td><a href='./account_document?engId=" + encodeURIComponent(g_iidAccountEngId) + "&acNo=" + encodeURIComponent(accountNo) + "' class='btn btn-primary btn-sm'>Documents</a></td>";
            bodyHtml += "<td><a href='./account_transaction?engId=" + encodeURIComponent(g_iidAccountEngId) + "&acNo=" + encodeURIComponent(accountNo) + "' class='btn btn-success btn-sm'>Transactions</a></td>";
        }

        bodyHtml += "</tr>";
    });

    $("#dynamicBody").html(bodyHtml);
    initializeDataTable("biomet_sample_list");
}

function normalizeColumnName(column) {
    var name = column && (column.columnName || column.ColumnName || column.COLUMN_NAME || column.columN_NAME) || "";
    return name ? name.toString() : "";
}

function normalizeColumnHeader(column) {
    return column && (column.columnHeader || column.ColumnHeader || column.COLUMN_HEADER || column.columN_HEADER) || "";
}

function formatCellValue(columnName, value) {
    if (value === null || value === undefined) {
        return "";
    }

    return value;
}

