    var g_engId = 0;
    var g_accountNo = 0;
    var g_sampleType = "";

    $(document).ready(function () {
        var url_string = window.location.href;
        var url = new URL(url_string);
        g_engId = url.searchParams.get("engId");
        g_accountNo = url.searchParams.get("acNo");
        g_sampleType = url.searchParams.get("sample_type");
        if(g_sampleType){
            $("#sampleTypeHeader").text(g_sampleType);
        }
        loadAccountDocuments();
    });

    function loadAccountDocuments() {
        $('#wait').show();
        destroyDatatable('account_document_list');
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_biomet_account_documents_sampling_details",
            type: "POST",
            data: {
                'AC_NO': g_accountNo
            },
            success: function (data) {
                if (data.length > 0) {
                    populateTable(data);
                }
            },
            dataType:"json",
            complete: function(){
                $('#wait').hide();
            }
        });
    }
    function populateTable(data) {
        var tableBody = $("#account_document_list tbody");
        tableBody.empty();
        $.each(data, function (index, item) {
            var row = `<tr>
                <td>${index + 1}</td>
                <td>${item.oldAccountNo}</td>
                <td>${item.pageNo}</td>
                <td>${item.name}</td>
                <td>${item.docRemarks}</td>
                <td>
                    <a href="javascript:void(0);" onclick="viewDocument('${item.docImage}')" class="btn btn-info btn-sm">View Document</a>
                </td>
            </tr>`;
            tableBody.append(row);
        });
        initializeDataTable('account_document_list');
    }
    function viewDocument(docImage) {
        if (!docImage) {
            alert("No document data received.");
            return;
        }

        docImage = docImage.trim();

        let win = window.open("", "_blank");
        if (!win) {
            console.error("Popup blocked");
            alert("Please allow popups to view the document.");
            return;
        }

        let mime = '';
        if (docImage.startsWith('JVBERi0xL')) {
            mime = 'application/pdf';
        } else if (docImage.startsWith('iVBOR')) {
            mime = 'image/png';
        } else if (docImage.startsWith('/9j/')) {
            mime = 'image/jpeg';
        } else if (docImage.startsWith('R0lGOD')) {
            mime = 'image/gif';
        } else if (docImage.startsWith('Qk')) {
            mime = 'image/bmp';
        } else if (docImage.startsWith('SUkq')) {
            mime = 'image/tiff';
        }

        if (mime === 'application/pdf') {
            let pdfSrc = `data:${mime};base64,` + docImage;
            win.location.href = pdfSrc;
        } else if (mime.startsWith('image/')) {
            let imageSrc = `data:${mime};base64,` + docImage;
            win.document.write(`
                <html>
                <head><title>Document Viewer</title></head>
                <body style="margin:0; text-align:center; background:#f8f8f8;">
                    <img src="${imageSrc}" style="max-width:90vw; max-height:90vh; object-fit:contain;" />
                <\/body>
                </html>
            `);
        } else {
            console.error("Unsupported document type");
            alert("The document type is not supported.");
            win.close();
        }
    }
