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
        var allowClear = options.allowClear !== false;
        var currentReferenceLabel = options.currentReferenceLabel || 'Current Saved Reference';
        var selectedReferenceLabel = options.selectedReferenceLabel || 'Selected Reference';
        var emptyCurrentText = options.emptyCurrentText || 'No reference selected yet.';
        var apiBase = options.apiBaseUrl || (window.g_asiBaseURL ? window.g_asiBaseURL + '/ApiCalls' : '/ApiCalls');
        var instanceToken = 'obsref-' + Date.now() + '-' + Math.random().toString(36).slice(2);

        var state = {
            selected: null,
            current: null,
            isEditing: !isEditMode
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

        var missingControls = [];
        if (!$type.length) missingControls.push('Reference Type');
        if (!$manual.length) missingControls.push('Manual');
        if (!$section.length) missingControls.push('Section');
        if (!$chapter.length) missingControls.push('Chapter');
        if (!$manualGrid.length) missingControls.push('Grid');

        if (missingControls.length) {
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
                $currentDisplay.html(
                    '<div class="alert alert-secondary py-2 px-3 mb-0">'
                    + '<div><strong>' + $('<div/>').text(emptyCurrentText).html() + '</strong></div>'
                    + '<div class="small text-muted">Choose a reference and save it for this observation.</div>'
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
            if (!isEditMode) {
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
            if ($editorWrapper.length && isEditMode) {
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
            state.isEditing = true;
            if (state.current && state.current.refId) {
                state.selected = normalize(state.current);
            }
            renderState();
        }

        function cancelEdit() {
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
            } else if (typeVal === 'MANUAL' || typeVal === 'POLICY') {
                $container.find('#obsReferenceManualMode').removeClass('d-none');
                log('Manual/Policy mode displayed.');
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
                state.isEditing = isEditMode;
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
                    state.isEditing = isEditMode;
                    renderState();
                }
            }).fail(function (xhr, status, error) {
                if (!isActiveInstance()) {
                    return;
                }

                warn('GetReferenceDetailByRefId failed.', status || error || xhr.statusText);
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

        switchMode('initial-load');
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

    window.initObservationReference = initObservationReference;
    window.getObservationReferenceApi = getObservationReferenceApi;
    window.getSelectedObservationReference = getSelectedObservationReference;
    window.getCurrentObservationReference = getCurrentObservationReference;
    window.commitObservationReferenceSelection = commitObservationReferenceSelection;
})(window, window.jQuery);
