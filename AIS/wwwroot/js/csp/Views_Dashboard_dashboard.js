    $(document).ready(function () {

        getAuditScoreCard();
    });


    function getAuditScoreCard() {

        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_audit_performance_chart_for_dashboard",
            type: "POST",
            data: {
            },
            cache: false,
            success: function (data) {

                $('#headingSpanField').html(data[0].heading);
                $.each(data, function (i, v) {
                    $($('div.widget-title')[i]).html(v.department);
                    if (v.totaL_ENTITIES != "" && v.nO_OF_ENTITIES != "")
                        $($('span.ent_count_filed')[i]).html(v.nO_OF_ENTITIES + "/" + v.totaL_ENTITIES);

                    if (v.remarks != "")
                        $($('small.remarks_field_span')[i]).html(v.remarks);

                    $($('div.progress-value')[i]).html(v.percentage);
                     $('div.progress .progress-bar').css('animation', 'none');

                });

            },
            dataType: "json",
        });
    }
