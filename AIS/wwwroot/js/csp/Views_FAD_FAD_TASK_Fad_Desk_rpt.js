    $(document).ready(function () {
        $('#searchDeskReport').click(function () {
            if ($('#startDate').val() && $('#endDate').val()) {
                loadDeskReport();
            }
        });
    });

    function loadDeskReport() {
        destroyDatatable('fadDeskOfficerTable');
        $('#fadDeskOfficerTable tbody').empty();
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_fad_desk_officer_rpt_by_date_range",
            type: "POST",
            data: {
                "startDate": $('#startDate').val(),
                "endDate": $('#endDate').val()
            },
            cache: false,
            success: function (data) {
                $.each(data, function (i, v) {
                $('#fadDeskOfficerTable tbody').append('<tr><td>' + v.auditPeriod + '</td><td>' + v.childCode + '</td><td>' + v.cName + '</td><td>' + v.az + '</td><td>' + v.pName + '</td><td>' + v.annex + '</td><td>' + v.gistOfParas + '</td><td>' + v.paraNo + '</td><td>' + v.noOfInstances + '</td><td>' + v.risk + '</td><td>' + v.amount + '</td><td>' + v.status + '</td></tr>');                });
                initializeDataTable('fadDeskOfficerTable');
            },
            dataType: "json"
        });
    }
