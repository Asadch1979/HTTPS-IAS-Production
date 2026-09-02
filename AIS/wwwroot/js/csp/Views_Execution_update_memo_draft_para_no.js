var memoDraftRows = [];

$(document).ready(function () {
    $('#memoDraftEngagementSelect').select2();
    $('#memoDraftEngagementSelect').on('change', loadMemoDraftRows);

    if ($('#memoDraftEngagementSelect').val() && $('#memoDraftEngagementSelect').val() !== '0') {
        loadMemoDraftRows();
    }
});

function htmlEncode(value) {
    return $('<div/>').text(value == null ? '' : value).html();
}

function getRowByObservationId(obsId) {
    return memoDraftRows.find(function (row) {
        return parseInt(row.observationId || row.obS_ID || row.obsId, 10) === parseInt(obsId, 10);
    });
}

function normalizeNumber(value) {
    return $.trim((value || '').toString());
}

function loadMemoDraftRows() {
    var engagementId = parseInt($('#memoDraftEngagementSelect').val(), 10) || 0;
    $('#memoDraftUpdatePanel tbody').empty();
    memoDraftRows = [];

    if (engagementId <= 0) {
        return;
    }

    $.ajax({
        url: g_asiBaseURL + '/ApiCalls/get_memo_draft_para_update_observations',
        type: 'POST',
        data: { ENG_ID: engagementId },
        cache: false,
        success: function (data) {
            memoDraftRows = data || [];
            if (!memoDraftRows.length) {
                $('#memoDraftUpdatePanel tbody').append('<tr><td colspan="7" class="text-center text-muted">No non-finalized draft paras found for the selected engagement.</td></tr>');
                return;
            }

            $.each(memoDraftRows, function (i, row) {
                var obsId = row.observationId || row.obS_ID || row.obsId;
                var memoNo = row.memoNumber || row.memO_NO || 0;
                var draftNo = row.draftParaNumber || row.drafT_PARA_NO || 0;
                var isFinalized = row.isFinalized === true || row.iS_FINALIZED === true || row.isFinalized === 1;
                var disabled = isFinalized ? ' disabled="disabled"' : '';
                var action = isFinalized
                    ? '<span class="text-muted">Finalized</span>'
                    : '<button type="button" class="btn btn-sm btn-success" data-onclick="saveMemoDraftRow(' + obsId + ')">Save</button>';

                $('#memoDraftUpdatePanel tbody').append(
                    '<tr data-obs-id="' + obsId + '">' +
                    '<td>' + htmlEncode(row.observationTitle || row.obS_TITLE || row.heading) + '</td>' +
                    '<td>' + htmlEncode(row.statusName || row.statuS_NAME || row.obS_STATUS) + '</td>' +
                    '<td class="existing-memo">' + htmlEncode(memoNo) + '</td>' +
                    '<td><input class="form-control form-control-sm memo-input" value="' + htmlEncode(memoNo) + '"' + disabled + ' /></td>' +
                    '<td class="existing-draft">' + htmlEncode(draftNo) + '</td>' +
                    '<td><input class="form-control form-control-sm draft-input" value="' + htmlEncode(draftNo) + '"' + disabled + ' /></td>' +
                    '<td class="text-center">' + action + '</td>' +
                    '</tr>');
            });
        },
        error: function (xhr) {
            xhr.__iasSafetyHandled = true;
            showApiAlertFromXhr(xhr, xhr.status, getErrorReferenceIdFromXhr(xhr), 'Unable to load memo/draft para rows.');
        },
        dataType: 'json'
    });
}

function saveMemoDraftRow(obsId) {
    var engagementId = parseInt($('#memoDraftEngagementSelect').val(), 10) || 0;
    var $row = $('#memoDraftUpdatePanel tbody tr[data-obs-id="' + obsId + '"]');
    var current = getRowByObservationId(obsId);
    var oldMemo = normalizeNumber($row.find('.existing-memo').text());
    var oldDraft = normalizeNumber($row.find('.existing-draft').text());
    var newMemo = normalizeNumber($row.find('.memo-input').val());
    var newDraft = normalizeNumber($row.find('.draft-input').val());

    if (!newMemo || !/^[0-9]+$/.test(newMemo)) {
        alert('Memo Number is required and must contain digits only.');
        return;
    }

    if (!newDraft || !/^[0-9]+$/.test(newDraft)) {
        alert('Draft Para Number is required and must contain digits only.');
        return;
    }

    var duplicate = memoDraftRows.some(function (row) {
        var rowObsId = row.observationId || row.obS_ID || row.obsId;
        var rowDraft = normalizeNumber(row.draftParaNumber || row.drafT_PARA_NO);
        return parseInt(rowObsId, 10) !== parseInt(obsId, 10) && rowDraft === newDraft;
    });

    if (duplicate) {
        alert('Draft Para Number already exists against another observation in this engagement.');
        return;
    }

    if (current && (current.isFinalized === true || current.iS_FINALIZED === true || current.isFinalized === 1)) {
        alert('Finalized paras cannot be updated from this screen.');
        return;
    }

    if (oldMemo === newMemo && oldDraft === newDraft) {
        alert('No changes found.');
        return;
    }

    if (!confirm('Update Memo Number from ' + oldMemo + ' to ' + newMemo + ' and Draft Para Number from ' + oldDraft + ' to ' + newDraft + '?')) {
        return;
    }

    $.ajax({
        url: g_asiBaseURL + '/ApiCalls/update_memo_draft_para_no',
        type: 'POST',
        data: {
            EngagementId: engagementId,
            ObservationId: obsId,
            MemoNumber: newMemo,
            DraftParaNumber: newDraft
        },
        cache: false,
        success: function (data) {
            showApiAlert(data);
            if (data && (data.Status === true || data.status === true)) {
                onAlertCallback(loadMemoDraftRows);
            }
        },
        error: function (xhr) {
            xhr.__iasSafetyHandled = true;
            showApiAlertFromXhr(xhr, xhr.status, getErrorReferenceIdFromXhr(xhr), 'Unable to update Memo/Draft Para numbers.');
        },
        dataType: 'json'
    });
}
