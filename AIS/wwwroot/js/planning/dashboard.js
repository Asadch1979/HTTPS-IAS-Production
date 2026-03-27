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
            cache: 'no-store',
            headers: {
                'X-Requested-With': 'XMLHttpRequest'
            }
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
                    throw new Error('Failed to load planning step content.');
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



    function loadChildStep(stepKey, childKey, options) {
        var loadUrl = '/Planning/LoadPlanningChildStep';
        var query = new URLSearchParams();
        query.append('stepKey', stepKey || '');
        query.append('childKey', childKey || '');
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
            cache: 'no-store',
            headers: {
                'X-Requested-With': 'XMLHttpRequest'
            }
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
                    throw new Error('Failed to load planning child view.');
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
                executeInlineScripts(stepHost);
                setCurrentStepCode(stepKey);
                setActiveStep(stepKey);
                updateStepCounter('5');
            })
            .catch(function () {
                stepHost.innerHTML = '<div class="alert alert-danger mb-0">Unable to load planning workflow content right now. Please try again.</div>';
            });
    }

    function loadSubChildStep(stepKey, childKey, actionKey, options) {
        var loadUrl = '/Planning/LoadPlanningSubChildStep';
        var query = new URLSearchParams();
        query.append('stepKey', stepKey || '');
        query.append('childKey', childKey || '');
        query.append('actionKey', actionKey || '');

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
            cache: 'no-store',
            headers: {
                'X-Requested-With': 'XMLHttpRequest'
            }
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
                    throw new Error('Failed to load planning sub-child view.');
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
                executeInlineScripts(stepHost);
                setCurrentStepCode(stepKey);
                setActiveStep(stepKey);
                updateStepCounter('5');
            })
            .catch(function () {
                stepHost.innerHTML = '<div class="alert alert-danger mb-0">Unable to load planning workflow content right now. Please try again.</div>';
            });
    }

    window.planningDashboard = {
        loadStep: loadStep,
        loadChildStep: loadChildStep,
        loadSubChildStep: loadSubChildStep,
        loadNestedView: function (viewCode, options) {
            if ((viewCode || '').toUpperCase() === 'TENTATIVE_ENGAGEMENT_PLAN') {
                loadSubChildStep('AUDIT_PLAN', 'ENGAGEMENT_PLAN', 'CREATE', options);
                return;
            }

            loadChildStep('AUDIT_PLAN', viewCode, options);
        }
    };

    if (currentStepCode()) {
        var activeAnchor = stepper.querySelector('.step-pill[data-step-code="' + currentStepCode() + '"]');
        loadStep(currentStepCode(), activeAnchor ? activeAnchor.getAttribute('data-step-no') : '1');
    }
})();
