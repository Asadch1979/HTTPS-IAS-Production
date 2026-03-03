    var g_endId = 0;
    $(document).ready(function () {
        var url_string = window.location;
        var url = new URL(url_string);
        g_endId = url.searchParams.get("engId");
    });
    function proceedToSubChecklist(id) {
        window.location.href = g_asiBaseURL + '/Execution/subchecklist?engId=' + g_endId + '&id=' + id;
    }

$(document).on('click', "[data-action='sub-checklist']", function (event) {
    event.preventDefault();
    proceedToSubChecklist($(this).data('task-id'));
});
