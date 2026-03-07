(function(){
    function byId(id){ return document.getElementById(id); }

    var selector = byId('engagementSelector');
    if(!selector){ return; }

    selector.addEventListener('change', function(){
        var baseUrl = selector.getAttribute('data-dashboard-base-url') || '/FieldAudit/Dashboard';
        var stepHost = byId('fieldAuditStepHost');
        var stepCode = stepHost ? (stepHost.getAttribute('data-step-code') || '') : '';
        var engId = selector.value || '';

        if(!engId){
            window.location.href = baseUrl;
            return;
        }

        var query = '?engId=' + encodeURIComponent(engId);
        if(stepCode){
            query += '&stepCode=' + encodeURIComponent(stepCode);
        }

        window.location.href = baseUrl + query;
    });
})();
