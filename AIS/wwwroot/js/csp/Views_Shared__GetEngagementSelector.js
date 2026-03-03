    function validateEngagementSelection(selectElement) {
        const value = selectElement.value;
        if (!value || value === '0') {
            return false;
        }

        const hasOption = Array.from(selectElement.options)
            .some(option => option.value === value && option.value !== '0');

        if (!hasOption) {
            alert('Select a valid engagement to continue.');
            return false;
        }

        selectElement.form.submit();
        return false;
    }


(function(){
    document.addEventListener('change', function(event){
        var selectElement = event.target && event.target.closest ? event.target.closest('.js-engagement-select') : null;
        if (!selectElement) return;
        validateEngagementSelection(selectElement);
    });

    document.addEventListener('click', function(event){
        var button = event.target && event.target.closest ? event.target.closest('.js-confirm-clear-engagement') : null;
        if (!button) return;
        var message = button.getAttribute('data-confirm-message') || 'Are you sure?';
        if (!window.confirm(message)) {
            event.preventDefault();
        }
    });
})();
