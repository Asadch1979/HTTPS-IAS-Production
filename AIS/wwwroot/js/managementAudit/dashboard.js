(function () {
    function byId(id) { return document.getElementById(id); }

    var selector = byId('maEngagementSelector');
    var changeEngagementBtn = byId('maChangeEngagementBtn');
    var stepHost = byId('maStepHost');
    var stepper = byId('maWizardStepper');
    var stepCounter = byId('maStepCounter');
    var engagementAlert = byId('maEngagementRequiredAlert');
    var stepMessageHost = byId('maStepMessage');
    var markCompletedBtn = byId('maMarkCompletedBtn');
    var postJoiningStepCodes = ['CLOSING', 'MANAGE_OBSERVATIONS', 'QUALITY_REVIEW_HO', 'ISSUE_REPORT', 'EXIT_AUDIT', 'MANAGEMENT_REPORT'];
    var closingPerformedDisabledStepCodes = ['JOIN', 'CLOSING', 'EXIT_AUDIT'];

    if (!selector || !stepHost || !stepper) {
        return;
    }

    function selectedEngagementId() {
        return selector.value || '';
    }

    function getAppBaseUrl() {
        var base = (window.g_asiBaseURL || '').toString().trim();
        if (!base) {
            var meta = document.querySelector('meta[name="base-url"]');
            base = meta ? (meta.getAttribute('content') || '') : '';
        }

        if (!base || base === '/') {
            return '';
        }

        if (base.charAt(0) !== '/') {
            base = '/' + base;
        }

        return base.replace(/\/+$/, '');
    }

    function resolveAppUrl(url) {
        var value = (url || '').toString().trim();
        var base = getAppBaseUrl();

        if (!value) {
            return base || '';
        }

        if (/^https?:\/\//i.test(value)) {
            return value;
        }

        if (value.charAt(0) === '/') {
            if (base && value !== base && value.indexOf(base + '/') !== 0) {
                return base + value;
            }

            return value;
        }

        return (base ? base + '/' : '/') + value.replace(/^\/+/, '');
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

    function showStepMessage(message, type) {
        if (!stepMessageHost) {
            return;
        }

        if (!message) {
            clearStepMessage();
            return;
        }

        stepMessageHost.className = 'alert alert-' + (type || 'warning');
        stepMessageHost.textContent = message;
    }

    function clearStepMessage() {
        if (!stepMessageHost) {
            return;
        }

        stepMessageHost.className = 'alert d-none';
        stepMessageHost.textContent = '';
    }

    function destroyStepDataTables(container) {
        if (!container || !window.jQuery || !$.fn || !$.fn.DataTable) {
            return;
        }

        container.querySelectorAll('table').forEach(function (table) {
            if (!table || !table.id) {
                return;
            }

            var selectorValue = '#' + table.id;
            if ($.fn.DataTable.isDataTable(selectorValue)) {
                $(selectorValue).DataTable().clear().destroy(true);
            }
        });
    }

    function toggleEngagementAlert(isVisible) {
        if (!engagementAlert) {
            return;
        }

        engagementAlert.classList.toggle('d-none', !isVisible);
    }

    function selectedEngagementOption() {
        return selector.options && selector.selectedIndex >= 0
            ? selector.options[selector.selectedIndex]
            : null;
    }

    function selectedEngagementState() {
        var option = selectedEngagementOption();
        var rawStatusId = option ? option.getAttribute('data-status-id') : '';

        return {
            hasEngagement: !!selectedEngagementId(),
            engPlanId: parseInt(selectedEngagementId() || '0', 10) || 0,
            statusId: parseInt(rawStatusId || '0', 10) || 0,
            isTeamLead: ((option && option.getAttribute('data-is-team-lead')) || 'N').toUpperCase(),
            display: (option && (option.getAttribute('data-display') || option.textContent)) || ''
        };
    }

    function broadcastEngagementState(state) {
        document.dispatchEvent(new CustomEvent('managementAudit:engagement-state-changed', {
            detail: state || selectedEngagementState()
        }));
    }

    function arrayContains(values, candidate) {
        return values.indexOf((candidate || '').toUpperCase()) >= 0;
    }

    function resolveStepAccess(stepCode, state) {
        if (!state || !state.hasEngagement) {
            return {
                enabled: false,
                message: 'Please select an engagement before opening workflow steps.'
            };
        }

        var normalizedStepCode = (stepCode || '').toUpperCase();
        if (normalizedStepCode === 'JOIN') {
            return {
                enabled: state.statusId === 1,
                message: state.statusId === 1 ? '' : 'Joining is not available for the selected engagement.'
            };
        }

        if (normalizedStepCode === 'EXIT_AUDIT') {
            return {
                enabled: state.statusId === 2,
                message: state.statusId === 2 ? '' : 'Closing is not available for the selected engagement.'
            };
        }

        if (state.statusId <= 1 && arrayContains(postJoiningStepCodes, normalizedStepCode)) {
            return {
                enabled: false,
                message: 'Submit joining first.'
            };
        }

        if (state.statusId === 5 && arrayContains(closingPerformedDisabledStepCodes, normalizedStepCode)) {
            return {
                enabled: false,
                message: 'This step is disabled after closing is performed.'
            };
        }

        return {
            enabled: true,
            message: ''
        };
    }

    function applyStepAvailability() {
        var state = selectedEngagementState();
        stepper.querySelectorAll('.step-pill').forEach(function (anchor) {
            var access = resolveStepAccess(anchor.getAttribute('data-step-code'), state);
            anchor.classList.toggle('disabled', !access.enabled);
            anchor.setAttribute('data-step-enabled', access.enabled ? 'true' : 'false');
            anchor.setAttribute('data-disabled-message', access.message || '');
        });
    }

    function firstAvailableStepAnchor() {
        var anchors = stepper.querySelectorAll('.step-pill');
        for (var index = 0; index < anchors.length; index += 1) {
            if (!anchors[index].classList.contains('disabled')) {
                return anchors[index];
            }
        }

        return null;
    }

    function updateSelectedEngagementState(state) {
        var option = selectedEngagementOption();
        if (!option || !state) {
            return;
        }

        if (state.statusId !== undefined && state.statusId !== null) {
            option.setAttribute('data-status-id', state.statusId);
        }

        if (state.isTeamLead !== undefined && state.isTeamLead !== null) {
            option.setAttribute('data-is-team-lead', state.isTeamLead);
        }

        if (state.display) {
            option.setAttribute('data-display', state.display);
            option.textContent = state.display;
        }

        applyStepAvailability();
        broadcastEngagementState(selectedEngagementState());

        var activeAnchor = stepper.querySelector('.step-pill.active');
        if (activeAnchor && activeAnchor.classList.contains('disabled')) {
            var nextAnchor = firstAvailableStepAnchor();
            if (nextAnchor) {
                loadStepContent(nextAnchor.getAttribute('data-step-code'), nextAnchor.getAttribute('data-step-no'));
            }
        }
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
        var scripts = Array.prototype.slice.call(container.querySelectorAll('script'));
        return scripts.reduce(function (chain, script) {
            return chain.then(function () {
                return new Promise(function (resolve, reject) {
                    var newScript = document.createElement('script');
                    Array.from(script.attributes).forEach(function (attr) {
                        newScript.setAttribute(attr.name, attr.value);
                    });

                    if (newScript.src) {
                        newScript.async = false;
                        newScript.onload = function () { resolve(); };
                        newScript.onerror = function () { reject(new Error('Failed to load script: ' + newScript.src)); };
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

    function openManagementReport(engId) {
        var reportUrl = resolveAppUrl(stepHost.getAttribute('data-management-report-url') || 'ManagementAudit/OpenManagementReport');
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

        clearStepMessage();
        toggleEngagementAlert(false);
        destroyStepDataTables(stepHost);
        stepHost.innerHTML = '<div class="alert alert-secondary mb-0">Loading workflow content...</div>';

        var loadUrl = resolveAppUrl(stepHost.getAttribute('data-load-url') || 'ManagementAudit/LoadStep');
        var requestUrl = loadUrl + '?stepCode=' + encodeURIComponent(stepCode) + '&engId=' + encodeURIComponent(engId) + '&_=' + Date.now();

        fetch(requestUrl, {
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
                    return response.text().then(function (message) {
                        throw new Error(message || 'Failed to load step content.');
                    });
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
                stepHost.setAttribute('data-eng-id', engId);
                return executeInlineScripts(stepHost).then(function () {
                    setCurrentStepCode(stepCode);
                    setActiveStep(stepCode);
                    updateStepCounter(stepNo);
                });
            })
            .catch(function (error) {
                showStepMessage((error && error.message) || 'Unable to load workflow content right now. Please try again.', 'warning');
                clearStepContent('Unable to load workflow content right now. Please try again.');
            });
    }

    function reloadCurrentStepContent() {
        var activeAnchor = stepper.querySelector('.step-pill.active');
        var stepCode = currentStepCode() || (activeAnchor && activeAnchor.getAttribute('data-step-code')) || '';
        var stepNo = (activeAnchor && activeAnchor.getAttribute('data-step-no')) || '1';
        if (!stepCode || !selectedEngagementId()) {
            return;
        }

        loadStepContent(stepCode, stepNo);
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

                if (anchor.classList.contains('disabled')) {
                    showStepMessage(anchor.getAttribute('data-disabled-message') || 'This step is not available right now.', 'warning');
                    return;
                }

                loadStepContent(anchor.getAttribute('data-step-code'), anchor.getAttribute('data-step-no'));
            }
        });

        applyStepAvailability();
    }

    if (markCompletedBtn) {
        markCompletedBtn.addEventListener('click', function () {
            var stepCode = currentStepCode();
            var engId = selectedEngagementId();
            if (!stepCode || !engId) {
                return;
            }

            var tokenInput = document.querySelector('#maCsrfForm input[name="__RequestVerificationToken"]');
            var formData = new URLSearchParams();
            formData.append('stepCode', stepCode);
            formData.append('engId', engId);
            if (tokenInput && tokenInput.value) {
                formData.append('__RequestVerificationToken', tokenInput.value);
            }

            fetch(resolveAppUrl(getActionUrl('data-mark-complete-url', 'ManagementAudit/MarkStepCompleted')), {
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
            clearStepMessage();
            clearStepContent('Select an engagement from the dropdown above to load workflow content.');
            applyStepAvailability();
            broadcastEngagementState(selectedEngagementState());
            return;
        }

        applyStepAvailability();
        broadcastEngagementState(selectedEngagementState());
        stepHost.setAttribute('data-eng-id', engId);

        var activeAnchor = stepper.querySelector('.step-pill.active');
        var targetAnchor = activeAnchor && !activeAnchor.classList.contains('disabled')
            ? activeAnchor
            : (firstAvailableStepAnchor() || stepper.querySelector('.step-pill'));
        var targetStepCode = (targetAnchor && targetAnchor.getAttribute('data-step-code')) || currentStepCode();
        var targetStepNo = (targetAnchor && targetAnchor.getAttribute('data-step-no')) || '1';
        if (targetAnchor && !targetAnchor.classList.contains('disabled') && targetStepCode) {
            loadStepContent(targetStepCode, targetStepNo);
        } else {
            clearStepContent('No workflow steps are available for the selected engagement right now.');
        }
    });

    if (changeEngagementBtn) {
        changeEngagementBtn.addEventListener('click', function () {
            selector.disabled = false;
            changeEngagementBtn.classList.add('d-none');
        });
    }

    function refreshEngagementState() {
        var engId = selectedEngagementId();
        if (!engId) {
            return Promise.resolve(null);
        }

        var stateUrl = resolveAppUrl(stepHost.getAttribute('data-engagement-state-url') || 'ManagementAudit/GetDashboardEngagementState');
        return fetch(stateUrl + '?engId=' + encodeURIComponent(engId) + '&_=' + Date.now(), {
            method: 'GET',
            credentials: 'same-origin',
            cache: 'no-store'
        })
            .then(function (response) {
                if (!response.ok) {
                    throw new Error('Failed to refresh engagement state.');
                }

                return response.json();
            })
            .then(function (state) {
                updateSelectedEngagementState(state);
                return state;
            })
            .catch(function () {
                return null;
            });
    }

    window.managementAuditDashboard = {
        loadStepContent: loadStepContent,
        reloadCurrentStepContent: reloadCurrentStepContent,
        refreshEngagementState: refreshEngagementState,
        updateEngagementState: updateSelectedEngagementState,
        getSelectedEngagementState: selectedEngagementState
    };

    window.fieldAuditDashboard = window.managementAuditDashboard;

    initStepperTheme();
    applyStepAvailability();
    broadcastEngagementState(selectedEngagementState());
    toggleEngagementAlert(!selectedEngagementId());

    if (selectedEngagementId() && currentStepCode()) {
        var activeAnchor = stepper.querySelector('.step-pill[data-step-code="' + currentStepCode() + '"]');
        if (activeAnchor && !activeAnchor.classList.contains('disabled')) {
            loadStepContent(currentStepCode(), activeAnchor.getAttribute('data-step-no'));
        } else {
            var nextAnchor = firstAvailableStepAnchor();
            if (nextAnchor) {
                loadStepContent(nextAnchor.getAttribute('data-step-code'), nextAnchor.getAttribute('data-step-no'));
            }
        }
    }
})();
