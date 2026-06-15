var arpseFollowUpState = {
    arpseId: 0,
    header: null,
    dacEntries: [],
    pacEntries: [],
    activeType: "dac",
    activeMode: "add",
    activeEntryId: 0
};

$(document).ready(initArpseFollowUpPage);

function initArpseFollowUpPage() {
    var page = $(".arpse-follow-up-page");
    if (!page.length) {
        return;
    }

    arpseFollowUpState.arpseId = parseInt(page.data("arpse-id"), 10) || 0;
    $("#btnAddArpseFollowUpDac").off("click").on("click", function () { openArpseFollowUpEntryModal("dac", null); });
    $("#btnAddArpseFollowUpPac").off("click").on("click", function () { openArpseFollowUpEntryModal("pac", null); });
    $("#btnSaveArpseFollowUpEntry").off("click").on("click", saveArpseFollowUpEntry);
    $("#tblArpseFollowUpDac").off("click", ".btn-view-follow-up-dac").on("click", ".btn-view-follow-up-dac", function () { openArpseFollowUpEntryModal("dac", $(this).data("row")); });
    $("#tblArpseFollowUpPac").off("click", ".btn-view-follow-up-pac").on("click", ".btn-view-follow-up-pac", function () { openArpseFollowUpEntryModal("pac", $(this).data("row")); });

    initializeArpseFollowUpEditors();
    loadArpseFollowUpHeader();
}

function loadArpseFollowUpHeader() {
    if (!arpseFollowUpState.arpseId) {
        setArpseFollowUpMessage("warning", "No ARPSE para was selected.");
        return;
    }

    $.ajax({
        url: String($(".arpse-follow-up-page").data("headers-url") || ""),
        type: "GET",
        cache: false,
        success: function (data) {
            var rows = Array.isArray(data) ? data.map(normalizeArpseFollowUpHeader) : [];
            arpseFollowUpState.header = rows.find(function (item) { return item.ArpseId === arpseFollowUpState.arpseId; }) || null;

            if (!arpseFollowUpState.header) {
                setArpseFollowUpMessage("warning", "No record found for selected ARPSE para.");
                return;
            }

            $("#arpseFollowUpHeaderText").text("Para " + arpseFollowUpState.header.ParaNo + " - " + arpseFollowUpState.header.GistOfPara);
            setArpseFollowUpMessage("info", "Loading DAC and PAC entries...");
            loadArpseFollowUpDacEntries();
            loadArpseFollowUpPacEntries();
        },
        error: function (xhr) {
            setArpseFollowUpMessage("danger", resolveAjaxMessage(xhr, "Unable to load ARPSE para."));
        }
    });
}

function loadArpseFollowUpDacEntries() {
    $.ajax({
        url: String($(".arpse-follow-up-page").data("dac-url") || ""),
        type: "GET",
        cache: false,
        data: { arpse_id: arpseFollowUpState.arpseId },
        success: function (data) {
            arpseFollowUpState.dacEntries = Array.isArray(data) ? data.map(normalizeArpseFollowUpDac) : [];
            renderArpseFollowUpDacTable();
            setArpseFollowUpMessage("success", "Follow-up entries loaded.");
        },
        error: function (xhr) {
            arpseFollowUpState.dacEntries = [];
            renderArpseFollowUpDacTable();
            setArpseFollowUpMessage("danger", resolveAjaxMessage(xhr, "Unable to load DAC entries."));
        }
    });
}

function loadArpseFollowUpPacEntries() {
    $.ajax({
        url: String($(".arpse-follow-up-page").data("pac-url") || ""),
        type: "GET",
        cache: false,
        data: { arpse_id: arpseFollowUpState.arpseId },
        success: function (data) {
            arpseFollowUpState.pacEntries = Array.isArray(data) ? data.map(normalizeArpseFollowUpPac) : [];
            renderArpseFollowUpPacTable();
        },
        error: function (xhr) {
            arpseFollowUpState.pacEntries = [];
            renderArpseFollowUpPacTable();
            setArpseFollowUpMessage("danger", resolveAjaxMessage(xhr, "Unable to load PAC entries."));
        }
    });
}

function renderArpseFollowUpDacTable() {
    var tbody = $("#tblArpseFollowUpDac tbody");
    tbody.empty();

    if (!arpseFollowUpState.dacEntries.length) {
        tbody.append('<tr><td colspan="3" class="text-center">No record found</td></tr>');
        return;
    }

    arpseFollowUpState.dacEntries.forEach(function (item) {
        var button = $("<button>").addClass("btn btn-sm btn-primary btn-view-follow-up-dac").attr("type", "button").text("View").data("row", item);
        var row = $("<tr>");
        row.append($("<td>").text(formatFollowUpDisplayDate(item.DacDate)));
        row.append($("<td>").addClass("commercial-audit-longtext-cell").text(stripFollowUpHtml(item.DacRecommendation)));
        row.append($("<td>").addClass("text-center").append(button));
        tbody.append(row);
    });
}

function renderArpseFollowUpPacTable() {
    var tbody = $("#tblArpseFollowUpPac tbody");
    tbody.empty();

    if (!arpseFollowUpState.pacEntries.length) {
        tbody.append('<tr><td colspan="3" class="text-center">No record found</td></tr>');
        return;
    }

    arpseFollowUpState.pacEntries.forEach(function (item) {
        var button = $("<button>").addClass("btn btn-sm btn-primary btn-view-follow-up-pac").attr("type", "button").text("View").data("row", item);
        var row = $("<tr>");
        row.append($("<td>").text(formatFollowUpDisplayDate(item.PacDate)));
        row.append($("<td>").addClass("commercial-audit-longtext-cell").text(stripFollowUpHtml(item.PacDirective)));
        row.append($("<td>").addClass("text-center").append(button));
        tbody.append(row);
    });
}

function openArpseFollowUpEntryModal(type, item) {
    if (!arpseFollowUpState.arpseId) {
        alert("Select an ARPSE para first.");
        return;
    }

    var isDac = type === "dac";
    arpseFollowUpState.activeType = isDac ? "dac" : "pac";
    arpseFollowUpState.activeMode = item ? "edit" : "add";
    arpseFollowUpState.activeEntryId = item ? (isDac ? item.DacEntryId : item.PacEntryId) : 0;

    $("#arpseFollowUpEntryType").val(arpseFollowUpState.activeType);
    $("#arpseFollowUpEntryId").val(arpseFollowUpState.activeEntryId || 0);
    $("#arpseFollowUpEntryModalTitle").text((item ? "Update " : "Add ") + (isDac ? "DAC Recommendation" : "PAC Directive"));
    $("#arpseFollowUpDateLabel").text(isDac ? "DAC Date" : "PAC Date");
    $("#arpseFollowUpTextLabel").text(isDac ? "Recommendation/Decision" : "Directive/Recommendation");
    $("#btnSaveArpseFollowUpEntry").text(item ? "Update" : "Save");

    $("#arpseFollowUpEntryDate").val(item ? formatFollowUpInputDate(isDac ? item.DacDate : item.PacDate) : "");
    setFollowUpEditorValue("arpseFollowUpEntryText", item ? (isDac ? item.DacRecommendation : item.PacDirective) : "");
    setFollowUpEditorValue("arpseFollowUpUpdatedStatus", item ? item.UpdatedStatus : "");

    bootstrap.Modal.getOrCreateInstance(document.getElementById("arpseFollowUpEntryModal")).show();
}

function saveArpseFollowUpEntry() {
    var isDac = arpseFollowUpState.activeType === "dac";
    var mainText = getFollowUpEditorValue("arpseFollowUpEntryText");
    if (!mainText) {
        alert(isDac ? "DAC Recommendation is required." : "PAC Directive is required.");
        return;
    }

    var payload = isDac ? {
        DacEntryId: arpseFollowUpState.activeMode === "edit" ? arpseFollowUpState.activeEntryId : 0,
        ArpseId: arpseFollowUpState.arpseId,
        DacRecommendation: mainText,
        DacDate: $("#arpseFollowUpEntryDate").val() || null,
        UpdatedStatus: getFollowUpEditorValue("arpseFollowUpUpdatedStatus"),
        IsActive: "Y"
    } : {
        PacEntryId: arpseFollowUpState.activeMode === "edit" ? arpseFollowUpState.activeEntryId : 0,
        ArpseId: arpseFollowUpState.arpseId,
        PacDirective: mainText,
        PacDate: $("#arpseFollowUpEntryDate").val() || null,
        UpdatedStatus: getFollowUpEditorValue("arpseFollowUpUpdatedStatus"),
        IsActive: "Y"
    };

    $("#btnSaveArpseFollowUpEntry").prop("disabled", true);
    $.ajax({
        url: String($(".arpse-follow-up-page").data(isDac ? "dac-save-url" : "pac-save-url") || ""),
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(payload),
        success: function (data) {
            if (!isFollowUpSaveSuccessful(data)) {
                alert(resolveActionMessage(data, "Unable to save follow-up entry."));
                return;
            }

            bootstrap.Modal.getOrCreateInstance(document.getElementById("arpseFollowUpEntryModal")).hide();
            setArpseFollowUpMessage("success", isDac ? "DAC entry saved successfully." : "PAC entry saved successfully.");
            if (isDac) {
                loadArpseFollowUpDacEntries();
            } else {
                loadArpseFollowUpPacEntries();
            }
        },
        error: function (xhr) {
            alert(resolveAjaxMessage(xhr, "Unable to save follow-up entry."));
        },
        complete: function () {
            $("#btnSaveArpseFollowUpEntry").prop("disabled", false);
        }
    });
}

function initializeArpseFollowUpEditors() {
    if (!$.fn.richText) {
        return;
    }

    $(".arpse-follow-up-editor").each(function () {
        var field = $(this);
        if (field.closest(".richText").length) {
            return;
        }

        field.richText({
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
        });
    });
}

function getFollowUpEditorValue(fieldId) {
    var field = $("#" + fieldId);
    var editor = field.closest(".richText").find(".richText-editor:visible").first();
    if (editor.length) {
        field.val(String(editor.html() || ""));
    }

    var value = String(field.val() || "").trim();
    return stripFollowUpHtml(value) ? value : "";
}

function setFollowUpEditorValue(fieldId, value) {
    var field = $("#" + fieldId);
    field.val(String(value || ""));
    var editor = field.closest(".richText").find(".richText-editor").first();
    if (editor.length) {
        editor.html(String(value || ""));
    }
}

function setArpseFollowUpMessage(kind, message) {
    $("#arpseFollowUpMessage").removeClass("alert-info alert-success alert-warning alert-danger").addClass("alert-" + kind).text(message || "");
}

function normalizeArpseFollowUpHeader(item) {
    return {
        ArpseId: parseInt(resolveFollowUpValue(item, "ArpseId", "arpseId", "ARPSE_ID"), 10) || 0,
        ParaNo: String(resolveFollowUpValue(item, "ParaNo", "paraNo", "PARA_NO") || ""),
        GistOfPara: String(resolveFollowUpValue(item, "GistOfPara", "gistOfPara", "GIST_OF_PARA") || "")
    };
}

function normalizeArpseFollowUpDac(item) {
    return {
        DacEntryId: parseInt(resolveFollowUpValue(item, "DacEntryId", "dacEntryId", "DAC_ENTRY_ID"), 10) || 0,
        DacRecommendation: String(resolveFollowUpValue(item, "DacRecommendation", "dacRecommendation", "DAC_RECOMMENDATION") || ""),
        DacDate: resolveFollowUpValue(item, "DacDate", "dacDate", "DAC_DATE"),
        UpdatedStatus: String(resolveFollowUpValue(item, "UpdatedStatus", "updatedStatus", "UPDATED_STATUS") || "")
    };
}

function normalizeArpseFollowUpPac(item) {
    return {
        PacEntryId: parseInt(resolveFollowUpValue(item, "PacEntryId", "pacEntryId", "PAC_ENTRY_ID"), 10) || 0,
        PacDirective: String(resolveFollowUpValue(item, "PacDirective", "pacDirective", "PAC_DIRECTIVE") || ""),
        PacDate: resolveFollowUpValue(item, "PacDate", "pacDate", "PAC_DATE"),
        UpdatedStatus: String(resolveFollowUpValue(item, "UpdatedStatus", "updatedStatus", "UPDATED_STATUS") || "")
    };
}

function resolveFollowUpValue(obj) {
    for (var index = 1; index < arguments.length; index++) {
        var key = arguments[index];
        if (obj && Object.prototype.hasOwnProperty.call(obj, key) && obj[key] !== null && obj[key] !== undefined) {
            return obj[key];
        }
    }
    return null;
}

function stripFollowUpHtml(value) {
    var wrapper = document.createElement("div");
    wrapper.innerHTML = String(value || "");
    return String(wrapper.textContent || wrapper.innerText || "").replace(/\s+/g, " ").trim();
}

function formatFollowUpDisplayDate(value) {
    var date = parseFollowUpDate(value);
    if (!date) {
        return "";
    }
    return date.toLocaleDateString("en-GB", { day: "2-digit", month: "short", year: "numeric" });
}

function formatFollowUpInputDate(value) {
    var date = parseFollowUpDate(value);
    if (!date) {
        return "";
    }
    return date.getFullYear() + "-" + String(date.getMonth() + 1).padStart(2, "0") + "-" + String(date.getDate()).padStart(2, "0");
}

function parseFollowUpDate(value) {
    if (!value) {
        return null;
    }
    var date = new Date(value);
    return isNaN(date.getTime()) ? null : date;
}

function isFollowUpSaveSuccessful(data) {
    var status = String(resolveFollowUpValue(data || {}, "Status", "status") || "").toUpperCase();
    return status === "SUCCESS" || status === "TRUE" || resolveFollowUpValue(data || {}, "Id", "id") > 0;
}

function resolveActionMessage(data, fallback) {
    return String(resolveFollowUpValue(data || {}, "Message", "message") || fallback || "Request failed.");
}

function resolveAjaxMessage(xhr, fallback) {
    if (xhr && xhr.responseJSON && xhr.responseJSON.message) {
        return xhr.responseJSON.message;
    }
    return fallback || "Request failed.";
}