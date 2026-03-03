    $(document).ready(function () {
        $('.menu_page_selectAll').on('click', function () {

            if ($('.menu_page_selectAll').is(':checked'))
                $('.menu_page_tick').attr('checked', true);
            else
                $('.menu_page_tick').attr('checked', false);

        });
       
    });
