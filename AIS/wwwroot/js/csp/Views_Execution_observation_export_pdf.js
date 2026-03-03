    var g_tablePage = 0;
    var g_scrollPos = 0;

    function preserveTablePosition() {
        g_scrollPos = $('html').scrollTop();
        if ($.fn.DataTable.isDataTable('#manageObsPanel')) {
            g_tablePage = $('#manageObsPanel').DataTable().page();
        }
    }

    $(document).ready(function () {
        $('#entitySelectField').select2();
    });

    function getEntityObservation() {
        destroyDatatable('manageObsPanel');
        $('#manageObsPanel tbody').empty();
        var selectedEngId = $('#entitySelectField').val();
        $('#engIdHidden').val(selectedEngId);
        if ($('#entitySelectField option:selected').val() != 0) {
            $.ajax({
                url: g_asiBaseURL + "/ApiCalls/get_observation_branches",
                type: "POST",
                data: {
                    'ENG_ID': $('#entitySelectField option:selected').val()
                },
                cache: false,
                success: function (data) {
                    $.each(data, function (i, v) {
                        $('#manageObsPanel tbody').append(
                            '<tr id="' + v.obS_ID + '">'
                            + '<td class="text-center">' + v.memO_NO + '</td>'
                            + '<td class="text-center">' + v.annexurE_CODE + '</td>'
                            + '<td>' + v.heading + '</td>'
                            + '<td>' + v.nO_OF_INSTANCES + '</td>'
                            + '<td>' + v.obS_RISK + '</td>'
                            + '<td>' + v.obS_STATUS + '</td>'
                            + '</tr>'
                        );
                    });

                    initializeDataTable('manageObsPanel');
                    var tbl = $('#manageObsPanel').DataTable();
                    tbl.page(g_tablePage).draw('page');
                    setTimeout(function () {
                        $('html').scrollTop(g_scrollPos);
                    }, 200)
                },
                dataType: "json",
            });
        }
    }
