    var g_procId = 0;
    var g_subProcId = 0;
    var g_procDetailId = 0;
    var g_subProcList = [];
    var g_procDetailList = [];
    $(document).ready(function () {
        $("#searchTableRecord").on("keyup", function () {
            var value = $(this).val().toLowerCase();
            $("#authorizeMergeChecklistPanel tbody tr").filter(function () {
               
                $(this).toggle($(this).text().toLowerCase().indexOf(value) > -1)

            });
        });

       
    });


    function getAuditChecklistDetailsForMergeDuplicate() {
        var processId = $('#checklistSelectField').val();       
        $('#authorizeMergeChecklistPanel tbody').empty();
        if (processId != 0) {
            $.ajax({
                url: g_asiBaseURL + "/ApiCalls/get_duplicate_checklists",
                type: "POST",
                data: {
                    'PROCESS_ID': processId
                },
                cache: false,
                success: function (data) {
                    $.each(data, function (i, v) {
                        $('#authorizeMergeChecklistPanel').append('<tr><td>' + v.id + '</td><td>' + v.heading + '</td></tr>');

                    });
                },
                dataType: "json",
            });
        }
    }
    function countDuplicate(){
        var processId = $('#checklistSelectField').val();
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_duplicate_checklists_count",
            type: "POST",
            data: {
                'PROCESS_ID': processId
            },
            cache: false,
            success: function (data) {
                console.log("newww count",data);
                $('#legacyObsLinked').html(data.olD_COUNT);
                $('#newObsLinked').html(data.neW_COUNT);
            },
            dataType: "json",
        });
    }
    function authorizeMergeDuplicate() {
        var processId = $('#checklistSelectField').val();
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/authorize_merge_duplicate_checklists",
            type: "POST",
            data: {
                'PROCESS_ID': processId
            },
            cache: false,
            success: function (data) {
                showApiAlert(data);
            },
            dataType: "json",
        });
    }
