    function getEntityObservation() {
        $('#manageObsPanel tbody').empty();
        if ($('#entitySelectField option:selected').val() != 0) {
            $.ajax({
                url: g_asiBaseURL + "/ApiCalls/get_assigned_observation_old_paras",
                type: "POST",
                data: {
                    'ENTITY_ID': $('#entitySelectField option:selected').val()
                },
                cache: false,
                success: function (data) {
                    g_obsList = data;
                    $.each(data, function (i, v) {                      
                        $('#manageObsPanel tbody').append('<tr id="assignedObRow_' + v.obS_ID + '"><td>' + v.audiT_YEAR + '</td><td>' + v.memO_NUMBER + '</td><td class="text-center">' + sdate + '</td><td class="text-center">' + edate + '</td><td class="text-center">' + opsdate + '</td><td class="text-center">' + opedate + '</td><td class="text-center">' + v.status + '</td><td class="text-center"><a onclick="event.preventDefault();showMemo(' + v.obS_ID + ',' + v.resP_ID + ');" class="text-hover font-weight-bold text-success">Reply</a></td></tr>');

                    });

                    setTimeout(function () {
                        if (g_obsId != 0) {
                            var rowpos = $('#assignedObRow_' + g_obsId).position();
                            $('html').scrollTop(rowpos.top);
                        }
                    }, 200)



                },
                dataType: "json",
            });

        }
    }
