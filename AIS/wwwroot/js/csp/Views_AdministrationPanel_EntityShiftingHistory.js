(function () {
    "use strict";

    var page = document.getElementById("entityShiftingHistoryPage");
    if (!page) {
        return;
    }

    var detailUrl = page.dataset.detailUrl;
    var message = document.getElementById("entityShiftingParasMessage");
    var tableContainer = document.getElementById("entityShiftingParasTableContainer");
    var tableBody = document.querySelector("#entityShiftingParasTable tbody");
    var selectedReference = document.getElementById("selectedShiftingReference");
    var selectedEntities = document.getElementById("selectedShiftingEntities");
    var alertBox = document.getElementById("entityShiftingHistoryAlert");

    if (window.jQuery && $.fn.DataTable &&
        !$.fn.DataTable.isDataTable("#entityShiftingHistoryTable")) {
        $("#entityShiftingHistoryTable").DataTable({
            responsive: true,
            pageLength: 10,
            order: [[8, "desc"]],
            columnDefs: [{ targets: 9, orderable: false, searchable: false }]
        });
    }

    function displayValue(value, fallback) {
        return value === null || value === undefined || value === "" ? fallback : String(value);
    }

    function addTextCell(row, value, fallback) {
        var cell = document.createElement("td");
        cell.textContent = displayValue(value, fallback || "-");
        row.appendChild(cell);
    }

    function addStatusCell(row, statusValue) {
        var cell = document.createElement("td");
        var badge = document.createElement("span");
        var status = displayValue(statusValue, "-");
        badge.textContent = status;
        badge.className = "badge " +
            (status.toLowerCase() === "open" ? "bg-success" : "bg-secondary");
        cell.appendChild(badge);
        row.appendChild(cell);
    }

    function clearDetails() {
        while (tableBody.firstChild) {
            tableBody.removeChild(tableBody.firstChild);
        }
        tableContainer.classList.add("d-none");
    }

    function showDetailMessage(text, isError) {
        message.textContent = text;
        message.className = isError ? "text-danger" : "text-muted";
        message.classList.remove("d-none");
    }

    function showAlert(text) {
        alertBox.textContent = text;
        alertBox.className = "alert alert-danger";
    }

    function renderParas(paras) {
        clearDetails();
        if (!Array.isArray(paras) || paras.length === 0) {
            showDetailMessage("No para history was found for the selected shifting transaction.", false);
            return;
        }

        paras.forEach(function (item) {
            var row = document.createElement("tr");
            addTextCell(row, item.auditPeriod, "-");
            addTextCell(row, item.paraNo, "-");
            addTextCell(row, item.gistOfParas, "No gist available");
            addStatusCell(row, item.paraStatus);
            addTextCell(row, item.annex, "-");
            tableBody.appendChild(row);
        });

        message.classList.add("d-none");
        tableContainer.classList.remove("d-none");
    }

    document.addEventListener("click", async function (event) {
        var button = event.target.closest(".view-shifted-paras");
        if (!button) {
            return;
        }

        var refId = Number.parseInt(button.dataset.refId, 10);
        if (!Number.isInteger(refId) || refId <= 0) {
            showAlert("A valid shifting reference is required.");
            return;
        }

        document.querySelectorAll("#entityShiftingHistoryTable tbody tr.table-active")
            .forEach(function (row) { row.classList.remove("table-active"); });
        button.closest("tr").classList.add("table-active");

        selectedReference.textContent = "Shifting Reference: " + refId;
        selectedEntities.textContent =
            "Old: " + displayValue(button.dataset.oldCode, "-") + " " +
            displayValue(button.dataset.oldName, "") + " \u2192 New: " +
            displayValue(button.dataset.newCode, "-") + " " +
            displayValue(button.dataset.newName, "");

        clearDetails();
        showDetailMessage("Loading shifted paras...", false);
        button.disabled = true;
        var originalText = button.textContent.trim();
        button.textContent = "Loading...";

        try {
            var response = await fetch(detailUrl + "?refId=" + encodeURIComponent(refId), {
                method: "GET",
                headers: {
                    "Accept": "application/json",
                    "X-Requested-With": "XMLHttpRequest"
                },
                credentials: "same-origin"
            });
            var result = await response.json();
            if (!response.ok || !result.success) {
                throw new Error(result.message || "Unable to retrieve shifted paras.");
            }
            renderParas(result.data);
            document.getElementById("entityShiftingParasSection")
                .scrollIntoView({ behavior: "smooth", block: "start" });
        } catch (error) {
            clearDetails();
            showDetailMessage("Unable to retrieve shifted paras.", true);
            showAlert(error.message || "Unable to retrieve shifted paras.");
        } finally {
            button.disabled = false;
            button.textContent = originalText;
        }
    });
}());
