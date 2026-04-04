var g_iidLoanEngId = 0;
var g_iidLoanStatus = 0;
var g_iidLoanIndicator = "L";

$(document).ready(function () {
    var url = new URL(window.location.href);
    g_iidLoanEngId = url.searchParams.get("engId") || 0;
    g_iidLoanStatus = url.searchParams.get("loan_status") || url.searchParams.get("loaN_STATUS") || 0;
    loadLoanSamples();
});

function loadLoanSamples() {
    $.ajax({
        url: g_asiBaseURL + "/ApiCalls/get_iid_loan_Exceptions",
        type: "POST",
        cache: false,
        data: {
            ENG_ID: g_iidLoanEngId,
            INDICATOR: g_iidLoanIndicator,
            STATUS_ID: g_iidLoanStatus
        },
        success: function (data) {
            if (Array.isArray(data) && data.length > 0) {
                populateTable(data);
            } else {
                destroyDatatable("loan_sample_list");
                $("#loan_sample_list tbody").html('<tr><td colspan="13" class="text-center text-muted">No data found.</td></tr>');
            }
        },
        error: function () {
            destroyDatatable("loan_sample_list");
            $("#loan_sample_list tbody").html('<tr><td colspan="13" class="text-center text-danger">Failed to load records.</td></tr>');
        },
        dataType: "json"
    });
}

function populateTable(data) {
    $("#wait").show();
    destroyDatatable("loan_sample_list");
    var tableBody = $("#loan_sample_list tbody");
    tableBody.empty();

    $.each(data, function (index, item) {
        var loanDisbId = item.loaN_DISB_ID || item.loanDisbId || item.LOAN_DISB_ID || "";
        var row = "<tr>" +
            "<td>" + (index + 1) + "</td>" +
            "<td>" + (item.type || item.TYPE || "") + "</td>" +
            "<td>" + (item.scheme || item.SCHEME || "") + "</td>" +
            "<td>" + (item.l_PURPOSE || item.L_PURPOSE || "") + "</td>" +
            "<td>" + (item.lC_NO || item.LC_NO || "") + "</td>" +
            "<td>" + (item.cnic || item.CNIC || "") + "</td>" +
            "<td>" + (item.customername || item.CUSTOMERNAME || "") + "</td>" +
            "<td>" + (item.apP_DATE_DISP || item.app_DATE_DISP || item.appDateDisp || item.APP_DATE_DISP || "") + "</td>" +
            "<td>" + (item.disB_DATE_DISP || item.disb_DATE_DISP || item.disbDateDisp || item.DISB_DATE_DISP || "") + "</td>" +
            "<td>" + formatCurrency(item.deV_AMOUNT || item.DEV_AMOUNT) + "</td>" +
            "<td>" + formatCurrency(item.outstanding || item.OUTSTANDING) + "</td>" +
            "<td><a href='#' data-disb-id='" + loanDisbId + "' class='js-iid-loan-transactions'>View Transactions</a></td>" +
            "<td><a href='#' data-disb-id='" + loanDisbId + "' class='js-iid-loan-documents'>View Documents</a></td>" +
            "</tr>";
        tableBody.append(row);
    });

    initializeDataTable("loan_sample_list");
    $("#wait").hide();
}

function formatCurrency(amount) {
    var value = parseFloat(amount);
    if (isNaN(value)) {
        return "PKR 0.00";
    }
    return "PKR " + value.toLocaleString("en-PK", { minimumFractionDigits: 2, maximumFractionDigits: 2 });
}

$(document).off("click.iidLoanExceptions", ".js-iid-loan-transactions").on("click.iidLoanExceptions", ".js-iid-loan-transactions", function (event) {
    event.preventDefault();
    getLoanTransactions($(this).data("disb-id"));
});

$(document).off("click.iidLoanExceptionsDocs", ".js-iid-loan-documents").on("click.iidLoanExceptionsDocs", ".js-iid-loan-documents", function (event) {
    event.preventDefault();
    getLoanDocuments($(this).data("disb-id"));
});

function getLoanTransactions(loanDisbId) {
    window.location.href = "./loan_transactions?engId=" + encodeURIComponent(g_iidLoanEngId) + "&disbId=" + encodeURIComponent(loanDisbId || "");
}

function getLoanDocuments(loanDisbId) {
    window.location.href = "./loan_documents?engId=" + encodeURIComponent(g_iidLoanEngId) + "&disbId=" + encodeURIComponent(loanDisbId || "");
}

