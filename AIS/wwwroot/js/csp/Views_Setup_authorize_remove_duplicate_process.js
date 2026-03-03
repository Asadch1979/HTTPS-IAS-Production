    var g_procId = 0;
    var g_subProcId = 0;
    var g_procDetailId = 0;
    var g_subProcList = [];
    var g_procDetailList = [];
    var g_procMergeIds=[];
    $(document).ready(function () {
        $("#searchTableRecord").on("keyup", function () {
            var value = $(this).val().toLowerCase();
            $("#authorizeMergeChecklistPanel tbody tr").filter(function () {
               
                $(this).toggle($(this).text().toLowerCase().indexOf(value) > -1)

            });
        });
       
    });


    function getProcessesForMergeDuplicate() {
        g_procMergeIds = [];
        var processId=$('#checklistSelectField').val();
        $('#authorizeMergeChecklistPanel tbody').empty();
        if (processId != 0) {
            $.ajax({
                url: g_asiBaseURL + "/ApiCalls/get_duplicate_Processes",
                type: "POST",
                data: {
                    'PROCESS_ID': processId
                },
                cache: false,
                success: function (data) {
                    
                    $.each(data, function (i, v) {
                        g_procMergeIds.push(v.m_ID);
                        $('#authorizeMergeChecklistPanel').append('<tr><td>' + v.m_ID + '</td><td>' + v.heading + '</td></tr>');

                    });
                },
                dataType: "json",
            });
        }
    }
 
    function authorizeMergeDuplicate() {
        var processId = $('#checklistSelectField').val();
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/authorize_merge_duplicate_process",
            type: "POST",
            data: {
                'PROCESS_ID': processId,
                'AUTH_P_IDS': g_procMergeIds,

            },
            cache: false,
            success: function (data) {
                showApiAlert(data);
                onAlertCallback(getProcessesForMergeDuplicate);
            },
            dataType: "json",
        });
    }
