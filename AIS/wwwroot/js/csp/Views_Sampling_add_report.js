    var g_engID = 0;
    var g_reportId = 0;
    var g_action = "A";
    $(document).ready(function () {
        var url_string = window.location;
        var url = new URL(url_string);
        //g_engID = url.searchParams.get("engId");
        CommonValidation.attachAlnumOnly("#reportName", { allowSpace: true, maxLen: 100 });
        loadReports();
    });

    function loadReports() {
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_list_of_reports",
            type: "POST",
            cache: false,
            data: { 'ENG_ID': g_engID },
            success: function (data) {
                if (data.length > 0) {
                    populateTable(data);
                }
            },
            dataType: "json"
        });
    }

    function populateTable(data) {
        $('#wait').show();
        destroyDatatable('reportList');
        let tableBody = $("#reportList tbody");
        tableBody.empty();
        data.forEach((item, index) => {
            let row = `<tr>
                <td>${index + 1}</td>
                <td>${item.reporT_TITLE}</td>
                <td class="text-center">${item.discription}</td>
    <td class="text-center"><button class="btn btn-primary btn-sm" data-onclick="openUpdateModal(${item.reporT_ID}, '${item.reporT_TITLE}', '${item.discription}', '${item.reporT_INDICATOR}', ${item.loaN_STATUS})">Update</button></td>            </tr>`;
            tableBody.append(row);
        });
        initializeDataTable("reportList");
        $('#wait').hide();
    }

    function openAddModal() {
        g_reportId = 0;
        g_action = 'A';
        $('#modalTitle').text('Add Exception Report');
        $('#reportName').val('');
        $('#thingsCheck').val('');
        $('#reportType').val('L');
        $('#addReportModal').modal('show');
    }

    function openUpdateModal(id, title, desc, type, statusId) {
        g_reportId = id;
        g_action = 'U';
        $('#modalTitle').text('Update Exception Report');
        $('#reportName').val(title);
        $('#thingsCheck').val(desc);
        $('#reportType').val(type);
        $('#loanStatus').val(statusId);
        $('#addReportModal').modal('show');
    }

    function saveReport() {
        if (!CommonValidation.isAlnumOk("#reportName", { allowSpace: true, required: true })) {
            alert("Report Name List must contain only letters, numbers, and spaces.");
            return;
        }

        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/add_exception_account_report",
            type: "POST",
            data: {
                'IND': g_action,
                'REPORT_ID': g_reportId,
                'REPORT_TITLE': $('#reportName').val(),
                'DESCRIPTION': $('#thingsCheck').val(),
                'TYPE': $('#reportType').val(),
                'LOAN_STATUS_ID': $('#loanStatus').val()
            },
            success: function (data) {
                if (data && data.Status) {
                    $('#addReportModal').modal('hide');
                    loadReports();
                }
            },
            error: function (xhr) {
                showApiAlertFromXhr(xhr, xhr ? xhr.status : null, getErrorReferenceIdFromXhr(xhr), "Unable to save report. Please check your input.");
            }
        });
    }
