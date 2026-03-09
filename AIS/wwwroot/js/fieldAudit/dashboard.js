(function () {
    function byId(id) { return document.getElementById(id); }

    var selector = byId('engagementSelector');
    var stepHost = byId('fieldAuditStepHost');
    var stepper = byId('wizardStepper');
    var stepCounter = byId('stepCounter');
    var engagementAlert = byId('engagementRequiredAlert');
    var markCompletedBtn = byId('fieldAuditMarkCompletedBtn');

    if (!selector || !stepHost || !stepper) {
        return;
    }

    function selectedEngagementId() {
        return selector.value || '';
    }

    function getActionUrl(name, fallback) {
        var cardBody = stepHost.closest('.card-body');
        if (!cardBody) {
            return fallback;
        }

        return cardBody.getAttribute(name) || fallback;
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

    function setStepCompleted(stepCode, isCompleted) {
        var anchor = stepper.querySelector('.step-pill[data-step-code="' + stepCode + '"]');
        if (!anchor) {
            return;
        }

        anchor.classList.toggle('completed', !!isCompleted);
        anchor.classList.toggle('not-saved', !isCompleted);
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

    function loadStepContent(stepCode, stepNo) {
        var engId = selectedEngagementId();
        if (!engId) {
            toggleEngagementAlert(true);
            clearStepContent('Select an engagement from the dropdown above to load workflow content.');
            setCurrentStepCode('');
            return;
        }

        toggleEngagementAlert(false);
        stepHost.innerHTML = '<div class="alert alert-secondary mb-0">Loading workflow content...</div>';

        var loadUrl = stepHost.getAttribute('data-load-url') || '/FieldAudit/LoadStep';
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

    if (markCompletedBtn) {
        markCompletedBtn.addEventListener('click', function () {
            var stepCode = currentStepCode();
            var engId = selectedEngagementId();
            if (!stepCode || !engId) {
                return;
            }

            var tokenInput = document.querySelector('#fieldAuditCsrfForm input[name="__RequestVerificationToken"]');
            var formData = new URLSearchParams();
            formData.append('stepCode', stepCode);
            formData.append('engId', engId);
            if (tokenInput && tokenInput.value) {
                formData.append('__RequestVerificationToken', tokenInput.value);
            }

            fetch(getActionUrl('data-mark-complete-url', '/FieldAudit/MarkStepCompleted'), {
                method: 'POST',
                credentials: 'same-origin',
                headers: {
                    'Content-Type': 'application/x-www-form-urlencoded; charset=UTF-8'
                },
                body: formData.toString()
            })
                .then(function (response) {
                    if (!response.ok) {
                        throw new Error('Failed to mark step completed.');
                    }
                    return response.json();
                })
                .then(function (payload) {
                    if (!payload || payload.success !== true) {
                        throw new Error('Failed to mark step completed.');
                    }

                    setStepCompleted(stepCode, true);
                })
                .catch(function () {
                    alert('Unable to mark this step as completed right now.');
                });
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

        var dashboardBaseUrl = selector.getAttribute('data-dashboard-base-url') || '/FieldAudit/Dashboard';
        window.location.href = dashboardBaseUrl + '?engId=' + encodeURIComponent(engId);
    });

    stepper.querySelectorAll('.step-pill[data-step-code]').forEach(function (anchor) {
        anchor.addEventListener('click', function () {
            if (!selectedEngagementId()) {
                toggleEngagementAlert(true);
                clearStepContent('Please select an engagement before opening workflow steps.');
                return;
            }

            loadStepContent(anchor.getAttribute('data-step-code'), anchor.getAttribute('data-step-no'));
        });
    });


    window.fieldAuditDashboard = {
        loadStepContent: loadStepContent,
        loadNestedView: function (viewCode, options) {
            var engId = selectedEngagementId();
            if (!engId) {
                return;
            }

            options = options || {};
            var loadUrl = (stepHost.getAttribute('data-load-url') || '/FieldAudit/LoadStep').replace('/LoadStep', '/LoadNestedStepView');
            var query = new URLSearchParams();
            query.append('viewCode', viewCode || '');
            query.append('engId', engId);

            Object.keys(options).forEach(function (key) {
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
                        throw new Error('Failed to load nested view.');
                    }
                    return response.text();
                })
                .then(function (html) {
                    stepHost.innerHTML = html;
                    executeInlineScripts(stepHost);
                    stepHost.setAttribute('data-eng-id', engId);
                })
                .catch(function () {
                    clearStepContent('Unable to load workflow content right now. Please try again.');
                });
        }
    };

    toggleEngagementAlert(!selectedEngagementId());

    if (selectedEngagementId() && currentStepCode()) {
        var activeAnchor = stepper.querySelector('.step-pill[data-step-code="' + currentStepCode() + '"]');
        if (activeAnchor) {
            loadStepContent(currentStepCode(), activeAnchor.getAttribute('data-step-no'));
        }
    }
})();
