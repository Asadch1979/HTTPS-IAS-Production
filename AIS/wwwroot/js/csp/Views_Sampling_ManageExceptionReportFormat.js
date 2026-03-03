    var g_engID = 0;
    var editingFormatId = null;
    var isEditMode = false;

    $(document).ready(function () {
        var urlParams = new URLSearchParams(window.location.search);
        g_engID = urlParams.get("engId") || 0;

        CommonValidation.attachAlnumOnly("#txtHeader", { allowSpace: true, maxLen: 100 });

        loadReports();

        $("#ddlReports").on("change", function () {
            if (this.value) {
                loadFormat();
            } else {
                $("#tblFormat tbody").empty();
            }
        });

        $("#btnSave").on("click", function () {
            saveFormat();
        });

        $("#btnCancelEdit").on("click", function () {
            clearInputs();
        });

        $("#tblFormat").on("click", ".btn-edit-format", function () {
            var rowData = $(this).data("row");
            if (!rowData) return;
            beginEdit(rowData);
        });
    });

    function beginEdit(rowData) {
        isEditMode = true;
        editingFormatId = rowData.FormatId || 0;

        $("#ddlColumnName").val(rowData.ColumnName).prop("disabled", true);
        $("#txtHeader").val(rowData.ColumnHeader);
        $("#txtOrder").val(rowData.ColumnOrder);
        $("#ddlType").val(rowData.DataType);
        $("#ddlStatus").val(rowData.IsActive || "Y");

        $("#btnSave").text("Update");
        $("#btnCancelEdit").removeClass("d-none");
    }

    function loadReports() {
        $("#ddlReports").empty().append('<option value="">-- Select Report --</option>');

        $.post(g_asiBaseURL + "/ApiCalls/get_list_of_reports", { ENG_ID: g_engID }, function (data) {
            data.forEach(function (item) {
                $("#ddlReports").append(`<option value="${item.reporT_ID}">${item.reporT_TITLE}</option>`);
            });
        }).done(function () {
            if ($("#ddlReports").val()) loadFormat();
        });
    }

    function loadFormat() {
        var rpt = $("#ddlReports").val();
        if (!rpt) return;

        $.post(g_asiBaseURL + "/ApiCalls/get_exception_report_format", { report_id: rpt }, function (data) {
            var tbody = $("#tblFormat tbody");
            tbody.empty();

            if (!data || data.length === 0) {
                tbody.append('<tr><td colspan="6" class="text-center">No columns found.</td></tr>');
                return;
            }

            data.forEach(function (item) {
                var rowData = normalizeFormatRow(item);
                var statusText = rowData.IsActive === "N" ? "Inactive" : "Active";

                var row = $("<tr>");
                row.append($("<td>").text(rowData.ColumnName));
                row.append($("<td>").text(rowData.ColumnHeader));
                row.append($("<td>").text(rowData.ColumnOrder));
                row.append($("<td>").text(rowData.DataType));
                row.append($("<td>").text(statusText));

                var editBtn = $("<button>")
                    .addClass("btn btn-sm btn-primary btn-edit-format")
                    .attr("type", "button")
                    .text("Update")
                    .data("row", rowData);

                row.append($("<td>").append(editBtn));
                tbody.append(row);
            });
        });
    }

    function saveFormat() {
        var reportId = $("#ddlReports").val();
        if (!reportId) {
            alert("Please select a report first.");
            return;
        }

        if (!CommonValidation.isAlnumOk("#txtHeader", { allowSpace: true, required: true })) {
            alert("Header must contain only letters, numbers, and spaces.");
            return;
        }

        var headerValue = $("#txtHeader").val();

        var model = {
            ReportId: reportId,
            ColumnName: $("#ddlColumnName").val(),
            ColumnHeader: headerValue,
            ColumnOrder: parseInt($("#txtOrder").val() || "0", 10),
            DataType: $("#ddlType").val(),
            IsActive: $("#ddlStatus").val()
        };

        if (isEditMode) {
            model.FormatId = editingFormatId;
        }

        var url = isEditMode
            ? "/ApiCalls/update_exception_report_format"
            : "/ApiCalls/save_exception_report_format";

        $.ajax({
            url: g_asiBaseURL + url,
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(model),
            success: function () {
                clearInputs();
                loadFormat();
            },
            error: function (xhr) {
                showApiAlertFromXhr(xhr, xhr ? xhr.status : null, getErrorReferenceIdFromXhr(xhr), "Unable to save format. Please try again.");
            }
        });
    }

    function clearInputs() {
        isEditMode = false;
        editingFormatId = null;

        $("#ddlColumnName").prop("selectedIndex", 0).prop("disabled", false);
        $("#txtHeader").val("");
        $("#txtOrder").val("");
        $("#ddlType").val("TEXT");
        $("#ddlStatus").val("Y");

        $("#btnSave").text("Add");
        $("#btnCancelEdit").addClass("d-none");
    }

    function normalizeFormatRow(item) {
        // Handle PascalCase, camelCase, and legacy UPPER_CASE / weird columN_NAME etc.
        var orderValue = coalesce(
            item.ColumnOrder,
            item.columnOrder,
            item.COLUMN_ORDER,
            item.columN_ORDER,
            0
        );
        var parsedOrder = parseInt(orderValue, 10);

        return {
            FormatId: coalesce(item.FormatId, item.formatId, item.FORMAT_ID),
            ReportId: coalesce(item.ReportId, item.reportId, item.R_ID),
            ColumnName: coalesce(item.ColumnName, item.columnName, item.COLUMN_NAME, item.columN_NAME, ""),
            ColumnHeader: coalesce(item.ColumnHeader, item.columnHeader, item.COLUMN_HEADER, item.columN_HEADER, ""),
            ColumnOrder: isNaN(parsedOrder) ? 0 : parsedOrder,
            DataType: coalesce(item.DataType, item.dataType, item.DATA_TYPE, item.datA_TYPE, ""),
            IsActive: coalesce(item.IsActive, item.isActive, item.IS_ACTIVE, "Y")
        };
    }

    function coalesce() {
        for (var i = 0; i < arguments.length; i++) {
            var value = arguments[i];
            if (value !== undefined && value !== null && value !== "") return value;
        }
        return null;
    }
