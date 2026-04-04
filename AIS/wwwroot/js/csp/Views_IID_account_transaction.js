var g_iidTxnEngId = 0;
var g_iidTxnAccountNo = 0;

$(document).ready(function () {
    var url = new URL(window.location.href);
    g_iidTxnEngId = url.searchParams.get("engId") || 0;
    g_iidTxnAccountNo = url.searchParams.get("acNo") || 0;
    loadAccountTransactions();
});

function loadAccountTransactions() {
    $("#wait").show();
    destroyDatatable("account_transaction_list");
    $.ajax({
        url: g_asiBaseURL + "/ApiCalls/get_iid_biomet_account_transaction_sampling_details",
        type: "POST",
        data: {
            ENG_ID: g_iidTxnEngId,
            AC_NO: g_iidTxnAccountNo
        },
        success: function (data) {
            if (Array.isArray(data) && data.length > 0) {
                populateTable(data);
            } else {
                $("#account_transaction_list tbody").html('<tr><td colspan="9" class="text-center text-muted">No data found.</td></tr>');
            }
        },
        error: function (xhr, status, error) {
            console.error("Error fetching data:", error);
        },
        complete: function () {
            $("#wait").hide();
        }
    });
}

function populateTable(data) {
    var tableBody = $("#account_transaction_list tbody");
    tableBody.empty();

    $.each(data, function (index, item) {
        var row = "<tr>" +
            "<td>" + (index + 1) + "</td>" +
            "<td>" + (item.transactionMasterCode || item.TransactionMasterCode || "") + "</td>" +
            "<td>" + (item.instrumentNo || item.InstrumentNo || "") + "</td>" +
            "<td>" + (item.description || item.Description || "") + "</td>" +
            "<td>" + (item.transactionDateDisp || item.TransactionDateDisp || item.transactionDate || item.TransactionDate || "") + "</td>" +
            "<td>" + (item.authorizationDateDisp || item.AuthorizationDateDisp || item.authorizationDate || item.AuthorizationDate || "") + "</td>" +
            "<td>" + formatAmount(item.drAmount || item.DrAmount) + "</td>" +
            "<td>" + formatAmount(item.crAmount || item.CrAmount) + "</td>" +
            "<td>" + (item.remarks || item.Remarks || "") + "</td>" +
            "</tr>";
        tableBody.append(row);
    });

    initializeDataTable("account_transaction_list");
}

function formatAmount(amount) {
    var value = parseFloat(amount);
    return isNaN(value) ? "0.00" : value.toFixed(2);
}

