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
        var $container = $(containerSelector || '#observationReferenceSection');
        if (!$container.length || $container.data('obs-reference-initialized')) {
            return;
        }

        options = options || {};
        var state = { selected: null };
        var apiBase = options.apiBaseUrl || (window.g_asiBaseURL ? window.g_asiBaseURL + '/ApiCalls' : '/ApiCalls');

        var $hiddenRef = $container.find('#observationReferenceId');
        var $type = $container.find('#obsReferenceType');
        var $search = $container.find('#obsReferenceSearchText');
        var $selected = $container.find('#obsReferenceSelected');
        var $manual = $container.find('#obsReferenceManual');
        var $section = $container.find('#obsReferenceSectionSelect');
        var $chapter = $container.find('#obsReferenceChapter');

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

        function switchMode() {
            var typeVal = ($type.val() || '').toUpperCase();
            $container.find('.obs-ref-mode').addClass('d-none');
            if (typeVal === 'CIRCULAR') {
                $container.find('#obsReferenceCircularMode').removeClass('d-none');
            } else if (typeVal === 'MANUAL' || typeVal === 'POLICY') {
                $container.find('#obsReferenceManualMode').removeClass('d-none');
            }
            clearResults();
        }

        function loadCircular() {
            $.get(apiBase + '/GetReferenceMasterDetail', {
                searchText: $search.val() || '',
                sourceType: 'CIRCULAR'
            }).done(function (rows) {
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
            });
        }

        function loadManualMaster() {
            $manual.empty().append('<option value="">--Select Manual--</option>');
            $.get(apiBase + '/GetObservationManualMaster').done(function (rows) {
                (rows || []).forEach(function (item) {
                    $manual.append('<option value="' + (item.manualId || item.MANUAL_ID || 0) + '">' + $('<div/>').text(item.displayLabel || item.DISPLAY_NAME || '').html() + '</option>');
                });
            });
        }

        function loadSections() {
            $section.empty().append('<option value="">--Select Section--</option>');
            $chapter.empty().append('<option value="">--Select Chapter--</option>');
            if (!$manual.val()) return;
            $.get(apiBase + '/GetObservationManualSections', { manualId: $manual.val() }).done(function (rows) {
                (rows || []).forEach(function (item) {
                    var sectionText = item.sectionText || item.SECTION_TEXT || item.sectionName || '';
                    $section.append('<option value="' + $('<div/>').text(sectionText).html() + '">' + $('<div/>').text(sectionText).html() + '</option>');
                });
            });
        }

        function loadChapters() {
            $chapter.empty().append('<option value="">--Select Chapter--</option>');
            if (!$manual.val() || !$section.val()) return;
            $.get(apiBase + '/GetObservationManualChapters', { manualId: $manual.val(), sectionText: $section.val() }).done(function (rows) {
                (rows || []).forEach(function (item) {
                    var chapterNo = item.chapterNo || item.CHAPTER_NO || '';
                    $chapter.append('<option value="' + $('<div/>').text(chapterNo).html() + '">' + $('<div/>').text(chapterNo).html() + '</option>');
                });
            });
        }

        function loadManualGrid() {
            var $tbody = $container.find('#obsReferenceManualGrid tbody');
            $tbody.empty();
            if (!$manual.val() || !$section.val() || !$chapter.val()) return;
            $.get(apiBase + '/GetObservationManualReferenceGrid', {
                manualId: $manual.val(),
                sectionText: $section.val(),
                chapterNo: $chapter.val(),
                sourceType: ($type.val() || '').toUpperCase()
            }).done(function (rows) {
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
            });
        }

        function loadExistingByRefId(refId) {
            if (!refId) {
                renderSelected();
                return;
            }
            $.get(apiBase + '/GetReferenceDetailByRefId', { refId: refId }).done(function (item) {
                if (item) {
                    selectReference(item);
                }
            });
        }

        $container.on('click', '.obs-ref-select', function () {
            var row = $(this).data('row');
            selectReference(row);
        });

        $container.on('click', '#obsReferenceSearchBtn', loadCircular);
        $container.on('change', '#obsReferenceType', switchMode);
        $container.on('change', '#obsReferenceManual', loadSections);
        $container.on('change', '#obsReferenceSectionSelect', loadChapters);
        $container.on('change', '#obsReferenceChapter', loadManualGrid);
        $container.on('click', '#obsReferenceClear', function () {
            state.selected = null;
            renderSelected();
        });

        $container.data('obs-reference-initialized', true);
        $container.data('obs-reference-state', state);

        loadManualMaster();
        switchMode();
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
