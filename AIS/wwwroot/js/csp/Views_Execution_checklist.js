    var g_endId = 0;
    $(document).ready(function () {
        var url_string = window.location;
        var url = new URL(url_string);
        g_endId = url.searchParams.get("engId");

        $(document).on('click', '.js-proceed-subchecklist', function (event) {
            event.preventDefault();
            proceedToSubChecklist($(this).data('task-id'));
        });
    });
    function proceedToSubChecklist(id) {
        window.location.href = g_asiBaseURL + '/Execution/subchecklist?engId=' + g_endId + '&id=' + id;
    }
