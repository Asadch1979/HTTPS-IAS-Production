var g_engId = 0;
$('#document').ready(function () {
    var hiddenEngagement = document.getElementById('maClosingEngagementId');
    if (hiddenEngagement && hiddenEngagement.value) {
        g_engId = hiddenEngagement.value;
    } else {
        var url = new URL(window.location.href);
        g_engId = url.searchParams.get("engId");
    }
    $('#template_box').richText({
        imageUpload: false,
        fileUpload: false,
        videoEmbed: false,
        urls: false
    });
    $('#otherDepSelectField').select2();
});

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
            url: g_asiBaseURL + "/Execution/sub_voilation",
            type: "POST",
            data: {
                'V_ID': $('#riskGroupSelectBox option:selected').val(),
            },
            cache: false,
            success: function (data) {
                $('#riskSubGroupSelectBox').append("<option value=\"0\" id=\"0\">--Select Sub Group--</option>");
                $.each(data, function (index, item) {
                    $('#riskSubGroupSelectBox').append("<option value=\"" + item.id + "\"> " + item.suB_V_NAME + " </option> ");
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
        url: g_asiBaseURL + "/Execution/risk_activities",
        type: "POST",
        data: {
            'S_GR_ID': $('#riskSubGroupSelectBox option:selected').val(),
        },
        cache: false,
        success: function (data) {
            $('#riskActivitiesSelectBox').append("<option value=\"0\" id=\"0\">--Select Sub Group--</option>");
            $.each(data, function (index, item) {
                $('#riskActivitiesSelectBox').append("<option value=\"" + item.activitY_ID + "\"> " + item.description + "</option>");
            });

        },
        dataType: "json",
    });
}

function reloadPage() {
    window.location.reload();
}
function saveChecklistObservations() {
    if ($('#riskGroupSelectBox').val() == 0) {
        alert('Select Violation Category');
        return;
    }
    if ($('#riskSubGroupSelectBox').val() == 0) {
        alert('Select Violation Nature');
        return;
    }
    if ($('#auditCriteriaRiskField').val() == 0) {
        alert('Select Risk');
        return;
    }
    if ($('#viewMemo_heading').val() == 0) {
        alert('Please Enter Para Heading');
        return;
    }
    var g_memoObj = [];
    var memo = {
        'MEMO': $('.richText-editor').html(),
        'ID': 'obs_0',
        'HEADING': $('#viewMemo_heading').val(),
        'RISK': $('#auditCriteriaRiskField').val(),
        'DAYS': $('#viewMemo_replydays option:selected').val(),
        'LOANCASE': '',
        'NO_OF_INSTANCES': $('#viewMemo_noinstances').val(),
        'AMOUNT_INVOLVED': '0',
        'ATTACHMENTS': ''
    };
    g_memoObj.push(memo);
    var engIdValue = parseInt(g_engId, 10);
    if (Number.isNaN(engIdValue)) {
        engIdValue = null;
    }
    var violationCategoryId = parseInt($('#riskGroupSelectBox').val(), 10);
    if (Number.isNaN(violationCategoryId) || violationCategoryId === 0) {
        violationCategoryId = null;
    }
    var violationNatureId = parseInt($('#riskSubGroupSelectBox').val(), 10);
    if (Number.isNaN(violationNatureId) || violationNatureId === 0) {
        violationNatureId = null;
    }
    var otherEntityId = parseInt($('#otherDepSelectField').val(), 10);
    if (Number.isNaN(otherEntityId) || otherEntityId === 0) {
        otherEntityId = null;
    }
    var payload = {
        'LIST_OBS': g_memoObj,
        'ENG_ID': engIdValue,
        'S_ID': null,
        'V_CAT_NATURE_ID': violationNatureId,
        'V_CAT_ID': violationCategoryId,
        'OTHER_ENTITY_ID': otherEntityId,
        'IS_FINAL': false
    };
    $.ajax({
        url: g_asiBaseURL + "/ApiCalls/save_observations",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(payload),
        cache: false,
        success: function (data) {
            showApiAlert(data);
            onAlertCallback(reloadPage);
        },
        dataType: "json",
    });

}
