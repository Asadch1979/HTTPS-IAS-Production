    var g_engId = 0;
    var g_accountNo = 0;

    $(document).ready(function () {
        var url_string = window.location.href;
        var url = new URL(url_string);
        g_engId = url.searchParams.get("engId");
        g_accountNo = url.searchParams.get("acNo");
        loadAccountTransactions();
    });

    function loadAccountTransactions() {
        $('#wait').show();
        destroyDatatable('account_transaction_list');
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_biomet_account_transaction_sampling_details",
            type: "POST",
            data: {
                ENG_ID: g_engId,
                AC_NO: g_accountNo
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
      //  tableBody.empty(); // Clear existing rows

        $.each(data, function (index, item) {
            var row = `<tr>
                <td>${index + 1}</td>
                <td>${item.transactionMasterCode}</td>
                <td>${item.instrumentNo}</td>
                 <td>${item.description}</td>
                <td>${item.transactionDateDisp || item.transactionDate || ""}</td>
                <td>${item.authorizationDateDisp || item.authorizationDate || ""}</td>
                <td>${formatAmount(item.drAmount)}</td>
                <td>${formatAmount(item.crAmount)}</td>

                <td>${item.remarks}</td>


            </tr>`;
            tableBody.append(row);
        });
        initializeDataTable('account_transaction_list');
    }

    function formatAmount(amount) {
        return amount ? parseFloat(amount).toFixed(2) : "0.00";
    }
