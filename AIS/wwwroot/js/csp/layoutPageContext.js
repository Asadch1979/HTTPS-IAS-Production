(function () {
    function getMeta(name) {
        var node = document.querySelector('meta[name="' + name + '"]');
        return node ? (node.getAttribute('content') || '') : '';
    }

    var pageId = parseInt(getMeta('ias-page-id'), 10);
    window.PAGE_ID = Number.isNaN(pageId) ? 0 : pageId;
    window.IAS_HIDE_SIDEBAR = (getMeta('ias-hide-sidebar') || '').toLowerCase() === 'true';
})();
