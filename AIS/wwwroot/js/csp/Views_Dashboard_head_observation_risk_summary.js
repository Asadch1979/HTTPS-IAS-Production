(function () {
    "use strict";

    $(document).ready(function () {
        loadObservationSummary("OVER_THREE", "overThreeObservationTable");
        loadObservationSummary("ZERO", "zeroCycleObservationTable");
    });

    function loadObservationSummary(cycleBucket, tableId) {
        destroyDatatable(tableId);
        var tableBody = document.querySelector("#" + tableId + " tbody");
        tableBody.replaceChildren();

        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_head_observation_risk_summary",
            type: "POST",
            data: { cycleBucket: cycleBucket },
            cache: false,
            success: function (data) {
                var totals = {
                    totalObservations: 0,
                    highRisk: 0,
                    mediumRisk: 0,
                    lowRisk: 0,
                    unratedRisk: 0
                };

                data.forEach(function (item, index) {
                    appendSummaryRow(tableBody, item, index + 1);
                    totals.totalObservations += item.totalObservations;
                    totals.highRisk += item.highRisk;
                    totals.mediumRisk += item.mediumRisk;
                    totals.lowRisk += item.lowRisk;
                    totals.unratedRisk += item.unratedRisk;
                });

                appendTotalRow(tableBody, totals);
                initializeReadOnlyTable(tableId);
            },
            error: function () {
                appendMessageRow(tableBody, "Unable to load the observation summary.");
            },
            dataType: "json"
        });
    }

    function appendSummaryRow(tableBody, item, serialNumber) {
        var row = document.createElement("tr");
        appendCell(row, serialNumber);
        appendCell(row, item.departmentName);
        appendCell(row, item.totalObservations, "text-end");
        appendCell(row, item.highRisk, "text-end", "#ff968f");
        appendCell(row, item.mediumRisk, "text-end", "#f9e10a6b");
        appendCell(row, item.lowRisk, "text-end", "#82f386");
        appendCell(row, item.unratedRisk, "text-end");

        var statusCell = appendCell(row, item.riskStatus, "text-center");
        statusCell.classList.add(getRiskStatusClass(item.riskStatus));
        tableBody.appendChild(row);
    }

    function appendTotalRow(tableBody, totals) {
        var row = document.createElement("tr");
        appendCell(row, "");
        var labelCell = appendCell(row, "Total");
        labelCell.classList.add("fw-bold");
        appendCell(row, totals.totalObservations, "text-end fw-bold");
        appendCell(row, totals.highRisk, "text-end fw-bold");
        appendCell(row, totals.mediumRisk, "text-end fw-bold");
        appendCell(row, totals.lowRisk, "text-end fw-bold");
        appendCell(row, totals.unratedRisk, "text-end fw-bold");
        appendCell(row, "");
        tableBody.appendChild(row);
    }

    function appendMessageRow(tableBody, message) {
        var row = document.createElement("tr");
        var cell = document.createElement("td");
        cell.colSpan = 8;
        cell.className = "text-center text-muted";
        cell.textContent = message;
        row.appendChild(cell);
        tableBody.appendChild(row);
    }

    function appendCell(row, value, className, backgroundColor) {
        var cell = document.createElement("td");
        cell.textContent = value === null || value === undefined ? "" : value;
        if (className) {
            className.split(" ").forEach(function (cssClass) {
                if (cssClass) {
                    cell.classList.add(cssClass);
                }
            });
        }
        if (backgroundColor) {
            cell.style.backgroundColor = backgroundColor;
        }
        row.appendChild(cell);
        return cell;
    }

    function getRiskStatusClass(riskStatus) {
        switch ((riskStatus || "").toUpperCase()) {
            case "HIGH":
                return "table-danger";
            case "MEDIUM":
                return "table-warning";
            case "LOW":
                return "table-success";
            default:
                return "table-secondary";
        }
    }

    function initializeReadOnlyTable(tableId) {
        $("#" + tableId).DataTable({
            dom: '<"top"lf>rt<"bottom"ip><"clear">',
            autoWidth: true,
            ordering: false,
            lengthMenu: [
                [10, 50, 100, -1],
                [10, 50, 100, "All"]
            ]
        });
    }
}());
