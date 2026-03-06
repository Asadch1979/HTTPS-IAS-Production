(function () {
    function showAlert(msg, type) {
        var safe = (typeof sanitizeAlertMessageText === 'function')
            ? sanitizeAlertMessageText(msg)
            : ((msg || 'Unexpected error').toString().trim());
        var text = safe || 'Unexpected error';
        var $alert = $('<div/>', { 'class': 'alert alert-' + (type || 'danger') + ' text-prewrap' });
        $alert.text(text);
        $('#monitoringAlertHost').empty().append($alert);
    }

    $(function () {
        if (!$('#iidMonitoringTable').length) {
            showAlert('Monitoring table is unavailable.');
            return;
        }

        destroyDatatable('iidMonitoringTable');
        initializeDataTable('iidMonitoringTable');
    });
}(window));
