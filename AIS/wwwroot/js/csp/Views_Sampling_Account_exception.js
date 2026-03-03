    var g_engId = 0;
    var g_rptID = 0;
    $(document).ready(function() {
        var url_string = window.location;
        var url = new URL(url_string);
        g_engId = url.searchParams.get("engId");
        g_rptID  = url.searchParams.get("report_id");
        loadexceptionReport();
    });

    function loadexceptionReport(){
        $("#wait").show();
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_exception_account_report",
            cache: false,
            type: "POST",
            data:{ENG_ID:g_engId, RPT_ID:g_rptID },
            success: function (data) {
                populateDynamicReport(data);
            },
            dataType: "json",
            complete: function(){
                $("#wait").hide();
            },
        });
    }
    function populateDynamicReport(data) {
        if (!data || !Array.isArray(data.columns) || data.columns.length === 0) {
            $("#dynamicHeader").html("");
            $("#dynamicBody").html("<tr><td colspan='10'>No data found</td></tr>");
            destroyDatatable('biomet_sample_list');
            return;
        }

        buildDynamicTable(data);
    }

            function buildDynamicTable(result) {
    destroyDatatable('biomet_sample_list');

    var columns = result.columns || [];
    var rows = result.rows || [];

    // Check if ACCOUNT_NO exists
    var hasAccountColumn = columns.some(function (c) {
        return normalizeColumnName(c).toUpperCase() === "ACCOUNT_NO";
    });
    var actionColumnCount = hasAccountColumn ? 2 : 0;

    // ----------------------------
    // 1. Build Header
    // ----------------------------
    var headerHtml = "<tr>";
    headerHtml += "<th>Sr No</th>";

    columns.forEach(function(col){
        headerHtml += `<th>${normalizeColumnHeader(col)}</th>`;
    });

    if (hasAccountColumn) {
        headerHtml += "<th>Documents</th><th>Transactions</th>";
    }

    headerHtml += "</tr>";
    $("#dynamicHeader").html(headerHtml);

    // ----------------------------
    // 2. No-data case (IMPORTANT)
    // ----------------------------
    if (!rows || rows.length === 0) {
        var emptyColspan = columns.length + 1 + actionColumnCount; // Sr No + columns + action cols
        var bodyHtml = `<tr>
                            <td colspan="${emptyColspan}" class="text-center text-muted">
                                No data found for this report.
                            </td>
                        </tr>`;
        $("#dynamicBody").html(bodyHtml);

        // Do NOT initialize DataTables when there is only the message row
        return;
    }

    // ----------------------------
    // 3. Build Body with data
    // ----------------------------
    var bodyHtml = "";
    rows.forEach(function (row, idx) {
        bodyHtml += "<tr>";
        bodyHtml += `<td>${idx + 1}</td>`;

            columns.forEach(function(col){
                var colName = normalizeColumnName(col);
                var rowKey = colName.toUpperCase();
                var value = row[rowKey];
                if (value === undefined) {
                    value = row[colName];
                }
                var formattedValue = formatCellValue(colName, value ?? "");
                var numericClass = typeof value === "number" ? ` class="text-end"` : "";
                bodyHtml += `<td${numericClass}>${formattedValue}</td>`;
            });

            if (hasAccountColumn) {
                var accountNo = row["ACCOUNT_NO"];
                bodyHtml += `<td><a href="./account_document?engId=${g_engId}&acNo=${accountNo}" class="btn btn-primary btn-sm">Documents</a></td>`;
                bodyHtml += `<td><a href="./account_transaction?engId=${g_engId}&acNo=${accountNo}" class="btn btn-success btn-sm">Transactions</a></td>`;
            }

        bodyHtml += "</tr>";
    });

    $("#dynamicBody").html(bodyHtml);
    initializeDataTable('biomet_sample_list');
}


    function normalizeColumnName(column) {
        var name = column?.columnName ?? column?.ColumnName ?? column?.COLUMN_NAME ?? column?.columN_NAME ?? "";
        return name ? name.toString() : "";
    }

    function normalizeColumnHeader(column) {
        return column?.columnHeader ?? column?.ColumnHeader ?? column?.COLUMN_HEADER ?? column?.columN_HEADER ?? "";
    }

    function formatCellValue(columnName, value) {
        if (value === null || value === undefined) {
            return "";
        }

        return value;
    }
