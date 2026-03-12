(function () {
    function byId(id) { return document.getElementById(id); }

    var selector = byId('maEngagementSelector');
    var stepHost = byId('maStepHost');
    var stepper = byId('maWizardStepper');
    var stepCounter = byId('maStepCounter');
    var engagementAlert = byId('maEngagementRequiredAlert');

    if (!selector || !stepHost || !stepper) {
        return;
    }

    function selectedEngagementId() {
        return selector.value || '';
    }

    function currentStepCode() {
        return stepHost.getAttribute('data-step-code') || '';
    }

    function setCurrentStepCode(stepCode) {
        stepHost.setAttribute('data-step-code', stepCode || '');
    }

    function clearStepContent(message) {
        stepHost.innerHTML = '<div class="alert alert-info mb-0">' + message + '</div>';
    }

    function toggleEngagementAlert(isVisible) {
        if (!engagementAlert) {
            return;
        }

        engagementAlert.classList.toggle('d-none', !isVisible);
    }

    function setActiveStep(stepCode) {
        stepper.querySelectorAll('.step-pill').forEach(function (anchor) {
            if ((anchor.getAttribute('data-step-code') || '') === (stepCode || '')) {
                anchor.classList.add('active');
            } else {
                anchor.classList.remove('active');
            }
        });
    }

    function updateStepCounter(stepNo) {
        if (!stepCounter) {
            return;
        }

        var total = stepper.querySelectorAll('.step-pill').length;
        var resolved = parseInt(stepNo || '1', 10);
        if (!resolved || resolved < 1) {
            resolved = 1;
        }

        stepCounter.textContent = 'Step ' + resolved + ' of ' + total;
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

    function openManagementReport(engId) {
        var reportUrl = stepHost.getAttribute('data-management-report-url') || '/ManagementAudit/OpenManagementReport';
        window.location.href = reportUrl + '?engId=' + encodeURIComponent(engId);
    }

    function loadStepContent(stepCode, stepNo) {
        var engId = selectedEngagementId();
        if (!engId) {
            toggleEngagementAlert(true);
            clearStepContent('Select an engagement from the dropdown above to load workflow content.');
            setCurrentStepCode('');
            return;
        }

        if (stepCode === 'MANAGEMENT_REPORT') {
            openManagementReport(engId);
            return;
        }

        toggleEngagementAlert(false);
        stepHost.innerHTML = '<div class="alert alert-secondary mb-0">Loading workflow content...</div>';

        var loadUrl = stepHost.getAttribute('data-load-url') || '/ManagementAudit/LoadStep';
        var requestUrl = loadUrl + '?stepCode=' + encodeURIComponent(stepCode) + '&engId=' + encodeURIComponent(engId) + '&_=' + Date.now();

        fetch(requestUrl, {
            method: 'GET',
            credentials: 'same-origin',
            cache: 'no-store'
        })
            .then(function (response) {
                if (!response.ok) {
                    throw new Error('Failed to load step content.');
                }
                return response.text();
            })
            .then(function (html) {
                stepHost.innerHTML = html;
                executeInlineScripts(stepHost);
                stepHost.setAttribute('data-eng-id', engId);
                setCurrentStepCode(stepCode);
                setActiveStep(stepCode);
                updateStepCounter(stepNo);
            })
            .catch(function () {
                clearStepContent('Unable to load workflow content right now. Please try again.');
            });
    }

    function initStepperTheme() {
        var stepData = Array.isArray(window.managementAuditStepperData) ? window.managementAuditStepperData : [];
        if (!window.fieldAuditStepperTheme || !window.fieldAuditStepperTheme.render || !stepData.length) {
            return;
        }

        window.fieldAuditStepperTheme.render({
            containerId: 'maWizardStepper',
            steps: stepData,
            currentStepCode: currentStepCode(),
            disabled: !selectedEngagementId(),
            linkMode: 'button',
            onStepClick: function (anchor) {
                if (!selectedEngagementId()) {
                    toggleEngagementAlert(true);
                    clearStepContent('Please select an engagement before opening workflow steps.');
                    return;
                }

                loadStepContent(anchor.getAttribute('data-step-code'), anchor.getAttribute('data-step-no'));
            }
        });
    }

    selector.addEventListener('change', function () {
        var engId = selectedEngagementId();
        if (!engId) {
            toggleEngagementAlert(true);
            clearStepContent('Select an engagement from the dropdown above to load workflow content.');
            stepper.querySelectorAll('.step-pill').forEach(function (anchor) {
                anchor.classList.add('disabled');
            });
            return;
        }

        var dashboardBaseUrl = selector.getAttribute('data-dashboard-base-url') || '/ManagementAudit/MA_Dashboard';
        window.location.href = dashboardBaseUrl + '?engId=' + encodeURIComponent(engId);
    });

    initStepperTheme();
    toggleEngagementAlert(!selectedEngagementId());

    if (selectedEngagementId() && currentStepCode()) {
        var activeAnchor = stepper.querySelector('.step-pill[data-step-code="' + currentStepCode() + '"]');
        if (activeAnchor) {
            loadStepContent(currentStepCode(), activeAnchor.getAttribute('data-step-no'));
        }
    }
})();
