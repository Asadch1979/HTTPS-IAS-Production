    function handleAjaxError(jqXHR, textStatus) {
        var status = jqXHR.status || 0;
        if (status === 401) {
            alert('Session expired');
            return;
        }
        if (status === 403) {
            alert('No permission');
            return;
        }

        var contentType = (jqXHR.getResponseHeader && jqXHR.getResponseHeader('content-type')) || '';
        if (textStatus === 'parsererror' || contentType.indexOf('text/html') !== -1) {
            alert('Unexpected response from server');
            return;
        }

        alert('Request failed');
    }

    $(document).on('click', '.menu_page_selectAll', function () {
        var isChecked = $(this).is(':checked');
        $('.menu_page_tick').prop('checked', isChecked);
    });

    $(document).on('change', '#menuSelectionBox', function () {
        showPagesBlock();
    });

    $(document).on('click', '.js-menu-assignment-save', function (e) {
        e.preventDefault();
        publishSaveChanges();
    });
    function showPagesBlock() {

        if ($('#menuSelectionBox option:selected').val() == 0) {
            $('.pagesBlock').addClass('d-none');

        }
        else {
            $('.menu_page_tick').prop('checked', false);
            $('.pagesBlock').addClass('d-none');
            $.ajax({
                url: g_asiBaseURL + "/AdministrationPanel/menu_pages",
                type: "POST",
                data: {
                    'MENU_ID': $('#menuSelectionBox option:selected').val()
                },
                cache: false,
                success: function (data) {
                    if (!Array.isArray(data)) {
                        alert('Unexpected response from server');
                        return;
                    }

                    $('.menu_page_tick').prop('checked', false);
                    $.each(data, function (index, page) {
                        $('#pagemenuitem_' + page.id).prop('checked', true);
                    });
                    $('.pagesBlock').removeClass('d-none');
                },
                error: function (jqXHR, textStatus) {
                    handleAjaxError(jqXHR, textStatus);
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

        if (pageIds.length === 0) {
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
            error: function (jqXHR, textStatus) {
                handleAjaxError(jqXHR, textStatus);
            },
            dataType: "json",
        });
    }
