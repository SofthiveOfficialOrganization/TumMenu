(function () {
    const badge = document.querySelector('[data-order-notification-badge]');
    const list = document.querySelector('[data-order-notification-list]');
    const bell = document.querySelector('[data-order-notification-bell]');
    const soundTestButton = document.querySelector('[data-order-notification-sound-test]');
    const volumeInput = document.querySelector('[data-order-notification-volume]');
    const volumeText = document.querySelector('[data-order-notification-volume-text]');
    let unseenCount = Number(sessionStorage.getItem('tummenu.orderNotificationCount') || '0');
    let audioContext = null;
    let audioUnlocked = false;
    let soundEnabled = true;
    let volumeMultiplier = Number(localStorage.getItem('tummenu.orderNotificationVolume') || '2');
    volumeMultiplier = Math.max(1, Math.min(3, volumeMultiplier));

    function renderBadge() {
        if (!badge) return;
        badge.textContent = String(unseenCount);
        badge.hidden = unseenCount <= 0;
    }

    function addDropdownItem(order) {
        if (!list) return;
        const empty = list.querySelector('[data-order-notification-empty]');
        if (empty) empty.remove();

        const href = '/Admin/OrderRequests';
        const row = document.createElement('a');
        row.href = href;
        row.className = 'tm-admin-order-notification-item';
        row.innerHTML = `
            <strong>${escapeHtml(order.storeName || 'Dükkan')}</strong>
            <span>${escapeHtml(order.customerName || 'Müşteri')} · Masa ${escapeHtml(order.tableNumber || '-')}</span>
            <small>${formatMoney(order.subtotal)} · ${Number(order.itemCount || 0)} ürün</small>
        `;
        list.prepend(row);

        while (list.querySelectorAll('.tm-admin-order-notification-item').length > 5) {
            list.querySelector('.tm-admin-order-notification-item:last-child')?.remove();
        }
    }

    function renderVolumeState() {
        if (volumeInput) {
            volumeInput.value = String(volumeMultiplier);
            volumeInput.title = `Bildirim sesi ${volumeMultiplier}x`;
        }
        if (volumeText) {
            volumeText.textContent = `${volumeMultiplier}x`;
        }
        if (soundTestButton) {
            soundTestButton.classList.toggle('is-enabled', audioUnlocked);
            soundTestButton.title = audioUnlocked
                ? 'Bildirim sesini test et'
                : 'Bildirim sesini etkinleştir ve test et';
        }
    }

    function getAudioContext() {
        const AudioContext = window.AudioContext || window.webkitAudioContext;
        if (!AudioContext) return null;
        audioContext = audioContext || new AudioContext();
        return audioContext;
    }

    async function unlockAudio(playTestTone) {
        try {
            const ctx = getAudioContext();
            if (!ctx) return false;
            if (ctx.state === 'suspended') {
                await ctx.resume();
            }

            audioUnlocked = ctx.state === 'running';
            if (audioUnlocked) {
                soundEnabled = true;
                if (playTestTone) playSound();
            }
        } catch (_) {
            audioUnlocked = false;
        }

        renderVolumeState();
        return audioUnlocked;
    }

    function playSound() {
        if (!soundEnabled || !audioUnlocked) return;
        try {
            const ctx = getAudioContext();
            if (!ctx || ctx.state !== 'running') return;

            const master = ctx.createGain();
            const toneFilter = ctx.createBiquadFilter();
            const delay = ctx.createDelay();
            const wet = ctx.createGain();

            master.gain.setValueAtTime(0.36 * volumeMultiplier, ctx.currentTime);
            toneFilter.type = 'lowpass';
            toneFilter.frequency.setValueAtTime(4200, ctx.currentTime);
            delay.delayTime.setValueAtTime(0.09, ctx.currentTime);
            wet.gain.setValueAtTime(0.08, ctx.currentTime);

            master.connect(toneFilter);
            toneFilter.connect(ctx.destination);
            toneFilter.connect(delay);
            delay.connect(wet);
            wet.connect(ctx.destination);

            const melody = [
                { start: 0.00, frequency: 523.25, duration: 0.18 },
                { start: 0.20, frequency: 659.25, duration: 0.18 },
                { start: 0.40, frequency: 783.99, duration: 0.18 },
                { start: 0.65, frequency: 880.00, duration: 0.30 },
                { start: 1.00, frequency: 783.99, duration: 0.55 }
            ];

            melody.forEach((tone, index) => {
                const start = ctx.currentTime + tone.start;
                const end = start + tone.duration;
                const noteGain = ctx.createGain();
                const body = ctx.createOscillator();
                const sparkle = ctx.createOscillator();

                body.type = 'triangle';
                body.frequency.setValueAtTime(tone.frequency, start);
                sparkle.type = 'sine';
                sparkle.frequency.setValueAtTime(tone.frequency * 2, start);

                noteGain.gain.setValueAtTime(0.001, start);
                noteGain.gain.exponentialRampToValueAtTime(index === melody.length - 1 ? 0.2 : 0.24, start + 0.018);
                noteGain.gain.exponentialRampToValueAtTime(0.055, start + Math.min(tone.duration * 0.55, 0.16));
                noteGain.gain.exponentialRampToValueAtTime(0.001, end);

                body.connect(noteGain);
                sparkle.connect(noteGain);
                noteGain.connect(master);
                body.start(start);
                sparkle.start(start);
                body.stop(end + 0.03);
                sparkle.stop(end + 0.03);
            });
        } catch (_) {
        }
    }

    function notify(order) {
        unseenCount += 1;
        sessionStorage.setItem('tummenu.orderNotificationCount', String(unseenCount));
        renderBadge();
        addDropdownItem(order);
        playSound();

        if (window.TumMenuAlerts) {
            TumMenuAlerts.toast('info', `${order.storeName || 'Dükkan'} için yeni sipariş talebi`);
        }

        if (window.TumMenuOrderRequestsPage && window.TumMenuAdminOrderRequests) {
            window.TumMenuAdminOrderRequests.prependOrder(order);
        }
    }

    function connect() {
        if (!window.signalR) return;
        const connection = new signalR.HubConnectionBuilder()
            .withUrl('/hubs/order-requests')
            .withAutomaticReconnect()
            .build();

        connection.on('orderRequestCreated', notify);
        connection.start().catch(function () {
            window.setTimeout(connect, 5000);
        });
    }

    function escapeHtml(value) {
        return String(value || '').replace(/[&<>"']/g, char => ({
            '&': '&amp;',
            '<': '&lt;',
            '>': '&gt;',
            '"': '&quot;',
            "'": '&#39;'
        }[char]));
    }

    function formatMoney(value) {
        return new Intl.NumberFormat('tr-TR', { minimumFractionDigits: 2, maximumFractionDigits: 2 }).format(Number(value || 0)) + ' TL';
    }

    soundTestButton?.addEventListener('click', function (event) {
        event.preventDefault();
        event.stopPropagation();
        unlockAudio(true);
    });

    volumeInput?.addEventListener('click', function (event) {
        event.stopPropagation();
    });

    volumeInput?.addEventListener('input', function (event) {
        event.stopPropagation();
        volumeMultiplier = Math.max(1, Math.min(3, Number(volumeInput.value || 2)));
        localStorage.setItem('tummenu.orderNotificationVolume', String(volumeMultiplier));
        renderVolumeState();
    });

    volumeInput?.addEventListener('change', function (event) {
        event.stopPropagation();
        unlockAudio(true);
    });

    document.addEventListener('pointerdown', function () {
        if (soundEnabled && !audioUnlocked) {
            unlockAudio(false);
        }
    }, { once: true });

    bell?.addEventListener('click', function () {
        unseenCount = 0;
        sessionStorage.setItem('tummenu.orderNotificationCount', '0');
        renderBadge();
    });

    renderBadge();
    renderVolumeState();
    connect();
})();
