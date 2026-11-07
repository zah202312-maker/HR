// HR account pages: theme toggle, password visibility, and following the app's language once.
(function () {
    var root = document.documentElement;

    var toggle = document.getElementById('HrThemeToggle');
    function setTitle() {
        if (!toggle) return;
        var dark = root.getAttribute('data-theme') === 'dark';
        toggle.title = dark ? toggle.dataset.titleLight : toggle.dataset.titleDark;
    }
    if (toggle) {
        setTitle();
        toggle.addEventListener('click', function () {
            var next = root.getAttribute('data-theme') === 'dark' ? 'light' : 'dark';
            root.setAttribute('data-theme', next);
            try { localStorage.setItem('hr-theme', next); } catch (e) { }
            setTitle();
        });
    }

    document.querySelectorAll('.hr-eye').forEach(function (btn) {
        btn.addEventListener('click', function () {
            var input = document.getElementById(btn.dataset.target);
            if (!input) return;
            var show = input.type === 'password';
            input.type = show ? 'text' : 'password';
            btn.setAttribute('aria-pressed', show ? 'true' : 'false');
        });
    });

    // The app sends its language as ui_locales inside the return URL. Switch to it once per tab,
    // so the page opens in the app's language; a manual switch afterwards is kept.
    var q = new URLSearchParams(location.search);
    var ru = q.get('ReturnUrl') || q.get('returnUrl') || '';
    var m = /[?&]ui_locales=([a-z]{2})/.exec(ru);
    var lang = root.getAttribute('lang');
    var link = document.getElementById('HrLangSwitch');
    var synced = false;
    try { synced = sessionStorage.getItem('hr-lang-synced') === '1'; } catch (e) { }
    if (link) {
        link.addEventListener('click', function () {
            try { sessionStorage.setItem('hr-lang-synced', '1'); } catch (e) { }
        });
    }
    if (m && !synced && m[1] !== lang && link && link.dataset.lang === m[1]) {
        try { sessionStorage.setItem('hr-lang-synced', '1'); } catch (e) { }
        location.replace(link.href);
    }

    // Avoid double submits.
    document.querySelectorAll('form.hr-form').forEach(function (f) {
        f.addEventListener('submit', function (ev) {
            if (window.jQuery && jQuery(f).valid && !jQuery(f).valid()) return;
            var b = f.querySelector('button[type=submit].hr-btn');
            if (b) setTimeout(function () { b.disabled = true; }, 0);
        });
    });
})();
