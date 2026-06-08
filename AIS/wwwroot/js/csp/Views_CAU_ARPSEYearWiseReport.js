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
            pageSize: "LEGAL",
            exportOptions: displayExportOptions,
            customize: customizeArpseYearReportPdf,
            action: exportArpseYearReportToPdf
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
            { width: "4%", className: "arpse-report-number-cell" },
            { width: "4%", className: "arpse-report-para-cell" },
            { width: "22%" },
            { width: "24%" },
            { width: "11%" },
            { width: "11%" },
            { width: "28%" }
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
                var htmlContent = getExportCellHtml(node, data);
                if (!applyCsvSafety) {
                    return htmlContent;
                }

                var textContent = getDelimitedExportTextFromHtml(htmlContent);

                if (applyCsvSafety && typeof sanitizeCsvValue === "function") {
                    return sanitizeCsvValue(textContent);
                }

                return textContent;
            }
        }
    };
}

function exportArpseYearReportToPdf(e, dt, button, config, complete) {
    var cleanup = createArpseYearReportPdfCleanup(dt, button, complete);
    var asyncExportStarted = false;

    try {
        var table = document.getElementById("tblArpseYearWiseReport");

        if (!table) {
            alert("PDF export is unavailable right now. Please refresh the page or contact support.");
            return;
        }

        var exportData = dt && dt.buttons && typeof dt.buttons.exportData === "function"
            ? dt.buttons.exportData(config.exportOptions)
            : null;
        var hasContent = exportData &&
            Array.isArray(exportData.body) &&
            exportData.body.some(function (row) {
                return Array.isArray(row) && row.some(function (cell) {
                    return String(cell || "").trim().length > 0;
                });
            });

        if (!hasContent) {
            alert("There is no data to export to PDF.");
            return;
        }

        if (hasArpseYearReportHtmlPdfSupport()) {
            asyncExportStarted = true;
            exportArpseYearReportToPdfFromHtml(table)
                .catch(function (error) {
                    console.error("ARPSE Year Wise HTML PDF export failed; retrying with DataTables PDF export.", error);
                    return runArpseYearReportFallbackPdfExport(this, e, dt, button, config, cleanup);
                }.bind(this))
                .then(cleanup, cleanup);
            return;
        }

        var pdfFactory = getArpseYearReportPdfFactory();
        var hasPdfMake = pdfFactory &&
            typeof pdfFactory.createPdf === "function" &&
            pdfFactory.vfs &&
            Object.keys(pdfFactory.vfs).length > 0 &&
            !(pdfFactory.version && pdfFactory.version.indexOf("placeholder") === 0);

        if (!hasPdfMake) {
            alert("PDF export is unavailable right now. Please refresh the page or contact support.");
            return;
        }

        var actionResult = $.fn.dataTable.ext.buttons.pdfHtml5.action.call(this, e, dt, button, config, cleanup);
        attachArpseYearReportPdfFailureHandler(actionResult, this, e, dt, button, config, cleanup);
    } catch (error) {
        console.error("ARPSE Year Wise PDF export failed.", error);
        runArpseYearReportFallbackPdfExport(this, e, dt, button, config, cleanup);
    } finally {
        if (!asyncExportStarted) {
            window.setTimeout(cleanup, 0);
        }
    }
}

function hasArpseYearReportHtmlPdfSupport() {
    return typeof window.html2canvas === "function" &&
        typeof getArpseYearReportJsPdfConstructor() === "function";
}

function getArpseYearReportJsPdfConstructor() {
    if (typeof window.jsPDF === "function") {
        return window.jsPDF;
    }

    if (window.jspdf && typeof window.jspdf.jsPDF === "function") {
        return window.jspdf.jsPDF;
    }

    return null;
}

function exportArpseYearReportToPdfFromHtml(table) {
    var title = getCurrentArpseReportTitle();
    var html = buildArpseYearReportExportHtml(table, title, getArpseYearReportPdfRenderStyles());
    var renderHost = createArpseYearReportPdfRenderHost(html);

    return window.html2canvas(renderHost.section, {
        backgroundColor: "#ffffff",
        scale: 1.35,
        useCORS: true,
        logging: false,
        windowWidth: renderHost.section.scrollWidth
    }).then(function (canvas) {
        var JsPdf = getArpseYearReportJsPdfConstructor();
        var pdf = new JsPdf("l", "pt", [1008, 612]);
        var pageWidth = 1008;
        var pageHeight = 612;
        var margin = 25.2;
        var imageWidth = pageWidth - (margin * 2);
        var imageHeight = canvas.height * imageWidth / canvas.width;
        var pageContentHeight = pageHeight - (margin * 2);
        var totalPages = Math.max(1, Math.ceil(imageHeight / pageContentHeight));
        var imageData = canvas.toDataURL("image/jpeg", 0.95);

        for (var pageIndex = 0; pageIndex < totalPages; pageIndex++) {
            if (pageIndex > 0) {
                pdf.addPage(pageWidth, pageHeight);
            }

            pdf.addImage(imageData, "JPEG", margin, margin - (pageIndex * pageContentHeight), imageWidth, imageHeight);
        }

        pdf.save(getSafeReportFileName(title) + ".pdf");
    }).then(function () {
        if (renderHost && renderHost.host && renderHost.host.parentNode) {
            renderHost.host.parentNode.removeChild(renderHost.host);
        }
    }, function (error) {
        if (renderHost && renderHost.host && renderHost.host.parentNode) {
            renderHost.host.parentNode.removeChild(renderHost.host);
        }
        throw error;
    });
}

function createArpseYearReportPdfRenderHost(html) {
    var parser = new DOMParser();
    var parsed = parser.parseFromString(html, "text/html");
    var section = parsed.querySelector(".WordSection1");
    var host = document.createElement("div");
    var style = document.createElement("style");

    host.className = "arpse-pdf-render-host";
    host.setAttribute("aria-hidden", "true");
    host.style.position = "absolute";
    host.style.left = "-20000px";
    host.style.top = "0";
    host.style.width = "1344px";
    host.style.background = "#ffffff";

    style.textContent = Array.from(parsed.querySelectorAll("style")).map(function (styleNode) {
        return styleNode.textContent || "";
    }).join("") || (getArpseYearReportWordStyles() + getArpseYearReportPdfRenderStyles());
    host.appendChild(style);
    host.appendChild(section ? section.cloneNode(true) : document.createElement("div"));
    document.body.appendChild(host);

    return {
        host: host,
        section: host.querySelector(".WordSection1") || host
    };
}

function attachArpseYearReportPdfFailureHandler(actionResult, context, e, dt, button, config, cleanup) {
    if (!actionResult || typeof actionResult.catch !== "function") {
        return;
    }

    actionResult.catch(function () {
        console.warn("ARPSE Year Wise PDF export promise failed; retrying with standard PDF export.");
        return runArpseYearReportFallbackPdfExport(context, e, dt, button, config, cleanup);
    }).catch(function () {
        console.error("ARPSE Year Wise fallback PDF export promise failed.");
        alert("Unable to generate PDF export. Please review the report content and try again.");
    }).then(cleanup, cleanup);
}

function runArpseYearReportFallbackPdfExport(context, e, dt, button, config, cleanup) {
    try {
        var fallbackConfig = $.extend(true, {}, config || {});
        fallbackConfig.customize = customizeArpseYearReportStandardPdf;
        var fallbackResult = $.fn.dataTable.ext.buttons.pdfHtml5.action.call(context, e, dt, button, fallbackConfig, cleanup);
        if (fallbackResult && typeof fallbackResult.catch === "function") {
            return fallbackResult.catch(function (fallbackError) {
                console.error("ARPSE Year Wise fallback PDF export failed.", fallbackError);
                alert("Unable to generate PDF export. Please review the report content and try again.");
            });
        }
        return fallbackResult;
    } catch (fallbackError) {
        console.error("ARPSE Year Wise fallback PDF export failed.", fallbackError);
        alert("Unable to generate PDF export. Please review the report content and try again.");
        return null;
    }
}

function customizeArpseYearReportPdf(doc) {
    try {
        applyArpseYearReportBasicPdfSettings(doc);

        var tableBlock = Array.isArray(doc.content)
            ? doc.content.find(function (item) {
                return item && item.table && Array.isArray(item.table.body);
            })
            : null;

        var body = [];
        try {
            var table = document.getElementById("tblArpseYearWiseReport");
            body = table ? buildArpseYearReportPdfBody(table) : [];
        } catch (formatError) {
            console.error("Unable to apply rich ARPSE PDF formatting; exporting with default PDF table.", formatError);
        }

        if (tableBlock && body.length > 1) {
            tableBlock.table.headerRows = 1;
            tableBlock.table.widths = getArpseYearReportPdfColumnWidths();
            tableBlock.table.body = body;
            tableBlock.layout = getArpseYearReportPdfTableLayout();
            tableBlock.margin = [0, 0, 0, 0];
        }

        normalizeArpsePdfMakeTablesInDoc(doc);
    } catch (error) {
        console.error("Unable to customize ARPSE PDF export; exporting with DataTables defaults.", error);
    }
}

function customizeArpseYearReportStandardPdf(doc) {
    try {
        applyArpseYearReportBasicPdfSettings(doc);
        normalizeArpsePdfMakeTablesInDoc(doc);
    } catch (error) {
        console.error("Unable to apply standard ARPSE PDF settings.", error);
    }
}

function applyArpseYearReportBasicPdfSettings(doc) {
    doc.pageSize = "LEGAL";
    doc.pageOrientation = "landscape";
    doc.pageMargins = [12, 18, 12, 18];
    doc.defaultStyle = doc.defaultStyle || {};
    doc.defaultStyle.fontSize = 7.5;
    doc.defaultStyle.lineHeight = 1.15;
    doc.styles = doc.styles || {};
    doc.styles.tableHeader = $.extend({}, doc.styles.tableHeader || {}, {
        bold: true,
        fontSize: 8,
        alignment: "center",
        fillColor: "#8399c7",
        color: "#000000"
    });

    if (Array.isArray(doc.content) && doc.content.length) {
        doc.content[0].text = getCurrentArpseReportTitle();
        doc.content[0].fontSize = 12;
        doc.content[0].bold = true;
        doc.content[0].margin = [0, 0, 0, 8];
    }
}

function createArpseYearReportPdfCleanup(dt, button, complete) {
    var didCleanup = false;

    return function () {
        if (didCleanup) {
            return;
        }

        didCleanup = true;
        try {
            if (dt && typeof dt.button === "function" && button) {
                dt.button(button).processing(false);
            }
        } catch (error) {
            console.warn("Unable to reset ARPSE PDF button processing state.", error);
        }

        try {
            $(button).removeClass("processing disabled").prop("disabled", false);
        } catch (error) {
            console.warn("Unable to reset ARPSE PDF button element state.", error);
        }

        if (typeof complete === "function") {
            complete();
        }
    };
}

function getArpseYearReportPdfFactory() {
    if (window.pdfMake) {
        return window.pdfMake;
    }

    if (window.pdfmake) {
        return window.pdfmake;
    }

    return null;
}

function getArpseYearReportPdfColumnWidths() {
    return [30, 49, 197, 216, 167, 148, 177];
}

function normalizeArpsePdfMakeTablesInDoc(node) {
    if (Array.isArray(node)) {
        node.forEach(function (item) {
            normalizeArpsePdfMakeTablesInDoc(item);
        });
        return node;
    }

    if (!node || typeof node !== "object") {
        return node;
    }

    if (node.table && Array.isArray(node.table.body)) {
        normalizeArpsePdfMakeTableBlock(node);
    }

    Object.keys(node).forEach(function (key) {
        if (key !== "table") {
            normalizeArpsePdfMakeTablesInDoc(node[key]);
        }
    });

    return node;
}

function normalizeArpsePdfMakeTableBlock(tableBlock) {
    if (!tableBlock || !tableBlock.table) {
        return tableBlock;
    }

    var table = tableBlock.table;
    var body = Array.isArray(table.body) ? table.body : [];
    var widthCount = getArpsePdfMakeTableWidthCount(table, body);
    var expectedCount = Math.max(1, widthCount);

    if (Array.isArray(table.widths)) {
        table.widths = table.widths.map(function (width) {
            return isValidArpsePdfMakeWidth(width) ? width : "*";
        });
    } else {
        table.widths = Array.from({ length: expectedCount }).map(function () { return "*"; });
    }

    if (table.widths.length < expectedCount) {
        while (table.widths.length < expectedCount) {
            table.widths.push("*");
        }
    } else if (table.widths.length > expectedCount) {
        table.widths = table.widths.slice(0, expectedCount);
    }

    var normalizedBody = body.map(function (row) {
        return normalizeArpsePdfMakeTableRow(row, expectedCount);
    });

    if (!normalizedBody.length) {
        normalizedBody.push(Array.from({ length: expectedCount }).map(function () {
            return { text: "" };
        }));
    }

    table.body = normalizedBody;
    normalizeArpsePdfMakeTablesInDoc(table.body);
    return tableBlock;
}

function getArpsePdfMakeTableWidthCount(table, body) {
    var widthCount = Array.isArray(table.widths) && table.widths.length ? table.widths.length : 0;
    var maxRowCount = 0;

    body.forEach(function (row) {
        var rowArray = Array.isArray(row) ? row : [];
        maxRowCount = Math.max(maxRowCount, getArpsePdfMakeRowCellCount(rowArray));
    });

    if (!widthCount) {
        return maxRowCount || 1;
    }

    var canTrimToWidths = body.every(function (row) {
        return canTrimArpsePdfMakeRowToCount(Array.isArray(row) ? row : [], widthCount);
    });

    return canTrimToWidths ? widthCount : Math.max(widthCount, maxRowCount || 1);
}

function getArpsePdfMakeRowCellCount(row) {
    var count = 0;

    row.forEach(function (cell) {
        if (isArpsePdfMakeColSpanPlaceholder(cell)) {
            count++;
            return;
        }

        var span = getArpsePdfMakeColSpan(cell);
        count += Math.max(1, span);
    });

    return count;
}

function canTrimArpsePdfMakeRowToCount(row, expectedCount) {
    if (row.length <= expectedCount) {
        return true;
    }

    return row.slice(expectedCount).every(function (cell) {
        return isEmptyArpsePdfMakeCell(cell);
    });
}

function normalizeArpsePdfMakeTableRow(row, expectedCount) {
    var source = Array.isArray(row) ? row : [];
    var normalized = [];

    for (var index = 0; index < source.length && normalized.length < expectedCount; index++) {
        var sourceCell = source[index];
        var cell = normalizeArpsePdfMakeCell(sourceCell);

        if (isArpsePdfMakeColSpanPlaceholder(cell)) {
            normalized.push({});
            continue;
        }

        var colSpan = Math.min(getArpsePdfMakeColSpan(cell), expectedCount - normalized.length);
        if (colSpan > 1) {
            cell.colSpan = colSpan;
        } else if (cell && typeof cell === "object" && Object.prototype.hasOwnProperty.call(cell, "colSpan")) {
            delete cell.colSpan;
        }

        normalized.push(cell);

        for (var spanIndex = 1; spanIndex < colSpan && normalized.length < expectedCount; spanIndex++) {
            normalized.push({});
            if (index + 1 < source.length && isArpsePdfMakeColSpanPlaceholder(source[index + 1])) {
                index++;
            }
        }
    }

    while (normalized.length < expectedCount) {
        normalized.push({ text: "" });
    }

    return normalized.slice(0, expectedCount);
}

function normalizeArpsePdfMakeCell(cell) {
    if (cell === undefined || cell === null) {
        return { text: "" };
    }

    if (isArpsePdfMakeColSpanPlaceholder(cell)) {
        return {};
    }

    if (typeof cell === "string" || typeof cell === "number" || typeof cell === "boolean") {
        return { text: formatArpsePdfTextForWrapping(String(cell)) };
    }

    if (Array.isArray(cell)) {
        return { stack: cell };
    }

    if (typeof cell !== "object") {
        return { text: "" };
    }

    if (Object.prototype.hasOwnProperty.call(cell, "rowSpan")) {
        delete cell.rowSpan;
    }

    normalizeArpsePdfMakeTablesInDoc(cell);
    return cell;
}

function getArpsePdfMakeColSpan(cell) {
    if (!cell || typeof cell !== "object" || isArpsePdfMakeColSpanPlaceholder(cell)) {
        return 1;
    }

    var colSpan = parseInt(cell.colSpan, 10);
    if (isNaN(colSpan) || colSpan < 1) {
        return 1;
    }

    return colSpan;
}

function isArpsePdfMakeColSpanPlaceholder(cell) {
    return cell &&
        typeof cell === "object" &&
        !Array.isArray(cell) &&
        Object.keys(cell).length === 0;
}

function isEmptyArpsePdfMakeCell(cell) {
    if (cell === undefined || cell === null) {
        return true;
    }

    if (isArpsePdfMakeColSpanPlaceholder(cell)) {
        return true;
    }

    if (typeof cell === "string") {
        return !cell.trim();
    }

    if (typeof cell === "number" || typeof cell === "boolean") {
        return false;
    }

    if (typeof cell !== "object") {
        return true;
    }

    if (typeof cell.text === "string" && cell.text.trim()) {
        return false;
    }

    if (Array.isArray(cell.text)) {
        return !cell.text.some(function (fragment) {
            return !isEmptyArpsePdfMakeCell(fragment);
        });
    }

    if (Array.isArray(cell.stack)) {
        return !cell.stack.some(function (item) {
            return !isEmptyArpsePdfMakeCell(item);
        });
    }

    return Object.keys(cell).every(function (key) {
        return key === "margin" || key === "alignment" || key === "style" || key === "bold" || key === "fontSize" || key === "fillColor" || key === "color" || key === "noWrap";
    });
}

function isValidArpsePdfMakeWidth(width) {
    if (typeof width === "number") {
        return isFinite(width) && width > 0;
    }

    if (width === "auto" || width === "*") {
        return true;
    }

    return false;
}

function buildArpseYearReportPdfBody(table) {
    var exportTable = table.cloneNode(true);
    prepareArpseExportTableHtml(exportTable);

    var body = [];
    var headerCells = Array.from(exportTable.querySelectorAll("thead th"));
    var rows = getArpseYearReportExportRows(exportTable);

    body.push(headerCells.map(function (cell) {
        return $.extend(buildPdfCellFromHtmlElement(cell, true, false), {
            style: "tableHeader",
            alignment: "center",
            fillColor: "#8399c7",
            bold: true
        });
    }));

    rows.forEach(function (row) {
        body.push(Array.from(row.cells).map(function (cell, columnIndex) {
            return $.extend(buildPdfCellFromHtmlElement(cell, false, false), {
                alignment: "left"
            });
        }));
    });

    return normalizeArpsePdfMakeTableBlock({
        table: {
            widths: getArpseYearReportPdfColumnWidths(),
            body: body
        }
    }).table.body;
}

function getArpseYearReportExportRows(table) {
    if ($.fn.DataTable && $.fn.DataTable.isDataTable(table)) {
        return $(table).DataTable().rows({
            search: "applied",
            order: "applied",
            page: "all"
        }).nodes().toArray();
    }

    return Array.from(table.querySelectorAll("tbody tr"));
}

function buildPdfCellFromHtmlElement(element, isHeader, isNested) {
    var stack = convertHtmlChildrenToPdfBlocks(element, {
        bold: isHeader,
        fontSize: isNested ? 6.5 : 7.5
    }, isNested);

    if (!stack.length) {
        stack.push({ text: "" });
    }

    return {
        stack: stack,
        margin: isNested ? [1, 1, 1, 1] : [1, 2, 1, 2],
        noWrap: false
    };
}

function convertHtmlChildrenToPdfBlocks(root, inheritedStyle, isNested) {
    var blocks = [];
    var inlineBuffer = [];

    Array.from(root.childNodes || []).forEach(function (child) {
        appendHtmlNodeToPdfBlocks(child, inheritedStyle || {}, blocks, inlineBuffer, isNested);
    });

    flushPdfInlineBuffer(blocks, inlineBuffer);
    return blocks;
}

function appendHtmlNodeToPdfBlocks(node, inheritedStyle, blocks, inlineBuffer, isNested) {
    if (!node) {
        return;
    }

    if (node.nodeType === 3) {
        appendPdfTextFragment(inlineBuffer, node.nodeValue, inheritedStyle);
        return;
    }

    if (node.nodeType !== 1) {
        return;
    }

    var tagName = node.tagName;
    var childStyle = getPdfStyleForHtmlNode(node, inheritedStyle, isNested);

    if (tagName === "BR") {
        inlineBuffer.push({ text: "\n" });
        return;
    }

    if (tagName === "TABLE") {
        flushPdfInlineBuffer(blocks, inlineBuffer);
        blocks.push(convertHtmlTableToPdfTextBlock(node, childStyle));
        return;
    }

    if (tagName === "UL" || tagName === "OL") {
        flushPdfInlineBuffer(blocks, inlineBuffer);
        blocks.push(convertHtmlListToPdfBlock(node, tagName === "OL", childStyle, isNested));
        return;
    }

    if (isPdfBlockHtmlNode(tagName)) {
        flushPdfInlineBuffer(blocks, inlineBuffer);
        var childBlocks = convertHtmlChildrenToPdfBlocks(node, childStyle, isNested);
        if (childBlocks.length) {
            var block = childBlocks.length === 1 ? childBlocks[0] : { stack: childBlocks };
            block.margin = tagName.indexOf("H") === 0 ? [0, 2, 0, 3] : [0, 0, 0, 3];
            blocks.push(block);
        }
        return;
    }

    convertHtmlNodeToPdfFragments(node, childStyle, isNested).forEach(function (fragment) {
        inlineBuffer.push(fragment);
    });
}

function convertHtmlNodeToPdfFragments(node, inheritedStyle, isNested) {
    var fragments = [];

    Array.from(node.childNodes || []).forEach(function (child) {
        if (child.nodeType === 3) {
            appendPdfTextFragment(fragments, child.nodeValue, inheritedStyle);
            return;
        }

        if (child.nodeType !== 1) {
            return;
        }

        var tagName = child.tagName;
        var childStyle = getPdfStyleForHtmlNode(child, inheritedStyle, isNested);
        if (tagName === "BR") {
            fragments.push({ text: "\n" });
            return;
        }

        convertHtmlNodeToPdfFragments(child, childStyle, isNested).forEach(function (fragment) {
            fragments.push(fragment);
        });
    });

    return fragments;
}

function appendPdfTextFragment(fragments, value, style) {
    var text = formatArpsePdfTextForWrapping(String(value || "").replace(/\r\n|\r|\n/g, " "));
    if (!text) {
        return;
    }

    fragments.push($.extend({ text: text }, style || {}));
}

function formatArpsePdfTextForWrapping(value) {
    return String(value || "").replace(/(\S{48,})/g, function (match) {
        return match.replace(/(.{24})/g, "$1\u200b");
    });
}

function flushPdfInlineBuffer(blocks, inlineBuffer) {
    if (!inlineBuffer.length) {
        return;
    }

    blocks.push({
        text: inlineBuffer.splice(0, inlineBuffer.length),
        margin: [0, 0, 0, 2]
    });
}

function getPdfStyleForHtmlNode(node, inheritedStyle, isNested) {
    var tagName = node && node.tagName ? node.tagName : "";
    var style = $.extend({}, inheritedStyle || {});
    var alignment = getHtmlElementAlignment(node);
    var fontWeight = getInlineStyleValue(node, "font-weight").toLowerCase();
    var fontStyle = getInlineStyleValue(node, "font-style").toLowerCase();
    var textDecoration = getInlineStyleValue(node, "text-decoration").toLowerCase();

    if (tagName === "B" || tagName === "STRONG" || /^H[1-6]$/.test(tagName) || fontWeight === "bold" || parseInt(fontWeight, 10) >= 600) {
        style.bold = true;
    }

    if (tagName === "I" || tagName === "EM" || fontStyle === "italic") {
        style.italics = true;
    }

    if (tagName === "U" || textDecoration.indexOf("underline") >= 0) {
        style.decoration = "underline";
    }

    if (tagName === "SUB") {
        style.subscript = true;
    }

    if (tagName === "SUP") {
        style.superscript = true;
    }

    if (/^H[1-6]$/.test(tagName)) {
        style.fontSize = isNested ? 7 : 8.5;
    }

    if (alignment) {
        style.alignment = alignment;
    }

    return style;
}

function isPdfBlockHtmlNode(tagName) {
    return tagName === "P" ||
        tagName === "DIV" ||
        tagName === "BLOCKQUOTE" ||
        tagName === "LI" ||
        /^H[1-6]$/.test(tagName);
}

function convertHtmlListToPdfBlock(listNode, isOrdered, inheritedStyle, isNested) {
    var items = Array.from(listNode.children || []).filter(function (child) {
        return child.tagName === "LI";
    }).map(function (child) {
        var blocks = convertHtmlChildrenToPdfBlocks(child, inheritedStyle, isNested);
        if (!blocks.length) {
            return { text: "" };
        }

        return blocks.length === 1 ? blocks[0] : { stack: blocks };
    });

    var listBlock = {
        margin: [8, 0, 0, 3]
    };

    listBlock[isOrdered ? "ol" : "ul"] = items;
    return listBlock;
}

function convertHtmlTableToPdfTextBlock(tableNode, inheritedStyle) {
    var rows = Array.from(tableNode.rows || []);
    var lineBlocks = rows.map(function (row) {
        var rowFragments = [];

        Array.from(row.cells || []).forEach(function (cell) {
            if (rowFragments.length) {
                rowFragments.push({ text: " | ", color: "#4b5563" });
            }

            var cellStyle = getPdfStyleForHtmlNode(cell, inheritedStyle || {}, true);
            if (cell.tagName === "TH") {
                cellStyle.bold = true;
            }

            var cellFragments = convertHtmlTableCellToPdfTextFragments(cell, cellStyle);
            if (cellFragments.length) {
                cellFragments.forEach(function (fragment) {
                    rowFragments.push(fragment);
                });
            } else {
                rowFragments.push({ text: "" });
            }
        });

        if (!rowFragments.length) {
            rowFragments.push({ text: "" });
        }

        return {
            text: rowFragments,
            margin: [0, 0, 0, 1.5],
            noWrap: false
        };
    });

    return {
        stack: lineBlocks.length ? lineBlocks : [{ text: "" }],
        margin: [0, 2, 0, 4],
        noWrap: false
    };
}

function convertHtmlTableCellToPdfTextFragments(cell, inheritedStyle) {
    var fragments = [];
    Array.from(cell.childNodes || []).forEach(function (child) {
        appendHtmlNodeToPdfTableTextFragments(child, inheritedStyle || {}, fragments);
    });

    return trimArpsePdfTextFragments(fragments);
}

function appendHtmlNodeToPdfTableTextFragments(node, inheritedStyle, fragments) {
    if (!node) {
        return;
    }

    if (node.nodeType === 3) {
        appendPdfTextFragment(fragments, node.nodeValue, inheritedStyle);
        return;
    }

    if (node.nodeType !== 1) {
        return;
    }

    var tagName = node.tagName;
    var childStyle = getPdfStyleForHtmlNode(node, inheritedStyle || {}, true);

    if (tagName === "BR") {
        appendArpsePdfTextBreak(fragments);
        return;
    }

    if (tagName === "TABLE") {
        appendNestedHtmlTableAsPdfText(node, childStyle, fragments);
        return;
    }

    if (tagName === "UL" || tagName === "OL") {
        appendHtmlListAsPdfText(node, tagName === "OL", childStyle, fragments);
        return;
    }

    var needsTrailingBreak = tagName === "P" ||
        tagName === "DIV" ||
        tagName === "BLOCKQUOTE" ||
        tagName === "LI" ||
        /^H[1-6]$/.test(tagName);

    Array.from(node.childNodes || []).forEach(function (child) {
        appendHtmlNodeToPdfTableTextFragments(child, childStyle, fragments);
    });

    if (needsTrailingBreak) {
        appendArpsePdfTextBreak(fragments);
    }
}

function appendNestedHtmlTableAsPdfText(tableNode, inheritedStyle, fragments) {
    var rows = Array.from(tableNode.rows || []);
    rows.forEach(function (row, rowIndex) {
        if (rowIndex > 0) {
            appendArpsePdfTextBreak(fragments);
        }

        Array.from(row.cells || []).forEach(function (cell, cellIndex) {
            if (cellIndex > 0) {
                fragments.push({ text: " | ", color: "#4b5563" });
            }

            var cellStyle = getPdfStyleForHtmlNode(cell, inheritedStyle || {}, true);
            if (cell.tagName === "TH") {
                cellStyle.bold = true;
            }

            var cellFragments = convertHtmlTableCellToPdfTextFragments(cell, cellStyle);
            if (cellFragments.length) {
                cellFragments.forEach(function (fragment) {
                    fragments.push(fragment);
                });
            }
        });
    });
}

function appendHtmlListAsPdfText(listNode, isOrdered, inheritedStyle, fragments) {
    Array.from(listNode.children || []).filter(function (child) {
        return child.tagName === "LI";
    }).forEach(function (child, index) {
        if (index > 0) {
            appendArpsePdfTextBreak(fragments);
        }

        fragments.push({ text: isOrdered ? (index + 1) + ". " : "- " });
        Array.from(child.childNodes || []).forEach(function (childNode) {
            appendHtmlNodeToPdfTableTextFragments(childNode, inheritedStyle || {}, fragments);
        });
    });
}

function appendArpsePdfTextBreak(fragments) {
    if (!fragments.length) {
        return;
    }

    var last = fragments[fragments.length - 1];
    if (last && last.text === "\n") {
        return;
    }

    fragments.push({ text: "\n" });
}

function trimArpsePdfTextFragments(fragments) {
    while (fragments.length && fragments[0] && fragments[0].text === "\n") {
        fragments.shift();
    }

    while (fragments.length && fragments[fragments.length - 1] && fragments[fragments.length - 1].text === "\n") {
        fragments.pop();
    }

    return fragments;
}

function getHtmlElementAlignment(node) {
    if (!node || !node.getAttribute) {
        return "";
    }

    var align = String(node.getAttribute("align") || "").toLowerCase();
    if (!align) {
        align = getInlineStyleValue(node, "text-align").toLowerCase();
    }

    if (align === "center" || align === "right" || align === "justify") {
        return align;
    }

    return "";
}

function getInlineStyleValue(node, propertyName) {
    var style = String(node && node.getAttribute ? node.getAttribute("style") || "" : "");
    var declarations = style.split(";");

    for (var index = 0; index < declarations.length; index++) {
        var parts = declarations[index].split(":");
        if (parts.length > 1 && parts[0].trim().toLowerCase() === propertyName) {
            return parts.slice(1).join(":").trim();
        }
    }

    return "";
}

function getHtmlTableColumnCount(rows) {
    var maxColumns = 0;
    rows.forEach(function (row) {
        var columns = 0;
        Array.from(row.cells || []).forEach(function (cell) {
            columns += cell.colSpan || 1;
        });
        maxColumns = Math.max(maxColumns, columns);
    });

    return maxColumns;
}

function hasHeaderRow(tableNode) {
    var firstRow = tableNode && tableNode.rows && tableNode.rows.length ? tableNode.rows[0] : null;
    if (!firstRow) {
        return false;
    }

    return Array.from(firstRow.cells || []).some(function (cell) {
        return cell.tagName === "TH";
    });
}

function getArpseYearReportPdfTableLayout() {
    return {
        hLineWidth: function () { return 0.5; },
        vLineWidth: function () { return 0.5; },
        hLineColor: function () { return "#9ca3af"; },
        vLineColor: function () { return "#9ca3af"; },
        paddingLeft: function () { return 3; },
        paddingRight: function () { return 3; },
        paddingTop: function () { return 3; },
        paddingBottom: function () { return 3; }
    };
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
    normalizeReportRichTextSpacing(template.content);
    formatArpseDacPacSectionLabels(template.content);
    normalizeReportRichTextSpacing(template.content);
    return template.innerHTML.trim();
}

function normalizeReportRichTextSpacing(root) {
    normalizeReportRichTextSpaces(root);
    removeEmptyReportRichTextBlocks(root);
    normalizeRepeatedReportRichTextBreaks(root);
    trimReportRichTextBoundaryBreaks(root);
}

function normalizeReportRichTextSpaces(root) {
    var walker = document.createTreeWalker(root, NodeFilter.SHOW_TEXT, null);
    var node = walker.nextNode();

    while (node) {
        node.nodeValue = String(node.nodeValue || "").replace(/\u00a0/g, " ");
        node = walker.nextNode();
    }
}

function removeEmptyReportRichTextBlocks(root) {
    var selector = "p,div,span";
    var removedAny = true;

    while (removedAny) {
        removedAny = false;
        Array.from(root.querySelectorAll(selector)).forEach(function (element) {
            if (isEmptyReportRichTextBlock(element)) {
                element.parentNode.removeChild(element);
                removedAny = true;
            }
        });
    }
}

function isEmptyReportRichTextBlock(element) {
    if (!element) {
        return true;
    }

    if (element.querySelector("table,thead,tbody,tfoot,tr,td,th,ul,ol,li,hr")) {
        return false;
    }

    var text = String(element.textContent || "").replace(/\u00a0/g, " ").trim();
    return !text;
}

function normalizeRepeatedReportRichTextBreaks(root) {
    Array.from(root.querySelectorAll("br")).forEach(function (br) {
        if (!br.parentNode) {
            return;
        }

        var previous = getPreviousReportRichTextContentSibling(br);
        if (previous && previous.nodeType === 1 && previous.tagName === "BR") {
            br.parentNode.removeChild(br);
        }
    });
}

function trimReportRichTextBoundaryBreaks(root) {
    trimReportRichTextBreaksInParent(root);
    Array.from(root.querySelectorAll("p,div,span,td,th")).forEach(trimReportRichTextBreaksInParent);
}

function trimReportRichTextBreaksInParent(parent) {
    var first = getFirstReportRichTextContentChild(parent);
    while (first && first.nodeType === 1 && first.tagName === "BR") {
        var next = getNextReportRichTextContentSibling(first);
        first.parentNode.removeChild(first);
        first = next;
    }

    var last = getLastReportRichTextContentChild(parent);
    while (last && last.nodeType === 1 && last.tagName === "BR") {
        var previous = getPreviousReportRichTextContentSibling(last);
        last.parentNode.removeChild(last);
        last = previous;
    }
}

function getFirstReportRichTextContentChild(parent) {
    var child = parent && parent.firstChild;
    while (child && isIgnorableReportRichTextTextNode(child)) {
        child = child.nextSibling;
    }

    return child;
}

function getLastReportRichTextContentChild(parent) {
    var child = parent && parent.lastChild;
    while (child && isIgnorableReportRichTextTextNode(child)) {
        child = child.previousSibling;
    }

    return child;
}

function getPreviousReportRichTextContentSibling(node) {
    var sibling = node && node.previousSibling;
    while (sibling && isIgnorableReportRichTextTextNode(sibling)) {
        sibling = sibling.previousSibling;
    }

    return sibling;
}

function getNextReportRichTextContentSibling(node) {
    var sibling = node && node.nextSibling;
    while (sibling && isIgnorableReportRichTextTextNode(sibling)) {
        sibling = sibling.nextSibling;
    }

    return sibling;
}

function isIgnorableReportRichTextTextNode(node) {
    return node &&
        node.nodeType === 3 &&
        !String(node.nodeValue || "").replace(/\u00a0/g, " ").trim();
}

function formatArpseDacPacSectionLabels(root) {
    var datePattern = "(?:\\(?\\d{1,2}[\\/.]\\d{1,2}[\\/.]\\d{2,4}\\)?|\\(?\\d{1,2}-[A-Za-z]{3}-\\d{4}\\)?|[A-Za-z]{3,9}\\s+\\d{1,2},?\\s+\\d{4})";
    var labelPattern = new RegExp("\\b((?:DAC Recommendation|PAC Directive)(?:s)?(?:\\s*(?:&|and|\\/|-)?\\s*Date\\b)?(?:\\s*(?:dated\\b|date\\b)?\\s*[:\\-]?\\s*" + datePattern + "\\s*:?)?)", "gi");
    var walker = document.createTreeWalker(root, NodeFilter.SHOW_TEXT, {
        acceptNode: function (node) {
            var value = node.nodeValue || "";
            if (!labelPattern.test(value)) {
                labelPattern.lastIndex = 0;
                return NodeFilter.FILTER_REJECT;
            }

            labelPattern.lastIndex = 0;
            return NodeFilter.FILTER_ACCEPT;
        }
    });
    var textNodes = [];
    var currentNode = walker.nextNode();

    while (currentNode) {
        textNodes.push(currentNode);
        currentNode = walker.nextNode();
    }

    textNodes.forEach(function (textNode) {
        var value = textNode.nodeValue || "";
        var fragment = document.createDocumentFragment();
        var lastIndex = 0;

        value.replace(labelPattern, function (match, label, offset) {
            if (offset > lastIndex) {
                fragment.appendChild(document.createTextNode(value.substring(lastIndex, offset)));
            }

            var labelSpan = document.createElement("span");
            labelSpan.setAttribute("style", "font-weight:bold;text-decoration:underline;");
            labelSpan.appendChild(document.createTextNode(match));
            fragment.appendChild(labelSpan);
            lastIndex = offset + match.length;
            return match;
        });

        if (lastIndex < value.length) {
            fragment.appendChild(document.createTextNode(value.substring(lastIndex)));
        }

        textNode.parentNode.replaceChild(fragment, textNode);
    });
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
        align: true,
        border: true,
        cellpadding: true,
        cellspacing: true,
        class: true,
        height: true,
        rowspan: true,
        style: true,
        target: true,
        rel: true,
        width: true
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

            if (attrName === "style") {
                var safeStyle = sanitizeReportStyleAttribute(attrValue);
                if (safeStyle) {
                    node.setAttribute(attr.name, safeStyle);
                } else {
                    node.removeAttribute(attr.name);
                }
            }
        });

        if (node.tagName === "A" && node.getAttribute("target") === "_blank") {
            node.setAttribute("rel", "noopener noreferrer");
        }
    });
}

function sanitizeReportStyleAttribute(value) {
    var allowedStyles = {
        "background-color": true,
        "border": true,
        "border-bottom": true,
        "border-collapse": true,
        "border-left": true,
        "border-right": true,
        "border-spacing": true,
        "border-top": true,
        "color": true,
        "font-style": true,
        "font-weight": true,
        "height": true,
        "margin": true,
        "margin-bottom": true,
        "margin-left": true,
        "margin-right": true,
        "margin-top": true,
        "padding": true,
        "padding-bottom": true,
        "padding-left": true,
        "padding-right": true,
        "padding-top": true,
        "text-align": true,
        "text-decoration": true,
        "vertical-align": true,
        "white-space": true,
        "width": true
    };

    return String(value || "")
        .split(";")
        .map(function (declaration) {
            var parts = declaration.split(":");
            if (parts.length < 2) {
                return "";
            }

            var propertyName = parts[0].trim().toLowerCase();
            var propertyValue = parts.slice(1).join(":").trim();
            if (!allowedStyles[propertyName] || !propertyValue) {
                return "";
            }

            if (/expression|javascript:|url\s*\(|behavior\s*:/i.test(propertyValue)) {
                return "";
            }

            return propertyName + ": " + propertyValue;
        })
        .filter(function (declaration) {
            return declaration;
        })
        .join("; ");
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

    var title = getCurrentArpseReportTitle();
    var html = buildArpseYearReportExportHtml(table, title, "");

    var blob = new Blob(["\ufeff", html], { type: "application/msword" });
    var fileName = getSafeReportFileName(title) + ".doc";
    downloadBlob(blob, fileName);
}

function buildArpseYearReportExportHtml(table, title, extraStyles) {
    var escapedTitle = escapeHtml(title);
    var rowsHtml = buildArpseYearReportWordRows(table);

    return [
        "<!DOCTYPE html>",
        "<html>",
        "<head>",
        "<meta charset='utf-8'>",
        "<title>", escapedTitle, "</title>",
        "<style>",
        getArpseYearReportWordStyles(),
        extraStyles || "",
        "</style>",
        "</head>",
        "<body>",
        "<div class='WordSection1'>",
        "<h2>", escapedTitle, "</h2>",
        "<table class='arpse-export-table' border='1' cellspacing='0' cellpadding='3' style='border-collapse:collapse;width:100%;table-layout:fixed;'>",
        "<colgroup>",
        "<col style='width:4%'>",
        "<col style='width:4%'>",
        "<col style='width:22%'>",
        "<col style='width:24%'>",
        "<col style='width:11%'>",
        "<col style='width:11%'>",
        "<col style='width:28%'>",
        "</colgroup>",
        rowsHtml,
        "</table>",
        "</div>",
        "</body>",
        "</html>"
    ].join("");
}

function buildArpseYearReportWordRows(table) {
    var rows = getArpseYearReportExportRows(table);
    var html = ["<thead><tr>"];

    Array.from(table.querySelectorAll("thead th")).forEach(function (cell) {
        html.push("<th valign='top' style='border:1px solid #9ca3af;vertical-align:top;text-align:center;'>", getExportCellHtml(cell, ""), "</th>");
    });

    html.push("</tr></thead><tbody>");

    rows.forEach(function (row) {
        html.push("<tr>");
        Array.from(row.cells || []).forEach(function (cell, index) {
            var cssClass = index < 2 ? " class='num'" : " class='rich-html'";
            html.push("<td", cssClass, " valign='top' style='border:1px solid #9ca3af;vertical-align:top;text-align:left;'>", getExportCellHtml(cell, ""), "</td>");
        });
        html.push("</tr>");
    });

    html.push("</tbody>");
    return html.join("");
}

function getArpseYearReportWordStyles() {
    return [
        "@page { size: legal landscape; margin: 0.35in; }",
        "@page WordSection1 { size: 14in 8.5in; margin: 0.35in; mso-page-orientation: landscape; }",
        "div.WordSection1 { page: WordSection1; }",
        "body { font-family: Arial, sans-serif; font-size: 9pt; color: #111; }",
        "h2 { font-size: 12pt; margin: 0 0 10px 0; text-align: left; }",
        ".arpse-export-table { border-collapse: collapse; table-layout: fixed; width: 100%; }",
        ".arpse-export-table > thead > tr > th, .arpse-export-table > tbody > tr > td { border: 1px solid #9ca3af; padding: 5px; vertical-align: top; white-space: normal; word-break: break-word; overflow-wrap: anywhere; }",
        ".arpse-export-table > thead > tr > th { background: #8399c7; color: #000; font-weight: bold; text-align: center; }",
        ".arpse-export-table > tbody > tr > td { text-align: left; }",
        ".num { text-align: left; }",
        ".rich-html, .rich-html * { max-width: 100%; box-sizing: border-box; overflow-wrap: anywhere; word-break: break-word; vertical-align: top; text-align: left; }",
        ".rich-html p, .rich-html div, .rich-html span { margin: 0 0 6px 0; max-width: 100%; box-sizing: border-box; overflow-wrap: anywhere; word-break: break-word; text-align: left; vertical-align: top; }",
        ".rich-html ul, .rich-html ol { margin: 0 0 6px 18px; padding: 0; }",
        ".rich-html b, .rich-html strong { font-weight: bold; }",
        ".rich-html u { text-decoration: underline; }",
        ".rich-html table { border-collapse: collapse; table-layout: fixed; width: 100%; max-width: 100%; margin: 0 0 6px 0; box-sizing: border-box; }",
        ".rich-html table th, .rich-html table td { border: 1px solid #9ca3af; padding: 3px; vertical-align: top; text-align: left; white-space: normal; word-break: break-word; overflow-wrap: anywhere; max-width: 100%; box-sizing: border-box; font-size: 8pt; }",
        ".rich-html table th { background: #eef2f7; font-weight: bold; text-align: center; }"
    ].join("");
}

function getArpseYearReportPdfRenderStyles() {
    return [
        "html, body { margin: 0; padding: 0; background: #fff; }",
        ".WordSection1 { width: 1344px; min-height: 816px; padding: 34px; box-sizing: border-box; background: #fff; overflow: visible; }",
        ".arpse-export-table { width: 100%; max-width: 100%; table-layout: fixed; border-collapse: collapse; box-sizing: border-box; }",
        ".arpse-export-table > thead > tr > th, .arpse-export-table > tbody > tr > td { box-sizing: border-box; vertical-align: top; overflow-wrap: anywhere; word-break: break-word; }",
        ".rich-html, .rich-html * { max-width: 100%; box-sizing: border-box; }",
        ".rich-html table { width: 100% !important; max-width: 100% !important; table-layout: fixed !important; border-collapse: collapse !important; box-sizing: border-box !important; display: table; }",
        ".rich-html thead, .rich-html tbody, .rich-html tfoot { max-width: 100%; }",
        ".rich-html tr { page-break-inside: avoid; break-inside: avoid; }",
        ".rich-html th, .rich-html td { max-width: 100%; box-sizing: border-box; overflow-wrap: anywhere; word-break: break-word; vertical-align: top; }"
    ].join("");
}

function getExportCellHtml(node, fallback) {
    var html = "";
    if (node && typeof node.innerHTML === "string") {
        html = node.innerHTML;
    } else if (typeof fallback === "string") {
        html = fallback;
    } else if (fallback !== null && fallback !== undefined) {
        html = String(fallback);
    }

    return normalizeArpseExportHtml(html);
}

function prepareArpseExportTableHtml(table) {
    Array.from(table.querySelectorAll("th,td")).forEach(function (cell) {
        cell.innerHTML = normalizeArpseExportHtml(cell.innerHTML);
    });
}

function normalizeArpseExportHtml(html) {
    var template = document.createElement("template");
    template.innerHTML = String(html || "");

    normalizeArpseExportBreaks(template.content);
    applyArpseExportHtmlConstraints(template.content);

    return template.innerHTML;
}

function normalizeArpseExportBreaks(root) {
    Array.from(root.querySelectorAll("br")).forEach(function (br) {
        var next = br.nextSibling;
        var consecutiveBreaks = 0;

        while (next) {
            if (next.nodeType === 3 && !String(next.nodeValue || "").trim()) {
                next = next.nextSibling;
                continue;
            }

            if (next.nodeType === 1 && next.tagName === "BR") {
                consecutiveBreaks++;
                var duplicate = next;
                next = next.nextSibling;
                if (consecutiveBreaks > 1) {
                    duplicate.parentNode.removeChild(duplicate);
                }
                continue;
            }

            break;
        }
    });
}

function applyArpseExportHtmlConstraints(root) {
    Array.from(root.querySelectorAll("table")).forEach(function (table) {
        table.setAttribute("border", "1");
        table.setAttribute("cellspacing", "0");
        table.setAttribute("cellpadding", "3");
        mergeArpseExportStyle(table, "border-collapse:collapse;width:100%;max-width:100%;table-layout:fixed;box-sizing:border-box;overflow-wrap:anywhere;word-break:break-word;");
    });

    Array.from(root.querySelectorAll("td,th")).forEach(function (cell) {
        cell.setAttribute("valign", "top");
        var alignment = cell.tagName === "TH" ? "center" : "left";
        mergeArpseExportStyle(cell, "border:1px solid #9ca3af;vertical-align:top;text-align:" + alignment + ";max-width:100%;box-sizing:border-box;overflow-wrap:anywhere;word-break:break-word;");
    });

    Array.from(root.querySelectorAll("div,span,p")).forEach(function (element) {
        mergeArpseExportStyle(element, "max-width:100%;width:100%;box-sizing:border-box;overflow-wrap:anywhere;word-break:break-word;text-align:left;vertical-align:top;");
    });
}

function mergeArpseExportStyle(element, styleText) {
    var existing = String(element.getAttribute("style") || "").trim();
    var merged = existing ? existing.replace(/;?\s*$/, ";") + styleText : styleText;
    element.setAttribute("style", merged);
}

function getDelimitedExportTextFromHtml(html) {
    var template = document.createElement("template");
    template.innerHTML = decodeHtmlRepeatedly(html || "");
    return collectDelimitedExportText(template.content)
        .replace(/\u00a0/g, " ")
        .replace(/[ \t]+\n/g, "\n")
        .replace(/\n{3,}/g, "\n\n")
        .trim();
}

function collectDelimitedExportText(node) {
    var parts = [];

    Array.from(node.childNodes || []).forEach(function (child) {
        if (child.nodeType === 3) {
            parts.push(child.nodeValue || "");
            return;
        }

        if (child.nodeType !== 1) {
            return;
        }

        if (child.tagName === "BR") {
            parts.push("\n");
            return;
        }

        parts.push(collectDelimitedExportText(child));

        if (/^(P|DIV|LI|TR|TABLE)$/.test(child.tagName)) {
            parts.push("\n");
        } else if (/^(TD|TH)$/.test(child.tagName)) {
            parts.push(" ");
        }
    });

    return parts.join("");
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
