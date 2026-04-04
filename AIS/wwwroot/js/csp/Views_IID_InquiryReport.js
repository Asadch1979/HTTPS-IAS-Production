$(function(){
    console.log('Views_IID_InquiryReport.js loaded.');
    var pageRoot = document.getElementById('iidReportWorkspaceRoot');
    if(!pageRoot){ return; }

    function getComplaintId(){
        var queryComplaintId = parseInt(new URLSearchParams(window.location.search).get('complaintId'), 10);
        if(!isNaN(queryComplaintId) && queryComplaintId > 0){ return queryComplaintId; }

        var metaComplaintId = parseInt($('meta[name="iid-complaint-id"]').attr('content'), 10);
        if(!isNaN(metaComplaintId) && metaComplaintId > 0){ return metaComplaintId; }

        var hiddenComplaintId = parseInt($('#iidComplaintId').val(), 10);
        if(!isNaN(hiddenComplaintId) && hiddenComplaintId > 0){ return hiddenComplaintId; }

        return 0;
    }

    var complaintId = getComplaintId();
    var reportId = pageRoot ? (parseInt(pageRoot.getAttribute('data-report-id') || '0', 10) || 0) : 0;
    var reportMode = pageRoot ? String(pageRoot.getAttribute('data-report-mode') || 'edit').toLowerCase() : 'edit';
    var isReadOnlyMode = reportMode === 'analysis' || reportMode === 'final';
    var allowFinalAction = reportMode === 'edit' || reportMode === 'final';
    var finalActionLabel = reportMode === 'final' ? 'Finalize Report' : 'Submit for Analysis';
    var finalActionSuccessMessage = reportMode === 'final'
        ? 'Inquiry report finalized successfully.'
        : 'Inquiry report submitted for analysis successfully.';
    var currentStep = 1;
    var isLocked = false;
    var userId = 0;
    var alertTimer = null;
    var accusedRoleOptions = ['Main', 'Co', 'Witness'];
    var steps = [
        { id: 1, code: 'SNAPSHOT', title: 'Snapshot' },
        { id: 2, code: 'ACCUSATIONS', title: 'Accusations' },
        { id: 3, code: 'ACCUSED_LIST', title: 'Accused List' },
        { id: 4, code: 'RECORDS', title: 'Record Scrutinized' },
        { id: 5, code: 'STATEMENTS', title: 'Statement Register' },
        { id: 6, code: 'EVIDENCE', title: 'Evidence' },
        { id: 7, code: 'PROCEEDINGS', title: 'Inquiry Proceedings' },
        { id: 8, code: 'FINDINGS_RECOMM', title: 'Findings & Recommendations' },
        { id: 9, code: 'VIOLATIONS', title: 'Violations' },
        { id: 10, code: 'DSA', title: reportMode === 'final' ? 'DSA / Finalize Report' : (reportMode === 'analysis' ? 'DSA / Review' : 'DSA / Submit') }
    ];

    var state = {
        snapshot: {},
        complainantName: '',
        complainantCnic: '',
        accusations: [{ accusationId: 0, accusationText: '', sortOrder: 1 }],
        accusedEmployeeRows: [],
        accusedManualRows: [],
        accusedEmployeeDraft: { ppnoNumber: '', personName: '', fatherName: '', cnic: '', designation: '', roleType: 'Main' },
        accusedManualDraft: { personName: '', fatherName: '', cnic: '', designation: '', roleType: 'Main' },
        records: [{ recId: 0, recordTitle: '', recordDetails: '', sortOrder: 1 }],
        proceedings: [{ proceedingId: 0, noticeReference: '', visitDate: '', placeVisited: '', participantsDetail: '', missingParticipantsReason: '', sortOrder: 1 }],
        statementRegister: { rows: [] },
        evidence: { files: [], materialEvidenceDetail: '', circumstantialEvidenceDetail: '' },
        findingsRecomm: {
            selectedAccusationId: '',
            findingText: '',
            recommendationText: '',
            accusationOptions: [],
            statusRows: [],
            outcomes: {},
            lockedOutcomes: {},
            savedFindingsMap: {}
        },
        violations: [{ violationId: 0, category: 'Internal', violationDetail: '', referenceText: '', recommendation: '', sortOrder: 1 }],
        dsa: [{ dsaId: 0, personName: '', designation: '', ppnoNumber: '', cnic: '', dsaStatus: '', remarks: '', sortOrder: 1 }],
        deleteQueue: { accusations: [], accused: [], records: [], proceedings: [], violations: [], dsa: [] },
        savedSteps: { 1: true, 2: false, 3: false, 4: false, 5: false, 6: false, 7: false, 8: false, 9: false, 10: false },
        isDsaVisible: true,
        dirtySteps: {}
    };

    function shouldLockByStatus(status){
        var normalized = String(status || '').trim().toUpperCase();
        return normalized === 'QC_CLEARED' || normalized === 'REPORT_SUBMITTED' || normalized === 'CLOSED';
    }

    function statusBadgeText(status){
        var normalized = String(status || '').trim().toUpperCase();
        if(!normalized){
            if(reportMode === 'analysis'){ return 'Analysis Review'; }
            if(reportMode === 'final'){ return 'Pending Finalization'; }
            return 'Draft';
        }

        if(normalized === 'QC_CLEARED'){ return 'Ready for Analysis'; }
        if(normalized === 'REPORT_SUBMITTED'){ return 'Submitted'; }
        if(normalized === 'CLOSED'){ return 'Finalized'; }

        return normalized.replace(/_/g, ' ');
    }

    function applyStatusBadge(status){
        var badge = $('#reportStatusBadge');
        var normalized = String(status || '').trim().toUpperCase();
        badge.removeClass('bg-secondary bg-success bg-warning text-dark');

        if(normalized === 'CLOSED' || normalized === 'REPORT_SUBMITTED'){
            badge.addClass('bg-success');
        } else if(normalized === 'QC_CLEARED'){
            badge.addClass('bg-warning text-dark');
        } else {
            badge.addClass('bg-secondary');
        }

        badge.text(statusBadgeText(status));
    }

    function token(){ return $('#iidAntiForgeryWrap input[name="__RequestVerificationToken"]').val(); }
    function esc(v){ return $('<div/>').text(v || '').html(); }
    function buildUploadUrl(value){
        var fileValue = String(value || '').trim();
        if(!fileValue || fileValue.toUpperCase() === 'N/A'){ return ''; }
        if(typeof window.iidResolveFileUrl === 'function'){
            return window.iidResolveFileUrl(fileValue, (window.g_asiBaseURL || '') + '/Uploads');
        }
        if(/^(https?:)?\/\//i.test(fileValue) || fileValue.charAt(0) === '/'){
            return fileValue;
        }
        return (window.g_asiBaseURL || '').replace(/\/$/, '') + '/Uploads/' + encodeURIComponent(fileValue);
    }
    function extractData(resp){
        var payload = resp;
        if(payload && !Array.isArray(payload)){
            payload = payload.data || payload.Data || payload;
        }

        if(Array.isArray(payload)){ return payload; }
        if(payload && Array.isArray(payload.rows)){ return payload.rows; }
        if(payload && Array.isArray(payload.Rows)){ return payload.Rows; }
        if(payload && payload.data && Array.isArray(payload.data)){ return payload.data; }
        if(payload && payload.Data && Array.isArray(payload.Data)){ return payload.Data; }
        return [];
    }

    function getResponseMessage(resp){
        if(!resp){ return ''; }
        if(resp.message){ return resp.message; }
        if(resp.data && resp.data.message){ return resp.data.message; }
        return '';
    }

    function ensureApiSuccess(resp, fallback){
        if(resp && resp.ok === false){
            throw new Error(getResponseMessage(resp) || fallback || 'Operation failed.');
        }
        return resp;
    }

    function showAlert(message, type){
        clearTimeout(alertTimer);
        var safeMessage = (typeof sanitizeAlertMessageText === 'function')
            ? sanitizeAlertMessageText(message)
            : ((message || '').toString().trim());
        var finalMessage = safeMessage || 'Unexpected error.';
        var $alert = $('<div/>', {
            'class': 'alert alert-' + (type || 'danger') + ' alert-dismissible fade show text-prewrap',
            'role': 'alert'
        });
        $alert.text(finalMessage);
        $alert.append('<button type="button" class="btn-close" data-bs-dismiss="alert"></button>');
        $('#iidAlertHost').empty().append($alert);
        alertTimer = setTimeout(function(){ $('#iidAlertHost .alert').alert('close'); }, 4500);
    }

    function markDirty(step){
        if(step > 1){
            state.dirtySteps[step] = true;
            state.savedSteps[step] = false;
            if(step === currentStep){
                renderDirtyStepAlert();
            }
        }
    }

    function hasUnsavedStep(step){ return !!state.dirtySteps[step]; }

    function isStepSaved(step){
        if(step === 1){ return true; }
        return !!state.savedSteps[step] && !state.dirtySteps[step];
    }

    function getVisibleSteps(){
        return steps.filter(function(s){ return s.id !== 10 || state.isDsaVisible; });
    }

    function getStepPosition(stepId){
        var visible = getVisibleSteps();
        var idx = visible.findIndex(function(s){ return s.id === stepId; });
        return { index: idx, total: visible.length };
    }

    function getNextVisibleStep(stepId){
        var visible = getVisibleSteps();
        var idx = visible.findIndex(function(s){ return s.id === stepId; });
        if(idx < 0 || idx >= visible.length - 1){ return null; }
        return visible[idx + 1].id;
    }

    function getPreviousVisibleStep(stepId){
        var visible = getVisibleSteps();
        var idx = visible.findIndex(function(s){ return s.id === stepId; });
        if(idx <= 0){ return null; }
        return visible[idx - 1].id;
    }

    function getLastVisibleStepId(){
        var visible = getVisibleSteps();
        return visible.length ? visible[visible.length - 1].id : 10;
    }

    function renderStepper(){
        var pos = getStepPosition(currentStep);
        var stepNumber = pos.index >= 0 ? (pos.index + 1) : 1;
        $('#stepCounter').text('Step ' + stepNumber + ' of ' + pos.total);
        $('#wizardStepper').html(getVisibleSteps().map(function(s){
            var saved = isStepSaved(s.id);
            return '<button type="button" class="step-pill ' + (s.id===currentStep?'active':'') + ' ' + (saved?'completed':'not-saved') + '" data-step-jump="' + s.id + '"><span class="num">' + s.id + '</span>' + esc(s.title) + '<span class="step-state ms-2 badge ' + (saved?'bg-success':'bg-secondary') + '">' + (saved?'Saved':'Not Saved') + '</span></button>';
        }).join(''));
        renderDirtyStepAlert();
    }

    function renderDirtyStepAlert(){
        var host = $('#iidDirtyStepHost');
        if(!host.length){ return; }
        if(isReadOnlyMode){
            host.html('<div class="alert alert-light border mb-0 readonly-note" role="alert">This report is available in read-only mode.</div>');
            return;
        }
        if(hasUnsavedStep(currentStep)){
            host.html('<div class="alert alert-warning mb-0" role="alert">You have unsaved changes. Save before moving.</div>');
            return;
        }
        host.empty();
    }

    function renderSnapshotStrip(){
        var s = state.snapshot;
        var badges = [
            ['Complaint No', s.complaintNo || 'N/A'], ['Submitted', s.submittedOn || 'N/A'], ['Region', s.region || 'N/A'], ['Branch', s.branch || 'N/A'], ['Category', s.category || 'N/A']
        ];
        $('#snapshotStrip').html(badges.map(function(b){ return '<div class="col-md-2 col-6"><div class="snap-label">' + esc(b[0]) + '</div><div class="snap-value">' + esc(b[1]) + '</div></div>'; }).join(''));
    }

    function rowInput(bind, value, type){
        return '<input ' + (type ? 'type="' + type + '"' : '') + ' class="form-control" data-bind="' + bind + '" value="' + esc(value) + '">';
    }

    function rowTextarea(bind, value, rows){
        return '<textarea class="form-control" rows="' + (rows || 2) + '" data-bind="' + bind + '">' + esc(value) + '</textarea>';
    }

    function formatDateInputValue(value){
        if(!value){ return ''; }
        var raw = String(value).trim();
        if(!raw){ return ''; }
        if(/^\d{4}-\d{2}-\d{2}$/.test(raw)){ return raw; }
        if(/^\d{4}-\d{2}-\d{2}T/.test(raw)){ return raw.slice(0, 10); }
        var parsed = new Date(raw);
        if(isNaN(parsed.getTime())){ return ''; }
        return parsed.toISOString().slice(0, 10);
    }

    function normalizeDateOnly(value){
        var raw = formatDateInputValue(value);
        return raw || '';
    }

    function hasProceedingContent(row){
        row = row || {};
        return !!((row.noticeReference || row.visitDate || row.placeVisited || row.participantsDetail || row.missingParticipantsReason || '').toString().trim());
    }

    function sectionActions(step){
        var prevStep = getPreviousVisibleStep(step);
        var nextStep = getNextVisibleStep(step);
        var isLastVisibleStep = step === getLastVisibleStepId();
        if(isReadOnlyMode){
            var finalBtn = (isLastVisibleStep && reportMode === 'final')
                ? '<button type="button" class="btn btn-danger" id="finalActionBtn" data-final-action="finalize">Finalize Report</button>'
                : '';
            return '<div class="d-flex justify-content-between mt-4"><button type="button" class="btn btn-outline-secondary" data-prev ' + (!prevStep?'disabled':'') + '>Previous</button><div class="d-flex gap-2"><button type="button" class="btn btn-success" data-next ' + (!nextStep?'disabled':'') + '>' + (!nextStep?'Review':'Next') + '</button>' + finalBtn + '</div></div>';
        }
        var saveBtn = (step === 5 || step === 7) ? '' : '<button type="button" class="btn btn-primary" data-save>Mark Completed</button>';
        var finalSubmitBtn = (isLastVisibleStep && allowFinalAction)
            ? '<button type="button" class="btn btn-danger" id="finalActionBtn" data-final-action="submit">' + finalActionLabel + '</button>'
            : '';
        return '<div class="d-flex justify-content-between mt-4"><button type="button" class="btn btn-outline-secondary" data-prev ' + (!prevStep?'disabled':'') + '>Previous</button><div class="d-flex gap-2">' + saveBtn + '<button type="button" class="btn btn-success" data-next>' + (!nextStep?'Review':'Next') + '</button>' + finalSubmitBtn + '</div></div>';
    }

    function roleOptionsHtml(selected){
        return accusedRoleOptions.map(function(opt){ return '<option value="' + esc(opt) + '" ' + (opt===selected?'selected':'') + '>' + esc(opt) + '</option>'; }).join('');
    }

    function destroyTinyMce(){
        if(window.tinymce){
            var active = window.tinymce.get('findingTextHtml');
            if(active){ active.remove(); }
            active = window.tinymce.get('recommendationTextHtml');
            if(active){ active.remove(); }
        }
    }

    function initTinyMceEditors(){
        if(currentStep !== 8 || !window.tinymce){ return; }
        ['findingTextHtml','recommendationTextHtml'].forEach(function(editorId){
            if(window.tinymce.get(editorId)){ return; }
            window.tinymce.init({
                selector: '#' + editorId,
                menubar: false,
                height: 320,
                plugins: 'lists link table code',
                toolbar: isReadOnlyMode ? false : 'undo redo | blocks | bold italic underline | bullist numlist | alignleft aligncenter alignright | link table | code',
                license_key: 'gpl',
                readonly: isReadOnlyMode ? 1 : 0,
                statusbar: !isReadOnlyMode,
                setup: function(editor){
                    if(isReadOnlyMode){ return; }
                    editor.on('change keyup', function(){
                        if(editor.id === 'findingTextHtml'){
                            state.findingsRecomm.findingText = editor.getContent();
                        } else {
                            state.findingsRecomm.recommendationText = editor.getContent();
                        }
                        markDirty(8);
                        renderStepper();
                    });
                }
            });
        });
        applyFindingsEditorContent();
    }

    function applyFindingsEditorContent(findingText, recommendationText){
        var findingValue = typeof findingText === 'undefined' ? state.findingsRecomm.findingText : findingText;
        var recommendationValue = typeof recommendationText === 'undefined' ? state.findingsRecomm.recommendationText : recommendationText;

        state.findingsRecomm.findingText = findingValue || '';
        state.findingsRecomm.recommendationText = recommendationValue || '';

        var setEditors = function(){
            var findingEditor = window.tinymce && window.tinymce.get('findingTextHtml');
            if(findingEditor){ findingEditor.setContent(state.findingsRecomm.findingText || ''); }
            var recommendationEditor = window.tinymce && window.tinymce.get('recommendationTextHtml');
            if(recommendationEditor){ recommendationEditor.setContent(state.findingsRecomm.recommendationText || ''); }
        };

        if(!window.tinymce){
            $('#findingTextHtml').val(state.findingsRecomm.findingText || '');
            $('#recommendationTextHtml').val(state.findingsRecomm.recommendationText || '');
            return;
        }
        if(window.tinymce.get('findingTextHtml') || window.tinymce.get('recommendationTextHtml')){
            setEditors();
            return;
        }

        window.tinymce.once('AddEditor', function(){ setEditors(); });
    }

    function normalizeStatusValue(row){
        return String((row && (row.isSaved || row.status || row.savedStatus)) || '').toUpperCase();
    }

    function isStatusRowSaved(row){
        if(!row){ return false; }
        if(typeof row.isSaved === 'boolean'){ return row.isSaved; }
        var normalized = normalizeStatusValue(row);
        return normalized === 'Y' || normalized === 'YES' || normalized === 'SAVED' || normalized === 'TRUE' || normalized === '1';
    }

    function formatSavedOn(value){
        if(!value){ return '-'; }
        var d = new Date(value);
        if(isNaN(d.getTime())){ return value; }
        return d.toLocaleString();
    }

    function isComplainantStatementRole(value){
        return String(value || '').trim().toLowerCase().indexOf('complain') >= 0;
    }

    function accusationIdValue(value){
        return value === null || typeof value === 'undefined' ? '' : String(value);
    }

    function statementRoleLabel(row){
        return isComplainantStatementRole(row && row.roleType) ? 'Complainant' : 'Accused';
    }

    function buildFindingsAccusationList(baseRows){
        var map = {};
        (baseRows || []).forEach(function(row){
            var id = parseInt(row.accusationId || row.AccusationId || row.id || row.Id, 10);
            if(isNaN(id) || id <= 0){ return; }
            var text = row.accusationText || row.AccusationText || row.text || row.Text || ('Accusation #' + id);
            map[id] = { accusationId: id, accusationText: text };
        });

        return Object.keys(map).map(function(key){ return map[key]; }).sort(function(a, b){
            return a.accusationId - b.accusationId;
        });
    }

    function upsertFindingsStatusRow(row){
        if(!row){ return; }
        var id = parseInt(row.accusationId, 10);
        if(isNaN(id)){ return; }
        var idx = state.findingsRecomm.statusRows.findIndex(function(x){ return parseInt(x.accusationId, 10) === id; });
        if(idx >= 0){
            state.findingsRecomm.statusRows[idx] = $.extend({}, state.findingsRecomm.statusRows[idx], row);
        } else {
            state.findingsRecomm.statusRows.push(row);
        }
    }

    function syncFindingsStatusRows(statusRows){
        var merged = {};
        (statusRows || []).forEach(function(row){
            var id = parseInt(row.accusationId, 10);
            if(isNaN(id) || id <= 0){ return; }
            merged[id] = $.extend({}, merged[id] || { accusationId: id, accusationText: row.accusationText || ('Accusation #' + id) }, row);
        });
        state.findingsRecomm.statusRows = Object.keys(merged).map(function(key){ return merged[key]; }).sort(function(a, b){
            return a.accusationId - b.accusationId;
        });
    }

    function renderFindingsStatusGrid(){
        var rows = state.findingsRecomm.statusRows || [];
        var savedCount = rows.filter(isStatusRowSaved).length;
        $('#findingsStatusSummary').text('Saved: ' + savedCount + ' / Total: ' + rows.length);

        var outcomeOptionsHtml = function(selected){
            return '<option value="">Select Outcome</option>' +
                '<option value="Established" ' + (selected === 'Established' ? 'selected' : '') + '>Established</option>' +
                '<option value="Not Established" ' + (selected === 'Not Established' ? 'selected' : '') + '>Not Established</option>' +
                '<option value="Sub-judice" ' + (selected === 'Sub-judice' ? 'selected' : '') + '>Sub-judice</option>' +
                '<option value="Withdrawn Closed" ' + (selected === 'Withdrawn Closed' ? 'selected' : '') + '>Withdrawn Closed</option>' +
                '<option value="Transfer Closed" ' + (selected === 'Transfer Closed' ? 'selected' : '') + '>Transfer Closed</option>';
        };

        var body = rows.map(function(r){
            var saved = isStatusRowSaved(r);
            var id = parseInt(r.accusationId, 10);
            var accusationId = accusationIdValue(r.accusationId);
            var selectedOutcome = state.findingsRecomm.outcomes[id] || '';
            return '<tr>' +
                '<td>' + esc(r.accusationText || '') + '</td>' +
                '<td><span class="badge ' + (saved ? 'bg-success' : 'bg-warning text-dark') + '">' + (saved ? 'Saved' : 'Not Saved') + '</span></td>' +
                '<td><label class="form-label mb-1">Outcome</label><select class="form-select outcome-select" disabled data-accusation-id="' + esc(accusationId) + '">' + outcomeOptionsHtml(selectedOutcome) + '</select></td>' +
                '<td>' + esc(formatSavedOn(r.savedOn || r.lastSavedOn)) + '</td>' +
                '<td><button type="button" class="btn btn-outline-primary btn-sm" data-findings-view-edit="' + esc(accusationId) + '">View/Edit</button></td>' +
                '</tr>';
        }).join('');

        $('#findingsStatusGridBody').html(body || '<tr><td colspan="5" class="text-muted">No accusations available.</td></tr>');
    }

    function loadFindingsForSelection(selectedAccusationId, forceRefresh){
        var parsedId = parseInt(selectedAccusationId, 10);
        if(isNaN(parsedId) || parsedId <= 0){
            applyFindingsEditorContent('', '');
            $('#findingsOutcomeSelect').val('');
            return $.Deferred().resolve().promise();
        }

        var applySelection = function(savedData){
            var normalized = {
                findingsText: savedData && typeof savedData.findingsText !== 'undefined' ? savedData.findingsText : (savedData && typeof savedData.findingText !== 'undefined' ? savedData.findingText : ''),
                recommendationText: savedData && typeof savedData.recommendationText !== 'undefined' ? savedData.recommendationText : (savedData && typeof savedData.recomText !== 'undefined' ? savedData.recomText : ''),
                outcome: savedData ? (savedData.outcome || savedData.Outcome || '') : ''
            };

            state.findingsRecomm.savedFindingsMap[parsedId] = normalized;
            state.findingsRecomm.outcomes[parsedId] = normalized.outcome || state.findingsRecomm.outcomes[parsedId] || '';
            applyFindingsEditorContent(normalized.findingsText || '', normalized.recommendationText || '');

            var selectedOutcome = state.findingsRecomm.outcomes[parsedId] || '';
            $('#findingsOutcomeSelect').val(selectedOutcome);
            $('.outcome-select[data-accusation-id="' + accusationIdValue(parsedId) + '"]').val(selectedOutcome);
            updateDsaVisibility();
            renderStepper();
        };

        var savedData = (state.findingsRecomm.savedFindingsMap || {})[parsedId] || null;
        if(savedData && !forceRefresh){
            applySelection(savedData);
            return $.Deferred().resolve().promise();
        }

        return window.iidGetIidFindingsRecommByAccusation({ complaintId: complaintId, accusationId: parsedId }).then(function(resp){
            ensureApiSuccess(resp, 'Failed to load findings for selected accusation.');
            applySelection(resp || {});
        });
    }

    function loadFindingsStatusGrid(){
        return window.iidGetIidFindingsRecommStatus(complaintId).then(function(resp){
            ensureApiSuccess(resp, 'Failed to load findings status grid.');
            var rows = extractData(resp).filter(isStatusRowSaved);
            rows.forEach(function(row){
                var id = parseInt(row.accusationId, 10);
                if(!isNaN(id) && id > 0){
                    var rowOutcome = row.outcome || row.Outcome || state.findingsRecomm.outcomes[id] || "";
                    state.findingsRecomm.outcomes[id] = rowOutcome;
                    state.findingsRecomm.lockedOutcomes[id] = isStatusRowSaved(row);
                }
            });
            syncFindingsStatusRows(rows);
            state.savedSteps[8] = state.findingsRecomm.statusRows.some(isStatusRowSaved);
            renderFindingsStatusGrid();
            updateDsaVisibility();
        });
    }

    function hasEstablishedOutcome(){
        var values = Object.keys(state.findingsRecomm.outcomes || {}).map(function(key){ return state.findingsRecomm.outcomes[key]; });
        return values.some(function(val){ return String(val || '').toLowerCase() === 'established'; });
    }

    function updateDsaVisibility(){
        var shouldShowDsa = hasEstablishedOutcome();
        state.isDsaVisible = shouldShowDsa;
        if(!shouldShowDsa){
            state.savedSteps[10] = true;
            state.dirtySteps[10] = false;
            if(currentStep === 10){ currentStep = 8; }
        }
        renderStepper();
    }

    function loadFindingsModule(){
        return $.when(window.iidGetIidAccusationsForFindings(complaintId), window.iidGetInqFindingsRecomm(complaintId)).then(function(accResp, findingsResp){
            var accusationsResponse = accResp && accResp[0] ? accResp[0] : accResp;
            var findingsResponse = findingsResp && findingsResp[0] ? findingsResp[0] : findingsResp;
            ensureApiSuccess(accusationsResponse, 'Failed to load accusations for findings.');
            ensureApiSuccess(findingsResponse, 'Failed to load saved findings and recommendations.');

            state.findingsRecomm.accusationOptions = buildFindingsAccusationList(extractData(accusationsResponse));
            state.findingsRecomm.savedFindingsMap = {};
            extractData(findingsResponse).forEach(function(row){
                var id = parseInt(row.accusationId, 10);
                if(isNaN(id) || id <= 0){ return; }
                state.findingsRecomm.savedFindingsMap[id] = {
                    findingsText: row.findingsText || row.findingText || '',
                    recommendationText: row.recommendationText || row.recomText || '',
                    outcome: row.outcome || row.Outcome || ''
                };
                if(state.findingsRecomm.savedFindingsMap[id].outcome){
                    state.findingsRecomm.outcomes[id] = state.findingsRecomm.savedFindingsMap[id].outcome;
                }
            });

            var selectedId = accusationIdValue(state.findingsRecomm.selectedAccusationId);
            var exists = (state.findingsRecomm.accusationOptions || []).some(function(opt){ return String(opt.accusationId) === selectedId; });
            if(!selectedId || !exists){
                state.findingsRecomm.selectedAccusationId = '';
            }
            syncFindingsStatusRows([]);
            return loadFindingsStatusGrid().then(function(){
                if(state.findingsRecomm.selectedAccusationId !== '' && state.findingsRecomm.selectedAccusationId !== null && typeof state.findingsRecomm.selectedAccusationId !== 'undefined'){
                    return loadFindingsForSelection(state.findingsRecomm.selectedAccusationId);
                }
                applyFindingsEditorContent('', '');
                $('#findingsOutcomeSelect').val('');
            });
        });
    }

    function renderSnapshotUploads(snapshot){
        var uploadBase = (window.g_asiBaseURL || '') + '/Uploads';
        return '<div class="card border mt-3"><div class="card-body"><h6 class="mb-3">Uploaded Documents</h6><dl class="row mb-0 small">' +
            '<dt class="col-md-4">Uploaded Complaint</dt><dd class="col-md-8">' + (window.iidFileCell ? window.iidFileCell(snapshot.uploadedComplaint || snapshot.UploadedComplaint, uploadBase) : 'N/A') + '</dd>' +
            '<dt class="col-md-4">Uploaded FFR</dt><dd class="col-md-8">' + (window.iidFileCell ? window.iidFileCell(snapshot.uploadedFFR || snapshot.UploadedFFR, uploadBase) : 'N/A') + '</dd>' +
            '<dt class="col-md-4">Uploaded Evidence</dt><dd class="col-md-8">' + (window.iidFileCell ? window.iidFileCell(snapshot.uploadedEvidence || snapshot.UploadedEvidence, uploadBase) : 'N/A') + '</dd>' +
            '</dl></div></div>';
    }

    function renderSection(step){
        var h = $('.wizard-section[data-step="' + step + '"]');
        var html = '<div class="card-body"><div class="validation-summary alert alert-warning d-none"></div>';
        if(step === 1){
            var snap = state.snapshot || {};
            html += '<h5>Snapshot</h5><p class="text-muted">Complaint and complainant data loaded.</p>';
            html += '<div class="row g-3"><div class="col-lg-6"><div class="card border-0 bg-light"><div class="card-body"><h6 class="mb-3">Complainant Details</h6><div class="row g-2 small"><div class="col-6"><span class="text-muted">Name</span><div class="fw-semibold">' + esc(snap.complainantName || 'N/A') + '</div></div><div class="col-6"><span class="text-muted">CNIC</span><div class="fw-semibold">' + esc(snap.cnic || 'N/A') + '</div></div><div class="col-6"><span class="text-muted">Cell No</span><div>' + esc(snap.cellularNumber || 'N/A') + '</div></div><div class="col-6"><span class="text-muted">Gender</span><div>' + esc(snap.gender || 'N/A') + '</div></div><div class="col-12"><span class="text-muted">Address</span><div>' + esc(snap.mailingAddress || 'N/A') + '</div></div><div class="col-12"><span class="text-muted">Received From</span><div>' + esc(snap.receivedFrom || 'N/A') + '</div></div></div></div></div></div><div class="col-lg-6"><div class="card border-0 bg-light"><div class="card-body"><h6 class="mb-3">Complaint Details</h6><div class="row g-2 small"><div class="col-6"><span class="text-muted">Complaint No</span><div class="fw-semibold">' + esc(snap.complaintNo || 'N/A') + '</div></div><div class="col-6"><span class="text-muted">Submitted On</span><div>' + esc(snap.submittedOn || 'N/A') + '</div></div><div class="col-6"><span class="text-muted">Nature</span><div>' + esc(snap.nature || 'N/A') + '</div></div><div class="col-6"><span class="text-muted">Category</span><div>' + esc(snap.category || 'N/A') + '</div></div><div class="col-6"><span class="text-muted">Region</span><div>' + esc(snap.region || 'N/A') + '</div></div><div class="col-6"><span class="text-muted">Branch</span><div>' + esc(snap.branch || 'N/A') + '</div></div><div class="col-12"><span class="text-muted">Action Required</span><div>' + esc(snap.actionRequired || 'N/A') + '</div></div><div class="col-12"><span class="text-muted">Contents</span><div>' + esc(snap.contents || 'N/A') + '</div></div></div></div></div></div></div>' + renderSnapshotUploads(snap);
        }
        if(step === 2){
            var rowsA = state.accusations.map(function(r,i){ return '<tr data-row-id="' + (r.accusationId || 0) + '"><td>' + rowInput('accusations['+i+'].accusationText', r.accusationText) + '</td><td><button type="button" class="btn btn-outline-danger btn-sm" data-remove-row="accusations" data-index="'+i+'">Remove</button></td></tr>'; }).join('');
            html += '<h5>Accusations</h5><table class="table table-sm"><thead><tr><th>Accusation</th><th></th></tr></thead><tbody>' + rowsA + '</tbody></table><button type="button" class="btn btn-outline-primary btn-sm" data-add-row="accusations">Add Row</button>';
        }
        if(step === 3){
            var rowsEmp = state.accusedEmployeeRows.map(function(r){
                return '<tr data-row-id="' + (r.accusedRowId || 0) + '"><td>' + esc(r.ppnoNumber) + '</td><td>' + esc(r.personName) + '</td><td>' + esc(r.fatherName) + '</td><td>' + esc(r.cnic) + '</td><td>' + esc(r.designation) + '</td><td>' + esc(r.roleType) + '</td><td><button type="button" class="btn btn-outline-danger btn-sm" data-remove-accused="employee" data-id="' + (r.accusedRowId || 0) + '">Remove</button></td></tr>';
            }).join('');
            var rowsManual = state.accusedManualRows.map(function(r){
                return '<tr data-row-id="' + (r.accusedRowId || 0) + '"><td>' + esc(r.personName) + '</td><td>' + esc(r.fatherName) + '</td><td>' + esc(r.cnic) + '</td><td>' + esc(r.designation) + '</td><td>' + esc(r.roleType) + '</td><td><button type="button" class="btn btn-outline-danger btn-sm" data-remove-accused="manual" data-id="' + (r.accusedRowId || 0) + '">Remove</button></td></tr>';
            }).join('');

            html += '<h5>Accused List</h5>' +
                '<div class="card border-0 bg-light mb-3"><div class="card-body">' +
                    '<h6 class="mb-3">Employee Accused (PPNO based)</h6>' +
                    '<div class="row g-2 align-items-end">' +
                        '<div class="col-md-2"><label class="form-label">PPNO</label><input type="text" class="form-control" maxlength="20" data-step3-field="employee.ppnoNumber" value="' + esc(state.accusedEmployeeDraft.ppnoNumber) + '"></div>' +
                        '<div class="col-md-2"><button type="button" class="btn btn-outline-primary w-100" data-step3-search>Search</button></div>' +
                        '<div class="col-md-2"><label class="form-label">Name</label><input type="text" class="form-control" data-step3-field="employee.personName" value="' + esc(state.accusedEmployeeDraft.personName) + '"></div>' +
                        '<div class="col-md-2"><label class="form-label">Father Name</label><input type="text" class="form-control" data-step3-field="employee.fatherName" value="' + esc(state.accusedEmployeeDraft.fatherName) + '"></div>' +
                        '<div class="col-md-2"><label class="form-label">CNIC</label><input type="text" class="form-control" maxlength="13" data-step3-field="employee.cnic" value="' + esc(state.accusedEmployeeDraft.cnic) + '"></div>' +
                        '<div class="col-md-1"><label class="form-label">Role</label><select class="form-select" data-step3-field="employee.roleType">' + roleOptionsHtml(state.accusedEmployeeDraft.roleType || 'Main') + '</select></div>' +
                        '<div class="col-md-1"><label class="form-label">Designation</label><input type="text" class="form-control" data-step3-field="employee.designation" value="' + esc(state.accusedEmployeeDraft.designation) + '"></div>' +
                        '<div class="col-md-2"><button type="button" class="btn btn-primary w-100" data-step3-save="employee">Save Employee Accused</button></div>' +
                    '</div>' +
                    '<div class="table-responsive mt-3"><table class="table table-sm"><thead><tr><th>PPNO</th><th>Name</th><th>Father Name</th><th>CNIC</th><th>Designation</th><th>Role</th><th>Action</th></tr></thead><tbody>' + (rowsEmp || '<tr><td colspan="7" class="text-muted">No employee accused rows saved.</td></tr>') + '</tbody></table></div>' +
                '</div></div>' +
                '<div class="card border-0 bg-light"><div class="card-body">' +
                    '<h6 class="mb-3">Non-Employee / Manual Accused</h6>' +
                    '<div class="row g-2 align-items-end">' +
                        '<div class="col-md-3"><label class="form-label">Name</label><input type="text" class="form-control" data-step3-field="manual.personName" value="' + esc(state.accusedManualDraft.personName) + '"></div>' +
                        '<div class="col-md-3"><label class="form-label">Father Name</label><input type="text" class="form-control" data-step3-field="manual.fatherName" value="' + esc(state.accusedManualDraft.fatherName) + '"></div>' +
                        '<div class="col-md-2"><label class="form-label">CNIC</label><input type="text" class="form-control" maxlength="13" data-step3-field="manual.cnic" value="' + esc(state.accusedManualDraft.cnic) + '"></div>' +
                        '<div class="col-md-2"><label class="form-label">Designation</label><input type="text" class="form-control" data-step3-field="manual.designation" value="' + esc(state.accusedManualDraft.designation) + '"></div>' +
                        '<div class="col-md-2"><label class="form-label">Role</label><select class="form-select" data-step3-field="manual.roleType">' + roleOptionsHtml(state.accusedManualDraft.roleType || 'Main') + '</select></div>' +
                        '<div class="col-md-2"><button type="button" class="btn btn-primary w-100" data-step3-save="manual">Save Manual Accused</button></div>' +
                    '</div>' +
                    '<div class="table-responsive mt-3"><table class="table table-sm"><thead><tr><th>Name</th><th>Father Name</th><th>CNIC</th><th>Designation</th><th>Role</th><th>Action</th></tr></thead><tbody>' + (rowsManual || '<tr><td colspan="6" class="text-muted">No manual accused rows saved.</td></tr>') + '</tbody></table></div>' +
                '</div></div>';
        }
        if(step === 4){
            var rowsR = state.records.map(function(r,i){ return '<tr data-row-id="' + (r.recId || 0) + '"><td>'+rowInput('records['+i+'].recordTitle',r.recordTitle)+'</td><td>'+rowInput('records['+i+'].recordDetails',r.recordDetails)+'</td><td><button type="button" class="btn btn-outline-danger btn-sm" data-remove-row="records" data-index="'+i+'">Remove</button></td></tr>'; }).join('');
            html += '<h5>Record Scrutinized</h5><table class="table table-sm"><thead><tr><th>Record Title</th><th>Details</th><th></th></tr></thead><tbody>'+rowsR+'</tbody></table><button type="button" class="btn btn-outline-primary btn-sm" data-add-row="records">Add Row</button>';
        }
        if(step === 5){
            var statementRows = (state.statementRegister.rows || []).map(function(r, i){
                var statementFileUrl = buildUploadUrl(r.uploadedStatement || r.uploadedStatementUrl || '');
                var fileCell = statementFileUrl
                    ? ('<a href="' + esc(statementFileUrl) + '" target="_blank" rel="noopener">View</a>')
                    : '<span class="text-muted">N/A</span>';
                return '<tr>' +
                    '<td><span class="badge bg-info-subtle text-dark">' + esc(statementRoleLabel(r)) + '</span></td>' +
                    '<td>' + esc(r.ppnoNumber || 'N/A') + '</td>' +
                    '<td>' + esc(r.personName || '') + '</td>' +
                    '<td>' + esc(r.fatherName || '') + '</td>' +
                    '<td>' + esc(r.cnic || '') + '</td>' +
                    '<td><input type="date" class="form-control" data-step5-field="'+ i +'" data-step5-key="statementDatetime" value="' + esc(formatDateInputValue(r.statementDatetime || '')) + '"></td>' +
                    '<td><input type="text" class="form-control" data-step5-field="'+ i +'" data-step5-key="place" value="' + esc(r.place || '') + '"></td>' +
                    '<td><input type="text" class="form-control" data-step5-field="'+ i +'" data-step5-key="modeType" value="' + esc(r.modeType || '') + '"></td>' +
                    '<td><textarea class="form-control" rows="2" data-step5-field="'+ i +'" data-step5-key="keyPoints">' + esc(r.keyPoints || '') + '</textarea></td>' +
                    '<td><div class="input-group">' +
                        '<input type="text" class="form-control" readonly placeholder="No file uploaded" value="' + esc(r.uploadedStatement || '') + '">' +
                        '<button type="button" class="btn btn-outline-primary" data-step5-upload-row="'+ i +'">Upload</button>' +
                    '</div><input type="file" class="d-none" data-step5-upload-input="'+ i +'"></td>' +
                    '<td>' + fileCell + '</td>' +
                    '<td><button type="button" class="btn btn-primary btn-sm" data-step5-save-row="'+ i +'">' + (parseInt(r.statementId || 0, 10) > 0 ? 'Update' : 'Save') + '</button></td>' +
                '</tr>';
            }).join('');

            html += '<h5>Statement Register</h5>' +
                '<div class="card border mb-3"><div class="card-body">' +
                    '<h6 class="mb-3">Statement of Complainant & Accused</h6>' +
                    '<div class="table-responsive"><table class="table table-sm align-middle"><thead><tr><th>Type</th><th>PPNO</th><th>Name</th><th>Father Name</th><th>CNIC</th><th>Date</th><th>Place of Statement</th><th>Mode Type</th><th>Key Points</th><th>Uploaded Statement</th><th>File</th><th>Action</th></tr></thead><tbody>' + (statementRows || '<tr><td colspan="12" class="text-muted">No statement rows available.</td></tr>') + '</tbody></table></div>' +
                '</div></div>';
        }
        if(step === 6){
            var rowsE = (state.evidence.files || []).map(function(r){
                var evidenceFileUrl = buildUploadUrl(r.filePath || r.fileName || '');
                var evidenceFileCell = evidenceFileUrl
                    ? '<a href="' + esc(evidenceFileUrl) + '" target="_blank" rel="noopener">View</a>'
                    : '<span class="text-muted">N/A</span>';
                return '<tr><td>'+esc(r.fileName)+'</td><td>'+esc(r.evidenceType || 'N/A')+'</td><td>'+esc(r.uploadedOn || 'N/A')+'</td><td>'+evidenceFileCell+'</td><td><button type="button" class="btn btn-outline-danger btn-sm" data-delete-evidence="'+ r.evidenceId +'">Remove</button></td></tr>';
            }).join('');
            html += '<h5>Evidence</h5>' +
                '<div class="card border-0 bg-light mb-3"><div class="card-body">' +
                    '<div class="row g-3">' +
                        '<div class="col-md-6"><label class="form-label fw-semibold">Detail of Material evidences</label><textarea class="form-control" rows="5" data-step6-key="materialEvidenceDetail">' + esc(state.evidence.materialEvidenceDetail || '') + '</textarea></div>' +
                        '<div class="col-md-6"><label class="form-label fw-semibold">Details of circumstantial evidences</label><textarea class="form-control" rows="5" data-step6-key="circumstantialEvidenceDetail">' + esc(state.evidence.circumstantialEvidenceDetail || '') + '</textarea></div>' +
                    '</div>' +
                '</div></div>' +
                '<div class="mb-3"><label class="form-label">Upload evidence files</label><input id="uploadedEvidence" type="file" class="form-control" multiple></div>' +
                '<table class="table table-sm"><thead><tr><th>File</th><th>Type</th><th>Uploaded On</th><th>View</th><th></th></tr></thead><tbody>' + (rowsE || '<tr><td colspan="5" class="text-muted">No evidence uploaded.</td></tr>') + '</tbody></table>';
        }
        if(step === 7){
            var proceedingRows = state.proceedings.map(function(r, i){
                return '<tr data-row-id="' + (r.proceedingId || 0) + '">' +
                    '<td>' + rowInput('proceedings['+i+'].noticeReference', r.noticeReference || '') + '</td>' +
                    '<td>' + rowInput('proceedings['+i+'].visitDate', formatDateInputValue(r.visitDate || ''), 'date') + '</td>' +
                    '<td>' + rowTextarea('proceedings['+i+'].placeVisited', r.placeVisited || '', 2) + '</td>' +
                    '<td>' + rowTextarea('proceedings['+i+'].participantsDetail', r.participantsDetail || '', 2) + '</td>' +
                    '<td>' + rowTextarea('proceedings['+i+'].missingParticipantsReason', r.missingParticipantsReason || '', 2) + '</td>' +
                    '<td class="text-nowrap">' +
                        '<button type="button" class="btn btn-primary btn-sm me-1" data-step7-save-row="'+ i +'">' + (parseInt(r.proceedingId || 0, 10) > 0 ? 'Update' : 'Save') + '</button>' +
                        '<button type="button" class="btn btn-outline-danger btn-sm" data-step7-delete-row="'+ i +'">Delete</button>' +
                    '</td>' +
                '</tr>';
            }).join('');

            html += '<h5>Inquiry Proceedings</h5>' +
                '<div class="card border mb-3"><div class="card-body">' +
                    '<div class="table-responsive"><table class="table table-sm align-middle"><thead><tr><th>Reference of Notices issued for inquiry schedule</th><th>Date of visit to place of incident</th><th>Places visited</th><th>Detail of participants</th><th>Detail of missing participants with reasons</th><th></th></tr></thead><tbody>' + proceedingRows + '</tbody></table></div>' +
                    '<button type="button" class="btn btn-outline-primary btn-sm" data-add-row="proceedings">Add More</button>' +
                '</div></div>';
        }
        if(step === 8){
            var accusationOptions = (state.findingsRecomm.accusationOptions || []).map(function(opt){
                var id = String(opt.accusationId);
                var selected = accusationIdValue(state.findingsRecomm.selectedAccusationId) === id ? ' selected' : '';
                return '<option value="' + esc(id) + '"' + selected + '>' + esc(opt.accusationText || '') + '</option>';
            }).join('');

            html += '<h5>Findings & Recommendations</h5>' +
                '<div class="card border-0 bg-light mb-3"><div class="card-body">' +
                    '<div class="row g-3 mb-3">' +
                        '<div class="col-md-6">' +
                            '<label class="form-label fw-semibold">Accusation <span class="text-danger">*</span></label>' +
                            '<select id="findingsAccusationSelect" class="form-select" data-findings-accusation-select>' +
                                '<option value="">Select Accusation</option>' + accusationOptions +
                            '</select>' +
                        '</div>' +
                    '</div>' +
                    '<label class="form-label fw-semibold">Findings <span class="text-danger">*</span></label>' +
                    '<textarea id="findingTextHtml" class="form-control" rows="12">' + esc(state.findingsRecomm.findingText || '') + '</textarea>' +
                '</div></div>' +
                '<div class="card border-0 bg-light mb-3">' +
                    '<div class="card-body">' +
                        '<label class="form-label fw-semibold">Recommendations <span class="text-danger">*</span></label>' +
                        '<textarea id="recommendationTextHtml" class="form-control" rows="12">' + esc(state.findingsRecomm.recommendationText || '') + '</textarea>' +
                        '<div class="mt-3"><label class="form-label fw-semibold">Complaint Outcome <span class="text-danger">*</span></label><select id="findingsOutcomeSelect" class="form-select" data-findings-outcome><option value="">Select Outcome</option><option value="Established">Established</option><option value="Not Established">Not Established</option><option value="Sub-judice">Sub-judice</option><option value="Withdrawn Closed">Withdrawn Closed</option><option value="Transfer Closed">Transfer Closed</option></select><div class="form-text">Only this (upper) outcome is editable. Grid outcome mirrors this value.</div></div>' +
                        '<div class="mt-3"><button type="button" class="btn btn-primary" data-save-findings-recomm>Save Findings & Recommendations</button></div>' +
                    '</div>' +
                '</div>' +
                '<div class="card border">' +
                    '<div class="card-body">' +
                        '<div class="d-flex justify-content-between align-items-center mb-2">' +
                            '<h6 class="mb-0">Saved Status</h6>' +
                            '<span id="findingsStatusSummary" class="fw-semibold">Saved: 0 / Total: 0</span>' +
                        '</div>' +
                        '<div class="table-responsive">' +
                            '<table class="table table-sm align-middle">' +
                                '<thead><tr><th>Accusation Title</th><th>Status</th><th>Outcome</th><th>Last Saved On</th><th>Action</th></tr></thead>' +
                                '<tbody id="findingsStatusGridBody"><tr><td colspan="5" class="text-muted">No accusations available.</td></tr></tbody>' +
                            '</table>' +
                        '</div>' +
                    '</div>' +
                '</div>';
        }
        if(step === 9){
            var rowsV = state.violations.map(function(r,i){ return '<tr data-row-id="' + (r.violationId || 0) + '"><td>'+rowInput('violations['+i+'].category',r.category)+'</td><td>'+rowInput('violations['+i+'].violationDetail',r.violationDetail)+'</td><td>'+rowInput('violations['+i+'].referenceText',r.referenceText)+'</td><td>'+rowInput('violations['+i+'].recommendation',r.recommendation)+'</td><td><button type="button" class="btn btn-outline-danger btn-sm" data-remove-row="violations" data-index="'+i+'">Remove</button></td></tr>'; }).join('');
            html += '<h5>Violations (Annex-III)</h5><table class="table table-sm"><thead><tr><th>Category</th><th>Detail</th><th>Reference</th><th>Recommendation</th><th></th></tr></thead><tbody>'+rowsV+'</tbody></table><button type="button" class="btn btn-outline-primary btn-sm" data-add-row="violations">Add Row</button>';
        }
        if(step === 10){
            var rowsD = state.dsa.map(function(r,i){ return '<tr data-row-id="' + (r.dsaId || 0) + '"><td>'+rowInput('dsa['+i+'].personName',r.personName)+'</td><td>'+rowInput('dsa['+i+'].designation',r.designation)+'</td><td>'+rowInput('dsa['+i+'].ppnoNumber',r.ppnoNumber)+'</td><td>'+rowInput('dsa['+i+'].cnic',r.cnic)+'</td><td>'+rowInput('dsa['+i+'].dsaStatus',r.dsaStatus)+'</td><td><button type="button" class="btn btn-outline-danger btn-sm" data-remove-row="dsa" data-index="'+i+'">Remove</button></td></tr>'; }).join('');
            html += '<h5>DSA</h5><table class="table table-sm"><thead><tr><th>Person</th><th>Designation</th><th>PPNO</th><th>CNIC</th><th>Status</th><th></th></tr></thead><tbody>'+rowsD+'</tbody></table><button type="button" class="btn btn-outline-primary btn-sm" data-add-row="dsa">Add Row</button>';
        }
        html += sectionActions(step) + '</div>';
        destroyTinyMce();
        h.html(html);
        if(isLocked){
            h.find('input,textarea,select').prop('disabled', true);
            h.find('button').not('[data-prev],[data-next],[data-final-action="finalize"]').prop('disabled', true);
        }
        if(isReadOnlyMode){
            h.find('[data-save],[data-add-row],[data-remove-row],[data-step3-search],[data-step3-save],[data-remove-accused],[data-step5-upload-row],[data-step5-save-row],[data-step7-save-row],[data-step7-delete-row],[data-delete-evidence],[data-save-findings-recomm],[data-findings-view-edit]').addClass('d-none');
            h.find('[data-step5-upload-input]').prop('disabled', true);
        }
        initTinyMceEditors();
        if(step === 8){ renderFindingsStatusGrid(); }
    }

    function bindStepInputs(){
        if(isReadOnlyMode){ return; }
        var host = $('.wizard-section[data-step="' + currentStep + '"]');
        host.find('[data-bind]').off('input change').on('input change', function(){
            var key = $(this).data('bind');
            var m = key.match(/^(\w+)\[(\d+)\]\.(\w+)$/);
            if(!m){ return; }
            var sectionName = m[1];
            var rowIndex = parseInt(m[2],10);
            var fieldName = m[3];
            var value = $(this).val();
            if(sectionName === 'proceedings' && fieldName === 'visitDate'){
                value = normalizeDateOnly(value);
                $(this).val(value);
            }
            state[sectionName][rowIndex][fieldName] = value;
            markDirty(currentStep);
            renderStepper();
        });

        host.find('[data-step3-field]').off('input change keypress paste').on('keypress', function(e){
            var key = String($(this).data('step3-field') || '');
            if(key.indexOf('.cnic') < 0 && key.indexOf('.ppnoNumber') < 0){ return; }
            if(e.which && !/\d/.test(String.fromCharCode(e.which))){
                e.preventDefault();
            }
        }).on('input change', function(){
            var key = String($(this).data('step3-field') || '');
            var parts = key.split('.');
            if(parts.length !== 2){ return; }
            var group = parts[0];
            var field = parts[1];
            var draft = group === 'employee' ? state.accusedEmployeeDraft : state.accusedManualDraft;
            var val = $(this).val();
            if(field === 'cnic' || field === 'ppnoNumber'){ val = digitsOnly(val); }
            if(field === 'cnic'){ val = val.substring(0, 13); $(this).val(val); }
            if(field === 'ppnoNumber'){ $(this).val(val); }
            draft[field] = val;
        }).on('paste', function(e){
            var key = String($(this).data('step3-field') || '');
            if(key.indexOf('.cnic') < 0 && key.indexOf('.ppnoNumber') < 0){ return; }
            var paste = (e.originalEvent || e).clipboardData.getData('text') || '';
            if(/\D/.test(paste)){
                e.preventDefault();
            }
        });

        host.find('[data-step5-field]').off('input change keypress paste').on('keypress', function(e){
            var key = String($(this).data('step5-key') || '');
            if(key !== 'cnic'){ return; }
            if(e.which && !/\d/.test(String.fromCharCode(e.which))){
                e.preventDefault();
            }
        }).on('paste', function(e){
            var key = String($(this).data('step5-key') || '');
            if(key !== 'cnic'){ return; }
            var paste = (e.originalEvent || e).clipboardData.getData('text') || '';
            if(/\D/.test(paste)){
                e.preventDefault();
            }
        }).on('input change', function(){
            var rowIndex = parseInt($(this).data('step5-field'), 10);
            var fieldKey = String($(this).data('step5-key') || '');
            if(isNaN(rowIndex) || !fieldKey || !state.statementRegister.rows[rowIndex]){ return; }
            var val = $(this).val();
            if(fieldKey === 'cnic'){ val = digitsOnly(val).substring(0, 13); $(this).val(val); }
            state.statementRegister.rows[rowIndex][fieldKey] = val;
            markDirty(5);
            renderStepper();
        });

        host.find('[data-step6-key]').off('input change').on('input change', function(){
            var fieldKey = String($(this).data('step6-key') || '');
            if(!fieldKey){ return; }
            state.evidence[fieldKey] = $(this).val();
            markDirty(6);
            renderStepper();
        });
    }

    function validateStep(step){
        var errs = [];
        var has = function(v){ return !!(v && String(v).trim()); };
        if(step === 2 && !state.accusations.some(function(x){ return has(x.accusationText); })) errs.push('At least one accusation is required.');
        if(step === 3 && !state.accusedEmployeeRows.length && !state.accusedManualRows.length) errs.push('At least one accused row is required.');
        if(step === 4 && !state.records.some(function(x){ return has(x.recordTitle) || has(x.recordDetails); })) errs.push('At least one record scrutinized row is required.');
        if(step === 5 && !(state.statementRegister.rows || []).some(function(x){ return parseInt(x.statementId || 0, 10) > 0; })) errs.push('At least one saved statement is required.');
        if(step === 6 && !(state.evidence.files || []).length) errs.push('At least one evidence file is mandatory.');
        if(step === 7){
            var proceedingRows = (state.proceedings || []).filter(hasProceedingContent);
            if(!proceedingRows.length){
                errs.push('At least one inquiry proceeding row is required.');
            } else {
                proceedingRows.forEach(function(row, idx){
                    if(!has(row.noticeReference) || !has(row.visitDate) || !has(row.placeVisited) || !has(row.participantsDetail) || !has(row.missingParticipantsReason)){
                        errs.push('All inquiry proceeding fields are required for row ' + (idx + 1) + '.');
                    }
                });
            }
        }
        if(step === 8){
            if(window.tinymce && window.tinymce.get('findingTextHtml')){ state.findingsRecomm.findingText = window.tinymce.get('findingTextHtml').getContent(); }
            if(window.tinymce && window.tinymce.get('recommendationTextHtml')){ state.findingsRecomm.recommendationText = window.tinymce.get('recommendationTextHtml').getContent(); }
            if(state.findingsRecomm.selectedAccusationId === '' || state.findingsRecomm.selectedAccusationId === null || typeof state.findingsRecomm.selectedAccusationId === 'undefined') errs.push('Accusation selection is required.');
            if(!has(state.findingsRecomm.findingText)) errs.push('Findings text is required.');
            if(!has(state.findingsRecomm.recommendationText)) errs.push('Recommendation text is required.');
            var selectedOutcome = state.findingsRecomm.outcomes[parseInt(state.findingsRecomm.selectedAccusationId, 10)] || '';
            if(!has(selectedOutcome)) errs.push('Outcome is required for selected accusation.');
        }
        if(step === 9 && !state.violations.some(function(x){ return has(x.category) && has(x.violationDetail) && has(x.referenceText) && has(x.recommendation); })) errs.push('At least one complete violation row is required.');
        if(step === 10 && state.isDsaVisible && !state.dsa.some(function(x){ return has(x.personName) || has(x.dsaStatus); })) errs.push('At least one DSA row is required.');
        return errs;
    }

    function validateStepForFinalSubmit(step){
        var missingFields = [];
        var has = function(v){ return !!(v && String(v).trim()); };
        if(step === 2 && !state.accusations.some(function(x){ return has(x.accusationText); })) missingFields.push('At least one accusation is required');
        if(step === 3 && !state.accusedEmployeeRows.length && !state.accusedManualRows.length) missingFields.push('At least one accused row is required');
        if(step === 4 && !state.records.some(function(x){ return has(x.recordTitle) || has(x.recordDetails); })) missingFields.push('At least one record scrutinized row is required');
        if(step === 5){
            if(!(state.statementRegister.rows || []).some(function(x){ return parseInt(x.statementId || 0, 10) > 0; })){
                missingFields.push('At least one saved statement is required');
            } else {
                (state.statementRegister.rows || []).forEach(function(row){
                    if(!has(row.place)){
                        missingFields.push('Place of Statement (' + (row.ppnoNumber || row.personName || 'Unknown') + ')');
                    }
                });
            }
        }
        if(step === 6 && !(state.evidence.files || []).length) missingFields.push('Upload Evidence File');
        if(step === 7){
            var proceedingRows = (state.proceedings || []).filter(hasProceedingContent);
            if(!proceedingRows.length){
                missingFields.push('At least one inquiry proceedings row is required');
            } else {
                proceedingRows.forEach(function(row, idx){
                    if(!has(row.noticeReference)){ missingFields.push('Notice Reference (Proceedings row ' + (idx + 1) + ')'); }
                    if(!has(row.visitDate)){ missingFields.push('Visit Date (Proceedings row ' + (idx + 1) + ')'); }
                    if(!has(row.placeVisited)){ missingFields.push('Place Visited (Proceedings row ' + (idx + 1) + ')'); }
                    if(!has(row.participantsDetail)){ missingFields.push('Participants Detail (Proceedings row ' + (idx + 1) + ')'); }
                    if(!has(row.missingParticipantsReason)){ missingFields.push('Missing Participants / Reasons (Proceedings row ' + (idx + 1) + ')'); }
                });
            }
        }
        if(step === 8){
            (state.findingsRecomm.statusRows || []).forEach(function(row){
                var id = parseInt(row.accusationId, 10);
                if(isNaN(id)){ return; }
                if(!has(state.findingsRecomm.outcomes[id])){
                    missingFields.push('Outcome (Accusation: ' + (row.accusationText || ('#' + id)) + ')');
                }
            });
        }
        if(step === 9 && !state.violations.some(function(x){ return has(x.category) && has(x.violationDetail) && has(x.referenceText) && has(x.recommendation); })) missingFields.push('At least one complete violation row is required');
        if(step === 10 && state.isDsaVisible && !state.dsa.some(function(x){ return has(x.personName) || has(x.dsaStatus); })) missingFields.push('At least one DSA row is required');
        return { ok: missingFields.length === 0, missingFields: missingFields };
    }

    function showValidation(step, errors){
        var host = $('.wizard-section[data-step="' + step + '"] .validation-summary');
        if(!errors.length){ host.addClass('d-none').html(''); return; }
        host.removeClass('d-none').html('<strong>Missing mandatory fields:</strong><ul class="mb-0">' + errors.map(function(e){ return '<li>' + esc(e) + '</li>'; }).join('') + '</ul>');
    }

    function ensureOneRow(section){
        if(state[section].length){ return; }
        if(section === 'accusations') state.accusations.push({ accusationId: 0, accusationText: '', sortOrder: 1 });
        if(section === 'records') state.records.push({ recId: 0, recordTitle: '', recordDetails: '', sortOrder: 1 });
        if(section === 'proceedings') state.proceedings.push({ proceedingId: 0, noticeReference: '', visitDate: '', placeVisited: '', participantsDetail: '', missingParticipantsReason: '', sortOrder: 1 });
        if(section === 'violations') state.violations.push({ violationId: 0, category: 'Internal', violationDetail: '', referenceText: '', recommendation: '', sortOrder: 1 });
        if(section === 'dsa') state.dsa.push({ dsaId: 0, personName: '', designation: '', ppnoNumber: '', cnic: '', dsaStatus: '', remarks: '', sortOrder: 1 });
    }

    function digitsOnly(value){ return String(value || '').replace(/\D/g, ''); }

    function normalizeAccusedDraft(draft, isEmployee){
        var row = $.extend({}, draft || {});
        row.cnic = digitsOnly(row.cnic).substring(0, 13);
        row.ppnoNumber = isEmployee ? digitsOnly(row.ppnoNumber) : '';
        row.personName = row.personName || '';
        row.fatherName = row.fatherName || '';
        row.designation = row.designation || '';
        row.roleType = accusedRoleOptions.indexOf(row.roleType) >= 0 ? row.roleType : 'Main';
        return row;
    }

    function splitAccusedRows(rows){
        var employee = [];
        var manual = [];
        (rows || []).forEach(function(x){
            var row = {
                accusedRowId: x.accusedRowId || 0,
                personName: x.personName || '',
                fatherName: x.fatherName || '',
                designation: x.designation || '',
                roleType: accusedRoleOptions.indexOf(x.roleType) >= 0 ? x.roleType : 'Main',
                ppnoNumber: digitsOnly(x.ppnoNumber || ''),
                cnic: digitsOnly(x.cnic || '').substring(0, 13)
            };
            if(row.ppnoNumber){ employee.push(row); }
            else { manual.push(row); }
        });
        state.accusedEmployeeRows = employee;
        state.accusedManualRows = manual;
    }

    function clearAccusedDraft(type){
        if(type === 'employee'){
            state.accusedEmployeeDraft = { ppnoNumber: '', personName: '', fatherName: '', cnic: '', designation: '', roleType: 'Main' };
            return;
        }
        state.accusedManualDraft = { personName: '', fatherName: '', cnic: '', designation: '', roleType: 'Main' };
    }

    function saveAccusedSection(type){
        var isEmployee = type === 'employee';
        var draft = normalizeAccusedDraft(isEmployee ? state.accusedEmployeeDraft : state.accusedManualDraft, isEmployee);
        var errors = [];
        if(isEmployee && !draft.ppnoNumber){ errors.push('PPNO is required.'); }
        if(!draft.personName.trim()){ errors.push('Name is required.'); }
        if(!draft.fatherName.trim()){ errors.push('Father Name is required.'); }
        if(draft.cnic.length !== 13){ errors.push('CNIC must be 13 digits.'); }
        if(!draft.designation.trim()){ errors.push('Designation is required.'); }
        if(!draft.roleType){ errors.push('Role is required.'); }
        if(errors.length){ return $.Deferred().reject({ message: errors.join(' ') }).promise(); }

        var payload = {
            accusedRowId: 0,
            complaintId: complaintId,
            personName: draft.personName,
            fatherName: draft.fatherName,
            designation: draft.designation,
            roleType: draft.roleType,
            ppnoNumber: draft.ppnoNumber,
            cnic: draft.cnic,
            remarks: '',
            sortOrder: (state.accusedEmployeeRows.length + state.accusedManualRows.length) + 1,
            status: 'A',
            createdBy: userId,
            updatedBy: userId
        };

        return window.iidAddInqAccused(payload).then(function(resp){
            ensureApiSuccess(resp, 'Failed to save accused row.');
            var msg = getResponseMessage(resp) || 'Accused row saved successfully.';
            return loadStepData(3).then(function(){
                clearAccusedDraft(type);
                state.savedSteps[3] = state.accusedEmployeeRows.length > 0 || state.accusedManualRows.length > 0;
                state.dirtySteps[3] = false;
                renderCurrent(true);
                return { message: msg };
            });
        });
    }

    function normalizeStatementRegisterRow(accused){
        accused = accused || {};
        var accusedId = parseInt(accused.accusedId || accused.accusedRowId || accused.accuseD_ID || accused.id || accused.rowId || 0, 10) || 0;
        var ppno = digitsOnly(accused.ppnoNumber || accused.ppno || accused.ppNo || accused.PP_NO || '');
        var cnic = digitsOnly(accused.cnic || accused.cniC_NO || accused.CNIC || '').substring(0, 13);
        return {
            accusedId: accusedId,
            ppnoNumber: ppno,
            personName: accused.personName || accused.name || accused.emP_NAME || '',
            fatherName: accused.fatherName || accused.fatheR_NAME || '',
            roleType: accused.roleType || accused.rolE_TYPE || 'Main',
            cnic: cnic,
            statementDatetime: '',
            statementDatetimeDisplay: '',
            place: '',
            modeType: '',
            keyPoints: '',
            uploadedStatement: '',
            statementId: 0
        };
    }

    function normalizeSavedStatementRow(item){
        item = item || {};
        return {
            accusedId: parseInt(item.accusedId || item.accusedRowId || item.accuseD_ID || item.id || 0, 10) || 0,
            ppnoNumber: digitsOnly(item.ppnoNumber || item.ppno || item.ppNo || ''),
            personName: item.personName || item.name || '',
            fatherName: item.fatherName || item.fatheR_NAME || '',
            roleType: item.roleType || item.rolE_TYPE || '',
            cnic: digitsOnly(item.cnic || item.cniC_NO || '').substring(0, 13),
            statementDatetime: item.statementDatetime || item.stmtDatetime || item.datE_TIME || '',
            statementDatetimeDisplay: item.statementDatetimeDisplay || item.statementDatetime || item.stmtDatetime || item.datE_TIME || '',
            place: item.place || item.statementPlace || item.stmT_PLACE || '',
            modeType: item.modeType || item.ModeType || item.recordingMode || '',
            keyPoints: item.keyPoints || item.KeyPoints || '',
            uploadedStatement: item.uploadedStatement || item.UploadedStatement || '',
            statementId: parseInt(item.statementId || item.statemenT_ID || item.id || 0, 10) || 0
        };
    }

    function findSavedComplainantStatement(savedRows){
        var complainantCnic = digitsOnly(state.complainantCnic || state.snapshot.cnic || '').substring(0, 13);
        var complainantName = String(state.complainantName || state.snapshot.complainantName || '').trim().toLowerCase();
        return (savedRows || []).map(normalizeSavedStatementRow).find(function(item){
            var itemPpno = digitsOnly(item.ppnoNumber);
            var itemCnic = digitsOnly(item.cnic).substring(0, 13);
            var itemName = String(item.personName || '').trim().toLowerCase();
            if(isComplainantStatementRole(item.roleType)){ return true; }
            if(!itemPpno && complainantCnic && itemCnic === complainantCnic){ return true; }
            if(!itemPpno && complainantName && itemName === complainantName){ return true; }
            return false;
        }) || null;
    }

    function buildComplainantStatementRow(savedRows){
        var complainantRow = findSavedComplainantStatement(savedRows);
        return {
            accusedId: 0,
            ppnoNumber: '',
            personName: complainantRow ? (complainantRow.personName || state.complainantName || state.snapshot.complainantName || 'Complainant') : (state.complainantName || state.snapshot.complainantName || 'Complainant'),
            fatherName: '',
            roleType: complainantRow ? (complainantRow.roleType || 'Complainant') : 'Complainant',
            cnic: state.complainantCnic || state.snapshot.cnic || '',
            statementDatetime: complainantRow ? (complainantRow.statementDatetime || '') : '',
            statementDatetimeDisplay: complainantRow ? (complainantRow.statementDatetimeDisplay || complainantRow.statementDatetime || '') : '',
            place: complainantRow ? (complainantRow.place || '') : '',
            modeType: complainantRow ? (complainantRow.modeType || '') : '',
            keyPoints: complainantRow ? (complainantRow.keyPoints || '') : '',
            uploadedStatement: complainantRow ? (complainantRow.uploadedStatement || '') : '',
            statementId: complainantRow ? (parseInt(complainantRow.statementId || 0, 10) || 0) : 0
        };
    }

    function getStatementRowKey(row){
        var ppno = digitsOnly(row && row.ppnoNumber).trim();
        if(ppno){ return 'PPNO|' + ppno; }

        var cnic = digitsOnly(row && row.cnic).substring(0, 13);
        if(cnic){ return 'CNIC|' + cnic; }

        var accusedId = parseInt(row && row.accusedId || 0, 10) || 0;
        if(accusedId > 0){ return 'ACCUSED|' + accusedId; }

        if(isComplainantStatementRole(row && row.roleType)){ return 'ROLE|COMPLAINANT'; }

        var personName = String((row && row.personName) || '').trim().toLowerCase();
        var fatherName = String((row && row.fatherName) || '').trim().toLowerCase();
        return 'NAME|' + personName + '|FATHER|' + fatherName;
    }

    function mergeStatementRegister(accusedRows, savedRows){
        var savedMap = {};
        (savedRows || []).forEach(function(item){
            var normalized = normalizeSavedStatementRow(item);
            savedMap[getStatementRowKey(normalized)] = normalized;
        });

        var rows = [];
        rows.push(buildComplainantStatementRow(savedRows));

        (accusedRows || []).forEach(function(accused){
            var baseRow = normalizeStatementRegisterRow(accused);
            var key = getStatementRowKey(baseRow);
            var savedRow = savedMap[key];
            if(savedRow){
                var mergedRow = $.extend({}, baseRow, savedRow);
                if(!String(savedRow.fatherName || '').trim()){
                    mergedRow.fatherName = baseRow.fatherName || '';
                }
                rows.push(mergedRow);
                return;
            }

            rows.push(baseRow);
        });

        state.statementRegister.rows = rows;
    }

    function normalizeStatementDatetime(value){
        var raw = String(value || '').trim();
        if(!raw){ return ''; }

        if(/^\d{4}-\d{2}-\d{2}$/.test(raw)){
            return raw;
        }

        if(/^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}$/.test(raw)){
            return raw + ':00';
        }

        if(/^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}$/.test(raw)){
            return raw;
        }

        var parsed = new Date(raw);
        if(isNaN(parsed.getTime())){ return ''; }

        var yyyy = parsed.getFullYear();
        var mm = String(parsed.getMonth() + 1).padStart(2, '0');
        var dd = String(parsed.getDate()).padStart(2, '0');
        var hh = String(parsed.getHours()).padStart(2, '0');
        var mi = String(parsed.getMinutes()).padStart(2, '0');
        var ss = String(parsed.getSeconds()).padStart(2, '0');
        return yyyy + '-' + mm + '-' + dd + 'T' + hh + ':' + mi + ':' + ss;
    }

    function saveStatementRow(index){
        var row = state.statementRegister.rows[index];
        if(!row){ return $.Deferred().reject({ message: 'Invalid statement row.' }).promise(); }

        row.cnic = digitsOnly(row.cnic).substring(0, 13);
        row.ppnoNumber = digitsOnly(row.ppnoNumber);
        row.statementDatetime = normalizeStatementDatetime(row.statementDatetime);
        row.modeType = row.modeType || '';
        row.keyPoints = row.keyPoints || '';
        row.roleType = row.roleType || (isComplainantStatementRole(row.roleType) ? 'Complainant' : 'Main');

        var errors = [];
        if(!row.statementDatetime){ errors.push('A valid Date is required.'); }
        if(!row.place || !row.place.trim()){ errors.push('Place of statement is required.'); }
        if(errors.length){ return $.Deferred().reject({ message: errors.join(' ') }).promise(); }

        var payload = {
            complaintId: complaintId,
            personName: row.personName || '',
            roleType: row.roleType || (isComplainantStatementRole(row.roleType) ? 'Complainant' : 'Main'),
            ppnoNumber: row.ppnoNumber || '',
            cnic: row.cnic || '',
            statementDatetime: row.statementDatetime,
            place: row.place,
            modeType: row.modeType || '',
            keyPoints: row.keyPoints || '',
            uploadedStatement: row.uploadedStatement || '',
            userId: userId
        };

        return window.iidSaveInqStatement(payload).then(function(resp){
            ensureApiSuccess(resp, 'Failed to save statement register row.');
            return loadStepData(5).then(function(){
                state.savedSteps[5] = (state.statementRegister.rows || []).some(function(x){ return parseInt(x.statementId || 0, 10) > 0; });
                state.dirtySteps[5] = false;
                renderCurrent(true);
                return { message: getResponseMessage(resp) || 'Statement saved successfully.' };
            });
        });
    }

    function reloadProceedingsSection(message){
        return loadStepData(7).then(function(){
            state.savedSteps[7] = (state.proceedings || []).some(function(x){ return parseInt(x.proceedingId || 0, 10) > 0; });
            state.dirtySteps[7] = false;
            renderCurrent(true);
            return { message: message || 'Inquiry proceedings refreshed successfully.' };
        });
    }

    function validateProceedingRow(row, rowIndex){
        var errors = [];
        var label = 'Proceedings row ' + (rowIndex + 1);
        if(!row.noticeReference || !row.noticeReference.trim()){ errors.push(label + ': Notice Reference is required.'); }
        if(!row.visitDate || !normalizeDateOnly(row.visitDate)){ errors.push(label + ': Visit Date is required.'); }
        if(!row.placeVisited || !row.placeVisited.trim()){ errors.push(label + ': Place Visited is required.'); }
        if(!row.participantsDetail || !row.participantsDetail.trim()){ errors.push(label + ': Participants Detail is required.'); }
        if(!row.missingParticipantsReason || !row.missingParticipantsReason.trim()){ errors.push(label + ': Missing Participants / Reason is required.'); }
        return errors;
    }

    function saveProceedingRow(index){
        var row = state.proceedings[index];
        if(!row){ return $.Deferred().reject({ message: 'Invalid inquiry proceeding row.' }).promise(); }

        row.visitDate = normalizeDateOnly(row.visitDate);
        var errors = validateProceedingRow(row, index);
        if(errors.length){ return $.Deferred().reject({ message: errors.join(' ') }).promise(); }

        return window.iidSaveInqProceeding({
            proceedingId: parseInt(row.proceedingId || 0, 10) || 0,
            complaintId: complaintId,
            noticeReference: row.noticeReference || '',
            visitDate: row.visitDate || '',
            placeVisited: row.placeVisited || '',
            participantsDetail: row.participantsDetail || '',
            missingParticipantsReason: row.missingParticipantsReason || '',
            sortOrder: index + 1,
            status: row.status || 'A',
            userId: userId
        }).then(function(resp){
            ensureApiSuccess(resp, 'Failed to save inquiry proceeding row.');
            return reloadProceedingsSection(getResponseMessage(resp) || 'Inquiry proceeding row saved successfully.');
        });
    }

    function deleteProceedingRow(index){
        var row = state.proceedings[index];
        if(!row){ return $.Deferred().reject({ message: 'Invalid inquiry proceeding row.' }).promise(); }

        var proceedingId = parseInt(row.proceedingId || 0, 10) || 0;
        if(proceedingId <= 0){
            state.proceedings.splice(index, 1);
            ensureOneRow('proceedings');
            markDirty(7);
            renderCurrent(true);
            return $.Deferred().resolve({ message: 'Inquiry proceeding row removed.' }).promise();
        }

        return window.iidDeleteInqProceeding(proceedingId, userId || 0).then(function(resp){
            ensureApiSuccess(resp, 'Failed to delete inquiry proceeding row.');
            return reloadProceedingsSection(getResponseMessage(resp) || 'Inquiry proceeding row deleted successfully.');
        });
    }

    function saveCollection(opts){
        var queue = $.Deferred().resolve().promise();
        var lastMessage = '';

        (state.deleteQueue[opts.section] || []).forEach(function(id){
            queue = queue.then(function(){
                return opts.deleteFn(id, userId).then(function(resp){
                    ensureApiSuccess(resp, 'Delete failed.');
                    lastMessage = getResponseMessage(resp) || lastMessage;
                });
            });
        });
        state.deleteQueue[opts.section] = [];

        opts.rows.forEach(function(row, idx){
            var payload = opts.map(row, idx);
            queue = queue.then(function(){
                var request = row[opts.idKey] ? opts.updateFn(payload) : opts.addFn(payload);
                return request.then(function(resp){
                    ensureApiSuccess(resp, 'Save failed.');
                    lastMessage = getResponseMessage(resp) || lastMessage;
                });
            });
        });

        return queue.then(function(){ return opts.loadFn(complaintId); }).then(function(resp){
            ensureApiSuccess(resp, 'Load failed.');
            opts.hydrate(resp);
            return { message: lastMessage || opts.successMessage };
        });
    }

    function loadStepData(step){
        if(step < 2 || step > 10){ return $.Deferred().resolve().promise(); }
        var cfg = {
            2: { loadFn: window.iidGetInqAccusations, hydrate: function(resp){ state.accusations = extractData(resp).map(function(x){ return { accusationId: x.accusationId, accusationText: x.accusationText, sortOrder: x.sortOrder || 1 }; }); ensureOneRow('accusations'); }, section: 'accusations' },
            3: { loadFn: window.iidGetInqAccusedList, hydrate: function(resp){ splitAccusedRows(extractData(resp)); clearAccusedDraft('employee'); clearAccusedDraft('manual'); }, section: 'accusedRows' },
            4: { loadFn: window.iidGetInqRecords, hydrate: function(resp){ state.records = extractData(resp); ensureOneRow('records'); }, section: 'records' },
            5: { loadFn: function(id){
                    return $.when(window.iidGetInqAccusedList(id), window.iidGetInqStatements(id)).then(function(accusedResp, savedResp){
                        return {
                            accusedRows: extractData(accusedResp && accusedResp[0] ? accusedResp[0] : accusedResp),
                            savedRows: extractData(savedResp && savedResp[0] ? savedResp[0] : savedResp)
                        };
                    });
                }, hydrate: function(resp){
                    mergeStatementRegister(resp.accusedRows || [], resp.savedRows || []);
                }, section: 'statementRegister' },
            6: { loadFn: window.iidGetInqEvidenceStep, hydrate: function(resp){
                    state.evidence = {
                        files: extractData(resp.evidenceFiles || resp),
                        materialEvidenceDetail: resp.materialEvidenceDetail || '',
                        circumstantialEvidenceDetail: resp.circumstantialEvidenceDetail || ''
                    };
                }, section: 'evidence' },
            7: { loadFn: window.iidGetInqProceedings, hydrate: function(resp){
                    state.proceedings = extractData(resp).map(function(x){
                        return {
                            proceedingId: x.proceedingId || x.rowId || 0,
                            noticeReference: x.noticeReference || '',
                            visitDate: formatDateInputValue(x.visitDate || ''),
                            placeVisited: x.placeVisited || '',
                            participantsDetail: x.participantsDetail || '',
                            missingParticipantsReason: x.missingParticipantsReason || '',
                            sortOrder: x.sortOrder || 1,
                            status: x.status || 'A'
                        };
                    });
                    state.savedSteps[7] = state.proceedings.some(function(x){ return parseInt(x.proceedingId || 0, 10) > 0; });
                    ensureOneRow('proceedings');
                }, section: 'proceedings' },
            8: { loadFn: loadFindingsModule, hydrate: function(){}, section: 'findingsRecomm' },
            9: { loadFn: window.iidGetInqViolations, hydrate: function(resp){ state.violations = extractData(resp); ensureOneRow('violations'); }, section: 'violations' },
            10: { loadFn: window.iidGetInqDsa, hydrate: function(resp){ state.dsa = extractData(resp); ensureOneRow('dsa'); }, section: 'dsa' }
        }[step];

        return cfg.loadFn(complaintId).then(function(resp){
            ensureApiSuccess(resp, 'Failed to load saved step data.');
            cfg.hydrate(resp);
            state.dirtySteps[step] = false;
            if(step === 5){
                state.savedSteps[step] = (state.statementRegister.rows || []).some(function(x){ return parseInt(x.statementId || 0, 10) > 0; });
                return;
            }
            if(step === 6){
                state.savedSteps[step] = !!((state.evidence.files || []).length || (state.evidence.materialEvidenceDetail || '').trim() || (state.evidence.circumstantialEvidenceDetail || '').trim());
                return;
            }
            if(cfg.section === 'findingsRecomm'){
                state.savedSteps[step] = (state.findingsRecomm.statusRows || []).some(isStatusRowSaved);
                return;
            }
            var list = cfg.section === 'accusedRows' ? state.accusedEmployeeRows.concat(state.accusedManualRows) : state[cfg.section];
            state.savedSteps[step] = (list || []).some(function(x){ return Object.keys(x || {}).some(function(k){ return k.toLowerCase().indexOf('id') >= 0 && Number(x[k] || 0) > 0; }); });
        });
    }

    function saveStep(step){
        if(isReadOnlyMode){
            return $.Deferred().reject({ message: 'This report is read-only and cannot be edited.' }).promise();
        }
        var errs = validateStep(step);
        showValidation(step, errs);
        if(errs.length){ return $.Deferred().reject().promise(); }

        if(step === 2){
            return saveCollection({ section: 'accusations', rows: state.accusations.filter(function(x){ return x.accusationText && x.accusationText.trim(); }), idKey: 'accusationId', addFn: window.iidAddInqAccusation, updateFn: window.iidUpdateInqAccusation, deleteFn: window.iidDeleteInqAccusation, loadFn: window.iidGetInqAccusations, map: function(row, idx){ return { accusationId: row.accusationId || 0, complaintId: complaintId, accusationText: row.accusationText, sortOrder: idx + 1, status: 'A', createdBy: userId, updatedBy: userId }; }, hydrate: function(resp){ state.accusations = extractData(resp).map(function(x){ return { accusationId: x.accusationId, accusationText: x.accusationText, sortOrder: x.sortOrder || 1 }; }); ensureOneRow('accusations'); }, successMessage: 'Accusations saved successfully.' });
        }
        if(step === 3){
            return loadStepData(3).then(function(){
                state.savedSteps[3] = state.accusedEmployeeRows.length > 0 || state.accusedManualRows.length > 0;
                state.dirtySteps[3] = false;
                return { message: 'Accused list refreshed from database.' };
            });
        }
        if(step === 4){
            return saveCollection({ section: 'records', rows: state.records.filter(function(x){ return ((x.recordTitle || '') + (x.recordDetails || '')).trim(); }), idKey: 'recId', addFn: window.iidAddInqRecord, updateFn: window.iidUpdateInqRecord, deleteFn: window.iidDeleteInqRecord, loadFn: window.iidGetInqRecords, map: function(row, idx){ return $.extend({}, row, { complaintId: complaintId, sortOrder: idx + 1, status: 'A', createdBy: userId, updatedBy: userId }); }, hydrate: function(resp){ state.records = extractData(resp); ensureOneRow('records'); }, successMessage: 'Record scrutinized saved successfully.' });
        }
        if(step === 5){
            return $.Deferred().resolve({ message: 'Use section-level save buttons for Statement Register.' }).promise();
        }
        if(step === 6){
            return window.iidSaveInqEvidenceStep({
                complaintId: complaintId,
                materialEvidenceDetail: state.evidence.materialEvidenceDetail || '',
                circumstantialEvidenceDetail: state.evidence.circumstantialEvidenceDetail || ''
            }).then(function(resp){
                ensureApiSuccess(resp, 'Failed to save evidence details.');
                return loadStepData(6).then(function(){
                    state.dirtySteps[6] = false;
                    return { message: getResponseMessage(resp) || 'Evidence details saved successfully.' };
                });
            });
        }
        if(step === 7){
            return $.Deferred().resolve({ message: 'Use row-level Save / Update / Delete buttons for Inquiry Proceedings.' }).promise();
        }
        if(step === 8){
            var selectedAccusationId = parseInt(state.findingsRecomm.selectedAccusationId, 10);
            var findingHtml = (window.tinymce && window.tinymce.get('findingTextHtml') ? window.tinymce.get('findingTextHtml').getContent() : state.findingsRecomm.findingText) || '';
            var recommendationHtml = (window.tinymce && window.tinymce.get('recommendationTextHtml') ? window.tinymce.get('recommendationTextHtml').getContent() : state.findingsRecomm.recommendationText) || '';
            var outcome = state.findingsRecomm.outcomes[selectedAccusationId] || '';
            if(isNaN(selectedAccusationId) || selectedAccusationId <= 0){
                return $.Deferred().reject({ message: 'Please select an accusation saved in Section 2 before saving findings/recommendation.' }).promise();
            }
            state.findingsRecomm.findingText = findingHtml;
            state.findingsRecomm.recommendationText = recommendationHtml;

            return window.iidSaveIidFindingsRecommByAccusation({ complaintId: complaintId, accusationId: selectedAccusationId, findingText: findingHtml, recomText: recommendationHtml, outcome: outcome }).then(function(resp){
                ensureApiSuccess(resp, 'Findings & recommendations save failed.');
                state.findingsRecomm.savedFindingsMap[selectedAccusationId] = { findingsText: findingHtml, recommendationText: recommendationHtml, outcome: outcome };
                state.findingsRecomm.lockedOutcomes[selectedAccusationId] = true;
                upsertFindingsStatusRow({ accusationId: selectedAccusationId, accusationText: (state.findingsRecomm.accusationOptions || []).filter(function(x){ return parseInt(x.accusationId, 10) === selectedAccusationId; }).map(function(x){ return x.accusationText; })[0] || '', isSaved: true, outcome: outcome, savedOn: (resp && resp.savedOn) || (resp && resp.data && resp.data.savedOn) || new Date().toISOString() });
                return loadFindingsStatusGrid().then(function(){
                    state.savedSteps[8] = (state.findingsRecomm.statusRows || []).some(isStatusRowSaved);
                    state.dirtySteps[8] = false;
                    return { message: getResponseMessage(resp) || 'Findings & recommendations saved successfully.' };
                });
            });
        }
        if(step === 9){
            return saveCollection({ section: 'violations', rows: state.violations.filter(function(x){ return x.violationDetail && x.referenceText && x.recommendation; }), idKey: 'violationId', addFn: window.iidAddInqViolation, updateFn: window.iidUpdateInqViolation, deleteFn: window.iidDeleteInqViolation, loadFn: window.iidGetInqViolations, map: function(row, idx){ return $.extend({}, row, { complaintId: complaintId, sortOrder: idx + 1, status: 'A', createdBy: userId, updatedBy: userId }); }, hydrate: function(resp){ state.violations = extractData(resp); ensureOneRow('violations'); }, successMessage: 'Violations saved successfully.' });
        }
        if(step === 10){
            return saveCollection({ section: 'dsa', rows: state.dsa.filter(function(x){ return x.personName || x.dsaStatus; }), idKey: 'dsaId', addFn: window.iidAddInqDsa, updateFn: window.iidUpdateInqDsa, deleteFn: window.iidDeleteInqDsa, loadFn: window.iidGetInqDsa, map: function(row, idx){ return $.extend({}, row, { complaintId: complaintId, sortOrder: idx + 1, status: 'A', createdBy: userId, updatedBy: userId }); }, hydrate: function(resp){ state.dsa = extractData(resp); ensureOneRow('dsa'); }, successMessage: 'DSA saved successfully.' });
        }
        return $.Deferred().resolve({ message: 'Saved.' }).promise();
    }

    function renderCurrent(skipLoad){
        var render = function(){
            renderSection(currentStep);
            $('.wizard-section').addClass('d-none');
            $('.wizard-section[data-step="' + currentStep + '"]').removeClass('d-none');
            renderStepper();
            bindStepInputs();
        };
        if(skipLoad){ render(); return; }
        loadStepData(currentStep).always(render);
    }

    function navigateToStep(nextStep){
        if(!isReadOnlyMode && hasUnsavedStep(currentStep)){
            renderDirtyStepAlert();
            return;
        }
        currentStep = nextStep;
        renderCurrent();
    }

    function markSectionCompleted(step){
        if(isReadOnlyMode){
            return $.Deferred().reject({ message: 'This report is read-only and cannot be edited.' }).promise();
        }
        var errs = validateStep(step);
        if(step === 8){
            var step8Completion = validateStepForFinalSubmit(8);
            errs = errs.concat(step8Completion.missingFields);
        }
        showValidation(step, errs);
        if(errs.length){ return $.Deferred().reject({ message: 'Please complete required fields before marking this section completed.' }).promise(); }
        state.savedSteps[step] = true;
        state.dirtySteps[step] = false;
        renderStepper();
        return $.Deferred().resolve({ message: 'Section marked completed.' }).promise();
    }

    function showFinalSubmitErrors(missingSections, missingFieldsBySection){
        var html = '<div><strong>Missing sections:</strong> ' + esc(missingSections.join(', ')) + '</div>';
        html += '<div class="mt-2"><strong>Missing fields:</strong><ul>';
        Object.keys(missingFieldsBySection).forEach(function(section){
            missingFieldsBySection[section].forEach(function(field){
                html += '<li><strong>' + esc(section) + '</strong> → ' + esc(field) + '</li>';
            });
        });
        html += '</ul></div>';

        if(window.bootstrap && window.bootstrap.Modal){
            var modal = $('#iidValidationModal');
            if(!modal.length){
                $('body').append('<div class="modal fade" id="iidValidationModal" tabindex="-1" aria-hidden="true"><div class="modal-dialog modal-lg"><div class="modal-content"><div class="modal-header"><h5 class="modal-title">' + esc(finalActionLabel) + ' Validation</h5><button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button></div><div class="modal-body" id="iidValidationModalBody"></div><div class="modal-footer"><button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Close</button></div></div></div></div>');
                modal = $('#iidValidationModal');
            }
            $('#iidValidationModalBody').html(html);
            window.bootstrap.Modal.getOrCreateInstance(modal[0]).show();
            return;
        }

        var lines = ['Missing sections: ' + missingSections.join(', '), 'Missing fields:'];
        Object.keys(missingFieldsBySection).forEach(function(section){
            missingFieldsBySection[section].forEach(function(field){ lines.push('- ' + section + ' -> ' + field); });
        });
        alert(lines.join('\n'));
    }

    function loadComplaint(){
        return $.post(g_asiBaseURL + '/ApiCalls/GetComplaint', { complaintId: complaintId }).done(function(resp){
            ensureApiSuccess(resp, 'Failed to load complaint snapshot.');
            var c = resp || {};
            state.complainantName = c.complainantName || '';
            state.complainantCnic = c.cnic || '';
            reportId = reportId || parseInt(c.reportId || 0, 10) || reportId;
            state.snapshot.status = c.status || '';
            if(isReadOnlyMode || shouldLockByStatus(c.status)){ isLocked = true; }
            $('#headerComplainantName').text(state.complainantName || 'N/A');
            $('#iidComplaintHiddenCache').text(JSON.stringify(c));
            state.snapshot = { complaintNo: c.complaintNo, submittedOn: c.submittedOn, region: c.region, branch: c.branch, category: c.category, nature: c.nature, complainantName: c.complainantName, cnic: c.cnic, cellularNumber: c.cellularNumber, mailingAddress: c.mailingAddress, gender: c.gender, receivedFrom: c.receivedFrom, actionRequired: c.actionRequired, contents: c.contents, uploadedComplaint: c.uploadedComplaint || c.UploadedComplaint || c.uploaded_complaint, uploadedFFR: c.uploadedFFR || c.UploadedFFR || c.uploaded_FFR, uploadedEvidence: c.uploadedEvidence || c.UploadedEvidence || c.uploaded_evidence, status: c.status || '' };
            applyStatusBadge(c.status || '');
            renderSnapshotStrip();
        });
    }

    function submitForAnalysis(){
        return $.ajax({ url: g_asiBaseURL + '/ApiCalls/SubmitIidInquiryReportForAnalysis', method: 'POST', contentType: 'application/json; charset=utf-8', dataType: 'json', data: JSON.stringify({ complaintId: complaintId }) }).done(function(resp){
            if(!resp || resp.ok === false){ showAlert(getResponseMessage(resp) || 'Submission failed.', 'danger'); return; }
            isLocked = true;
            state.snapshot.status = 'QC_CLEARED';
            applyStatusBadge('QC_CLEARED');
            showAlert(resp.message || finalActionSuccessMessage, 'success');
            renderCurrent(true);
        });
    }

    function finalizeReportClosure(){
        return $.ajax({
            url: g_asiBaseURL + '/ApiCalls/FinalizeIidReport',
            method: 'POST',
            contentType: 'application/json; charset=utf-8',
            dataType: 'json',
            data: JSON.stringify({ complaintId: complaintId, reportId: reportId || 0 })
        }).done(function(resp){
            if(!resp || resp.ok === false){ showAlert(getResponseMessage(resp) || 'Finalization failed.', 'danger'); return; }
            isLocked = true;
            state.snapshot.status = 'CLOSED';
            applyStatusBadge('CLOSED');
            showAlert(resp.message || finalActionSuccessMessage, 'success');
            renderCurrent(true);
        });
    }

    $(document).off('.iidInquiryReport');
    $(document).on('click.iidInquiryReport', '[data-step-jump]', function(){ navigateToStep(parseInt($(this).data('step-jump'), 10)); });
    $(document).on('click.iidInquiryReport', '[data-prev]', function(){ var prevStep = getPreviousVisibleStep(currentStep); if(prevStep){ navigateToStep(prevStep); } });
    $(document).on('click.iidInquiryReport', '[data-next]', function(){
        if(isReadOnlyMode){
            var readonlyNext = getNextVisibleStep(currentStep);
            if(readonlyNext){ navigateToStep(readonlyNext); }
            return;
        }
        var e = validateStep(currentStep);
        showValidation(currentStep, e);
        if(!e.length){
            var nextStep = getNextVisibleStep(currentStep);
            if(nextStep){ navigateToStep(nextStep); }
        }
    });
    $(document).on('click.iidInquiryReport', '[data-save]', function(){
        var action = currentStep === 8 ? markSectionCompleted(currentStep) : saveStep(currentStep);
        action.done(function(result){
            if(currentStep !== 8){
                state.savedSteps[currentStep] = true;
                state.dirtySteps[currentStep] = false;
            }
            showAlert((result && result.message) || 'Saved successfully.', 'success');
            renderCurrent(true);
        }).fail(function(err){
            showAlert((err && err.message) || 'Save failed.', 'danger');
        });
    });

    $(document).on('click.iidInquiryReport', '[data-step3-search]', function(){
        var ppno = digitsOnly(state.accusedEmployeeDraft.ppnoNumber);
        state.accusedEmployeeDraft.ppnoNumber = ppno;
        if(!ppno){ showAlert('PPNO is required for search.', 'danger'); renderCurrent(true); return; }
        window.iidGetIidEmployeeInfo(ppno).done(function(resp){
            if(!resp || resp.ok === false){
                showAlert((resp && resp.message) || 'No employee found for this PPNO', 'danger');
                return;
            }
            var emp = resp.data || {};
            state.accusedEmployeeDraft.ppnoNumber = digitsOnly(emp.ppno || ppno);
            state.accusedEmployeeDraft.personName = emp.name || '';
            state.accusedEmployeeDraft.fatherName = emp.fatherName || '';
            state.accusedEmployeeDraft.cnic = digitsOnly(emp.cnic || '').substring(0, 13);
            renderCurrent(true);
            showAlert('Employee details loaded. You can edit before save.', 'success');
        }).fail(function(){ showAlert('Employee lookup failed for entered PPNO.', 'danger'); });
    });

    $(document).on('click.iidInquiryReport', '[data-step3-save]', function(){
        var type = $(this).data('step3-save');
        saveAccusedSection(type).done(function(resp){
            showAlert((resp && resp.message) || 'Accused row saved successfully.', 'success');
        }).fail(function(err){
            showAlert((err && err.message) || 'Failed to save accused row.', 'danger');
        });
    });

    $(document).on('click.iidInquiryReport', '[data-remove-accused]', function(){
        var id = parseInt($(this).data('id'), 10) || 0;
        if(!id){ showAlert('Invalid accused row id.', 'danger'); return; }
        window.iidDeleteInqAccused(id, userId || 0).then(function(resp){
            ensureApiSuccess(resp, 'Failed to delete accused row.');
            var msg = getResponseMessage(resp) || 'Accused row removed successfully.';
            return loadStepData(3).then(function(){
                state.savedSteps[3] = state.accusedEmployeeRows.length > 0 || state.accusedManualRows.length > 0;
                state.dirtySteps[3] = false;
                renderCurrent(true);
                showAlert(msg, 'success');
            });
        }).fail(function(err){ showAlert((err && err.message) || 'Failed to delete accused row.', 'danger'); });
    });

    $(document).on('click.iidInquiryReport', '[data-step5-save-row]', function(){
        var rowIndex = parseInt($(this).data('step5-save-row'), 10);
        saveStatementRow(rowIndex).done(function(resp){
            showAlert((resp && resp.message) || 'Statement saved successfully.', 'success');
        }).fail(function(err){
            showAlert((err && err.message) || 'Failed to save statement.', 'danger');
        });
    });

    $(document).on('click.iidInquiryReport', '[data-step5-upload-row]', function(){
        var rowIndex = parseInt($(this).data('step5-upload-row'), 10);
        $('[data-step5-upload-input="' + rowIndex + '"]').trigger('click');
    });

    $(document).on('change.iidInquiryReport', '[data-step5-upload-input]', function(){
        var rowIndex = parseInt($(this).data('step5-upload-input'), 10);
        var row = state.statementRegister.rows[rowIndex];
        var file = this.files && this.files[0] ? this.files[0] : null;
        var input = this;

        if(!row || !file){
            input.value = '';
            return;
        }

        var fd = new FormData();
        fd.append('__RequestVerificationToken', token());
        fd.append('file', file);
        fd.append('complaintId', complaintId);

        window.iidUploadInqStatementFile(fd).then(function(resp){
            ensureApiSuccess(resp, 'Statement file upload failed.');
            row.uploadedStatement = resp.fileName || (resp.data && resp.data.fileName) || '';
            row.uploadedStatementUrl = resp.fileUrl || (resp.data && resp.data.fileUrl) || buildUploadUrl(row.uploadedStatement);
            markDirty(5);

            if(row.statementDatetime && row.place && row.place.trim()){
                return saveStatementRow(rowIndex).then(function(saveResp){
                    showAlert((saveResp && saveResp.message) || 'Statement file uploaded and saved successfully.', 'success');
                });
            }

            renderCurrent(true);
            showAlert((getResponseMessage(resp) || 'Statement file uploaded.') + ' Complete Date and Place, then click Save.', 'success');
        }).fail(function(err){
            showAlert((err && err.message) || 'Statement file upload failed.', 'danger');
        }).always(function(){
            input.value = '';
        });
    });

    $(document).on('click.iidInquiryReport', '[data-step7-save-row]', function(){
        var rowIndex = parseInt($(this).data('step7-save-row'), 10);
        saveProceedingRow(rowIndex).done(function(resp){
            showAlert((resp && resp.message) || 'Inquiry proceeding row saved successfully.', 'success');
        }).fail(function(err){
            showAlert((err && err.message) || 'Failed to save inquiry proceeding row.', 'danger');
        });
    });

    $(document).on('click.iidInquiryReport', '[data-step7-delete-row]', function(){
        var rowIndex = parseInt($(this).data('step7-delete-row'), 10);
        deleteProceedingRow(rowIndex).done(function(resp){
            showAlert((resp && resp.message) || 'Inquiry proceeding row deleted successfully.', 'success');
        }).fail(function(err){
            showAlert((err && err.message) || 'Failed to delete inquiry proceeding row.', 'danger');
        });
    });

    $(document).on('click.iidInquiryReport', '[data-add-row]', function(){
        var s = $(this).data('add-row');
        if(s==='accusations') state.accusations.push({ accusationId: 0, accusationText: '', sortOrder: state.accusations.length + 1 });
        if(s==='records') state.records.push({ recId: 0, recordTitle: '', recordDetails: '', sortOrder: state.records.length + 1 });
        if(s==='proceedings') state.proceedings.push({ proceedingId: 0, noticeReference: '', visitDate: '', placeVisited: '', participantsDetail: '', missingParticipantsReason: '', sortOrder: state.proceedings.length + 1 });
        if(s==='violations') state.violations.push({ violationId: 0, category: 'Internal', violationDetail: '', referenceText: '', recommendation: '', sortOrder: state.violations.length + 1 });
        if(s==='dsa') state.dsa.push({ dsaId: 0, personName: '', designation: '', ppnoNumber: '', cnic: '', dsaStatus: '', remarks: '', sortOrder: state.dsa.length + 1 });
        markDirty(currentStep);
        renderCurrent(true);
    });

    $(document).on('click.iidInquiryReport', '[data-remove-row]', function(){
        var section = $(this).data('remove-row');
        var idx = parseInt($(this).data('index'), 10);
        var row = state[section][idx];

        if(row){
            var idKeyMap = { accusations: 'accusationId', records: 'recId', proceedings: 'proceedingId', violations: 'violationId', dsa: 'dsaId' };
            var idKey = idKeyMap[section];
            if(idKey && row[idKey] && row[idKey] > 0){ state.deleteQueue[section].push(row[idKey]); }
        }
        state[section].splice(idx, 1);
        ensureOneRow(section);
        markDirty(currentStep);
        renderCurrent(true);
    });

    $(document).on('change.iidInquiryReport', '#uploadedEvidence', function(){
        var files = Array.prototype.slice.call(this.files || []);
        var chain = $.Deferred().resolve().promise();
        files.forEach(function(file){
            chain = chain.then(function(){
                var fd = new FormData();
                fd.append('__RequestVerificationToken', token());
                fd.append('ComplaintId', complaintId);
                fd.append('EvidenceType', 'Document');
                fd.append('Description', file.name);
                fd.append('Status', 'A');
                fd.append('UploadedBy', userId || 0);
                fd.append('file', file);
                return window.iidAddInqEvidenceFile(fd).then(function(resp){ ensureApiSuccess(resp, 'Evidence upload failed.'); return getResponseMessage(resp) || 'Uploaded successfully'; });
            });
        });
        chain.then(function(msg){
            return window.iidGetInqEvidenceStep(complaintId).then(function(resp){
                ensureApiSuccess(resp, 'Failed to refresh evidence list.');
                state.evidence = {
                    files: extractData(resp.evidenceFiles || resp),
                    materialEvidenceDetail: resp.materialEvidenceDetail || state.evidence.materialEvidenceDetail || '',
                    circumstantialEvidenceDetail: resp.circumstantialEvidenceDetail || state.evidence.circumstantialEvidenceDetail || ''
                };
                state.savedSteps[6] = true;
                state.dirtySteps[6] = false;
                showAlert(msg || 'Uploaded successfully', 'success');
                renderCurrent(true);
            });
        }).fail(function(err){ showAlert((err && err.message) || 'Evidence upload failed.', 'danger'); });
        this.value = '';
    });

    $(document).on('click.iidInquiryReport', '[data-delete-evidence]', function(){
        var evidenceId = parseInt($(this).data('delete-evidence'), 10);
        window.iidDeleteInqEvidenceFile(evidenceId, userId || 0).then(function(resp){
            ensureApiSuccess(resp, 'Failed to delete evidence file.');
            var msg = getResponseMessage(resp) || 'Evidence removed successfully.';
            return window.iidGetInqEvidenceStep(complaintId).then(function(loadResp){
                ensureApiSuccess(loadResp, 'Failed to refresh evidence list.');
                state.evidence = {
                    files: extractData(loadResp.evidenceFiles || loadResp),
                    materialEvidenceDetail: loadResp.materialEvidenceDetail || state.evidence.materialEvidenceDetail || '',
                    circumstantialEvidenceDetail: loadResp.circumstantialEvidenceDetail || state.evidence.circumstantialEvidenceDetail || ''
                };
                showAlert(msg, 'success');
                renderCurrent(true);
            });
        }).fail(function(err){ showAlert((err && err.message) || 'Failed to delete evidence file.', 'danger'); });
    });

    $(document).on('change.iidInquiryReport', '[data-findings-accusation-select]', function(){
        state.findingsRecomm.selectedAccusationId = $(this).val();
        markDirty(8);
        loadFindingsForSelection(state.findingsRecomm.selectedAccusationId).fail(function(err){
            showAlert((err && err.message) || 'Failed to load findings for selected accusation.', 'danger');
        });
    });

    $(document).on('click.iidInquiryReport', '[data-findings-view-edit]', function(){
        var accusationId = accusationIdValue($(this).data('findings-view-edit'));
        state.findingsRecomm.selectedAccusationId = accusationId;
        $('#findingsAccusationSelect').val(accusationId);
        loadFindingsForSelection(accusationId, true).fail(function(err){
            showAlert((err && err.message) || 'Failed to load findings for selected accusation.', 'danger');
        });
    });

    $(document).on('click.iidInquiryReport', '[data-save-findings-recomm]', function(){
        saveStep(8).done(function(result){
            showAlert((result && result.message) || 'Saved successfully.', 'success');
            renderCurrent(true);
        }).fail(function(err){
            showAlert((err && err.message) || 'Save failed.', 'danger');
        });
    });

    $(document).on('change.iidInquiryReport', '.outcome-select', function(){
        var accusationId = parseInt($(this).data('accusation-id'), 10);
        if(isNaN(accusationId)){ return; }
        $(this).val(state.findingsRecomm.outcomes[accusationId] || '');
    });

    $(document).on('change.iidInquiryReport', '[data-findings-outcome]', function(){
        var accusationId = parseInt(state.findingsRecomm.selectedAccusationId, 10);
        if(isNaN(accusationId)){ return; }
        state.findingsRecomm.outcomes[accusationId] = $(this).val() || '';
        var row = (state.findingsRecomm.statusRows || []).find(function(x){ return parseInt(x.accusationId, 10) === accusationId; });
        if(row){ row.outcome = state.findingsRecomm.outcomes[accusationId]; }
        $('.outcome-select[data-accusation-id="' + accusationId + '"]').val(state.findingsRecomm.outcomes[accusationId] || '');
        renderFindingsStatusGrid();
        markDirty(8);
        updateDsaVisibility();
        renderStepper();
    });

    $(document).on('click.iidInquiryReport', '#finalActionBtn', function(){
        if(reportMode === 'final'){
            finalizeReportClosure();
            return;
        }
        var mandatorySteps = [2,3,4,5,6,7,8,9].concat(state.isDsaVisible ? [10] : []);
        var sectionValidators = mandatorySteps.map(function(stepId){
            var stepInfo = steps.filter(function(s){ return s.id === stepId; })[0] || { title: 'Step ' + stepId };
            return {
                stepId: stepId,
                stepTitle: stepInfo.title,
                validate: function(){ return validateStepForFinalSubmit(stepId); }
            };
        });

        var missingSections = [];
        var missingFieldsBySection = {};
        sectionValidators.forEach(function(validator){
            var result = validator.validate();
            if(!result.ok){
                missingSections.push(validator.stepTitle);
                missingFieldsBySection[validator.stepTitle] = result.missingFields;
            }
        });

        if(missingSections.length){
            showFinalSubmitErrors(missingSections, missingFieldsBySection);
            return;
        }
        var finalStepId = getLastVisibleStepId();
        var finalizeChain = hasUnsavedStep(finalStepId) ? saveStep(finalStepId) : $.Deferred().resolve({}).promise();
        finalizeChain.done(function(){
            state.savedSteps[finalStepId] = true;
            state.dirtySteps[finalStepId] = false;
            submitForAnalysis();
        }).fail(function(err){
            showAlert((err && err.message) || 'Please save the last section before submitting for analysis.', 'danger');
        });
    });

    if(!complaintId){ showAlert('Complaint ID is missing. Open this page from Task List.', 'danger'); return; }

    loadComplaint().done(function(){
        loadStepData(8).always(function(){ renderCurrent(); });
    }).fail(function(){ showAlert('Failed to load inquiry wizard data.', 'danger'); renderCurrent(true); });
});

