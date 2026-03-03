    var g_engId = 0;
    var g_loanDisbId = 0;
    var g_sampleType = "";

    $(document).ready(function () {
        var url_string = window.location.href;
        var url = new URL(url_string);
          g_engId = url.searchParams.get("engId");
        g_loanDisbId = url.searchParams.get("disbId");
        g_sampleType = url.searchParams.get("sample_type");
        if(g_sampleType){
            $("#sampleTypeHeader").text(g_sampleType);
        }
        loadAccountTransactions();
    });

    function loadAccountTransactions() {
        $('#wait').show();
        destroyDatatable('account_transaction_list');
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_sample_loan_transactions",
            type: "POST",
            data: {
                'LOAN_DISB_ID': g_loanDisbId,
                'ENG_ID': g_engId
            },
            success: function (data) {
                if (data.length > 0) {
                    populateTable(data);
                }
            },
            error: function (xhr, status, error) {
                console.error("Error fetching data:", error);
            },
            complete: function(){
                $('#wait').hide();
            }
        });
    }

    function populateTable(data) {
        var tableBody = $("#account_transaction_list tbody");

        $.each(data, function (index, item) {
            var row = `<tr>
                <td>${index + 1}</td>
                  <td>${item.lN_ACCOUNT_ID}</td>
                <td>${item.manuaL_VOUCHER_NO}</td>
                <td>${item.description}</td>
                <td>${item.transactioN_DATE_DISP || item.transactionDateDisp || item.transactioN_DATE || item.transactionDate || ""}</td>
                <td>${formatAmount(item.dR_AMOUNT)}</td>
                <td>${formatAmount(item.cR_AMOUNT)}</td>
                <td>${item.mcO_RECEIPT_NO}</td>
                <td>${item.mcO_BOOK_NO}</td>
                <td>${item.authorizatioN_DATE_DISP || item.authorizationDateDisp || item.authorizatioN_DATE || item.authorizationDate || ""}</td>
                <td>${item.rejectioN_DATE_DISP || item.rejectionDateDisp || item.rejectioN_DATE || item.rejectionDate || ""}</td>
                <td>${item.reversaL_DATE_DISP || item.reversalDateDisp || item.reversaL_DATE || item.reversalDate || ""}</td>
                <td>${item.remarks}</td>
            </tr>`;
            tableBody.append(row);
        });

        initializeDataTable('account_transaction_list');
    }

    function formatAmount(amount) {
        return amount ? parseFloat(amount).toFixed(2) : "0.00";
    }
