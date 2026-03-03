    var g_engId = 0;
    var g_sampleType = "";
    $(document).ready(function () {
        var url_string = window.location;
        var url = new URL(url_string);

       g_engId = url.searchParams.get("engId");
        g_sampleID  = url.searchParams.get("sample_id");
        g_sampleType = url.searchParams.get("sample_type");

       loadBIOMETSamples();
    });


    function loadBIOMETSamples() {
       destroyDatatable('biomet_sample_list');
        $.ajax({
             url: g_asiBaseURL + "/ApiCalls/get_biomet_sampling_details",
            type: "POST",
               cache: false,
            data: { ENG_ID: g_engId },
            success: function (data) {
                if (data.length > 0) {
                    populateTable(data);
                }
            },
            dataType: "json",
        });
    }

    function populateTable(data) {
        $('#wait').show();
        var tableBody = $("#biomet_sample_list tbody");
        tableBody.empty(); // Clear existing rows

        $.each(data, function (index, item) {
               var row = `<tr>
        <td>${index + 1}</td>
        <td>${item.accounT_NO}</td>
        <td>${item.accounT_TITLE}</td>
        <td>${item.customeR_NAME}</td>
        <td>${item.dob_DISP || item.doB_DISP || item.dob || ""}</td>
        <td>${item.phonE_CELL}</td>
        <td>${item.cnic}</td>
        <td>${item.cniC_EXPIRY_DATE_DISP || item.cniC_EXPIRY_DATE || ""}</td>
        <td>${item.accounT_TYPE}</td>
        <td>${item.accounT_CATEGORY}</td>
        <td>
            <a href="./account_document?engId=${g_engId}&acNo=${item.accounT_NO}&sample_type=${encodeURIComponent(g_sampleType)}" class="btn btn-primary btn-sm">Documents</a>
        </td>
        <td>
            <a href="./account_transaction?engId=${g_engId}&acNo=${item.accounT_NO}" class="btn btn-success btn-sm">Transactions</a>
        </td>
    </tr>`;
            tableBody.append(row);
        });

        initializeDataTable('biomet_sample_list');
        $('#wait').hide();
    }
