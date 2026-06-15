$(document).ready(function () {
    var page = $(".para-detail-page");
    if (!page.length) {
        return;
    }

    $("#btnLoadParaDetailGrid").on("click", loadParaDetailedGrid);
    $("#paraDetailYearFilter").on("change", function () {
        setParaDetailMessage("info", "Select an ARPSE Year and load paras.");
        $("#paraDetailGridContainer").addClass("d-none");
        destroyParaDetailedTable();
        $("#tblParaDetailedView tbody").empty();
    });
    $("#tblParaDetailedView").on("click", ".btn-view-para-detail", function () {
        loadParaDetailedModal(parseInt($(this).data("arpse-id"), 10) || 0);
    });
});

function loadParaDetailedGrid() {
    var selectedYear = parseInt($("#paraDetailYearFilter").val(), 10);
    if (isNaN(selectedYear) || selectedYear <= 0) {
        setParaDetailMessage("warning", "Please select ARPSE Year.");
        $("#paraDetailGridContainer").addClass("d-none");
        return;
    }

    setParaDetailMessage("info", "Loading paras...");
    $("#btnLoadParaDetailGrid").prop("disabled", true);
    $.ajax({
        url: String($(".para-detail-page").data("grid-url") || ""),
        type: "GET",
        cache: false,
        data: { arpseYear: selectedYear },
        success: function (data) {
            var rows = Array.isArray(data) ? data.map(normalizeParaDetailedGridRow) : [];
            renderParaDetailedGrid(rows);
            if (!rows.length) {
                setParaDetailMessage("warning", "No record found");
                $("#paraDetailGridContainer").addClass("d-none");
                return;
            }

            $("#paraDetailGridContainer").removeClass("d-none");
            setParaDetailMessage("success", rows.length + " record(s) loaded.");
        },
        error: function (xhr) {
            renderParaDetailedGrid([]);
            $("#paraDetailGridContainer").addClass("d-none");
            setParaDetailMessage("danger", resolveParaDetailAjaxMessage(xhr, "Unable to load paras."));
        },
        complete: function () {
            $("#btnLoadParaDetailGrid").prop("disabled", false);
        }
    });
}

function renderParaDetailedGrid(rows) {
    destroyParaDetailedTable();
    var tbody = $("#tblParaDetailedView tbody");
    tbody.empty();

    rows.forEach(function (row) {
        var viewButton = $("<button>").addClass("btn btn-sm btn-primary btn-view-para-detail").attr("type", "button").text("View").attr("data-arpse-id", row.ArpseId);
        var tr = $("<tr>");
        tr.append($("<td>").text(row.ParaNo));
        tr.append($("<td>").addClass("para-detail-title-cell").text(row.ParaTitle));
        tr.append($("<td>").addClass("text-center").append(viewButton));
        tbody.append(tr);
    });

    if ($.fn.DataTable && rows.length) {
        $("#tblParaDetailedView").DataTable({
            autoWidth: false,
            ordering: true,
            columns: [
                { width: "14%" },
                { width: "72%" },
                { width: "14%", orderable: false }
            ]
        });
    }
}

function loadParaDetailedModal(arpseId) {
    if (!arpseId) {
        alert("ARPSE para is required.");
        return;
    }

    $("#paraDetailModalTitle").text("Para Detail");
    $("#paraDetailModalSubtitle").text("");
    $("#paraDetailModalContent").html('<div class="alert alert-info mb-0">Loading para detail...</div>');
    bootstrap.Modal.getOrCreateInstance(document.getElementById("paraDetailModal")).show();

    $.ajax({
        url: String($(".para-detail-page").data("detail-url") || ""),
        type: "GET",
        cache: false,
        data: { arpseId: arpseId },
        success: function (data) {
            renderParaDetailedModal(data || {});
        },
        error: function (xhr) {
            $("#paraDetailModalContent").html('<div class="alert alert-warning mb-0">' + escapeParaDetailHtml(resolveParaDetailAjaxMessage(xhr, "No record found")) + '</div>');
        }
    });
}

function renderParaDetailedModal(detail) {
    var paraNo = String(resolveParaDetailValue(detail, "ParaNo", "paraNo") || "");
    var paraTitle = String(resolveParaDetailValue(detail, "ParaTitle", "paraTitle") || "");
    var sections = resolveParaDetailValue(detail, "Sections", "sections") || [];

    $("#paraDetailModalTitle").text(paraNo ? "Para " + paraNo : "Para Detail");
    $("#paraDetailModalSubtitle").text(paraTitle);

    var container = $("#paraDetailModalContent");
    container.empty();

    if (!Array.isArray(sections) || !sections.length) {
        container.append('<div class="alert alert-warning mb-0">No record found</div>');
        return;
    }

    sections.forEach(function (section) {
        var title = String(resolveParaDetailValue(section, "Title", "title") || "");
        var items = resolveParaDetailValue(section, "Items", "items") || [];
        var block = $("<section>").addClass("para-detail-section");
        block.append($("<h6>").text(title));

        if (!Array.isArray(items) || !items.some(hasParaDetailText)) {
            block.append('<div class="para-detail-empty">No record found</div>');
        } else {
            items.forEach(function (item) {
                if (!hasParaDetailText(item)) {
                    return;
                }
                block.append($("<div>").addClass("para-detail-rich-text").html(String(item || "")));
            });
        }

        container.append(block);
    });
}

function destroyParaDetailedTable() {
    if ($.fn.DataTable && $.fn.DataTable.isDataTable("#tblParaDetailedView")) {
        $("#tblParaDetailedView").DataTable().clear().destroy();
    }
}

function normalizeParaDetailedGridRow(item) {
    return {
        ArpseId: parseInt(resolveParaDetailValue(item, "ArpseId", "arpseId", "ARPSE_ID"), 10) || 0,
        ParaNo: String(resolveParaDetailValue(item, "ParaNo", "paraNo", "PARA_NO") || ""),
        ParaTitle: String(resolveParaDetailValue(item, "ParaTitle", "paraTitle", "PARA_TITLE") || "")
    };
}

function setParaDetailMessage(kind, message) {
    $("#paraDetailMessage").removeClass("alert-info alert-success alert-warning alert-danger").addClass("alert-" + kind).text(message || "");
}

function resolveParaDetailValue(obj) {
    for (var index = 1; index < arguments.length; index++) {
        var key = arguments[index];
        if (obj && Object.prototype.hasOwnProperty.call(obj, key) && obj[key] !== null && obj[key] !== undefined) {
            return obj[key];
        }
    }
    return null;
}

function hasParaDetailText(value) {
    var wrapper = document.createElement("div");
    wrapper.innerHTML = String(value || "");
    return String(wrapper.textContent || wrapper.innerText || "").replace(/\s+/g, " ").trim().length > 0;
}

function escapeParaDetailHtml(value) {
    return String(value || "").replace(/[&<>'"]/g, function (char) {
        return ({ "&": "&amp;", "<": "&lt;", ">": "&gt;", "'": "&#39;", '"': "&quot;" })[char];
    });
}

function resolveParaDetailAjaxMessage(xhr, fallback) {
    if (xhr && xhr.responseJSON && xhr.responseJSON.message) {
        return xhr.responseJSON.message;
    }
    return fallback || "Request failed.";
}