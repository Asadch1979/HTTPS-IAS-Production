        $(document).ready(function () {
            $('#riskRegisterTable').DataTable({
                paging: false,
                searching: false,
                info: false,
                ordering: false,
                dom: '<"top"lfB>rt<"bottom"ip><"clear">',
                buttons: [
                    getPdfExportButtonConfig(),
                    getExcelExportButtonConfig('Export to Excel'),
                    getCsvExportButtonConfig('Export to CSV'),
                    { extend: 'copyHtml5', text: 'Copy to Clipboard' }
                ]
            });
        });
