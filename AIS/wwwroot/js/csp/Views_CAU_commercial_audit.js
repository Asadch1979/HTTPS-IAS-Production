var commercialAuditPage = {
    stage: "",
    omList: [],
    pdpList: [],
    arpseList: [],
    selectedOmId: 0,
    selectedPdpId: 0,
    selectedArpseId: 0,
    selectedPdpMappings: [],
    omMode: "add",
    pdpMode: "add",
    arpseMode: "add",
    dacMode: "add",
    pacMode: "add",
    editingDacId: 0,
    editingPacId: 0
};

$(document).ready(function () {
    var page = $(".commercial-audit-page");
    if (!page.length) {
        return;
    }

    commercialAuditPage.stage = String(page.data("stage") || "").toLowerCase();

    if (commercialAuditPage.stage === "om") {
        initCommercialAuditOm();
    } else if (commercialAuditPage.stage === "pdp") {
        initCommercialAuditPdp();
    } else if (commercialAuditPage.stage === "arpse") {
        initCommercialAuditArpse();
    }
});

function initCommercialAuditOm() {
    $("#btnSaveOm").on("click", saveCommercialAuditOm);
    $("#btnCancelOmEdit").on("click", resetCommercialAuditOmForm);
    $("#tblCommercialOm").on("click", ".btn-edit-om", function () {
        var rowData = $(this).data("row");
        if (rowData) {
            populateCommercialAuditOmForm(rowData);
        }
    });

    loadCommercialAuditOms();
}

function loadCommercialAuditOms(selectionHint) {
    $.ajax({
        url: g_asiBaseURL + "/ApiCalls/get_commercial_audit_oms",
        type: "GET",
        cache: false,
        success: function (data) {
            commercialAuditPage.omList = Array.isArray(data) ? data.map(normalizeCommercialOm) : [];

            if (selectionHint) {
                commercialAuditPage.selectedOmId = resolveCommercialSelectionId(commercialAuditPage.omList, selectionHint, "OmId", "OmNo");
            }

            renderCommercialAuditOmTable(commercialAuditPage.omList);

            if (commercialAuditPage.selectedOmId) {
                var selected = findById(commercialAuditPage.omList, "OmId", commercialAuditPage.selectedOmId);
                if (selected) {
                    populateCommercialAuditOmForm(selected);
                }
            }
        },
        error: function (xhr) {
            commercialAuditPage.omList = [];
            renderCommercialAuditOmTable([]);
            showApiAlertFromXhr(xhr, xhr ? xhr.status : null, getErrorReferenceIdFromXhr(xhr), "Unable to load OM records.");
        }
    });
}

function renderCommercialAuditOmTable(list) {
    destroyDatatable("tblCommercialOm");

    var tbody = $("#tblCommercialOm tbody");
    tbody.empty();

    if (!list.length) {
        tbody.append('<tr><td colspan="5" class="text-center">No OM records found.</td></tr>');
        return;
    }

    list.forEach(function (item) {
        var row = $("<tr>").toggleClass("table-active", commercialAuditPage.selectedOmId === item.OmId);
        row.append($("<td>").text(item.AuditYearText));
        row.append($("<td>").text(item.OmNo));
        row.append($("<td>").text(item.GistOfOm));
        row.append($("<td>").text(item.ManagementResponse));

        var editButton = $("<button>")
            .addClass("btn btn-sm btn-primary btn-edit-om")
            .attr("type", "button")
            .text(commercialAuditPage.selectedOmId === item.OmId ? "Editing" : "Edit")
            .data("row", item);

        row.append($("<td>").addClass("text-center").append(editButton));
        tbody.append(row);
    });

    initializeDataTable("tblCommercialOm");
}

function populateCommercialAuditOmForm(item) {
    commercialAuditPage.omMode = "edit";
    commercialAuditPage.selectedOmId = item.OmId || 0;

    $("#omFormTitle").text("Update OM");
    $("#omAuditYear").val(item.AuditYearId);
    $("#omNo").val(item.OmNo || "");
    $("#omGist").val(item.GistOfOm || "");
    $("#omBody").val(item.BodyOfOm || "");
    $("#omManagementResponse").val(item.ManagementResponse || "");
    $("#btnSaveOm").text("Update OM");
    $("#btnCancelOmEdit").removeClass("d-none");
    renderCommercialAuditOmTable(commercialAuditPage.omList);
}

function resetCommercialAuditOmForm() {
    commercialAuditPage.omMode = "add";
    commercialAuditPage.selectedOmId = 0;

    $("#omFormTitle").text("Create OM");
    $("#omAuditYear").val("");
    $("#omNo").val("");
    $("#omGist").val("");
    $("#omBody").val("");
    $("#omManagementResponse").val("");
    $("#btnSaveOm").text("Save OM");
    $("#btnCancelOmEdit").addClass("d-none");
    renderCommercialAuditOmTable(commercialAuditPage.omList);
}

function saveCommercialAuditOm() {
    var model = {
        OmId: commercialAuditPage.omMode === "edit" ? commercialAuditPage.selectedOmId : 0,
        AuditYearId: parseNullableInt($("#omAuditYear").val()),
        OmNo: $("#omNo").val().trim(),
        GistOfOm: $("#omGist").val().trim(),
        BodyOfOm: $("#omBody").val().trim(),
        ManagementResponse: $("#omManagementResponse").val().trim(),
        IsActive: "Y"
    };

    if (!model.AuditYearId || !model.OmNo || !model.GistOfOm || !model.BodyOfOm) {
        alert("Audit Year, OM No, Gist of OM, and Body of OM are required.");
        return;
    }

    $.ajax({
        url: g_asiBaseURL + "/ApiCalls/save_commercial_audit_om",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(model),
        success: function (data) {
            if (!isCommercialAuditActionSuccessful(data)) {
                showApiAlert(data, "Unable to save OM.");
                return;
            }

            commercialAuditPage.selectedOmId = parseInt(coalesce(data.Id, data.id, 0), 10) || commercialAuditPage.selectedOmId;
            loadCommercialAuditOms({ OmId: commercialAuditPage.selectedOmId, OmNo: model.OmNo });
            showApiAlert(data, commercialAuditPage.omMode === "edit" ? "OM updated successfully." : "OM saved successfully.");
        },
        error: function (xhr) {
            showApiAlertFromXhr(xhr, xhr ? xhr.status : null, getErrorReferenceIdFromXhr(xhr), "Unable to save OM.");
        }
    });
}

function initCommercialAuditPdp() {
    $("#btnSavePdp").on("click", saveCommercialAuditPdp);
    $("#btnCancelPdpEdit").on("click", resetCommercialAuditPdpForm);
    $("#btnSavePdpMappings").on("click", saveCommercialAuditPdpMappings);

    $("#tblCommercialPdp").on("click", ".btn-edit-pdp", function () {
        var rowData = $(this).data("row");
        if (rowData) {
            populateCommercialAuditPdpForm(rowData);
            loadCommercialAuditPdpMappings(rowData.PdpId);
        }
    });

    loadCommercialAuditPdpOmLookup();
    loadCommercialAuditPdps();
}

function loadCommercialAuditPdpOmLookup() {
    $.ajax({
        url: g_asiBaseURL + "/ApiCalls/get_commercial_audit_oms",
        type: "GET",
        cache: false,
        success: function (data) {
            commercialAuditPage.omList = Array.isArray(data) ? data.map(normalizeCommercialOm) : [];
            renderCommercialAuditPdpOmLookup(commercialAuditPage.omList, commercialAuditPage.selectedPdpMappings);
        },
        error: function (xhr) {
            commercialAuditPage.omList = [];
            renderCommercialAuditPdpOmLookup([], []);
            showApiAlertFromXhr(xhr, xhr ? xhr.status : null, getErrorReferenceIdFromXhr(xhr), "Unable to load OM lookup.");
        }
    });
}

function loadCommercialAuditPdps(selectionHint) {
    $.ajax({
        url: g_asiBaseURL + "/ApiCalls/get_commercial_audit_pdps",
        type: "GET",
        cache: false,
        success: function (data) {
            commercialAuditPage.pdpList = Array.isArray(data) ? data.map(normalizeCommercialPdp) : [];

            if (selectionHint) {
                commercialAuditPage.selectedPdpId = resolveCommercialSelectionId(commercialAuditPage.pdpList, selectionHint, "PdpId", "PdpNo");
            }

            renderCommercialAuditPdpTable(commercialAuditPage.pdpList);

            if (commercialAuditPage.selectedPdpId) {
                var selected = findById(commercialAuditPage.pdpList, "PdpId", commercialAuditPage.selectedPdpId);
                if (selected) {
                    populateCommercialAuditPdpForm(selected);
                    loadCommercialAuditPdpMappings(selected.PdpId);
                }
            }
        },
        error: function (xhr) {
            commercialAuditPage.pdpList = [];
            renderCommercialAuditPdpTable([]);
            showApiAlertFromXhr(xhr, xhr ? xhr.status : null, getErrorReferenceIdFromXhr(xhr), "Unable to load PDP records.");
        }
    });
}

function renderCommercialAuditPdpTable(list) {
    destroyDatatable("tblCommercialPdp");

    var tbody = $("#tblCommercialPdp tbody");
    tbody.empty();

    if (!list.length) {
        tbody.append('<tr><td colspan="6" class="text-center">No PDP records found.</td></tr>');
        return;
    }

    list.forEach(function (item) {
        var row = $("<tr>").toggleClass("table-active", commercialAuditPage.selectedPdpId === item.PdpId);
        row.append($("<td>").text(item.AuditYearText));
        row.append($("<td>").text(item.PdpNo));
        row.append($("<td>").text(item.GistOfPdp));
        row.append($("<td>").text(item.UpdatedStatus));
        row.append($("<td>").text(item.LinkedOmNumbers || item.LinkedOmCount));

        var editButton = $("<button>")
            .addClass("btn btn-sm btn-primary btn-edit-pdp")
            .attr("type", "button")
            .text(commercialAuditPage.selectedPdpId === item.PdpId ? "Editing" : "Edit")
            .data("row", item);

        row.append($("<td>").addClass("text-center").append(editButton));
        tbody.append(row);
    });

    initializeDataTable("tblCommercialPdp");
}

function renderCommercialAuditPdpOmLookup(omList, selectedMappings) {
    var selectedOmIds = (selectedMappings || []).map(function (item) {
        return item.OmId;
    });

    var tbody = $("#tblPdpOmLookup tbody");
    tbody.empty();

    if (!omList.length) {
        tbody.append('<tr><td colspan="4" class="text-center">No OM records available for linking.</td></tr>');
        updateCommercialAuditPdpSelectedSummary();
        return;
    }

    omList.forEach(function (item) {
        var row = $("<tr>");
        var checkbox = $("<input>")
            .attr("type", "checkbox")
            .addClass("form-check-input commercial-pdp-om-checkbox")
            .attr("data-om-id", item.OmId)
            .prop("checked", selectedOmIds.indexOf(item.OmId) >= 0);

        row.append($("<td>").addClass("text-center").append(checkbox));
        row.append($("<td>").text(item.AuditYearText));
        row.append($("<td>").text(item.OmNo));
        row.append($("<td>").text(item.GistOfOm));
        tbody.append(row);
    });

    $(".commercial-pdp-om-checkbox").off("change").on("change", updateCommercialAuditPdpSelectedSummary);
    updateCommercialAuditPdpSelectedSummary();
}

function updateCommercialAuditPdpSelectedSummary() {
    var selected = $(".commercial-pdp-om-checkbox:checked").length;
    $("#pdpSelectedSummary").text(selected > 0 ? selected + " OMs selected" : "No OMs selected");
}

function populateCommercialAuditPdpForm(item) {
    commercialAuditPage.pdpMode = "edit";
    commercialAuditPage.selectedPdpId = item.PdpId || 0;

    $("#pdpFormTitle").text("Update PDP");
    $("#pdpAuditYear").val(item.AuditYearId);
    $("#pdpNo").val(item.PdpNo || "");
    $("#pdpGist").val(item.GistOfPdp || "");
    $("#pdpBody").val(item.BodyOfPdp || "");
    $("#pdpManagementResponse").val(item.ManagementResponse || "");
    $("#pdpDacRecommendations").val(item.DacRecommendations || "");
    $("#pdpUpdatedStatus").val(item.UpdatedStatus || "");
    $("#btnSavePdp").text("Update PDP");
    $("#btnCancelPdpEdit").removeClass("d-none");
    $("#pdpOmMappingFieldset").prop("disabled", false);
    $("#pdpMappingHint").text("Select one or more OMs and save the mapping for the selected PDP.");
    renderCommercialAuditPdpTable(commercialAuditPage.pdpList);
}

function resetCommercialAuditPdpForm() {
    commercialAuditPage.pdpMode = "add";
    commercialAuditPage.selectedPdpId = 0;
    commercialAuditPage.selectedPdpMappings = [];

    $("#pdpFormTitle").text("Create PDP");
    $("#pdpAuditYear").val("");
    $("#pdpNo").val("");
    $("#pdpGist").val("");
    $("#pdpBody").val("");
    $("#pdpManagementResponse").val("");
    $("#pdpDacRecommendations").val("");
    $("#pdpUpdatedStatus").val("");
    $("#btnSavePdp").text("Save PDP");
    $("#btnCancelPdpEdit").addClass("d-none");
    $("#pdpOmMappingFieldset").prop("disabled", true);
    $("#pdpMappingHint").text("Save or select a PDP first, then choose one or more OMs to link.");
    renderCommercialAuditPdpOmLookup(commercialAuditPage.omList, []);
    renderCommercialAuditPdpTable(commercialAuditPage.pdpList);
}

function saveCommercialAuditPdp() {
    var model = {
        PdpId: commercialAuditPage.pdpMode === "edit" ? commercialAuditPage.selectedPdpId : 0,
        AuditYearId: parseNullableInt($("#pdpAuditYear").val()),
        PdpNo: $("#pdpNo").val().trim(),
        GistOfPdp: $("#pdpGist").val().trim(),
        BodyOfPdp: $("#pdpBody").val().trim(),
        ManagementResponse: $("#pdpManagementResponse").val().trim(),
        DacRecommendations: $("#pdpDacRecommendations").val().trim(),
        UpdatedStatus: $("#pdpUpdatedStatus").val().trim(),
        IsActive: "Y"
    };

    if (!model.AuditYearId || !model.PdpNo || !model.GistOfPdp || !model.BodyOfPdp) {
        alert("Audit Year, PDP No, Gist of PDP, and Body of PDP are required.");
        return;
    }

    $.ajax({
        url: g_asiBaseURL + "/ApiCalls/save_commercial_audit_pdp",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(model),
        success: function (data) {
            if (!isCommercialAuditActionSuccessful(data)) {
                showApiAlert(data, "Unable to save PDP.");
                return;
            }

            commercialAuditPage.selectedPdpId = parseInt(coalesce(data.Id, data.id, 0), 10) || commercialAuditPage.selectedPdpId;
            loadCommercialAuditPdps({ PdpId: commercialAuditPage.selectedPdpId, PdpNo: model.PdpNo });
            $("#pdpOmMappingFieldset").prop("disabled", false);
            $("#pdpMappingHint").text("Select one or more OMs and save the mapping for the selected PDP.");
            showApiAlert(data, commercialAuditPage.pdpMode === "edit" ? "PDP updated successfully." : "PDP saved successfully.");
        },
        error: function (xhr) {
            showApiAlertFromXhr(xhr, xhr ? xhr.status : null, getErrorReferenceIdFromXhr(xhr), "Unable to save PDP.");
        }
    });
}

function loadCommercialAuditPdpMappings(pdpId) {
    if (!pdpId) {
        commercialAuditPage.selectedPdpMappings = [];
        renderCommercialAuditPdpOmLookup(commercialAuditPage.omList, []);
        return;
    }

    $.ajax({
        url: g_asiBaseURL + "/ApiCalls/get_commercial_audit_pdp_om_mappings",
        type: "GET",
        cache: false,
        data: { pdp_id: pdpId },
        success: function (data) {
            commercialAuditPage.selectedPdpMappings = Array.isArray(data) ? data.map(normalizeCommercialPdpOmMapping) : [];
            renderCommercialAuditPdpOmLookup(commercialAuditPage.omList, commercialAuditPage.selectedPdpMappings);
        },
        error: function (xhr) {
            commercialAuditPage.selectedPdpMappings = [];
            renderCommercialAuditPdpOmLookup(commercialAuditPage.omList, []);
            showApiAlertFromXhr(xhr, xhr ? xhr.status : null, getErrorReferenceIdFromXhr(xhr), "Unable to load PDP OM mappings.");
        }
    });
}

function saveCommercialAuditPdpMappings() {
    if (!commercialAuditPage.selectedPdpId) {
        alert("Save or select a PDP first.");
        return;
    }

    var omIds = $(".commercial-pdp-om-checkbox:checked").map(function () {
        return parseInt($(this).attr("data-om-id"), 10) || 0;
    }).get().filter(function (value) {
        return value > 0;
    });

    $.ajax({
        url: g_asiBaseURL + "/ApiCalls/save_commercial_audit_pdp_om_mapping",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({
            PdpId: commercialAuditPage.selectedPdpId,
            OmIds: omIds,
            IsActive: "Y"
        }),
        success: function (data) {
            if (!isCommercialAuditActionSuccessful(data)) {
                showApiAlert(data, "Unable to save linked OMs.");
                return;
            }

            loadCommercialAuditPdps({ PdpId: commercialAuditPage.selectedPdpId });
            showApiAlert(data, "Linked OMs saved successfully.");
        },
        error: function (xhr) {
            showApiAlertFromXhr(xhr, xhr ? xhr.status : null, getErrorReferenceIdFromXhr(xhr), "Unable to save linked OMs.");
        }
    });
}

function initCommercialAuditArpse() {
    $("#btnSaveArpse").on("click", saveCommercialAuditArpseHeader);
    $("#btnCancelArpseEdit").on("click", resetCommercialAuditArpseForm);
    $("#btnSaveArpseDac").on("click", saveCommercialAuditArpseDacEntry);
    $("#btnCancelArpseDacEdit").on("click", resetCommercialAuditArpseDacForm);
    $("#btnSaveArpsePac").on("click", saveCommercialAuditArpsePacEntry);
    $("#btnCancelArpsePacEdit").on("click", resetCommercialAuditArpsePacForm);

    $("#tblCommercialArpse").on("click", ".btn-edit-arpse", function () {
        var rowData = $(this).data("row");
        if (rowData) {
            populateCommercialAuditArpseHeaderForm(rowData);
            loadCommercialAuditArpseChildren(rowData.ArpseId);
        }
    });

    $("#tblArpseDac").on("click", ".btn-edit-arpse-dac", function () {
        var rowData = $(this).data("row");
        if (rowData) {
            populateCommercialAuditArpseDacForm(rowData);
        }
    });

    $("#tblArpsePac").on("click", ".btn-edit-arpse-pac", function () {
        var rowData = $(this).data("row");
        if (rowData) {
            populateCommercialAuditArpsePacForm(rowData);
        }
    });

    loadCommercialAuditArpseHeaders();
}

function loadCommercialAuditArpseHeaders(selectionHint) {
    $.ajax({
        url: g_asiBaseURL + "/ApiCalls/get_commercial_audit_arpse_headers",
        type: "GET",
        cache: false,
        success: function (data) {
            commercialAuditPage.arpseList = Array.isArray(data) ? data.map(normalizeCommercialArpseHeader) : [];

            if (selectionHint) {
                commercialAuditPage.selectedArpseId = resolveCommercialSelectionId(commercialAuditPage.arpseList, selectionHint, "ArpseId", "ParaNo");
            }

            renderCommercialAuditArpseTable(commercialAuditPage.arpseList);

            if (commercialAuditPage.selectedArpseId) {
                var selected = findById(commercialAuditPage.arpseList, "ArpseId", commercialAuditPage.selectedArpseId);
                if (selected) {
                    populateCommercialAuditArpseHeaderForm(selected);
                    loadCommercialAuditArpseChildren(selected.ArpseId);
                }
            }
        },
        error: function (xhr) {
            commercialAuditPage.arpseList = [];
            renderCommercialAuditArpseTable([]);
            showApiAlertFromXhr(xhr, xhr ? xhr.status : null, getErrorReferenceIdFromXhr(xhr), "Unable to load ARPSE headers.");
        }
    });
}

function renderCommercialAuditArpseTable(list) {
    destroyDatatable("tblCommercialArpse");

    var tbody = $("#tblCommercialArpse tbody");
    tbody.empty();

    if (!list.length) {
        tbody.append('<tr><td colspan="5" class="text-center">No ARPSE headers found.</td></tr>');
        return;
    }

    list.forEach(function (item) {
        var row = $("<tr>").toggleClass("table-active", commercialAuditPage.selectedArpseId === item.ArpseId);
        row.append($("<td>").text(item.ArpseYearText));
        row.append($("<td>").text(item.ParaNo));
        row.append($("<td>").text(item.GistOfPara));
        row.append($("<td>").text(item.ManagementResponse));

        var editButton = $("<button>")
            .addClass("btn btn-sm btn-primary btn-edit-arpse")
            .attr("type", "button")
            .text(commercialAuditPage.selectedArpseId === item.ArpseId ? "Editing" : "Edit")
            .data("row", item);

        row.append($("<td>").addClass("text-center").append(editButton));
        tbody.append(row);
    });

    initializeDataTable("tblCommercialArpse");
}

function populateCommercialAuditArpseHeaderForm(item) {
    commercialAuditPage.arpseMode = "edit";
    commercialAuditPage.selectedArpseId = item.ArpseId || 0;

    $("#arpseFormTitle").text("Update ARPSE Header");
    $("#arpseYear").val(item.ArpseYearId);
    $("#arpseParaNo").val(item.ParaNo || "");
    $("#arpseGist").val(item.GistOfPara || "");
    $("#arpseManagementResponse").val(item.ManagementResponse || "");
    $("#btnSaveArpse").text("Update Header");
    $("#btnCancelArpseEdit").removeClass("d-none");
    $("#arpseChildrenFieldset").prop("disabled", false);
    renderCommercialAuditArpseTable(commercialAuditPage.arpseList);
}

function resetCommercialAuditArpseForm() {
    commercialAuditPage.arpseMode = "add";
    commercialAuditPage.selectedArpseId = 0;

    $("#arpseFormTitle").text("Create ARPSE Header");
    $("#arpseYear").val("");
    $("#arpseParaNo").val("");
    $("#arpseGist").val("");
    $("#arpseManagementResponse").val("");
    $("#btnSaveArpse").text("Save Header");
    $("#btnCancelArpseEdit").addClass("d-none");
    $("#arpseChildrenFieldset").prop("disabled", true);
    resetCommercialAuditArpseDacForm();
    resetCommercialAuditArpsePacForm();
    renderCommercialAuditArpseDacTable([]);
    renderCommercialAuditArpsePacTable([]);
    renderCommercialAuditArpseTable(commercialAuditPage.arpseList);
}

function saveCommercialAuditArpseHeader() {
    var model = {
        ArpseId: commercialAuditPage.arpseMode === "edit" ? commercialAuditPage.selectedArpseId : 0,
        ArpseYearId: parseNullableInt($("#arpseYear").val()),
        ParaNo: $("#arpseParaNo").val().trim(),
        GistOfPara: $("#arpseGist").val().trim(),
        ManagementResponse: $("#arpseManagementResponse").val().trim(),
        IsActive: "Y"
    };

    if (!model.ArpseYearId || !model.ParaNo || !model.GistOfPara) {
        alert("ARPSE Year, Para No, and Gist of Para are required.");
        return;
    }

    $.ajax({
        url: g_asiBaseURL + "/ApiCalls/save_commercial_audit_arpse_header",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(model),
        success: function (data) {
            if (!isCommercialAuditActionSuccessful(data)) {
                showApiAlert(data, "Unable to save ARPSE header.");
                return;
            }

            commercialAuditPage.selectedArpseId = parseInt(coalesce(data.Id, data.id, 0), 10) || commercialAuditPage.selectedArpseId;
            $("#arpseChildrenFieldset").prop("disabled", false);
            loadCommercialAuditArpseHeaders({ ArpseId: commercialAuditPage.selectedArpseId, ParaNo: model.ParaNo });
            showApiAlert(data, commercialAuditPage.arpseMode === "edit" ? "ARPSE header updated successfully." : "ARPSE header saved successfully.");
        },
        error: function (xhr) {
            showApiAlertFromXhr(xhr, xhr ? xhr.status : null, getErrorReferenceIdFromXhr(xhr), "Unable to save ARPSE header.");
        }
    });
}

function loadCommercialAuditArpseChildren(arpseId) {
    if (!arpseId) {
        renderCommercialAuditArpseDacTable([]);
        renderCommercialAuditArpsePacTable([]);
        return;
    }

    $("#arpseChildrenFieldset").prop("disabled", false);
    loadCommercialAuditArpseDacEntries(arpseId);
    loadCommercialAuditArpsePacEntries(arpseId);
}

function loadCommercialAuditArpseDacEntries(arpseId) {
    $.ajax({
        url: g_asiBaseURL + "/ApiCalls/get_commercial_audit_arpse_dac_entries",
        type: "GET",
        cache: false,
        data: { arpse_id: arpseId },
        success: function (data) {
            renderCommercialAuditArpseDacTable(Array.isArray(data) ? data.map(normalizeCommercialArpseDac) : []);
        },
        error: function (xhr) {
            renderCommercialAuditArpseDacTable([]);
            showApiAlertFromXhr(xhr, xhr ? xhr.status : null, getErrorReferenceIdFromXhr(xhr), "Unable to load DAC entries.");
        }
    });
}

function loadCommercialAuditArpsePacEntries(arpseId) {
    $.ajax({
        url: g_asiBaseURL + "/ApiCalls/get_commercial_audit_arpse_pac_entries",
        type: "GET",
        cache: false,
        data: { arpse_id: arpseId },
        success: function (data) {
            renderCommercialAuditArpsePacTable(Array.isArray(data) ? data.map(normalizeCommercialArpsePac) : []);
        },
        error: function (xhr) {
            renderCommercialAuditArpsePacTable([]);
            showApiAlertFromXhr(xhr, xhr ? xhr.status : null, getErrorReferenceIdFromXhr(xhr), "Unable to load PAC entries.");
        }
    });
}

function renderCommercialAuditArpseDacTable(list) {
    var tbody = $("#tblArpseDac tbody");
    tbody.empty();

    if (!list.length) {
        tbody.append('<tr><td colspan="4" class="text-center">No DAC entries found.</td></tr>');
        return;
    }

    list.forEach(function (item) {
        var row = $("<tr>");
        row.append($("<td>").text(item.DacRecommendation));
        row.append($("<td>").text(formatDisplayDate(item.DacDate)));
        row.append($("<td>").text(item.UpdatedStatus));

        var editButton = $("<button>")
            .addClass("btn btn-sm btn-primary btn-edit-arpse-dac")
            .attr("type", "button")
            .text("Edit")
            .data("row", item);

        row.append($("<td>").addClass("text-center").append(editButton));
        tbody.append(row);
    });
}

function renderCommercialAuditArpsePacTable(list) {
    var tbody = $("#tblArpsePac tbody");
    tbody.empty();

    if (!list.length) {
        tbody.append('<tr><td colspan="4" class="text-center">No PAC entries found.</td></tr>');
        return;
    }

    list.forEach(function (item) {
        var row = $("<tr>");
        row.append($("<td>").text(item.PacDirective));
        row.append($("<td>").text(formatDisplayDate(item.PacDate)));
        row.append($("<td>").text(item.UpdatedStatus));

        var editButton = $("<button>")
            .addClass("btn btn-sm btn-primary btn-edit-arpse-pac")
            .attr("type", "button")
            .text("Edit")
            .data("row", item);

        row.append($("<td>").addClass("text-center").append(editButton));
        tbody.append(row);
    });
}

function populateCommercialAuditArpseDacForm(item) {
    commercialAuditPage.dacMode = "edit";
    commercialAuditPage.editingDacId = item.DacEntryId || 0;

    $("#arpseDacRecommendation").val(item.DacRecommendation || "");
    $("#arpseDacDate").val(formatInputDate(item.DacDate));
    $("#arpseDacUpdatedStatus").val(item.UpdatedStatus || "");
    $("#btnSaveArpseDac").text("Update DAC");
    $("#btnCancelArpseDacEdit").removeClass("d-none");
}

function resetCommercialAuditArpseDacForm() {
    commercialAuditPage.dacMode = "add";
    commercialAuditPage.editingDacId = 0;

    $("#arpseDacRecommendation").val("");
    $("#arpseDacDate").val("");
    $("#arpseDacUpdatedStatus").val("");
    $("#btnSaveArpseDac").text("Add DAC");
    $("#btnCancelArpseDacEdit").addClass("d-none");
}

function saveCommercialAuditArpseDacEntry() {
    if (!commercialAuditPage.selectedArpseId) {
        alert("Save or select an ARPSE header first.");
        return;
    }

    var model = {
        DacEntryId: commercialAuditPage.dacMode === "edit" ? commercialAuditPage.editingDacId : 0,
        ArpseId: commercialAuditPage.selectedArpseId,
        DacRecommendation: $("#arpseDacRecommendation").val().trim(),
        DacDate: $("#arpseDacDate").val() || null,
        UpdatedStatus: $("#arpseDacUpdatedStatus").val().trim(),
        IsActive: "Y"
    };

    if (!model.DacRecommendation) {
        alert("DAC Recommendation is required.");
        return;
    }

    $.ajax({
        url: g_asiBaseURL + "/ApiCalls/save_commercial_audit_arpse_dac_entry",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(model),
        success: function (data) {
            if (!isCommercialAuditActionSuccessful(data)) {
                showApiAlert(data, "Unable to save DAC entry.");
                return;
            }

            var wasEdit = commercialAuditPage.dacMode === "edit";
            resetCommercialAuditArpseDacForm();
            loadCommercialAuditArpseDacEntries(commercialAuditPage.selectedArpseId);
            showApiAlert(data, wasEdit ? "DAC entry updated successfully." : "DAC entry saved successfully.");
        },
        error: function (xhr) {
            showApiAlertFromXhr(xhr, xhr ? xhr.status : null, getErrorReferenceIdFromXhr(xhr), "Unable to save DAC entry.");
        }
    });
}

function populateCommercialAuditArpsePacForm(item) {
    commercialAuditPage.pacMode = "edit";
    commercialAuditPage.editingPacId = item.PacEntryId || 0;

    $("#arpsePacDirective").val(item.PacDirective || "");
    $("#arpsePacDate").val(formatInputDate(item.PacDate));
    $("#arpsePacUpdatedStatus").val(item.UpdatedStatus || "");
    $("#btnSaveArpsePac").text("Update PAC");
    $("#btnCancelArpsePacEdit").removeClass("d-none");
}

function resetCommercialAuditArpsePacForm() {
    commercialAuditPage.pacMode = "add";
    commercialAuditPage.editingPacId = 0;

    $("#arpsePacDirective").val("");
    $("#arpsePacDate").val("");
    $("#arpsePacUpdatedStatus").val("");
    $("#btnSaveArpsePac").text("Add PAC");
    $("#btnCancelArpsePacEdit").addClass("d-none");
}

function saveCommercialAuditArpsePacEntry() {
    if (!commercialAuditPage.selectedArpseId) {
        alert("Save or select an ARPSE header first.");
        return;
    }

    var model = {
        PacEntryId: commercialAuditPage.pacMode === "edit" ? commercialAuditPage.editingPacId : 0,
        ArpseId: commercialAuditPage.selectedArpseId,
        PacDirective: $("#arpsePacDirective").val().trim(),
        PacDate: $("#arpsePacDate").val() || null,
        UpdatedStatus: $("#arpsePacUpdatedStatus").val().trim(),
        IsActive: "Y"
    };

    if (!model.PacDirective) {
        alert("PAC Directive is required.");
        return;
    }

    $.ajax({
        url: g_asiBaseURL + "/ApiCalls/save_commercial_audit_arpse_pac_entry",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(model),
        success: function (data) {
            if (!isCommercialAuditActionSuccessful(data)) {
                showApiAlert(data, "Unable to save PAC entry.");
                return;
            }

            var wasEdit = commercialAuditPage.pacMode === "edit";
            resetCommercialAuditArpsePacForm();
            loadCommercialAuditArpsePacEntries(commercialAuditPage.selectedArpseId);
            showApiAlert(data, wasEdit ? "PAC entry updated successfully." : "PAC entry saved successfully.");
        },
        error: function (xhr) {
            showApiAlertFromXhr(xhr, xhr ? xhr.status : null, getErrorReferenceIdFromXhr(xhr), "Unable to save PAC entry.");
        }
    });
}

function normalizeCommercialOm(item) {
    return {
        OmId: parseInt(coalesce(item.OmId, item.omId, item.OM_ID, 0), 10) || 0,
        AuditYearId: parseInt(coalesce(item.AuditYearId, item.auditYearId, item.AUDIT_YEAR_ID, 0), 10) || 0,
        AuditYearText: String(coalesce(item.AuditYearText, item.auditYearText, item.AUDIT_YEAR_TEXT, "") || ""),
        OmNo: String(coalesce(item.OmNo, item.omNo, item.OM_NO, "") || ""),
        GistOfOm: String(coalesce(item.GistOfOm, item.gistOfOm, item.GIST_OF_OM, "") || ""),
        BodyOfOm: String(coalesce(item.BodyOfOm, item.bodyOfOm, item.BODY_OF_OM, "") || ""),
        ManagementResponse: String(coalesce(item.ManagementResponse, item.managementResponse, item.MANAGEMENT_RESPONSE, "") || ""),
        IsActive: String(coalesce(item.IsActive, item.isActive, item.IS_ACTIVE, "Y") || "Y")
    };
}

function normalizeCommercialPdp(item) {
    return {
        PdpId: parseInt(coalesce(item.PdpId, item.pdpId, item.PDP_ID, 0), 10) || 0,
        AuditYearId: parseInt(coalesce(item.AuditYearId, item.auditYearId, item.AUDIT_YEAR_ID, 0), 10) || 0,
        AuditYearText: String(coalesce(item.AuditYearText, item.auditYearText, item.AUDIT_YEAR_TEXT, "") || ""),
        PdpNo: String(coalesce(item.PdpNo, item.pdpNo, item.PDP_NO, "") || ""),
        GistOfPdp: String(coalesce(item.GistOfPdp, item.gistOfPdp, item.GIST_OF_PDP, "") || ""),
        BodyOfPdp: String(coalesce(item.BodyOfPdp, item.bodyOfPdp, item.BODY_OF_PDP, "") || ""),
        ManagementResponse: String(coalesce(item.ManagementResponse, item.managementResponse, item.MANAGEMENT_RESPONSE, "") || ""),
        DacRecommendations: String(coalesce(item.DacRecommendations, item.dacRecommendations, item.DAC_RECOMMENDATIONS, "") || ""),
        UpdatedStatus: String(coalesce(item.UpdatedStatus, item.updatedStatus, item.UPDATED_STATUS, "") || ""),
        LinkedOmCount: parseInt(coalesce(item.LinkedOmCount, item.linkedOmCount, item.LINKED_OM_COUNT, 0), 10) || 0,
        LinkedOmNumbers: String(coalesce(item.LinkedOmNumbers, item.linkedOmNumbers, item.LINKED_OM_NUMBERS, "") || ""),
        IsActive: String(coalesce(item.IsActive, item.isActive, item.IS_ACTIVE, "Y") || "Y")
    };
}

function normalizeCommercialPdpOmMapping(item) {
    return {
        MappingId: parseInt(coalesce(item.MappingId, item.mappingId, item.MAPPING_ID, 0), 10) || 0,
        PdpId: parseInt(coalesce(item.PdpId, item.pdpId, item.PDP_ID, 0), 10) || 0,
        OmId: parseInt(coalesce(item.OmId, item.omId, item.OM_ID, 0), 10) || 0,
        OmNo: String(coalesce(item.OmNo, item.omNo, item.OM_NO, "") || ""),
        GistOfOm: String(coalesce(item.GistOfOm, item.gistOfOm, item.GIST_OF_OM, "") || "")
    };
}

function normalizeCommercialArpseHeader(item) {
    return {
        ArpseId: parseInt(coalesce(item.ArpseId, item.arpseId, item.ARPSE_ID, 0), 10) || 0,
        ArpseYearId: parseInt(coalesce(item.ArpseYearId, item.arpseYearId, item.ARPSE_YEAR_ID, 0), 10) || 0,
        ArpseYearText: String(coalesce(item.ArpseYearText, item.arpseYearText, item.ARPSE_YEAR_TEXT, "") || ""),
        ParaNo: String(coalesce(item.ParaNo, item.paraNo, item.PARA_NO, "") || ""),
        GistOfPara: String(coalesce(item.GistOfPara, item.gistOfPara, item.GIST_OF_PARA, "") || ""),
        ManagementResponse: String(coalesce(item.ManagementResponse, item.managementResponse, item.MANAGEMENT_RESPONSE, "") || "")
    };
}

function normalizeCommercialArpseDac(item) {
    return {
        DacEntryId: parseInt(coalesce(item.DacEntryId, item.dacEntryId, item.DAC_ENTRY_ID, 0), 10) || 0,
        ArpseId: parseInt(coalesce(item.ArpseId, item.arpseId, item.ARPSE_ID, 0), 10) || 0,
        DacRecommendation: String(coalesce(item.DacRecommendation, item.dacRecommendation, item.DAC_RECOMMENDATION, "") || ""),
        DacDate: coalesce(item.DacDate, item.dacDate, item.DAC_DATE, null),
        UpdatedStatus: String(coalesce(item.UpdatedStatus, item.updatedStatus, item.UPDATED_STATUS, "") || "")
    };
}

function normalizeCommercialArpsePac(item) {
    return {
        PacEntryId: parseInt(coalesce(item.PacEntryId, item.pacEntryId, item.PAC_ENTRY_ID, 0), 10) || 0,
        ArpseId: parseInt(coalesce(item.ArpseId, item.arpseId, item.ARPSE_ID, 0), 10) || 0,
        PacDirective: String(coalesce(item.PacDirective, item.pacDirective, item.PAC_DIRECTIVE, "") || ""),
        PacDate: coalesce(item.PacDate, item.pacDate, item.PAC_DATE, null),
        UpdatedStatus: String(coalesce(item.UpdatedStatus, item.updatedStatus, item.UPDATED_STATUS, "") || "")
    };
}

function resolveCommercialSelectionId(list, hint, idKey, numberKey) {
    if (hint && hint[idKey]) {
        return parseInt(hint[idKey], 10) || 0;
    }

    if (!hint || !hint[numberKey]) {
        return 0;
    }

    var matched = list.find(function (item) {
        return normalizeText(item[numberKey]) === normalizeText(hint[numberKey]);
    });

    return matched ? matched[idKey] : 0;
}

function findById(list, key, id) {
    return list.find(function (item) {
        return item[key] === id;
    }) || null;
}

function parseNullableInt(value) {
    var parsed = parseInt(value, 10);
    return isNaN(parsed) ? null : parsed;
}

function formatDisplayDate(value) {
    if (!value) {
        return "";
    }

    var date = new Date(value);
    if (isNaN(date.getTime())) {
        return String(value);
    }

    var day = ("0" + date.getDate()).slice(-2);
    var month = ("0" + (date.getMonth() + 1)).slice(-2);
    var year = date.getFullYear();
    return day + "/" + month + "/" + year;
}

function formatInputDate(value) {
    if (!value) {
        return "";
    }

    var date = new Date(value);
    if (isNaN(date.getTime())) {
        return "";
    }

    var day = ("0" + date.getDate()).slice(-2);
    var month = ("0" + (date.getMonth() + 1)).slice(-2);
    return date.getFullYear() + "-" + month + "-" + day;
}

function isCommercialAuditActionSuccessful(payload) {
    if (!payload) {
        return false;
    }

    if (typeof payload === "string") {
        return payload.trim().length > 0;
    }

    var status = String(coalesce(payload.Status, payload.status, "") || "").toLowerCase();
    if (status) {
        return status === "success" || status === "true" || status === "1" || status === "ok";
    }

    return parseInt(coalesce(payload.Id, payload.id, 0), 10) > 0;
}

function normalizeText(value) {
    return String(value || "").trim().toLowerCase();
}

function coalesce() {
    for (var i = 0; i < arguments.length; i++) {
        var value = arguments[i];
        if (value !== undefined && value !== null && value !== "") {
            return value;
        }
    }

    return null;
}
