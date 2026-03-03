    var g_engId = 0;

    $(document).ready(function () {
        var gl_Id = 0;
        var url_string = window.location;
        var url = new URL(url_string);
        var eng_id = url.searchParams.get("engId");
        if (typeof eng_id != 'undefined')
            g_engId = eng_id;
        $("#searchTableRecord").on("keyup", function () {
            var value = $(this).val().toLowerCase();
            $("#listOfGlHead tbody tr").filter(function () {
                $(this).toggle($(this).text().toLowerCase().indexOf(value) > -1)
            });
        });
    });
