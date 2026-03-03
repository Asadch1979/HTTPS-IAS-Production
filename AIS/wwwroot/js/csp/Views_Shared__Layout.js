        (function(){
            var body = document.body || document.querySelector('body');
            if (body){
                window.PAGE_ID = Number(body.getAttribute('data-page-id') || 0) || 0;
                window.IAS_HIDE_SIDEBAR = String(body.getAttribute('data-hide-sidebar') || "false").toLowerCase() === "true";
            }
            var sidebar = document.getElementById('iasSidebar');
            var isDesktop = window.matchMedia('(min-width: 992px)').matches;
            var sidebarDisabled = !!window.IAS_HIDE_SIDEBAR || !sidebar;
            var openTimer = null;
            var closeTimer = null;

            function clearSidebarTimers(){
                if (openTimer){
                    clearTimeout(openTimer);
                    openTimer = null;
                }
                if (closeTimer){
                    clearTimeout(closeTimer);
                    closeTimer = null;
                }
            }

            function setSidebarCollapsed(collapsed, persist){
                if (collapsed){
                    document.body.classList.add('ias-sidebar-collapsed');
                } else {
                    document.body.classList.remove('ias-sidebar-collapsed');
                }

                if (persist){
                    try{
                        localStorage.setItem('ias.sidebar.collapsed', collapsed ? '1' : '0');
                    }catch(e){}
                }
            }

            if (!sidebarDisabled && isDesktop){
                setSidebarCollapsed(true, true);
            }

            // Collapse toggle (desktop)
            var collapseBtn = document.getElementById('iasSidebarCollapseBtn');
            if (collapseBtn){
                collapseBtn.addEventListener('click', function(){
                    var shouldCollapse = !document.body.classList.contains('ias-sidebar-collapsed');
                    clearSidebarTimers();
                    setSidebarCollapsed(shouldCollapse, true);
                });
            }

            if (!sidebarDisabled && isDesktop){
                document.addEventListener('mousemove', function(event){
                    if (event.clientX <= 18 && document.body.classList.contains('ias-sidebar-collapsed')){
                        if (!openTimer){
                            openTimer = setTimeout(function(){
            var body = document.body || document.querySelector('body');
            if (body){
                window.PAGE_ID = Number(body.getAttribute('data-page-id') || 0) || 0;
                window.IAS_HIDE_SIDEBAR = String(body.getAttribute('data-hide-sidebar') || "false").toLowerCase() === "true";
            }
                                setSidebarCollapsed(false, false);
                                openTimer = null;
                            }, 120);
                        }
                    } else if (openTimer){
                        clearTimeout(openTimer);
                        openTimer = null;
                    }

                    if (!document.body.classList.contains('ias-sidebar-collapsed')){
                        var rect = sidebar.getBoundingClientRect();
                        var pointerAway = event.clientX > (rect.right + 60);
                        if (pointerAway){
                            if (!closeTimer){
                                closeTimer = setTimeout(function(){
            var body = document.body || document.querySelector('body');
            if (body){
                window.PAGE_ID = Number(body.getAttribute('data-page-id') || 0) || 0;
                window.IAS_HIDE_SIDEBAR = String(body.getAttribute('data-hide-sidebar') || "false").toLowerCase() === "true";
            }
                                    setSidebarCollapsed(true, false);
                                    closeTimer = null;
                                }, 260);
                            }
                        } else if (closeTimer){
                            clearTimeout(closeTimer);
                            closeTimer = null;
                        }
                    }
                });

                sidebar.addEventListener('mouseenter', function(){
                    if (closeTimer){
                        clearTimeout(closeTimer);
                        closeTimer = null;
                    }
                });

                sidebar.addEventListener('mouseleave', function(){
                    if (!document.body.classList.contains('ias-sidebar-collapsed') && !closeTimer){
                        closeTimer = setTimeout(function(){
            var body = document.body || document.querySelector('body');
            if (body){
                window.PAGE_ID = Number(body.getAttribute('data-page-id') || 0) || 0;
                window.IAS_HIDE_SIDEBAR = String(body.getAttribute('data-hide-sidebar') || "false").toLowerCase() === "true";
            }
                            setSidebarCollapsed(true, false);
                            closeTimer = null;
                        }, 260);
                    }
                });

                sidebar.addEventListener('click', function(event){
                    var link = event.target && event.target.closest ? event.target.closest('a[href]') : null;
                    if (link){
                        clearSidebarTimers();
                        setSidebarCollapsed(true, false);
                    }
                }, true);
            }

            // Mobile open/close
            var toggleBtn = document.getElementById('iasSidebarToggleBtn');
            var backdrop = document.getElementById('iasSidebarBackdrop');
            function closeMobile(){
                document.body.classList.remove('ias-sidebar-open');
            }
            function openMobile(){
                document.body.classList.add('ias-sidebar-open');
            }

            if (toggleBtn){
                toggleBtn.addEventListener('click', function(){
                    if (document.body.classList.contains('ias-sidebar-open')) closeMobile();
                    else openMobile();
                });
            }
            if (backdrop){
                backdrop.addEventListener('click', closeMobile);
            }

            // Sidebar accordion + search + Home quick links hook
            var sideMenu = document.getElementById('iasSideMenu');
            var search = document.getElementById('iasMenuSearch');

            var openMenus = new Set();
            try{
                var saved = (localStorage.getItem('ias.sidebar.openMenus') || '').split(',').map(function(x){ return x.trim(); }).filter(Boolean);
                saved.forEach(function(id){ openMenus.add(id); });
            }catch(e){}

            function getMenuBtn(menuId){
                return sideMenu ? sideMenu.querySelector('[data-menu-btn][data-menu-id="' + menuId + '"]') : null;
            }
            function getMenuPanel(menuId){
                return document.getElementById('iasMenuPanel_' + menuId);
            }
            function setMenuOpen(menuId, open){
                var btn = getMenuBtn(menuId);
                var panel = getMenuPanel(menuId);
                if (!btn || !panel) return;

                var icon = btn.querySelector('.ias-acc-icon');
                if (open){
                    panel.removeAttribute('hidden');
                    btn.setAttribute('aria-expanded', 'true');
                    if (icon) icon.textContent = '−';
                    openMenus.add(String(menuId));
                }else{
                    panel.setAttribute('hidden', 'hidden');
                    btn.setAttribute('aria-expanded', 'false');
                    if (icon) icon.textContent = '+';
                    openMenus.delete(String(menuId));
                }

                try{
                    localStorage.setItem('ias.sidebar.openMenus', Array.from(openMenus).join(','));
                }catch(e){}
            }
            function toggleMenu(menuId){
                var panel = getMenuPanel(menuId);
                if (!panel) return;
                var isOpen = !panel.hasAttribute('hidden');
                setMenuOpen(menuId, !isOpen);
            }

            // Restore open menus on load (only if sidebar isn't collapsed)
            if (sideMenu && !document.body.classList.contains('ias-sidebar-collapsed')){
                openMenus.forEach(function(id){ setMenuOpen(id, true); });
            }

            // Attach handlers to menu buttons
            if (sideMenu){
                sideMenu.querySelectorAll('[data-menu-btn]').forEach(function(btn){
                    btn.addEventListener('click', function(){
                        var menuId = btn.getAttribute('data-menu-id');
                        var menuName = btn.getAttribute('data-menu-name') || (btn.textContent || '').trim();
                        // If sidebar is collapsed, expand it first for better UX
                        if (document.body.classList.contains('ias-sidebar-collapsed')){
                            document.body.classList.remove('ias-sidebar-collapsed');
                            try{ localStorage.setItem('ias.sidebar.collapsed','0'); }catch(e){}
                            if (typeof updateCollapseIcon === 'function') updateCollapseIcon();
                        }
                        toggleMenu(menuId);

                        // Home: refresh quick links tiles for this menu
                        if (window.IASHomeSetQuickLinks){
                            try{ window.IASHomeSetQuickLinks(parseInt(menuId,10) || 0, menuName); }catch(e){}
                        }
                    });
                });
            }

            // Search filters pages and auto-opens matching menus
            function applySearch(){
                if (!search || !sideMenu) return;
                var q = (search.value || '').trim().toLowerCase();

                var groups = sideMenu.querySelectorAll('[data-group]');
                groups.forEach(function(g){
                    var btn = g.querySelector('[data-menu-btn]');
                    var panel = g.querySelector('.ias-acc-panel');
                    var links = g.querySelectorAll('[data-link]');
                    var subtitles = g.querySelectorAll('[data-subtitle]');
                    if (!btn || !panel) return;

                    // reset visibility
                    links.forEach(function(a){ a.style.display = ''; });
                    subtitles.forEach(function(s){ s.style.display = ''; });

                    if (!q){
                        g.style.display = '';
                        // restore saved open state
                        var id = btn.getAttribute('data-menu-id');
                        setMenuOpen(id, openMenus.has(String(id)));
                        return;
                    }

                    var menuName = (btn.getAttribute('data-menu-name') || btn.textContent || '').toLowerCase();
                    var menuMatch = menuName.includes(q);

                    var anyVisible = false;
                    links.forEach(function(a){
                        var t = (a.textContent || '').toLowerCase();
                        var show = menuMatch || t.includes(q);
                        a.style.display = show ? '' : 'none';
                        if (show) anyVisible = true;
                    });

                    // Hide subtitle blocks if all subsequent links are hidden (simple pass)
                    subtitles.forEach(function(sub){
                        // find links until next subtitle
                        var node = sub.nextElementSibling;
                        var hasVisible = false;
                        while (node && !(node.hasAttribute && node.hasAttribute('data-subtitle'))){
                            if (node.matches && node.matches('[data-link]') && node.style.display !== 'none'){
                                hasVisible = true;
                                break;
                            }
                            node = node.nextElementSibling;
                        }
                        sub.style.display = hasVisible ? '' : 'none';
                    });

                    g.style.display = anyVisible ? '' : 'none';
                    if (anyVisible){
                        var id = btn.getAttribute('data-menu-id');
                        setMenuOpen(id, true);
                    }
                });
            }

            if (search && sideMenu){
                search.addEventListener('input', applySearch);
            }
// Active link highlight
            try{
                var path = window.location.pathname.toLowerCase();
                document.querySelectorAll('.ias-page-link').forEach(function(a){
                    var href = (a.getAttribute('href') || '').toLowerCase();
                    // href may be ~/path; browser resolves to /path in rendered HTML, so compare by endsWith
                    if (href && path && (path === href || path.endsWith(href.replace('~','')))){
                        a.classList.add('active');
                    }
                });
            }catch(e){}
        })();
