    var g_trId = 0;

    $(document).ready(function () {
        $("#searchTableRecord").on("keyup", function () {
            var value = $(this).val().toLowerCase();
            $("#listOfProcTransactions tbody tr").filter(function () {
                $(this).toggle($(this).text().toLowerCase().indexOf(value) > -1);
            });
        });

        function updateSelectAllState() {
            var total = $('.actionCriteria').length;
            var selected = $('.actionCriteria:checked').length;
            $('#selectAllCriteria').prop('checked', total > 0 && total === selected);
        }

        $('#selectAllCriteria').on('change', function () {
            var isChecked = $(this).is(':checked');
            $('.actionCriteria').prop('checked', isChecked);
            updateSelectAllState();
        });

        $(document).on('change', '.actionCriteria', function () {
            updateSelectAllState();
        });

        updateSelectAllState();

        $('#approveAuditCriteriaButton').on('click', function () {
            approveAuditCriterias();
        });

        $('#referBackAuditCriteriaButton').on('click', function () {
            referredBackAuditCriterias();
        });
    });

    function reloadLocation() {
        if (window.planningDashboard && typeof window.planningDashboard.reloadCurrentStep === 'function') {
            window.planningDashboard.reloadCurrentStep();
            return;
        }

        location.reload();
    }

    function showApprovalAlert(message, onClose) {
        if (typeof onClose === 'function' && typeof onAlertCallback === 'function') {
            onAlertCallback(onClose);
        }

        var popup = $('#alertMessagesPopup');
        var content = $('#content_alertMessagesPopup');
        if (popup.length && content.length && $.fn.modal) {
            content.empty();
            content.text(message);
            popup.modal('show');
            return;
        }

        alert(message);
        if (typeof onClose === 'function') {
            onClose();
        }
    }

    function referredBackAuditCriterias() {
        var datalist = [];

        $.each($('.actionCriteria'), function (i, v) {
            if ($(v).is(':checked')) {
                var id = $(v).attr('id');
                var comment = $(v).closest('tr').find('.criteriaComment').val();
                datalist.push({ ID: id, COMMENT: comment });
            }
        });

        if (datalist.length === 0) {
            alert("Please select at least one criteria.");
            return;
        }

        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/ReferredBackAuditCriteria",
            type: "POST",
            dataType: "json",
            cache: false,
            data: { DATALIST: datalist },
            success: function () {
                setApprovalMessage('', false);
                showApprovalAlert("Audit Criteria selected Cases Referred Back", reloadLocation);
            }
        });
    }

    function approveAuditCriterias() {
        var datalist = [];
        setApprovalMessage('', false);

        $.each($('.actionCriteria'), function (i, v) {
            if ($(v).is(':checked')) {
                var rawId = $(v).attr('id');
                var id = parseInt(rawId, 10);
                if (Number.isNaN(id)) {
                    id = 0;
                }

                var comment = $(v).closest('tr').find('.criteriaComment').val() || '';
                datalist.push({ ID: id, COMMENT: comment });
            }
        });

        if (datalist.length === 0) {
            setApprovalMessage("Please select at least one criteria.", false);
            return;
        }

        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/AuthorizeAuditCriteria",
            type: "POST",
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            cache: false,
            data: JSON.stringify(datalist),
            success: function (response) {
                if (!response || response.status !== true) {
                    var message = extractApiMessage(response, "Unable to approve audit criteria.");
                    setApprovalMessage(message, false);
                    return;
                }

                var successMessage = "Audit Criteria selected Cases Successfully Approved";
                setApprovalMessage('', false);
                showApprovalAlert(successMessage, reloadLocation);
            },
            error: function (xhr) {
                var message = extractApiMessageFromXhr(xhr, "Unable to approve audit criteria.");
                if (xhr && xhr.status) {
                    message += " (HTTP " + xhr.status + ")";
                }
                setApprovalMessage(message, false);
            }
        });
    }

    function setApprovalMessage(message, isSuccess) {
        var messageElement = $('#approvalMessage');
        messageElement.removeClass('text-success text-danger');
        if (!message) {
            messageElement.text('');
            return;
        }
        messageElement.addClass(isSuccess ? 'text-success' : 'text-danger');
        messageElement.text(message);
    }
