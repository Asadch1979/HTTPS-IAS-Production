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
            titleOrHeading: item.titleOrHeading || item.TITLE_OR_HEADING || item.TitleOrHeading || '',
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
            log('Step 5 reference picker already initialized for container.', containerSelector || '#observationReferenceSection');
            return;
        }

        var state = { selected: null };
        var apiBase = options.apiBaseUrl || (window.g_asiBaseURL ? window.g_asiBaseURL + '/ApiCalls' : '/ApiCalls');

        var $hiddenRef = $container.find('#observationReferenceId');
        var $type = $container.find('#obsReferenceType');
        var $search = $container.find('#obsReferenceSearchText');
        var $selected = $container.find('#obsReferenceSelected');
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
            warn('Step 5 markup is missing reference controls.', missingControls);
        }

        log('Initializing Step 5 reference picker.', {
            containerId: $container.attr('id') || null,
            referenceType: ($type.val() || '').toUpperCase()
        });

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

        function renderSelected() {
            if (!state.selected || !state.selected.refId) {
                $selected.html('<span class="text-muted">No reference selected.</span>');
                setHidden('');
                return;
            }

            setHidden(state.selected.refId);
            $selected.html(
                '<div class="alert alert-success py-2 px-3 mb-0">'
                + '<div><strong>Selected Reference:</strong> ' + $('<div/>').text(formatDisplay(state.selected)).html() + '</div>'
                + '<div class="small text-muted">REF_ID: ' + state.selected.refId + '</div>'
                + '<button type="button" class="btn btn-sm btn-outline-danger mt-2" id="obsReferenceClear">Clear</button>'
                + '</div>'
            );
        }

        function selectReference(item) {
            state.selected = normalize(item);
            renderSelected();
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
                log('GetManualMaster completed.', { rowCount: (rows || []).length });
                (rows || []).forEach(function (item) {
                    $manual.append('<option value="' + (item.manualId || item.MANUAL_ID || 0) + '">' + $('<div/>').text(item.displayLabel || item.DISPLAY_NAME || '').html() + '</option>');
                });
            }).fail(function (xhr, status, error) {
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
                log('GetManualSections completed.', { rowCount: (rows || []).length, manualId: $manual.val() });
                (rows || []).forEach(function (item) {
                    var sectionText = item.sectionText || item.SECTION_TEXT || item.sectionName || '';
                    $section.append('<option value="' + $('<div/>').text(sectionText).html() + '">' + $('<div/>').text(sectionText).html() + '</option>');
                });
            }).fail(function (xhr, status, error) {
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
                warn('GetManualReferenceGrid failed.', status || error || xhr.statusText);
            });
        }

        function loadExistingByRefId(refId) {
            if (!refId) {
                renderSelected();
                return;
            }
            log('Loading existing reference by REF_ID.', { refId: refId });
            $.get(apiBase + '/GetReferenceDetailByRefId', { refId: refId }).done(function (item) {
                if (item) {
                    log('Existing reference loaded.', { refId: refId });
                    selectReference(item);
                }
            }).fail(function (xhr, status, error) {
                warn('GetReferenceDetailByRefId failed.', status || error || xhr.statusText);
            });
        }

        $container.off('.obsReference');

        $container.on('click.obsReference', '.obs-ref-select', function () {
            var row = $(this).data('row');
            log('Reference selected from grid.', { refId: row && row.refId ? row.refId : 0 });
            selectReference(row);
        });

        $container.on('click.obsReference', '#obsReferenceSearchBtn', loadCircular);
        $container.on('change.obsReference', '#obsReferenceType', function () {
            switchMode('change');
        });
        $container.on('change.obsReference', '#obsReferenceManual', loadSections);
        $container.on('change.obsReference', '#obsReferenceSectionSelect', loadChapters);
        $container.on('change.obsReference', '#obsReferenceChapter', loadManualGrid);
        $container.on('click.obsReference', '#obsReferenceClear', function () {
            log('Selected reference cleared.');
            state.selected = null;
            renderSelected();
        });

        $container.data('obs-reference-initialized', true);
        $container.data('obs-reference-state', state);

        switchMode('initial-load');
        loadExistingByRefId($hiddenRef.val() || options.initialRefId);
        renderSelected();
    }

    function getSelectedObservationReference(containerSelector) {
        var $container = $(containerSelector || '#observationReferenceSection');
        var state = $container.data('obs-reference-state') || {};
        return state.selected || null;
    }

    window.initObservationReference = initObservationReference;
    window.getSelectedObservationReference = getSelectedObservationReference;
})(window, window.jQuery);
