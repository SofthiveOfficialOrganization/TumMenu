(function () {
    const badge = document.querySelector('[data-order-notification-badge]');
    const list = document.querySelector('[data-order-notification-list]');
    const bell = document.querySelector('[data-order-notification-bell]');
    const soundButton = document.querySelector('[data-order-notification-sound]');
    const soundText = document.querySelector('[data-order-notification-sound-text]');
    let unseenCount = Number(sessionStorage.getItem('tummenu.orderNotificationCount') || '0');
    let audioContext = null;
    let audioUnlocked = false;
    let soundEnabled = localStorage.getItem('tummenu.orderNotificationSound') === 'enabled';

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

    function renderSoundState() {
        if (!soundButton || !soundText) return;
        soundButton.classList.toggle('is-enabled', audioUnlocked);
        soundText.textContent = audioUnlocked ? 'Ses açık' : 'Sesi aç';
        soundButton.title = audioUnlocked
            ? 'Yeni siparişlerde ses çalacak'
            : 'Tarayıcı sesini etkinleştir';
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
                localStorage.setItem('tummenu.orderNotificationSound', 'enabled');
                if (playTestTone) playSound();
            }
        } catch (_) {
            audioUnlocked = false;
        }

        renderSoundState();
        return audioUnlocked;
    }

    function playSound() {
        if (!soundEnabled || !audioUnlocked) return;
        try {
            const ctx = getAudioContext();
            if (!ctx || ctx.state !== 'running') return;
            const master = ctx.createGain();
            master.gain.setValueAtTime(0.55, ctx.currentTime);
            master.connect(ctx.destination);

            [
                { start: 0, frequency: 880, duration: 0.22 },
                { start: 0.28, frequency: 1175, duration: 0.28 },
                { start: 0.68, frequency: 988, duration: 0.38 }
            ].forEach(tone => {
                const osc = ctx.createOscillator();
                const gain = ctx.createGain();
                const start = ctx.currentTime + tone.start;
                const end = start + tone.duration;

                osc.type = 'square';
                osc.frequency.setValueAtTime(tone.frequency, start);
                gain.gain.setValueAtTime(0.001, start);
                gain.gain.exponentialRampToValueAtTime(0.22, start + 0.025);
                gain.gain.exponentialRampToValueAtTime(0.001, end);

                osc.connect(gain);
                gain.connect(master);
                osc.start(start);
                osc.stop(end + 0.02);
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

    soundButton?.addEventListener('click', function (event) {
        event.preventDefault();
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
    renderSoundState();
    connect();
})();
