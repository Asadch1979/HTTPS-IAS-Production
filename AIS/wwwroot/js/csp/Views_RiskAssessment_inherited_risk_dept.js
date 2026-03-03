    function getCOSORisk() {
        if ($('#auditCriteriaPeriodField').val() != 0) {

            $.ajax({
                url: g_asiBaseURL + "/ApiCalls/GetCOSORiskForDepartment",
                type: "POST",
                data: {
                    'PERIOD_ID': $('#auditCriteriaPeriodField').val()
                },
                cache: false,
                success: function (data) {
                    $('#COSORiskTable tbody').empty();
                    if (data.length > 0) {
                        
                        var rowSpan = 1;
                        var unique = 1;
                        entity = data[0].depT_NAME;
                        var sf_total = 0;
                        var ms_total = 0;
                        var wa_total = 0;
                        var nob_total = 0;
                        var was_total = 0;
                        $('#COSORiskTable tbody').append('<tr id="rowspan_' + unique + '"><td class="rowspancol" rowspan="' + rowSpan + '">' + entity + '</td></tr>');
                        $.each(data, function (i, v) {
                            sf_total += v.suB_FACTORS;
                            ms_total += v.maX_SCORE;
                            wa_total += v.weighT_ASSIGNED;
                            nob_total += v.nO_OF_OBSERVATIONS;
                            was_total += v.weighteD_AVERAGE_SCORE;
                            if (entity != v.depT_NAME) {
                                $('#COSORiskTable tbody').append('<tr><td><b>Total</b></td><td colspan=5><center>'+v.finaL_SCORE+'</center></td><td>'+v.finaL_AUDIT_RATING+'</td></tr>');
                                rowSpan++;
                                $('#COSORiskTable #rowspan_' + unique + ' .rowspancol').attr('rowspan', rowSpan);

                                ////////////NEW ROW ENTRY//////////////
                                rowSpan = 1;
                                unique++;
                                entity = v.depT_NAME;
                                sf_total = 0;
                                ms_total = 0;
                                wa_total = 0;
                                nob_total = 0;
                                was_total = 0;
                                sf_total += v.suB_FACTORS;
                                ms_total += v.maX_SCORE;
                                wa_total += v.weighT_ASSIGNED;
                                nob_total += v.nO_OF_OBSERVATIONS;
                                was_total += v.weighteD_AVERAGE_SCORE;
                                $('#COSORiskTable tbody').append('<tr id="rowspan_' + unique + '"><td class="rowspancol" rowspan="' + rowSpan + '">' + v.depT_NAME + '</td></tr>');
                                $('#COSORiskTable tbody').append('<tr><td>' + v.ratinG_FACTORS + '</td><td>' + v.suB_FACTORS + '</td><td>' + v.maX_SCORE + '</td><td>' + v.weighT_ASSIGNED + '</td><td>' + v.nO_OF_OBSERVATIONS + '</td><td>' + v.weighteD_AVERAGE_SCORE + '</td><td>' + v.audiT_RATING + '</td></tr>');
                                rowSpan++;
                            }
                            else {
                                $('#COSORiskTable tbody').append('<tr><td>' + v.ratinG_FACTORS + '</td><td>' + v.suB_FACTORS + '</td><td>' + v.maX_SCORE + '</td><td>' + v.weighT_ASSIGNED + '</td><td>' + v.nO_OF_OBSERVATIONS + '</td><td>' + v.weighteD_AVERAGE_SCORE + '</td><td>' + v.audiT_RATING + '</td></tr>');
                                rowSpan++;
                            }
                        });
                        $('#COSORiskTable tbody').append('<tr><td><b>Total</b></td><td colspan=5><center>' + data[data.length - 1].finaL_SCORE + '</center></td><td>'+data[data.length - 1].finaL_AUDIT_RATING +'</td></tr>');
                        rowSpan++;
                        $('#COSORiskTable #rowspan_' + unique + ' .rowspancol').attr('rowspan', rowSpan);
                    }
                },
                dataType: "json",
            });
        }
        else {
            $('#COSORiskTable tbody').empty();
        }
    }
