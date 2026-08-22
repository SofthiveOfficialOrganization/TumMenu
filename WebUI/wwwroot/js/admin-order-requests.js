(function () {
    const grid = document.getElementById('orderRequestsGrid');
    const modalElement = document.getElementById('orderRequestDetailModal');
    const modalBody = document.getElementById('orderRequestDetailBody');
    const modalTitle = document.getElementById('orderRequestDetailTitle');
    const modal = modalElement && window.bootstrap ? bootstrap.Modal.getOrCreateInstance(modalElement) : null;

    function money(value) {
        return new Intl.NumberFormat('tr-TR', { minimumFractionDigits: 2, maximumFractionDigits: 2 }).format(Number(value || 0)) + ' TL';
    }

    function statusText(status) {
        if (status === 'New' || status === 1) return 'Yeni';
        if (status === 'Seen' || status === 2) return 'Görüldü';
        if (status === 'Completed' || status === 3) return 'Tamamlandı';
        return String(status || '');
    }

    function statusValue(status) {
        if (status === 1) return 'New';
        if (status === 2) return 'Seen';
        if (status === 3) return 'Completed';
        return String(status || '');
    }

    function statusClass(status) {
        if (status === 'New' || status === 1) return 'bg-danger';
        if (status === 'Seen' || status === 2) return 'bg-warning text-dark';
        if (status === 'Completed' || status === 3) return 'bg-success';
        return 'bg-secondary';
    }

    function actionButtons(order) {
        const id = escapeHtml(order.id);
        const status = statusValue(order.status);
        return `
            <button type="button" class="btn btn-sm btn-outline-primary" data-order-detail="${id}" title="Detay">
                <i class="fas fa-eye me-1"></i> Detay
            </button>
            ${status === 'New' ? `
                <button type="button" class="btn btn-sm btn-outline-secondary" data-order-seen="${id}" title="Görüldü yap">
                    <i class="fas fa-check me-1"></i> Görüldü
                </button>` : ''}
            ${status !== 'Completed' ? `
                <button type="button" class="btn btn-sm btn-outline-success" data-order-complete="${id}" title="Tamamlandı yap">
                    <i class="fas fa-check-double me-1"></i> Tamamla
                </button>` : ''}
        `;
    }

    function productRows(order) {
        const items = Array.isArray(order.items) ? order.items : [];
        if (!items.length) {
            return '<div class="text-secondary small py-2">Ürün bilgisi alınamadı.</div>';
        }

        return items.map(item => {
            const size = item.productPriceSize ? escapeHtml(item.productPriceSize) : '';
            const note = item.note ? `Not: ${escapeHtml(item.note)}` : '';
            const meta = [size, note].filter(Boolean).join(' · ');
            return `
                <div class="tm-order-product-row">
                    <div class="tm-order-product-quantity">${Number(item.quantity || 0)}</div>
                    <div>
                        <div class="tm-order-product-title">${escapeHtml(item.productTitle)}</div>
                        ${meta ? `<div class="tm-order-product-meta">${meta}</div>` : ''}
                    </div>
                    <div class="tm-order-product-price">${money(item.lineTotal)}</div>
                </div>
            `;
        }).join('');
    }

    function cardHtml(order) {
        const created = order.createdAt ? new Date(order.createdAt) : new Date();
        const id = escapeHtml(order.id);
        const status = statusValue(order.status);
        const items = Array.isArray(order.items) ? order.items : [];

        return `
            <article class="tm-order-request-card" data-order-id="${id}" data-status="${escapeHtml(status)}">
                <div class="tm-order-request-top">
                    <div>
                        <h5 class="tm-order-request-title">${escapeHtml(order.storeName)}</h5>
                        <div class="tm-order-request-meta">
                            <span><i class="far fa-clock me-1"></i>${created.toLocaleTimeString('tr-TR', { hour: '2-digit', minute: '2-digit' })}</span>
                            <span><i class="fas fa-utensils me-1"></i>${Number(order.itemCount || 0)} ürün</span>
                            <span class="badge ${statusClass(order.status)}" data-order-status>${statusText(order.status)}</span>
                        </div>
                    </div>
                    <div class="tm-order-request-total">
                        <span class="text-secondary small fw-semibold">Toplam</span>
                        <strong>${money(order.subtotal)}</strong>
                    </div>
                </div>

                <div class="tm-order-request-summary">
                    <div class="tm-order-request-summary-item">
                        <span>Müşteri</span>
                        <strong>${escapeHtml(order.customerName)}</strong>
                    </div>
                    <div class="tm-order-request-summary-item">
                        <span>Masa</span>
                        <strong>${escapeHtml(order.tableNumber)}</strong>
                    </div>
                    <div class="tm-order-request-summary-item">
                        <span>Sipariş</span>
                        <strong>${items.length} satır</strong>
                    </div>
                </div>

                <div class="tm-order-products">
                    <div class="tm-order-products-heading">
                        <span>Ürünler</span>
                        <span>Tutar</span>
                    </div>
                    ${productRows(order)}
                </div>

                <div class="tm-order-request-actions">
                    ${actionButtons(order)}
                </div>
            </article>`;
    }

    function prependOrder(order) {
        if (!grid || !order?.id || grid.querySelector(`[data-order-id="${CSS.escape(order.id)}"]`)) return;
        document.getElementById('orderRequestsEmptyState')?.remove();
        grid.insertAdjacentHTML('afterbegin', cardHtml(order));
    }

    async function showDetail(id) {
        if (!modal || !modalBody || !modalTitle) return;
        modalBody.innerHTML = '<div class="text-center text-secondary py-4">Yükleniyor...</div>';
        modal.show();

        try {
            const response = await fetch(`/Admin/OrderRequests/Details/${encodeURIComponent(id)}`, { headers: { 'Accept': 'application/json' } });
            const order = await response.json();
            if (!response.ok) throw new Error(order.message || 'Detay alınamadı.');
            modalTitle.textContent = `${order.storeName} · Masa ${order.tableNumber}`;
            modalBody.innerHTML = detailHtml(order);
        } catch (error) {
            modalBody.innerHTML = `<div class="alert alert-danger">${escapeHtml(error.message || 'Detay alınamadı.')}</div>`;
        }
    }

    function detailHtml(order) {
        const items = (order.items || []).map(item => `
            <tr>
                <td>
                    <div class="fw-semibold">${escapeHtml(item.productTitle)}</div>
                    ${item.productPriceSize ? `<div class="text-secondary small">${escapeHtml(item.productPriceSize)}</div>` : ''}
                    ${item.note ? `<div class="text-secondary small">Not: ${escapeHtml(item.note)}</div>` : ''}
                </td>
                <td>${Number(item.quantity || 0)}</td>
                <td>${money(item.unitPrice)}</td>
                <td class="text-end">${money(item.lineTotal)}</td>
            </tr>
        `).join('');

        return `
            <div class="row g-3 mb-3">
                <div class="col-md-4"><div class="p-3 bg-light rounded-3"><div class="text-secondary small">Müşteri</div><strong>${escapeHtml(order.customerName)}</strong></div></div>
                <div class="col-md-4"><div class="p-3 bg-light rounded-3"><div class="text-secondary small">Masa</div><strong>${escapeHtml(order.tableNumber)}</strong></div></div>
                <div class="col-md-4"><div class="p-3 bg-light rounded-3"><div class="text-secondary small">Toplam</div><strong>${money(order.subtotal)}</strong></div></div>
            </div>
            ${order.note ? `<div class="alert alert-light border">Not: ${escapeHtml(order.note)}</div>` : ''}
            <div class="table-responsive">
                <table class="table align-middle">
                    <thead><tr><th>Ürün</th><th>Adet</th><th>Birim</th><th class="text-end">Tutar</th></tr></thead>
                    <tbody>${items}</tbody>
                </table>
            </div>`;
    }

    async function updateStatus(id, action, status) {
        try {
            const response = await fetch(`/Admin/OrderRequests/${action}/${encodeURIComponent(id)}`, {
                method: 'POST',
                headers: { 'Accept': 'application/json' }
            });
            const payload = await response.json().catch(() => ({}));
            if (!response.ok) throw new Error(payload.message || 'Durum güncellenemedi.');

            const card = grid?.querySelector(`[data-order-id="${CSS.escape(id)}"]`);
            const badge = card?.querySelector('[data-order-status]');
            if (card) {
                card.dataset.status = status;
            }
            if (badge) {
                badge.className = `badge ${statusClass(status)}`;
                badge.textContent = statusText(status);
            }
            if (status === 'Seen') {
                card?.querySelector('[data-order-seen]')?.remove();
            }
            if (status === 'Completed') {
                card?.querySelector('[data-order-seen]')?.remove();
                card?.querySelector('[data-order-complete]')?.remove();
            }
            window.TumMenuAlerts?.toast('success', 'Durum güncellendi.');
        } catch (error) {
            window.TumMenuAlerts?.toast('error', error.message || 'Durum güncellenemedi.');
        }
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

    document.addEventListener('click', function (event) {
        const detail = event.target.closest('[data-order-detail]');
        if (detail) {
            showDetail(detail.dataset.orderDetail);
            return;
        }

        const seen = event.target.closest('[data-order-seen]');
        if (seen) {
            updateStatus(seen.dataset.orderSeen, 'MarkSeen', 'Seen');
            return;
        }

        const complete = event.target.closest('[data-order-complete]');
        if (complete) {
            updateStatus(complete.dataset.orderComplete, 'Complete', 'Completed');
        }
    });

    window.TumMenuAdminOrderRequests = { prependOrder };
})();
