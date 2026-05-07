(function() {
    // Selector for the main content container
    const MAIN_SELECTOR = '.dmfs-main-inner';
    // Intercept sidebar links and any link marked with data-pjax
    const NAV_LINK_SELECTOR = '.dmfs-sidebar-nav a.dmfs-link, a[data-pjax]';

    function parseHTML(html) {
        const parser = new DOMParser();
        return parser.parseFromString(html, 'text/html');
    }

    function initializeMilkProductionForm(container) {
        container = container || document;
        const form = container.querySelector('form[data-pjax-submit]');
        if (!form) return;
        const cowSelect = form.querySelector('#milkCowSelect');
        const hiddenName = form.querySelector('#SelectedCowName');
        const hiddenTag = form.querySelector('#SelectedCowEarTag');
        const totalEl = form.querySelector('#milkTotal');
        const amInput = form.querySelector('#Draft_AmMilk');
        const noonInput = form.querySelector('#Draft_NoonMilk');
        const pmInput = form.querySelector('#Draft_PmMilk');

        function parseValue(v) {
            const n = parseFloat(v);
            return Number.isFinite(n) ? n : 0;
        }

        function syncSelectedCowMetadata() {
            if (!cowSelect || !hiddenName || !hiddenTag) return;
            const opt = cowSelect.options[cowSelect.selectedIndex];
            hiddenName.value = opt ? (opt.getAttribute('data-cow-name') || '') : '';
            hiddenTag.value = opt ? (opt.getAttribute('data-cow-tag') || '') : '';
        }

        function calculateTotal() {
            if (!totalEl) return;
            const am = parseValue(amInput?.value);
            const noon = parseValue(noonInput?.value);
            const pm = parseValue(pmInput?.value);
            totalEl.value = (am + noon + pm).toFixed(1);
        }

        cowSelect?.removeEventListener('change', syncSelectedCowMetadata);
        cowSelect?.addEventListener('change', syncSelectedCowMetadata);
        [amInput, noonInput, pmInput].forEach(function (input) {
            if (!input) return;
            input.removeEventListener('input', calculateTotal);
            input.addEventListener('input', calculateTotal);
        });

        calculateTotal();
        syncSelectedCowMetadata();
    }

    function dismissAlerts(container) {
        container = container || document;
        const alerts = container.querySelectorAll('.alert');
        alerts.forEach(function (a) {
            // remove after a short delay
            setTimeout(function () {
                try { a.remove(); } catch (e) { try { a.style.display = 'none'; } catch (e) {} }
            }, 1000);
        });
    }

    function showTempAlert(message, type) {
        try {
            var main = document.querySelector(MAIN_SELECTOR) || document.body;
            var div = document.createElement('div');
            div.className = 'alert ' + (type === 'success' ? 'alert-success' : 'alert-danger');
            div.textContent = message || '';
            // insert at top
            main.insertAdjacentElement('afterbegin', div);
            // let dismissAlerts remove it
            setTimeout(function() { try { div.remove(); } catch (e) {} }, 1000);
        } catch (e) { console.error('showTempAlert failed', e); }
    }

    // Table filter utilities (support Manage cows page)
    function checkStatusFilterRow(row) {
        const chkActive = document.getElementById('filterActive');
        const chkInactive = document.getElementById('filterInactive');
        const showActive = chkActive ? chkActive.checked : true;
        const showInactive = chkInactive ? chkInactive.checked : true;
        const status = row.getAttribute('data-status');
        if (!status) return true;
        if (status === 'active' && showActive) return true;
        if (status === 'inactive' && showInactive) return true;
        return false;
    }

    function applyTableFilters(container) {
        container = container || document;
        const filterInput = container.querySelector('#cowFilter');
        const q = (filterInput && filterInput.value) ? filterInput.value.toLowerCase() : '';
        const rows = container.querySelectorAll('#cowsTable tbody tr');
        rows.forEach(function (row) {
            const t = row.innerText.toLowerCase();
            const matchesText = q === '' || t.includes(q);
            const matchesStatus = checkStatusFilterRow(row);
            row.style.display = (matchesText && matchesStatus) ? '' : 'none';
        });
        // toggle visibility of inactive-only columns depending on active/inactive filter
        const showInactiveColumn = (document.getElementById('filterInactive')?.checked ?? false);
        const th = document.getElementById('thReason');
        if (th) th.style.display = showInactiveColumn ? '' : 'none';
        document.querySelectorAll('.td-reason').forEach(function(td) { td.style.display = showInactiveColumn ? '' : 'none'; });
        const thDate = document.getElementById('thInactiveDate');
        if (thDate) thDate.style.display = showInactiveColumn ? '' : 'none';
        document.querySelectorAll('.td-inactive-date').forEach(function(td) { td.style.display = showInactiveColumn ? '' : 'none'; });

        // visible rows should always be serially numbered from 1.
        let serial = 1;
        rows.forEach(function (row) {
            if (row.style.display === 'none') return;
            const idxCell = row.querySelector('.td-row-index');
            if (idxCell) idxCell.textContent = String(serial++);
        });
    }

    function executeScripts(container) {
        // Find scripts in the container and re-insert them so they execute
        const scripts = Array.from(container.querySelectorAll('script'));
        scripts.forEach(s => {
            const script = document.createElement('script');
            if (s.src) {
                script.src = s.src;
                script.async = false;
            } else {
                script.textContent = s.textContent;
            }
            document.body.appendChild(script);
            // remove original to avoid duplication
            s.parentNode && s.parentNode.removeChild(s);
        });
    }

    function updateInactiveReasonControls(container) {
        container = container || document;
        // Support both main-page form IDs and modal form IDs.
        var sel = container.querySelector('#cowStatusSelect, #modalCowStatusSelect');
        if (!sel) return;
        var row = container.querySelector('#inactiveReasonRow, #modalInactiveReasonRow');
        var input = container.querySelector('input[name="Draft.InactiveReason"]');
        var val = String(sel.value).toLowerCase() === 'true';
        if (row) row.style.display = val ? 'none' : 'block';
        if (input) {
            if (!val) {
                // inactive => require reason
                input.setAttribute('required', 'required');
                // add jQuery validation rule if available
                if (window.jQuery && jQuery.validator && jQuery(input).rules) {
                    try {
                        jQuery(input).rules('add', { required: true, messages: { required: 'Inactive reason is required.' } });
                    } catch (e) { }
                }
            } else {
                input.removeAttribute('required');
                if (window.jQuery && jQuery.validator && jQuery(input).rules) {
                    try { jQuery(input).rules('remove', 'required'); } catch (e) { }
                }
            }
        }

        // InactiveDate required logic
        var inactiveDateRow = container.querySelector('#inactiveDateRow, #modalInactiveDateRow');
        var inactiveDateInput = container.querySelector('input[name="Draft.InactiveDate"]');
        if (inactiveDateRow) inactiveDateRow.style.display = val ? 'none' : 'block';
        if (inactiveDateInput) {
            if (!val) {
                inactiveDateInput.setAttribute('required', 'required');
                if (window.jQuery && jQuery.validator && jQuery(inactiveDateInput).rules) {
                    try {
                        jQuery(inactiveDateInput).rules('add', { required: true, messages: { required: 'Inactive date is required.' } });
                    } catch (e) { }
                }
            } else {
                inactiveDateInput.removeAttribute('required');
                if (window.jQuery && jQuery.validator && jQuery(inactiveDateInput).rules) {
                    try { jQuery(inactiveDateInput).rules('remove', 'required'); } catch (e) { }
                }
            }
        }
    }

    async function loadUrl(url, push = true) {
        try {
            const resp = await fetch(url, {
                headers: { 'X-Requested-With': 'XMLHttpRequest' },
                credentials: 'same-origin'
            });
            const text = await resp.text();
            const doc = parseHTML(text);
            const newMain = doc.querySelector(MAIN_SELECTOR);
            const curMain = document.querySelector(MAIN_SELECTOR);
            if (newMain && curMain) {
                curMain.innerHTML = newMain.innerHTML;
                executeScripts(curMain);
                // re-parse unobtrusive validation in new main content
                try {
                    if (window.jQuery && jQuery.validator && jQuery.validator.unobtrusive && typeof jQuery.validator.unobtrusive.parse === 'function') {
                        jQuery.validator.unobtrusive.parse(curMain);
                    }
                } catch (e) { }
                // initialize inactive-reason controls in the new content
                try { updateInactiveReasonControls(curMain); } catch (e) { }
                try { applyTableFilters(curMain); } catch (e) { }
                    try { dismissAlerts(curMain); } catch (e) { }
            } else {
                // fallback: replace body if main not found
                document.body.innerHTML = text;
            }

            // update document title
            const newTitle = doc.querySelector('title');
            if (newTitle) document.title = newTitle.textContent;

            // update active nav link
            updateActiveLink(url);

            if (push) history.pushState({ url: url }, '', url);
        } catch (err) {
            console.error('PJAX load failed', err);
            window.location.href = url; // fallback to full navigation
        }
    }

    function updateActiveLink(url) {
        // mark link with matching href as active
        const links = document.querySelectorAll(NAV_LINK_SELECTOR);
        links.forEach(a => a.classList.remove('active'));
        const match = Array.from(links).find(a => {
            try { return new URL(a.href).pathname === new URL(url, location.origin).pathname; }
            catch { return a.getAttribute('href') === url; }
        });
        if (match) match.classList.add('active');
    }

    // intercept nav clicks (sidebar links and any a[data-pjax])
    document.addEventListener('click', function(e) {
        const a = e.target.closest('a');
        if (!a) return;
        if (!a.matches(NAV_LINK_SELECTOR)) return;
        if (a.hasAttribute('data-no-ajax') || a.target === '_blank') return;
        e.preventDefault();
        const href = a.href;
        loadUrl(href, true);
    });

    // Handle edit button to open various modals (uses Bootstrap)
    document.addEventListener('click', function(e) {
        const btn = e.target.closest('[data-edit-id], [data-edit-mid], [data-edit-breed], [data-edit-health]');
        if (!btn) return;
        e.preventDefault();
        const id = btn.getAttribute('data-edit-id') || btn.getAttribute('data-edit-mid');
        const isMilk = btn.hasAttribute('data-edit-mid');
        let url;
        let modalId;
        if (btn.hasAttribute('data-edit-mid')) {
            url = '/MilkProduction/EditModal?mid=' + encodeURIComponent(id);
            modalId = 'milkEditModal';
        } else if (btn.hasAttribute('data-edit-breed')) {
            url = '/Breeding/EditModal?id=' + encodeURIComponent(btn.getAttribute('data-edit-breed'));
            modalId = 'breedEditModal';
        } else if (btn.hasAttribute('data-edit-health')) {
            url = '/Health/EditModal?id=' + encodeURIComponent(btn.getAttribute('data-edit-health'));
            modalId = 'healthEditModal';
        } else if (btn.hasAttribute('data-edit-id')) {
            url = '/Cows/EditModal?id=' + encodeURIComponent(btn.getAttribute('data-edit-id'));
            modalId = 'cowEditModal';
        } else {
            return;
        }
        // debug log
        console.log('Edit clicked, id=', id, 'url=', url);
        // load the edit modal partial via AJAX
        fetch(url, {
            headers: { 'X-Requested-With': 'XMLHttpRequest' },
            credentials: 'same-origin'
        })
            .then(r => r.text())
            .then(html => {
                console.log('EditModal response length:', html.length);
                const doc = parseHTML(html);
                let modalPartial = doc.querySelector('#' + modalId);
                // Fallback for fragment-only responses (without <html>/<body>)
                if (!modalPartial) {
                    const tmp = document.createElement('div');
                    tmp.innerHTML = html;
                    modalPartial = tmp.querySelector('#' + modalId);
                }

                if (modalPartial) {
                    // remove existing modal if any
                    const existing = document.getElementById(modalId);
                    if (existing) existing.remove();
                    // remove any existing backdrop to avoid overlay issues
                    const existingBackdrop = document.querySelector('.modal-backdrop');
                    if (existingBackdrop) existingBackdrop.remove();
                    // insert modal into modalContainer
                    const container = document.getElementById('modalContainer') || document.body;
                    container.insertAdjacentHTML('beforeend', modalPartial.outerHTML);
                    const newModalEl = document.getElementById(modalId);
                    // execute any scripts inside the modal (if present)
                    try { executeScripts(newModalEl); } catch (e) {}
                    // ensure unobtrusive validation is parsed inside the modal so required rules apply
                    try {
                        if (window.jQuery && jQuery.validator && jQuery.validator.unobtrusive && typeof jQuery.validator.unobtrusive.parse === 'function') {
                            jQuery.validator.unobtrusive.parse(newModalEl);
                        }
                    } catch (e) { }
                    if (modalId === 'cowEditModal') {
                        // set initial visibility for reason and status
                        try {
                            const isActiveAttr = newModalEl.getAttribute('data-isactive');
                            const sel = newModalEl.querySelector('#modalCowStatusSelect');
                            if (sel && isActiveAttr !== null) {
                                sel.value = (isActiveAttr === 'true') ? 'true' : 'false';
                            }
                            updateInactiveReasonControls(newModalEl);
                        } catch (e) { }
                    }
                    // If this is the breeding modal, re-bind sync script
                    try {
                        if (modalId === 'breedEditModal') {
                            var sel = newModalEl.querySelector('select[name="Draft.CowId"]');
                            if (sel) sel.dispatchEvent(new Event('change'));
                        }
                    } catch (e) {}
                    var modal = new bootstrap.Modal(newModalEl);
                    modal.show();
                } else {
                    console.error('Edit modal element not found in response', {
                        responsePreview: html.slice(0, 300)
                    });
                }
            }).catch(err => console.error('Failed to load edit modal', err));
    });

    // Keep inactive-reason visibility in sync when status changes.
    document.addEventListener('change', function(e) {
        const target = e.target;
        if (!target || !target.matches('#cowStatusSelect, #modalCowStatusSelect')) return;
        const scope = target.closest('#cowEditModal') || document;
        updateInactiveReasonControls(scope);
    });

    // handle back/forward
    window.addEventListener('popstate', function(e) {
        const url = (e.state && e.state.url) || location.href;
        loadUrl(url, false);
    });

    // intercept AJAX forms inside main container (and modal forms)
    document.addEventListener('submit', function(e) {
        const form = e.target;
        const isModalForm = form.hasAttribute('data-pjax-modal');
        // Allow handling of forms explicitly marked to use PJAX (data-pjax-submit) even
        // when they are not inside the main container.
        const isPjaxMarked = form.hasAttribute('data-pjax-submit');
        if (!isModalForm && !isPjaxMarked && !form.closest(MAIN_SELECTOR)) return;
        // only handle forms marked to be ajax or those without data-fullsubmit="true"
        if (form.getAttribute('data-fullsubmit') === 'true') return;
        if (form.hasAttribute('data-no-ajax')) return;

        // respect data-fullsubmit to force regular submit
        if (form.getAttribute('data-fullsubmit') === 'true') return;

        // handle optional confirmation attribute
        const confirmMsg = form.getAttribute('data-confirm');
        if (confirmMsg && !window.confirm(confirmMsg)) {
            e.preventDefault();
            return;
        }

        e.preventDefault();
        const action = form.action || location.href;
        const method = (form.method || 'get').toUpperCase();

        const formData = new FormData(form);
        if (isModalForm) {
            // Force AJAX response shape for modal saves.
            formData.append('IsAjax', 'true');
        }

        let options = {
            method: method,
            credentials: 'same-origin',
            headers: { 'X-Requested-With': 'XMLHttpRequest' }
        };
        if (method === 'GET') {
            const params = new URLSearchParams(formData);
            loadUrl(action + (action.includes('?') ? '&' : '?') + params.toString(), true);
            return;
        } else {
            options.body = formData;
        }

        fetch(action, options).then(async r => {
            const resUrl = r.url;
            const ct = r.headers.get('content-type') || '';
            // If server returned 400 with partial HTML (validation), return that
            if (r.status === 400) {
                const text = await r.text();
                return { status: 400, text };
            }
            if (ct.indexOf('application/json') !== -1) {
                return { json: await r.json(), url: resUrl };
            }
            return { text: await r.text(), url: resUrl };
        }).then(res => {
            if (res.status === 400 && res.text) {
                // Server returned validation errors as HTML.
                // If this was a modal submission, replace modal markup so validation appears inside modal.
                const tmp = document.createElement('div');
                tmp.innerHTML = res.text;
                const modal = document.getElementById('cowEditModal') || document.getElementById('milkEditModal');
                if (modal) {
                    const newModal = tmp.querySelector('#cowEditModal') || tmp.querySelector('#milkEditModal');
                    if (newModal) {
                        modal.parentNode.replaceChild(newModal, modal);
                        try {
                            if (window.jQuery && jQuery.validator && jQuery.validator.unobtrusive && typeof jQuery.validator.unobtrusive.parse === 'function') {
                                jQuery.validator.unobtrusive.parse(newModal);
                            }
                        } catch (e) { }
                        try { updateInactiveReasonControls(newModal); } catch (e) { }
                        // re-show modal
                        var bs = new bootstrap.Modal(newModal);
                        bs.show();
                        // auto-dismiss any alert messages inside the modal after a short delay
                        try {
                            newModal.querySelectorAll('.alert').forEach(function (a) {
                                setTimeout(function () { try { a.remove(); } catch (e) { } }, 1000);
                            });
                        } catch (e) { }
                        return;
                    }
                }

                // Non-modal form: replace main container content with returned HTML so validation displays inline.
                try {
                    const doc = tmp; // already parsed into tmp
                    const newMain = doc.querySelector(MAIN_SELECTOR);
                    const curMain = document.querySelector(MAIN_SELECTOR);
                    if (newMain && curMain) {
                        curMain.innerHTML = newMain.innerHTML;
                        executeScripts(curMain);
                        try { initializeMilkProductionForm(curMain); } catch (e) { }
                        try {
                            if (window.jQuery && jQuery.validator && jQuery.validator.unobtrusive && typeof jQuery.validator.unobtrusive.parse === 'function') {
                                jQuery.validator.unobtrusive.parse(curMain);
                            }
                        } catch (e) { }
                        try { updateInactiveReasonControls(curMain); } catch (e) { }
                        try { applyTableFilters(curMain); } catch (e) { }
                        try { dismissAlerts(curMain); } catch (e) { }
                        // auto-dismiss any transient alerts inserted by server after 3s
                        try {
                            curMain.querySelectorAll('.alert').forEach(function (a) {
                                setTimeout(function () { try { a.remove(); } catch (e) { } }, 1000);
                            });
                        } catch (e) { }
                    } else {
                        // fallback: log full response for debugging
                        console.log(res.text);
                    }
                } catch (e) {
                    console.error('Failed to apply validation HTML', e);
                    console.log(res.text);
                }
                return;
            }

            if (res.json) {
                const j = res.json;
                if (j.duplicate) {
                    // Show duplicate message inside modal if present, auto-dismiss after short time.
                    const modalEl = document.getElementById('cowEditModal') || document.getElementById('milkEditModal') || document.getElementById('breedEditModal') || document.getElementById('healthEditModal');
                    const msg = j.message || 'Duplicate record.';
                    if (modalEl) {
                        try {
                            const container = modalEl.querySelector('.modal-body') || modalEl;
                            const alertDiv = document.createElement('div');
                            alertDiv.className = 'alert alert-danger';
                            alertDiv.textContent = msg;
                            container.insertAdjacentElement('afterbegin', alertDiv);
                            setTimeout(function () { try { alertDiv.remove(); } catch (e) { } }, 3000);
                        } catch (e) { console.error('show duplicate in modal failed', e); showTempAlert(msg, 'error'); }
                    } else {
                        showTempAlert(msg, 'error');
                    }
                    // do not redirect or refresh; keep modal open so user can correct
                    return;
                }
                if (j.success === false && j.message) {
                    // show server-side error message for failed AJAX saves
                    showTempAlert(j.message, 'error');
                    return;
                }
                if (j.redirect) {
                    // if modal post success, close modal and reload main content
                    if (j.success) {
                        const modalEl = document.getElementById('cowEditModal') || document.getElementById('milkEditModal');
                        if (modalEl) {
                            try {
                                const bsInstance = bootstrap.Modal.getInstance(modalEl) || new bootstrap.Modal(modalEl);
                                // when modal fully hidden, remove DOM and reload main content
                                modalEl.addEventListener('hidden.bs.modal', function onHidden() {
                                    modalEl.removeEventListener('hidden.bs.modal', onHidden);
                                    // ensure any backdrop removed
                                    const backdrop = document.querySelector('.modal-backdrop');
                                    if (backdrop) backdrop.remove();
                                    // remove modal-open class from body (cleanup)
                                    try { document.body.classList.remove('modal-open'); } catch(e) {}
                                    if (modalEl.parentNode) modalEl.parentNode.removeChild(modalEl);
                                    // small delay to allow DOM to settle then reload main
                                    setTimeout(() => loadUrl(j.redirect, true), 50);
                                });
                                bsInstance.hide();
                            } catch (e) {
                                // fallback: remove and reload
                                try { modalEl.remove(); } catch (e) {}
                                loadUrl(j.redirect, true);
                            }
                        } else {
                            loadUrl(j.redirect, true);
                        }
                        return;
                    }
                    // non-successful AJAX response with redirect - show message then reload
                    if (j.message) showTempAlert(j.message, j.success ? 'success' : 'error');
                    loadUrl(j.redirect, true);
                    return;
                }
                return;
            }

            const text = res.text;
            const doc = parseHTML(text);
            const newMain = doc.querySelector(MAIN_SELECTOR);
            const curMain = document.querySelector(MAIN_SELECTOR);
            if (newMain && curMain) {
                curMain.innerHTML = newMain.innerHTML;
                executeScripts(curMain);
                try { initializeMilkProductionForm(curMain); } catch (e) { }
            } else {
                document.body.innerHTML = text;
            }
            const newTitle = doc.querySelector('title');
            if (newTitle) document.title = newTitle.textContent;
            try { history.replaceState({ url: res.url }, '', res.url); } catch (e) {}
        }).catch(err => {
            console.error('AJAX form submit failed', err);
            form.submit();
        });
    });

    // initialize active link on load
    document.addEventListener('DOMContentLoaded', function() {
        updateActiveLink(location.href);
        try { initializeMilkProductionForm(document); } catch (e) { }
        try { updateInactiveReasonControls(document); } catch (e) { }
        try { applyTableFilters(document); } catch (e) { }
        try { dismissAlerts(document); } catch (e) { }
        // Bind global filter controls so they work even after PJAX navigation
        document.addEventListener('input', function (e) {
            if (e.target && e.target.id === 'cowFilter') {
                try { applyTableFilters(document); } catch (err) { }
            }
        });
        document.addEventListener('change', function (e) {
            if (e.target && (e.target.id === 'filterActive' || e.target.id === 'filterInactive')) {
                try { applyTableFilters(document); } catch (err) { }
            }
        });
    });
})();
