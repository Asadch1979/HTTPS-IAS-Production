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

    function selectedEngagementId() {
        return selector.value || '';
    }

    function showAlert(show) {
        if (!requiredAlert) {
            return;
        }
        requiredAlert.classList.toggle('d-none', !show);
    }

    function updateCounter(stepNo) {
        if (!stepCounter) {
            return;
        }

        stepCounter.textContent = 'Step ' + stepNo + ' of 5';
    }

    function executeInlineScripts(container) {
        container.querySelectorAll('script').forEach(function (script) {
            var newScript = document.createElement('script');
            Array.from(script.attributes).forEach(function (attr) {
                newScript.setAttribute(attr.name, attr.value);
            });
            if (!newScript.src) {
                newScript.textContent = script.textContent;
            }
            script.parentNode.replaceChild(newScript, script);
        });
    }

    function clearContent(message) {
        stepHost.innerHTML = '<div class="alert alert-info mb-0">' + message + '</div>';
    }

    function setEngagementLockedState(isLocked) {
        selector.disabled = !!isLocked;
        if (changeEngagementButton) {
            changeEngagementButton.classList.toggle('d-none', !isLocked);
        }
    }

    function syncStepperAvailability(hasSelection) {
        stepper.querySelectorAll('.step-pill').forEach(function (anchor) {
            anchor.classList.toggle('disabled', !hasSelection);
        });
    }

    function loadStep(stepCode, stepNo) {
        var engId = lockedEngagementId || selectedEngagementId();
        if (!engId) {
            showAlert(true);
            clearContent('Please select an engagement before opening workflow tabs.');
            return;
        }

        showAlert(false);
        stepHost.innerHTML = '<div class="alert alert-secondary mb-0">Loading workflow content...</div>';

        var readOnly = stepCode === 'CHECKING_DRAFT_REPORT' || stepCode === 'CHECKING_QUALITY_REVIEW';
        var loadUrl = stepHost.getAttribute('data-load-url') || '/FieldAudit/LoadBackOfficeStep';
        var url = loadUrl + '?stepCode=' + encodeURIComponent(stepCode) + '&engId=' + encodeURIComponent(engId) + '&isReadOnly=' + (readOnly ? 'true' : 'false') + '&_=' + Date.now();

        fetch(url, {
            method: 'GET',
            credentials: 'same-origin',
            cache: 'no-store'
        })
            .then(function (response) {
                if (!response.ok) {
                    throw new Error('Failed to load Back Office step.');
                }
                return response.text();
            })
            .then(function (html) {
                stepHost.innerHTML = html;
                executeInlineScripts(stepHost);
                activeStepCode = stepCode;
                updateCounter(stepNo || 1);
                stepper.querySelectorAll('.step-pill').forEach(function (anchor) {
                    anchor.classList.toggle('active', (anchor.getAttribute('data-step-code') || '') === stepCode);
                });
            })
            .catch(function () {
                clearContent('Unable to load workflow content right now. Please try again.');
            });
    }

    function lockAndLoadCurrentStep() {
        lockedEngagementId = selectedEngagementId();
        var hasSelection = !!lockedEngagementId;
        showAlert(!hasSelection);
        syncStepperAvailability(hasSelection);

        if (!hasSelection) {
            setEngagementLockedState(false);
            clearContent('Select an engagement from the dropdown above to load Back Office workflow content.');
            return;
        }

        setEngagementLockedState(true);
        var active = stepper.querySelector('.step-pill.active') || stepper.querySelector('.step-pill[data-step-code="DRAFT_REPORT"]');
        loadStep(active ? active.getAttribute('data-step-code') : 'DRAFT_REPORT', active ? active.getAttribute('data-step-no') : 1);
    }

    function unlockEngagementSelection() {
        lockedEngagementId = '';
        setEngagementLockedState(false);
        showAlert(false);
        syncStepperAvailability(true);
    }

    function initStepper() {
        var steps = Array.isArray(window.fieldAuditBoStepperData) ? window.fieldAuditBoStepperData : [];
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
        if (selector.disabled) {
            return;
        }
        lockAndLoadCurrentStep();
    });

    if (changeEngagementButton) {
        changeEngagementButton.addEventListener('click', function () {
            unlockEngagementSelection();
            selector.focus();
        });
    }

    window.fieldAuditBoDashboard = {
        loadStepContent: loadStep,
        unlockEngagementSelection: unlockEngagementSelection,
        getLockedEngagementId: function () { return lockedEngagementId; }
    };

    initStepper();
    loadEngagements();
    setEngagementLockedState(false);
    showAlert(true);
    syncStepperAvailability(false);
})();
