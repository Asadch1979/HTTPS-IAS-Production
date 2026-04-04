var g_iidLoanDocEngId = 0;
var g_iidLoanDisbId = 0;
var g_iidLoanDocSampleType = "";

$(document).ready(function () {
    var url = new URL(window.location.href);
    g_iidLoanDocEngId = url.searchParams.get("engId") || 0;
    g_iidLoanDisbId = url.searchParams.get("disbId") || 0;
    g_iidLoanDocSampleType = url.searchParams.get("sample_type") || "";
    if (g_iidLoanDocSampleType) {
        $("#sampleTypeHeader").text(g_iidLoanDocSampleType);
    }
    loadLoanDocuments();
});

function loadLoanDocuments() {
    $("#wait").show();
    destroyDatatable("loan_document_list");
    $.ajax({
        url: g_asiBaseURL + "/ApiCalls/get_iid_loan_documents",
        type: "POST",
        data: {
            LOAN_DISB_ID: g_iidLoanDisbId,
            ENG_ID: g_iidLoanDocEngId
        },
        success: function (data) {
            if (Array.isArray(data) && data.length > 0) {
                populateTable(data);
            } else {
                $("#loan_document_list tbody").html('<tr><td colspan="6" class="text-center text-muted">No data found.</td></tr>');
            }
        },
        dataType: "json",
        error: function (xhr, status, error) {
            console.error("AJAX Error:", status, error);
        },
        complete: function () {
            $("#wait").hide();
        }
    });
}

function populateTable(data) {
    var tableBody = $("#loan_document_list tbody");
    tableBody.empty();

    $.each(data, function (index, item) {
        var imageId = item.imagE_ID || item.IMAGE_ID || item.imageId || "";
        var docName = item.doC_NAME || item.DOC_NAME || item.docName || "";
        var row = "<tr>" +
            "<td>" + (index + 1) + "</td>" +
            "<td>" + (item.brancH_CODE || item.BRANCH_CODE || "") + "</td>" +
            "<td>" + (item.loaN_CASE_NO || item.LOAN_CASE_NO || "") + "</td>" +
            "<td>" + (item.customeR_NAME || item.CUSTOMER_NAME || "") + "</td>" +
            "<td>" + docName + "</td>" +
            "<td class='text-center'>" +
                (imageId ? "<a href='#' data-image-id='" + imageId + "' data-doc-name='" + docName + "' class='btn btn-info btn-sm js-iid-loan-document-view'>View Document</a>" : "<span class='text-muted'>No Image</span>") +
            "</td>" +
            "</tr>";
        tableBody.append(row);
    });

    initializeDataTable("loan_document_list");
}

$(document).off("click.iidLoanDocumentView", ".js-iid-loan-document-view").on("click.iidLoanDocumentView", ".js-iid-loan-document-view", function (event) {
    event.preventDefault();
    viewDocument($(this).data("image-id"), $(this).data("doc-name"));
});

function viewDocument(docId, docName) {
    if (!docId) {
        alert("Document ID is missing.");
        return;
    }

    $.ajax({
        url: g_asiBaseURL + "/ApiCalls/get_iid_loan_document_data",
        type: "POST",
        data: { IMAGE_ID: docId },
        dataType: "json",
        success: function (data) {
            var first = Array.isArray(data) && data.length ? data[0] : null;
            var docImage = first ? (first.imagE_DATA || first.IMAGE_DATA || first.imageData || "") : "";
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

            var extension = "";
            if (docName && docName.indexOf(".") > -1) {
                extension = docName.split(".").pop().toLowerCase();
            }

            var mime = "";
            if (extension === "pdf" || docImage.indexOf("JVBERi0xL") === 0) {
                mime = "application/pdf";
            } else if (extension === "png" || docImage.indexOf("iVBOR") === 0) {
                mime = "image/png";
            } else if (extension === "gif" || docImage.indexOf("R0lGOD") === 0) {
                mime = "image/gif";
            } else if (extension === "bmp" || docImage.indexOf("Qk") === 0) {
                mime = "image/bmp";
            } else if (extension === "tiff" || extension === "tif" || docImage.indexOf("SUkq") === 0) {
                mime = "image/tiff";
            } else if (extension === "jpeg" || extension === "jpg" || docImage.indexOf("/9j/") === 0) {
                mime = "image/jpeg";
            }

            if (mime === "application/pdf") {
                var pdfSrc = "data:" + mime + ";base64," + docImage;
                win.document.write("<html><body style='margin:0;'><embed src='" + pdfSrc + "' type='application/pdf' width='100%' height='100%'/></body></html>");
                win.document.close();
            } else if (mime.indexOf("image/") === 0) {
                var imageSrc = "data:" + mime + ";base64," + docImage;
                win.document.write("<html><body style='margin:0;'><img src='" + imageSrc + "' style='width:100%; height:auto;'/></body></html>");
                win.document.close();
            } else {
                alert("The document type is not supported.");
                win.close();
            }
        },
        error: function (xhr, status, error) {
            console.error("AJAX Error:", status, error);
        }
    });
}

