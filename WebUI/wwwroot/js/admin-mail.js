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
        const reader = document.querySelector('.mail-reader');
        const modalElement = document.getElementById('composeMailModal');
        if (!reader || !modalElement || !window.bootstrap) {
            return;
        }

        const modal = bootstrap.Modal.getOrCreateInstance(modalElement);
        const toInput = document.getElementById('composeTo');
        const subjectInput = document.getElementById('composeSubject');
        const editor = document.getElementById('composeEditor');
        const inReplyTo = document.getElementById('composeInReplyTo');
        const references = document.getElementById('composeReferences');
        const body = document.getElementById('mailBody');

        document.querySelectorAll('[data-mail-action]').forEach(function (button) {
            button.addEventListener('click', function () {
                const action = button.dataset.mailAction;
                const subject = reader.dataset.subject || '';
                const from = reader.dataset.from || '';
                const messageId = reader.dataset.messageId || '';
                const existingReferences = reader.dataset.references || '';

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
        });

        function prefixSubject(subject, prefix) {
            return subject.toLocaleLowerCase('tr-TR').startsWith(prefix.toLocaleLowerCase('tr-TR'))
                ? subject
                : `${prefix} ${subject}`;
        }
    }

    function initMoveForm() {
        const form = document.querySelector('.mail-move-form');
        const select = form?.querySelector('select');
        const button = form?.querySelector('button');
        if (!select || !button) return;

        select.addEventListener('change', function () {
            button.disabled = !select.value;
        });
    }

    function initDeleteConfirmation() {
        document.querySelectorAll('.mail-delete-form').forEach(function (form) {
            form.addEventListener('submit', async function (event) {
                event.preventDefault();
                const confirmed = window.TumMenuAlerts
                    ? await TumMenuAlerts.confirm('Maili çöp kutusuna taşı?', 'Mail çöp kutusuna taşınacak.', 'Taşı')
                    : window.confirm('Maili çöp kutusuna taşımak istiyor musunuz?');

                if (confirmed) {
                    form.submit();
                }
            });
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
        if (!document.querySelector('.mail-page--selected')) return;
        const readerPanel = document.querySelector('.mail-reader-panel');
        if (!readerPanel) return;

        const back = document.createElement('a');
        back.className = 'mail-mobile-back';
        back.href = readerPanel.closest('.mail-page')?.dataset.backUrl || '#';
        back.textContent = '← Listeye dön';
        readerPanel.prepend(back);
    }

    document.addEventListener('DOMContentLoaded', function () {
        initMailComposer();
        initReaderActions();
        initMoveForm();
        initDeleteConfirmation();
        initBulkActions();
        initMobileBack();
    });
})();
