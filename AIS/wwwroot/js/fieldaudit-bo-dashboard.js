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
    var scriptLoadCache = {};
    var apiCallsByStep = {};
    var apiTrackerInstalled = false;

    var steps = [
        { stepCode: 'DRAFT_REPORT', stepNo: 1, stepTitle: 'Draft Report', isCompleted: false, isSaved: false },
        { stepCode: 'QUALITY_REVIEW', stepNo: 2, stepTitle: 'Quality Review', isCompleted: false, isSaved: false },
        { stepCode: 'ISSUE_REPORT', stepNo: 3, stepTitle: 'Issue Report', isCompleted: false, isSaved: false },
        { stepCode: 'CHECKING_DRAFT_REPORT', stepNo: 4, stepTitle: 'Checking of Draft Report', isCompleted: false, isSaved: false },
        { stepCode: 'CHECKING_QUALITY_REVIEW', stepNo: 5, stepTitle: 'Checking of Quality Review', isCompleted: false, isSaved: false }
    ];

    var scriptDependenciesByStep = {
        DRAFT_REPORT: [
            '/js/responsibilitySection.js',
            '/js/csp/Views_Execution_draft_audit_report_branch.js?v=1'
        ],
        CHECKING_DRAFT_REPORT: [
            '/js/responsibilitySection.js',
            '/js/csp/Views_Execution_draft_audit_report_branch.js?v=1'
        ],
        QUALITY_REVIEW: [
            '/js/responsibilitySection.js',
            '/js/csp/Views_Execution_pre_concluding_audit.js?v=1'
        ],
        CHECKING_QUALITY_REVIEW: [
            '/js/responsibilitySection.js',
            '/js/csp/Views_Execution_pre_concluding_audit.js?v=1'
        ],
        ISSUE_REPORT: [
            '/js/csp/Views_Execution_Concluding_Closing_Audit.js?v=1'
        ]
    };

    function selectedEngagementId() {
        return lockedEngagementId || selector.value || '';
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
            stepCounter.textContent = 'Step ' + stepNo + ' of 5';
        }
    }

    function clearContent(message) {
        stepHost.innerHTML = '<div class="alert alert-info mb-0">' + message + '</div>';
    }

    function normalizeApiPath(url) {
        if (!url) {
            return '';
        }

        var candidate = String(url);
        var protocolIndex = candidate.indexOf('://');
        if (protocolIndex !== -1) {
            var pathIndex = candidate.indexOf('/', protocolIndex + 3);
            candidate = pathIndex !== -1 ? candidate.substring(pathIndex) : '/';
        }

        return candidate.split('?')[0].toLowerCase();
    }

    function installApiTracker() {
        if (apiTrackerInstalled || !window.jQuery) {
            return;
        }

        apiTrackerInstalled = true;
        window.jQuery(document).ajaxSend(function (_, __, settings) {
            if (!activeStepCode || !settings || !settings.url) {
                return;
            }

            if (!apiCallsByStep[activeStepCode]) {
                apiCallsByStep[activeStepCode] = [];
            }

            apiCallsByStep[activeStepCode].push(normalizeApiPath(settings.url));
        });
    }

    function verifyStepApiCoverage(stepCode, requiredApis) {
        if (!requiredApis || !requiredApis.length) {
            return;
        }

        var observed = (apiCallsByStep[stepCode] || []).slice();
        var missing = requiredApis.filter(function (apiName) {
            return observed.indexOf(normalizeApiPath('/ApiCalls/' + apiName)) === -1;
        });

        if (missing.length) {
            console.error('Back Office step initializer did not reach required API(s).', {
                stepCode: stepCode,
                missingApis: missing,
                observedApis: observed
            });
        }
    }

    function ensureScriptLoaded(scriptUrl) {
        if (scriptLoadCache[scriptUrl]) {
            return scriptLoadCache[scriptUrl];
        }

        var scripts = Array.prototype.slice.call(document.getElementsByTagName('script'));
        var existing = scripts.find(function (script) {
            return (script.getAttribute('src') || '').indexOf(scriptUrl) !== -1;
        });

        if (existing) {
            scriptLoadCache[scriptUrl] = Promise.resolve();
            return scriptLoadCache[scriptUrl];
        }

        scriptLoadCache[scriptUrl] = new Promise(function (resolve, reject) {
            var script = document.createElement('script');
            script.src = scriptUrl;
            script.async = false;
            script.onload = resolve;
            script.onerror = function () { reject(new Error('Failed to load script: ' + scriptUrl)); };
            document.body.appendChild(script);
        });

        return scriptLoadCache[scriptUrl];
    }

    function ensureStepDependencies(stepCode) {
        var dependencies = scriptDependenciesByStep[stepCode] || [];
        return dependencies.reduce(function (chain, scriptUrl) {
            return chain.then(function () {
                return ensureScriptLoaded(scriptUrl);
            });
        }, Promise.resolve());
    }

    function applyReadOnlyMode(readOnly) {
        if (!readOnly) {
            return;
        }

        stepHost.querySelectorAll('input, select, textarea, button').forEach(function (el) {
            if (!el.classList.contains('btn-close')) {
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

    function initializeDraftStep(engId) {
        assignEngagementToPartial(engId);
        if (typeof window.fieldAuditBoLoadDraftReport !== 'function') {
            throw new Error('fieldAuditBoLoadDraftReport is required for DRAFT_REPORT/CHECKING_DRAFT_REPORT.');
        }

        window.fieldAuditBoLoadDraftReport(engId);
    }

    function initializeQualityReviewStep(engId, readOnly) {
        assignEngagementToPartial(engId);
        if (typeof window.fieldAuditBoLoadPreConcluding !== 'function') {
            throw new Error('fieldAuditBoLoadPreConcluding is required for QUALITY_REVIEW/CHECKING_QUALITY_REVIEW.');
        }

        window.fieldAuditBoLoadPreConcluding(engId, readOnly);
    }

    function initializeIssueReportStep(engId) {
        assignEngagementToPartial(engId);
        if (typeof window.fieldAuditBoLoadIssueReport !== 'function') {
            throw new Error('fieldAuditBoLoadIssueReport is required for ISSUE_REPORT.');
        }

        window.fieldAuditBoLoadIssueReport(engId);
    }

    function withReadOnlyWrapper(initializer) {
        return function (engId, readOnly) {
            initializer(engId, readOnly);
            applyReadOnlyMode(readOnly);
        };
    }

    var stepSourceMap = {
        DRAFT_REPORT: {
            readOnly: false,
            initialize: withReadOnlyWrapper(initializeDraftStep),
            requiredApis: ['get_finalized_observations_draft_branch', 'draft_report_summary']
        },
        QUALITY_REVIEW: {
            readOnly: false,
            initialize: withReadOnlyWrapper(initializeQualityReviewStep),
            requiredApis: ['get_obs_for_pre_concluding']
        },
        ISSUE_REPORT: {
            readOnly: false,
            initialize: withReadOnlyWrapper(initializeIssueReportStep),
            requiredApis: ['get_address', 'GetTeamDetails']
        },
        CHECKING_DRAFT_REPORT: {
            readOnly: true,
            initialize: withReadOnlyWrapper(initializeDraftStep),
            requiredApis: ['get_finalized_observations_draft_branch', 'draft_report_summary']
        },
        CHECKING_QUALITY_REVIEW: {
            readOnly: true,
            initialize: withReadOnlyWrapper(initializeQualityReviewStep),
            requiredApis: ['get_obs_for_pre_concluding']
        }
    };

    function initializeStep(stepCode, engId, readOnly) {
        window.fieldAuditBoContext = {
            engId: engId,
            readOnly: !!readOnly
        };

        var stepConfig = stepSourceMap[stepCode];
        if (!stepConfig || typeof stepConfig.initialize !== 'function') {
            throw new Error('No Back Office initializer configured for step: ' + stepCode);
        }

        apiCallsByStep[stepCode] = [];
        stepConfig.initialize(engId, readOnly);

        window.setTimeout(function () {
            verifyStepApiCoverage(stepCode, stepConfig.requiredApis);
        }, 1500);
    }

    function loadStep(stepCode, stepNo) {
        var engId = selectedEngagementId();
        if (!engId) {
            showAlert(true);
            clearContent('Please select an engagement before opening workflow tabs.');
            return Promise.resolve();
        }

        showAlert(false);
        stepHost.innerHTML = '<div class="alert alert-secondary mb-0">Loading workflow content...</div>';

        var stepConfig = stepSourceMap[stepCode] || null;
        var readOnly = !!(stepConfig && stepConfig.readOnly);
        var loadUrl = stepHost.getAttribute('data-load-url') || '/FieldAudit/LoadBackOfficeStep';
        var url = loadUrl + '?stepCode=' + encodeURIComponent(stepCode) + '&engId=' + encodeURIComponent(engId) + '&isReadOnly=' + (readOnly ? 'true' : 'false') + '&_=' + Date.now();

        return ensureStepDependencies(stepCode)
            .then(function () {
                installApiTracker();
                return fetch(url, {
                    method: 'GET',
                    credentials: 'same-origin',
                    cache: 'no-store'
                });
            })
            .then(function (response) {
                if (!response.ok) {
                    throw new Error('Failed to load Back Office step.');
                }
                return response.text();
            })
            .then(function (html) {
                stepHost.innerHTML = html;
                activeStepCode = stepCode;
                initializeStep(stepCode, String(engId), readOnly);
                updateCounter(stepNo || 1);
                stepper.querySelectorAll('.step-pill').forEach(function (anchor) {
                    anchor.classList.toggle('active', (anchor.getAttribute('data-step-code') || '') === stepCode);
                });
            })
            .catch(function () {
                clearContent('Unable to load workflow content right now. Please try again.');
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
                loadStep(anchor.getAttribute('data-step-code'), anchor.getAttribute('data-step-no'));
            }
        });
    }

    function loadEngagements() {
        var apiUrl = selector.getAttribute('data-api-url') || '/ApiCalls/GetBackOfficeDashboardEngagements';
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
                    var option = document.createElement('option');
                    option.value = item.engagementId;
                    option.textContent = item.label || ((item.entityName || 'Engagement') + ' (' + item.engagementId + ')');
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
            setEngagementLocked(false);
            clearContent('Select an engagement from the dropdown above to load Back Office workflow content.');
            return;
        }

        lockedEngagementId = selector.value;
        setEngagementLocked(true);

        var active = stepper.querySelector('.step-pill.active') || stepper.querySelector('.step-pill[data-step-code="DRAFT_REPORT"]');
        loadStep(active ? active.getAttribute('data-step-code') : 'DRAFT_REPORT', active ? active.getAttribute('data-step-no') : 1);
    });

    if (changeEngagementButton) {
        changeEngagementButton.addEventListener('click', function () {
            lockedEngagementId = '';
            selector.value = '';
            setEngagementLocked(false);
            showAlert(true);
            stepper.querySelectorAll('.step-pill').forEach(function (anchor) {
                anchor.classList.add('disabled');
            });
            clearContent('Select an engagement from the dropdown above to load Back Office workflow content.');
        });
    }

    window.fieldAuditBoDashboard = {
        loadStepContent: loadStep
    };

    initStepper();
    loadEngagements();
    setEngagementLocked(false);
    showAlert(true);
})();
