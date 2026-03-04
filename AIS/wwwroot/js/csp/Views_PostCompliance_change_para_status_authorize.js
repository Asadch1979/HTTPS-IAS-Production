    var g_obsList = [];
    var g_comId = "";
    var g_newId = 0;
    var g_oldId = 0;
    var g_ind = "";
    var g_action = "A";

    $(document).ready(function () {
        loadRequests();
    });

    function loadRequests() {
        $('#manageObsPanel tbody').empty();
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_paras_for_status_change_authorize",
            type: "POST",
            data: {},
            cache: false,
            success: function (data) {
                g_obsList = data;
                $.each(data, function (index, child) {
                    $('#manageObsPanel tbody').append('<tr>' +
                        '<td>' + (child.parA_NO || '') + '</td>' +
                        '<td>' + (child.audiT_PERIOD || '') + '</td>' +
                         '<td>' + (child.gisT_OF_PARAS || '') + '</td>' +
                        '<td>' + (child.parA_STATUS || '') + '</td>' +
                        '<td>' + (child.neW_PARA_STATUS || '') + '</td>' +                        
                        '<td class="text-center"><a class="text-success" style="cursor:pointer" data-onclick="openAction(\'' + child.coM_ID + '\',' + child.NEW_PARA_ID + ',' + child.OLD_PARA_ID + ',\'' + child.ind + '\',\'A\')">Approve</a> | <a class="text-danger" style="cursor:pointer" data-onclick="openAction(\'' + child.coM_ID + '\',' + child.NEW_PARA_ID + ',' + child.OLD_PARA_ID + ',\'' + child.ind + '\',\'R\')">Reject</a></td>' +                        '</tr>');
                });
            },
            dataType: 'json'
        });
    }

    function openAction(comId, newId, oldId, ind, act) {
        g_comId = comId;
        g_newId = newId;
        g_oldId = oldId;
        g_ind = ind;
        g_action = act;
        $('#RemarkField').val('');
        $('#process_detail').modal('show');
    }

    function submitAction() {
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/authorize_para_change_status",
            type: "POST",
            data: {
                'COM_ID': g_comId,
                'NEW_PARA_ID': g_newId,
                'OLD_PARA_ID': g_oldId,
                'REMARKS': $('#RemarkField').val(),
                'IND': g_ind,
                'Action_IND': g_action
            },
            cache: false,
            success: function (data) {
                showApiAlert(data);
                $('#process_detail').modal('hide');
                loadRequests();
            },
            dataType: 'json'
        });
    }
