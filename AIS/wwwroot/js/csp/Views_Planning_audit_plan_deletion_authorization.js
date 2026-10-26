(function () {
    if (window.__iasPlanDeletionApprovalBound) return;
    window.__iasPlanDeletionApprovalBound = true;
    document.addEventListener('submit', function (event) {
        var form = event.target;
        if (!form || !form.classList.contains('js-plan-delete-approval')) return;
        if (!window.confirm('Authorize permanent deletion of this audit plan? This cannot be undone.')) {
            event.preventDefault();
            return;
        }
        var button = form.querySelector('button[type="submit"]');
        if (button) button.disabled = true;
    });
})();
