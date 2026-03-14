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
    var qualityReviewStepCode = 'QUALITY_REVIEW';
    var checkingQualityReviewStepCode = 'CHECKING_QUALITY_REVIEW';
    var preConcludingScriptUrl = '/js/csp/Views_Execution_pre_concluding_audit.js?v=1';

    function selectedEngagementId() {
        return lockedEngagementId || selector.value || '';
    }

    function setEngagementLocked(locked) {
        selector.disabled = locked;
        if (changeEngagementButton) {
            changeEngagementButton.classList.toggle('d-none', !locked);
        }
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
        var scripts = Array.prototype.slice.call(container.querySelectorAll('script'));

        return scripts.reduce(function (chain, script) {
            return chain.then(function () {
                return new Promise(function (resolve) {
                    var newScript = document.createElement('script');
                    Array.from(script.attributes).forEach(function (attr) {
                        newScript.setAttribute(attr.name, attr.value);
                    });

                    if (newScript.src) {
                        newScript.onload = resolve;
                        newScript.onerror = resolve;
                    } else {
                        newScript.textContent = script.textContent;
                    }

                    script.parentNode.replaceChild(newScript, script);

                    if (!newScript.src) {
                        resolve();
                    }
                });
            });
        }, Promise.resolve());
    }

    function ensureScriptLoaded(scriptUrl) {
        var scripts = Array.prototype.slice.call(document.getElementsByTagName('script'));
        var existing = scripts.find(function (script) {
            return (script.getAttribute('src') || '').indexOf(scriptUrl) !== -1;
        });

        if (existing) {
            return Promise.resolve();
        }

        return new Promise(function (resolve) {
            var script = document.createElement('script');
            script.src = scriptUrl;
            script.async = false;
            script.onload = function () {
                script.setAttribute('data-loaded', 'true');
                resolve();
            };
            script.onerror = resolve;
            document.body.appendChild(script);
        });
    }

    function initializeBoQualityReview(engId, readOnly) {
        if (!engId) {
            return Promise.resolve();
        }

        window.fieldAuditBoContext = {
            engId: engId,
            readOnly: !!readOnly
        };

        return ensureScriptLoaded(preConcludingScriptUrl)
            .then(function () {
                return new Promise(function (resolve) {
                    var attempts = 20;

                    function run() {
                        var selector = byId('entitySelectField');
                        var hidden = byId('engIdHidden');

                        if (selector) {
                            if (!selector.querySelector('option[value="' + engId + '"]')) {
                                var option = document.createElement('option');
                                option.value = engId;
                                option.textContent = engId;
                                selector.appendChild(option);
                            }
                            selector.value = String(engId);
                            selector.setAttribute('disabled', 'disabled');
                        }

                        if (hidden) {
                            hidden.value = String(engId);
                        }

                        if (typeof window.getEntityObservations === 'function') {
                            window.getEntityObservations();
                            resolve();
                            return;
                        }

                        attempts -= 1;
                        if (attempts <= 0) {
                            resolve();
                            return;
                        }

                        setTimeout(run, 120);
                    }

                    run();
                });
            });
    }

    function clearContent(message) {
        stepHost.innerHTML = '<div class="alert alert-info mb-0">' + message + '</div>';
    }

    function loadStep(stepCode, stepNo) {
        var engId = selectedEngagementId();
        if (!engId) {
            showAlert(true);
            clearContent('Please select an engagement before opening workflow tabs.');
            return Promise.resolve();
        }

        showAlert(false);
        stepHost.innerHTML = '<div class="alert alert-secondary mb-0">Loading workflow content...</div>';

        var readOnly = stepCode === 'CHECKING_DRAFT_REPORT' || stepCode === 'CHECKING_QUALITY_REVIEW';
        var loadUrl = stepHost.getAttribute('data-load-url') || '/FieldAudit/LoadBackOfficeStep';
        var url = loadUrl + '?stepCode=' + encodeURIComponent(stepCode) + '&engId=' + encodeURIComponent(engId) + '&isReadOnly=' + (readOnly ? 'true' : 'false') + '&_=' + Date.now();

        return fetch(url, {
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
                return executeInlineScripts(stepHost);
            })
            .then(function () {
                if (stepCode !== qualityReviewStepCode && stepCode !== checkingQualityReviewStepCode) {
                    return Promise.resolve();
                }

                return initializeBoQualityReview(String(engId), readOnly);
            })
            .then(function () {
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
        var hasSelection = !!selector.value;
        showAlert(!hasSelection);
        stepper.querySelectorAll('.step-pill').forEach(function (anchor) {
            anchor.classList.toggle('disabled', !hasSelection);
        });

        if (!hasSelection) {
            lockedEngagementId = '';
            setEngagementLocked(false);
            clearContent('Select an engagement from the dropdown above to load Back Office workflow content.');
            return;
        }

        lockedEngagementId = selector.value;
        setEngagementLocked(true);

        var active = stepper.querySelector('.step-pill.active') || stepper.querySelector('.step-pill[data-step-code="DRAFT_REPORT"]');
        loadStep(active ? active.getAttribute('data-step-code') : 'DRAFT_REPORT', active ? active.getAttribute('data-step-no') : 1);
    });

    if (changeEngagementButton) {
        changeEngagementButton.addEventListener('click', function () {
            lockedEngagementId = '';
            selector.value = '';
            setEngagementLocked(false);
            showAlert(true);
            stepper.querySelectorAll('.step-pill').forEach(function (anchor) {
                anchor.classList.add('disabled');
            });
            clearContent('Select an engagement from the dropdown above to load Back Office workflow content.');
        });
    }

    window.fieldAuditBoDashboard = {
        loadStepContent: loadStep
    };

    initStepper();
    loadEngagements();
    setEngagementLocked(false);
    showAlert(true);
})();
