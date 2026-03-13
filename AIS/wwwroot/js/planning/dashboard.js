(function () {
    function byId(id) { return document.getElementById(id); }

    var stepHost = byId('planningStepHost');
    var stepper = byId('planningStepper');
    var stepCounter = byId('planningStepCounter');

    if (!stepHost || !stepper) {
        return;
    }

    function currentStepCode() {
        return stepHost.getAttribute('data-step-code') || '';
    }

    function setCurrentStepCode(stepCode) {
        stepHost.setAttribute('data-step-code', stepCode || '');
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

    function setActiveStep(stepCode) {
        stepper.querySelectorAll('.step-pill').forEach(function (anchor) {
            anchor.classList.toggle('active', (anchor.getAttribute('data-step-code') || '') === (stepCode || ''));
        });
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

    function loadStep(stepCode, stepNo) {
        if (!stepCode) {
            return;
        }

        stepHost.innerHTML = '<div class="alert alert-secondary mb-0">Loading workflow content...</div>';

        var loadUrl = stepHost.getAttribute('data-load-url') || '/Planning/LoadPlanningStep';
        var query = new URLSearchParams();
        query.append('stepCode', stepCode);

        var contextId = stepHost.getAttribute('data-context-id');
        var contextSecondaryId = stepHost.getAttribute('data-context-secondary-id');
        if (contextId) {
            query.append('contextId', contextId);
        }
        if (contextSecondaryId) {
            query.append('contextSecondaryId', contextSecondaryId);
        }

        fetch(loadUrl + '?' + query.toString() + '&_=' + Date.now(), {
            method: 'GET',
            credentials: 'same-origin',
            cache: 'no-store'
        })
            .then(function (response) {
                if (!response.ok) {
                    throw new Error('Failed to load planning step content.');
                }
                return response.text();
            })
            .then(function (html) {
                stepHost.innerHTML = html;
                executeInlineScripts(stepHost);
                setCurrentStepCode(stepCode);
                setActiveStep(stepCode);
                updateStepCounter(stepNo);
            })
            .catch(function () {
                stepHost.innerHTML = '<div class="alert alert-danger mb-0">Unable to load planning workflow content right now. Please try again.</div>';
            });
    }

    var stepData = Array.isArray(window.planningStepperData) ? window.planningStepperData : [];
    if (window.fieldAuditStepperTheme && window.fieldAuditStepperTheme.render && stepData.length) {
        window.fieldAuditStepperTheme.render({
            containerId: 'planningStepper',
            steps: stepData,
            currentStepCode: currentStepCode(),
            linkMode: 'button',
            onStepClick: function (anchor) {
                loadStep(anchor.getAttribute('data-step-code'), anchor.getAttribute('data-step-no'));
            }
        });
    }



    function loadNestedView(viewCode, options) {
        var loadUrl = '/Planning/LoadPlanningNestedView';
        var query = new URLSearchParams();
        query.append('viewCode', viewCode || '');
        Object.keys(options || {}).forEach(function (key) {
            var value = options[key];
            if (value !== undefined && value !== null && value !== '') {
                query.append(key, value);
            }
        });

        stepHost.innerHTML = '<div class="alert alert-secondary mb-0">Loading workflow content...</div>';
        fetch(loadUrl + '?' + query.toString() + '&_=' + Date.now(), {
            method: 'GET',
            credentials: 'same-origin',
            cache: 'no-store'
        })
            .then(function (response) {
                if (!response.ok) {
                    throw new Error('Failed to load nested planning view.');
                }
                return response.text();
            })
            .then(function (html) {
                stepHost.innerHTML = html;
                executeInlineScripts(stepHost);
                setCurrentStepCode('AUDIT_PLAN');
                setActiveStep('AUDIT_PLAN');
                updateStepCounter('5');
            })
            .catch(function () {
                stepHost.innerHTML = '<div class="alert alert-danger mb-0">Unable to load planning workflow content right now. Please try again.</div>';
            });
    }

    window.planningDashboard = {
        loadStep: loadStep,
        loadNestedView: loadNestedView
    };

    if (currentStepCode()) {
        var activeAnchor = stepper.querySelector('.step-pill[data-step-code="' + currentStepCode() + '"]');
        loadStep(currentStepCode(), activeAnchor ? activeAnchor.getAttribute('data-step-no') : '1');
    }
})();
