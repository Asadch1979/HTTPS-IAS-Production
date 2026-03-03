    let g_obsList = [];
    let g_newComId = 0;
    let g_ind = 0;

    function appendOptions($select, data, valueProp, textProp, selectedVal) {
        $select.empty();
        $select.append('<option id="0" value="0">--Select--</option>');
        $.each(data, function (index, item) {
            let selected = (item[valueProp] == selectedVal) ? 'selected="selected"' : '';
            $select.append('<option ' + selected + ' value="' + item[valueProp] + '" id="' + item[valueProp] + '">' + item[textProp] + '</option>');
        });
    }

    function getrelation(parentEntityId = 0, userEntityId = 0) {
        $('#controlingsearch').empty();
        $('#childposting').empty();
        let relId = $('#RelationshipField option:selected').val();
        if (!relId || relId === "0") return;

        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/getparentrel",
            type: "POST",
            data: { 'ENTITY_REALTION_ID': relId },
            cache: false,
            success: function (data) {
                appendOptions($('#controlingsearch'), data, 'entitY_ID', 'description', parentEntityId);
                if (userEntityId != 0) getplacepost(userEntityId);
            },
            error: function (xhr) {
                alert("getrelation AJAX error: " + xhr.responseText);
            },
            dataType: "json",
        });
    }

    function getplacepost(userEntityId = 0) {
        $('#childposting').empty();
        let ctrlId = $('#controlingsearch option:selected').val();
        if (!ctrlId || ctrlId === "0") return;

        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/getpostplace",
            type: "POST",
            data: { 'E_R_ID': ctrlId },
            cache: false,
            success: function (data) {
                appendOptions($('#childposting'), data, 'entitY_ID', 'c_NAME', userEntityId);
            },
            error: function (xhr) {
                alert("getplacepost AJAX error: " + xhr.responseText);
            },
            dataType: "json",
        });
    }

    function getParas() {
        $('#paraTable tbody').empty();
        let entityId = $('#childposting option:selected').val();
        if (!entityId || entityId === "0") return;

        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_paras_for_status_change",
            type: "POST",
            data: { 'ENTITY_ID': entityId },
            cache: false,
            success: function (data) {
                g_obsList = data;
                let rows = '';
                $.each(data, function (idx, child) {
                    rows += '<tr>' +
                        '<td>' + (child.audiT_PERIOD || '') + '</td>' +
                        '<td>' + (child.parA_NO || '') + '</td>' +
                        '<td>' + (child.gisT_OF_PARAS || '') + '</td>' +
                        '<td>' + (child.risk || '') + '</td>' +
                        '<td>' + (child.parA_STATUS || '') + '</td>' +
                        '<td class="text-center"><a class="text-primary" style="cursor:pointer" onclick="paraText(\'' + child.coM_ID + '\')">View Para Text</a></td>' +
                        '<td class="text-center"><a class="text-danger" style="cursor:pointer" onclick="openChange(\'' + child.coM_ID + '\',\'' + child.ind + '\')">Change Status</a></td>' +
                        '</tr>';
                });
                $('#paraTable tbody').append(rows);
            },
            error: function (xhr) {
                alert("getParas AJAX error: " + xhr.responseText);
            },
            dataType: "json"
        });
    }

    function paraText(comId) {
        if (!comId || comId === "0") return;
        $('#paraTextDisplayModel').modal('show');
        $('#paraTextModelPanel').empty();
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_all_para_text",
            type: "POST",
            data: {
                'COM_ID': comId
            },
            cache: false,
            success: function (data) {
                $('#paraTextModelPanel').html(data);
            },
            error: function (xhr) {
                alert("paraText AJAX error: " + xhr.responseText);
            }
        });
    }

    function openChange(com_id, ind) {
        g_newComId = com_id;
        g_ind = ind;
        $('#actionSelect').val('S');
        $('#statusSelect').val('9');
        $('#Reason').val('');
        $('#process_detail').modal('show');
    }

    function Publishchange() {
        if ($('#Reason').val().trim() === "") {
            alert("Please enter remarks");
            return;
        }
        let act = $('#actionSelect').val();
        let status = $('#statusSelect').val();
        if (act === 'D') status = 0;
        $('#Publishchange').prop('disabled', true);

        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/Add_Para_Change_status_Request",
            type: "POST",
            data: {
                'COM_ID': g_newComId,
                'NEW_STATUS': status,
                'REMARKS': $('#Reason').val(),
                'IND': g_ind,
                'Action_IND': act
            },
            cache: false,
            success: function (data) {
                showApiAlert(data);
                $('#process_detail').modal('hide');
                getParas();
            },
            error: function (xhr) {
                alert("Publishchange AJAX error: " + xhr.responseText);
            },
            complete: function () {
                $('#Publishchange').prop('disabled', false);
            },
            dataType: "json"
        });
    }
