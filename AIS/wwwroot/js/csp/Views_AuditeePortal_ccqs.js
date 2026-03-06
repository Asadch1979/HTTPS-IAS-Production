    var g_id = 0;
    var g_response = [];

    document.addEventListener("DOMContentLoaded", function () {
        if (window.CommonValidation && CommonValidation.attachAlnumOnly) {
            CommonValidation.attachAlnumOnly("textarea.alnum-only", { allowSpace: true, maxLen: 1000 });
        }
    });
    function updateCCQBinding(id, question, cv_id, risk_id,status) {
        g_id = id;
        var question = '';
        $.each(g_response, function (i, e) {
            if (e.id == id) {
                question = e.questions;
            }
        });
        $('#updateCCQModal').modal('show');
        $('#CCQquestion_textarea').val(question);
        $('#CCQcontrolViolation_selectarea').val(cv_id);
        $('#CCQrisk_selectarea').val(risk_id);
        $('#CCQstatus_selectarea').val(status);
    }
    function getEntityObservation() {
        $('#ccqs_panel tbody').empty();
        if ($('#entitySelectField option:selected').val() != 0) {
            $.ajax({
                url: g_asiBaseURL + "/ApiCalls/get_ccqs",
                type: "POST",
                data: {
                    'ENTITY_ID': $('#entitySelectField option:selected').val()
                },
                cache: false,
                success: function (data) {
                    g_response = data;
                    var sr = 1;
                    $.each(data, function (i, v) {
                        $('#ccqs_panel tbody').append('  <tr id="' + v.id + '"><td align="justify"> ' + sr + '</td> <td align="left">' + v.questions + '</td> <td align="left">' + v.controL_VIOLATION + '</td> <td align="left">' + v.risk + '</td> <td align="left">' + v.status + '</td><td align="justify"><a data-click="event.preventDefault();updateCCQBinding(' + v.id + ', \'\',' + v.controL_VIOLATION_ID + ' , ' + v.risK_ID + ', \'' + v.status + '\');" href="#">Edit</a></td> </tr>');
                        sr++;
                    });

                },
                dataType: "json",
            });

        }
    }

    function addCCQsUpdateToTable() {
        $('#updateCCQModal').modal('hide');
        $($('#ccqs_panel tbody tr#' + g_id).find('td').eq(1)).html($('#CCQquestion_textarea').val());
        $($('#ccqs_panel tbody tr#' + g_id).find('td').eq(2)).html($('#CCQcontrolViolation_selectarea option:selected').text());
        $($('#ccqs_panel tbody tr#' + g_id).find('td').eq(3)).html($('#CCQrisk_selectarea option:selected').text());
        $($('#ccqs_panel tbody tr#' + g_id).find('td').eq(4)).html($('#CCQstatus_selectarea option:selected').text());
        $($('#ccqs_panel tbody tr#' + g_id).find('td').eq(5)).html('<a data-click="event.preventDefault();updateCCQBinding(' + g_id + ', \'\',' + $('#CCQcontrolViolation_selectarea option:selected').text() + ' , ' + $('#CCQrisk_selectarea option:selected').text() + ', \'' + $('#CCQstatus_selectarea option:selected').text() + '\');" href="#">Edit</a>');
       
        $.each(g_response, function (i, e) {
            if (e.id == id) {
                e.questions = $('#CCQquestion_textarea').val();
            }
        });
       
    }
    function reloadLocation() {
        addCCQsUpdateToTable();
    }
    function finalUpdateCCQHandler() {
        if ($('#CCQquestion_textarea').val() == "") {
            alert('Please Enter Question');
            return true;            

        }
        if ($('#CCQcontrolViolation_selectarea').val() == 0) {
            alert('Please Select Control Violation');
            return true;

        }
        if ($('#CCQrisk_selectarea').val() == 0) {
            alert('Please Select Risk');
            return true;

        }
        if ($('#CCQstatus_selectarea').val() == 0) {
            alert('Please Select Status');
            return true;

        }
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/update_ccq",
            type: "POST",
            data: {
                'ID': g_id,
                'QUESTIONS': $('#CCQquestion_textarea').val(),
                'CONTROL_VIOLATION_ID': $('#CCQcontrolViolation_selectarea').val(),
                'RISK_ID': $('#CCQrisk_selectarea').val(),
                'STATUS': $('#CCQstatus_selectarea').val()
            },
            cache: false,
            success: function (data) {
                alert('Record Updated Successfully');
                onAlertCallback(addCCQsUpdateToTable);
            },
            dataType: "json",
        });

    }
