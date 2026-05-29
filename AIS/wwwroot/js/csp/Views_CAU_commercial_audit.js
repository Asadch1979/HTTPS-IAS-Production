var commercialAuditPage = {
    loadUrl: "",
    currentStepKey: "",
    workflowSteps: [],
    yearLookups: {},
    omList: [],
    pdpList: [],
    arpseList: [],
    selectedOmId: 0,
    selectedPdpId: 0,
    selectedArpseId: 0,
    selectedOmSnapshot: null,
    selectedPdpSnapshot: null,
    selectedArpseSnapshot: null,
    selectedPdpMappings: [],
    selectedArpsePdpMappings: [],
    omMode: "add",
    pdpMode: "add",
    arpseMode: "add",
    dacMode: "add",
    pacMode: "add",
    editingDacId: 0,
    editingPacId: 0
};

var commercialAuditRichTextEditorIds = [
    "omBody",
    "omManagementResponse",
    "pdpBody",
    "pdpManagementResponse",
    "pdpDacRecommendations",
    "pdpUpdateManagementResponse",
    "arpseBody",
    "arpseManagementResponse",
    "arpseDacRecommendation",
    "arpseDacUpdatedStatus",
    "arpsePacUpdatedStatus",
    "arpsePacDirective"
];

$(document).ready(function () {
    var workflow = $(".commercial-audit-workflow");
    if (!workflow.length) {
        return;
    }

    commercialAuditPage.loadUrl = String(workflow.data("load-url") || "");
    var requestedInitialStepKey = normalizeText(workflow.data("current-step"));
    commercialAuditPage.currentStepKey = "";
    commercialAuditPage.workflowSteps = Array.isArray(window.commercialAuditWorkflowSteps) ? window.commercialAuditWorkflowSteps.slice() : [];
    commercialAuditPage.yearLookups = window.commercialAuditYearLookups || {};

    bindCommercialAuditWorkflowNavigation();

    var initialStep = resolveCommercialAuditStep(requestedInitialStepKey)
        || (commercialAuditPage.workflowSteps.length ? commercialAuditPage.workflowSteps[0] : null);

    if (!initialStep) {
        $("#commercialAuditStepHost").html('<div class="alert alert-warning mb-0">No Commercial Audit steps are available.</div>');
        return;
    }

    syncCommercialAuditWorkflowChrome(initialStep.stepKey, false);
    loadCommercialAuditStep(initialStep.stepKey);
});

function bindCommercialAuditWorkflowNavigation() {
    var workflow = $(".commercial-audit-workflow");

    workflow.on("click", ".commercial-audit-step-link, .commercial-audit-step-target", function (event) {
        var stepKey = normalizeText($(this).attr("data-step") || $(this).attr("data-step-key"));
        if (!stepKey) {
            return;
        }

        event.preventDefault();

        if (stepKey === commercialAuditPage.currentStepKey) {
            syncCommercialAuditWorkflowChrome(stepKey);
            initCommercialAuditStep(stepKey);
            return;
        }

        loadCommercialAuditStep(stepKey);
    });
}

function loadCommercialAuditStep(stepKey) {
    var resolvedStep = resolveCommercialAuditStep(stepKey);
    if (!resolvedStep || !commercialAuditPage.loadUrl) {
        return;
    }

    destroyCommercialAuditRichTextEditors();
    setCommercialAuditStepMessage("", "");
    $("#commercialAuditStepHost").html('<div class="alert alert-secondary mb-0">Loading workflow content...</div>');

    syncCommercialAuditWorkflowChrome(resolvedStep.stepKey, false);

    var requestQuery = buildCommercialAuditStepRequestQuery(resolvedStep.stepKey);
    requestQuery.append("_", Date.now());

    fetch(commercialAuditPage.loadUrl + "?" + requestQuery.toString(), {
        method: "GET",
        credentials: "same-origin",
        cache: "no-store",
        headers: {
            "X-Requested-With": "XMLHttpRequest",
            "Accept": "text/html, */*; q=0.01"
        }
    })
        .then(function (response) {
            return response.text().then(function (html) {
                return {
                    ok: response.ok,
                    status: response.status,
                    html: html
                };
            });
        })
        .then(function (payload) {
            if (!payload.ok) {
                throw payload;
            }

            if (looksLikeCommercialAuditFullHtmlDocument(payload.html)) {
                console.error("Commercial Audit step request returned a full HTML document.", {
                    stepKey: resolvedStep.stepKey,
                    status: payload.status,
                    preview: String(payload.html || "").substring(0, 300)
                });

                showCommercialAuditStepLoadFailure("Unexpected HTML response. Please try again.");
                return;
            }

            $("#commercialAuditStepHost").html(payload.html);
            executeCommercialAuditInlineScripts(document.getElementById("commercialAuditStepHost"));
            commercialAuditPage.currentStepKey = resolvedStep.stepKey;
            $(".commercial-audit-workflow").attr("data-current-step", resolvedStep.stepKey);
            $("#commercialAuditStepHost")
                .attr("data-step", resolvedStep.stepKey)
                .attr("data-step-key", resolvedStep.stepKey);
            syncCommercialAuditWorkflowChrome(resolvedStep.stepKey);
            initCommercialAuditStep(resolvedStep.stepKey);
        })
        .catch(function (error) {
            var message = resolveCommercialAuditStepLoadErrorMessage(error);
            console.error("Unable to load Commercial Audit step.", {
                stepKey: resolvedStep.stepKey,
                status: error && error.status ? error.status : null
            });
            showCommercialAuditStepLoadFailure(message);
        });
}

function syncCommercialAuditWorkflowChrome(stepKey, setCurrentStep) {
    var resolvedStep = resolveCommercialAuditStep(stepKey);
    if (!resolvedStep) {
        return;
    }

    if (setCurrentStep !== false) {
        commercialAuditPage.currentStepKey = resolvedStep.stepKey;
    }

    $(".commercial-audit-step-link").removeClass("active is-active");
    $('.commercial-audit-step-link[data-step="' + resolvedStep.stepKey + '"], .commercial-audit-step-link[data-step-key="' + resolvedStep.stepKey + '"]').addClass("active is-active");

    $("#commercialAuditStepCounter").text("Step " + resolvedStep.stepNo + " of " + commercialAuditPage.workflowSteps.length);
    $("#commercialAuditCurrentStage").text("Stage " + String(resolvedStep.stageKey || "").toUpperCase());
    $("#commercialAuditCurrentTitle").text(resolvedStep.title || "");
    $("#commercialAuditCurrentDescription").text(resolvedStep.description || "");
}

function resolveCommercialAuditStep(stepKey) {
    var normalizedStepKey = normalizeText(stepKey);
    if (!normalizedStepKey || !commercialAuditPage.workflowSteps.length) {
        return null;
    }

    return commercialAuditPage.workflowSteps.find(function (step) {
        return normalizeText(step.stepKey) === normalizedStepKey;
    }) || null;
}

function initCommercialAuditStep(stepKey) {
    switch (normalizeText(stepKey)) {
        case "om-entry":
            initCommercialAuditOmEntry();
            break;
        case "om-register":
            initCommercialAuditOmRegister();
            break;
        case "pdp-entry":
            initCommercialAuditPdpEntry();
            break;
        case "pdp-linking":
            initCommercialAuditPdpLinking();
            break;
        case "arpse-header":
            initCommercialAuditArpseHeader();
            break;
        case "arpse-linking":
            initCommercialAuditArpseLinking();
            break;
        case "arpse-monitoring":
            initCommercialAuditArpseMonitoring();
            break;
        default:
            break;
    }
}

function setCommercialAuditStepMessage(kind, message) {
    var alertBox = $("#commercialAuditStepMessage");
    if (!alertBox.length) {
        return;
    }

    if (!kind || !message) {
        alertBox.addClass("d-none").removeClass("alert-success alert-danger alert-warning alert-info").empty();
        return;
    }

    alertBox
        .removeClass("d-none alert-success alert-danger alert-warning alert-info")
        .addClass("alert-" + kind)
        .text(message);
}

function buildCommercialAuditStepRequestQuery(stepKey) {
    var query = new URLSearchParams();
    query.append("stepKey", stepKey || "");

    if (commercialAuditPage.selectedOmId) {
        query.append("omId", commercialAuditPage.selectedOmId);
    }

    if (commercialAuditPage.selectedPdpId) {
        query.append("pdpId", commercialAuditPage.selectedPdpId);
    }

    if (commercialAuditPage.selectedArpseId) {
        query.append("arpseId", commercialAuditPage.selectedArpseId);
    }

    return query;
}

function executeCommercialAuditInlineScripts(container) {
    if (!container) {
        return;
    }

    container.querySelectorAll("script").forEach(function (script) {
        var newScript = document.createElement("script");
        Array.from(script.attributes).forEach(function (attr) {
            newScript.setAttribute(attr.name, attr.value);
        });

        if (!newScript.src) {
            newScript.textContent = script.textContent;
        }

        script.parentNode.replaceChild(newScript, script);
    });
}

function looksLikeCommercialAuditFullHtmlDocument(html) {
    var normalizedHtml = String(html || "").trim().toLowerCase();
    if (!normalizedHtml) {
        return false;
    }

    return normalizedHtml.indexOf("<!doctype html") >= 0
        || normalizedHtml.indexOf("<html") >= 0
        || normalizedHtml.indexOf("<body") >= 0;
}

function resolveCommercialAuditStepLoadErrorMessage(error) {
    if (error && looksLikeCommercialAuditFullHtmlDocument(error.html)) {
        return "Unexpected HTML response. Please try again.";
    }

    if (error && (error.status === 401 || error.status === 403)) {
        return "Your session has expired or you no longer have access to this workflow.";
    }

    return "Unable to load the requested Commercial Audit step.";
}

function showCommercialAuditStepLoadFailure(message) {
    var resolvedMessage = message || "Unable to load the requested Commercial Audit step.";
    $("#commercialAuditStepHost").html('<div class="alert alert-danger mb-0">' + resolvedMessage + "</div>");
    setCommercialAuditStepMessage("danger", resolvedMessage);
}

function initCommercialAuditOmEntry() {
    $("#btnSaveOm").off("click").on("click", saveCommercialAuditOm);
    $("#btnCancelOmEdit").off("click").on("click", resetCommercialAuditOmForm);

    applyCommercialAuditOmFormState();
    initializeCommercialAuditRichTextEditors(["omBody", "omManagementResponse"]);

    if (commercialAuditPage.selectedOmId && !commercialAuditPage.selectedOmSnapshot) {
        loadCommercialAuditOms({ OmId: commercialAuditPage.selectedOmId });
    }
}

function initCommercialAuditOmRegister() {
    $("#tblCommercialOm").off("click", ".btn-edit-om").on("click", ".btn-edit-om", function () {
        var rowData = $(this).data("row");
        if (!rowData) {
            return;
        }

        populateCommercialAuditOmForm(rowData);
        loadCommercialAuditStep("om-entry", false);
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
    if (!$("#tblCommercialOm").length) {
        return;
    }

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
        row.append($("<td>").text(getCommercialAuditPreviewText(item.ManagementResponse, 140)));

        var editButton = $("<button>")
            .addClass("btn btn-sm btn-primary btn-edit-om")
            .attr("type", "button")
            .text("Edit")
            .data("row", item);

        row.append($("<td>").addClass("text-center").append(editButton));
        tbody.append(row);
    });

    initializeDataTable("tblCommercialOm");
}

function populateCommercialAuditOmForm(item) {
    commercialAuditPage.omMode = "edit";
    commercialAuditPage.selectedOmSnapshot = normalizeCommercialOm(item);
    commercialAuditPage.selectedOmId = commercialAuditPage.selectedOmSnapshot.OmId;
    applyCommercialAuditOmFormState();
    renderCommercialAuditOmTable(commercialAuditPage.omList);
}

function applyCommercialAuditOmFormState() {
    if (!$("#omFormTitle").length) {
        return;
    }

    if (commercialAuditPage.omMode === "edit" && commercialAuditPage.selectedOmSnapshot) {
        $("#omFormTitle").text("Update OM");
        $("#omAuditYear").val(commercialAuditPage.selectedOmSnapshot.AuditYearId || "");
        $("#omNo").val(commercialAuditPage.selectedOmSnapshot.OmNo || "");
        $("#omGist").val(commercialAuditPage.selectedOmSnapshot.GistOfOm || "");
        setCommercialAuditFieldValue("omBody", commercialAuditPage.selectedOmSnapshot.BodyOfOm || "");
        setCommercialAuditFieldValue("omManagementResponse", commercialAuditPage.selectedOmSnapshot.ManagementResponse || "");
        $("#btnSaveOm").text("Update OM");
        $("#btnCancelOmEdit").removeClass("d-none");
        return;
    }

    $("#omFormTitle").text("Create OM");
    $("#omAuditYear").val("");
    $("#omNo").val("");
    $("#omGist").val("");
    setCommercialAuditFieldValue("omBody", "");
    setCommercialAuditFieldValue("omManagementResponse", "");
    $("#btnSaveOm").text("Save OM");
    $("#btnCancelOmEdit").addClass("d-none");
}

function resetCommercialAuditOmForm() {
    commercialAuditPage.omMode = "add";
    commercialAuditPage.selectedOmId = 0;
    commercialAuditPage.selectedOmSnapshot = null;
    applyCommercialAuditOmFormState();
    renderCommercialAuditOmTable(commercialAuditPage.omList);
}

function saveCommercialAuditOm() {
    var wasEdit = commercialAuditPage.omMode === "edit";
    var model = {
        OmId: wasEdit ? commercialAuditPage.selectedOmId : 0,
        AuditYearId: parseNullableInt($("#omAuditYear").val()),
        OmNo: $("#omNo").val().trim(),
        GistOfOm: $("#omGist").val().trim(),
        BodyOfOm: getCommercialAuditRichTextValue("omBody"),
        ManagementResponse: getCommercialAuditRichTextValue("omManagementResponse"),
        IsActive: "Y"
    };

    if (!model.AuditYearId || !model.OmNo || !model.GistOfOm || !model.BodyOfOm) {
        alert("Audit Year, OM No, Gist of OM, and Body of OM are required.");
        return;
    }

    console.debug("Commercial Audit OM save payload", model);

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

            commercialAuditPage.selectedOmId = 0;
            commercialAuditPage.omMode = "add";
            commercialAuditPage.selectedOmSnapshot = null;
            resetCommercialAuditOmForm();
            loadCommercialAuditOms();
            showApiAlert(data, wasEdit ? "OM updated successfully." : "OM saved successfully.");
        },
        error: function (xhr) {
            console.error("Commercial Audit OM save failed", {
                status: xhr ? xhr.status : null,
                responseJSON: xhr ? xhr.responseJSON : null,
                responseText: xhr ? xhr.responseText : null
            });
            showApiAlertFromXhr(xhr, xhr ? xhr.status : null, getErrorReferenceIdFromXhr(xhr), "Unable to save OM.");
        }
    });
}

function initCommercialAuditPdpEntry() {
    $("#btnSavePdp").off("click").on("click", saveCommercialAuditPdp);
    $("#btnCancelPdpEdit").off("click").on("click", resetCommercialAuditPdpForm);

    applyCommercialAuditPdpState();
    initializeCommercialAuditRichTextEditors(["pdpBody", "pdpManagementResponse", "pdpDacRecommendations", "pdpUpdateManagementResponse"]);

    if (commercialAuditPage.selectedPdpId && !commercialAuditPage.selectedPdpSnapshot) {
        loadCommercialAuditPdps({ PdpId: commercialAuditPage.selectedPdpId });
    }
}

function initCommercialAuditPdpLinking() {
    $("#btnSavePdpMappings").off("click").on("click", saveCommercialAuditPdpMappings);
    $("#tblCommercialPdp").off("click", ".btn-manage-pdp").on("click", ".btn-manage-pdp", function () {
        var rowData = $(this).data("row");
        if (!rowData) {
            return;
        }

        populateCommercialAuditPdpForm(rowData);
        loadCommercialAuditPdpMappings(rowData.PdpId);
    });

    applyCommercialAuditPdpState();
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
    if (!$("#tblCommercialPdp").length) {
        return;
    }

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
        row.append($("<td>").text(getCommercialAuditPreviewText(item.UpdateManagementResponse, 120)));
        row.append($("<td>").text(item.LinkedOmNumbers || item.LinkedOmCount));

        var manageButton = $("<button>")
            .addClass("btn btn-sm btn-primary btn-manage-pdp")
            .attr("type", "button")
            .text(commercialAuditPage.selectedPdpId === item.PdpId ? "Managing" : "Manage")
            .data("row", item);

        row.append($("<td>").addClass("text-center").append(manageButton));
        tbody.append(row);
    });

    initializeDataTable("tblCommercialPdp");
}

function renderCommercialAuditPdpOmLookup(omList, selectedMappings) {
    if (!$("#tblPdpOmLookup").length) {
        return;
    }

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
    commercialAuditPage.selectedPdpSnapshot = normalizeCommercialPdp(item);
    commercialAuditPage.selectedPdpId = commercialAuditPage.selectedPdpSnapshot.PdpId;
    applyCommercialAuditPdpState();
    renderCommercialAuditPdpTable(commercialAuditPage.pdpList);
}

function applyCommercialAuditPdpState() {
    if ($("#pdpFormTitle").length) {
        if (commercialAuditPage.pdpMode === "edit" && commercialAuditPage.selectedPdpSnapshot) {
            $("#pdpFormTitle").text("Update PDP");
            $("#pdpAuditYear").val(commercialAuditPage.selectedPdpSnapshot.AuditYearId || "");
            $("#pdpNo").val(commercialAuditPage.selectedPdpSnapshot.PdpNo || "");
            $("#pdpGist").val(commercialAuditPage.selectedPdpSnapshot.GistOfPdp || "");
            setCommercialAuditFieldValue("pdpBody", commercialAuditPage.selectedPdpSnapshot.BodyOfPdp || "");
            setCommercialAuditFieldValue("pdpManagementResponse", commercialAuditPage.selectedPdpSnapshot.ManagementResponse || "");
            setCommercialAuditFieldValue("pdpDacRecommendations", commercialAuditPage.selectedPdpSnapshot.DacRecommendations || "");
            setCommercialAuditFieldValue("pdpUpdateManagementResponse", commercialAuditPage.selectedPdpSnapshot.UpdateManagementResponse || "");
            $("#btnSavePdp").text("Update PDP");
            $("#btnCancelPdpEdit").removeClass("d-none");
        } else {
            $("#pdpFormTitle").text("Create PDP");
            $("#pdpAuditYear").val("");
            $("#pdpNo").val("");
            $("#pdpGist").val("");
            setCommercialAuditFieldValue("pdpBody", "");
            setCommercialAuditFieldValue("pdpManagementResponse", "");
            setCommercialAuditFieldValue("pdpDacRecommendations", "");
            setCommercialAuditFieldValue("pdpUpdateManagementResponse", "");
            $("#btnSavePdp").text("Save PDP");
            $("#btnCancelPdpEdit").addClass("d-none");
        }
    }

    if ($("#pdpOmMappingFieldset").length) {
        var hasSelection = !!commercialAuditPage.selectedPdpId;
        $("#pdpOmMappingFieldset").prop("disabled", !hasSelection);
        $("#pdpMappingHint").text(hasSelection
            ? "Select one or more OMs and save the mapping for the selected PDP."
            : "Save or select a PDP first, then choose one or more OMs to link.");
        $("#pdpSelectedPdpLabel").text(hasSelection && commercialAuditPage.selectedPdpSnapshot
            ? commercialAuditPage.selectedPdpSnapshot.PdpNo + " selected"
            : "No PDP selected");
        renderCommercialAuditPdpOmLookup(commercialAuditPage.omList, hasSelection ? commercialAuditPage.selectedPdpMappings : []);
    }
}

function resetCommercialAuditPdpForm() {
    commercialAuditPage.pdpMode = "add";
    commercialAuditPage.selectedPdpId = 0;
    commercialAuditPage.selectedPdpSnapshot = null;
    commercialAuditPage.selectedPdpMappings = [];
    applyCommercialAuditPdpState();
    renderCommercialAuditPdpTable(commercialAuditPage.pdpList);
}

function saveCommercialAuditPdp() {
    var wasEdit = commercialAuditPage.pdpMode === "edit";
    var model = {
        PdpId: wasEdit ? commercialAuditPage.selectedPdpId : 0,
        AuditYearId: parseNullableInt($("#pdpAuditYear").val()),
        PdpNo: $("#pdpNo").val().trim(),
        GistOfPdp: $("#pdpGist").val().trim(),
        BodyOfPdp: getCommercialAuditRichTextValue("pdpBody"),
        ManagementResponse: getCommercialAuditRichTextValue("pdpManagementResponse"),
        DacRecommendations: getCommercialAuditRichTextValue("pdpDacRecommendations"),
        UpdateManagementResponse: getCommercialAuditRichTextValue("pdpUpdateManagementResponse"),
        UpdatedStatus: getCommercialAuditRichTextValue("pdpUpdateManagementResponse"),
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
            commercialAuditPage.pdpMode = "edit";
            loadCommercialAuditPdps({ PdpId: commercialAuditPage.selectedPdpId, PdpNo: model.PdpNo });
            showApiAlert(data, wasEdit ? "PDP updated successfully." : "PDP saved successfully.");
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

function initCommercialAuditArpseLinking() {
    $("#btnSaveArpsePdpMappings").off("click").on("click", saveCommercialAuditArpsePdpMappings);
    $("#tblCommercialArpsePdpRegister").off("click", ".btn-manage-arpse-pdp").on("click", ".btn-manage-arpse-pdp", function () {
        var rowData = $(this).data("row");
        if (!rowData) {
            return;
        }

        populateCommercialAuditArpseHeaderForm(rowData);
        loadCommercialAuditArpsePdpMappings(rowData.ArpseId);
    });

    applyCommercialAuditArpseLinkingState();
    loadCommercialAuditPdpLookupForArpse();
    loadCommercialAuditArpseHeaders();
}

function loadCommercialAuditPdpLookupForArpse() {
    $.ajax({
        url: g_asiBaseURL + "/ApiCalls/get_commercial_audit_pdps",
        type: "GET",
        cache: false,
        success: function (data) {
            commercialAuditPage.pdpList = Array.isArray(data) ? data.map(normalizeCommercialPdp) : [];
            renderCommercialAuditArpsePdpLookup(commercialAuditPage.pdpList, commercialAuditPage.selectedArpsePdpMappings);
        },
        error: function (xhr) {
            commercialAuditPage.pdpList = [];
            renderCommercialAuditArpsePdpLookup([], []);
            showApiAlertFromXhr(xhr, xhr ? xhr.status : null, getErrorReferenceIdFromXhr(xhr), "Unable to load PDP lookup.");
        }
    });
}

function loadCommercialAuditArpsePdpMappings(arpseId) {
    if (!arpseId) {
        commercialAuditPage.selectedArpsePdpMappings = [];
        renderCommercialAuditArpsePdpLookup(commercialAuditPage.pdpList, []);
        return;
    }

    $.ajax({
        url: g_asiBaseURL + "/ApiCalls/get_commercial_audit_arpse_pdp_mappings",
        type: "GET",
        cache: false,
        data: { arpse_id: arpseId },
        success: function (data) {
            commercialAuditPage.selectedArpsePdpMappings = Array.isArray(data) ? data.map(normalizeCommercialArpsePdpMapping) : [];
            renderCommercialAuditArpsePdpLookup(commercialAuditPage.pdpList, commercialAuditPage.selectedArpsePdpMappings);
        },
        error: function (xhr) {
            commercialAuditPage.selectedArpsePdpMappings = [];
            renderCommercialAuditArpsePdpLookup(commercialAuditPage.pdpList, []);
            showApiAlertFromXhr(xhr, xhr ? xhr.status : null, getErrorReferenceIdFromXhr(xhr), "Unable to load ARPSE PDP mappings.");
        }
    });
}

function renderCommercialAuditArpsePdpLookup(pdpList, selectedMappings) {
    if (!$("#tblArpsePdpLookup").length) {
        return;
    }

    var selectedPdpIds = (selectedMappings || []).map(function (item) {
        return item.PdpId;
    });

    var tbody = $("#tblArpsePdpLookup tbody");
    tbody.empty();

    if (!pdpList.length) {
        tbody.append('<tr><td colspan="6" class="text-center">No PDP records available for linking.</td></tr>');
        updateCommercialAuditArpsePdpSelectedSummary();
        return;
    }

    pdpList.forEach(function (item) {
        var row = $("<tr>");
        var checkbox = $("<input>")
            .attr("type", "checkbox")
            .addClass("form-check-input commercial-arpse-pdp-checkbox")
            .attr("data-pdp-id", item.PdpId)
            .prop("checked", selectedPdpIds.indexOf(item.PdpId) >= 0);

        row.append($("<td>").addClass("text-center").append(checkbox));
        row.append($("<td>").text(item.AuditYearText));
        row.append($("<td>").text(item.PdpNo));
        row.append($("<td>").text(item.GistOfPdp));
        row.append($("<td>").text(getCommercialAuditPreviewText(item.BodyOfPdp, 120)));
        row.append($("<td>").text(item.LinkedOmNumbers || item.LinkedOmCount));
        tbody.append(row);
    });

    $(".commercial-arpse-pdp-checkbox").off("change").on("change", updateCommercialAuditArpsePdpSelectedSummary);
    updateCommercialAuditArpsePdpSelectedSummary();
}

function updateCommercialAuditArpsePdpSelectedSummary() {
    var selected = $(".commercial-arpse-pdp-checkbox:checked").length;
    $("#arpsePdpSelectedSummary").text(selected > 0 ? selected + " PDPs selected" : "No PDPs selected");
}

function saveCommercialAuditArpsePdpMappings() {
    if (!commercialAuditPage.selectedArpseId) {
        alert("Save or select an ARPSE first.");
        return;
    }

    var pdpIds = $(".commercial-arpse-pdp-checkbox:checked").map(function () {
        return parseInt($(this).attr("data-pdp-id"), 10) || 0;
    }).get().filter(function (value) {
        return value > 0;
    });

    $.ajax({
        url: g_asiBaseURL + "/ApiCalls/save_commercial_audit_arpse_pdp_mapping",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({
            ArpseId: commercialAuditPage.selectedArpseId,
            PdpIds: pdpIds,
            IsActive: "Y"
        }),
        success: function (data) {
            if (!isCommercialAuditActionSuccessful(data)) {
                showApiAlert(data, "Unable to save linked PDPs.");
                return;
            }

            loadCommercialAuditArpseHeaders({ ArpseId: commercialAuditPage.selectedArpseId });
            showApiAlert(data, "Linked PDPs saved successfully.");
        },
        error: function (xhr) {
            showApiAlertFromXhr(xhr, xhr ? xhr.status : null, getErrorReferenceIdFromXhr(xhr), "Unable to save linked PDPs.");
        }
    });
}

function applyCommercialAuditArpseLinkingState() {
    if (!$("#arpsePdpMappingFieldset").length) {
        return;
    }

    var hasSelection = !!commercialAuditPage.selectedArpseId;
    $("#arpsePdpMappingFieldset").prop("disabled", !hasSelection);
    $("#arpsePdpMappingHint").text(hasSelection
        ? "Select one or more PDPs and save the mapping for the selected ARPSE."
        : "Save or select an ARPSE first, then choose one or more PDPs to link.");
    $("#arpsePdpSelectedHeaderLabel").text(hasSelection && commercialAuditPage.selectedArpseSnapshot
        ? commercialAuditPage.selectedArpseSnapshot.ParaNo + " selected"
        : "No ARPSE selected");
    renderCommercialAuditArpsePdpLookup(commercialAuditPage.pdpList, hasSelection ? commercialAuditPage.selectedArpsePdpMappings : []);
}

function initCommercialAuditArpseHeader() {
    $("#btnSaveArpse").off("click").on("click", saveCommercialAuditArpseHeader);
    $("#btnCancelArpseEdit").off("click").on("click", resetCommercialAuditArpseForm);

    applyCommercialAuditArpseHeaderState();
    initializeCommercialAuditRichTextEditors(["arpseBody", "arpseManagementResponse"]);

    if (commercialAuditPage.selectedArpseId && !commercialAuditPage.selectedArpseSnapshot) {
        loadCommercialAuditArpseHeaders({ ArpseId: commercialAuditPage.selectedArpseId });
    }
}

function initCommercialAuditArpseMonitoring() {
    $("#btnSaveArpseDac").off("click").on("click", saveCommercialAuditArpseDacEntry);
    $("#btnCancelArpseDacEdit").off("click").on("click", resetCommercialAuditArpseDacForm);
    $("#btnSaveArpsePac").off("click").on("click", saveCommercialAuditArpsePacEntry);
    $("#btnCancelArpsePacEdit").off("click").on("click", resetCommercialAuditArpsePacForm);

    $("#tblCommercialArpse").off("click", ".btn-manage-arpse").on("click", ".btn-manage-arpse", function () {
        var rowData = $(this).data("row");
        if (!rowData) {
            return;
        }

        populateCommercialAuditArpseHeaderForm(rowData);
        loadCommercialAuditArpseChildren(rowData.ArpseId);
    });

    $("#tblArpseDac").off("click", ".btn-edit-arpse-dac").on("click", ".btn-edit-arpse-dac", function () {
        var rowData = $(this).data("row");
        if (rowData) {
            populateCommercialAuditArpseDacForm(rowData);
        }
    });

    $("#tblArpsePac").off("click", ".btn-edit-arpse-pac").on("click", ".btn-edit-arpse-pac", function () {
        var rowData = $(this).data("row");
        if (rowData) {
            populateCommercialAuditArpsePacForm(rowData);
        }
    });

    applyCommercialAuditArpseHeaderState();
    applyCommercialAuditArpseDacState();
    applyCommercialAuditArpsePacState();
    initializeCommercialAuditRichTextEditors(["arpseDacRecommendation", "arpseDacUpdatedStatus", "arpsePacDirective", "arpsePacUpdatedStatus"]);
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
            renderCommercialAuditArpsePdpRegister(commercialAuditPage.arpseList);

            if (commercialAuditPage.selectedArpseId) {
                var selected = findById(commercialAuditPage.arpseList, "ArpseId", commercialAuditPage.selectedArpseId);
                if (selected) {
                    populateCommercialAuditArpseHeaderForm(selected);
                    if ($("#arpseChildrenFieldset").length) {
                        loadCommercialAuditArpseChildren(selected.ArpseId);
                    }
                    if ($("#arpsePdpMappingFieldset").length) {
                        loadCommercialAuditArpsePdpMappings(selected.ArpseId);
                    }
                }
            }
        },
        error: function (xhr) {
            commercialAuditPage.arpseList = [];
            renderCommercialAuditArpseTable([]);
            renderCommercialAuditArpsePdpRegister([]);
            showApiAlertFromXhr(xhr, xhr ? xhr.status : null, getErrorReferenceIdFromXhr(xhr), "Unable to load ARPSE headers.");
        }
    });
}

function renderCommercialAuditArpseTable(list) {
    if (!$("#tblCommercialArpse").length) {
        return;
    }

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
        row.append($("<td>").text(getCommercialAuditPreviewText(item.ManagementResponse, 140)));

        var manageButton = $("<button>")
            .addClass("btn btn-sm btn-primary btn-manage-arpse")
            .attr("type", "button")
            .text(commercialAuditPage.selectedArpseId === item.ArpseId ? "Managing" : "Manage")
            .data("row", item);

        row.append($("<td>").addClass("text-center").append(manageButton));
        tbody.append(row);
    });

    initializeDataTable("tblCommercialArpse");
}

function renderCommercialAuditArpsePdpRegister(list) {
    if (!$("#tblCommercialArpsePdpRegister").length) {
        return;
    }

    destroyDatatable("tblCommercialArpsePdpRegister");

    var tbody = $("#tblCommercialArpsePdpRegister tbody");
    tbody.empty();

    if (!list.length) {
        tbody.append('<tr><td colspan="7" class="text-center">No ARPSE headers found.</td></tr>');
        return;
    }

    list.forEach(function (item) {
        var row = $("<tr>").toggleClass("table-active", commercialAuditPage.selectedArpseId === item.ArpseId);
        row.append($("<td>").text(item.ArpseYearText));
        row.append($("<td>").text(item.ParaNo));
        row.append($("<td>").text(item.GistOfPara));
        row.append($("<td>").text(getCommercialAuditPreviewText(item.BodyOfPara, 120)));
        row.append($("<td>").text(getCommercialAuditPreviewText(item.ManagementResponse, 120)));
        row.append($("<td>").text(item.LinkedPdpNumbers || item.LinkedPdpCount));

        var manageButton = $("<button>")
            .addClass("btn btn-sm btn-primary btn-manage-arpse-pdp")
            .attr("type", "button")
            .text(commercialAuditPage.selectedArpseId === item.ArpseId ? "Managing" : "Manage")
            .data("row", item);

        row.append($("<td>").addClass("text-center").append(manageButton));
        tbody.append(row);
    });

    initializeDataTable("tblCommercialArpsePdpRegister");
}

function populateCommercialAuditArpseHeaderForm(item) {
    commercialAuditPage.arpseMode = "edit";
    commercialAuditPage.selectedArpseSnapshot = normalizeCommercialArpseHeader(item);
    commercialAuditPage.selectedArpseId = commercialAuditPage.selectedArpseSnapshot.ArpseId;
    applyCommercialAuditArpseHeaderState();
    applyCommercialAuditArpseLinkingState();
    renderCommercialAuditArpseTable(commercialAuditPage.arpseList);
    renderCommercialAuditArpsePdpRegister(commercialAuditPage.arpseList);
}

function applyCommercialAuditArpseHeaderState() {
    if ($("#arpseFormTitle").length) {
        if (commercialAuditPage.arpseMode === "edit" && commercialAuditPage.selectedArpseSnapshot) {
            $("#arpseFormTitle").text("Update ARPSE Header");
            $("#arpseYear").val(commercialAuditPage.selectedArpseSnapshot.ArpseYearId || "");
            $("#arpseParaNo").val(commercialAuditPage.selectedArpseSnapshot.ParaNo || "");
            $("#arpseGist").val(commercialAuditPage.selectedArpseSnapshot.GistOfPara || "");
            setCommercialAuditFieldValue("arpseBody", commercialAuditPage.selectedArpseSnapshot.BodyOfPara || "");
            setCommercialAuditFieldValue("arpseManagementResponse", commercialAuditPage.selectedArpseSnapshot.ManagementResponse || "");
            $("#btnSaveArpse").text("Update Header");
            $("#btnCancelArpseEdit").removeClass("d-none");
        } else {
            $("#arpseFormTitle").text("Create ARPSE Header");
            $("#arpseYear").val("");
            $("#arpseParaNo").val("");
            $("#arpseGist").val("");
            setCommercialAuditFieldValue("arpseBody", "");
            setCommercialAuditFieldValue("arpseManagementResponse", "");
            $("#btnSaveArpse").text("Save Header");
            $("#btnCancelArpseEdit").addClass("d-none");
        }
    }

    if ($("#arpseChildrenFieldset").length) {
        var hasSelection = !!commercialAuditPage.selectedArpseId;
        $("#arpseChildrenFieldset").prop("disabled", !hasSelection);
        $("#arpseSelectedHeaderLabel").text(hasSelection && commercialAuditPage.selectedArpseSnapshot
            ? commercialAuditPage.selectedArpseSnapshot.ParaNo + " selected"
            : "No ARPSE header selected");
    }
}

function resetCommercialAuditArpseForm() {
    commercialAuditPage.arpseMode = "add";
    commercialAuditPage.selectedArpseId = 0;
    commercialAuditPage.selectedArpseSnapshot = null;
    commercialAuditPage.selectedArpsePdpMappings = [];
    applyCommercialAuditArpseHeaderState();
    applyCommercialAuditArpseLinkingState();
    resetCommercialAuditArpseDacForm();
    resetCommercialAuditArpsePacForm();
    renderCommercialAuditArpseDacTable([]);
    renderCommercialAuditArpsePacTable([]);
    renderCommercialAuditArpseTable(commercialAuditPage.arpseList);
    renderCommercialAuditArpsePdpRegister(commercialAuditPage.arpseList);
}

function saveCommercialAuditArpseHeader() {
    var wasEdit = commercialAuditPage.arpseMode === "edit";
    var model = {
        ArpseId: wasEdit ? commercialAuditPage.selectedArpseId : 0,
        ArpseYearId: parseNullableInt($("#arpseYear").val()),
        ParaNo: $("#arpseParaNo").val().trim(),
        GistOfPara: $("#arpseGist").val().trim(),
        BodyOfPara: getCommercialAuditRichTextValue("arpseBody"),
        ManagementResponse: getCommercialAuditRichTextValue("arpseManagementResponse"),
        IsActive: "Y"
    };

    if (!model.ArpseYearId || !model.ParaNo || !model.GistOfPara || !model.BodyOfPara) {
        alert("ARPSE Year, Para No, Gist of Para, and Body of Para are required.");
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
            commercialAuditPage.arpseMode = "edit";
            loadCommercialAuditArpseHeaders({ ArpseId: commercialAuditPage.selectedArpseId, ParaNo: model.ParaNo });
            showApiAlert(data, wasEdit ? "ARPSE header updated successfully." : "ARPSE header saved successfully.");
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
    if (!$("#tblArpseDac").length) {
        return;
    }

    var tbody = $("#tblArpseDac tbody");
    tbody.empty();

    if (!list.length) {
        tbody.append('<tr><td colspan="4" class="text-center">No DAC entries found.</td></tr>');
        return;
    }

    list.forEach(function (item) {
        var row = $("<tr>");
        row.append($("<td>").text(formatDisplayDate(item.DacDate)));
        row.append($("<td>").addClass("commercial-audit-longtext-cell").text(stripHtmlToText(item.DacRecommendation)));
        row.append($("<td>").addClass("commercial-audit-longtext-cell").text(stripHtmlToText(item.UpdatedStatus)));

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
    if (!$("#tblArpsePac").length) {
        return;
    }

    var tbody = $("#tblArpsePac tbody");
    tbody.empty();

    if (!list.length) {
        tbody.append('<tr><td colspan="4" class="text-center">No PAC entries found.</td></tr>');
        return;
    }

    list.forEach(function (item) {
        var row = $("<tr>");
        row.append($("<td>").text(formatDisplayDate(item.PacDate)));
        row.append($("<td>").addClass("commercial-audit-longtext-cell").text(stripHtmlToText(item.PacDirective)));
        row.append($("<td>").addClass("commercial-audit-longtext-cell").text(stripHtmlToText(item.UpdatedStatus)));

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
    commercialAuditPage.editingDacId = parseInt(coalesce(item.DacEntryId, item.dacEntryId, 0), 10) || 0;

    setCommercialAuditFieldValue("arpseDacRecommendation", coalesce(item.DacRecommendation, item.dacRecommendation, "") || "");
    $("#arpseDacDate").val(formatInputDate(coalesce(item.DacDate, item.dacDate, null)));
    setCommercialAuditFieldValue("arpseDacUpdatedStatus", coalesce(item.UpdatedStatus, item.updatedStatus, "") || "");
    applyCommercialAuditArpseDacState();
}

function applyCommercialAuditArpseDacState() {
    if (!$("#btnSaveArpseDac").length) {
        return;
    }

    $("#btnSaveArpseDac").text(commercialAuditPage.dacMode === "edit" ? "Update DAC" : "Add DAC");
    $("#btnCancelArpseDacEdit").toggleClass("d-none", commercialAuditPage.dacMode !== "edit");
}

function resetCommercialAuditArpseDacForm() {
    commercialAuditPage.dacMode = "add";
    commercialAuditPage.editingDacId = 0;

    setCommercialAuditFieldValue("arpseDacRecommendation", "");
    $("#arpseDacDate").val("");
    setCommercialAuditFieldValue("arpseDacUpdatedStatus", "");
    applyCommercialAuditArpseDacState();
}

function saveCommercialAuditArpseDacEntry() {
    if (!commercialAuditPage.selectedArpseId) {
        alert("Save or select an ARPSE header first.");
        return;
    }

    var wasEdit = commercialAuditPage.dacMode === "edit";
    var model = {
        DacEntryId: wasEdit ? commercialAuditPage.editingDacId : 0,
        ArpseId: commercialAuditPage.selectedArpseId,
        DacRecommendation: getCommercialAuditRichTextValue("arpseDacRecommendation"),
        DacDate: $("#arpseDacDate").val() || null,
        UpdatedStatus: getCommercialAuditRichTextValue("arpseDacUpdatedStatus"),
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
    commercialAuditPage.editingPacId = parseInt(coalesce(item.PacEntryId, item.pacEntryId, 0), 10) || 0;

    setCommercialAuditFieldValue("arpsePacDirective", coalesce(item.PacDirective, item.pacDirective, "") || "");
    $("#arpsePacDate").val(formatInputDate(coalesce(item.PacDate, item.pacDate, null)));
    setCommercialAuditFieldValue("arpsePacUpdatedStatus", coalesce(item.UpdatedStatus, item.updatedStatus, "") || "");
    applyCommercialAuditArpsePacState();
}

function applyCommercialAuditArpsePacState() {
    if (!$("#btnSaveArpsePac").length) {
        return;
    }

    $("#btnSaveArpsePac").text(commercialAuditPage.pacMode === "edit" ? "Update PAC" : "Add PAC");
    $("#btnCancelArpsePacEdit").toggleClass("d-none", commercialAuditPage.pacMode !== "edit");
}

function resetCommercialAuditArpsePacForm() {
    commercialAuditPage.pacMode = "add";
    commercialAuditPage.editingPacId = 0;

    setCommercialAuditFieldValue("arpsePacDirective", "");
    $("#arpsePacDate").val("");
    setCommercialAuditFieldValue("arpsePacUpdatedStatus", "");
    applyCommercialAuditArpsePacState();
}

function saveCommercialAuditArpsePacEntry() {
    if (!commercialAuditPage.selectedArpseId) {
        alert("Save or select an ARPSE header first.");
        return;
    }

    var wasEdit = commercialAuditPage.pacMode === "edit";
    var model = {
        PacEntryId: wasEdit ? commercialAuditPage.editingPacId : 0,
        ArpseId: commercialAuditPage.selectedArpseId,
        PacDirective: getCommercialAuditRichTextValue("arpsePacDirective"),
        PacDate: $("#arpsePacDate").val() || null,
        UpdatedStatus: getCommercialAuditRichTextValue("arpsePacUpdatedStatus"),
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

            resetCommercialAuditArpsePacForm();
            loadCommercialAuditArpsePacEntries(commercialAuditPage.selectedArpseId);
            showApiAlert(data, wasEdit ? "PAC entry updated successfully." : "PAC entry saved successfully.");
        },
        error: function (xhr) {
            showApiAlertFromXhr(xhr, xhr ? xhr.status : null, getErrorReferenceIdFromXhr(xhr), "Unable to save PAC entry.");
        }
    });
}

function getCommercialAuditRichTextConfig() {
    return {
        bold: true,
        italic: true,
        underline: true,
        leftAlign: true,
        centerAlign: true,
        rightAlign: true,
        justify: true,
        ol: true,
        ul: true,
        heading: true,
        fonts: true,
        fontColor: true,
        fontSize: true,
        table: true,
        removeStyles: true,
        code: true,
        imageUpload: false,
        fileUpload: false,
        videoEmbed: false,
        urls: false
    };
}

function initCommercialAuditEditors(editorIds) {
    if (!$.fn.richText) {
        return;
    }

    var targets = Array.isArray(editorIds) && editorIds.length ? editorIds : commercialAuditRichTextEditorIds;
    var config = getCommercialAuditRichTextConfig();

    targets.forEach(function (editorId) {
        var $field = $("#" + editorId);
        if (!$field.length) {
            return;
        }

        if ($field.closest(".richText").length) {
            $field.trigger("change");
            return;
        }

        $field.richText($.extend({}, config, {
            id: editorId + "-editor"
        }));
        $field.trigger("change");
    });
}

function initializeCommercialAuditRichTextEditors(editorIds) {
    initCommercialAuditEditors(editorIds);
}

function destroyCommercialAuditRichTextEditors() {
    if (!$.fn.unRichText) {
        return;
    }

    commercialAuditRichTextEditorIds.forEach(function (editorId) {
        var $field = $("#" + editorId);
        if ($field.length && $field.closest(".richText").length) {
            $field.unRichText();
        }
    });
}

function getCommercialAuditRichTextValue(fieldId) {
    var field = $("#" + fieldId);
    if (!field.length) {
        return "";
    }

    var editor = field.closest(".richText").find(".richText-editor:visible").first();
    if (editor.length) {
        field.val(String(editor.html() || ""));
    }

    return normalizeCommercialAuditRichText(field.val());
}

function setCommercialAuditFieldValue(fieldId, value) {
    var field = $("#" + fieldId);
    if (!field.length) {
        return;
    }

    field.val(String(value || ""));

    if (field.closest(".richText").length) {
        field.closest(".richText").find(".richText-editor").first().html(String(value || ""));
        field.trigger("change");
    }
}

function normalizeCommercialAuditRichText(value) {
    var html = String(value || "").trim();
    if (!html) {
        return "";
    }

    if (!stripHtmlToText(html) && html.indexOf("<img") < 0 && html.indexOf("<table") < 0 && html.indexOf("<hr") < 0) {
        return "";
    }

    return html;
}

function getCommercialAuditPreviewText(value, maxLength) {
    var text = stripHtmlToText(value);
    if (!text) {
        return "";
    }

    if (maxLength && text.length > maxLength) {
        return text.substring(0, maxLength).trim() + "...";
    }

    return text;
}

function stripHtmlToText(value) {
    if (!value) {
        return "";
    }

    var wrapper = document.createElement("div");
    wrapper.innerHTML = String(value);
    return String(wrapper.textContent || wrapper.innerText || "").replace(/\s+/g, " ").trim();
}

function getCommercialAuditYearLookupEntries(lookupKey) {
    var lookups = commercialAuditPage.yearLookups || {};
    var entries = lookups[lookupKey];
    return Array.isArray(entries) ? entries : [];
}

function resolveCommercialAuditYearText(rawText, yearId, lookupKey) {
    var resolvedRawText = String(rawText || "").trim();
    if (resolvedRawText && !/^\d+$/.test(resolvedRawText)) {
        return resolvedRawText;
    }

    var numericYearId = parseInt(yearId, 10) || 0;
    if (!numericYearId) {
        return resolvedRawText;
    }

    var lookupEntries = getCommercialAuditYearLookupEntries(lookupKey);
    var matchedEntry = lookupEntries.find(function (entry) {
        return (parseInt(coalesce(entry.id, entry.Id, entry.AUDITPERIODID, 0), 10) || 0) === numericYearId;
    });

    var lookupText = matchedEntry ? String(coalesce(matchedEntry.text, matchedEntry.Text, matchedEntry.DESCRIPTION, "") || "").trim() : "";
    if (lookupText) {
        return lookupText;
    }

    return resolvedRawText || String(numericYearId);
}

function normalizeCommercialOm(item) {
    var auditYearId = parseInt(coalesce(item.AuditYearId, item.auditYearId, item.AUDIT_YEAR_ID, 0), 10) || 0;
    var rawAuditYearText = String(coalesce(item.AuditYearText, item.auditYearText, item.AUDIT_YEAR_TEXT, "") || "");
    return {
        OmId: parseInt(coalesce(item.OmId, item.omId, item.OM_ID, 0), 10) || 0,
        AuditYearId: auditYearId,
        AuditYearText: resolveCommercialAuditYearText(rawAuditYearText, auditYearId, "om"),
        OmNo: String(coalesce(item.OmNo, item.omNo, item.OM_NO, "") || ""),
        GistOfOm: String(coalesce(item.GistOfOm, item.gistOfOm, item.GIST_OF_OM, "") || ""),
        BodyOfOm: String(coalesce(item.BodyOfOm, item.bodyOfOm, item.BODY_OF_OM, "") || ""),
        ManagementResponse: String(coalesce(item.ManagementResponse, item.managementResponse, item.MANAGEMENT_RESPONSE, "") || ""),
        IsActive: String(coalesce(item.IsActive, item.isActive, item.IS_ACTIVE, "Y") || "Y")
    };
}

function normalizeCommercialPdp(item) {
    var auditYearId = parseInt(coalesce(item.AuditYearId, item.auditYearId, item.AUDIT_YEAR_ID, 0), 10) || 0;
    var rawAuditYearText = String(coalesce(item.AuditYearText, item.auditYearText, item.AUDIT_YEAR_TEXT, "") || "");
    return {
        PdpId: parseInt(coalesce(item.PdpId, item.pdpId, item.PDP_ID, 0), 10) || 0,
        AuditYearId: auditYearId,
        AuditYearText: resolveCommercialAuditYearText(rawAuditYearText, auditYearId, "pdp"),
        PdpNo: String(coalesce(item.PdpNo, item.pdpNo, item.PDP_NO, "") || ""),
        GistOfPdp: String(coalesce(item.GistOfPdp, item.gistOfPdp, item.GIST_OF_PDP, "") || ""),
        BodyOfPdp: String(coalesce(item.BodyOfPdp, item.bodyOfPdp, item.BODY_OF_PDP, "") || ""),
        ManagementResponse: String(coalesce(item.ManagementResponse, item.managementResponse, item.MANAGEMENT_RESPONSE, "") || ""),
        DacRecommendations: String(coalesce(item.DacRecommendations, item.dacRecommendations, item.DAC_RECOMMENDATIONS, "") || ""),
        UpdateManagementResponse: String(coalesce(item.UpdateManagementResponse, item.updateManagementResponse, item.UPDATE_MANAGEMENT_RESPONSE, item.UpdatedStatus, item.updatedStatus, item.UPDATED_STATUS, "") || ""),
        UpdatedStatus: String(coalesce(item.UpdateManagementResponse, item.updateManagementResponse, item.UPDATE_MANAGEMENT_RESPONSE, item.UpdatedStatus, item.updatedStatus, item.UPDATED_STATUS, "") || ""),
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

function normalizeCommercialArpsePdpMapping(item) {
    return {
        MappingId: parseInt(coalesce(item.MappingId, item.mappingId, item.MAPPING_ID, 0), 10) || 0,
        ArpseId: parseInt(coalesce(item.ArpseId, item.arpseId, item.ARPSE_ID, 0), 10) || 0,
        PdpId: parseInt(coalesce(item.PdpId, item.pdpId, item.PDP_ID, 0), 10) || 0,
        PdpNo: String(coalesce(item.PdpNo, item.pdpNo, item.PDP_NO, "") || ""),
        GistOfPdp: String(coalesce(item.GistOfPdp, item.gistOfPdp, item.GIST_OF_PDP, "") || "")
    };
}

function normalizeCommercialArpseHeader(item) {
    var arpseYearId = parseInt(coalesce(item.ArpseYearId, item.arpseYearId, item.ARPSE_YEAR_ID, 0), 10) || 0;
    var rawArpseYearText = String(coalesce(item.ArpseYearText, item.arpseYearText, item.ARPSE_YEAR_TEXT, "") || "");
    return {
        ArpseId: parseInt(coalesce(item.ArpseId, item.arpseId, item.ARPSE_ID, 0), 10) || 0,
        ArpseYearId: arpseYearId,
        ArpseYearText: resolveCommercialAuditYearText(rawArpseYearText, arpseYearId, "arpse"),
        ParaNo: String(coalesce(item.ParaNo, item.paraNo, item.PARA_NO, "") || ""),
        GistOfPara: String(coalesce(item.GistOfPara, item.gistOfPara, item.GIST_OF_PARA, "") || ""),
        BodyOfPara: String(coalesce(item.BodyOfPara, item.bodyOfPara, item.BODY_OF_PARA, "") || ""),
        ManagementResponse: String(coalesce(item.ManagementResponse, item.managementResponse, item.MANAGEMENT_RESPONSE, "") || ""),
        LinkedPdpCount: parseInt(coalesce(item.LinkedPdpCount, item.linkedPdpCount, item.LINKED_PDP_COUNT, 0), 10) || 0,
        LinkedPdpNumbers: String(coalesce(item.LinkedPdpNumbers, item.linkedPdpNumbers, item.LINKED_PDP_NUMBERS, "") || "")
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
