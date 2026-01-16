function initReferenceSection(comId, readOnly, containerSelector) {
    var container = containerSelector ? $(containerSelector) : $("#referenceSection");
    var searchSection = container.find('#searchSection');
    var saveBtn = container.find('#saveBtn');
    var resultTbl = container.find('#resultTbl');
    var refList = container.find('#refList');
    var refType = container.find('#refType');
    var keywordInput = container.find('#keyword');
    var searchBtn = container.find('#searchBtn');
    var manualInputs = container.find('#manualInputs');
    var manualName = container.find('#manualName');
    var chapterSection = container.find('#chapterSection');
    var addManualBtn = container.find('#addManualBtn');
    var searchInputs = container.find('#searchInputs');

    var refs = [];
    var refDetails = [];
    var refLinks = [];
    var manualCounter = -1;

    if (readOnly) {
        searchSection.hide();
        saveBtn.hide();
    } else {
        searchSection.show();
        saveBtn.show();
    }

    resultTbl.find('tbody').empty();
    refList.empty();

    function formatDate(dateStr) {
        if (!dateStr) return '';
        var parts = dateStr.split('T')[0].split('-');
        if (parts.length !== 3) return '';
        return parts[2] + '/' + parts[1] + '/' + parts[0];
    }

    $.get(g_asiBaseURL + '/ApiCalls/GetParaReferenceData', { comId: comId }, function (d) {
        refs = d.references || [];
        refDetails = d.referenceDetails || [];
        refLinks = d.referenceLinks || [];
        renderRefs();
    });

    searchBtn.off('click').on('click', function () {
        $.post(g_asiBaseURL + '/ApiCalls/SearchReferences', { referenceType: refType.val(), keyword: keywordInput.val() }, function (d) {
            var body = resultTbl.find('tbody'); body.empty();
            $.each(d, function (i, it) {
                var dateTxt = formatDate(it.instructionsDate);
                body.append('<tr>' +
                    '<td>' + it.title + '</td>' +
                    '<td>' + dateTxt + '</td>' +
                    '<td>' + it.instructionsdetails + '</td>' +
                    '<td>' + it.keywords + '</td>' +
                    '<td><button type="button" class="view btn btn-sm btn-secondary" data-url="' + it.referenceurl + '">View</button></td>' +
                    (readOnly ? '' : '<td><button type="button" class="attach btn btn-sm btn-primary" data-id="' + it.id + '">Attach</button></td>') +
                    '</tr>');
            });
        });
    });

    addManualBtn.off('click').on('click', function () {
        if (readOnly) return;
        var name = manualName.val();
        var chap = chapterSection.val();
        if (!name) return;
        refDetails.push({
            id: manualCounter--,
            instructionsTitle: name,
            instructionsDate: null,
            referenceType: refType.val(),
            division: chap,
            divisionEntId: 0,
            linkId: null
        });
        manualName.val('');
        chapterSection.val('');
        renderRefs();
    });

    refType.off('change').on('change', toggleInputMode);
    toggleInputMode();

    container.off('click', '.attach').on('click', '.attach', function (e) {
        e.preventDefault();
        if (readOnly) return;
        var ref = parseInt($(this).data('id'), 10);
        if ($.inArray(ref, refs) === -1) {
            refs.push(ref);
            $.get(g_asiBaseURL + '/ApiCalls/GetReferenceDetail', { refId: ref }, function (d) {
                if (d) {
                    d.linkId = null;
                    refDetails.push(d);
                }
                renderRefs();
            });
        }
    });

    container.off('click', '.remove-ref').on('click', '.remove-ref', function () {
        if (readOnly) return;
        var id = parseInt($(this).data('id'), 10);
        refs = $.grep(refs, function (v) { return v != id; });
        refDetails = $.grep(refDetails, function (v) { return v.id != id; });
        renderRefs();
    });

    container.off('click', '.view').on('click', '.view', function () {
        var url = $(this).data('url');
        window.open(url, '_blank');
    });

    saveBtn.off('click').on('click', function () {
        if (readOnly) return;
        var payload = { comId: comId, references: [] };
        $.each(refDetails, function (i, r) {
            var lnk = r.linkId !== undefined && r.linkId !== null ? r.linkId : null;
            if (lnk === null) {
                var found = refLinks.find(function (fl) { return fl.instructionsDate === r.instructionsDate; });
                if (found) lnk = found.linkId;
            }
            payload.references.push({
                linkId: lnk,
                referenceId: r.id > 0 ? r.id : null,
                entityId: r.divisionEntId || 0,
                oldParaId: 0,
                newParaId: 0,
                paraId: comId,
                // ensure the date is serialised in ISO format so that the
                // backend model binder correctly parses it as a DateTime
                instructionsDate: r.instructionsDate ? new Date(r.instructionsDate).toISOString() : null,
                referenceTitle: r.instructionsTitle,
                creditManualId: null,
                opManualId: null,
                manualType: r.referenceType,
                chapter: r.division,
                matchedText: null,
                linkType: r.referenceType
            });
        });

        $.ajax({
            url: g_asiBaseURL + '/ApiCalls/SaveParaReferences',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(payload),
            success: function (msg) {
                alert(msg);
                container.trigger('referenceSectionSaved');
                // refresh current model instead of entire view
                initReferenceSection(comId, readOnly, containerSelector);
            }
        });
    });

    function toggleInputMode() {
        var t = refType.val();
        if (t === 'Circular') {
            searchInputs.show();
            resultTbl.show();
            manualInputs.hide();
        } else if (t === 'Manual' || t === 'Policy') {
            searchInputs.hide();
            resultTbl.hide();
            manualInputs.show();
        } else {
            searchInputs.hide();
            resultTbl.hide();
            manualInputs.hide();
        }
    }

    function renderRefs() {
        var ul = refList; ul.empty();
        $.each(refDetails, function (i, r) {
            var dateTxt = formatDate(r.instructionsDate);
            ul.append('<li class="list-group-item">'
                + '<div><strong>Reference No ' + (i + 1) + '</strong></div>'
                + '<div>' + r.instructionsTitle + '</div>'
                + '<div>Issuance Date: ' + dateTxt + '</div>'
                + '<div>Reference Type: ' + r.referenceType + '</div>'
                + '<div>Division Code: ' + r.division + '</div>'
                + (readOnly ? '' : ' <button type="button" class="btn btn-danger btn-sm float-end remove-ref" data-id="' + r.id + '">Delete</button>') + '</li>');
        });
    }
}
