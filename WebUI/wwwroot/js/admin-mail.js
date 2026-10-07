(function () {
    'use strict';

    function initMailComposer() {
        const form = document.getElementById('composeMailForm');
        const editor = document.getElementById('composeEditor');
        const bodyInput = document.getElementById('composeBodyHtml');
        const modalElement = document.getElementById('composeMailModal');

        if (!form || !editor || !bodyInput) {
            return;
        }

        function syncBody() {
            bodyInput.value = editor.innerHTML.trim();
        }

        document.querySelectorAll('[data-command]').forEach(function (button) {
            button.addEventListener('click', function () {
                editor.focus();
                const command = button.dataset.command;
                if (command === 'createLink') {
                    const url = window.prompt('Bağlantı adresi');
                    if (url) {
                        document.execCommand(command, false, url);
                    }
                    return;
                }

                document.execCommand(command, false, null);
                syncBody();
            });
        });

        editor.addEventListener('input', syncBody);

        form.querySelectorAll('[data-toggle-recipient]').forEach(function (button) {
            button.addEventListener('click', function () {
                const target = document.querySelector('[data-recipient-row="' + button.dataset.toggleRecipient + '"]');
                target?.classList.toggle('d-none');
                if (target && !target.classList.contains('d-none')) {
                    target.querySelector('input')?.focus();
                }
            });
        });

        const fileInput = form.querySelector('input[type="file"]');
        const selectedFiles = form.querySelector('[data-selected-files]');
        fileInput?.addEventListener('change', function () {
            if (!selectedFiles) return;
            const files = Array.from(fileInput.files || []);
            selectedFiles.textContent = files.length
                ? files.map(file => `${file.name} (${formatBytes(file.size)})`).join(' · ')
                : '';
        });

        form.addEventListener('submit', function (event) {
            syncBody();
            if (!editor.innerText.trim()) {
                event.preventDefault();
                editor.focus();
                window.TumMenuAlerts?.toast('warning', 'Mesaj gövdesi boş olamaz.');
                return;
            }

            const submitButton = form.querySelector('.mail-send-button');
            if (submitButton) {
                submitButton.disabled = true;
                submitButton.innerHTML = '<span class="spinner-border spinner-border-sm me-2" aria-hidden="true"></span>Gönderiliyor...';
            }
        });

        modalElement?.addEventListener('hidden.bs.modal', function () {
            form.reset();
            editor.innerHTML = '';
            bodyInput.value = '';
            modalElement.querySelector('.mail-modal-kicker').textContent = 'Yeni mesaj';
            modalElement.querySelector('.modal-title').textContent = 'Mail oluştur';
            form.querySelectorAll('[data-recipient-row]').forEach(row => row.classList.add('d-none'));
            if (selectedFiles) selectedFiles.textContent = '';
            form.querySelectorAll('.mail-send-button').forEach(button => {
                button.disabled = false;
                button.innerHTML = '<i class="fas fa-paper-plane me-2"></i>Gönder';
            });
        });

        function formatBytes(bytes) {
            if (bytes < 1024) return `${bytes} B`;
            if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
            return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
        }
    }

    function initReaderActions() {
        const modalElement = document.getElementById('composeMailModal');
        if (!modalElement || !window.bootstrap) {
            return;
        }

        const modal = bootstrap.Modal.getOrCreateInstance(modalElement);
        const toInput = document.getElementById('composeTo');
        const subjectInput = document.getElementById('composeSubject');
        const editor = document.getElementById('composeEditor');
        const inReplyTo = document.getElementById('composeInReplyTo');
        const references = document.getElementById('composeReferences');

        document.addEventListener('click', function (event) {
            const button = event.target.closest('[data-mail-action]');
            if (!button) return;

            const reader = button.closest('.mail-reader');
            if (!reader) return;

            const action = button.dataset.mailAction;
            const subject = reader.dataset.subject || '';
            const from = reader.dataset.from || '';
            const messageId = reader.dataset.messageId || '';
            const existingReferences = reader.dataset.references || '';
            const body = reader.querySelector('.mail-reader-body');

            toInput.value = action === 'reply' ? from : '';
            subjectInput.value = prefixSubject(subject, action === 'reply' ? 'Re:' : 'Fwd:');
            inReplyTo.value = action === 'reply' ? messageId : '';
            references.value = [existingReferences, messageId].filter(Boolean).join(' ');

            if (action === 'reply') {
                editor.innerHTML = `<p><br></p><blockquote class="mail-quoted-message">${body?.innerHTML || ''}</blockquote>`;
            } else {
                editor.innerHTML = `<p><br></p><p>---------- İletilen mesaj ----------</p>${body?.innerHTML || ''}`;
            }

            modalElement.querySelector('.mail-modal-kicker').textContent = action === 'reply' ? 'Yanıt' : 'İlet';
            modalElement.querySelector('.modal-title').textContent = action === 'reply' ? 'Yanıt yaz' : 'Maili ilet';
            modal.show();
        });

        function prefixSubject(subject, prefix) {
            return subject.toLocaleLowerCase('tr-TR').startsWith(prefix.toLocaleLowerCase('tr-TR'))
                ? subject
                : `${prefix} ${subject}`;
        }
    }

    function initMoveForm() {
        document.addEventListener('change', function (event) {
            const select = event.target.closest('.mail-move-form select');
            if (!select) return;

            const button = select.closest('form')?.querySelector('button[type="submit"]');
            if (button) button.disabled = !select.value;
        });
    }

    function initDeleteConfirmation() {
        document.addEventListener('submit', async function (event) {
            const form = event.target.closest('.mail-delete-form');
            if (!form || form.dataset.confirmed === 'true') return;

            event.preventDefault();
            const confirmed = window.TumMenuAlerts
                ? await TumMenuAlerts.confirm('Maili çöp kutusuna taşı?', 'Mail çöp kutusuna taşınacak.', 'Taşı')
                : window.confirm('Maili çöp kutusuna taşımak istiyor musunuz?');

            if (confirmed) {
                form.dataset.confirmed = 'true';
                form.requestSubmit();
            }
        });
    }

    function initBulkActions() {
        const form = document.getElementById('mailBulkForm');
        if (!form) return;

        const checkboxes = Array.from(form.querySelectorAll('[data-mail-select]'));
        const selectAll = document.querySelector('[data-select-all]');
        const toolbar = form.querySelector('[data-bulk-toolbar]');
        const count = form.querySelector('[data-bulk-count]');
        const actionButtons = Array.from(form.querySelectorAll('[data-bulk-action]'));
        const moveSelect = form.querySelector('[data-bulk-move-select]');
        const moveButton = form.querySelector('[data-bulk-move-button]');
        let allowConfirmedDelete = false;

        function refreshSelection() {
            const selected = checkboxes.filter(checkbox => checkbox.checked);
            const selectedCount = selected.length;
            if (count) count.textContent = String(selectedCount);
            if (toolbar) toolbar.hidden = selectedCount === 0;

            actionButtons.forEach(button => {
                button.disabled = selectedCount === 0;
            });
            if (moveButton) {
                moveButton.disabled = selectedCount === 0 || !moveSelect?.value;
            }

            if (selectAll) {
                selectAll.checked = checkboxes.length > 0 && selectedCount === checkboxes.length;
                selectAll.indeterminate = selectedCount > 0 && selectedCount < checkboxes.length;
            }

            checkboxes.forEach(checkbox => {
                checkbox.closest('[data-mail-row]')?.classList.toggle('is-bulk-selected', checkbox.checked);
            });
        }

        selectAll?.addEventListener('change', function () {
            checkboxes.forEach(checkbox => {
                checkbox.checked = selectAll.checked;
            });
            refreshSelection();
        });

        checkboxes.forEach(checkbox => checkbox.addEventListener('change', refreshSelection));
        moveSelect?.addEventListener('change', refreshSelection);

        form.addEventListener('submit', async function (event) {
            const operation = event.submitter?.value;
            const selectedCount = checkboxes.filter(checkbox => checkbox.checked).length;
            if (!operation || selectedCount === 0) {
                event.preventDefault();
                window.TumMenuAlerts?.toast('warning', 'Önce işlem yapılacak mailleri seçin.');
                return;
            }

            if (operation === 'move' && !moveSelect?.value) {
                event.preventDefault();
                moveSelect?.focus();
                window.TumMenuAlerts?.toast('warning', 'Taşımak için hedef klasör seçin.');
                return;
            }

            if (operation !== 'delete' || allowConfirmedDelete) {
                allowConfirmedDelete = false;
                return;
            }

            event.preventDefault();
            const confirmed = window.TumMenuAlerts
                ? await window.TumMenuAlerts.confirm(
                    `${selectedCount} mail çöp kutusuna taşınsın mı?`,
                    'Bu işlem seçili mailleri çöp kutusuna taşıyacak.',
                    'Mailleri sil')
                : window.confirm(`${selectedCount} mail çöp kutusuna taşınsın mı?`);

            if (confirmed) {
                allowConfirmedDelete = true;
                form.requestSubmit(event.submitter);
            }
        });

        refreshSelection();
    }

    function initMobileBack() {
        const root = document.querySelector('.mail-page');
        const readerPanel = root?.querySelector('.mail-reader-panel');
        if (!readerPanel) return;

        readerPanel.querySelector('.mail-mobile-back')?.remove();
        if (!root.classList.contains('mail-page--selected')) return;

        const back = document.createElement('a');
        back.className = 'mail-mobile-back';
        back.href = root.dataset.backUrl || '#';
        back.textContent = '← Listeye dön';
        readerPanel.prepend(back);
    }

    function initMailNavigation() {
        const root = document.querySelector('.mail-page');
        const panel = root?.querySelector('.mail-reader-panel');
        if (!root || !panel) return;

        const cache = new Map();
        const maxCachedMessages = 12;
        let activeRequest;

        function cacheMarkup(key, markup) {
            cache.delete(key);
            cache.set(key, markup);
            while (cache.size > maxCachedMessages) {
                cache.delete(cache.keys().next().value);
            }
        }

        function getMessageKey(url) {
            const folder = url.searchParams.get('folder') || '';
            const uid = url.searchParams.get('uid') || '';
            return `${folder}:${uid}`;
        }

        function selectListRow(folder, uid) {
            let selectedRow;
            root.querySelectorAll('[data-mail-row]').forEach(function (row) {
                const link = row.querySelector('.mail-list-item-link');
                const url = link ? new URL(link.href, window.location.origin) : null;
                const isSelected = url?.searchParams.get('folder') === folder
                    && url?.searchParams.get('uid') === uid;
                row.classList.toggle('is-selected', isSelected);
                if (isSelected) selectedRow = row;
            });

            if (selectedRow?.classList.contains('is-unread')) {
                selectedRow.classList.remove('is-unread');
                selectedRow.querySelector('.mail-unread-dot')?.remove();
                const folderLink = root.querySelector(`.mail-folder-link[data-folder-key="${CSS.escape(folder)}"]`);
                const unreadCount = folderLink?.querySelector('.mail-unread-count');
                if (unreadCount) {
                    const count = Number.parseInt(unreadCount.textContent, 10) - 1;
                    if (count > 0) unreadCount.textContent = String(count);
                    else unreadCount.remove();
                }
            }
        }

        function populateFolderOptions(reader) {
            const select = reader.querySelector('.mail-move-form select');
            if (!select || select.options.length > 1) return;

            const currentFolder = reader.dataset.folderKey;
            root.querySelectorAll('.mail-folder-link[data-folder-key]').forEach(function (folderLink) {
                const key = folderLink.dataset.folderKey;
                if (key === currentFolder) return;

                const label = folderLink.querySelector('.mail-folder-link-label span:last-child')?.textContent.trim();
                if (!key || !label) return;
                const option = document.createElement('option');
                option.value = key;
                option.textContent = label;
                select.append(option);
            });
        }

        function showMessage(markup, folder, uid) {
            panel.innerHTML = markup;
            const reader = panel.querySelector('.mail-reader');
            if (reader) populateFolderOptions(reader);
            root.classList.add('mail-page--selected');
            selectListRow(folder, uid);
            initMobileBack();
        }

        function showEmptyList() {
            activeRequest?.abort();
            activeRequest = undefined;
            panel.innerHTML = '<div class="mail-reader-empty"><div class="mail-reader-empty-mark"><i class="fas fa-envelope-open-text"></i></div><h2>Bir mail seçin</h2><p>Okumak istediğiniz maili listeden seçtiğinizde içeriği burada açılır.</p></div>';
            root.classList.remove('mail-page--selected');
            root.querySelectorAll('[data-mail-row].is-selected').forEach(row => row.classList.remove('is-selected'));
            initMobileBack();
        }

        async function loadMessage(urlValue, { push = false, fallbackUrl = null } = {}) {
            const url = new URL(urlValue, window.location.href);
            const folder = url.searchParams.get('folder') || '';
            const uid = url.searchParams.get('uid') || '';
            if (!folder || !uid) {
                showEmptyList();
                return;
            }

            const key = getMessageKey(url);
            if (panel.querySelector(`.mail-reader[data-folder-key="${CSS.escape(folder)}"][data-uid="${CSS.escape(uid)}"]`)) {
                if (push) window.history.pushState({ mail: true }, '', url);
                return;
            }

            activeRequest?.abort();
            activeRequest = undefined;

            if (cache.has(key)) {
                const markup = cache.get(key);
                cache.delete(key);
                cache.set(key, markup);
                showMessage(markup, folder, uid);
                if (push) window.history.pushState({ mail: true }, '', url);
                return;
            }

            const controller = new AbortController();
            activeRequest = controller;
            panel.setAttribute('aria-busy', 'true');
            panel.innerHTML = '<div class="mail-reader-empty" role="status"><span class="spinner-border spinner-border-sm me-2" aria-hidden="true"></span>Mail açılıyor...</div>';
            root.classList.add('mail-page--selected');
            initMobileBack();

            const requestUrl = new URL(root.dataset.messageUrl, window.location.origin);
            requestUrl.searchParams.set('folder', folder);
            requestUrl.searchParams.set('uid', uid);
            const search = url.searchParams.get('q');
            if (search) requestUrl.searchParams.set('search', search);

            try {
                const response = await fetch(requestUrl, {
                    credentials: 'same-origin',
                    headers: { 'X-Requested-With': 'XMLHttpRequest' },
                    signal: controller.signal
                });
                if (!response.ok) throw new Error(`Mail isteği başarısız: ${response.status}`);

                const markup = await response.text();
                const template = document.createElement('template');
                template.innerHTML = markup;
                const reader = template.content.querySelector('.mail-reader');
                if (!reader) throw new Error('Mail içeriği yanıtı beklenen biçimde değil.');

                cacheMarkup(key, markup);
                showMessage(markup, folder, uid);
                if (push) window.history.pushState({ mail: true }, '', url);
            } catch (error) {
                if (error.name === 'AbortError') return;
                if (fallbackUrl) {
                    window.location.assign(fallbackUrl);
                    return;
                }

                panel.innerHTML = '<div class="mail-reader-empty" role="alert"><h2>Mail açılamadı</h2><p>Mail içeriği alınamadı. Lütfen yeniden deneyin.</p></div>';
                root.classList.add('mail-page--selected');
                initMobileBack();
            } finally {
                if (activeRequest === controller) {
                    activeRequest = undefined;
                    panel.removeAttribute('aria-busy');
                }
            }
        }

        const initialReader = panel.querySelector('.mail-reader');
        if (initialReader) {
            cacheMarkup(`${initialReader.dataset.folderKey}:${initialReader.dataset.uid}`, panel.innerHTML);
            selectListRow(initialReader.dataset.folderKey, initialReader.dataset.uid);
        }

        document.addEventListener('click', function (event) {
            if (event.button !== 0 || event.metaKey || event.ctrlKey || event.shiftKey || event.altKey) return;

            const backLink = event.target.closest('.mail-mobile-back');
            if (backLink) {
                event.preventDefault();
                window.history.pushState({ mail: true }, '', backLink.href);
                showEmptyList();
                return;
            }

            const messageLink = event.target.closest('a.mail-list-item-link');
            if (!messageLink || !root.contains(messageLink)) return;

            event.preventDefault();
            loadMessage(messageLink.href, { push: true, fallbackUrl: messageLink.href });
        });

        window.addEventListener('popstate', function () {
            const url = new URL(window.location.href);
            if (url.searchParams.has('uid')) loadMessage(url, { push: false });
            else showEmptyList();
        });
    }

    document.addEventListener('DOMContentLoaded', function () {
        initMailComposer();
        initReaderActions();
        initMoveForm();
        initDeleteConfirmation();
        initBulkActions();
        initMobileBack();
        initMailNavigation();
    });
})();
