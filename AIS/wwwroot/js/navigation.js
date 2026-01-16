(() => {
    const hideNestedMenus = () => {
        document.querySelectorAll('.dropdown-submenu .dropdown-menu').forEach(menu => {
            menu.style.display = 'none';
        });
    };

    document.addEventListener('DOMContentLoaded', () => {
        document.querySelectorAll('.dropdown-submenu a.nested-menu').forEach(anchor => {
            anchor.addEventListener('click', event => {
                const nestedMenu = anchor.nextElementSibling;
                if (nestedMenu) {
                    const isVisible = nestedMenu.style.display === 'block';
                    hideNestedMenus();
                    nestedMenu.style.display = isVisible ? 'none' : 'block';
                }
                event.stopPropagation();
                event.preventDefault();
            });
        });

        document.querySelectorAll('.dropdown-toggle').forEach(toggle => {
            toggle.addEventListener('click', () => hideNestedMenus());
        });
    });
})();
