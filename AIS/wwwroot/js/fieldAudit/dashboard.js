(function () {
    function byId(id) { return document.getElementById(id); }

    var selector = byId('engagementSelector');
    var stepHost = byId('fieldAuditStepHost');
    var stepper = byId('wizardStepper');
    var stepCounter = byId('stepCounter');

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

    function loadStepContent(stepCode, stepNo) {
        var engId = selectedEngagementId();
        if (!engId) {
            clearStepContent('Select an engagement from the dropdown above to load workflow content.');
            setCurrentStepCode('');
            return;
        }

        stepHost.innerHTML = '<div class="alert alert-secondary mb-0">Loading workflow content...</div>';

        var loadUrl = stepHost.getAttribute('data-load-url') || '/FieldAudit/LoadStep';
        var requestUrl = loadUrl + '?stepCode=' + encodeURIComponent(stepCode) + '&engId=' + encodeURIComponent(engId);

        fetch(requestUrl, {
            method: 'GET',
            credentials: 'same-origin'
        })
            .then(function (response) {
                if (!response.ok) {
                    throw new Error('Failed to load step content.');
                }
                return response.text();
            })
            .then(function (html) {
                stepHost.innerHTML = html;
                stepHost.setAttribute('data-eng-id', engId);
                setCurrentStepCode(stepCode);
                setActiveStep(stepCode);
                updateStepCounter(stepNo);
            })
            .catch(function () {
                clearStepContent('Unable to load workflow content right now. Please try again.');
            });
    }

    selector.addEventListener('change', function () {
        var engId = selectedEngagementId();
        if (!engId) {
            clearStepContent('Select an engagement from the dropdown above to load workflow content.');
            stepper.querySelectorAll('.step-pill').forEach(function (anchor) {
                anchor.classList.add('disabled');
            });
            return;
        }

        stepper.querySelectorAll('.step-pill').forEach(function (anchor) {
            anchor.classList.remove('disabled');
        });

        var firstStep = stepper.querySelector('.step-pill[data-step-code]');
        if (!firstStep) {
            return;
        }

        loadStepContent(firstStep.getAttribute('data-step-code'), firstStep.getAttribute('data-step-no'));
    });

    stepper.querySelectorAll('.step-pill[data-step-code]').forEach(function (anchor) {
        anchor.addEventListener('click', function () {
            if (!selectedEngagementId()) {
                clearStepContent('Please select an engagement before opening workflow steps.');
                return;
            }

            loadStepContent(anchor.getAttribute('data-step-code'), anchor.getAttribute('data-step-no'));
        });
    });

    if (selectedEngagementId() && currentStepCode()) {
        var activeAnchor = stepper.querySelector('.step-pill[data-step-code="' + currentStepCode() + '"]');
        if (activeAnchor) {
            loadStepContent(currentStepCode(), activeAnchor.getAttribute('data-step-no'));
        }
    }
})();
