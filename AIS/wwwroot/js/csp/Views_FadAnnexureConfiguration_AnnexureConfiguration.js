(function () {
    "use strict";

    var alertBox = document.getElementById("annexure-config-alert");
    var token = document.querySelector("#annexure-config-token input[name='__RequestVerificationToken']");

    function showMessage(success, message) {
        alertBox.textContent = message;
        alertBox.className = "alert " + (success ? "alert-success" : "alert-danger");
    }

    document.querySelectorAll(".update-annexure").forEach(function (button) {
        button.addEventListener("click", async function () {
            var row = button.closest("tr");
            var select = row.querySelector(".annexure-status");
            var originalValue = select.dataset.originalValue;
            button.disabled = true;
            select.disabled = true;

            try {
                var response = await fetch("UpdateAnnexureStatus", {
                    method: "POST",
                    headers: {
                        "Content-Type": "application/json",
                        "RequestVerificationToken": token ? token.value : ""
                    },
                    body: JSON.stringify({
                        annexureId: Number(row.dataset.annexureId),
                        shiftApplicable: select.value
                    })
                });
                var result = await response.json();
                if (!response.ok || !result.success) {
                    select.value = originalValue;
                    throw new Error(result.message || "Unable to update the Annexure status.");
                }

                select.dataset.originalValue = select.value;
                if (result.updatedOn)
                    row.querySelector(".last-updated").textContent = result.updatedOn;
                showMessage(true, result.message);
            } catch (error) {
                select.value = originalValue;
                showMessage(false, error.message || "A network error prevented the update.");
            } finally {
                button.disabled = false;
                select.disabled = false;
            }
        });
    });
}());
