$(document).ready(function () {
    var page = $(".arpse-report-page");
    if (!page.length) {
        return;
    }

    setArpseReportTitle(null);
    $("#btnLoadArpseYearReport").on("click", loadArpseYearWiseReport);
    $("#arpseYearFilter").on("change", function () {
        setArpseYearReportMessage("info", "Select an ARPSE Year and load the report.");
        $("#arpseYearReportContainer").addClass("d-none");
        setArpseReportTitle(null);
    });
});

function loadArpseYearWiseReport() {
    var selectedYear = parseInt($("#arpseYearFilter").val(), 10);
    if (isNaN(selectedYear) || selectedYear <= 0) {
        setArpseYearReportMessage("warning", "Please select ARPSE Year.");
        $("#arpseYearReportContainer").addClass("d-none");
        return;
    }

    var page = $(".arpse-report-page");
    var reportUrl = String(page.data("report-url") || "");
    if (!reportUrl) {
        setArpseYearReportMessage("danger", "Report URL is not configured.");
        return;
    }

    setArpseYearReportMessage("info", "Loading report...");
    $("#btnLoadArpseYearReport").prop("disabled", true);
    $("#arpseYearReportContainer").addClass("d-none");

    $.ajax({
        url: reportUrl,
        type: "GET",
        cache: false,
        data: { arpseYear: selectedYear },
        success: function (data) {
            var rows = Array.isArray(data) ? data : [];

            if (!rows.length) {
                renderArpseYearWiseReport(rows);
                setArpseYearReportMessage("warning", "No record found for selected ARPSE Year.");
                return;
            }

            var selectedText = $("#arpseYearFilter option:selected").text();
            setArpseReportTitle(selectedText);
            $("#arpseYearReportSelectedLabel").text("ARPSE Year: " + selectedText);
            $("#arpseYearReportContainer").removeClass("d-none");
            renderArpseYearWiseReport(rows);
            setArpseYearReportMessage("success", rows.length + " record(s) loaded.");
        },
        error: function (xhr) {
            var message = "Unable to load report data.";
            if (xhr && xhr.responseJSON && xhr.responseJSON.message) {
                message = xhr.responseJSON.message;
            }

            renderArpseYearWiseReport([]);
            setArpseYearReportMessage("danger", message);
        },
        complete: function () {
            $("#btnLoadArpseYearReport").prop("disabled", false);
        }
    });
}

function renderArpseYearWiseReport(rows) {
    destroyArpseYearReportTable();
    forceArpseReportTableLayout();

    var tbody = $("#tblArpseYearWiseReport tbody");
    tbody.empty();

    if (!Array.isArray(rows) || !rows.length) {
        $("#arpseYearReportContainer").addClass("d-none");
        return;
    }

    rows.forEach(function (row, index) {
        var tr = $("<tr>");
        tr.append($("<td>").addClass("text-center").text(resolveValue(row, "SrNo", "srNo", "SR_NO") || (index + 1)));
        tr.append($("<td>").text(resolveValue(row, "ParaNo", "paraNo", "PARA_NO")));
        appendReportRichTextCell(tr, resolveValue(row, "ContentsOfPara", "contentsOfPara", "CONTENTS_OF_PARA"));
        appendReportRichTextCell(tr, resolveValue(row, "ReplyOfManagement", "replyOfManagement", "REPLY_OF_MANAGEMENT"));
        appendReportRichTextCell(tr, resolveValue(row, "DACRecommendations", "dacRecommendations", "DAC_RECOMMENDATIONS"));
        appendReportRichTextCell(tr, resolveValue(row, "PACDirectives", "pacDirectives", "PAC_DIRECTIVES"));
        appendReportRichTextCell(tr, resolveValue(row, "Progress", "progress", "PROGRESS"));
        tbody.append(tr);
    });

    initializeArpseYearReportDataTable();
}

function destroyArpseYearReportTable() {
    if ($.fn.DataTable && $.fn.DataTable.isDataTable("#tblArpseYearWiseReport")) {
        $("#tblArpseYearWiseReport").DataTable().clear().destroy();
    }

    forceArpseReportTableLayout();
}

function initializeArpseYearReportDataTable() {
    if (!$.fn.DataTable) {
        return;
    }

    var table = $("#tblArpseYearWiseReport");
    var displayExportOptions = getArpseYearReportExportOptions(false);
    var safeTextExportOptions = getArpseYearReportExportOptions(true);
    var reportTitle = getCurrentArpseReportTitle();
    var buttons = [];

    if (typeof getPdfExportButtonConfig === "function") {
        buttons.push($.extend(true, {}, getPdfExportButtonConfig(), {
            title: reportTitle,
            orientation: "landscape",
            pageSize: "A4",
            exportOptions: displayExportOptions,
            customize: customizeArpseYearReportPdf
        }));
    }

    if (typeof getExcelExportButtonConfig === "function") {
        buttons.push($.extend(true, {}, getExcelExportButtonConfig("Export to Excel"), {
            title: reportTitle,
            exportOptions: safeTextExportOptions
        }));
    }

    if (typeof getCsvExportButtonConfig === "function") {
        buttons.push($.extend(true, {}, getCsvExportButtonConfig("Export to CSV"), {
            exportOptions: safeTextExportOptions
        }));
    }

    buttons.push({
        text: "Export to Word",
        className: "btn btn-primary",
        action: exportArpseYearReportToWord
    });

    buttons.push({
        extend: "copyHtml5",
        text: "Copy to Clipboard",
        exportOptions: displayExportOptions
    });

    var dataTable = table.DataTable({
        dom: '<"top"lfB>rt<"bottom"ip><"clear">',
        autoWidth: false,
        ordering: false,
        scrollX: false,
        deferRender: true,
        columns: [
            { width: "5%", className: "arpse-report-number-cell" },
            { width: "7%", className: "arpse-report-para-cell" },
            { width: "18%" },
            { width: "18%" },
            { width: "18%" },
            { width: "18%" },
            { width: "16%" }
        ],
        buttons: buttons,
        lengthMenu: [
            [10, 50, 100, -1],
            [10, 50, 100, "All"]
        ],
        initComplete: forceArpseReportTableLayout,
        drawCallback: forceArpseReportTableLayout
    });

    dataTable.columns.adjust().draw(false);
    forceArpseReportTableLayout();
    window.setTimeout(function () {
        dataTable.columns.adjust();
        forceArpseReportTableLayout();
    }, 0);
}

function getArpseYearReportExportOptions(applyCsvSafety) {
    return {
        format: {
            body: function (data, row, column, node) {
                var textContent = "";
                if (node && typeof node.innerText === "string") {
                    textContent = node.innerText;
                } else if (node && typeof node.textContent === "string") {
                    textContent = node.textContent;
                } else if (typeof data === "string") {
                    textContent = data;
                } else if (data !== null && data !== undefined) {
                    textContent = String(data);
                }

                textContent = decodeHtmlRepeatedly(textContent)
                    .replace(/\u00a0/g, " ")
                    .replace(/[ \t]+\n/g, "\n")
                    .replace(/\n{3,}/g, "\n\n")
                    .trim();

                if (applyCsvSafety && typeof sanitizeCsvValue === "function") {
                    return sanitizeCsvValue(textContent);
                }

                return textContent;
            }
        }
    };
}

function customizeArpseYearReportPdf(doc) {
    if (!doc) {
        return;
    }

    doc.pageOrientation = "landscape";
    doc.pageSize = "A4";
    doc.pageMargins = [10, 16, 10, 16];
    doc.defaultStyle = doc.defaultStyle || {};
    doc.defaultStyle.fontSize = 8;
    doc.styles = doc.styles || {};
    doc.styles.tableHeader = doc.styles.tableHeader || {};
    doc.styles.tableHeader.fontSize = 8;
    doc.styles.tableHeader.bold = true;

    if (Array.isArray(doc.content) && doc.content.length) {
        doc.content[0].text = getCurrentArpseReportTitle();
        doc.content[0].fontSize = 12;
        doc.content[0].bold = true;
        doc.content[0].margin = [0, 0, 0, 8];

        var tableBlock = doc.content.find(function (item) {
            return item && item.table && Array.isArray(item.table.body);
        });
        if (tableBlock && tableBlock.table) {
            tableBlock.layout = {
                hLineWidth: function () { return 0.5; },
                vLineWidth: function () { return 0.5; },
                hLineColor: function () { return "#9ca3af"; },
                vLineColor: function () { return "#9ca3af"; },
                paddingLeft: function () { return 3; },
                paddingRight: function () { return 3; },
                paddingTop: function () { return 3; },
                paddingBottom: function () { return 3; }
            };
            tableBlock.margin = [0, 0, 0, 0];
            tableBlock.table.headerRows = 1;
            tableBlock.table.widths = ["5%", "7%", "18%", "18%", "18%", "18%", "16%"];

            tableBlock.table.body.forEach(function (row, rowIndex) {
                if (!Array.isArray(row)) {
                    return;
                }

                row.forEach(function (cell, columnIndex) {
                    if (cell && typeof cell === "object") {
                        cell.noWrap = false;
                        cell.margin = [1, 2, 1, 2];
                        cell.alignment = columnIndex < 2 ? "center" : "left";
                        if (rowIndex === 0) {
                            cell.fillColor = "#8399c7";
                            cell.color = "#000000";
                            cell.bold = true;
                            cell.alignment = "center";
                        }
                    }
                });
            });
        }
    }
}

function forceArpseReportTableLayout() {
    var table = $("#tblArpseYearWiseReport");
    if (!table.length) {
        return;
    }

    table.attr("style", "width: 100% !important; min-width: 2250px !important; table-layout: fixed !important;");
    table.closest(".arpse-report-table-wrap").css({
        width: "100%",
        maxWidth: "100%",
        overflowX: "auto"
    });
    table.closest(".dataTables_wrapper").css({
        width: "100%",
        maxWidth: "100%"
    });
}

function setArpseYearReportMessage(kind, message) {
    var alertBox = $("#arpseYearReportMessage");
    alertBox
        .removeClass("alert-info alert-success alert-warning alert-danger d-none")
        .addClass("alert-" + (kind || "info"))
        .text(message || "");
}

function setArpseReportTitle(selectedYearText) {
    var yearText = String(selectedYearText || "").trim();
    var title = yearText
        ? "ARPSE Year " + yearText + " DAC / PAC Report"
        : "ARPSE Year DAC / PAC Report";

    $("#arpseReportHeading").text(title);
    $("#arpseYearReportPrintTitle").text(title);

    if (typeof document !== "undefined") {
        document.title = title + " - IAS";
    }
}

function getCurrentArpseReportTitle() {
    return String($("#arpseReportHeading").text() || "ARPSE Year DAC / PAC Report").trim();
}

function appendReportRichTextCell(row, value) {
    row.append($("<td>").addClass("text-wrap report-rich-text").html(cleanReportRichHtml(value)));
}

function resolveValue(source) {
    if (!source) {
        return "";
    }

    for (var index = 1; index < arguments.length; index++) {
        var key = arguments[index];
        if (source[key] !== undefined && source[key] !== null) {
            return source[key];
        }
    }

    return "";
}

function cleanReportRichHtml(value) {
    var decoded = decodeHtmlRepeatedly(value || "").replace(/\u00a0/g, " ").trim();
    if (!decoded) {
        return "";
    }

    decoded = decoded.replace(/\r\n|\r|\n/g, "<br />");

    var template = document.createElement("template");
    template.innerHTML = decoded;

    sanitizeReportHtmlNode(template.content);
    return template.innerHTML.trim();
}

function sanitizeReportHtmlNode(root) {
    var allowedTags = {
        A: true,
        B: true,
        BLOCKQUOTE: true,
        BR: true,
        CAPTION: true,
        COL: true,
        COLGROUP: true,
        DIV: true,
        EM: true,
        H1: true,
        H2: true,
        H3: true,
        H4: true,
        H5: true,
        H6: true,
        HR: true,
        I: true,
        LI: true,
        OL: true,
        P: true,
        SPAN: true,
        STRONG: true,
        SUB: true,
        SUP: true,
        TABLE: true,
        TBODY: true,
        TD: true,
        TFOOT: true,
        TH: true,
        THEAD: true,
        TR: true,
        U: true,
        UL: true
    };
    var allowedAttributes = {
        href: true,
        title: true,
        colspan: true,
        rowspan: true,
        target: true,
        rel: true
    };

    Array.from(root.querySelectorAll("*")).forEach(function (node) {
        if (node.tagName === "SCRIPT" || node.tagName === "STYLE") {
            node.remove();
            return;
        }

        if (!allowedTags[node.tagName]) {
            var parent = node.parentNode;
            while (node.firstChild) {
                parent.insertBefore(node.firstChild, node);
            }
            parent.removeChild(node);
            return;
        }

        Array.from(node.attributes).forEach(function (attr) {
            var attrName = attr.name.toLowerCase();
            var attrValue = String(attr.value || "");
            if (attrName.indexOf("on") === 0 || !allowedAttributes[attrName]) {
                node.removeAttribute(attr.name);
                return;
            }

            if (attrName === "href" && !/^(https?:|#|\/)/i.test(attrValue)) {
                node.removeAttribute(attr.name);
            }
        });

        if (node.tagName === "A" && node.getAttribute("target") === "_blank") {
            node.setAttribute("rel", "noopener noreferrer");
        }
    });
}

function decodeHtmlRepeatedly(value) {
    var current = String(value || "");
    var textarea = document.createElement("textarea");

    for (var index = 0; index < 4; index++) {
        textarea.innerHTML = current;
        var decoded = textarea.value;
        if (decoded === current) {
            break;
        }

        current = decoded;
    }

    return current;
}

function exportArpseYearReportToWord() {
    var table = document.getElementById("tblArpseYearWiseReport");
    if (!table) {
        return;
    }

    var title = escapeHtml(getCurrentArpseReportTitle());
    var rowsHtml = buildArpseYearReportWordRows(table);
    var html = [
        "<!DOCTYPE html>",
        "<html>",
        "<head>",
        "<meta charset='utf-8'>",
        "<title>", title, "</title>",
        "<style>",
        "@page { size: A4 landscape; margin: 0.35in; }",
        "body { font-family: Arial, sans-serif; font-size: 9pt; color: #111; }",
        "h2 { font-size: 12pt; margin: 0 0 10px 0; text-align: left; }",
        "table { border-collapse: collapse; table-layout: fixed; width: 100%; }",
        "th, td { border: 1px solid #9ca3af; padding: 5px; vertical-align: top; white-space: normal; word-break: normal; overflow-wrap: break-word; }",
        "th { background: #8399c7; color: #000; font-weight: bold; text-align: center; }",
        ".num { text-align: center; }",
        "</style>",
        "</head>",
        "<body>",
        "<h2>", title, "</h2>",
        "<table>",
        "<colgroup>",
        "<col style='width:2%'>",
        "<col style='width:3%'>",
        "<col style='width:18%'>",
        "<col style='width:22%'>",
        "<col style='width:22%'>",
        "<col style='width:22%'>",
        "<col style='width:11%'>",
        "</colgroup>",
        rowsHtml,
        "</table>",
        "</body>",
        "</html>"
    ].join("");

    var blob = new Blob(["\ufeff", html], { type: "application/msword" });
    var fileName = getSafeReportFileName(getCurrentArpseReportTitle()) + ".doc";
    downloadBlob(blob, fileName);
}

function buildArpseYearReportWordRows(table) {
    var exportData = getArpseYearReportWordExportData(table);
    var html = ["<thead><tr>"];
    exportData.header.forEach(function (cell) {
        html.push("<th>", escapeHtml(cell), "</th>");
    });
    html.push("</tr></thead><tbody>");

    exportData.body.forEach(function (row) {
        html.push("<tr>");
        row.forEach(function (cell, index) {
            var cssClass = index < 2 ? " class='num'" : "";
            html.push("<td", cssClass, ">", escapeHtml(cell).replace(/\n/g, "<br>"), "</td>");
        });
        html.push("</tr>");
    });

    html.push("</tbody>");
    return html.join("");
}

function getArpseYearReportWordExportData(table) {
    if ($.fn.DataTable && $.fn.DataTable.isDataTable(table)) {
        var api = $(table).DataTable();
        if (api.buttons && typeof api.buttons.exportData === "function") {
            return api.buttons.exportData({
                columns: ":visible",
                modifier: {
                    search: "applied",
                    order: "applied",
                    page: "all"
                },
                format: {
                    header: function (data, column, node) {
                        return getCleanExportText(node || data);
                    },
                    body: function (data, row, column, node) {
                        return getCleanExportText(node || data);
                    }
                }
            });
        }
    }

    return {
        header: Array.from(table.querySelectorAll("thead th")).map(getCleanExportText),
        body: Array.from(table.querySelectorAll("tbody tr")).map(function (row) {
            return Array.from(row.cells).map(getCleanExportText);
        })
    };
}

function getCleanExportText(node) {
    var text = "";
    if (node && typeof node.innerText === "string") {
        text = node.innerText;
    } else if (node && typeof node.textContent === "string") {
        text = node.textContent;
    } else if (node !== null && node !== undefined) {
        text = String(node).replace(/<\s*br\s*\/?\s*>/gi, "\n").replace(/<\/\s*(div|p|li)\s*>/gi, "\n").replace(/<[^>]*>/g, "");
    }

    return decodeHtmlRepeatedly(text)
        .replace(/\u00a0/g, " ")
        .replace(/[ \t]+\n/g, "\n")
        .replace(/\n{3,}/g, "\n\n")
        .trim();
}

function downloadBlob(blob, fileName) {
    if (window.navigator && typeof window.navigator.msSaveOrOpenBlob === "function") {
        window.navigator.msSaveOrOpenBlob(blob, fileName);
        return;
    }

    var link = document.createElement("a");
    link.href = URL.createObjectURL(blob);
    link.download = fileName;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    window.setTimeout(function () {
        URL.revokeObjectURL(link.href);
    }, 0);
}

function getSafeReportFileName(value) {
    return String(value || "ARPSE Year DAC PAC Report")
        .replace(/[\\/:*?"<>|]+/g, "")
        .replace(/\s+/g, "_")
        .replace(/_+/g, "_")
        .replace(/^_+|_+$/g, "");
}

function escapeHtml(value) {
    return String(value || "")
        .replace(/&/g, "&amp;")
        .replace(/</g, "&lt;")
        .replace(/>/g, "&gt;")
        .replace(/"/g, "&quot;")
        .replace(/'/g, "&#39;");
}
