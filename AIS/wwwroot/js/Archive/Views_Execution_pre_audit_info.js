    var g_engId = 0;  


    $(document).ready(function () {

        var url_string = window.location;
        var url = new URL(url_string);
        var eng_id = url.searchParams.get("engId");
        if (typeof eng_id != 'undefined')
            g_engId = eng_id;
    });
