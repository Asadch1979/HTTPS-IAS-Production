(function () {
    function showAlert(msg, type) {
        $('#monitoringAlertHost').html('<div class="alert alert-' + (type || 'danger') + '">' + $('<div/>').text(msg || 'Unexpected error').html() + '</div>');
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
