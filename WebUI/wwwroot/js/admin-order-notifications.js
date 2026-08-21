(function () {
    const badge = document.querySelector('[data-order-notification-badge]');
    const list = document.querySelector('[data-order-notification-list]');
    const bell = document.querySelector('[data-order-notification-bell]');
    let unseenCount = Number(sessionStorage.getItem('tummenu.orderNotificationCount') || '0');
    let audioUnlocked = false;

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

    function playSound() {
        if (!audioUnlocked) return;
        try {
            const AudioContext = window.AudioContext || window.webkitAudioContext;
            if (!AudioContext) return;
            const ctx = new AudioContext();
            const osc = ctx.createOscillator();
            const gain = ctx.createGain();
            osc.type = 'sine';
            osc.frequency.setValueAtTime(880, ctx.currentTime);
            gain.gain.setValueAtTime(0.001, ctx.currentTime);
            gain.gain.exponentialRampToValueAtTime(0.14, ctx.currentTime + 0.02);
            gain.gain.exponentialRampToValueAtTime(0.001, ctx.currentTime + 0.28);
            osc.connect(gain);
            gain.connect(ctx.destination);
            osc.start();
            osc.stop(ctx.currentTime + 0.3);
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

    document.addEventListener('click', function () {
        audioUnlocked = true;
    }, { once: true });

    bell?.addEventListener('click', function () {
        unseenCount = 0;
        sessionStorage.setItem('tummenu.orderNotificationCount', '0');
        renderBadge();
    });

    renderBadge();
    connect();
})();
