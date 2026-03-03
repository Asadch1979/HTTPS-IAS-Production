    $(document).ready(function () {

        getb_depositinfo();

        $('#sidebar_policy').hide();
        $("#searchTableRecord").on("keyup", function () {
            var value = $(this).val().toLowerCase();
            $("#b_d_T tbody tr").filter(function () {
                $(this).toggle($(this).text().toLowerCase().indexOf(value) > -1)
            });
        });


    });
