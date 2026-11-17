// NashaMukti JS — Sidebar toggle, confirmations, alerts
(function () {
    'use strict';

    // ── Sidebar toggle (mobile) ─────────────────────────────────
    const sidebar  = document.getElementById('appSidebar');
    const overlay  = document.getElementById('sidebarOverlay');
    const toggleBtn = document.getElementById('sidebarToggle');

    function openSidebar() {
        sidebar?.classList.add('open');
        overlay?.classList.add('open');
        document.body.style.overflow = 'hidden';
    }

    function closeSidebar() {
        sidebar?.classList.remove('open');
        overlay?.classList.remove('open');
        document.body.style.overflow = '';
    }

    toggleBtn?.addEventListener('click', () => {
        sidebar?.classList.contains('open') ? closeSidebar() : openSidebar();
    });

    overlay?.addEventListener('click', closeSidebar);

    // ── Active nav link ─────────────────────────────────────────
    const currentPath = window.location.pathname.toLowerCase();
    document.querySelectorAll('.sidebar-nav a').forEach(link => {
        const href = link.getAttribute('href')?.toLowerCase() ?? '';
        if (href && href !== '/' && currentPath.startsWith(href)) {
            link.classList.add('active');
        } else if (href === '/' && currentPath === '/') {
            link.classList.add('active');
        }
    });

    // ── Auto-dismiss alerts ─────────────────────────────────────
    document.querySelectorAll('.alert-auto-dismiss').forEach(alert => {
        setTimeout(() => {
            alert.style.transition = 'opacity .4s ease';
            alert.style.opacity = '0';
            setTimeout(() => alert.remove(), 400);
        }, 4000);
    });

    // ── Confirm delete dialogs ─────────────────────────────────
    document.querySelectorAll('[data-confirm]').forEach(el => {
        el.addEventListener('click', function (e) {
            const msg = this.dataset.confirm || 'Are you sure you want to perform this action?';
            if (!confirm(msg)) {
                e.preventDefault();
                e.stopPropagation();
            }
        });
    });

    // ── Form submit loading state ──────────────────────────────
    document.querySelectorAll('form[data-loading]').forEach(form => {
        form.addEventListener('submit', function () {
            const btn = this.querySelector('button[type="submit"]');
            if (btn) {
                btn.disabled = true;
                const original = btn.innerHTML;
                btn.innerHTML = `<span class="spinner-nm me-2"></span> Processing...`;
                // restore if form fails (validation)
                setTimeout(() => {
                    btn.disabled = false;
                    btn.innerHTML = original;
                }, 8000);
            }
        });
    });

    // ── Tooltip init (Bootstrap) ───────────────────────────────
    if (typeof bootstrap !== 'undefined' && bootstrap.Tooltip) {
        document.querySelectorAll('[data-bs-toggle="tooltip"]').forEach(el => {
            new bootstrap.Tooltip(el, { trigger: 'hover' });
        });
    }

    // ── Search input debounce helper ───────────────────────────
    window.NM = window.NM || {};
    window.NM.debounce = function (fn, delay) {
        let timer;
        return function (...args) {
            clearTimeout(timer);
            timer = setTimeout(() => fn.apply(this, args), delay);
        };
    };

    // ── Live search: auto-submit filter form on typing ─────────
    const searchInput = document.getElementById('liveSearch');
    if (searchInput) {
        searchInput.addEventListener('input', window.NM.debounce(function () {
            const form = this.closest('form');
            if (form) form.submit();
        }, 500));
    }

})();
