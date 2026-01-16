(function ($) {
    $(function () {
        var $statusInput = $('#sessionStatusValue');
        if (!$statusInput.length) {
            return;
        }

        var statusMessage = ($statusInput.val() || '').trim();
        if (!statusMessage) {
            return;
        }

        document.dispatchEvent(new CustomEvent('login:sessionStatus', { detail: statusMessage }));
    });
})(jQuery);
