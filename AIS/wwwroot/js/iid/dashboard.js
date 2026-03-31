(function (window, document) {
    'use strict';

    function byId(id) { return document.getElementById(id); }

    var root = byId('iidDashboardRoot');
    if (!root) {
        return;
    }

    var selector = byId('iidComplaintSelect');
    var selectedComplaintIdInput = byId('selectedComplaintId');
    var stepper = byId('iidDashboardStepper');
    var utilityNav = byId('iidDashboardUtilityNav');
    var contentHost = byId('iidDashboardContentHost');
    var counter = byId('iidDashboardStepCounter');
    var messageHost = byId('iidDashboardMessageHost');
    var loadPanelUrl = root.getAttribute('data-load-panel-url') || '/IID/LoadDashboardPanel';
    var steps = Array.isArray(window.iidDashboardSteps) ? window.iidDashboardSteps : [];
    var utilities = Array.isArray(window.iidDashboardUtilities) ? window.iidDashboardUtilities : [];
    var allItems = steps.concat(utilities);
    var currentItemKey = root.getAttribute('data-current-item-key') || '';
    var selectedComplaintId = parseInt(root.getAttribute('data-selected-complaint-id') || '0', 10) || 0;

    function normalizeKey(value) {
        return (value || '').toString().trim().toUpperCase();
    }

    function findItem(itemKey) {
        var normalized = normalizeKey(itemKey);
        for (var index = 0; index < allItems.length; index += 1) {
            if (normalizeKey(allItems[index].itemKey || allItems[index].ItemKey) === normalized) {
                return allItems[index];
            }
        }

        return null;
    }

    function itemKey(item) {
        return item ? (item.itemKey || item.ItemKey || '') : '';
    }

    function itemTitle(item) {
        return item ? (item.title || item.Title || '') : '';
    }

    function itemPageId(item) {
        var raw = item ? (item.requiredPermissionPageId || item.RequiredPermissionPageId || 0) : 0;
        return parseInt(raw || '0', 10) || 0;
    }

    function itemSequence(item) {
        var raw = item ? (item.sequenceNo || item.SequenceNo || 0) : 0;
        return parseInt(raw || '0', 10) || 0;
    }

    function itemRequiresComplaint(item) {
        return !!(item && (item.requiresComplaintSelection || item.RequiresComplaintSelection));
    }

    function itemReloadOnComplaintChange(item) {
        return !!(item && (item.reloadOnComplaintChange || item.ReloadOnComplaintChange));
    }

    function itemIsStep(item) {
        return !!(item && (item.isStep || item.IsStep));
    }

    function setSelectedComplaintId(value) {
        selectedComplaintId = parseInt(value || '0', 10) || 0;
        root.setAttribute('data-selected-complaint-id', String(selectedComplaintId || 0));
        if (selectedComplaintIdInput) {
            selectedComplaintIdInput.value = selectedComplaintId > 0 ? String(selectedComplaintId) : '';
        }
    }

    function showMessage(message, type) {
        if (!messageHost) {
            return;
        }

        if (!message) {
            messageHost.className = 'alert alert-warning d-none';
            messageHost.textContent = '';
            return;
        }

        messageHost.className = 'alert alert-' + (type || 'warning');
        messageHost.textContent = message;
    }

    function updateStepCounter(activeKey) {
        if (!counter) {
            return;
        }

        var active = findItem(activeKey);
        var stepNo = itemSequence(active);
        var total = steps.length;
        if (!stepNo || total <= 0) {
            counter.textContent = 'Step 1 of ' + total;
            return;
        }

        counter.textContent = 'Step ' + stepNo + ' of ' + total;
    }

    function renderUtilities() {
        if (!utilityNav) {
            return;
        }

        utilityNav.innerHTML = utilities.map(function (item) {
            return '' +
                '<button type="button" class="btn btn-outline-success iid-utility-btn" ' +
                'data-item-key="' + itemKey(item) + '" ' +
                'data-requires-complaint="' + (itemRequiresComplaint(item) ? 'true' : 'false') + '">' +
                itemTitle(item) +
                '</button>';
        }).join('');

        utilityNav.querySelectorAll('[data-item-key]').forEach(function (button) {
            button.addEventListener('click', function () {
                var targetKey = button.getAttribute('data-item-key') || '';
                var targetItem = findItem(targetKey);
                if (!targetItem) {
                    return;
                }

                if (itemRequiresComplaint(targetItem) && !selectedComplaintId) {
                    showMessage('Select a complaint first to open this work area.', 'info');
                    return;
                }

                loadPanel(targetKey);
            });
        });
    }

    function setActiveItem(itemKeyValue) {
        currentItemKey = itemKeyValue || '';
        contentHost.setAttribute('data-current-item-key', currentItemKey);
        updateStepCounter(currentItemKey);

        if (stepper) {
            stepper.querySelectorAll('.step-pill').forEach(function (anchor) {
                anchor.classList.toggle('active', normalizeKey(anchor.getAttribute('data-step-code')) === normalizeKey(currentItemKey));
            });
        }

        if (utilityNav) {
            utilityNav.querySelectorAll('[data-item-key]').forEach(function (button) {
                button.classList.toggle('active', normalizeKey(button.getAttribute('data-item-key')) === normalizeKey(currentItemKey));
            });
        }
    }

    function updateUrl(item) {
        if (!window.history || !window.history.replaceState || !item) {
            return;
        }

        var url = new URL(window.location.href);
        url.searchParams.delete('stepCode');
        url.searchParams.delete('utilityCode');

        var queryKey = itemIsStep(item) ? 'stepCode' : 'utilityCode';
        url.searchParams.set(queryKey, itemKey(item));

        if (selectedComplaintId > 0) {
            url.searchParams.set('complaintId', String(selectedComplaintId));
        } else {
            url.searchParams.delete('complaintId');
        }

        window.history.replaceState({}, '', url.toString());
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

    function buildDropdownOption(row) {
        var complaintIdValue = row && row.complaintId ? row.complaintId : '';
        var text = row && row.displayText ? row.displayText : complaintIdValue;
        return '<option value="' + complaintIdValue + '">' + text + '</option>';
    }

    function populateComplaintSelector(panel) {
        if (!selector) {
            return Promise.resolve(selectedComplaintId);
        }

        var pageId = itemPageId(panel);
        if (!pageId) {
            selector.innerHTML = '<option value="">-- Select Complaint --</option>';
            selector.disabled = true;
            setSelectedComplaintId(0);
            return Promise.resolve(0);
        }

        selector.disabled = false;

        return fetch((window.g_asiBaseURL || '') + '/ApiCalls/GetComplaintsDropdown', {
            method: 'POST',
            credentials: 'same-origin',
            cache: 'no-store',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded; charset=UTF-8',
                'X-Requested-With': 'XMLHttpRequest'
            },
            body: 'pageId=' + encodeURIComponent(pageId)
        })
            .then(function (response) {
                if (!response.ok) {
                    throw new Error('Failed to load complaints dropdown.');
                }

                return response.json();
            })
            .then(function (rows) {
                var options = Array.isArray(rows) ? rows : [];
                var resolvedComplaintId = selectedComplaintId;
                var hasSelected = options.some(function (row) {
                    return Number(row.complaintId) === Number(resolvedComplaintId);
                });

                if (!hasSelected) {
                    resolvedComplaintId = 0;
                }

                selector.innerHTML = '<option value="">-- Select Complaint --</option>' +
                    options.map(buildDropdownOption).join('');

                selector.value = resolvedComplaintId > 0 ? String(resolvedComplaintId) : '';
                setSelectedComplaintId(selector.value);
                refreshStepperAvailability();
                return selectedComplaintId;
            })
            .catch(function (error) {
                selector.innerHTML = '<option value="">-- Select Complaint --</option>';
                selector.disabled = false;
                setSelectedComplaintId(0);
                refreshStepperAvailability();
                showMessage((error && error.message) || 'Failed to load complaints dropdown.', 'warning');
                return 0;
            });
    }

    function renderEmptyState(message) {
        if (!contentHost) {
            return;
        }

        contentHost.innerHTML = '<div class="alert alert-info mb-0">' + (message || 'Nothing to display right now.') + '</div>';
    }

    function loadPanel(itemKeyValue) {
        var panel = findItem(itemKeyValue);
        if (!panel || !contentHost) {
            return;
        }

        populateComplaintSelector(panel).then(function () {
            if (itemRequiresComplaint(panel) && !selectedComplaintId) {
                setActiveItem(itemKey(panel));
                updateUrl(panel);
                renderEmptyState('Select a complaint from the dashboard selector to open this work area.');
                return;
            }

            showMessage('');
            setActiveItem(itemKey(panel));
            updateUrl(panel);
            contentHost.innerHTML = '<div class="alert alert-secondary mb-0">Loading workflow content...</div>';

            var requestUrl = loadPanelUrl +
                '?panelKey=' + encodeURIComponent(itemKey(panel)) +
                '&complaintId=' + encodeURIComponent(selectedComplaintId || 0) +
                '&_=' + Date.now();

            fetch(requestUrl, {
                method: 'GET',
                credentials: 'same-origin',
                cache: 'no-store',
                headers: {
                    'X-Requested-With': 'XMLHttpRequest'
                }
            })
                .then(function (response) {
                    if (!response.ok) {
                        return response.text().then(function (message) {
                            throw new Error(message || 'Failed to load dashboard content.');
                        });
                    }

                    return response.text();
                })
                .then(function (html) {
                    contentHost.innerHTML = html;
                    return executeInlineScripts(contentHost);
                })
                .catch(function (error) {
                    showMessage((error && error.message) || 'Unable to load dashboard content right now.', 'warning');
                    renderEmptyState('Unable to load dashboard content right now. Please try again.');
                });
        });
    }

    function renderStepper() {
        if (!stepper || !window.fieldAuditStepperTheme || !window.fieldAuditStepperTheme.render) {
            return;
        }

        window.fieldAuditStepperTheme.render({
            containerId: 'iidDashboardStepper',
            steps: steps.map(function (item) {
                return {
                    stepCode: itemKey(item),
                    stepNo: itemSequence(item),
                    stepTitle: itemTitle(item),
                    isCompleted: false,
                    isSaved: true,
                    isEnabled: !itemRequiresComplaint(item) || selectedComplaintId > 0,
                    disabledMessage: itemRequiresComplaint(item) && !selectedComplaintId
                        ? 'Select a complaint first to open this work area.'
                        : ''
                };
            }),
            currentStepCode: currentItemKey,
            linkMode: 'button',
            onStepClick: function (anchor) {
                if (anchor.classList.contains('disabled')) {
                    showMessage(anchor.getAttribute('data-disabled-message') || 'Select a complaint first to open this work area.', 'info');
                    return;
                }

                loadPanel(anchor.getAttribute('data-step-code') || '');
            }
        });
    }

    function refreshStepperAvailability() {
        if (!stepper) {
            return;
        }

        stepper.querySelectorAll('.step-pill').forEach(function (anchor) {
            var panel = findItem(anchor.getAttribute('data-step-code') || '');
            var isEnabled = panel && (!itemRequiresComplaint(panel) || selectedComplaintId > 0);
            anchor.classList.toggle('disabled', !isEnabled);
            anchor.setAttribute('data-step-enabled', isEnabled ? 'true' : 'false');
            anchor.setAttribute('data-disabled-message', isEnabled ? '' : 'Select a complaint first to open this work area.');
        });
    }

    if (selector) {
        selector.addEventListener('change', function () {
            setSelectedComplaintId(selector.value);
            refreshStepperAvailability();

            var currentPanel = findItem(currentItemKey);
            if (currentPanel && itemReloadOnComplaintChange(currentPanel)) {
                loadPanel(currentItemKey);
            } else {
                updateUrl(currentPanel);
            }
        });
    }

    renderUtilities();
    renderStepper();
    refreshStepperAvailability();

    if (!currentItemKey && steps.length) {
        currentItemKey = itemKey(steps[0]);
    }

    loadPanel(currentItemKey);
}(window, document));
