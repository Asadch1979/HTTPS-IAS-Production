(function (window, document) {
    function getStepElements(stepper) {
        return Array.from(stepper.querySelectorAll('.workflow-step-pill[data-step-code]'));
    }

    function updateCounter(stepper, counter, stepNo) {
        if (!stepper || !counter) {
            return;
        }

        var total = getStepElements(stepper).length;
        var resolved = parseInt(stepNo || '1', 10);
        if (!resolved || resolved < 1) {
            resolved = 1;
        }

        counter.textContent = 'Step ' + resolved + ' of ' + total;
    }

    function setActive(stepper, stepCode) {
        getStepElements(stepper).forEach(function (step) {
            step.classList.toggle('active', (step.getAttribute('data-step-code') || '') === (stepCode || ''));
        });
    }

    function setCompletionState(stepper, stepCode, isCompleted) {
        var step = stepper.querySelector('.workflow-step-pill[data-step-code="' + stepCode + '"]');
        if (!step) {
            return;
        }

        step.classList.toggle('completed', !!isCompleted);
        step.classList.toggle('not-saved', !isCompleted);
    }

    function bindStepClicks(stepper, onClick) {
        if (typeof onClick !== 'function') {
            return;
        }

        getStepElements(stepper).forEach(function (step) {
            step.addEventListener('click', function (event) {
                onClick(event, step);
            });
        });
    }

    function init(options) {
        options = options || {};
        var stepper = document.querySelector(options.stepperSelector || '#wizardStepper');
        if (!stepper) {
            return null;
        }

        var counter = options.counterSelector ? document.querySelector(options.counterSelector) : null;

        if (options.bindClicks !== false) {
            bindStepClicks(stepper, options.onStepClick);
        }

        return {
            stepper: stepper,
            setActiveStep: function (stepCode) {
                setActive(stepper, stepCode);
            },
            setStepCompleted: function (stepCode, isCompleted) {
                setCompletionState(stepper, stepCode, isCompleted);
            },
            updateCounter: function (stepNo) {
                updateCounter(stepper, counter, stepNo);
            },
            getSteps: function () {
                return getStepElements(stepper);
            }
        };
    }

    window.WorkflowStepperTheme = {
        init: init,
        updateCounter: updateCounter,
        setActive: setActive,
        setCompletionState: setCompletionState
    };
})(window, document);
