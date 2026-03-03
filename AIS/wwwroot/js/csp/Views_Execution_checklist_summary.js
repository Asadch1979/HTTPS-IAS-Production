   
    $(document).ready(function () {

        $('#detailsPanelMain').addClass('d-none');
        $('#detailsPanelMainGrid').addClass('d-none');
        
    });
  
    function showDetailsCount() {
        
        $('#detailsPanelMain').removeClass('d-none');
        $('#detailsPanelGrid').addClass('d-none');
     
    }

    function detailsGridPanel(){
        $('#detailsPanelGrid').removeClass('d-none');
    }
