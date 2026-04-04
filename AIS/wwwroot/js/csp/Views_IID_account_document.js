var g_iidDocEngId = 0;
var g_iidAccountNo = 0;
var g_iidSampleType = "";

$(document).ready(function () {
    var url = new URL(window.location.href);
    g_iidDocEngId = url.searchParams.get("engId") || 0;
    g_iidAccountNo = url.searchParams.get("acNo") || 0;
    g_iidSampleType = url.searchParams.get("sample_type") || "";
    if (g_iidSampleType) {
        $("#sampleTypeHeader").text(g_iidSampleType);
    }
    loadAccountDocuments();
});

function loadAccountDocuments() {
    $("#wait").show();
    destroyDatatable("account_document_list");
    $.ajax({
        url: g_asiBaseURL + "/ApiCalls/get_iid_biomet_account_documents_sampling_details",
        type: "POST",
        data: {
            AC_NO: g_iidAccountNo
        },
        success: function (data) {
            if (Array.isArray(data) && data.length > 0) {
                populateTable(data);
            } else {
                $("#account_document_list tbody").html('<tr><td colspan="6" class="text-center text-muted">No data found.</td></tr>');
            }
        },
        dataType: "json",
        complete: function () {
            $("#wait").hide();
        }
    });
}

function populateTable(data) {
    var tableBody = $("#account_document_list tbody");
    tableBody.empty();
    $.each(data, function (index, item) {
        var row = "<tr>" +
            "<td>" + (index + 1) + "</td>" +
            "<td>" + (item.oldAccountNo || item.OldAccountNo || "") + "</td>" +
            "<td>" + (item.pageNo || item.PageNo || "") + "</td>" +
            "<td>" + (item.name || item.Name || "") + "</td>" +
            "<td>" + (item.docRemarks || item.DocRemarks || "") + "</td>" +
            "<td><a href='javascript:void(0);' data-doc-image='" + (item.docImage || item.DocImage || "") + "' class='btn btn-info btn-sm js-iid-view-account-document'>View Document</a></td>" +
            "</tr>";
        tableBody.append(row);
    });
    initializeDataTable("account_document_list");
}

$(document).off("click.iidAccountDocument", ".js-iid-view-account-document").on("click.iidAccountDocument", ".js-iid-view-account-document", function () {
    viewDocument($(this).data("doc-image"));
});

function viewDocument(docImage) {
    if (!docImage) {
        alert("No document data received.");
        return;
    }

    docImage = String(docImage).trim();

    var win = window.open("", "_blank");
    if (!win) {
        alert("Please allow popups to view the document.");
        return;
    }

    var mime = "";
    if (docImage.indexOf("JVBERi0xL") === 0) {
        mime = "application/pdf";
    } else if (docImage.indexOf("iVBOR") === 0) {
        mime = "image/png";
    } else if (docImage.indexOf("/9j/") === 0) {
        mime = "image/jpeg";
    } else if (docImage.indexOf("R0lGOD") === 0) {
        mime = "image/gif";
    } else if (docImage.indexOf("Qk") === 0) {
        mime = "image/bmp";
    } else if (docImage.indexOf("SUkq") === 0) {
        mime = "image/tiff";
    }

    if (mime === "application/pdf") {
        win.location.href = "data:" + mime + ";base64," + docImage;
    } else if (mime.indexOf("image/") === 0) {
        var imageSrc = "data:" + mime + ";base64," + docImage;
        win.document.write("<html><head><title>Document Viewer</title></head><body style='margin:0; text-align:center; background:#f8f8f8;'><img src='" + imageSrc + "' style='max-width:90vw; max-height:90vh; object-fit:contain;' /></body></html>");
        win.document.close();
    } else {
        alert("The document type is not supported.");
        win.close();
    }
}

