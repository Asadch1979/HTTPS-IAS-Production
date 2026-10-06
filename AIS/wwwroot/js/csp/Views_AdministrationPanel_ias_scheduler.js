(function () {
    'use strict';

    var app = document.getElementById('iasSchedulerAdminApp');
    if (!app) return;

    var schedules = readJson('iasSchedulerSchedulesData');
    var jobs = readJson('iasSchedulerJobsData');
    var scheduleModal = new bootstrap.Modal(document.getElementById('scheduleModal'));
    var jobModal = new bootstrap.Modal(document.getElementById('jobModal'));
    var runNowModal = new bootstrap.Modal(document.getElementById('runNowModal'));
    var resolveModal = new bootstrap.Modal(document.getElementById('resolveModal'));

    function readJson(id) {
        var node = document.getElementById(id);
        return node ? JSON.parse(node.textContent || '[]') : [];
    }

    function value(item, name) {
        if (!item) return null;
        return item[name] !== undefined ? item[name] : item[name.charAt(0).toLowerCase() + name.slice(1)];
    }

    function dateValue(input) {
        return input ? String(input).slice(0, 10) : '';
    }

    function dateTimeLocalValue(input) {
        if (!input) return '';
        var value = String(input);
        var match = value.match(/^(\d{4}-\d{2}-\d{2})T(\d{2}:\d{2})/);
        return match ? match[1] + 'T' + match[2] : '';
    }

    function csrfToken() {
        var token = document.querySelector('#iasSchedulerCsrf input[name="__RequestVerificationToken"]');
        return token ? token.value : '';
    }

    function today() {
        var now = new Date();
        var local = new Date(now.getTime() - now.getTimezoneOffset() * 60000);
        return local.toISOString().slice(0, 10);
    }

    function showMessage(message, success) {
        var alert = document.getElementById('iasSchedulerMessage');
        alert.classList.remove('d-none', 'alert-success', 'alert-danger');
        alert.classList.add(success ? 'alert-success' : 'alert-danger');
        alert.textContent = message;
        alert.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
    }

    function clearFormError(form) {
        var alert = form.querySelector('.form-error');
        if (!alert) return;
        alert.textContent = '';
        alert.classList.add('d-none');
    }

    function showFormError(form, message) {
        var alert = form.querySelector('.form-error');
        if (!alert) {
            showMessage(message, false);
            return;
        }
        alert.textContent = message;
        alert.classList.remove('d-none');
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

    function findSchedule(scheduleId) {
        return schedules.find(function (item) { return Number(value(item, 'ScheduleId')) === Number(scheduleId); });
    }

    function findJob(jobId) {
        return jobs.find(function (item) { return Number(value(item, 'JobId')) === Number(jobId); });
    }

    function resetScheduleForm() {
        var form = document.getElementById('scheduleForm');
        form.reset();
        clearFormError(form);
        document.getElementById('scheduleModalTitle').textContent = 'Add Schedule';
        document.getElementById('scheduleId').value = '';
        document.getElementById('runHour').value = '7';
        document.getElementById('runMinute').value = '0';
        document.getElementById('maxRetry').value = '0';
        document.getElementById('retryIntervalMinutes').value = '30';
        document.getElementById('effectiveFrom').value = today();
        document.getElementById('emailRequired').checked = false;
        document.getElementById('isActive').checked = true;
        updateScheduleFrequencyFields();
    }

    function openScheduleEditor(scheduleId) {
        resetScheduleForm();
        var schedule = findSchedule(scheduleId);
        if (!schedule) return;
        document.getElementById('scheduleModalTitle').textContent = 'Edit Schedule';
        document.getElementById('scheduleId').value = value(schedule, 'ScheduleId');
        document.getElementById('scheduleJobId').value = value(schedule, 'JobId');
        document.getElementById('frequencyType').value = value(schedule, 'FrequencyType') || 'DAILY';
        document.getElementById('periodType').value = value(schedule, 'PeriodType') || 'DAILY';
        document.getElementById('runHour').value = value(schedule, 'RunHour');
        document.getElementById('runMinute').value = value(schedule, 'RunMinute');
        document.getElementById('dayOfWeek').value = value(schedule, 'DayOfWeek') || '';
        document.getElementById('dayOfMonth').value = value(schedule, 'DayOfMonth') || '';
        document.getElementById('maxRetry').value = value(schedule, 'MaxRetry') || 0;
        document.getElementById('retryIntervalMinutes').value = value(schedule, 'RetryIntervalMinutes') || 0;
        document.getElementById('effectiveFrom').value = dateValue(value(schedule, 'EffectiveFrom'));
        document.getElementById('effectiveTo').value = dateValue(value(schedule, 'EffectiveTo'));
        document.getElementById('nextDueOn').value = dateTimeLocalValue(value(schedule, 'NextDueOn'));
        document.getElementById('emailRequired').checked = !!value(schedule, 'EmailRequired');
        document.getElementById('isActive').checked = !!value(schedule, 'IsActive');
        document.getElementById('scheduleRemarks').value = value(schedule, 'Remarks') || '';
        updateScheduleFrequencyFields();
        scheduleModal.show();
    }

    function setGroupVisible(groupId, inputId, visible, required) {
        var group = document.getElementById(groupId);
        var input = document.getElementById(inputId);
        group.classList.toggle('d-none', !visible);
        input.disabled = !visible;
        input.required = !!required && visible;
        if (!visible) input.value = '';
    }

    function updateScheduleFrequencyFields() {
        var frequency = (document.getElementById('frequencyType').value || '').toUpperCase();
        setGroupVisible('dayOfWeekGroup', 'dayOfWeek', frequency === 'WEEKLY', false);
        setGroupVisible('dayOfMonthGroup', 'dayOfMonth', frequency === 'MONTHLY', false);
        setGroupVisible('nextDueOnGroup', 'nextDueOn', frequency === 'MANUAL', true);
    }

    function resetJobForm() {
        var form = document.getElementById('jobForm');
        form.reset();
        clearFormError(form);
        document.getElementById('jobModalTitle').textContent = 'Add Job Definition';
        document.getElementById('jobId').value = '';
        document.getElementById('executionType').value = 'APPLICATION';
    }

    function openJobEditor(jobId) {
        resetJobForm();
        var job = findJob(jobId);
        if (!job) return;
        document.getElementById('jobModalTitle').textContent = 'Edit Job Definition';
        document.getElementById('jobId').value = value(job, 'JobId');
        document.getElementById('jobCode').value = value(job, 'JobCode') || '';
        document.getElementById('jobName').value = value(job, 'JobName') || '';
        document.getElementById('executionType').value = value(job, 'ExecutionType') || 'APPLICATION';
        document.getElementById('packageName').value = value(job, 'PackageName') || '';
        document.getElementById('procedureName').value = value(job, 'ProcedureName') || '';
        document.getElementById('applicationHandler').value = value(job, 'ApplicationHandler') || '';
        document.getElementById('jobDescription').value = value(job, 'Description') || '';
        jobModal.show();
    }

    function openRunNow(scheduleId, jobName) {
        var form = document.getElementById('runNowForm');
        form.reset();
        clearFormError(form);
        document.getElementById('runNowScheduleId').value = scheduleId;
        document.getElementById('runNowModalTitle').textContent = 'Run Now - ' + (jobName || 'Schedule');
        toggleRunNowCustom();
        runNowModal.show();
    }

    function openResolve(executionId, jobName) {
        var form = document.getElementById('resolveForm');
        form.reset();
        clearFormError(form);
        document.getElementById('resolveExecutionId').value = executionId;
        document.getElementById('resolveModalTitle').textContent = 'Resolve - ' + (jobName || 'Execution');
        resolveModal.show();
    }

    function toggleRunNowCustom() {
        var custom = document.getElementById('runNowMode').value === 'CUSTOM';
        document.querySelectorAll('.run-now-custom').forEach(function (node) {
            node.classList.toggle('d-none', !custom);
        });
    }

    document.getElementById('addSchedulerSchedule').addEventListener('click', function () {
        resetScheduleForm();
        scheduleModal.show();
    });

    document.getElementById('addSchedulerJob').addEventListener('click', function () {
        resetJobForm();
        jobModal.show();
    });

    document.querySelectorAll('.edit-schedule').forEach(function (button) {
        button.addEventListener('click', function () { openScheduleEditor(button.dataset.scheduleId); });
    });

    document.querySelectorAll('.edit-job').forEach(function (button) {
        button.addEventListener('click', function () { openJobEditor(button.dataset.jobId); });
    });

    document.querySelectorAll('.run-now').forEach(function (button) {
        button.addEventListener('click', function () { openRunNow(button.dataset.scheduleId, button.dataset.jobName); });
    });

    document.querySelectorAll('.resolve-execution').forEach(function (button) {
        button.addEventListener('click', function () { openResolve(button.dataset.executionId, button.dataset.jobName); });
    });

    document.querySelectorAll('.toggle-schedule').forEach(function (button) {
        button.addEventListener('click', async function () {
            var makeActive = button.dataset.active === 'True' || button.dataset.active === 'true';
            if (!window.confirm((makeActive ? 'Activate' : 'Deactivate') + ' this schedule?')) return;
            var data = new FormData();
            data.append('scheduleId', button.dataset.scheduleId);
            data.append('isActive', makeActive ? 'true' : 'false');
            try {
                await postForm(app.dataset.setActiveUrl, data);
                window.location.reload();
            } catch (error) {
                showMessage(error.message, false);
            }
        });
    });

    document.querySelectorAll('.retry-run-request').forEach(function (button) {
        button.addEventListener('click', async function () {
            if (!window.confirm('Retry this failed Run Now request? Completed work will not be repeated; only eligible failed work will be processed.')) return;
            button.disabled = true;
            var data = new FormData();
            data.append('requestId', button.dataset.requestId);
            try {
                await postForm(app.dataset.retryRunUrl, data);
                window.location.reload();
            } catch (error) {
                showMessage(error.message, false);
                button.disabled = false;
            }
        });
    });

    document.querySelectorAll('.cancel-run-request').forEach(function (button) {
        button.addEventListener('click', async function () {
            if (!window.confirm('Cancel this pending Run Now request?')) return;
            var data = new FormData();
            data.append('requestId', button.dataset.requestId);
            try {
                await postForm(app.dataset.cancelRunUrl, data);
                window.location.reload();
            } catch (error) {
                showMessage(error.message, false);
            }
        });
    });

    document.getElementById('runNowMode').addEventListener('change', toggleRunNowCustom);
    document.getElementById('frequencyType').addEventListener('change', updateScheduleFrequencyFields);

    document.getElementById('scheduleForm').addEventListener('submit', async function (event) {
        event.preventDefault();
        clearFormError(event.target);
        updateScheduleFrequencyFields();
        if (!event.target.reportValidity()) return;
        try {
            await postForm(app.dataset.saveScheduleUrl, new FormData(event.target));
            window.location.reload();
        } catch (error) {
            showFormError(event.target, error.message);
        }
    });

    document.getElementById('jobForm').addEventListener('submit', async function (event) {
        event.preventDefault();
        clearFormError(event.target);
        if (!event.target.reportValidity()) return;
        try {
            await postForm(app.dataset.saveJobUrl, new FormData(event.target));
            window.location.reload();
        } catch (error) {
            showFormError(event.target, error.message);
        }
    });

    document.getElementById('runNowForm').addEventListener('submit', async function (event) {
        event.preventDefault();
        clearFormError(event.target);
        if (!event.target.reportValidity()) return;
        try {
            await postForm(app.dataset.runNowUrl, new FormData(event.target));
            window.location.reload();
        } catch (error) {
            showFormError(event.target, error.message);
        }
    });

    document.getElementById('resolveForm').addEventListener('submit', async function (event) {
        event.preventDefault();
        clearFormError(event.target);
        if (!event.target.reportValidity()) return;
        if (!window.confirm('Resolve this scheduler execution with the selected result?')) return;
        try {
            await postForm(app.dataset.resolveUrl, new FormData(event.target));
            window.location.reload();
        } catch (error) {
            showFormError(event.target, error.message);
        }
    });
})();
