    $(document).ready(function () {
     
    });

  
    function getEntityObservation() {
        destroyDatatable('observation_panel');
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_functional_responsibility_wise_paras_for_dashboard",
            type: "POST",
            data: {
                'FUNCTIONAL_ENTITY_ID': $('#functionalGroupSelectBox option:selected').val()             
            },
            cache: false,
            success: function (data) {
                g_obsList = data;
               
                var sr = 1;
              
                $.each(data, function (i, v) {
                        $('#observation_panel tbody').append(
                        '<tr id="' + v.id + '">' +
                            '<td align="center">' + sr + '</td>' +
                            '<td align="left">' + v.reP_OFFICE + '</td>' +
                            '<td align="left">' + v.entitY_NAME + '</td>' +
                            '<td align="center">' + v.annexure + '</td>' +
                            '<td align="left">' + v.checK_LIST + '</td>' +
                            '<td align="center">' + v.parA_NO + '</td>' +
                            '<td align="center">' + v.risk + '</td>' +
                            '<td align="left">' + v.gist + '</td>' +
                            '<td align="center">' +
                                '<a href="#" data-click="event.preventDefault();getParaText( \'' + v.obS_ID + '\', \'' + v.parA_CATEGORY + '\');">View Para Text</a>' +
                            '</td>' +
                        '</tr>'
                    );

                    sr++;
                });

                initializeDataTable('observation_panel');
            },
            dataType: "json",
        });
    }

    function getParaText(id, pc) {
        $('#paraTextViewerModel').modal("show");
        $('#paraTextDivField').empty();

        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_functional_observation_text",
            type: "POST",
            data: {
                'PARA_ID': id,
                'PARA_CATEGORY': pc
            },
            cache: false,
            success: function (data) {
                $('#paraTextDivField').html(data);
            }
        });
    }
