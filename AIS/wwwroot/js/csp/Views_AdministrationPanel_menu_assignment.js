    $(document).ready(function () {
        $('.menu_page_selectAll').on('click', function () {

            if ($('.menu_page_selectAll').is(':checked'))
                $('.menu_page_tick').attr('checked', true);
            else
                $('.menu_page_tick').attr('checked', false);

        });


    });
    function showPagesBlock() {

        if ($('#menuSelectionBox option:selected').val() == 0) {
            $('.pagesBlock').addClass('d-none');

        }
        else {
            $('.menu_page_tick').attr('checked', false);
            $('.pagesBlock').addClass('d-none');
            $.ajax({
                url: g_asiBaseURL + "/AdministrationPanel/menu_pages",
                type: "POST",
                data: {
                    'MENU_ID': $('#menuSelectionBox option:selected').val()
                },
                cache: false,
                success: function (data) {
                    $('.menu_page_tick').attr('checked', false);
                    $.each(data, function (index, page) {
                        $('#pagemenuitem_' + page.id).attr('checked', true);
                    });
                    $('.pagesBlock').removeClass('d-none');
                },
                dataType: "json",
            });

        }
    }

    function publishSaveChanges() {
        var pageIds = [];
        $.each($('.menu_page_tick:checked'), function (i, v) {
            pageIds.push($(v).attr('id').split('_')[1]);
        });

        if (pageIds.length > 0) {
            alert('Please check atleast one page to proceed');
            return false;
        }
        
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/menu_pages_updation",
            type: "POST",
            data: {
                'MENU_ID': $('#menuSelectionBox option:selected').val(),
                'PAGE_IDS': pageIds
            },
            cache: false,
            success: function (data) {
                alert("Menu Pages assignment Succesfully completed");
                
            },
            dataType: "json",
        });
    }
