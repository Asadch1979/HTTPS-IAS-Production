(function () {
    function byId(id) { return document.getElementById(id); }

    var stepHost = byId('entityDashboardStepHost');
    var stepper = byId('entityDashboardStepper');
    var stepCounter = byId('entityDashboardStepCounter');
    var storageKey = 'ais.entity.dashboard.step';
    var selectionStorageKey = 'ais.entity.dashboard.shifting.selection';
    var stepCache = {};

    if (!stepHost || !stepper) {
        return;
    }

    function stepData() {
        return Array.isArray(window.entityDashboardStepData) ? window.entityDashboardStepData : [];
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

        if (value.indexOf('~/') === 0) {
            value = value.substring(1);
        }

        if (value.charAt(0) === '/') {
            if (base && value !== base && value.indexOf(base + '/') !== 0) {
                return base + value;
            }

            return value;
        }

        return (base ? base + '/' : '/') + value.replace(/^\/+/, '');
    }

    function setCurrentStepKey(stepKey) {
        stepHost.setAttribute('data-step-key', stepKey || '');
        storeStepKey(stepKey);
    }

    function updateBrowserState(stepKey) {
        var baseUrl = resolveAppUrl(stepHost.getAttribute('data-base-url') || '/AdministrationPanel/Entity_Dashboard');
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

    function prepareStepDataTablesForCaching(container) {
        if (!container || !window.jQuery || !window.jQuery.fn || !window.jQuery.fn.DataTable) {
            return;
        }

        container.querySelectorAll('table').forEach(function (table) {
            if (!table || !table.id) {
                return;
            }

            var selector = '#' + table.id;
            if (window.jQuery.fn.DataTable.isDataTable(selector)) {
                window.jQuery(selector).DataTable().destroy(false);
            }
        });
    }

    function captureControlState(container) {
        return Array.prototype.map.call(container.querySelectorAll('input, select, textarea'), function (control, index) {
            return {
                index: index,
                value: control.value,
                checked: !!control.checked,
                selectedIndex: control.selectedIndex
            };
        });
    }

    function restoreControlState(container, controls) {
        var currentControls = container.querySelectorAll('input, select, textarea');
        (controls || []).forEach(function (state) {
            var control = currentControls[state.index];
            if (!control || control.type === 'file') {
                return;
            }

            if (control.type === 'checkbox' || control.type === 'radio') {
                control.checked = state.checked;
            } else {
                control.value = state.value;
                if (control.tagName === 'SELECT' && state.selectedIndex >= 0 && control.value !== state.value) {
                    control.selectedIndex = state.selectedIndex;
                }
            }
        });
    }

    function captureCurrentStep() {
        var stepKey = currentStepKey();
        if (!stepKey || !stepHost.firstElementChild || stepHost.querySelector('.alert-secondary')) {
            return;
        }

        prepareStepDataTablesForCaching(stepHost);
        var adapter = window.entityDashboardStepStateAdapters
            ? window.entityDashboardStepStateAdapters[stepKey]
            : null;

        stepCache[stepKey] = {
            html: stepHost.innerHTML,
            controls: captureControlState(stepHost),
            custom: adapter && typeof adapter.capture === 'function' ? adapter.capture() : null
        };
    }

    function restoreCachedStep(stepKey, stepNo) {
        var cached = stepCache[stepKey];
        if (!cached) {
            return false;
        }

        stepHost.innerHTML = cached.html;
        executeInlineScripts(stepHost).then(function () {
            restoreControlState(stepHost, cached.controls);
            var adapter = window.entityDashboardStepStateAdapters
                ? window.entityDashboardStepStateAdapters[stepKey]
                : null;
            if (adapter && typeof adapter.restore === 'function') {
                adapter.restore(cached.custom);
            }
            setCurrentStepKey(stepKey);
            setActiveStep(stepKey);
            updateStepCounter(stepNo);
            updateBrowserState(stepKey);
        }).catch(function () {
            delete stepCache[stepKey];
            loadStep(stepKey, stepNo);
        });

        return true;
    }

    function executeInlineScripts(container) {
        var scripts = Array.prototype.slice.call(container.querySelectorAll('script'));
        return scripts.reduce(function (chain, script) {
            return chain.then(function () {
                return new Promise(function (resolve, reject) {
                    var newScript = document.createElement('script');
                    var sourceUrl = script.getAttribute('src');
                    Array.from(script.attributes).forEach(function (attr) {
                        if (attr.name === 'src') {
                            return;
                        }

                        newScript.setAttribute(attr.name, attr.value);
                    });

                    if (sourceUrl) {
                        newScript.src = resolveAppUrl(sourceUrl);
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

        if (currentStepKey() === stepKey && stepHost.firstElementChild && !stepHost.querySelector('.alert-secondary')) {
            return;
        }

        captureCurrentStep();
        if (restoreCachedStep(stepKey, stepNo)) {
            return;
        }

        stepHost.innerHTML = '<div class="alert alert-secondary mb-0">Loading workflow content...</div>';

        var loadUrl = resolveAppUrl(stepHost.getAttribute('data-load-url') || '/AdministrationPanel/LoadEntityDashboardStep');
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

    function readShiftingSelection() {
        try {
            return JSON.parse(window.sessionStorage.getItem(selectionStorageKey) || '{}') || {};
        } catch (error) {
            return {};
        }
    }

    window.entityDashboardSelectEntity = function (entityId, entityName, target) {
        var parsedId = parseInt(entityId, 10);
        if (!parsedId || parsedId <= 0 || (target !== 'from' && target !== 'to')) {
            return;
        }

        var selection = readShiftingSelection();
        selection[target] = {
            id: parsedId,
            name: entityName || ''
        };
        try {
            window.sessionStorage.setItem(selectionStorageKey, JSON.stringify(selection));
        } catch (error) {
        }

        window.dispatchEvent(new CustomEvent('entity-dashboard-shifting-selection', {
            detail: selection
        }));
    };

    window.entityDashboardGetShiftingSelection = readShiftingSelection;

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
            containerId: 'entityDashboardStepper',
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
