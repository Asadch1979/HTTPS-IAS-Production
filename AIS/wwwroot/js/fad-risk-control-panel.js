(function ($) {
    var baseUrl = (window.g_asiBaseURL || '').toString();
    let currentMainKpiId = null;
    let currentSubKpiId = null;
    let currentProcessId = null;
    let currentSubProcessId = null;

    $(function () {
        ensureModals();
        bindHandlers();
        loadMainKpis();
        loadGravityOptions();
    });

    function bindHandlers() {
        $('#btnAddMainKpi').on('click', function () {
            openMainKpiModal();
        });

        $('#tblMainKpi').on('click', '.btn-edit-main-kpi', function () {
            openMainKpiModal($(this).data());
        });

        $('#btnSaveMainKpi').on('click', function () {
            saveMainKpi();
        });

        $('#ddlMainKpi').on('change', function () {
            currentMainKpiId = valueOrNull($(this).val());
            loadSubKpis(currentMainKpiId);
            updateAddButtons();
        });

        $('#btnAddSubKpi').on('click', function () {
            openSubKpiModal();
        });

        $('#tblSubKpi').on('click', '.btn-edit-sub-kpi', function () {
            openSubKpiModal($(this).data());
        });

        $('#btnSaveSubKpi').on('click', function () {
            saveSubKpi();
        });

        $('#ddlSubKpi').on('change', function () {
            currentSubKpiId = valueOrNull($(this).val());
            loadProcesses(currentSubKpiId);
            updateAddButtons();
        });

        $('#btnAddProcess').on('click', function () {
            openProcessModal();
        });

        $('#tblProcess').on('click', '.btn-edit-process', function () {
            openProcessModal($(this).data());
        });

        $('#btnSaveProcess').on('click', function () {
            saveProcess();
        });

        $('#ddlProcess').on('change', function () {
            currentProcessId = valueOrNull($(this).val());
            loadSubProcesses(currentProcessId);
            updateAddButtons();
        });

        $('#btnAddSubProcess').on('click', function () {
            openSubProcessModal();
        });

        $('#tblSubProcess').on('click', '.btn-edit-sub-process', function () {
            openSubProcessModal($(this).data());
        });

        $('#btnSaveSubProcess').on('click', function () {
            saveSubProcess();
        });

        $('#ddlSubProcess').on('change', function () {
            currentSubProcessId = valueOrNull($(this).val());
            loadAnnexures(currentSubProcessId);
            updateAddButtons();
        });

        $('#btnAddAnnexure').on('click', function () {
            openAnnexureModal();
        });

        $('#tblAnnexure').on('click', '.btn-edit-annexure', function () {
            openAnnexureModal($(this).data());
        });

        $('#btnSaveAnnexure').on('click', function () {
            saveAnnexure();
        });
    }

    function ensureModals() {
        if (!$('#mainKpiModal').length) {
            $('body').append(
                '<div class="modal fade" id="mainKpiModal" tabindex="-1" aria-hidden="true">'
                + '<div class="modal-dialog">'
                + '<div class="modal-content">'
                + '<div class="modal-header">'
                + '<h5 class="modal-title">Main KPI</h5>'
                + '<button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>'
                + '</div>'
                + '<div class="modal-body">'
                + '<input type="hidden" id="kpiMainId" />'
                + '<div class="mb-2">'
                + '<label class="form-label">KPI Code</label>'
                + '<input type="text" class="form-control" id="txtKpiCode" />'
                + '</div>'
                + '<div class="mb-2">'
                + '<label class="form-label">KPI Name</label>'
                + '<input type="text" class="form-control" id="txtKpiName" />'
                + '</div>'
                + '<div class="mb-2">'
                + '<label class="form-label">Display Order</label>'
                + '<input type="number" class="form-control" id="txtOrder" />'
                + '</div>'
                + '<div class="mb-2">'
                + '<label class="form-label">Active (Y/N)</label>'
                + '<select class="form-select" id="ddlActive">'
                + '<option value="Y">Y</option>'
                + '<option value="N">N</option>'
                + '</select>'
                + '</div>'
                + '</div>'
                + '<div class="modal-footer">'
                + '<button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Close</button>'
                + '<button type="button" class="btn btn-primary" id="btnSaveMainKpi">Save</button>'
                + '</div>'
                + '</div>'
                + '</div>'
                + '</div>'
            );
        }

        if (!$('#subKpiModal').length) {
            $('body').append(
                '<div class="modal fade" id="subKpiModal" tabindex="-1" aria-hidden="true">'
                + '<div class="modal-dialog">'
                + '<div class="modal-content">'
                + '<div class="modal-header">'
                + '<h5 class="modal-title">Sub KPI</h5>'
                + '<button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>'
                + '</div>'
                + '<div class="modal-body">'
                + '<input type="hidden" id="kpiSubId" />'
                + '<div class="mb-2">'
                + '<label class="form-label">Sub KPI Code</label>'
                + '<input type="text" class="form-control" id="txtSubKpiCode" />'
                + '</div>'
                + '<div class="mb-2">'
                + '<label class="form-label">Sub KPI Name</label>'
                + '<input type="text" class="form-control" id="txtSubKpiName" />'
                + '</div>'
                + '<div class="mb-2">'
                + '<label class="form-label">Weightage</label>'
                + '<input type="number" class="form-control" id="txtWeightage" />'
                + '</div>'
                + '<div class="mb-2">'
                + '<label class="form-label">Display Order</label>'
                + '<input type="number" class="form-control" id="txtOrder" />'
                + '</div>'
                + '<div class="mb-2">'
                + '<label class="form-label">Active (Y/N)</label>'
                + '<select class="form-select" id="ddlActive">'
                + '<option value="Y">Y</option>'
                + '<option value="N">N</option>'
                + '</select>'
                + '</div>'
                + '</div>'
                + '<div class="modal-footer">'
                + '<button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Close</button>'
                + '<button type="button" class="btn btn-primary" id="btnSaveSubKpi">Save</button>'
                + '</div>'
                + '</div>'
                + '</div>'
                + '</div>'
            );
        }

        if (!$('#processModal').length) {
            $('body').append(
                '<div class="modal fade" id="processModal" tabindex="-1" aria-hidden="true">'
                + '<div class="modal-dialog">'
                + '<div class="modal-content">'
                + '<div class="modal-header">'
                + '<h5 class="modal-title">Process</h5>'
                + '<button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>'
                + '</div>'
                + '<div class="modal-body">'
                + '<input type="hidden" id="processId" />'
                + '<div class="mb-2">'
                + '<label class="form-label">Process Code</label>'
                + '<input type="text" class="form-control" id="txtProcessCode" />'
                + '</div>'
                + '<div class="mb-2">'
                + '<label class="form-label">Process Name</label>'
                + '<input type="text" class="form-control" id="txtProcessName" />'
                + '</div>'
                + '<div class="mb-2">'
                + '<label class="form-label">Weightage</label>'
                + '<input type="number" class="form-control" id="txtWeightage" />'
                + '</div>'
                + '<div class="mb-2">'
                + '<label class="form-label">Display Order</label>'
                + '<input type="number" class="form-control" id="txtOrder" />'
                + '</div>'
                + '<div class="mb-2">'
                + '<label class="form-label">Active (Y/N)</label>'
                + '<select class="form-select" id="ddlActive">'
                + '<option value="Y">Y</option>'
                + '<option value="N">N</option>'
                + '</select>'
                + '</div>'
                + '</div>'
                + '<div class="modal-footer">'
                + '<button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Close</button>'
                + '<button type="button" class="btn btn-primary" id="btnSaveProcess">Save</button>'
                + '</div>'
                + '</div>'
                + '</div>'
                + '</div>'
            );
        }

        if (!$('#subProcessModal').length) {
            $('body').append(
                '<div class="modal fade" id="subProcessModal" tabindex="-1" aria-hidden="true">'
                + '<div class="modal-dialog">'
                + '<div class="modal-content">'
                + '<div class="modal-header">'
                + '<h5 class="modal-title">Sub Process</h5>'
                + '<button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>'
                + '</div>'
                + '<div class="modal-body">'
                + '<input type="hidden" id="subProcessId" />'
                + '<div class="mb-2">'
                + '<label class="form-label">Sub Process Code</label>'
                + '<input type="text" class="form-control" id="txtSubProcessCode" />'
                + '</div>'
                + '<div class="mb-2">'
                + '<label class="form-label">Sub Process Name</label>'
                + '<input type="text" class="form-control" id="txtSubProcessName" />'
                + '</div>'
                + '<div class="mb-2">'
                + '<label class="form-label">Gravity</label>'
                + '<select class="form-select" id="ddlGravity">'
                + '<option value="">-- Select Gravity --</option>'
                + '</select>'
                + '</div>'
                + '<div class="mb-2">'
                + '<label class="form-label">Active (Y/N)</label>'
                + '<select class="form-select" id="ddlActive">'
                + '<option value="Y">Y</option>'
                + '<option value="N">N</option>'
                + '</select>'
                + '</div>'
                + '</div>'
                + '<div class="modal-footer">'
                + '<button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Close</button>'
                + '<button type="button" class="btn btn-primary" id="btnSaveSubProcess">Save</button>'
                + '</div>'
                + '</div>'
                + '</div>'
                + '</div>'
            );
        }

        if (!$('#annexureModal').length) {
            $('body').append(
                '<div class="modal fade" id="annexureModal" tabindex="-1" aria-hidden="true">'
                + '<div class="modal-dialog">'
                + '<div class="modal-content">'
                + '<div class="modal-header">'
                + '<h5 class="modal-title">Annexure</h5>'
                + '<button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>'
                + '</div>'
                + '<div class="modal-body">'
                + '<input type="hidden" id="annexureId" />'
                + '<div class="mb-2">'
                + '<label class="form-label">Annexure Code</label>'
                + '<input type="text" class="form-control" id="txtAnnexureCode" />'
                + '</div>'
                + '<div class="mb-2">'
                + '<label class="form-label">Annexure Name</label>'
                + '<input type="text" class="form-control" id="txtAnnexureName" />'
                + '</div>'
                + '<div class="mb-2">'
                + '<label class="form-label">Active (Y/N)</label>'
                + '<select class="form-select" id="ddlActive">'
                + '<option value="Y">Y</option>'
                + '<option value="N">N</option>'
                + '</select>'
                + '</div>'
                + '</div>'
                + '<div class="modal-footer">'
                + '<button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Close</button>'
                + '<button type="button" class="btn btn-primary" id="btnSaveAnnexure">Save</button>'
                + '</div>'
                + '</div>'
                + '</div>'
                + '</div>'
            );
        }
    }

    function openMainKpiModal(item) {
        var modal = $('#mainKpiModal');
        modal.find('#kpiMainId').val(item && item.kpiMainId ? item.kpiMainId : '');
        modal.find('#txtKpiCode').val(item && item.code ? item.code : '');
        modal.find('#txtKpiName').val(item && item.name ? item.name : '');
        modal.find('#txtOrder').val(item && item.displayOrder ? item.displayOrder : '');
        modal.find('#ddlActive').val(item && item.isActive ? item.isActive : 'Y');
        bootstrap.Modal.getOrCreateInstance(document.getElementById('mainKpiModal')).show();
    }

    function openSubKpiModal(item) {
        if (!currentMainKpiId) {
            alert('Please select a Main KPI first.');
            return;
        }
        var modal = $('#subKpiModal');
        modal.find('#kpiSubId').val(item && item.kpiSubId ? item.kpiSubId : '');
        modal.find('#txtSubKpiCode').val(item && item.code ? item.code : '');
        modal.find('#txtSubKpiName').val(item && item.name ? item.name : '');
        modal.find('#txtWeightage').val(item && item.weightage ? item.weightage : '');
        modal.find('#txtOrder').val(item && item.displayOrder ? item.displayOrder : '');
        modal.find('#ddlActive').val(item && item.isActive ? item.isActive : 'Y');
        bootstrap.Modal.getOrCreateInstance(document.getElementById('subKpiModal')).show();
    }

    function openProcessModal(item) {
        if (!currentSubKpiId) {
            alert('Please select a Sub KPI first.');
            return;
        }
        var modal = $('#processModal');
        modal.find('#processId').val(item && item.processId ? item.processId : '');
        modal.find('#txtProcessCode').val(item && item.code ? item.code : '');
        modal.find('#txtProcessName').val(item && item.name ? item.name : '');
        modal.find('#txtWeightage').val(item && item.weightage ? item.weightage : '');
        modal.find('#txtOrder').val(item && item.displayOrder ? item.displayOrder : '');
        modal.find('#ddlActive').val(item && item.isActive ? item.isActive : 'Y');
        bootstrap.Modal.getOrCreateInstance(document.getElementById('processModal')).show();
    }

    function openSubProcessModal(item) {
        if (!currentProcessId) {
            alert('Please select a Process first.');
            return;
        }
        var modal = $('#subProcessModal');
        modal.find('#subProcessId').val(item && item.subProcessId ? item.subProcessId : '');
        modal.find('#txtSubProcessCode').val(item && item.code ? item.code : '');
        modal.find('#txtSubProcessName').val(item && item.name ? item.name : '');
        modal.find('#ddlGravity').val(item && item.gravityId ? item.gravityId : '');
        modal.find('#ddlActive').val(item && item.isActive ? item.isActive : 'Y');
        bootstrap.Modal.getOrCreateInstance(document.getElementById('subProcessModal')).show();
    }

    function openAnnexureModal(item) {
        if (!currentSubProcessId) {
            alert('Please select a Sub Process first.');
            return;
        }
        var modal = $('#annexureModal');
        modal.find('#annexureId').val(item && item.annexureId ? item.annexureId : '');
        modal.find('#txtAnnexureCode').val(item && item.code ? item.code : '');
        modal.find('#txtAnnexureName').val(item && item.name ? item.name : '');
        modal.find('#ddlActive').val(item && item.isActive ? item.isActive : 'Y');
        bootstrap.Modal.getOrCreateInstance(document.getElementById('annexureModal')).show();
    }

    function saveMainKpi() {
        var modal = $('#mainKpiModal');
        var payload = {
            kpiMainId: valueOrNull(modal.find('#kpiMainId').val()),
            code: modal.find('#txtKpiCode').val(),
            name: modal.find('#txtKpiName').val(),
            displayOrder: parseInt(modal.find('#txtOrder').val(), 10) || 0,
            isActive: modal.find('#ddlActive').val()
        };

        $.ajax({
            url: baseUrl + '/FADRiskControlPanel/SaveMainKpi',
            type: 'POST',
            data: payload
        }).done(function () {
            bootstrap.Modal.getOrCreateInstance(document.getElementById('mainKpiModal')).hide();
            loadMainKpis();
        }).fail(function () {
            alert('Failed to save Main KPI.');
        });
    }

    function saveSubKpi() {
        if (!currentMainKpiId) {
            alert('Please select a Main KPI first.');
            return;
        }

        var modal = $('#subKpiModal');
        var payload = {
            kpiMainId: currentMainKpiId,
            kpiSubId: valueOrNull(modal.find('#kpiSubId').val()),
            code: modal.find('#txtSubKpiCode').val(),
            name: modal.find('#txtSubKpiName').val(),
            weightage: parseFloat(modal.find('#txtWeightage').val()) || 0,
            displayOrder: parseInt(modal.find('#txtOrder').val(), 10) || 0,
            isActive: modal.find('#ddlActive').val()
        };

        $.ajax({
            url: baseUrl + '/FADRiskControlPanel/SaveSubKpi',
            type: 'POST',
            data: payload
        }).done(function (response) {
            if (response && response.success === false) {
                alert(response.message || 'Failed to save Sub KPI.');
                return;
            }
            bootstrap.Modal.getOrCreateInstance(document.getElementById('subKpiModal')).hide();
            loadSubKpis(currentMainKpiId);
        }).fail(function () {
            alert('Failed to save Sub KPI.');
        });
    }

    function saveProcess() {
        if (!currentSubKpiId) {
            alert('Please select a Sub KPI first.');
            return;
        }

        var modal = $('#processModal');
        var payload = {
            kpiSubId: currentSubKpiId,
            processId: valueOrNull(modal.find('#processId').val()),
            code: modal.find('#txtProcessCode').val(),
            name: modal.find('#txtProcessName').val(),
            weightage: parseFloat(modal.find('#txtWeightage').val()) || 0,
            displayOrder: parseInt(modal.find('#txtOrder').val(), 10) || 0,
            isActive: modal.find('#ddlActive').val()
        };

        $.ajax({
            url: baseUrl + '/FADRiskControlPanel/SaveProcess',
            type: 'POST',
            data: payload
        }).done(function (response) {
            if (response && response.success === false) {
                alert(response.message || 'Failed to save Process.');
                return;
            }
            bootstrap.Modal.getOrCreateInstance(document.getElementById('processModal')).hide();
            loadProcesses(currentSubKpiId);
        }).fail(function () {
            alert('Failed to save Process.');
        });
    }

    function saveSubProcess() {
        if (!currentProcessId) {
            alert('Please select a Process first.');
            return;
        }

        var modal = $('#subProcessModal');
        var payload = {
            processId: currentProcessId,
            subProcessId: valueOrNull(modal.find('#subProcessId').val()),
            code: modal.find('#txtSubProcessCode').val(),
            name: modal.find('#txtSubProcessName').val(),
            gravityId: parseInt(modal.find('#ddlGravity').val(), 10) || 0,
            isActive: modal.find('#ddlActive').val()
        };

        $.ajax({
            url: baseUrl + '/FADRiskControlPanel/SaveSubProcess',
            type: 'POST',
            data: payload
        }).done(function (response) {
            if (response && response.success === false) {
                alert(response.message || 'Failed to save Sub Process.');
                return;
            }
            bootstrap.Modal.getOrCreateInstance(document.getElementById('subProcessModal')).hide();
            loadSubProcesses(currentProcessId);
        }).fail(function () {
            alert('Failed to save Sub Process.');
        });
    }

    function saveAnnexure() {
        if (!currentSubProcessId) {
            alert('Please select a Sub Process first.');
            return;
        }

        var modal = $('#annexureModal');
        var payload = {
            subProcessId: currentSubProcessId,
            annexureId: valueOrNull(modal.find('#annexureId').val()),
            code: modal.find('#txtAnnexureCode').val(),
            name: modal.find('#txtAnnexureName').val(),
            isActive: modal.find('#ddlActive').val()
        };

        $.ajax({
            url: baseUrl + '/FADRiskControlPanel/SaveAnnexure',
            type: 'POST',
            data: payload
        }).done(function (response) {
            if (response && response.success === false) {
                alert(response.message || 'Failed to save Annexure.');
                return;
            }
            bootstrap.Modal.getOrCreateInstance(document.getElementById('annexureModal')).hide();
            loadAnnexures(currentSubProcessId);
        }).fail(function () {
            alert('Failed to save Annexure.');
        });
    }

    function loadMainKpis() {
        $.get(baseUrl + '/FADRiskControlPanel/GetMainKpi', function (data) {
            renderMainKpiTable(data);
            populateMainKpiDropdown(data);
        }).fail(function () {
            alert('Failed to load Main KPIs.');
        });
    }

    function renderMainKpiTable(data) {
        var tbody = $('#tblMainKpi tbody');
        tbody.empty();

        $.each(data || [], function (_, item) {
            var id = pick(item, ['KPI_MAIN_ID']);
            var code = pick(item, ['KPI_CODE']);
            var name = pick(item, ['KPI_NAME']);
            var displayOrder = pick(item, ['DISPLAY_ORDER']);
            var isActive = pick(item, ['IS_ACTIVE']);

            tbody.append(
                '<tr>'
                + '<td>' + safeText(code) + '</td>'
                + '<td>' + safeText(name) + '</td>'
                + '<td>' + safeText(displayOrder) + '</td>'
                + '<td>' + safeText(isActive) + '</td>'
                + '<td>'
                + '<button type="button" class="btn btn-sm btn-secondary btn-edit-main-kpi"'
                + ' data-kpi-main-id="' + safeAttr(id) + '"'
                + ' data-code="' + safeAttr(code) + '"'
                + ' data-name="' + safeAttr(name) + '"'
                + ' data-display-order="' + safeAttr(displayOrder) + '"'
                + ' data-is-active="' + safeAttr(isActive) + '"'
                + '>Edit</button>'
                + '</td>'
                + '</tr>'
            );
        });
    }

    function populateMainKpiDropdown(data) {
        var ddl = $('#ddlMainKpi');
        ddl.empty();
        ddl.append('<option value="">-- Select Main KPI --</option>');

        $.each(data || [], function (_, item) {
            var id = pick(item, ['KPI_MAIN_ID']);
            var name = pick(item, ['KPI_NAME']);
            if (id !== '') {
                ddl.append('<option value="' + safeAttr(id) + '">' + safeText(name) + '</option>');
            }
        });

        if (ddl.find('option').length > 1) {
            ddl.val(ddl.find('option:eq(1)').val()).trigger('change');
        } else {
            currentMainKpiId = null;
            loadSubKpis(null);
        }
        updateAddButtons();
    }

    function loadSubKpis(kpiMainId) {
        var tbody = $('#tblSubKpi tbody');
        tbody.empty();
        populateSubKpiDropdown([]);

        if (!kpiMainId) {
            return;
        }

        $.get(baseUrl + '/FADRiskControlPanel/GetSubKpiByMain', { kpiMainId: kpiMainId }, function (data) {
            renderSubKpiTable(data);
            populateSubKpiDropdown(data);
        }).fail(function () {
            alert('Failed to load Sub KPIs.');
        });
    }

    function renderSubKpiTable(data) {
        var tbody = $('#tblSubKpi tbody');
        tbody.empty();

        $.each(data || [], function (_, item) {
            var id = pick(item, ['KPI_SUB_ID']);
            var code = pick(item, ['KPI_SUB_CODE']);
            var name = pick(item, ['KPI_SUB_NAME']);
            var weightage = pick(item, ['WEIGHTAGE']);
            var displayOrder = pick(item, ['DISPLAY_ORDER']);
            var isActive = pick(item, ['IS_ACTIVE']);

            tbody.append(
                '<tr>'
                + '<td>' + safeText(code) + '</td>'
                + '<td>' + safeText(name) + '</td>'
                + '<td>' + safeText(weightage) + '</td>'
                + '<td>' + safeText(displayOrder) + '</td>'
                + '<td>' + safeText(isActive) + '</td>'
                + '<td>'
                + '<button type="button" class="btn btn-sm btn-secondary btn-edit-sub-kpi"'
                + ' data-kpi-sub-id="' + safeAttr(id) + '"'
                + ' data-code="' + safeAttr(code) + '"'
                + ' data-name="' + safeAttr(name) + '"'
                + ' data-weightage="' + safeAttr(weightage) + '"'
                + ' data-display-order="' + safeAttr(displayOrder) + '"'
                + ' data-is-active="' + safeAttr(isActive) + '"'
                + '>Edit</button>'
                + '</td>'
                + '</tr>'
            );
        });
    }

    function populateSubKpiDropdown(data) {
        var ddl = $('#ddlSubKpi');
        ddl.empty();
        ddl.append('<option value="">-- Select Sub KPI --</option>');

        $.each(data || [], function (_, item) {
            var id = pick(item, ['KPI_SUB_ID']);
            var name = pick(item, ['KPI_SUB_NAME']);
            if (id !== '') {
                ddl.append('<option value="' + safeAttr(id) + '">' + safeText(name) + '</option>');
            }
        });

        if (ddl.find('option').length > 1) {
            ddl.val(ddl.find('option:eq(1)').val()).trigger('change');
        } else {
            currentSubKpiId = null;
            loadProcesses(null);
        }
        updateAddButtons();
    }

    function loadProcesses(kpiSubId) {
        var tbody = $('#tblProcess tbody');
        tbody.empty();
        populateProcessDropdown([]);

        if (!kpiSubId) {
            return;
        }

        $.get(baseUrl + '/FADRiskControlPanel/GetProcessBySubKpi', { kpiSubId: kpiSubId }, function (data) {
            renderProcessTable(data);
            populateProcessDropdown(data);
        }).fail(function () {
            alert('Failed to load Processes.');
        });
    }

    function renderProcessTable(data) {
        var tbody = $('#tblProcess tbody');
        tbody.empty();

        $.each(data || [], function (_, item) {
            var id = pick(item, ['PROCESS_ID']);
            var code = pick(item, ['PROCESS_CODE']);
            var name = pick(item, ['PROCESS_NAME']);
            var weightage = pick(item, ['WEIGHTAGE']);
            var displayOrder = pick(item, ['DISPLAY_ORDER']);
            var isActive = pick(item, ['IS_ACTIVE']);

            tbody.append(
                '<tr>'
                + '<td>' + safeText(code) + '</td>'
                + '<td>' + safeText(name) + '</td>'
                + '<td>' + safeText(weightage) + '</td>'
                + '<td>' + safeText(displayOrder) + '</td>'
                + '<td>' + safeText(isActive) + '</td>'
                + '<td>'
                + '<button type="button" class="btn btn-sm btn-secondary btn-edit-process"'
                + ' data-process-id="' + safeAttr(id) + '"'
                + ' data-code="' + safeAttr(code) + '"'
                + ' data-name="' + safeAttr(name) + '"'
                + ' data-weightage="' + safeAttr(weightage) + '"'
                + ' data-display-order="' + safeAttr(displayOrder) + '"'
                + ' data-is-active="' + safeAttr(isActive) + '"'
                + '>Edit</button>'
                + '</td>'
                + '</tr>'
            );
        });
    }

    function populateProcessDropdown(data) {
        var ddl = $('#ddlProcess');
        ddl.empty();
        ddl.append('<option value="">-- Select Process --</option>');

        $.each(data || [], function (_, item) {
            var id = pick(item, ['PROCESS_ID']);
            var name = pick(item, ['PROCESS_NAME']);
            if (id !== '') {
                ddl.append('<option value="' + safeAttr(id) + '">' + safeText(name) + '</option>');
            }
        });

        if (ddl.find('option').length > 1) {
            ddl.val(ddl.find('option:eq(1)').val()).trigger('change');
        } else {
            currentProcessId = null;
            loadSubProcesses(null);
        }
        updateAddButtons();
    }

    function loadSubProcesses(processId) {
        var tbody = $('#tblSubProcess tbody');
        tbody.empty();
        populateSubProcessDropdown([]);

        if (!processId) {
            return;
        }

        $.get(baseUrl + '/FADRiskControlPanel/GetSubProcessByProcess', { processId: processId }, function (data) {
            renderSubProcessTable(data);
            populateSubProcessDropdown(data);
        }).fail(function () {
            alert('Failed to load Sub Processes.');
        });
    }

    function renderSubProcessTable(data) {
        var tbody = $('#tblSubProcess tbody');
        tbody.empty();

        $.each(data || [], function (_, item) {
            var id = pick(item, ['SUB_PROCESS_ID']);
            var code = pick(item, ['SUB_PROCESS_CODE']);
            var name = pick(item, ['SUB_PROCESS_NAME']);
            var gravityId = pick(item, ['GRAVITY_ID']);
            var gravityName = pick(item, ['GRAVITY_NAME']);
            var isActive = pick(item, ['IS_ACTIVE']);
            var gravityDisplay = gravityName || gravityId;

            tbody.append(
                '<tr>'
                + '<td>' + safeText(code) + '</td>'
                + '<td>' + safeText(name) + '</td>'
                + '<td>' + safeText(gravityDisplay) + '</td>'
                + '<td>' + safeText(isActive) + '</td>'
                + '<td>'
                + '<button type="button" class="btn btn-sm btn-secondary btn-edit-sub-process"'
                + ' data-sub-process-id="' + safeAttr(id) + '"'
                + ' data-code="' + safeAttr(code) + '"'
                + ' data-name="' + safeAttr(name) + '"'
                + ' data-gravity-id="' + safeAttr(gravityId) + '"'
                + ' data-is-active="' + safeAttr(isActive) + '"'
                + '>Edit</button>'
                + '</td>'
                + '</tr>'
            );
        });
    }

    function populateSubProcessDropdown(data) {
        var ddl = $('#ddlSubProcess');
        ddl.empty();
        ddl.append('<option value="">-- Select Sub Process --</option>');

        $.each(data || [], function (_, item) {
            var id = pick(item, ['SUB_PROCESS_ID']);
            var name = pick(item, ['SUB_PROCESS_NAME']);
            if (id !== '') {
                ddl.append('<option value="' + safeAttr(id) + '">' + safeText(name) + '</option>');
            }
        });

        if (ddl.find('option').length > 1) {
            ddl.val(ddl.find('option:eq(1)').val()).trigger('change');
        } else {
            currentSubProcessId = null;
            loadAnnexures(null);
        }
        updateAddButtons();
    }

    function loadAnnexures(subProcessId) {
        var tbody = $('#tblAnnexure tbody');
        tbody.empty();

        if (!subProcessId) {
            return;
        }

        $.get(baseUrl + '/FADRiskControlPanel/GetAnnexureBySubProcess', { subProcessId: subProcessId }, function (data) {
            renderAnnexureTable(data);
        }).fail(function () {
            alert('Failed to load Annexures.');
        });
    }

    function renderAnnexureTable(data) {
        var tbody = $('#tblAnnexure tbody');
        tbody.empty();

        $.each(data || [], function (_, item) {
            var id = pick(item, ['ANNEXURE_ID']);
            var code = pick(item, ['ANNEXURE_CODE']);
            var name = pick(item, ['ANNEXURE_NAME']);
            var isActive = pick(item, ['IS_ACTIVE']);

            tbody.append(
                '<tr>'
                + '<td>' + safeText(code) + '</td>'
                + '<td>' + safeText(name) + '</td>'
                + '<td>' + safeText(isActive) + '</td>'
                + '<td>'
                + '<button type="button" class="btn btn-sm btn-secondary btn-edit-annexure"'
                + ' data-annexure-id="' + safeAttr(id) + '"'
                + ' data-code="' + safeAttr(code) + '"'
                + ' data-name="' + safeAttr(name) + '"'
                + ' data-is-active="' + safeAttr(isActive) + '"'
                + '>Edit</button>'
                + '</td>'
                + '</tr>'
            );
        });
    }

    function loadGravityOptions() {
        $.get(baseUrl + '/FADRiskControlPanel/GetGravity', function (data) {
            var ddl = $('#subProcessModal #ddlGravity');
            ddl.empty().append('<option value="">-- Select Gravity --</option>');
            $.each(data || [], function (_, item) {
                var id = pick(item, ['GRAVITY_ID']);
                var name = pick(item, ['GRAVITY_NAME', 'NAME']);
                if (id !== '') {
                    ddl.append('<option value="' + safeAttr(id) + '">' + safeText(name) + '</option>');
                }
            });
        }).fail(function () {
            alert('Failed to load gravity list.');
        });
    }

    function updateAddButtons() {
        $('#btnAddSubKpi').prop('disabled', !currentMainKpiId);
        $('#btnAddProcess').prop('disabled', !currentSubKpiId);
        $('#btnAddSubProcess').prop('disabled', !currentProcessId);
        $('#btnAddAnnexure').prop('disabled', !currentSubProcessId);
    }

    function pick(item, keys) {
        if (!item) {
            return '';
        }
        for (var i = 0; i < keys.length; i++) {
            var key = keys[i];
            if (item[key] !== undefined && item[key] !== null) {
                return item[key];
            }
            var lowerKey = key.toLowerCase();
            if (item[lowerKey] !== undefined && item[lowerKey] !== null) {
                return item[lowerKey];
            }
            var upperKey = key.toUpperCase();
            if (item[upperKey] !== undefined && item[upperKey] !== null) {
                return item[upperKey];
            }
        }
        return '';
    }

    function safeText(value) {
        if (value === undefined || value === null) {
            return '';
        }
        return $('<div>').text(value).html();
    }

    function safeAttr(value) {
        if (value === undefined || value === null) {
            return '';
        }
        return $('<div>').text(value).html();
    }

    function valueOrNull(value) {
        if (value === undefined || value === null || value === '') {
            return null;
        }
        return value;
    }
})(jQuery);
