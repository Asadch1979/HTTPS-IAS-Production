(function () {
    "use strict";

    $(document).ready(function () {
        loadObservationSummary("OVER_THREE", "overThreeObservationTable");
        loadObservationSummary("ZERO", "zeroCycleObservationTable");

        document.querySelectorAll("#observationCycleTabs [data-bs-toggle='tab']").forEach(function (tab) {
            tab.addEventListener("shown.bs.tab", function () {
                $.fn.dataTable.tables({ visible: true, api: true }).columns.adjust();
            });
        });
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
                    lowRisk: 0
                };

                data.forEach(function (item, index) {
                    appendSummaryRow(tableBody, item, index + 1);
                    totals.totalObservations += item.totalObservations;
                    totals.highRisk += item.highRisk;
                    totals.mediumRisk += item.mediumRisk;
                    totals.lowRisk += item.lowRisk;
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

        var detailsCell = appendCell(row, "", "text-center");
        var detailsButton = document.createElement("button");
        detailsButton.type = "button";
        detailsButton.className = "btn btn-sm btn-outline-primary view-department-paras";
        detailsButton.textContent = "View Para";
        detailsButton.dataset.departmentId = item.departmentId;
        detailsButton.dataset.departmentName = item.departmentName || "";
        detailsButton.dataset.cycleBucket = tableBody.closest("table").id === "zeroCycleObservationTable"
            ? "ZERO"
            : "OVER_THREE";
        detailsCell.appendChild(detailsButton);
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
        appendCell(row, "");
        tableBody.appendChild(row);
    }

    function appendMessageRow(tableBody, message) {
        var row = document.createElement("tr");
        var cell = document.createElement("td");
        cell.colSpan = 7;
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

    $(document).on("click", ".view-department-paras", function () {
        var button = this;
        loadDepartmentParas(
            button.dataset.departmentId,
            button.dataset.departmentName,
            button.dataset.cycleBucket
        );
    });

    function loadDepartmentParas(departmentId, departmentName, cycleBucket) {
        var tableId = "headObservationDetailsTable";
        destroyDatatable(tableId);
        var tableBody = document.querySelector("#" + tableId + " tbody");
        tableBody.replaceChildren();
        document.getElementById("headObservationDetailsTitle").textContent =
            "Department Paras - " + (departmentName || "");
        $("#headObservationDetailsModal").modal("show");

        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_head_observation_risk_details",
            type: "POST",
            data: {
                departmentId: departmentId,
                cycleBucket: cycleBucket
            },
            cache: false,
            success: function (data) {
                tableBody.replaceChildren();
                data.forEach(function (item, index) {
                    var row = document.createElement("tr");
                    appendCell(row, index + 1);
                    appendCell(row, item.auditPeriod);
                    appendCell(row, item.paraNo);
                    appendCell(row, item.paraGist);
                    appendCell(row, item.risk);

                    var actionCell = appendCell(row, "", "text-center");
                    var link = document.createElement("button");
                    link.type = "button";
                    link.className = "btn btn-sm btn-link view-head-para-text";
                    link.textContent = "View Para Text";
                    link.dataset.comId = item.comId;
                    actionCell.appendChild(link);
                    tableBody.appendChild(row);
                });

                if (!data.length) {
                    appendDetailMessageRow(tableBody, "No paras were found for this department.");
                }
                initializeReadOnlyTable(tableId);
            },
            error: function () {
                appendDetailMessageRow(tableBody, "Unable to load department paras.");
            },
            dataType: "json"
        });
    }

    function appendDetailMessageRow(tableBody, message) {
        tableBody.replaceChildren();
        var row = document.createElement("tr");
        var cell = document.createElement("td");
        cell.colSpan = 6;
        cell.className = "text-center text-muted";
        cell.textContent = message;
        row.appendChild(cell);
        tableBody.appendChild(row);
    }

    $(document).on("click", ".view-head-para-text", function () {
        var comId = this.dataset.comId;
        var target = document.getElementById("headParaTextDivField");
        target.innerHTML = '<p class="text-center text-muted">Loading para text...</p>';
        $("#headParaTextViewerModal").modal("show");

        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_all_para_text",
            type: "POST",
            data: { COM_ID: comId },
            cache: false,
            success: function (data) {
                target.innerHTML = data || '<p class="text-center text-muted">No para text available.</p>';
            },
            error: function () {
                target.innerHTML = '<p class="text-center text-danger">Unable to load para text.</p>';
            }
        });
    });
}());
