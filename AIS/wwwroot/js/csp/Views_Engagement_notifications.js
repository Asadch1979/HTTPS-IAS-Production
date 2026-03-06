    var g_checklist = [{ 'srNo': 1, 'observation': 'Lacking in account opening form' }, { 'srNo': 2, 'observation': 'Stop Payment not processed' }, { 'srNo': 3, 'observation': 'Loan application document not attached' }, { 'srNo': 4, 'observation': 'CNIC is expired' }, { 'srNo': 5, 'observation': 'Cash Counter is not openned timely' }]

    $('#document').ready(function () {
       
        $('#checklistPanel tbody').empty();
        $.each(g_checklist, function (i, v) {
            $('#checklistPanel tbody').append('<tr><td>' + v.srNo + '</td><td>' + v.observation + '</td><td><select class="checklistaction form-select form-control" data-change="showObservationArea($(this).val(),\'obs_' + v.srNo + '\');" aria-label="Default select example"><option value="0" id="0" selected>No</option><option value="1" id="1">Yes</option></select></td></tr>');
            $('#checklistPanel tbody').append('<tr><td id="obs_' + v.srNo + '" class="d-none" colspan="3" style="height:300px; overflow-y:auto;"><div class="page-wrapper box-content"><textarea class="template_box content" name="example"></textarea></div></td></tr>');
        });
        $('.template_box').richText({
            imageUpload: false,
            fileUpload: false,
            videoEmbed: false,
            urls: false
        });
       
    });
    function showObservationArea(value,id) {
        if(value==0)
            $('#' + id).addClass('d-none');
        else
            $('#' + id).removeClass('d-none');

    }
