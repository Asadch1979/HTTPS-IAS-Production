(() => {
    const pageIdValue = document.body && document.body.dataset ? document.body.dataset.pageId : null;
    const parsed = parseInt(pageIdValue || "0", 10);
    // expose the page_id global expected by existing scripts
    window.page_id = Number.isNaN(parsed) ? 0 : parsed;
})();
