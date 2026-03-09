(function (window, document) {
    'use strict';

    function buildStepUrl(template, stepCode) {
        if (!template) {
            return '#';
        }

        return template.replace(/__STEP_CODE__/g, encodeURIComponent(stepCode || ''));
    }

    function applyStepClasses(step, currentStepCode, disabled) {
        var classes = ['step-pill'];
        if ((step.stepCode || '') === (currentStepCode || '')) {
            classes.push('active');
        }
        if (step.isCompleted) {
            classes.push('completed');
        }
        if (!step.isSaved) {
            classes.push('not-saved');
        }
        if (disabled) {
            classes.push('disabled');
        }

        return classes.join(' ');
    }

    function render(config) {
        if (!config || !config.containerId) {
            return;
        }

        var container = document.getElementById(config.containerId);
        if (!container) {
            return;
        }

        var steps = Array.isArray(config.steps) ? config.steps : [];
        var currentStepCode = config.currentStepCode || '';
        var disabled = !!config.disabled;
        var linkMode = config.linkMode || 'button';
        var onStepClick = typeof config.onStepClick === 'function' ? config.onStepClick : null;

        var html = steps.map(function (step) {
            var classes = applyStepClasses(step, currentStepCode, disabled);
            var href = linkMode === 'link'
                ? buildStepUrl(config.urlTemplate || '#', step.stepCode)
                : 'javascript:void(0);';

            return '' +
                '<a class="' + classes + '" href="' + href + '" ' +
                'data-step-code="' + (step.stepCode || '') + '" ' +
                'data-step-no="' + (step.stepNo || '') + '" ' +
                'data-step-title="' + (step.stepTitle || '') + '">' +
                '<div class="step-title"><span class="num">' + (step.stepNo || '') + '.</span> ' + (step.stepTitle || '') + '</div>' +
                '</a>';
        }).join('');

        container.classList.add('fa-stepper-track');
        container.innerHTML = html;

        if (onStepClick) {
            container.querySelectorAll('.step-pill[data-step-code]').forEach(function (anchor) {
                anchor.addEventListener('click', function (event) {
                    event.preventDefault();
                    if (anchor.classList.contains('disabled')) {
                        return;
                    }

                    onStepClick(anchor, event);
                });
            });
        }
    }

    window.fieldAuditStepperTheme = {
        render: render
    };
})(window, document);
