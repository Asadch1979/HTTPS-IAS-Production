(function () {
    'use strict';

    var app = document.getElementById('managementNotificationApp');
    if (!app) return;
    if (!window.bootstrap || typeof window.bootstrap.Modal !== 'function') {
        console.error('Bootstrap Modal is unavailable on the Management Audit notification administration page.');
        return;
    }

    var dataNode = document.getElementById('managementNotificationData');
    var divisions = dataNode ? JSON.parse(dataNode.textContent || '[]') : [];
    var divisionModal = new bootstrap.Modal(document.getElementById('divisionConfigurationModal'));
    var recipientModal = new bootstrap.Modal(document.getElementById('recipientManagementModal'));
    var selectedDivisionId = 0;
    var editingDivisionWasActive = false;
    var editingRecipientWasActive = false;

    function value(item, name) {
        if (!item) return null;
        return item[name] !== undefined ? item[name] : item[name.charAt(0).toLowerCase() + name.slice(1)];
    }

    function dateValue(input) {
        if (!input) return '';
        return String(input).slice(0, 10);
    }

    function today() {
        var now = new Date();
        var local = new Date(now.getTime() - now.getTimezoneOffset() * 60000);
        return local.toISOString().slice(0, 10);
    }

    function validateDateRange(fromId, toId) {
        var from = document.getElementById(fromId);
        var to = document.getElementById(toId);
        var invalid = !!(from.value && to.value && to.value < from.value);
        to.setCustomValidity(invalid ? 'Effective To cannot be earlier than Effective From.' : '');
        return !invalid;
    }

    function normalizeEmailList(input) {
        var raw = input.value || '';
        var parts = raw.split(';');
        var emailPattern = /^[^\s@;,]+@[^\s@;,]+\.[^\s@;,]+$/;
        var normalized = [];
        var valid = parts.length > 0 && parts.every(function (part) {
            var address = part.trim();
            if (!address || !emailPattern.test(address)) return false;
            if (!normalized.some(function (item) { return item.toLowerCase() === address.toLowerCase(); })) {
                normalized.push(address);
            }
            return true;
        });

        input.setCustomValidity(valid ? '' : 'Enter valid email addresses separated by semicolons. Empty entries are not allowed.');
        if (valid) input.value = normalized.join('; ');
        return valid;
    }

    function csrfToken() {
        var token = document.querySelector('#managementNotificationCsrf input[name="__RequestVerificationToken"]');
        return token ? token.value : '';
    }

    function showMessage(message, success) {
        var alert = document.getElementById('managementNotificationMessage');
        alert.classList.remove('d-none', 'alert-success', 'alert-danger');
        alert.classList.add(success ? 'alert-success' : 'alert-danger');
        alert.textContent = message;
        alert.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
    }

    function showFormError(form, message) {
        var alert = form.querySelector('.form-error');
        alert.textContent = message;
        alert.classList.remove('d-none');
    }

    function clearFormError(form) {
        var alert = form.querySelector('.form-error');
        alert.textContent = '';
        alert.classList.add('d-none');
    }

    function errorMessage(response, fallback) {
        if (!response) return fallback;
        if (response.errors) {
            var messages = [];
            Object.keys(response.errors).forEach(function (key) {
                var values = response.errors[key];
                if (Array.isArray(values)) messages = messages.concat(values);
            });
            if (messages.length) return messages.join(' ');
        }
        return response.message || fallback;
    }

    async function postForm(url, formData) {
        var response = await fetch(url, {
            method: 'POST',
            headers: { 'RequestVerificationToken': csrfToken() },
            body: formData,
            credentials: 'same-origin'
        });
        var payload = await response.json().catch(function () { return null; });
        if (!response.ok || !payload || !payload.status) {
            throw new Error(errorMessage(payload, 'The request could not be completed.'));
        }
        return payload;
    }

    function findDivision(divisionId) {
        return divisions.find(function (item) { return Number(value(item, 'DivisionId')) === Number(divisionId); });
    }

    function resetDivisionForm() {
        var form = document.getElementById('divisionConfigurationForm');
        var divisionOption = document.getElementById('divisionOption');
        form.reset();
        clearFormError(form);
        document.getElementById('divisionConfigurationModalTitle').textContent = 'Add Division Notification Configuration';
        divisionOption.disabled = false;
        Array.from(divisionOption.options).forEach(function (option) {
            option.disabled = !!option.value && option.dataset.configured === 'true';
        });
        divisionOption.value = '';
        document.getElementById('divisionIdValue').value = '';
        document.getElementById('divisionNameValue').value = '';
        document.getElementById('divisionIsActive').checked = true;
        document.getElementById('divisionWeeklyEnabled').checked = true;
        document.getElementById('divisionEffectiveFrom').value = today();
        editingDivisionWasActive = false;
    }

    function openDivisionEditor(divisionId) {
        resetDivisionForm();
        var division = findDivision(divisionId);
        if (!division) return;
        var divisionOption = document.getElementById('divisionOption');
        var selectedOption = Array.from(divisionOption.options).find(function (option) {
            return Number(option.value) === Number(value(division, 'DivisionId'));
        });
        if (!selectedOption) {
            showMessage('The configured Division is not available in the Division reference list.', false);
            return;
        }
        document.getElementById('divisionConfigurationModalTitle').textContent = 'Edit Division Notification Configuration';
        selectedOption.disabled = false;
        divisionOption.value = selectedOption.value;
        divisionOption.disabled = true;
        document.getElementById('divisionIdValue').value = value(division, 'DivisionId');
        document.getElementById('divisionNameValue').value = value(division, 'DivisionName') || '';
        document.getElementById('divisionIsActive').checked = !!value(division, 'IsActive');
        document.getElementById('divisionWeeklyEnabled').checked = !!value(division, 'WeeklyEmailEnabled');
        document.getElementById('divisionEffectiveFrom').value = dateValue(value(division, 'EffectiveFrom'));
        document.getElementById('divisionEffectiveTo').value = dateValue(value(division, 'EffectiveTo'));
        document.getElementById('divisionRemarks').value = value(division, 'Remarks') || '';
        editingDivisionWasActive = !!value(division, 'IsActive');
        divisionModal.show();
    }

    function resetRecipientForm() {
        var form = document.getElementById('recipientConfigurationForm');
        form.reset();
        clearFormError(form);
        document.getElementById('recipientFormTitle').textContent = 'Add Recipient';
        document.getElementById('recipientId').value = '';
        document.getElementById('recipientDivisionId').value = selectedDivisionId;
        document.getElementById('recipientType').value = 'TO';
        document.getElementById('recipientDisplayOrder').value = '1';
        document.getElementById('recipientEffectiveFrom').value = today();
        document.getElementById('recipientIsActive').checked = true;
        editingRecipientWasActive = false;
    }

    function appendTextCell(row, text) {
        var cell = document.createElement('td');
        cell.textContent = text || '-';
        row.appendChild(cell);
        return cell;
    }

    function renderRecipients() {
        var division = findDivision(selectedDivisionId);
        var recipients = division ? value(division, 'Recipients') || [] : [];
        var body = document.getElementById('recipientListBody');
        body.replaceChildren();

        if (!recipients.length) {
            var emptyRow = document.createElement('tr');
            var emptyCell = document.createElement('td');
            emptyCell.colSpan = 5;
            emptyCell.className = 'text-center text-muted py-4';
            emptyCell.textContent = 'No recipients configured.';
            emptyRow.appendChild(emptyCell);
            body.appendChild(emptyRow);
            return;
        }

        recipients.forEach(function (recipient) {
            var row = document.createElement('tr');
            appendTextCell(row, value(recipient, 'RecipientType'));
            appendTextCell(row, [value(recipient, 'PersonName'), value(recipient, 'Designation')].filter(Boolean).join(' / '));
            var emailCell = appendTextCell(row, value(recipient, 'EmailAddress'));
            emailCell.className = 'text-break';
            appendTextCell(row, value(recipient, 'IsActive') ? 'Active' : 'Inactive');

            var actions = document.createElement('td');
            actions.className = 'text-end text-nowrap';
            var edit = document.createElement('button');
            edit.type = 'button';
            edit.className = 'btn btn-sm btn-outline-primary me-1';
            edit.title = 'Edit recipient';
            edit.setAttribute('aria-label', 'Edit recipient');
            edit.innerHTML = '<i class="fa-solid fa-pen" aria-hidden="true"></i>';
            edit.addEventListener('click', function () { editRecipient(recipient); });
            actions.appendChild(edit);

            if (value(recipient, 'IsActive')) {
                var remove = document.createElement('button');
                remove.type = 'button';
                remove.className = 'btn btn-sm btn-outline-danger';
                remove.title = 'Remove recipient';
                remove.setAttribute('aria-label', 'Remove recipient');
                remove.innerHTML = '<i class="fa-solid fa-trash" aria-hidden="true"></i>';
                remove.addEventListener('click', function () { removeRecipient(recipient); });
                actions.appendChild(remove);
            }
            row.appendChild(actions);
            body.appendChild(row);
        });
    }

    function openRecipients(divisionId) {
        selectedDivisionId = Number(divisionId);
        var division = findDivision(selectedDivisionId);
        if (!division) return;
        document.getElementById('recipientDivisionName').textContent = value(division, 'DivisionName') + ' (ID ' + value(division, 'DivisionId') + ')';
        resetRecipientForm();
        renderRecipients();
        recipientModal.show();
    }

    function editRecipient(recipient) {
        resetRecipientForm();
        document.getElementById('recipientFormTitle').textContent = 'Edit Recipient';
        document.getElementById('recipientId').value = value(recipient, 'RecipientId');
        document.getElementById('recipientType').value = value(recipient, 'RecipientType');
        document.getElementById('recipientDisplayOrder').value = value(recipient, 'DisplayOrder') || 1;
        document.getElementById('recipientPersonName').value = value(recipient, 'PersonName') || '';
        document.getElementById('recipientDesignation').value = value(recipient, 'Designation') || '';
        document.getElementById('recipientEmail').value = value(recipient, 'EmailAddress') || '';
        document.getElementById('recipientEffectiveFrom').value = dateValue(value(recipient, 'EffectiveFrom'));
        document.getElementById('recipientEffectiveTo').value = dateValue(value(recipient, 'EffectiveTo'));
        document.getElementById('recipientIsActive').checked = !!value(recipient, 'IsActive');
        editingRecipientWasActive = !!value(recipient, 'IsActive');
    }

    async function removeRecipient(recipient) {
        var address = value(recipient, 'EmailAddress') || 'this recipient';
        if (!window.confirm('Remove ' + address + ' from active notification routing?')) return;
        var data = new FormData();
        data.append('recipientId', value(recipient, 'RecipientId'));
        try {
            await postForm(app.dataset.removeRecipientUrl, data);
            window.location.reload();
        } catch (error) {
            showMessage(error.message, false);
        }
    }

    document.getElementById('addDivisionConfiguration').addEventListener('click', function () {
        resetDivisionForm();
        divisionModal.show();
    });

    document.querySelectorAll('.edit-division').forEach(function (button) {
        button.addEventListener('click', function () { openDivisionEditor(button.dataset.divisionId); });
    });

    document.querySelectorAll('.manage-recipients').forEach(function (button) {
        button.addEventListener('click', function () { openRecipients(button.dataset.divisionId); });
    });

    document.getElementById('divisionOption').addEventListener('change', function (event) {
        var option = event.target.options[event.target.selectedIndex];
        document.getElementById('divisionIdValue').value = option ? option.value : '';
        document.getElementById('divisionNameValue').value = option ? option.dataset.name || '' : '';
    });

    document.getElementById('divisionConfigurationForm').addEventListener('submit', async function (event) {
        event.preventDefault();
        clearFormError(event.target);
        validateDateRange('divisionEffectiveFrom', 'divisionEffectiveTo');
        if (!event.target.reportValidity()) return;
        if (editingDivisionWasActive && !document.getElementById('divisionIsActive').checked &&
            !window.confirm('Deactivate this Division notification configuration? Notifications for it will stop.')) return;
        try {
            await postForm(app.dataset.saveDivisionUrl, new FormData(event.target));
            window.location.reload();
        } catch (error) {
            showFormError(event.target, error.message);
        }
    });

    document.getElementById('recipientConfigurationForm').addEventListener('submit', async function (event) {
        event.preventDefault();
        clearFormError(event.target);
        normalizeEmailList(document.getElementById('recipientEmail'));
        validateDateRange('recipientEffectiveFrom', 'recipientEffectiveTo');
        if (!event.target.reportValidity()) return;
        if (editingRecipientWasActive && !document.getElementById('recipientIsActive').checked &&
            !window.confirm('Deactivate this recipient? The address will no longer receive notifications.')) return;
        try {
            await postForm(app.dataset.saveRecipientUrl, new FormData(event.target));
            window.location.reload();
        } catch (error) {
            showFormError(event.target, error.message);
        }
    });

    document.getElementById('addRecipient').addEventListener('click', resetRecipientForm);
    document.getElementById('cancelRecipientEdit').addEventListener('click', resetRecipientForm);

    [['divisionEffectiveFrom', 'divisionEffectiveTo'], ['recipientEffectiveFrom', 'recipientEffectiveTo']]
        .forEach(function (ids) {
            ids.forEach(function (id) {
                document.getElementById(id).addEventListener('input', function () {
                    validateDateRange(ids[0], ids[1]);
                });
            });
        });
    document.getElementById('recipientEmail').addEventListener('input', function (event) {
        event.target.setCustomValidity('');
    });

    function filterRows() {
        var search = document.getElementById('managementNotificationSearch').value.trim().toLowerCase();
        var status = document.getElementById('managementNotificationStatus').value;
        var visible = 0;
        document.querySelectorAll('#managementNotificationTable tbody tr').forEach(function (row) {
            var matchesSearch = !search || (row.dataset.search || '').indexOf(search) >= 0;
            var matchesStatus = status === 'all' || row.dataset.active === status || row.dataset.weekly === status;
            row.classList.toggle('d-none', !(matchesSearch && matchesStatus));
            if (matchesSearch && matchesStatus) visible++;
        });
        document.getElementById('managementNotificationEmptyFilter').classList.toggle('d-none', visible > 0);
    }

    document.getElementById('managementNotificationSearch').addEventListener('input', filterRows);
    document.getElementById('managementNotificationStatus').addEventListener('change', filterRows);
})();
