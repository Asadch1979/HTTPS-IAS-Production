function initReferenceSection(comId, readOnly, containerSelector) {
    var container = containerSelector ? $(containerSelector) : $("#referenceSection");
    var searchSection = container.find('#searchSection');
    var saveBtn = container.find('#saveBtn');
    var resultTbl = container.find('#resultTbl');
    var refList = container.find('#refList');
    var refType = container.find('#refType');
    var keywordInput = container.find('#keyword');
    var searchBtn = container.find('#searchBtn');
    var searchInputs = container.find('#searchInputs');

    var manualInputs = container.find('#manualInputs');
    var manualMaster = container.find('#manualMaster');
    var manualSection = container.find('#manualSection');
    var manualChapter = container.find('#manualChapter');
    var manualIndexGridWrapper = container.find('#manualIndexGridWrapper');
    var manualIndexTbl = container.find('#manualIndexTbl');

    var refDetails = [];
    var refLinks = [];

    if (readOnly) {
        searchSection.hide();
        saveBtn.hide();
    } else {
        searchSection.show();
        saveBtn.show();
    }

    resultTbl.find('tbody').empty();
    refList.empty();
    clearManualGrid();

    function formatDate(dateStr) {
        if (!dateStr) return '';
        var parts = dateStr.split('T')[0].split('-');
        if (parts.length !== 3) return '';
        return parts[2] + '/' + parts[1] + '/' + parts[0];
    }

    function isManualReference(item) {
        var type = (item.referenceType || item.manualType || item.linkType || '').toUpperCase();
        return type === 'MANUAL' || type === 'POLICY';
    }

    function getRefKey(item) {
        if (item.linkId) {
            return 'link-' + item.linkId;
        }

        if (isManualReference(item)) {
            var manualId = item.manualId || item.opManualId || item.creditManualId || 0;
            var manualIndexId = item.manualIndexId || item.id || 0;
            return 'manual-' + manualId + '-' + manualIndexId;
        }

        return 'ref-' + (item.id || 0);
    }

    function existsInSelection(item) {
        var key = getRefKey(item);
        return refDetails.some(function (r) { return getRefKey(r) === key; });
    }

    function normalizeLoadedReferences(data) {
        var details = data.referenceDetails || [];
        var links = data.referenceLinks || [];

        $.each(links, function (_, lnk) {
            var hasExistingDetail = details.some(function (d) { return d.linkId && d.linkId === lnk.linkId; });
            var type = (lnk.manualType || lnk.linkType || '').toUpperCase();
            if (!hasExistingDetail && (type === 'MANUAL' || type === 'POLICY')) {
                details.push({
                    id: lnk.manualIndexId || lnk.referenceId,
                    linkId: lnk.linkId,
                    divisionEntId: lnk.entityId,
                    referenceType: lnk.manualType || lnk.linkType,
                    instructionsTitle: lnk.referenceTitle,
                    instructionsDate: lnk.instructionsDate,
                    division: lnk.chapter,
                    manualId: lnk.manualId || lnk.opManualId || lnk.creditManualId,
                    manualIndexId: lnk.manualIndexId || lnk.referenceId,
                    chapterNo: lnk.chapterNo,
                    sectionName: lnk.sectionName,
                    subSectionNo: lnk.subSectionNo,
                    heading: lnk.heading
                });
            }
        });

        return details;
    }

    function buildManualDisplay(item) {
        if (item.instructionsTitle) {
            return item.instructionsTitle;
        }

        var parts = [];
        if (item.manualName) parts.push(item.manualName);
        if (item.sectionName) parts.push(item.sectionName);
        if (item.chapterNo) parts.push(item.chapterNo);
        if (item.subSectionNo) parts.push(item.subSectionNo);
        if (item.heading) parts.push(item.heading);
        return parts.join(' / ');
    }

    function clearManualGrid() {
        manualIndexTbl.find('tbody').empty();
        manualIndexGridWrapper.hide();
    }

    function resetSectionsAndBelow() {
        manualSection.val('');
        manualSection.prop('disabled', true);
        manualChapter.val('');
        manualChapter.prop('disabled', true);
        clearManualGrid();
    }

    function resetChaptersAndGrid() {
        manualChapter.val('');
        manualChapter.prop('disabled', true);
        clearManualGrid();
    }

    function loadManualMaster() {
        manualMaster.empty().append('<option value="">Select manual</option>');
        resetSectionsAndBelow();

        $.get(g_asiBaseURL + '/ApiCalls/GetManualMaster', function (d) {
            $.each(d || [], function (_, it) {
                var label = it.displayLabel || [it.manualName, it.volumeName].filter(Boolean).join(' ');
                manualMaster.append('<option value="' + it.manualId + '">' + label + '</option>');
            });
        });
    }

    function loadManualSections(manualId) {
        manualSection.empty().append('<option value="">Select section</option>');
        resetChaptersAndGrid();

        if (!manualId) {
            manualSection.prop('disabled', true);
            return;
        }

        $.get(g_asiBaseURL + '/ApiCalls/GetManualSections', { manualId: manualId }, function (d) {
            $.each(d || [], function (_, it) {
                manualSection.append('<option value="' + it.sectionName + '">' + it.sectionName + '</option>');
            });
            manualSection.prop('disabled', false);
        });
    }

    function loadManualChapters(manualId, sectionName) {
        manualChapter.empty().append('<option value="">Select chapter</option>');
        clearManualGrid();

        if (!manualId || !sectionName) {
            manualChapter.prop('disabled', true);
            return;
        }

        $.get(g_asiBaseURL + '/ApiCalls/GetManualChapters', { manualId: manualId, sectionName: sectionName }, function (d) {
            $.each(d || [], function (_, it) {
                manualChapter.append('<option value="' + it.chapterNo + '">' + it.chapterNo + '</option>');
            });
            manualChapter.prop('disabled', false);
        });
    }

    function loadManualIndexGrid(manualId, sectionName, chapterNo) {
        clearManualGrid();

        if (!manualId || !sectionName || !chapterNo) {
            return;
        }

        $.get(g_asiBaseURL + '/ApiCalls/GetManualIndexByChapter', {
            manualId: manualId,
            sectionName: sectionName,
            chapterNo: chapterNo
        }, function (d) {
            var body = manualIndexTbl.find('tbody');
            body.empty();

            $.each(d || [], function (_, it) {
                var payload = {
                    manualId: parseInt(manualId, 10),
                    manualIndexId: it.indexId,
                    manualName: manualMaster.find('option:selected').text(),
                    sectionName: sectionName,
                    chapterNo: chapterNo,
                    subSectionNo: it.subSectionNo,
                    heading: it.heading,
                    referenceType: refType.val()
                };

                body.append('<tr>' +
                    (readOnly ? '' : '<td><button type="button" class="btn btn-sm btn-primary attach-manual" data-item="' + encodeURIComponent(JSON.stringify(payload)) + '">Add</button></td>') +
                    '<td>' + (it.subSectionNo || '') + '</td>' +
                    '<td>' + (it.heading || '') + '</td>' +
                    '</tr>');
            });

            manualIndexGridWrapper.show();
        });
    }

    $.get(g_asiBaseURL + '/ApiCalls/GetParaReferenceData', { comId: comId }, function (d) {
        refDetails = normalizeLoadedReferences(d);
        refLinks = d.referenceLinks || [];
        renderRefs();
    });

    searchBtn.off('click').on('click', function () {
        $.post(g_asiBaseURL + '/ApiCalls/SearchReferences', { referenceType: refType.val(), keyword: keywordInput.val() }, function (d) {
            var body = resultTbl.find('tbody');
            body.empty();
            $.each(d, function (_, it) {
                var dateTxt = formatDate(it.instructionsDate);
                body.append('<tr>' +
                    '<td>' + (it.title || '') + '</td>' +
                    '<td>' + dateTxt + '</td>' +
                    '<td>' + (it.instructionsdetails || '') + '</td>' +
                    '<td>' + (it.keywords || '') + '</td>' +
                    '<td><button type="button" class="view btn btn-sm btn-secondary" data-url="' + (it.referenceurl || '') + '">View</button></td>' +
                    (readOnly ? '' : '<td><button type="button" class="attach btn btn-sm btn-primary" data-id="' + it.id + '">Attach</button></td>') +
                    '</tr>');
            });
        });
    });

    refType.off('change').on('change', toggleInputMode);
    manualMaster.off('change').on('change', function () {
        loadManualSections($(this).val());
    });

    manualSection.off('change').on('change', function () {
        loadManualChapters(manualMaster.val(), $(this).val());
    });

    manualChapter.off('change').on('change', function () {
        loadManualIndexGrid(manualMaster.val(), manualSection.val(), $(this).val());
    });

    toggleInputMode();

    container.off('click', '.attach').on('click', '.attach', function (e) {
        e.preventDefault();
        if (readOnly) return;

        var ref = parseInt($(this).data('id'), 10);
        $.get(g_asiBaseURL + '/ApiCalls/GetReferenceDetail', { refId: ref }, function (d) {
            if (!d) return;
            d.linkId = null;
            if (!existsInSelection(d)) {
                refDetails.push(d);
                renderRefs();
            }
        });
    });

    container.off('click', '.attach-manual').on('click', '.attach-manual', function () {
        if (readOnly) return;

        var raw = decodeURIComponent($(this).data('item'));
        var item = JSON.parse(raw);

        var manualItem = {
            id: item.manualIndexId,
            linkId: null,
            divisionEntId: 0,
            referenceType: item.referenceType,
            instructionsTitle: [item.manualName, item.sectionName, item.chapterNo, item.subSectionNo, item.heading].filter(Boolean).join(' / '),
            instructionsDate: null,
            division: item.chapterNo,
            manualId: item.manualId,
            manualIndexId: item.manualIndexId,
            sectionName: item.sectionName,
            chapterNo: item.chapterNo,
            subSectionNo: item.subSectionNo,
            heading: item.heading
        };

        if (!existsInSelection(manualItem)) {
            refDetails.push(manualItem);
            renderRefs();
        }
    });

    container.off('click', '.remove-ref').on('click', '.remove-ref', function () {
        if (readOnly) return;

        var refKey = $(this).data('key');
        refDetails = $.grep(refDetails, function (v) { return getRefKey(v) !== refKey; });
        renderRefs();
    });

    container.off('click', '.view').on('click', '.view', function () {
        var url = $(this).data('url');
        if (url) {
            window.open(url, '_blank');
        }
    });

    saveBtn.off('click').on('click', function () {
        if (readOnly) return;

        var payload = { comId: comId, references: [] };
        $.each(refDetails, function (_, r) {
            var existingLink = r.linkId ? r.linkId : null;
            if (existingLink === null) {
                var found = refLinks.find(function (fl) {
                    var left = fl.linkId || ('new-' + (fl.referenceId || 0) + '-' + (fl.manualId || fl.opManualId || fl.creditManualId || 0));
                    var right = r.linkId || ('new-' + (r.id || r.manualIndexId || 0) + '-' + (r.manualId || r.opManualId || r.creditManualId || 0));
                    return left === right;
                });
                if (found) existingLink = found.linkId;
            }

            var isManual = isManualReference(r);
            var type = r.referenceType || r.manualType || r.linkType || '';
            var manualId = r.manualId || r.opManualId || r.creditManualId || null;
            var manualIndexId = r.manualIndexId || (isManual ? r.id : null);

            payload.references.push({
                linkId: existingLink,
                referenceId: isManual ? (manualIndexId || 0) : (r.id > 0 ? r.id : 0),
                manualId: manualId,
                manualIndexId: manualIndexId,
                entityId: r.divisionEntId || 0,
                oldParaId: 0,
                newParaId: 0,
                paraId: comId,
                instructionsDate: r.instructionsDate ? new Date(r.instructionsDate).toISOString() : null,
                referenceTitle: isManual ? buildManualDisplay(r) : r.instructionsTitle,
                creditManualId: type === 'Policy' ? manualId : null,
                opManualId: type === 'Manual' ? manualId : null,
                manualType: type,
                chapter: r.chapterNo || r.division,
                matchedText: null,
                linkType: type
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
            loadManualMaster();
        } else {
            searchInputs.hide();
            resultTbl.hide();
            manualInputs.hide();
        }
    }

    function renderRefs() {
        refList.empty();
        $.each(refDetails, function (i, r) {
            var dateTxt = formatDate(r.instructionsDate);
            var refKey = getRefKey(r);
            var divisionTxt = r.division || r.chapterNo || '';

            refList.append('<li class="list-group-item">'
                + '<div><strong>Reference No ' + (i + 1) + '</strong></div>'
                + '<div>' + (buildManualDisplay(r) || '') + '</div>'
                + '<div>Issuance Date: ' + dateTxt + '</div>'
                + '<div>Reference Type: ' + (r.referenceType || '') + '</div>'
                + '<div>Division Code: ' + divisionTxt + '</div>'
                + (readOnly ? '' : ' <button type="button" class="btn btn-danger btn-sm float-end remove-ref" data-key="' + refKey + '">Delete</button>')
                + '</li>');
        });
    }
}
