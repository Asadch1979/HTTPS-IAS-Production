(function (window) {
    function applyTheme(stepper) {
        if (!stepper) {
            return;
        }

        stepper.classList.add('stepper-theme');
        stepper.querySelectorAll('a').forEach(function (anchor) {
            anchor.classList.add('stepper-theme-step');
            var title = anchor.querySelector('.step-title');
            if (title) {
                title.classList.add('stepper-theme-title');
            }

            var number = anchor.querySelector('.num, .step-no');
            if (number) {
                number.classList.add('stepper-theme-number');
            }
        });
    }

    function setActiveStep(stepper, stepCode) {
        if (!stepper) {
            return;
        }

        stepper.querySelectorAll('[data-step-code]').forEach(function (el) {
            var isActive = (el.getAttribute('data-step-code') || '') === (stepCode || '');
            el.classList.toggle('active', isActive);
        });
    }

    function setStepCompleted(stepper, stepCode, isCompleted) {
        if (!stepper || !stepCode) {
            return;
        }

        var anchor = stepper.querySelector('[data-step-code="' + stepCode + '"]');
        if (!anchor) {
            return;
        }

        anchor.classList.toggle('completed', !!isCompleted);
        anchor.classList.toggle('not-saved', !isCompleted);
    }

    function updateCounter(counter, stepper, stepNo) {
        if (!counter || !stepper) {
            return;
        }

        var total = stepper.querySelectorAll('[data-step-code]').length;
        var resolved = parseInt(stepNo || '1', 10);
        if (!resolved || resolved < 1) {
            resolved = 1;
        }

        counter.textContent = 'Step ' + resolved + ' of ' + total;
    }

    function init(options) {
        options = options || {};
        var stepper = typeof options.stepper === 'string' ? document.querySelector(options.stepper) : options.stepper;
        if (!stepper) {
            return null;
        }

        applyTheme(stepper);

        var counter = typeof options.counter === 'string' ? document.querySelector(options.counter) : options.counter;
        if (counter && options.currentStepNo) {
            updateCounter(counter, stepper, options.currentStepNo);
        }

        return {
            stepper: stepper,
            setActiveStep: function (stepCode) { setActiveStep(stepper, stepCode); },
            setStepCompleted: function (stepCode, isCompleted) { setStepCompleted(stepper, stepCode, isCompleted); },
            updateCounter: function (stepNo) { updateCounter(counter, stepper, stepNo); }
        };
    }

    window.fieldAuditStepperTheme = {
        init: init
    };
})(window);
