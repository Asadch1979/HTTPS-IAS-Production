var g_engShiftSelected = null;

function engShiftToken() {
    return $('#engagementShiftCsrfForm input[name="__RequestVerificationToken"]').val() || '';
}

function engShiftText(value) {
    return value == null ? '' : $('<div>').text(value).html();
}

function engShiftShowMessage(message, success) {
    $('#engagementShiftMessage')
        .removeClass('d-none alert-success alert-danger')
        .addClass(success ? 'alert-success' : 'alert-danger')
        .text(message);
}

function engShiftGetRelation(parentEntityId, userEntityId) {
    $('#engShiftControllingOffice').empty();
    $('#engShiftCurrentEntity').empty();
    $('#engagementShiftListPanel tbody').empty();
    $.ajax({
        url: g_asiBaseURL + "/ApiCalls/getparentrel",
        type: "POST",
        data: { 'ENTITY_REALTION_ID': $('#engShiftRelationshipField option:selected').val() },
        cache: false,
        success: function (data) {
            $('#engShiftControllingOffice').append('<option id="0" value="0">--Select Controlling/Reporting Office--</option>');
            $.each(data, function (index, item) {
                var selected = item.entitY_ID == parentEntityId ? 'selected="selected"' : '';
                $('#engShiftControllingOffice').append('<option ' + selected + ' value="' + item.entitY_ID + '" id="' + item.entitY_REALTION_ID + '">' + engShiftText(item.description) + '</option>');
            });
            if (userEntityId)
                engShiftGetPosting(userEntityId);
        },
        dataType: "json"
    });
}

function engShiftGetPosting(userEntityId) {
    $('#engShiftCurrentEntity').empty();
    $('#engagementShiftListPanel tbody').empty();
    $.ajax({
        url: g_asiBaseURL + "/ApiCalls/getpostplace",
        type: "POST",
        data: { 'E_R_ID': $('#engShiftControllingOffice option:selected').val() },
        cache: false,
        success: function (data) {
            $('#engShiftCurrentEntity').append('<option id="0" value="0" selected="selected">--Select Current Entity--</option>');
            $.each(data, function (index, item) {
                var selected = item.entitY_ID == userEntityId ? 'selected="selected"' : '';
                $('#engShiftCurrentEntity').append('<option ' + selected + ' value="' + item.entitY_ID + '" id="' + item.entitY_ID + '">' + engShiftText(item.c_NAME) + '</option>');
            });
        },
        dataType: "json"
    });
}

function engShiftGetEngagements() {
    $('#engagementShiftListPanel tbody').empty();
    $.ajax({
        url: g_asiBaseURL + "/ApiCalls/get_engagements_details_for_status_reversal",
        type: "POST",
        data: { 'ENTITY_ID': $('#engShiftCurrentEntity option:selected').val() },
        cache: false,
        success: function (data) {
            $.each(data, function (i, item) {
                var auditStart = item.audiT_START_DATE ? item.audiT_START_DATE.split(' ')[0] : '';
                var auditEnd = item.audiT_END_DATE ? item.audiT_END_DATE.split(' ')[0] : '';
                var opStart = item.oP_START_DATE ? item.oP_START_DATE.split(' ')[0] : '';
                var opEnd = item.oP_END_DATE ? item.oP_END_DATE.split(' ')[0] : '';
                var encoded = encodeURIComponent(JSON.stringify(item));
                $('#engagementShiftListPanel').append('<tr><td>' + (i + 1) + '</td><td>' + engShiftText(item.enG_ID) + '</td><td>' + engShiftText(item.teaM_NAME) + '</td><td>' + auditStart + '</td><td>' + auditEnd + '</td><td>' + opStart + '</td><td>' + opEnd + '</td><td>' + engShiftText(item.status) + '</td><td><a href="#" data-onclick="engShiftOpen(\'' + encoded + '\');" class="text-sucess text-center">Shift Engagement</a></td></tr>');
            });
        },
        dataType: "json"
    });
}

function engShiftOpen(encodedItem) {
    g_engShiftSelected = JSON.parse(decodeURIComponent(encodedItem));
    $('#engagementShiftMessage').addClass('d-none').text('');
    $('#engShiftConfirmText').addClass('d-none').text('');
    $('#engShiftReason').val('');
    $('#engShiftEngId').text(g_engShiftSelected.enG_ID || '');
    $('#engShiftAuditPeriod').text((g_engShiftSelected.oP_START_DATE || '').split(' ')[0] + ' to ' + (g_engShiftSelected.oP_END_DATE || '').split(' ')[0]);
    $('#engShiftOldEntity').text($('#engShiftCurrentEntity option:selected').text());
    $('#engShiftStatus').text(g_engShiftSelected.status || '');
    $('#engShiftTeam').text(g_engShiftSelected.teaM_NAME || '');
    engShiftLoadTeam(g_engShiftSelected.auditeD_BY_ID, g_engShiftSelected.teaM_ID);

    $('#engShiftNewEntity').empty().append('<option value="0">--Select Destination Entity--</option>');
    $('#engShiftCurrentEntity option').each(function () {
        if ($(this).val() !== '0' && Number($(this).val()) !== Number(g_engShiftSelected.entitY_ID))
            $('#engShiftNewEntity').append('<option value="' + $(this).val() + '">' + engShiftText($(this).text()) + '</option>');
    });
    $('#engagementShiftModal').modal('show');
}

function engShiftLoadTeam(auditedById, teamId) {
    if (!auditedById || !teamId)
        return;

    $.ajax({
        url: g_asiBaseURL + "/ApiCalls/get_team_memeber_details_for_post_changes_team_eng_reversal",
        type: "POST",
        data: {
            'AUDITED_BY_DEPT': auditedById,
            'CURRENT_TEAM_ID': teamId
        },
        cache: false,
        success: function (data) {
            var lead = '';
            var members = [];
            $.each(data, function (index, member) {
                if (Number(member.t_ID) !== Number(teamId))
                    return;

                var staffName = (member.employeename || '') + ' (' + (member.teammembeR_ID || '') + ')';
                if (member.iS_TEAMLEAD == 'Y')
                    lead = staffName;
                else
                    members.push(staffName);
            });

            var teamText = g_engShiftSelected.teaM_NAME || '';
            if (lead)
                teamText += ' | Lead: ' + lead;
            if (members.length)
                teamText += ' | Members: ' + members.join(', ');
            $('#engShiftTeam').text(teamText);
        },
        dataType: "json"
    });
}

function engShiftConfirm() {
    if (!g_engShiftSelected) {
        engShiftShowMessage('Please select an engagement first.', false);
        return;
    }
    var newEntityId = Number($('#engShiftNewEntity').val());
    var oldEntityId = Number(g_engShiftSelected.entitY_ID);
    var reason = $.trim($('#engShiftReason').val());
    if (!newEntityId) {
        engShiftShowMessage('Please select destination entity.', false);
        return;
    }
    if (newEntityId === oldEntityId) {
        engShiftShowMessage('Destination entity cannot be the current entity.', false);
        return;
    }
    if (!reason) {
        engShiftShowMessage('Reason / remarks are mandatory.', false);
        return;
    }
    var confirmText = $('#engShiftOldEntity').text() + ' -> ' + $('#engShiftNewEntity option:selected').text();
    $('#engShiftConfirmText').removeClass('d-none').text('Confirm shift: ' + confirmText);
    if (!window.confirm('Confirm engagement shift: ' + confirmText + '?'))
        return;

    $.ajax({
        url: g_asiBaseURL + "/AdministrationPanel/ShiftEngagementEntity",
        type: "POST",
        headers: { 'RequestVerificationToken': engShiftToken() },
        data: {
            EngagementId: g_engShiftSelected.enG_ID,
            NewEntityId: newEntityId,
            Reason: reason
        },
        cache: false,
        success: function (data) {
            showApiAlert(data);
            if (data && data.status) {
                $('#engagementShiftModal').modal('hide');
                engShiftGetEngagements();
            } else {
                engShiftShowMessage((data && data.message) || 'Engagement shift failed.', false);
            }
        },
        error: function (xhr) {
            var message = (xhr.responseJSON && xhr.responseJSON.message) || 'Engagement shift failed.';
            engShiftShowMessage(message, false);
        },
        dataType: "json"
    });
}
