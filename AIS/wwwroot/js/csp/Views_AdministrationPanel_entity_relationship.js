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

    function getrelation(parentEntityId = 0, userEntityId = 0) {


        $('#controlingsearch').empty();
        $('#childposting').empty();
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/getparentrel",
            type: "POST",
            data: {
                'ENTITY_REALTION_ID': $('#RelationshipField option:selected').val()
            },


            cache: false,
            success: function (data) {


                $('#controlingsearch').append('<option id="0" value="0">--Select Controlling/Reporting Office--</option>');
                $.each(data, function (index, contof) {

                    var selected = '';
                    if (contof.entitY_ID == parentEntityId)
                        selected = 'selected="selected"';

                    $('#controlingsearch').append('<option ' + selected + ' value="' + contof.entitY_ID + '" id="' + contof.entitY_REALTION_ID + '">' + contof.description + '</option>')
                });
                if (userEntityId != 0)
                    getplacepost(userEntityId)

                // console.log(data);

            },
            dataType: "json",
        });



    }

    function getplacepost(userEntityId = 0) {
        $('#childposting').empty();

        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/getpostplace",
            type: "POST",
            data: {
                'E_R_ID': $('#controlingsearch option:selected').val()
            },


            cache: false,
            success: function (data) {
                $('#childposting').append('<option id="0" value="0" selected="selected">--Select Place of Posting--</option>');
                $.each(data, function (index, gpp) {

                    var selected = '';
                    if (gpp.entitY_ID == userEntityId)
                        selected = 'selected="selected"';
                    $('#childposting').append('<option ' + selected + ' value="' + gpp.entitY_ID + '" id="' + gpp.entitY_ID + '">' + gpp.c_NAME + '</option>')
                });
            },
            dataType: "json",
        });
        //  getrelation();

    }
