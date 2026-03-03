    var g_engId = 0;
    var g_sampleId = 0;
    var g_loanStatus = 0;
    var g_indicator="L";
    var g_sampleType = "";
    $(document).ready(function () {
        var url_string = window.location;
        var url = new URL(url_string);
        g_engId = url.searchParams.get("engId");
        g_sampleId = url.searchParams.get("sample_id");
        g_loanStatus = url.searchParams.get("loan_status");
        g_sampleType = url.searchParams.get("sample_type");
        if(g_sampleType){
            $("#sampleTypeHeader").text(g_sampleType);
        }
        loadLoanSamples();
    });


    function loadLoanSamples() {

        $.ajax({
             url: g_asiBaseURL + "/ApiCalls/get_loan_samples",
            type: "POST",
               cache: false,

            data: {
                'ENG_ID': g_engId,
                'INDICATOR':g_indicator,
                'STATUS_ID':g_loanStatus,
                'SAMPLE_ID':g_sampleId
            },
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
        var tableBody = $("#loan_sample_list tbody");
        destroyDatatable('loan_sample_list'); // Clear existing DataTable

        $.each(data, function (index, item) {
            var row = `<tr>
                <td>${index + 1}</td>
                <td>${item.type}</td>
                <td>${item.scheme}</td>
                <td>${item.l_PURPOSE}</td>
                <td>${item.lC_NO}</td>
                <td>${item.cnic}</td>
                <td>${item.customername}</td>
                <td>${item.apP_DATE_DISP || item.app_DATE_DISP || item.appDateDisp || item.apP_DATE || item.app_DATE || ""}</td>
                <td>${item.disB_DATE_DISP || item.disb_DATE_DISP || item.disbDateDisp || item.disB_DATE || item.disb_DATE || ""}</td>
                <td>${formatCurrency(item.deV_AMOUNT)}</td>
                <td>${formatCurrency(item.outstanding)}</td>
                <td><a href="#" onclick="event.preventDefault();getLoanTransactions(${item.loaN_DISB_ID});" >View Transactions</a></td>
                <td><a href="#" onclick="event.preventDefault();getLoanDocuments(${item.loaN_DISB_ID});" >View Documents</a></td>
            </tr>`;
            tableBody.append(row);
        });

        initializeDataTable('loan_sample_list'); // Reinitialize DataTable
        $('#wait').hide();
    }

    // Function to format amounts properly
     function formatCurrency(amount) {
        if (isNaN(amount) || amount == null) return "₨0.00";
        return parseFloat(amount).toLocaleString('en-PK', { style: 'currency', currency: 'PKR' });
    }

    function getLoanTransactions (l_disb_id){
            window.location.href=  "./loan_transactions?engId="+g_engId+"&disbId="+l_disb_id+"&sample_type="+encodeURIComponent(g_sampleType);
        }
          function getLoanDocuments (l_disb_id){
            window.location.href=  "./loan_documents?engId="+g_engId+"&disbId="+l_disb_id+"&sample_type="+encodeURIComponent(g_sampleType);
        }
