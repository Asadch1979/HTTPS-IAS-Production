(function () {
    function byId(id) { return document.getElementById(id); }

    var stepHost = byId('userDashboardStepHost');
    var stepper = byId('userDashboardStepper');
    var stepCounter = byId('userDashboardStepCounter');
    var storageKey = 'ais.user.dashboard.step';

    if (!stepHost || !stepper) {
        return;
    }

    function stepData() {
        return Array.isArray(window.userDashboardStepData) ? window.userDashboardStepData : [];
    }

    function normalizePath(path) {
        if (!path) {
            return '';
        }

        var normalized = path.split('?')[0].replace(/\\/g, '/');
        if (normalized.charAt(0) !== '/') {
            normalized = '/' + normalized;
        }

        return normalized.replace(/\/+$/, '').toLowerCase();
    }

    function getStoredStepKey() {
        try {
            return window.sessionStorage.getItem(storageKey) || '';
        } catch (error) {
            return '';
        }
    }

    function storeStepKey(stepKey) {
        try {
            window.sessionStorage.setItem(storageKey, stepKey || '');
        } catch (error) {
        }
    }

    function currentStepKey() {
        return stepHost.getAttribute('data-step-key') || '';
    }

    function setCurrentStepKey(stepKey) {
        stepHost.setAttribute('data-step-key', stepKey || '');
        storeStepKey(stepKey);
    }

    function updateBrowserState(stepKey) {
        var baseUrl = stepHost.getAttribute('data-base-url') || '/AdministrationPanel/User_Dashboard';
        var targetUrl = baseUrl + '?stepKey=' + encodeURIComponent(stepKey || '');
        if (window.history && typeof window.history.replaceState === 'function') {
            window.history.replaceState(null, document.title, targetUrl);
        }
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

    function setActiveStep(stepKey) {
        stepper.querySelectorAll('.step-pill').forEach(function (anchor) {
            anchor.classList.toggle('active', (anchor.getAttribute('data-step-code') || '') === (stepKey || ''));
        });
    }

    function destroyStepDataTables(container) {
        if (!container || !window.jQuery || !window.jQuery.fn || !window.jQuery.fn.DataTable) {
            return;
        }

        container.querySelectorAll('table').forEach(function (table) {
            if (!table || !table.id) {
                return;
            }

            var selector = '#' + table.id;
            if (window.jQuery.fn.DataTable.isDataTable(selector)) {
                window.jQuery(selector).DataTable().clear().destroy(true);
            }
        });
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

    function getStepByKey(stepKey) {
        return stepData().find(function (step) {
            return (step.stepCode || '').toUpperCase() === (stepKey || '').toUpperCase();
        }) || null;
    }

    function loadStep(stepKey, stepNo) {
        if (!stepKey) {
            return;
        }

        destroyStepDataTables(stepHost);
        stepHost.innerHTML = '<div class="alert alert-secondary mb-0">Loading workflow content...</div>';

        var loadUrl = stepHost.getAttribute('data-load-url') || '/AdministrationPanel/LoadUserDashboardStep';
        var query = new URLSearchParams();
        query.append('stepKey', stepKey);

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
                    throw new Error('Failed to load workflow content.');
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
                return executeInlineScripts(stepHost).then(function () {
                    setCurrentStepKey(stepKey);
                    setActiveStep(stepKey);
                    updateStepCounter(stepNo);
                    updateBrowserState(stepKey);
                });
            })
            .catch(function () {
                stepHost.innerHTML = '<div class="alert alert-danger mb-0">Unable to load workflow content right now. Please try again.</div>';
            });
    }

    function interceptLegacyLinks() {
        stepHost.addEventListener('click', function (event) {
            var anchor = event.target.closest('a[href]');
            if (!anchor) {
                return;
            }

            var href = anchor.getAttribute('href');
            var normalizedHref = normalizePath(href);
            if (!normalizedHref) {
                return;
            }

            var matchingStep = stepData().find(function (step) {
                return normalizePath(step.legacyPath) === normalizedHref;
            });

            if (!matchingStep) {
                return;
            }

            event.preventDefault();
            loadStep(matchingStep.stepCode, matchingStep.stepNo);
        });
    }

    if (window.fieldAuditStepperTheme && window.fieldAuditStepperTheme.render && stepData().length) {
        window.fieldAuditStepperTheme.render({
            containerId: 'userDashboardStepper',
            steps: stepData(),
            currentStepCode: currentStepKey(),
            linkMode: 'button',
            onStepClick: function (anchor) {
                loadStep(anchor.getAttribute('data-step-code'), anchor.getAttribute('data-step-no'));
            }
        });
    }

    interceptLegacyLinks();

    var initialStepKey = currentStepKey();
    var storedStepKey = getStoredStepKey();
    if (storedStepKey && getStepByKey(storedStepKey)) {
        initialStepKey = storedStepKey;
    }

    var initialStep = getStepByKey(initialStepKey) || stepData()[0];
    if (initialStep) {
        loadStep(initialStep.stepCode, initialStep.stepNo);
    }
})();
