(function (window, $) {
    'use strict';

    if (!$) {
        return;
    }

    function normalize(item) {
        item = item || {};
        return {
            refId: item.refId || item.REF_ID || item.refID || item.RefId || item.id || 0,
            referenceSourceType: item.referenceSourceType || item.REFERENCE_SOURCE_TYPE || item.ReferenceSourceType || '',
            sourcePkId: item.sourcePkId || item.SOURCE_PK_ID || item.SourcePkId || null,
            manualId: item.manualId || item.MANUAL_ID || item.ManualId || null,
            manualName: item.manualName || item.MANUAL_NAME || item.ManualName || '',
            sectionText: item.sectionText || item.SECTION_TEXT || item.SectionText || '',
            chapterNo: item.chapterNo || item.CHAPTER_NO || item.ChapterNo || '',
            subSectionNo: item.subSectionNo || item.SUB_SECTION_NO || item.SubSectionNo || '',
            titleOrHeading: item.titleOrHeading || item.TITLE_OR_HEADING || item.TitleOrHeading || item.heading || item.HEADING || item.Heading || '',
            instructionDate: item.instructionDate || item.INSTRUCTION_DATE || item.InstructionDate || null,
            displayText: item.displayText || item.DISPLAY_TEXT || item.DisplayText || ''
        };
    }

    function initObservationReference(containerSelector, options) {
        var $container = $(containerSelector || '#observationReferenceSection').first();
        if (!$container.length) {
            return;
        }

        options = options || {};

        var logPrefix = '[obsreference]';
        function log(message, data) {
            if (!window.console || typeof window.console.log !== 'function') {
                return;
            }

            if (typeof data === 'undefined') {
                window.console.log(logPrefix + ' ' + message);
                return;
            }

            window.console.log(logPrefix + ' ' + message, data);
        }

        function warn(message, data) {
            if (!window.console || typeof window.console.warn !== 'function') {
                return;
            }

            if (typeof data === 'undefined') {
                window.console.warn(logPrefix + ' ' + message);
                return;
            }

            window.console.warn(logPrefix + ' ' + message, data);
        }

        if ($container.data('obs-reference-initialized')) {
            if (!options.forceReload) {
                log('Observation reference picker already initialized for container.', containerSelector || '#observationReferenceSection');
                return;
            }

            log('Reinitializing observation reference picker.', containerSelector || '#observationReferenceSection');
            $container.off('.obsReference');
            $container.removeData('obs-reference-initialized');
            $container.removeData('obs-reference-state');
            $container.removeData('obs-reference-api');
        }

        var isEditMode = !!options.editMode;
        var isReadOnly = !!options.readOnly;
        var allowClear = options.allowClear !== false;
        var currentReferenceLabel = options.currentReferenceLabel || 'Current Saved Reference';
        var selectedReferenceLabel = options.selectedReferenceLabel || 'Selected Reference';
        var emptyCurrentText = options.emptyCurrentText || 'No reference selected yet.';
        var apiBase = options.apiBaseUrl || (window.g_asiBaseURL ? window.g_asiBaseURL + '/ApiCalls' : '/ApiCalls');
        var instanceToken = 'obsref-' + Date.now() + '-' + Math.random().toString(36).slice(2);

        var state = {
            selected: null,
            current: null,
            isEditing: isReadOnly ? false : !isEditMode
        };

        var $hiddenRef = $container.find('#observationReferenceId');
        var $type = $container.find('#obsReferenceType');
        var $search = $container.find('#obsReferenceSearchText');
        var $selected = $container.find('#obsReferenceSelected');
        var $currentDisplay = $container.find('#obsReferenceCurrentDisplay');
        var $changeBtn = $container.find('#obsReferenceChangeBtn');
        var $cancelEditBtn = $container.find('#obsReferenceCancelEditBtn');
        var $saveUpdateBtn = $container.find('#obsReferenceSaveUpdateBtn');
        var $editorWrapper = $container.find('#obsReferenceEditorWrapper');
        var $manual = $container.find('#obsReferenceManual');
        var $section = $container.find('#obsReferenceSectionSelect');
        var $chapter = $container.find('#obsReferenceChapter');
        var $manualGrid = $container.find('#obsReferenceManualGrid');

        var supportsEditorControls = $type.length && $manual.length && $section.length && $chapter.length && $manualGrid.length;
        var missingControls = [];
        if (!supportsEditorControls) {
            if (!$type.length) missingControls.push('Reference Type');
            if (!$manual.length) missingControls.push('Manual');
            if (!$section.length) missingControls.push('Section');
            if (!$chapter.length) missingControls.push('Chapter');
            if (!$manualGrid.length) missingControls.push('Grid');
        }

        if (missingControls.length && !isReadOnly) {
            warn('Observation reference markup is missing controls.', missingControls);
        }

        function isActiveInstance() {
            return $container.data('obs-reference-instance-token') === instanceToken;
        }

        function setHidden(refId) {
            $hiddenRef.val(refId || '');
        }

        function formatDisplay(selected) {
            if (!selected) {
                return '';
            }

            if (selected.referenceSourceType && selected.referenceSourceType.toUpperCase() === 'MANUAL_INDEX') {
                return [selected.manualName, selected.sectionText, selected.chapterNo, selected.subSectionNo, selected.titleOrHeading]
                    .filter(Boolean)
                    .join(' / ');
            }

            var dt = selected.instructionDate ? (' (' + selected.instructionDate.toString().substring(0, 10) + ')') : '';
            return (selected.displayText || selected.titleOrHeading || ('Reference #' + selected.refId)) + dt;
        }

        function renderCurrentDisplay() {
            if (!$currentDisplay.length) {
                return;
            }

            if (!state.current || !state.current.refId) {
                var emptySubText = isReadOnly
                    ? 'No saved reference is available for this observation.'
                    : 'Choose a reference and save it for this observation.';
                $currentDisplay.html(
                    '<div class="alert alert-secondary py-2 px-3 mb-0">'
                    + '<div><strong>' + $('<div/>').text(emptyCurrentText).html() + '</strong></div>'
                    + '<div class="small text-muted">' + $('<div/>').text(emptySubText).html() + '</div>'
                    + '</div>'
                );
                return;
            }

            $currentDisplay.html(
                '<div class="alert alert-info py-2 px-3 mb-0">'
                + '<div><strong>' + $('<div/>').text(currentReferenceLabel + ':').html() + '</strong> ' + $('<div/>').text(formatDisplay(state.current)).html() + '</div>'
                + '<div class="small text-muted">REF_ID: ' + state.current.refId + '</div>'
                + '</div>'
            );
        }

        function renderSelected() {
            if (!$selected.length) {
                return;
            }

            if (isReadOnly) {
                $selected.empty();
                setHidden(state.current && state.current.refId ? state.current.refId : '');
                return;
            }

            if (isEditMode && !state.isEditing) {
                $selected.empty();
                setHidden(state.current && state.current.refId ? state.current.refId : '');
                return;
            }

            if (!state.selected || !state.selected.refId) {
                $selected.html('<span class="text-muted">No reference selected.</span>');
                setHidden(state.current && state.current.refId ? state.current.refId : '');
                return;
            }

            setHidden(state.selected.refId);
            $selected.html(
                '<div class="alert alert-success py-2 px-3 mb-0">'
                + '<div><strong>' + $('<div/>').text(selectedReferenceLabel + ':').html() + '</strong> ' + $('<div/>').text(formatDisplay(state.selected)).html() + '</div>'
                + '<div class="small text-muted">REF_ID: ' + state.selected.refId + '</div>'
                + (allowClear
                    ? '<button type="button" class="btn btn-sm btn-outline-danger mt-2" id="obsReferenceClear">Clear</button>'
                    : '')
                + '</div>'
            );
        }

        function renderActionButtons() {
            if (!isEditMode || isReadOnly) {
                if ($changeBtn.length) {
                    $changeBtn.addClass('d-none');
                }

                if ($cancelEditBtn.length) {
                    $cancelEditBtn.addClass('d-none');
                }

                if ($saveUpdateBtn.length) {
                    $saveUpdateBtn.addClass('d-none');
                }

                return;
            }

            var hasCurrent = !!(state.current && state.current.refId);
            var hasSelected = !!(state.selected && state.selected.refId);
            var selectionChanged = !hasCurrent || !state.current || !state.selected || state.current.refId !== state.selected.refId;

            if ($changeBtn.length) {
                $changeBtn.toggleClass('d-none', state.isEditing || !hasCurrent);
            }

            if ($cancelEditBtn.length) {
                $cancelEditBtn.toggleClass('d-none', !state.isEditing || !hasCurrent);
            }

            if ($saveUpdateBtn.length) {
                $saveUpdateBtn.toggleClass('d-none', !state.isEditing);
                $saveUpdateBtn.prop('disabled', !hasSelected || !selectionChanged);
            }
        }

        function syncEditorVisibility() {
            if (!$editorWrapper.length) {
                return;
            }

            if (isReadOnly) {
                $editorWrapper.addClass('d-none');
                return;
            }

            if (isEditMode) {
                $editorWrapper.toggleClass('d-none', !state.isEditing);
            }
        }

        function renderState() {
            renderCurrentDisplay();
            syncEditorVisibility();
            renderSelected();
            renderActionButtons();
        }

        function selectReference(item, markAsCurrent) {
            state.selected = normalize(item);
            if (markAsCurrent) {
                state.current = normalize(item);
            }
            renderState();
        }

        function beginEdit() {
            if (isReadOnly) {
                return;
            }

            state.isEditing = true;
            if (state.current && state.current.refId) {
                state.selected = normalize(state.current);
            }
            renderState();
        }

        function cancelEdit() {
            if (isReadOnly) {
                return;
            }

            if (state.current && state.current.refId) {
                state.selected = normalize(state.current);
                state.isEditing = false;
            } else {
                state.selected = null;
                state.isEditing = true;
            }
            renderState();
        }

        function commitSelected() {
            if (isReadOnly) {
                return state.current || null;
            }

            if (!state.selected || !state.selected.refId) {
                return null;
            }

            state.current = normalize(state.selected);
            state.isEditing = false;
            renderState();
            return state.current;
        }

        function clearResults() {
            $container.find('#obsReferenceCircularResults tbody').empty();
            $container.find('#obsReferenceManualGrid tbody').empty();
        }

        function resetManualControls() {
            if ($manual.length) {
                $manual.empty().append('<option value="">--Select Manual--</option>');
            }
            if ($section.length) {
                $section.empty().append('<option value="">--Select Section--</option>');
            }
            if ($chapter.length) {
                $chapter.empty().append('<option value="">--Select Chapter--</option>');
            }
            if ($manualGrid.length) {
                $manualGrid.find('tbody').empty();
            }
        }

        function switchMode(triggerSource) {
            var typeVal = ($type.val() || '').toUpperCase();
            log('toggle mode fired.', { trigger: triggerSource || 'unknown', referenceType: typeVal });
            $container.find('.obs-ref-mode').addClass('d-none');
            if (typeVal === 'CIRCULAR') {
                $container.find('#obsReferenceCircularMode').removeClass('d-none');
                log('Circular mode displayed.');
            } else if (typeVal === 'MANUAL') {
                $container.find('#obsReferenceManualMode').removeClass('d-none');
                log('Manual mode displayed.');
                loadManualMaster(triggerSource || 'type-change');
            } else {
                resetManualControls();
                log('Reference picker reset because no mode is selected.');
            }
            clearResults();
        }

        function loadCircular() {
            log('GetReferenceMasterDetail firing.', {
                searchText: $search.val() || '',
                sourceType: 'CIRCULAR'
            });
            $.get(apiBase + '/GetReferenceMasterDetail', {
                searchText: $search.val() || '',
                sourceType: 'CIRCULAR'
            }).done(function (rows) {
                if (!isActiveInstance()) {
                    log('Ignoring stale circular reference response.');
                    return;
                }

                log('GetReferenceMasterDetail completed.', { rowCount: (rows || []).length });
                var $tbody = $container.find('#obsReferenceCircularResults tbody');
                $tbody.empty();
                (rows || []).forEach(function (row) {
                    var n = normalize(row);
                    var dt = n.instructionDate ? n.instructionDate.toString().substring(0, 10) : '';
                    $tbody.append('<tr>'
                        + '<td>' + n.refId + '</td>'
                        + '<td>' + $('<div/>').text(n.displayText || n.titleOrHeading || '').html() + '</td>'
                        + '<td>' + dt + '</td>'
                        + '<td>' + $('<div/>').text(n.referenceSourceType || '').html() + '</td>'
                        + '<td><button type="button" class="btn btn-sm btn-primary obs-ref-select" data-refid="' + n.refId + '">Select</button></td>'
                        + '</tr>');
                    $tbody.find('button[data-refid="' + n.refId + '"]').data('row', n);
                });
            }).fail(function (xhr, status, error) {
                if (!isActiveInstance()) {
                    return;
                }

                warn('GetReferenceMasterDetail failed.', status || error || xhr.statusText);
            });
        }

        function loadManualMaster(triggerSource) {
            resetManualControls();
            log('GetManualMaster firing.', {
                trigger: triggerSource || 'unknown',
                referenceType: ($type.val() || '').toUpperCase()
            });
            $.get(apiBase + '/GetObservationManualMaster').done(function (rows) {
                if (!isActiveInstance()) {
                    log('Ignoring stale manual master response.');
                    return;
                }

                log('GetManualMaster completed.', { rowCount: (rows || []).length });
                (rows || []).forEach(function (item) {
                    $manual.append('<option value="' + (item.manualId || item.MANUAL_ID || 0) + '">' + $('<div/>').text(item.displayLabel || item.DISPLAY_NAME || '').html() + '</option>');
                });
            }).fail(function (xhr, status, error) {
                if (!isActiveInstance()) {
                    return;
                }

                warn('GetManualMaster failed.', status || error || xhr.statusText);
            });
        }

        function loadSections() {
            if ($section.length) {
                $section.empty().append('<option value="">--Select Section--</option>');
            }
            if ($chapter.length) {
                $chapter.empty().append('<option value="">--Select Chapter--</option>');
            }
            if ($manualGrid.length) {
                $manualGrid.find('tbody').empty();
            }
            if (!$manual.val()) {
                log('GetManualSections skipped because no manual is selected.');
                return;
            }

            log('GetManualSections firing.', { manualId: $manual.val() });
            $.get(apiBase + '/GetObservationManualSections', { manualId: $manual.val() }).done(function (rows) {
                if (!isActiveInstance()) {
                    log('Ignoring stale section response.');
                    return;
                }

                log('GetManualSections completed.', { rowCount: (rows || []).length, manualId: $manual.val() });
                (rows || []).forEach(function (item) {
                    var sectionText = item.sectionText || item.SECTION_TEXT || item.sectionName || '';
                    $section.append('<option value="' + $('<div/>').text(sectionText).html() + '">' + $('<div/>').text(sectionText).html() + '</option>');
                });
            }).fail(function (xhr, status, error) {
                if (!isActiveInstance()) {
                    return;
                }

                warn('GetManualSections failed.', status || error || xhr.statusText);
            });
        }

        function loadChapters() {
            if ($chapter.length) {
                $chapter.empty().append('<option value="">--Select Chapter--</option>');
            }
            if ($manualGrid.length) {
                $manualGrid.find('tbody').empty();
            }
            if (!$manual.val() || !$section.val()) {
                log('GetManualChapters skipped because manual or section is missing.', {
                    manualId: $manual.val() || '',
                    sectionText: $section.val() || ''
                });
                return;
            }

            log('GetManualChapters firing.', {
                manualId: $manual.val(),
                sectionText: $section.val()
            });
            $.get(apiBase + '/GetObservationManualChapters', { manualId: $manual.val(), sectionText: $section.val() }).done(function (rows) {
                if (!isActiveInstance()) {
                    log('Ignoring stale chapter response.');
                    return;
                }

                log('GetManualChapters completed.', {
                    rowCount: (rows || []).length,
                    manualId: $manual.val(),
                    sectionText: $section.val()
                });
                (rows || []).forEach(function (item) {
                    var chapterNo = item.chapterNo || item.CHAPTER_NO || '';
                    $chapter.append('<option value="' + $('<div/>').text(chapterNo).html() + '">' + $('<div/>').text(chapterNo).html() + '</option>');
                });
            }).fail(function (xhr, status, error) {
                if (!isActiveInstance()) {
                    return;
                }

                warn('GetManualChapters failed.', status || error || xhr.statusText);
            });
        }

        function loadManualGrid() {
            var $tbody = $container.find('#obsReferenceManualGrid tbody');
            $tbody.empty();
            if (!$manual.val() || !$section.val() || !$chapter.val()) {
                log('GetManualReferenceGrid skipped because manual, section, or chapter is missing.', {
                    manualId: $manual.val() || '',
                    sectionText: $section.val() || '',
                    chapterNo: $chapter.val() || ''
                });
                return;
            }

            log('GetManualReferenceGrid firing.', {
                manualId: $manual.val(),
                sectionText: $section.val(),
                chapterNo: $chapter.val(),
                sourceType: ($type.val() || '').toUpperCase()
            });
            $.get(apiBase + '/GetObservationManualReferenceGrid', {
                manualId: $manual.val(),
                sectionText: $section.val(),
                chapterNo: $chapter.val(),
                sourceType: ($type.val() || '').toUpperCase()
            }).done(function (rows) {
                if (!isActiveInstance()) {
                    log('Ignoring stale manual grid response.');
                    return;
                }

                log('GetManualReferenceGrid completed.', {
                    rowCount: (rows || []).length,
                    manualId: $manual.val(),
                    sectionText: $section.val(),
                    chapterNo: $chapter.val()
                });
                (rows || []).forEach(function (row) {
                    var n = normalize(row);
                    n.manualName = $manual.find(':selected').text();
                    $tbody.append('<tr>'
                        + '<td>' + $('<div/>').text(n.subSectionNo || '').html() + '</td>'
                        + '<td>' + $('<div/>').text(n.titleOrHeading || '').html() + '</td>'
                        + '<td><button type="button" class="btn btn-sm btn-primary obs-ref-select" data-refid="' + n.refId + '">Select</button></td>'
                        + '</tr>');
                    $tbody.find('button[data-refid="' + n.refId + '"]').data('row', n);
                });
            }).fail(function (xhr, status, error) {
                if (!isActiveInstance()) {
                    return;
                }

                warn('GetManualReferenceGrid failed.', status || error || xhr.statusText);
            });
        }

        function loadExistingByRefId(refId) {
            if (!refId) {
                state.current = null;
                state.selected = null;
                state.isEditing = isReadOnly ? false : isEditMode;
                renderState();
                return;
            }

            log('Loading existing reference by REF_ID.', { refId: refId });
            $.get(apiBase + '/GetReferenceDetailByRefId', { refId: refId }).done(function (item) {
                if (!isActiveInstance()) {
                    log('Ignoring stale existing reference response.', { refId: refId });
                    return;
                }

                if (item) {
                    log('Existing reference loaded.', { refId: refId });
                    state.isEditing = false;
                    selectReference(item, true);
                } else {
                    state.current = null;
                    state.selected = null;
                    state.isEditing = isReadOnly ? false : isEditMode;
                    renderState();
                }
            }).fail(function (xhr, status, error) {
                if (!isActiveInstance()) {
                    return;
                }

                warn('GetReferenceDetailByRefId failed.', status || error || xhr.statusText);
                state.current = normalize({
                    refId: refId,
                    titleOrHeading: 'Saved reference #' + refId
                });
                state.selected = normalize(state.current);
                state.isEditing = false;
                renderState();
            });
        }

        $container.off('.obsReference');

        $container.on('click.obsReference', '.obs-ref-select', function () {
            var row = $(this).data('row');
            log('Reference selected from grid.', { refId: row && row.refId ? row.refId : 0 });
            selectReference(row, false);
        });

        $container.on('click.obsReference', '#obsReferenceSearchBtn', loadCircular);
        $container.on('change.obsReference', '#obsReferenceType', function () {
            switchMode('change');
        });
        $container.on('change.obsReference', '#obsReferenceManual', loadSections);
        $container.on('change.obsReference', '#obsReferenceSectionSelect', loadChapters);
        $container.on('change.obsReference', '#obsReferenceChapter', loadManualGrid);
        $container.on('click.obsReference', '#obsReferenceChangeBtn', beginEdit);
        $container.on('click.obsReference', '#obsReferenceCancelEditBtn', cancelEdit);
        $container.on('click.obsReference', '#obsReferenceClear', function () {
            log('Selected reference cleared.');
            state.selected = null;
            renderState();
        });

        var api = {
            beginEdit: beginEdit,
            cancelEdit: cancelEdit,
            commitSelected: commitSelected,
            getSelected: function () { return state.selected || null; },
            getCurrent: function () { return state.current || null; },
            setCurrentByRefId: loadExistingByRefId
        };

        $container.data('obs-reference-initialized', true);
        $container.data('obs-reference-instance-token', instanceToken);
        $container.data('obs-reference-state', state);
        $container.data('obs-reference-api', api);

        if (supportsEditorControls) {
            switchMode('initial-load');
        }
        loadExistingByRefId($hiddenRef.val() || options.initialRefId);
        renderState();
    }

    function getObservationReferenceApi(containerSelector) {
        var $container = $(containerSelector || '#observationReferenceSection');
        return $container.data('obs-reference-api') || null;
    }

    function getSelectedObservationReference(containerSelector) {
        var api = getObservationReferenceApi(containerSelector);
        if (api && typeof api.getSelected === 'function') {
            return api.getSelected();
        }

        var $container = $(containerSelector || '#observationReferenceSection');
        var state = $container.data('obs-reference-state') || {};
        return state.selected || null;
    }

    function getCurrentObservationReference(containerSelector) {
        var api = getObservationReferenceApi(containerSelector);
        if (api && typeof api.getCurrent === 'function') {
            return api.getCurrent();
        }

        var $container = $(containerSelector || '#observationReferenceSection');
        var state = $container.data('obs-reference-state') || {};
        return state.current || null;
    }

    function commitObservationReferenceSelection(containerSelector) {
        var api = getObservationReferenceApi(containerSelector);
        if (api && typeof api.commitSelected === 'function') {
            return api.commitSelected();
        }

        return null;
    }

    function initLegacyReferenceSection(comId, readOnly, containerSelector) {
        var $legacyContainer = containerSelector ? $(containerSelector) : $('#referenceSection');
        if (!$legacyContainer.length) {
            return null;
        }

        var searchSection = $legacyContainer.find('#searchSection');
        var saveBtn = $legacyContainer.find('#saveBtn');
        var resultTbl = $legacyContainer.find('#resultTbl');
        var refList = $legacyContainer.find('#refList');
        var refType = $legacyContainer.find('#refType');
        var keywordInput = $legacyContainer.find('#keyword');
        var searchBtn = $legacyContainer.find('#searchBtn');
        var searchInputs = $legacyContainer.find('#searchInputs');

        var manualInputs = $legacyContainer.find('#manualInputs');
        var manualMaster = $legacyContainer.find('#manualMaster');
        var manualSection = $legacyContainer.find('#manualSection');
        var manualChapter = $legacyContainer.find('#manualChapter');
        var manualIndexGridWrapper = $legacyContainer.find('#manualIndexGridWrapper');
        var manualIndexTbl = $legacyContainer.find('#manualIndexTbl');

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
        clearLegacyManualGrid();

        function formatLegacyDate(dateStr) {
            if (!dateStr) {
                return '';
            }

            var parts = dateStr.split('T')[0].split('-');
            if (parts.length !== 3) {
                return '';
            }

            return parts[2] + '/' + parts[1] + '/' + parts[0];
        }

        function isLegacyManualReference(item) {
            var type = (item.referenceType || item.manualType || item.linkType || '').toUpperCase();
            return type === 'MANUAL';
        }

        function getLegacyRefKey(item) {
            if (item.linkId) {
                return 'link-' + item.linkId;
            }

            if (isLegacyManualReference(item)) {
                var manualId = item.manualId || item.opManualId || item.creditManualId || 0;
                var manualIndexId = item.manualIndexId || item.id || 0;
                return 'manual-' + manualId + '-' + manualIndexId;
            }

            return 'ref-' + (item.id || 0);
        }

        function existsInLegacySelection(item) {
            var key = getLegacyRefKey(item);
            return refDetails.some(function (entry) {
                return getLegacyRefKey(entry) === key;
            });
        }

        function normalizeLegacyLoadedReferences(data) {
            var details = data.referenceDetails || [];
            var links = data.referenceLinks || [];

            $.each(links, function (_, link) {
                var hasExistingDetail = details.some(function (detail) {
                    return detail.linkId && detail.linkId === link.linkId;
                });
                var type = (link.manualType || link.linkType || '').toUpperCase();

                if (!hasExistingDetail && type === 'MANUAL') {
                    details.push({
                        id: link.manualIndexId || link.referenceId,
                        linkId: link.linkId,
                        divisionEntId: link.entityId,
                        referenceType: link.manualType || link.linkType,
                        instructionsTitle: link.referenceTitle,
                        instructionsDate: link.instructionsDate,
                        division: link.chapter,
                        manualId: link.manualId || link.opManualId || link.creditManualId,
                        manualIndexId: link.manualIndexId || link.referenceId,
                        chapterNo: link.chapterNo,
                        sectionName: link.sectionName,
                        subSectionNo: link.subSectionNo,
                        heading: link.heading
                    });
                }
            });

            return details;
        }

        function buildLegacyManualDisplay(item) {
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

        function clearLegacyManualGrid() {
            manualIndexTbl.find('tbody').empty();
            manualIndexGridWrapper.hide();
        }

        function resetLegacySectionsAndBelow() {
            manualSection.val('');
            manualSection.prop('disabled', true);
            manualChapter.val('');
            manualChapter.prop('disabled', true);
            clearLegacyManualGrid();
        }

        function resetLegacyChaptersAndGrid() {
            manualChapter.val('');
            manualChapter.prop('disabled', true);
            clearLegacyManualGrid();
        }

        function loadLegacyManualMaster() {
            manualMaster.empty().append('<option value="">Select manual</option>');
            resetLegacySectionsAndBelow();

            $.get(g_asiBaseURL + '/ApiCalls/GetManualMaster', function (rows) {
                $.each(rows || [], function (_, item) {
                    var label = item.displayLabel || [item.manualName, item.volumeName].filter(Boolean).join(' ');
                    manualMaster.append('<option value="' + item.manualId + '">' + label + '</option>');
                });
            });
        }

        function loadLegacyManualSections(manualId) {
            manualSection.empty().append('<option value="">Select section</option>');
            resetLegacyChaptersAndGrid();

            if (!manualId) {
                manualSection.prop('disabled', true);
                return;
            }

            $.get(g_asiBaseURL + '/ApiCalls/GetManualSections', { manualId: manualId }, function (rows) {
                $.each(rows || [], function (_, item) {
                    manualSection.append('<option value="' + item.sectionName + '">' + item.sectionName + '</option>');
                });
                manualSection.prop('disabled', false);
            });
        }

        function loadLegacyManualChapters(manualId, sectionName) {
            manualChapter.empty().append('<option value="">Select chapter</option>');
            clearLegacyManualGrid();

            if (!manualId || !sectionName) {
                manualChapter.prop('disabled', true);
                return;
            }

            $.get(g_asiBaseURL + '/ApiCalls/GetManualChapters', { manualId: manualId, sectionName: sectionName }, function (rows) {
                $.each(rows || [], function (_, item) {
                    manualChapter.append('<option value="' + item.chapterNo + '">' + item.chapterNo + '</option>');
                });
                manualChapter.prop('disabled', false);
            });
        }

        function loadLegacyManualIndexGrid(manualId, sectionName, chapterNo) {
            clearLegacyManualGrid();

            if (!manualId || !sectionName || !chapterNo) {
                return;
            }

            $.get(g_asiBaseURL + '/ApiCalls/GetManualIndexByChapter', {
                manualId: manualId,
                sectionName: sectionName,
                chapterNo: chapterNo
            }, function (rows) {
                var body = manualIndexTbl.find('tbody');
                body.empty();

                $.each(rows || [], function (_, item) {
                    var payload = {
                        manualId: parseInt(manualId, 10),
                        manualIndexId: item.indexId,
                        manualName: manualMaster.find('option:selected').text(),
                        sectionName: sectionName,
                        chapterNo: chapterNo,
                        subSectionNo: item.subSectionNo,
                        heading: item.heading,
                        referenceType: refType.val()
                    };

                    body.append('<tr>'
                        + (readOnly ? '' : '<td><button type="button" class="btn btn-sm btn-primary attach-manual" data-item="' + encodeURIComponent(JSON.stringify(payload)) + '">Add</button></td>')
                        + '<td>' + (item.subSectionNo || '') + '</td>'
                        + '<td>' + (item.heading || '') + '</td>'
                        + '</tr>');
                });

                manualIndexGridWrapper.show();
            });
        }

        function toggleLegacyInputMode() {
            var type = refType.val();

            if (type === 'Circular') {
                searchInputs.show();
                resultTbl.show();
                manualInputs.hide();
            } else if (type === 'Manual') {
                searchInputs.hide();
                resultTbl.hide();
                manualInputs.show();
                loadLegacyManualMaster();
            } else {
                searchInputs.hide();
                resultTbl.hide();
                manualInputs.hide();
            }
        }

        function renderLegacyRefs() {
            refList.empty();
            $.each(refDetails, function (index, item) {
                var dateTxt = formatLegacyDate(item.instructionsDate);
                var refKey = getLegacyRefKey(item);
                var divisionTxt = item.division || item.chapterNo || '';

                refList.append('<li class="list-group-item">'
                    + '<div><strong>Reference No ' + (index + 1) + '</strong></div>'
                    + '<div>' + (buildLegacyManualDisplay(item) || '') + '</div>'
                    + '<div>Issuance Date: ' + dateTxt + '</div>'
                    + '<div>Reference Type: ' + (item.referenceType || '') + '</div>'
                    + '<div>Division Code: ' + divisionTxt + '</div>'
                    + (readOnly ? '' : ' <button type="button" class="btn btn-danger btn-sm float-end remove-ref" data-key="' + refKey + '">Delete</button>')
                    + '</li>');
            });
        }

        $.get(g_asiBaseURL + '/ApiCalls/GetParaReferenceData', { comId: comId }, function (data) {
            refDetails = normalizeLegacyLoadedReferences(data);
            refLinks = data.referenceLinks || [];
            renderLegacyRefs();
        });

        searchBtn.off('click.obsLegacyReference').on('click.obsLegacyReference', function () {
            $.post(g_asiBaseURL + '/ApiCalls/SearchReferences', { referenceType: refType.val(), keyword: keywordInput.val() }, function (rows) {
                var body = resultTbl.find('tbody');
                body.empty();

                $.each(rows || [], function (_, item) {
                    var dateTxt = formatLegacyDate(item.instructionsDate);
                    body.append('<tr>'
                        + '<td>' + (item.title || '') + '</td>'
                        + '<td>' + dateTxt + '</td>'
                        + '<td>' + (item.instructionsdetails || '') + '</td>'
                        + '<td>' + (item.keywords || '') + '</td>'
                        + '<td><button type="button" class="view btn btn-sm btn-secondary" data-url="' + (item.referenceurl || '') + '">View</button></td>'
                        + (readOnly ? '' : '<td><button type="button" class="attach btn btn-sm btn-primary" data-id="' + item.id + '">Attach</button></td>')
                        + '</tr>');
                });
            });
        });

        refType.off('change.obsLegacyReference').on('change.obsLegacyReference', toggleLegacyInputMode);
        manualMaster.off('change.obsLegacyReference').on('change.obsLegacyReference', function () {
            loadLegacyManualSections($(this).val());
        });
        manualSection.off('change.obsLegacyReference').on('change.obsLegacyReference', function () {
            loadLegacyManualChapters(manualMaster.val(), $(this).val());
        });
        manualChapter.off('change.obsLegacyReference').on('change.obsLegacyReference', function () {
            loadLegacyManualIndexGrid(manualMaster.val(), manualSection.val(), $(this).val());
        });

        $legacyContainer.off('click.obsLegacyReference', '.attach').on('click.obsLegacyReference', '.attach', function (event) {
            event.preventDefault();
            if (readOnly) {
                return;
            }

            var refId = parseInt($(this).data('id'), 10);
            $.get(g_asiBaseURL + '/ApiCalls/GetReferenceDetail', { refId: refId }, function (detail) {
                if (!detail) {
                    return;
                }

                detail.linkId = null;
                if (!existsInLegacySelection(detail)) {
                    refDetails.push(detail);
                    renderLegacyRefs();
                }
            });
        });

        $legacyContainer.off('click.obsLegacyReference', '.attach-manual').on('click.obsLegacyReference', '.attach-manual', function () {
            if (readOnly) {
                return;
            }

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

            if (!existsInLegacySelection(manualItem)) {
                refDetails.push(manualItem);
                renderLegacyRefs();
            }
        });

        $legacyContainer.off('click.obsLegacyReference', '.remove-ref').on('click.obsLegacyReference', '.remove-ref', function () {
            if (readOnly) {
                return;
            }

            var refKey = $(this).data('key');
            refDetails = $.grep(refDetails, function (item) {
                return getLegacyRefKey(item) !== refKey;
            });
            renderLegacyRefs();
        });

        $legacyContainer.off('click.obsLegacyReference', '.view').on('click.obsLegacyReference', '.view', function () {
            var url = $(this).data('url');
            if (url) {
                window.open(url, '_blank');
            }
        });

        saveBtn.off('click.obsLegacyReference').on('click.obsLegacyReference', function () {
            if (readOnly) {
                return;
            }

            var payload = { comId: comId, references: [] };

            $.each(refDetails, function (_, item) {
                var existingLink = item.linkId ? item.linkId : null;
                if (existingLink === null) {
                    var found = refLinks.find(function (link) {
                        var left = link.linkId || ('new-' + (link.referenceId || 0) + '-' + (link.manualId || link.opManualId || link.creditManualId || 0));
                        var right = item.linkId || ('new-' + (item.id || item.manualIndexId || 0) + '-' + (item.manualId || item.opManualId || item.creditManualId || 0));
                        return left === right;
                    });
                    if (found) {
                        existingLink = found.linkId;
                    }
                }

                var isManual = isLegacyManualReference(item);
                var type = item.referenceType || item.manualType || item.linkType || '';
                var manualId = item.manualId || item.opManualId || item.creditManualId || null;
                var manualIndexId = item.manualIndexId || (isManual ? item.id : null);

                payload.references.push({
                    linkId: existingLink,
                    referenceId: isManual ? (manualIndexId || 0) : (item.id > 0 ? item.id : 0),
                    manualId: manualId,
                    manualIndexId: manualIndexId,
                    entityId: item.divisionEntId || 0,
                    oldParaId: 0,
                    newParaId: 0,
                    paraId: comId,
                    instructionsDate: item.instructionsDate ? new Date(item.instructionsDate).toISOString() : null,
                    referenceTitle: isManual ? buildLegacyManualDisplay(item) : item.instructionsTitle,
                    creditManualId: null,
                    opManualId: type === 'Manual' ? manualId : null,
                    manualType: type,
                    chapter: item.chapterNo || item.division,
                    matchedText: null,
                    linkType: type
                });
            });

            $.ajax({
                url: g_asiBaseURL + '/ApiCalls/SaveParaReferences',
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(payload),
                success: function (message) {
                    alert(message);
                    $legacyContainer.trigger('referenceSectionSaved');
                    initLegacyReferenceSection(comId, readOnly, containerSelector);
                }
            });
        });

        toggleLegacyInputMode();

        var legacyApi = {
            getReferences: function () { return refDetails.slice(); },
            reload: function () { return initLegacyReferenceSection(comId, readOnly, containerSelector); }
        };

        $legacyContainer.data('obs-legacy-reference-api', legacyApi);
        return legacyApi;
    }

    window.initObservationReference = initObservationReference;
    window.getObservationReferenceApi = getObservationReferenceApi;
    window.getSelectedObservationReference = getSelectedObservationReference;
    window.getCurrentObservationReference = getCurrentObservationReference;
    window.commitObservationReferenceSelection = commitObservationReferenceSelection;
    window.initReferenceSection = initLegacyReferenceSection;
    window.ReferenceSection = window.ReferenceSection || {};
    window.ReferenceSection.init = function (options) {
        options = options || {};
        return initLegacyReferenceSection(
            options.comId || options.COM_ID || 0,
            !!options.readOnly,
            options.containerSelector
        );
    };
})(window, window.jQuery);
