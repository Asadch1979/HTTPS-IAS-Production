var exceptionReportPage = {
    engId: 0,
    preferredReportId: 0,
    reports: [],
    selectedReportId: 0,
    reportMode: "add",
    formatMode: "add",
    editingFormatId: 0,
    lastLoanStatusBeforeAccount: ""
};

$(document).ready(function () {
    var url = new URL(window.location.href);
    exceptionReportPage.engId = parseInt(url.searchParams.get("engId"), 10) || 0;
    exceptionReportPage.preferredReportId = parseInt(url.searchParams.get("reportId") || url.searchParams.get("report_id"), 10) || 0;

    CommonValidation.attachAlnumOnly("#reportName", { allowAmp: true, allowQuestion: true, maxLen: 100 });
    CommonValidation.attachAlnumOnly("#txtHeader", { allowAmp: true, allowQuestion: true, maxLen: 100 });

    ensureAccountLoanStatusOption();
    bindPageEvents();
    resetReportForm(false);
    setSelectedReport(null, { clearMasterForm: false });
    loadReports(exceptionReportPage.preferredReportId ? { reportId: exceptionReportPage.preferredReportId } : null);
});

function bindPageEvents() {
    $("#reportType").on("change", handleReportTypeChange);
    $("#btnSaveReport").on("click", saveReport);
    $("#btnCancelReportEdit").on("click", function () {
        resetReportForm(false);
    });
    $("#btnResetReportForm").on("click", function () {
        resetReportForm(true);
    });

    $("#btnSaveFormat").on("click", saveFormat);
    $("#btnCancelFormatEdit").on("click", clearFormatForm);

    $("#reportList").on("click", ".btn-report-edit", function () {
        var reportId = parseInt($(this).data("reportId"), 10) || 0;
        if (!reportId) {
            return;
        }

        selectReportById(reportId, { loadIntoMasterForm: true });
    });

    $("#tblFormat").on("click", ".btn-edit-format", function () {
        var rowData = $(this).data("row");
        if (!rowData) {
            return;
        }

        beginFormatEdit(rowData);
    });
}

function loadReports(selectionHint) {
    $.ajax({
        url: g_asiBaseURL + "/ApiCalls/get_list_of_reports",
        type: "POST",
        cache: false,
        data: { ENG_ID: exceptionReportPage.engId },
        dataType: "json",
        success: function (data) {
            exceptionReportPage.reports = normalizeReportList(data);
            var reportIdToSelect = resolveSelectedReportId(selectionHint);
            exceptionReportPage.selectedReportId = reportIdToSelect || 0;
            renderReportTable(exceptionReportPage.reports);

            if (reportIdToSelect) {
                selectReportById(reportIdToSelect, { loadIntoMasterForm: true, skipTableRender: true });
                return;
            }

            setSelectedReport(null, { clearMasterForm: false });
        },
        error: function (xhr) {
            exceptionReportPage.reports = [];
            renderReportTable([]);
            setSelectedReport(null, { clearMasterForm: false });
            showApiAlertFromXhr(xhr, xhr ? xhr.status : null, getErrorReferenceIdFromXhr(xhr), "Unable to load reports.");
        }
    });
}

function normalizeReportList(data) {
    if (!Array.isArray(data)) {
        return [];
    }

    return data.map(function (item) {
        return normalizeReportRow(item);
    });
}

function renderReportTable(data) {
    destroyDatatable("reportList");

    var tbody = $("#reportList tbody");
    tbody.empty();

    if (!data || data.length === 0) {
        tbody.append('<tr><td colspan="6" class="text-center">No reports found.</td></tr>');
        return;
    }

    data.forEach(function (item, index) {
        var isSelected = exceptionReportPage.selectedReportId === item.ReportId;
        var row = $("<tr>").toggleClass("table-active", isSelected);

        row.append($("<td>").text(index + 1));
        row.append($("<td>").text(item.ReportTitle));
        row.append($("<td>").text(item.Description));
        row.append($("<td>").text(getReportTypeLabel(item.ReportIndicator)));
        row.append($("<td>").text(getLoanStatusLabel(item.LoanStatus)));

        var editButton = $("<button>")
            .addClass("btn btn-primary btn-sm btn-report-edit")
            .attr("type", "button")
            .attr("data-report-id", item.ReportId)
            .text(isSelected ? "Editing" : "Edit");

        row.append($("<td>").addClass("text-center").append(editButton));
        tbody.append(row);
    });

    initializeDataTable("reportList");
}

function resolveSelectedReportId(selectionHint) {
    var currentId = exceptionReportPage.selectedReportId;
    if (selectionHint && selectionHint.reportId && hasReport(selectionHint.reportId)) {
        return selectionHint.reportId;
    }

    if (currentId && hasReport(currentId)) {
        return currentId;
    }

    if (!selectionHint) {
        return 0;
    }

    var matchedReport = exceptionReportPage.reports.find(function (item) {
        return normalizeText(item.ReportTitle) === normalizeText(selectionHint.reportTitle)
            && normalizeText(item.Description) === normalizeText(selectionHint.Description)
            && String(item.ReportIndicator || "") === String(selectionHint.ReportIndicator || "")
            && String(item.LoanStatus || "") === String(selectionHint.LoanStatus || "");
    });

    return matchedReport ? matchedReport.ReportId : 0;
}

function hasReport(reportId) {
    return exceptionReportPage.reports.some(function (item) {
        return item.ReportId === reportId;
    });
}

function selectReportById(reportId, options) {
    var settings = $.extend({ loadIntoMasterForm: true, skipTableRender: false }, options);
    var report = getReportById(reportId);

    if (!report) {
        setSelectedReport(null, { clearMasterForm: false });
        return;
    }

    setSelectedReport(report, { clearMasterForm: false });

    if (settings.loadIntoMasterForm) {
        populateReportForm(report);
    }

    clearFormatForm();
    loadFormat(report.ReportId);

    if (!settings.skipTableRender) {
        renderReportTable(exceptionReportPage.reports);
    }
}

function setSelectedReport(report, options) {
    var settings = $.extend({ clearMasterForm: false }, options);

    if (!report) {
        exceptionReportPage.selectedReportId = 0;
        updateSelectedReportLabels(null);
        toggleFormatSection(false);
        renderNoFormatRows("Save or select a report above to manage its format.");

        if (settings.clearMasterForm) {
            resetReportForm(false);
        }
        return;
    }

    exceptionReportPage.selectedReportId = report.ReportId;
    updateSelectedReportLabels(report);
    toggleFormatSection(true);
}

function updateSelectedReportLabels(report) {
    if (!report) {
        $("#selectedReportBadge").text("No report selected");
        $("#selectedFormatReportName").text("No report selected");
        $("#formatSectionHint").text("Save or select a report above to manage its format.");
        return;
    }

    $("#selectedReportBadge").text("Selected: " + report.ReportTitle);
    $("#selectedFormatReportName").text(report.ReportTitle);
    $("#formatSectionHint").text("Managing format for the selected report.");
}

function toggleFormatSection(enabled) {
    $("#formatSectionFieldset").prop("disabled", !enabled);
}

function populateReportForm(report) {
    exceptionReportPage.reportMode = "edit";

    $("#reportFormTitle").text("Update Report");
    $("#reportName").val(report.ReportTitle || "");
    $("#thingsCheck").val(report.Description || "");
    $("#reportType").val(report.ReportIndicator || "L");
    $("#loanStatus").val(String(report.LoanStatus || ""));

    syncLoanStatusState();
    $("#btnSaveReport").text("Update Report");
    $("#btnCancelReportEdit").removeClass("d-none");
}

function resetReportForm(clearSelection) {
    exceptionReportPage.reportMode = "add";
    exceptionReportPage.lastLoanStatusBeforeAccount = getDefaultLoanStatusValue();

    $("#reportFormTitle").text("Create Report");
    $("#reportName").val("");
    $("#thingsCheck").val("");
    $("#reportType").val("L");
    $("#loanStatus").val(getDefaultLoanStatusValue());

    syncLoanStatusState();
    $("#btnSaveReport").text("Save Report");
    $("#btnCancelReportEdit").addClass("d-none");

    if (clearSelection) {
        setSelectedReport(null, { clearMasterForm: false });
        renderReportTable(exceptionReportPage.reports);
    }
}

function handleReportTypeChange() {
    syncLoanStatusState();
}

function syncLoanStatusState() {
    var isAccountReport = $("#reportType").val() === "A";
    var loanStatus = $("#loanStatus");

    if (isAccountReport) {
        var currentValue = loanStatus.val();
        if (currentValue && currentValue !== "1") {
            exceptionReportPage.lastLoanStatusBeforeAccount = currentValue;
        }

        loanStatus.val("1");
        loanStatus.prop("disabled", true);
        return;
    }

    loanStatus.prop("disabled", false);

    if (!loanStatus.val() || loanStatus.val() === "1") {
        loanStatus.val(exceptionReportPage.lastLoanStatusBeforeAccount || getDefaultLoanStatusValue());
    }
}

function ensureAccountLoanStatusOption() {
    var loanStatus = $("#loanStatus");
    if (loanStatus.find('option[value="1"]').length === 0) {
        loanStatus.prepend('<option value="1">Account</option>');
    }
}

function getDefaultLoanStatusValue() {
    var nonAccountOption = $("#loanStatus option").filter(function () {
        return String($(this).val()) !== "1";
    }).first().val();

    if (nonAccountOption) {
        return String(nonAccountOption);
    }

    var firstOption = $("#loanStatus option:first").val();
    return firstOption ? String(firstOption) : "1";
}

function saveReport() {
    if (!CommonValidation.isAlnumOk("#reportName", { allowAmp: true, allowQuestion: true, required: true })) {
        alert("Report Name must contain only letters, numbers, spaces, ampersand (&), comma (,), and question mark (?).");
        return;
    }

    var reportPayload = collectReportPayload();
    var selectionHint = {
        reportId: exceptionReportPage.reportMode === "edit" ? exceptionReportPage.selectedReportId : 0,
        reportTitle: reportPayload.REPORT_TITLE,
        Description: reportPayload.DESCRIPTION,
        ReportIndicator: reportPayload.TYPE,
        LoanStatus: String(reportPayload.LOAN_STATUS_ID || "")
    };

    $.ajax({
        url: g_asiBaseURL + "/ApiCalls/add_exception_account_report",
        type: "POST",
        data: reportPayload,
        success: function (data) {
            if (!isSuccessfulResponse(data)) {
                showApiAlert(data, "Unable to save report. Please check your input.");
                return;
            }

            var successMessage = exceptionReportPage.reportMode === "edit"
                ? "Report updated successfully."
                : "Report saved successfully.";

            loadReports(selectionHint);
            showApiAlert(data, successMessage);
        },
        error: function (xhr) {
            showApiAlertFromXhr(xhr, xhr ? xhr.status : null, getErrorReferenceIdFromXhr(xhr), "Unable to save report. Please check your input.");
        }
    });
}

function collectReportPayload() {
    return {
        IND: exceptionReportPage.reportMode === "edit" ? "U" : "A",
        REPORT_ID: exceptionReportPage.reportMode === "edit" ? exceptionReportPage.selectedReportId : 0,
        REPORT_TITLE: $("#reportName").val().trim(),
        DESCRIPTION: $("#thingsCheck").val().trim(),
        TYPE: $("#reportType").val(),
        LOAN_STATUS_ID: $("#reportType").val() === "A" ? 1 : ($("#loanStatus").val() || 0)
    };
}

function loadFormat(reportId) {
    if (!reportId) {
        renderNoFormatRows("Save or select a report above to manage its format.");
        return;
    }

    $.ajax({
        url: g_asiBaseURL + "/ApiCalls/get_exception_report_format",
        type: "GET",
        cache: false,
        dataType: "json",
        data: { report_id: reportId },
        success: function (data) {
            renderFormatTable(Array.isArray(data) ? data.map(normalizeFormatRow) : []);
        },
        error: function (xhr) {
            renderNoFormatRows("Unable to load format rows.");
            showApiAlertFromXhr(xhr, xhr ? xhr.status : null, getErrorReferenceIdFromXhr(xhr), "Unable to load report format.");
        }
    });
}

function renderFormatTable(data) {
    var tbody = $("#tblFormat tbody");
    tbody.empty();

    if (!data || data.length === 0) {
        renderNoFormatRows("No format rows found for the selected report.");
        return;
    }

    data.forEach(function (item) {
        var row = $("<tr>");
        row.append($("<td>").text(item.ColumnName));
        row.append($("<td>").text(item.ColumnHeader));
        row.append($("<td>").text(item.ColumnOrder));
        row.append($("<td>").text(item.DataType));
        row.append($("<td>").text(item.IsActive === "N" ? "Inactive" : "Active"));

        var editButton = $("<button>")
            .addClass("btn btn-sm btn-primary btn-edit-format")
            .attr("type", "button")
            .text("Edit")
            .data("row", item);

        row.append($("<td>").addClass("text-center").append(editButton));
        tbody.append(row);
    });
}

function saveFormat() {
    if (!exceptionReportPage.selectedReportId) {
        alert("Please save or select a report first.");
        return;
    }

    if (!CommonValidation.isAlnumOk("#txtHeader", { allowAmp: true, allowQuestion: true, required: true })) {
        alert("Column Header must contain only letters, numbers, spaces, ampersand (&), comma (,), and question mark (?).");
        return;
    }

    var orderValue = parseInt($("#txtOrder").val(), 10);
    if (isNaN(orderValue) || orderValue < 1) {
        alert("Column Order must be greater than zero.");
        return;
    }

    var model = {
        ReportId: exceptionReportPage.selectedReportId,
        ColumnName: $("#ddlColumnName").val(),
        ColumnHeader: $("#txtHeader").val().trim(),
        ColumnOrder: orderValue,
        DataType: $("#ddlType").val(),
        IsActive: $("#ddlStatus").val()
    };

    if (exceptionReportPage.formatMode === "edit") {
        model.FormatId = exceptionReportPage.editingFormatId;
    }

    $.ajax({
        url: g_asiBaseURL + (exceptionReportPage.formatMode === "edit"
            ? "/ApiCalls/update_exception_report_format"
            : "/ApiCalls/save_exception_report_format"),
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(model),
        success: function (data) {
            if (!isSuccessfulSaveResponse(data)) {
                showApiAlert(data, "Unable to save format. Please try again.");
                return;
            }

            var successMessage = exceptionReportPage.formatMode === "edit"
                ? "Format updated successfully."
                : "Format added successfully.";

            clearFormatForm();
            loadFormat(exceptionReportPage.selectedReportId);
            showApiAlert(data, successMessage);
        },
        error: function (xhr) {
            showApiAlertFromXhr(xhr, xhr ? xhr.status : null, getErrorReferenceIdFromXhr(xhr), "Unable to save format. Please try again.");
        }
    });
}

function beginFormatEdit(rowData) {
    exceptionReportPage.formatMode = "edit";
    exceptionReportPage.editingFormatId = rowData.FormatId || 0;

    $("#ddlColumnName").val(rowData.ColumnName).prop("disabled", true);
    $("#txtHeader").val(rowData.ColumnHeader || "");
    $("#txtOrder").val(rowData.ColumnOrder || "");
    $("#ddlType").val(rowData.DataType || "TEXT");
    $("#ddlStatus").val(rowData.IsActive || "Y");

    $("#btnSaveFormat").text("Update");
    $("#btnCancelFormatEdit").removeClass("d-none");
}

function clearFormatForm() {
    exceptionReportPage.formatMode = "add";
    exceptionReportPage.editingFormatId = 0;

    $("#ddlColumnName").prop("disabled", false).prop("selectedIndex", 0);
    $("#txtHeader").val("");
    $("#txtOrder").val("");
    $("#ddlType").val("TEXT");
    $("#ddlStatus").val("Y");

    $("#btnSaveFormat").text("Add");
    $("#btnCancelFormatEdit").addClass("d-none");
}

function renderNoFormatRows(message) {
    $("#tblFormat tbody").html('<tr><td colspan="6" class="text-center">' + message + "</td></tr>");
}

function getReportById(reportId) {
    return exceptionReportPage.reports.find(function (item) {
        return item.ReportId === reportId;
    }) || null;
}

function normalizeReportRow(item) {
    return {
        ReportId: parseInt(coalesce(item.reporT_ID, item.reportId, item.REPORT_ID, item.R_ID, 0), 10) || 0,
        ReportTitle: String(coalesce(item.reporT_TITLE, item.reportTitle, item.REPORT_TITLE, "") || ""),
        Description: String(coalesce(item.discription, item.description, item.DISCRIPTION, item.DESCRIPTION, "") || ""),
        ReportIndicator: String(coalesce(item.reporT_INDICATOR, item.reportIndicator, item.REPORT_INDICATOR, item.ind, item.IND, "L") || "L"),
        LoanStatus: String(coalesce(item.loaN_STATUS, item.loanStatus, item.LOAN_STATUS, "0") || "0")
    };
}

function normalizeFormatRow(item) {
    var orderValue = coalesce(item.ColumnOrder, item.columnOrder, item.COLUMN_ORDER, item.columN_ORDER, 0);
    var parsedOrder = parseInt(orderValue, 10);

    return {
        FormatId: parseInt(coalesce(item.FormatId, item.formatId, item.FORMAT_ID, 0), 10) || 0,
        ReportId: parseInt(coalesce(item.ReportId, item.reportId, item.R_ID, 0), 10) || 0,
        ColumnName: String(coalesce(item.ColumnName, item.columnName, item.COLUMN_NAME, item.columN_NAME, "") || ""),
        ColumnHeader: String(coalesce(item.ColumnHeader, item.columnHeader, item.COLUMN_HEADER, item.columN_HEADER, "") || ""),
        ColumnOrder: isNaN(parsedOrder) ? 0 : parsedOrder,
        DataType: String(coalesce(item.DataType, item.dataType, item.DATA_TYPE, item.datA_TYPE, "TEXT") || "TEXT"),
        IsActive: String(coalesce(item.IsActive, item.isActive, item.IS_ACTIVE, "Y") || "Y")
    };
}

function getReportTypeLabel(reportIndicator) {
    return String(reportIndicator || "").toUpperCase() === "A" ? "Account" : "Loan";
}

function getLoanStatusLabel(loanStatusValue) {
    var option = $("#loanStatus option[value='" + loanStatusValue + "']");
    if (option.length > 0) {
        return option.text();
    }

    return loanStatusValue || "";
}

function normalizeText(value) {
    return String(value || "").trim().toLowerCase();
}

function isSuccessfulResponse(payload) {
    if (!payload) {
        return false;
    }

    if (typeof payload === "string") {
        return payload.trim().length > 0;
    }

    var statusValue = coalesce(payload.Status, payload.status, payload.Success, payload.success, payload.ok);
    return isTruthyValue(statusValue);
}

function isSuccessfulSaveResponse(payload) {
    if (!payload) {
        return false;
    }

    if (typeof payload === "string") {
        return payload.trim().length > 0;
    }

    var statusValue = coalesce(payload.Status, payload.status, payload.Success, payload.success, payload.ok);
    if (statusValue !== null) {
        return isTruthyValue(statusValue);
    }

    var messageValue = coalesce(payload.Message, payload.message);
    return typeof messageValue === "string" && messageValue.trim().length > 0;
}

function isTruthyValue(value) {
    if (typeof value === "boolean") {
        return value;
    }

    if (typeof value === "number") {
        return value > 0;
    }

    if (typeof value === "string") {
        var normalized = value.trim().toLowerCase();
        var numericValue = parseInt(normalized, 10);
        if (!isNaN(numericValue)) {
            return numericValue > 0;
        }

        return normalized === "true"
            || normalized === "1"
            || normalized === "y"
            || normalized === "yes"
            || normalized === "success"
            || normalized === "ok";
    }

    return false;
}

function coalesce() {
    for (var i = 0; i < arguments.length; i++) {
        var value = arguments[i];
        if (value !== undefined && value !== null && value !== "") {
            return value;
        }
    }

    return null;
}
