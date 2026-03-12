(function () {
    function byId(id) { return document.getElementById(id); }

    var selector = byId('executionDashboardEngagementSelector');
    var stepHost = byId('executionStepHost');
    var stepper = byId('executionWizardStepper');
    var stepCounter = byId('executionStepCounter');
    var engagementAlert = byId('executionEngagementRequiredAlert');
    var steps = Array.isArray(window.executionDashboardSteps) ? window.executionDashboardSteps : [];
    var activeStepCode = steps.length ? steps[0].stepCode : '';

    if (!selector || !stepHost || !stepper || !steps.length) {
        return;
    }

    function setAlert(show) {
        engagementAlert.classList.toggle('d-none', !show);
    }

    function setMessage(text, type) {
        stepHost.innerHTML = '<div class="alert alert-' + (type || 'info') + ' mb-0">' + text + '</div>';
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

    function setStepCounter(stepNo) {
        stepCounter.textContent = 'Step ' + stepNo + ' of ' + steps.length;
    }

    function activateStep(stepCode) {
        activeStepCode = stepCode;
        stepper.querySelectorAll('.step-pill').forEach(function (item) {
            item.classList.toggle('active', item.getAttribute('data-step-code') === stepCode);
        });
    }

    function loadStep(stepCode) {
        var engId = selector.value;
        var step = steps.find(function (s) { return s.stepCode === stepCode; });

        if (!engId) {
            setAlert(true);
            setMessage('Please select an engagement before opening workflow steps.', 'warning');
            return;
        }

        if (!step) {
            return;
        }

        setAlert(false);
        setMessage('Loading workflow content...', 'secondary');

        var loadUrl = stepHost.getAttribute('data-load-url');
        var requestUrl = loadUrl + '?step=' + encodeURIComponent(step.stepCode) + '&engId=' + encodeURIComponent(engId) + '&isReadOnly=' + encodeURIComponent(step.isReadOnly ? 'true' : 'false') + '&_=' + Date.now();

        fetch(requestUrl, {
            method: 'GET',
            credentials: 'same-origin',
            cache: 'no-store'
        })
            .then(function (response) {
                if (!response.ok) {
                    throw new Error('load failed');
                }
                return response.text();
            })
            .then(function (html) {
                stepHost.innerHTML = html;
                executeInlineScripts(stepHost);
                activateStep(step.stepCode);
                setStepCounter(step.stepNo || 1);
                if (typeof window.executionDashboardStepInit === 'function') {
                    window.executionDashboardStepInit(parseInt(engId, 10));
                }
            })
            .catch(function () {
                setMessage('Unable to load workflow content right now. Please try again.', 'danger');
            });
    }

    function loadEngagements() {
        var endpoint = selector.getAttribute('data-engagement-url');
        fetch(endpoint, {
            method: 'GET',
            credentials: 'same-origin',
            cache: 'no-store'
        })
            .then(function (response) {
                if (!response.ok) {
                    throw new Error('engagement fetch failed');
                }
                return response.json();
            })
            .then(function (rows) {
                rows.forEach(function (row) {
                    var option = document.createElement('option');
                    option.value = row.engId;
                    option.textContent = row.displayText;
                    selector.appendChild(option);
                });
            })
            .catch(function () {
                setMessage('Unable to load engagements right now.', 'danger');
            });
    }

    window.fieldAuditStepperTheme.render({
        containerId: 'executionWizardStepper',
        steps: steps.map(function (s) {
            return {
                stepCode: s.stepCode,
                stepNo: s.stepNo,
                stepTitle: s.stepTitle,
                isCompleted: false,
                isSaved: false
            };
        }),
        currentStepCode: activeStepCode,
        disabled: false,
        linkMode: 'button',
        onStepClick: function (anchor) {
            loadStep(anchor.getAttribute('data-step-code'));
        }
    });

    selector.addEventListener('change', function () {
        if (!selector.value) {
            setAlert(true);
            setMessage('Select an engagement from the dropdown above to load workflow content.', 'info');
            return;
        }

        loadStep(activeStepCode || steps[0].stepCode);
    });

    loadEngagements();
})();
