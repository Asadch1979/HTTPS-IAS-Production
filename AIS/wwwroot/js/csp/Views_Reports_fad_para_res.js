    var g_obsList = [];
    var g_id = 0;
    function div_risksubcategoryShowHide() {
        if ($('#riskGroupSelectBox option:selected').val() == 0) {
            $('#div_risksubcategory').hide();
            $('#div_activityContainer').hide();
        }
        else {
            $('#div_risksubcategory').show();
            $('#div_activityContainer').hide();
            $('#riskSubGroupSelectBox').empty();
            $.ajax({
                url: g_asiBaseURL + "/Setup/process_details",
                type: "POST",
                data: {
                    'ProcessId': $('#riskGroupSelectBox option:selected').val(),
                },
                cache: false,
                success: function (data) {
                    $('#riskSubGroupSelectBox').append("<option value=\"0\" id=\"0\">--Select Sub Group--</option>");
                    $.each(data, function (index, item) {
                        $('#riskSubGroupSelectBox').append("<option value=\"" + item.id + "\"> " + item.title + " </option > ");
                    });

                },
                dataType: "json",
            });
        }
    }
    function div_activityContainerShowHide() {
        if ($('#riskSubGroupSelectBox option:selected').val() == 0)
            $('#div_activityContainer').hide();
        else
            $('#div_activityContainer').show();
        $('#riskActivitiesSelectBox').empty();
        $.ajax({
            url: g_asiBaseURL + "/Setup/process_transactions",
            type: "POST",
            data: {
                'ProcessDetailId': $('#riskSubGroupSelectBox option:selected').val(),
            },
            cache: false,
            success: function (data) {
                $('#riskActivitiesSelectBox').append("<option value=\"0\" id=\"0\">--Select Sub Group--</option>");
                $.each(data, function (index, item) {
                    $('#riskActivitiesSelectBox').append("<option value=\"" + item.id + "\"> " + item.description + "</option>");
                });

            },
            dataType: "json",
        });
    }

    function getEntityObservation() {

        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_fad_paras",
            type: "POST",
            data: {
                'PROCESS_ID': $('#riskGroupSelectBox option:selected').val(),
                'SUB_PROCESS_ID': $('#riskSubGroupSelectBox option:selected').val(),
                'PROCESS_DETAIL_ID': $('#riskActivitiesSelectBox option:selected').val()
            },
            cache: false,
            success: function (data) {
                g_obsList = data;
                $('#observation_panel tbody').empty();
                $.each(data, function (i, v) {
                    $('#observation_panel tbody').append('<tr><td>' + v.period + '</td><td>' + v.entitY_NAME + '</td><td>' + v.process + '</td><td>' + v.suB_PROCESS + '</td><td>' + v.violation + '</td><td>' + v.obS_TEXT + '</td><td>' + v.obS_RISK + '</td><td>' + v.obS_STATUS + '</td></tr>');
                 });


            },
            dataType: "json",
        });
    }

    function ShowMemo(id) {
        g_id = id;
        $('#viewMemoModel').modal('show');
        $.each(g_obsList, function (i, v) {
            if (v.obS_ID == id) {
                $('#viewMemo_memo').html(v.obS_TEXT);
            }
        })
    }
    function ShowComments(id) {
        g_id = id;
        $('#commentsBox').modal('show');
       
    }
    function reloadLocation() {
        location.reload();
    }
    function finalCommentsButtonSave() {
        if ($('#concernedDeptInCommentsBox').val() == 0) {
            alert("Please select Concerned Department");
            return;
        }
        if ($('#commentAreaInCommentsBox').val() == "") {
            alert("Comments are Mandatory");
            return;
        }
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/divisional_head_remarks_on_functional_legacy_para",
            type: "POST",
            data: {
                'CONCERNED_DEPT_ID': $('#concernedDeptInCommentsBox').val(),
                'COMMENTS': $('#commentAreaInCommentsBox').val(),
                'REF_PARA_ID': g_id
            },
            cache: false,
            success: function (data) {
               alert("Successfully done");
                    onAlertCallback(reloadLocation);
            },
            dataType: "json",
        });
    }
