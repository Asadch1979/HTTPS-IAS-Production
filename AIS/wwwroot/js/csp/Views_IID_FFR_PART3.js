    $(function(){
        function toggleAuditDetails(){
            var isYes = $('#AuditHighlightedYes').is(':checked');
            if(isYes){
                $('#auditHighlightDetails').removeClass('d-none');
            } else {
                $('#auditHighlightDetails').addClass('d-none');
            }
        }

        function toggleOtherImplication(){
            if($('#ImplicationOther').is(':checked')){
                $('#ImplicationOtherDetails').removeClass('d-none');
            } else {
                $('#ImplicationOtherDetails').addClass('d-none');
                $('#ImplicationOtherDetails input').val('');
            }
        }

        $('input[name="AuditHighlighted"]').on('change', toggleAuditDetails);
        $('#ImplicationOther').on('change', toggleOtherImplication);

        toggleAuditDetails();
        toggleOtherImplication();
    });
