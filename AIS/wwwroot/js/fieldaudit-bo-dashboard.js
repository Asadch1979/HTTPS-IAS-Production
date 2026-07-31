(function () {
    function byId(id) { return document.getElementById(id); }

    var selector = byId('boEngagementSelector');
    var changeEngagementButton = byId('boChangeEngagementButton');
    var stepper = byId('boWizardStepper');
    var stepHost = byId('boStepHost');
    var stepCounter = byId('boStepCounter');
    var requiredAlert = byId('boEngagementRequiredAlert');

    if (!selector || !stepper || !stepHost) {
        return;
    }

    var activeStepCode = 'DRAFT_REPORT';
    var lockedEngagementId = '';
    var lockedEngagementStatusId = null;
    var scriptLoadCache = {};
    var steps = [
        { stepCode: 'DRAFT_REPORT', stepNo: 1, stepTitle: 'Draft Report (Branch)', isCompleted: false, isSaved: false },
        { stepCode: 'DRAFT_REPORT_HO', stepNo: 2, stepTitle: 'Draft Report (HO)', isCompleted: false, isSaved: false },
        { stepCode: 'QUALITY_REVIEW', stepNo: 3, stepTitle: 'Quality Review (Branch)', isCompleted: false, isSaved: false },
        { stepCode: 'QUALITY_REVIEW_HO', stepNo: 4, stepTitle: 'Quality Review (HO)', isCompleted: false, isSaved: false },
        { stepCode: 'ISSUE_REPORT', stepNo: 5, stepTitle: 'Issue Report', isCompleted: false, isSaved: false },
        { stepCode: 'CHECKING_DRAFT_REPORT', stepNo: 6, stepTitle: 'Checking of Draft Report', isCompleted: false, isSaved: false },
        { stepCode: 'CHECKING_QUALITY_REVIEW', stepNo: 7, stepTitle: 'Checking of Quality Review', isCompleted: false, isSaved: false }
    ];
    var visibleStepCodes = Array.isArray(window.backOfficeVisibleStepCodes) ? window.backOfficeVisibleStepCodes : null;

    if (visibleStepCodes) {
        steps = steps
            .filter(function (step) {
                return visibleStepCodes.indexOf(step.stepCode) >= 0;
            })
            .map(function (step, index) {
                return {
                    stepCode: step.stepCode,
                    stepNo: index + 1,
                    stepTitle: step.stepTitle,
                    isCompleted: !!step.isCompleted,
                    isSaved: !!step.isSaved
                };
            });
    }

    activeStepCode = steps.length ? steps[0].stepCode : '';

    function getAppBaseUrl() {
        var base = (window.g_asiBaseURL || '').toString().trim();
        if (!base) {
            var meta = document.querySelector('meta[name="base-url"]');
            base = meta ? (meta.getAttribute('content') || '') : '';
        }

        if (!base || base === '/') {
            return '';
        }

        if (base.charAt(0) !== '/') {
            base = '/' + base;
        }

        return base.replace(/\/+$/, '');
    }

    function resolveAppUrl(url) {
        var value = (url || '').toString().trim();
        var base = getAppBaseUrl();

        if (!value) {
            return base || '';
        }

        if (/^https?:\/\//i.test(value)) {
            return value;
        }

        if (value.charAt(0) === '/') {
            if (base && value !== base && value.indexOf(base + '/') !== 0) {
                return base + value;
            }

            return value;
        }

        return (base ? base + '/' : '/') + value.replace(/^\/+/, '');
    }

    var scriptDependenciesByStep = {
        DRAFT_REPORT: [
            '/js/responsibilitySection.js',
            '/js/obsreference.js?v=4',
            '/js/csp/Views_Execution_draft_audit_report_branch.js?v=4'
        ],
        DRAFT_REPORT_HO: [
            '/js/csp/Views_Execution_draft_audit_report.js?v=1'
        ],
        CHECKING_DRAFT_REPORT: [
            '/js/responsibilitySection.js',
            '/js/obsreference.js?v=4',
            '/js/csp/Views_Execution_draft_audit_report_branch.js?v=4'
        ],
        QUALITY_REVIEW: [
            '/js/responsibilitySection.js',
            '/js/obsreference.js?v=4',
            '/js/csp/Views_Execution_pre_concluding_audit.js?v=3'
        ],
        QUALITY_REVIEW_HO: [
            '/js/csp/Views_Execution_pre_concluding_audit_ho.js?v=1'
        ],
        CHECKING_QUALITY_REVIEW: [
            '/js/responsibilitySection.js',
            '/js/obsreference.js?v=4',
            '/js/csp/Views_Execution_pre_concluding_audit.js?v=3'
        ],
        ISSUE_REPORT: [
            '/js/csp/Views_Execution_Concluding_Closing_Audit.js?v=1'
        ]
    };

    function selectedEngagementId() {
        return lockedEngagementId || selector.value || '';
    }

    function selectedEngagementOption() {
        return selector.options && selector.selectedIndex >= 0
            ? selector.options[selector.selectedIndex]
            : null;
    }

    function selectedStatusId() {
        var option = selectedEngagementOption();
        var rawStatusId = lockedEngagementStatusId !== null && lockedEngagementStatusId !== undefined
            ? lockedEngagementStatusId
            : (option ? option.getAttribute('data-status-id') : '');
        return parseInt(rawStatusId || '0', 10) || 0;
    }

    function resolveStepStatusAccess(stepCode) {
        var statusId = selectedStatusId();
        var normalizedStepCode = (stepCode || '').toUpperCase();

        if (!selectedEngagementId()) {
            return {
                enabled: false,
                message: 'Please select an engagement before opening workflow steps.'
            };
        }

        if (statusId === 12 && normalizedStepCode === 'ISSUE_REPORT') {
            return {
                enabled: false,
                message: 'This step is not available for the selected engagement status.'
            };
        }

        if (statusId === 13 && (normalizedStepCode === 'DRAFT_REPORT' || normalizedStepCode === 'DRAFT_REPORT_HO' || normalizedStepCode === 'CHECKING_DRAFT_REPORT' || normalizedStepCode === 'QUALITY_REVIEW' || normalizedStepCode === 'QUALITY_REVIEW_HO')) {
            return {
                enabled: false,
                message: 'This step is not available for the selected engagement status.'
            };
        }

        return {
            enabled: true,
            message: ''
        };
    }

    function applyStepAvailability() {
        stepper.querySelectorAll('.step-pill').forEach(function (anchor) {
            var access = resolveStepStatusAccess(anchor.getAttribute('data-step-code'));
            anchor.classList.toggle('disabled', !access.enabled);
            anchor.setAttribute('data-step-enabled', access.enabled ? 'true' : 'false');
            anchor.setAttribute('data-disabled-message', access.message || '');
        });
    }

    function firstAvailableStepAnchor() {
        var anchors = stepper.querySelectorAll('.step-pill');
        for (var index = 0; index < anchors.length; index += 1) {
            if (!anchors[index].classList.contains('disabled')) {
                return anchors[index];
            }
        }

        return null;
    }

    function setEngagementLocked(locked) {
        selector.disabled = locked;
        if (changeEngagementButton) {
            changeEngagementButton.classList.toggle('d-none', !locked);
        }
    }

    function showAlert(show) {
        if (requiredAlert) {
            requiredAlert.classList.toggle('d-none', !show);
        }
    }

    function updateCounter(stepNo) {
        if (stepCounter) {
            if (!steps.length) {
                stepCounter.textContent = 'No steps available';
                return;
            }

            stepCounter.textContent = 'Step ' + stepNo + ' of ' + steps.length;
        }
    }

    function clearContent(message) {
        stepHost.innerHTML = '<div class="alert alert-info mb-0">' + message + '</div>';
    }

    function getScriptPathKey(url) {
        var anchor = document.createElement('a');
        anchor.href = resolveAppUrl(url);
        return (anchor.pathname || '').toLowerCase();
    }

    function removeLoadedScript(scriptUrl) {
        var targetPath = getScriptPathKey(scriptUrl);
        Array.prototype.slice.call(document.getElementsByTagName('script')).forEach(function (script) {
            var scriptSource = script.getAttribute('src') || '';
            if (scriptSource && getScriptPathKey(scriptSource) === targetPath) {
                script.parentNode.removeChild(script);
            }
        });
    }

    function ensureScriptLoaded(scriptUrl, forceReload) {
        var resolvedUrl = resolveAppUrl(scriptUrl);
        var cacheKey = getScriptPathKey(resolvedUrl);
        if (forceReload) {
            removeLoadedScript(resolvedUrl);
            delete scriptLoadCache[cacheKey];
            resolvedUrl += (resolvedUrl.indexOf('?') >= 0 ? '&' : '?') + 'boReload=' + Date.now();
        }

        if (scriptLoadCache[cacheKey]) {
            return scriptLoadCache[cacheKey];
        }

        var scripts = Array.prototype.slice.call(document.getElementsByTagName('script'));
        var existing = scripts.find(function (script) {
            return (script.getAttribute('src') || '') && getScriptPathKey(script.getAttribute('src')) === cacheKey;
        });

        if (existing) {
            scriptLoadCache[cacheKey] = Promise.resolve();
            return scriptLoadCache[cacheKey];
        }

        scriptLoadCache[cacheKey] = new Promise(function (resolve, reject) {
            var script = document.createElement('script');
            script.src = resolvedUrl;
            script.async = false;
            script.onload = resolve;
            script.onerror = function () { reject(new Error('Failed to load script: ' + resolvedUrl)); };
            document.body.appendChild(script);
        });

        return scriptLoadCache[cacheKey];
    }

    function shouldReloadStepScript(stepCode, scriptUrl) {
        var scriptPath = getScriptPathKey(scriptUrl);
        if (stepCode === 'DRAFT_REPORT' || stepCode === 'CHECKING_DRAFT_REPORT') {
            return scriptPath.indexOf('/views_execution_draft_audit_report_branch.js') >= 0;
        }

        if (stepCode === 'QUALITY_REVIEW' || stepCode === 'CHECKING_QUALITY_REVIEW') {
            return scriptPath.indexOf('/views_execution_pre_concluding_audit.js') >= 0;
        }

        return false;
    }

    function ensureStepDependencies(stepCode) {
        var dependencies = scriptDependenciesByStep[stepCode] || [];
        return dependencies.reduce(function (chain, scriptUrl) {
            return chain.then(function () {
                return ensureScriptLoaded(scriptUrl, shouldReloadStepScript(stepCode, scriptUrl));
            });
        }, Promise.resolve());
    }

    function applyReadOnlyMode(readOnly) {
        if (!readOnly) {
            return;
        }

        stepHost.querySelectorAll('input, select, textarea, button').forEach(function (el) {
            var isPreConcludingAction = (el.getAttribute('data-onclick') || '').indexOf('submitPreConcluding') !== -1;
            if (!el.classList.contains('btn-close') && !isPreConcludingAction) {
                el.disabled = true;
            }
        });

        stepHost.querySelectorAll('[data-onclick]').forEach(function (el) {
            el.removeAttribute('data-onclick');
        });
    }

    function assignEngagementToPartial(engId) {
        var entitySelect = byId('entitySelectField');
        if (entitySelect) {
            if (!entitySelect.querySelector('option[value="' + engId + '"]')) {
                var option = document.createElement('option');
                option.value = engId;
                option.textContent = engId;
                entitySelect.appendChild(option);
            }
            entitySelect.value = String(engId);
            entitySelect.setAttribute('disabled', 'disabled');
        }

        var hidden = byId('engIdHidden');
        if (hidden) {
            hidden.value = String(engId);
        }
    }

    function initializeDraftStep(engId, readOnly) {
        assignEngagementToPartial(engId);
        if (typeof window.fieldAuditBoLoadDraftReport !== 'function') {
            throw new Error('Missing BO initializer: fieldAuditBoLoadDraftReport');
        }

        window.fieldAuditBoLoadDraftReport(engId, readOnly);
        applyReadOnlyMode(readOnly);
    }

    function initializeQualityReviewStep(engId, readOnly) {
        assignEngagementToPartial(engId);
        if (typeof window.fieldAuditBoLoadPreConcluding !== 'function') {
            throw new Error('Missing BO initializer: fieldAuditBoLoadPreConcluding');
        }

        // Critical BO flow: _QualityReviewPartial load -> explicit pre-concluding bootstrap
        // -> getEntityObservations() -> POST /ApiCalls/get_obs_for_pre_concluding.
        window.fieldAuditBoLoadPreConcluding(engId, readOnly);
        applyReadOnlyMode(readOnly);
    }

    function initializeDraftHoStep(engId, readOnly) {
        assignEngagementToPartial(engId);
        if (typeof window.getEntityObservation !== 'function') {
            throw new Error('Missing BO initializer: getEntityObservation');
        }
        window.getEntityObservation();
        applyReadOnlyMode(readOnly);
    }

    function initializeQualityReviewHoStep(engId, readOnly) {
        assignEngagementToPartial(engId);
        if (typeof window.getEntityObservations !== 'function') {
            throw new Error('Missing BO initializer: getEntityObservations');
        }
        window.getEntityObservations();
        applyReadOnlyMode(readOnly);
    }

    function initializeIssueReportStep(engId, readOnly) {
        assignEngagementToPartial(engId);
        if (typeof window.fieldAuditBoLoadIssueReport !== 'function') {
            throw new Error('Missing BO initializer: fieldAuditBoLoadIssueReport');
        }

        window.fieldAuditBoLoadIssueReport(engId, readOnly);
        applyReadOnlyMode(readOnly);
    }

    var stepSourceMap = {
        DRAFT_REPORT: {
            readOnly: false,
            initialize: initializeDraftStep,
            requiredApis: ['get_finalized_observations_draft_branch', 'draft_report_summary']
        },
        DRAFT_REPORT_HO: {
            readOnly: false,
            initialize: initializeDraftHoStep,
            requiredApis: ['get_finalized_observations_draft']
        },
        QUALITY_REVIEW: {
            readOnly: false,
            initialize: initializeQualityReviewStep,
            requiredApis: ['get_obs_for_pre_concluding']
        },
        QUALITY_REVIEW_HO: {
            readOnly: false,
            initialize: initializeQualityReviewHoStep,
            requiredApis: ['get_obs_for_pre_concluding']
        },
        ISSUE_REPORT: {
            readOnly: false,
            initialize: initializeIssueReportStep,
            requiredApis: ['get_address', 'GetTeamDetails']
        },
        CHECKING_DRAFT_REPORT: {
            readOnly: true,
            initialize: initializeDraftStep,
            requiredApis: ['get_finalized_observations_draft_branch', 'draft_report_summary']
        },
        CHECKING_QUALITY_REVIEW: {
            readOnly: true,
            initialize: initializeQualityReviewStep,
            requiredApis: ['get_obs_for_pre_concluding']
        }
    };

    function initializeStep(stepCode, engId, readOnly) {
        window.fieldAuditBoContext = {
            engId: engId,
            statusId: selectedStatusId(),
            readOnly: !!readOnly
        };

        var stepConfig = stepSourceMap[stepCode];
        if (stepConfig && typeof stepConfig.initialize === 'function') {
            stepConfig.initialize(engId, readOnly);
        }
    }

    function loadStep(stepCode, stepNo) {
        if (!steps.length) {
            clearContent('You do not currently have access to any Back Office workflow steps.');
            updateCounter(0);
            return Promise.resolve();
        }

        var engId = selectedEngagementId();
        if (!engId) {
            showAlert(true);
            clearContent('Please select an engagement before opening workflow tabs.');
            applyStepAvailability();
            return Promise.resolve();
        }

        var statusAccess = resolveStepStatusAccess(stepCode);
        if (!statusAccess.enabled) {
            clearContent(statusAccess.message || 'This step is not available for the selected engagement status.');
            applyStepAvailability();
            return Promise.resolve();
        }

        showAlert(false);
        stepHost.innerHTML = '<div class="alert alert-secondary mb-0">Loading workflow content...</div>';

        var stepConfig = stepSourceMap[stepCode] || null;
        var readOnly = !!(stepConfig && stepConfig.readOnly);
        var loadUrl = resolveAppUrl(stepHost.getAttribute('data-load-url') || 'FieldAudit/LoadBackOfficeStep');
        var url = loadUrl + '?stepCode=' + encodeURIComponent(stepCode) + '&engId=' + encodeURIComponent(engId) + '&isReadOnly=' + (readOnly ? 'true' : 'false') + '&_=' + Date.now();

        return ensureStepDependencies(stepCode)
            .then(function () {
                return fetch(url, {
                    method: 'GET',
                    credentials: 'same-origin',
                    cache: 'no-store',
                    headers: {
                        'X-Requested-With': 'XMLHttpRequest'
                    }
                });
            })
            .then(function (response) {
                if (response.status === 403) {
                    return response.text().then(function (html) {
                        return {
                            status: 403,
                            html: html
                        };
                    });
                }

                if (!response.ok) {
                    return response.text().then(function (message) {
                        throw new Error(message || 'Failed to load Back Office step.');
                    });
                }
                return response.text().then(function (html) {
                    return {
                        status: response.status,
                        html: html
                    };
                });
            })
            .then(function (payload) {
                stepHost.innerHTML = payload.html;
                if (payload.status === 403) {
                    activeStepCode = stepCode;
                    updateCounter(stepNo || 1);
                    stepper.querySelectorAll('.step-pill').forEach(function (anchor) {
                        anchor.classList.toggle('active', (anchor.getAttribute('data-step-code') || '') === stepCode);
                    });
                    return;
                }

                initializeStep(stepCode, String(engId), readOnly);
                activeStepCode = stepCode;
                updateCounter(stepNo || 1);
                stepper.querySelectorAll('.step-pill').forEach(function (anchor) {
                    anchor.classList.toggle('active', (anchor.getAttribute('data-step-code') || '') === stepCode);
                });
            })
            .catch(function (error) {
                console.error('Back Office step load/initialize failed:', error);
                clearContent((error && error.message) || 'Unable to load workflow content right now. Please try again.');
            });
    }

    function initStepper() {
        if (!steps.length || !window.fieldAuditStepperTheme || !window.fieldAuditStepperTheme.render) {
            return;
        }

        window.fieldAuditStepperTheme.render({
            containerId: 'boWizardStepper',
            steps: steps,
            currentStepCode: activeStepCode,
            disabled: true,
            linkMode: 'button',
            onStepClick: function (anchor) {
                if (anchor.classList.contains('disabled') || anchor.getAttribute('data-step-enabled') === 'false') {
                    clearContent(anchor.getAttribute('data-disabled-message') || 'This step is not available for the selected engagement status.');
                    return;
                }

                loadStep(anchor.getAttribute('data-step-code'), anchor.getAttribute('data-step-no'));
            }
        });
    }

    function loadEngagements() {
        var apiUrl = resolveAppUrl(selector.getAttribute('data-api-url') || 'ApiCalls/GetBackOfficeDashboardEngagements');
        fetch(apiUrl, { credentials: 'same-origin', cache: 'no-store' })
            .then(function (response) {
                if (!response.ok) {
                    throw new Error('Failed to load engagements.');
                }
                return response.json();
            })
            .then(function (items) {
                selector.innerHTML = '<option value="">-- Select Engagement --</option>';
                (items || []).forEach(function (item) {
                    var engagementId = item.engagementId || item.ENG_ID || '';
                    var displayText = item.displayText || item.DISPLAY_TEXT || item.label || item.ENGAGEMENT_NAME || item.entityName || engagementId;
                    var statusId = item.statusId || item.STATUS_ID || '';
                    var statusName = item.statusName || item.STATUS_NAME || '';
                    var option = document.createElement('option');
                    option.value = engagementId;
                    option.textContent = displayText;
                    option.setAttribute('data-status-id', statusId);
                    option.setAttribute('data-status-name', statusName);
                    option.setAttribute('data-display', displayText);
                    selector.appendChild(option);
                });
            })
            .catch(function () {
                selector.innerHTML = '<option value="">-- Unable to load engagements --</option>';
            });
    }

    selector.addEventListener('change', function () {
        var hasSelection = !!selector.value;
        showAlert(!hasSelection);
        stepper.querySelectorAll('.step-pill').forEach(function (anchor) {
            anchor.classList.toggle('disabled', !hasSelection);
        });

        if (!hasSelection) {
            lockedEngagementId = '';
            lockedEngagementStatusId = null;
            setEngagementLocked(false);
            clearContent('Select an engagement from the dropdown above to load Back Office workflow content.');
            applyStepAvailability();
            return;
        }

        if (!steps.length) {
            clearContent('You do not currently have access to any Back Office workflow steps.');
            updateCounter(0);
            return;
        }

        lockedEngagementId = selector.value;
        lockedEngagementStatusId = selectedStatusId();
        setEngagementLocked(true);
        applyStepAvailability();

        var active = stepper.querySelector('.step-pill.active')
            || stepper.querySelector('.step-pill[data-step-code="' + activeStepCode + '"]')
            || stepper.querySelector('.step-pill[data-step-code="' + steps[0].stepCode + '"]');
        if (!active || active.classList.contains('disabled')) {
            active = firstAvailableStepAnchor();
        }

        if (!active) {
            clearContent('No workflow steps are available for the selected engagement right now.');
            return;
        }

        loadStep(active ? active.getAttribute('data-step-code') : steps[0].stepCode, active ? active.getAttribute('data-step-no') : steps[0].stepNo);
    });

    if (changeEngagementButton) {
        changeEngagementButton.addEventListener('click', function () {
            lockedEngagementId = '';
            lockedEngagementStatusId = null;
            selector.value = '';
            setEngagementLocked(false);
            showAlert(true);
            applyStepAvailability();
            clearContent('Select an engagement from the dropdown above to load Back Office workflow content.');
        });
    }

    window.fieldAuditBoDashboard = {
        loadStepContent: loadStep,
        getSelectedEngagementId: selectedEngagementId,
        getSelectedStatusId: selectedStatusId
    };

    initStepper();
    applyStepAvailability();
    loadEngagements();
    setEngagementLocked(false);
    showAlert(true);
    if (!steps.length) {
        clearContent('You do not currently have access to any Back Office workflow steps.');
        updateCounter(0);
    }
})();
