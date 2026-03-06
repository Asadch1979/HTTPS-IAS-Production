$(document).ready(function () {
    $('.menu_page_selectAll').on('click', function () {
        $('.menu_page_tick').prop('checked', $('.menu_page_selectAll').is(':checked'));
    });

    $('#groupSelectionBox').on('change', showMenuBlock);
    $('#menuSelectionBox').on('change', showMenuPagesBlock);
    $('#saveGroupAssignmentBtn').on('click', AddGroupItemAssignment);
});

function showMenuBlock() {
    if ($('#groupSelectionBox').val() === '') {
        $('.menuBlock').addClass('d-none');
        $('.pagesBlock').addClass('d-none');
    }
    else {
        $('.menu_page_tick').prop('checked', false);
        $('.menuBlock').removeClass('d-none');
        showMenuPagesBlock();
    }
}

function showMenuPagesBlock() {
    if ($('#menuSelectionBox').val() == 0) {
        $('.pagesBlock').addClass('d-none');
    }
    else {
        $('.menu_page_tick').prop('checked', false);
        $('.pagesBlock').removeClass('d-none');
        $('#menuPagesArea').empty();
        $.ajax({
            url: g_asiBaseURL + '/AdministrationPanel/menu_pages',
            type: 'POST',
            data: {
                'MENU_ID': $('#menuSelectionBox').val()
            },
            cache: false,
            success: function (data) {
                $.each(data, function (index, page) {
                    $('#menuPagesArea').append('<div class="col-md-4"><div class= "row col-md-12" ><div class="col-md-2"><input  id="pageId_' + page.id + '" class="menu_page_tick" type="checkbox" /></div><div class="col-md-10"><label class="font-weight-normal">' + page.page_Name + '</label></div></div></div>');
                });
                getAssignedMenuPages();
            },
            dataType: 'json'
        });
    }
}

function getAssignedMenuPages() {
    $.ajax({
        url: g_asiBaseURL + '/AdministrationPanel/assigned_menu_pages',
        type: 'POST',
        data: {
            'MENU_ID': $('#menuSelectionBox option:selected').val(),
            'GROUP_ID': $('#groupSelectionBox option:selected').val()
        },
        cache: false,
        success: function (data) {
            $.each(data, function (index, page) {
                $('#pageId_' + page.id).prop('checked', true);
            });
        },
        dataType: 'json'
    });
}

function AddGroupItemAssignment() {
    var menuItemIds = [];
    var unlinkMenuItemIds = [];
    $.each($('.menu_page_tick'), function (index, mItem) {
        if ($(mItem).is(':checked')) {
            menuItemIds.push($(mItem).attr('id').replace('pageId_', ''));
        }
        else {
            unlinkMenuItemIds.push($(mItem).attr('id').replace('pageId_', ''));
        }
    });

    $.ajax({
        url: g_asiBaseURL + '/AdministrationPanel/add_group_item_assignment',
        type: 'POST',
        data: {
            'GROUP_ID': $('#groupSelectionBox option:selected').val(),
            'MENU_ID': $('#menuSelectionBox option:selected').val(),
            'MENU_ITEM_IDs': menuItemIds,
            'UNLINK_MENU_ITEM_IDs': unlinkMenuItemIds
        },
        cache: false,
        success: function () {
            alert('Mapping successfuly completed ');
        },
        dataType: 'json'
    });
}
