var g_iidLoanTxnEngId = 0;
var g_iidLoanTxnDisbId = 0;
var g_iidLoanTxnSampleType = "";

$(document).ready(function () {
    var url = new URL(window.location.href);
    g_iidLoanTxnEngId = url.searchParams.get("engId") || 0;
    g_iidLoanTxnDisbId = url.searchParams.get("disbId") || 0;
    g_iidLoanTxnSampleType = url.searchParams.get("sample_type") || "";
    if (g_iidLoanTxnSampleType) {
        $("#sampleTypeHeader").text(g_iidLoanTxnSampleType);
    }
    loadLoanTransactions();
});

function loadLoanTransactions() {
    $("#wait").show();
    destroyDatatable("account_transaction_list");
    $.ajax({
        url: g_asiBaseURL + "/ApiCalls/get_iid_sample_loan_transactions",
        type: "POST",
        data: {
            LOAN_DISB_ID: g_iidLoanTxnDisbId,
            ENG_ID: g_iidLoanTxnEngId
        },
        success: function (data) {
            if (Array.isArray(data) && data.length > 0) {
                populateTable(data);
            } else {
                $("#account_transaction_list tbody").html('<tr><td colspan="13" class="text-center text-muted">No data found.</td></tr>');
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
            "<td>" + (item.lN_ACCOUNT_ID || item.LN_ACCOUNT_ID || "") + "</td>" +
            "<td>" + (item.manuaL_VOUCHER_NO || item.MANUAL_VOUCHER_NO || "") + "</td>" +
            "<td>" + (item.description || item.DESCRIPTION || "") + "</td>" +
            "<td>" + (item.transactioN_DATE_DISP || item.TRANSACTION_DATE_DISP || item.transactionDateDisp || item.transactioN_DATE || item.TRANSACTION_DATE || "") + "</td>" +
            "<td>" + formatAmount(item.dR_AMOUNT || item.DR_AMOUNT) + "</td>" +
            "<td>" + formatAmount(item.cR_AMOUNT || item.CR_AMOUNT) + "</td>" +
            "<td>" + (item.mcO_RECEIPT_NO || item.MCO_RECEIPT_NO || "") + "</td>" +
            "<td>" + (item.mcO_BOOK_NO || item.MCO_BOOK_NO || "") + "</td>" +
            "<td>" + (item.authorizatioN_DATE_DISP || item.AUTHORIZATION_DATE_DISP || item.authorizationDateDisp || item.authorizatioN_DATE || item.AUTHORIZATION_DATE || "") + "</td>" +
            "<td>" + (item.rejectioN_DATE_DISP || item.REJECTION_DATE_DISP || item.rejectionDateDisp || item.rejectioN_DATE || item.REJECTION_DATE || "") + "</td>" +
            "<td>" + (item.reversaL_DATE_DISP || item.REVERSAL_DATE_DISP || item.reversalDateDisp || item.reversaL_DATE || item.REVERSAL_DATE || "") + "</td>" +
            "<td>" + (item.remarks || item.REMARKS || "") + "</td>" +
            "</tr>";
        tableBody.append(row);
    });

    initializeDataTable("account_transaction_list");
}

function formatAmount(amount) {
    var value = parseFloat(amount);
    return isNaN(value) ? "0.00" : value.toFixed(2);
}

