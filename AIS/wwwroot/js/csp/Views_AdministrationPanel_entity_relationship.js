var g_entityRelationshipPostingRequest = null;

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
    showEntityRelationshipPostingMessage('Select a Controlling/Reporting Office to load Place of Posting records.');
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

        },
        dataType: "json",
    });



}

function getplacepost(userEntityId = 0) {
    $('#childposting').empty();
    var controllingOfficeId = $('#controlingsearch option:selected').val();

    if (g_entityRelationshipPostingRequest) {
        g_entityRelationshipPostingRequest.abort();
        g_entityRelationshipPostingRequest = null;
    }

    if (!controllingOfficeId || controllingOfficeId == 0) {
        $('#childposting').append('<option id="0" value="0" selected="selected">--Select Place of Posting--</option>');
        showEntityRelationshipPostingMessage('Select a Controlling/Reporting Office to load Place of Posting records.');
        return;
    }

    showEntityRelationshipPostingMessage('Loading Place of Posting records...');
    g_entityRelationshipPostingRequest = $.ajax({
        url: g_asiBaseURL + "/ApiCalls/getpostplace",
        type: "POST",
        data: {
            'E_R_ID': controllingOfficeId
        },


        cache: false,
        success: function (data) {
            $('#childposting').append('<option id="0" value="0" selected="selected">--Select Place of Posting--</option>');
            renderEntityRelationshipPostingGrid(data);
            $.each(data, function (index, gpp) {

                var selected = '';
                if (gpp.entitY_ID == userEntityId)
                    selected = 'selected="selected"';
                $('#childposting').append('<option ' + selected + ' value="' + gpp.entitY_ID + '" id="' + gpp.entitY_ID + '">' + gpp.c_NAME + '</option>')
            });
        },
        error: function () {
            showEntityRelationshipPostingMessage('Unable to load Place of Posting records.');
        },
        complete: function () {
            g_entityRelationshipPostingRequest = null;
        },
        dataType: "json",
    });

}

function renderEntityRelationshipPostingGrid(data) {
    var tbody = $('#entityRelationshipPostingGrid tbody');
    tbody.empty();

    if (!Array.isArray(data) || data.length === 0) {
        showEntityRelationshipPostingMessage('No Place of Posting records found.');
        return;
    }

    $.each(data, function (index, posting) {
        var entityId = parseInt(posting.entitY_ID, 10);
        var row = $('<tr>');
        row.append($('<td>').text(index + 1));
        row.append($('<td>').text(isNaN(entityId) ? '' : entityId));
        row.append($('<td>').text(posting.c_NAME || ''));
        row.append($('<td>').text(posting.c_TYPE_ID || ''));
        row.append($('<td>').text(posting.audiT_BY || ''));
        row.append($('<td>').text(posting.gM_OFFICE || ''));
        row.append($('<td>').text(posting.reporting || ''));

        var actionButton = $('<button>')
            .attr('type', 'button')
            .addClass('btn btn-sm btn-danger')
            .text('Update Entity Mapping');

        if (!isNaN(entityId) && entityId > 0) {
            actionButton.attr('data-onclick', 'updateAISEntityMapping(' + entityId + ');');
        } else {
            actionButton.prop('disabled', true);
        }

        var actionCell = $('<td>').append(actionButton);
        if (!isNaN(entityId) && entityId > 0) {
            actionCell.append(
                $('<div>').addClass('d-flex flex-wrap gap-1 mt-1').append(
                    $('<button>')
                        .attr('type', 'button')
                        .addClass('btn btn-sm btn-outline-primary')
                        .text('Use as From')
                        .attr('data-onclick', "entityDashboardSelectEntity(" + entityId + ",'" + String(posting.c_NAME || '').replace(/\\/g, '\\\\').replace(/'/g, "\\'") + "','from');"),
                    $('<button>')
                        .attr('type', 'button')
                        .addClass('btn btn-sm btn-outline-success')
                        .text('Use as To')
                        .attr('data-onclick', "entityDashboardSelectEntity(" + entityId + ",'" + String(posting.c_NAME || '').replace(/\\/g, '\\\\').replace(/'/g, "\\'") + "','to');")
                )
            );
        }

        row.append(actionCell);
        tbody.append(row);
    });
}

function showEntityRelationshipPostingMessage(message) {
    var tbody = $('#entityRelationshipPostingGrid tbody');
    if (!tbody.length) {
        return;
    }

    tbody.empty().append(
        $('<tr>').addClass('entity-relationship-empty-row').append(
            $('<td>').attr('colspan', 8).addClass('text-center text-muted').text(message)
        )
    );
}
