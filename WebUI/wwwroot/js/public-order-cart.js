(function () {
    const config = window.TumMenuPublicOrder;
    if (!config || !config.storeId) return;

    const storageKey = `tummenu.cart.${config.storeId}`;
    let sessionValid = false;
    let sessionExpiresAt = null;

    function money(value) {
        return new Intl.NumberFormat('tr-TR', { minimumFractionDigits: 2, maximumFractionDigits: 2 }).format(Number(value || 0)) + ' TL';
    }

    function readCart() {
        try {
            return JSON.parse(localStorage.getItem(storageKey) || '[]');
        } catch (_) {
            return [];
        }
    }

    function writeCart(items) {
        localStorage.setItem(storageKey, JSON.stringify(items));
        renderCart();
    }

    async function refreshSession() {
        try {
            const response = await fetch(`/siparis/oturum?storeId=${encodeURIComponent(config.storeId)}`, {
                headers: { 'Accept': 'application/json' }
            });
            const payload = await response.json();
            sessionValid = Boolean(payload.isValid);
            sessionExpiresAt = payload.expiresAt || null;
        } catch (_) {
            sessionValid = false;
            sessionExpiresAt = null;
        }

        document.body.classList.toggle('tm-order-session-valid', sessionValid);
        document.querySelectorAll('[data-order-add]').forEach(button => {
            if ('disabled' in button) {
                button.disabled = !sessionValid;
            }
            button.classList.toggle('is-disabled', !sessionValid);
            button.setAttribute('aria-disabled', sessionValid ? 'false' : 'true');
            button.title = sessionValid ? 'Sepete ekle' : 'Sipariş için QR kodu okutun';
        });

        renderCart();
    }

    async function ensureSession() {
        await refreshSession();
        if (!sessionValid) {
            toast('warning', 'Sipariş süreniz doldu. Sepetiniz duruyor; göndermek için QR kodu tekrar okutun.');
            return false;
        }
        return true;
    }

    function toast(type, message) {
        if (window.Swal) {
            Swal.fire({
                toast: true,
                position: 'top-end',
                icon: type,
                title: message,
                showConfirmButton: false,
                timer: 2800,
                timerProgressBar: true
            });
            return;
        }

        alert(message);
    }

    function addItem(item) {
        const items = readCart();
        const key = `${item.productId}:${item.productPriceId || ''}:${item.note || ''}`;
        const existing = items.find(x => `${x.productId}:${x.productPriceId || ''}:${x.note || ''}` === key);
        if (existing) {
            existing.quantity += item.quantity;
        } else {
            items.push(item);
        }
        writeCart(items);
        toast('success', 'Ürün sepete eklendi.');
    }

    function removeItem(index) {
        const items = readCart();
        items.splice(index, 1);
        writeCart(items);
    }

    function changeQuantity(index, delta) {
        const items = readCart();
        if (!items[index]) return;
        items[index].quantity = Math.max(1, Math.min(99, items[index].quantity + delta));
        writeCart(items);
    }

    function cartTotals(items) {
        return {
            quantity: items.reduce((sum, item) => sum + Number(item.quantity || 0), 0),
            subtotal: items.reduce((sum, item) => sum + Number(item.unitPrice || 0) * Number(item.quantity || 0), 0)
        };
    }

    function ensureShell() {
        if (document.getElementById('tmOrderCartBar')) return;

        document.body.insertAdjacentHTML('beforeend', `
            <div class="tm-order-cart-bar" id="tmOrderCartBar" hidden>
                <button type="button" class="tm-order-cart-button" data-cart-open>
                    <span><strong data-cart-count>0</strong> ürün</span>
                    <span data-cart-total>0,00 TL</span>
                </button>
            </div>
            <div class="tm-order-cart-modal" id="tmOrderCartModal" hidden>
                <button type="button" class="tm-order-cart-backdrop" data-cart-close aria-label="Sepeti kapat"></button>
                <section class="tm-order-cart-dialog" role="dialog" aria-modal="true" aria-labelledby="tmOrderCartTitle">
                    <div class="tm-order-cart-head">
                        <div>
                            <p>QR Sipariş Talebi</p>
                            <h2 id="tmOrderCartTitle">Sepet</h2>
                        </div>
                        <button type="button" class="tm-order-cart-close" data-cart-close aria-label="Kapat">&times;</button>
                    </div>
                    <div class="tm-order-cart-session" data-cart-session></div>
                    <div class="tm-order-cart-items" data-cart-items></div>
                    <form class="tm-order-checkout" data-cart-form>
                        <label>Adınız<input name="customerName" maxlength="150" required autocomplete="name"></label>
                        <label>Masa No<input name="tableNumber" maxlength="40" required inputmode="text"></label>
                        <label>Sipariş Notu<textarea name="note" maxlength="1000" rows="3"></textarea></label>
                        <button type="submit" class="tm-order-submit">Sipariş Talebini Gönder</button>
                    </form>
                </section>
            </div>
        `);

        document.querySelectorAll('[data-cart-open]').forEach(el => el.addEventListener('click', openCart));
        document.querySelectorAll('[data-cart-close]').forEach(el => el.addEventListener('click', closeCart));
        document.querySelector('[data-cart-form]')?.addEventListener('submit', submitOrder);
    }

    function renderCart() {
        ensureShell();
        const items = readCart();
        const totals = cartTotals(items);
        const bar = document.getElementById('tmOrderCartBar');
        const count = document.querySelector('[data-cart-count]');
        const total = document.querySelector('[data-cart-total]');
        const container = document.querySelector('[data-cart-items]');
        const session = document.querySelector('[data-cart-session]');

        if (bar) bar.hidden = items.length === 0;
        if (count) count.textContent = String(totals.quantity);
        if (total) total.textContent = money(totals.subtotal);

        if (session) {
            session.textContent = sessionValid
                ? `QR sipariş süreniz aktif${sessionExpiresAt ? ' (' + new Date(sessionExpiresAt).toLocaleTimeString('tr-TR', { hour: '2-digit', minute: '2-digit' }) + ' kadar)' : ''}.`
                : 'Sipariş göndermek için QR kodu tekrar okutun. Sepetiniz korunur.';
            session.classList.toggle('is-valid', sessionValid);
        }

        if (!container) return;
        if (items.length === 0) {
            container.innerHTML = '<div class="tm-order-empty">Sepetiniz boş.</div>';
            return;
        }

        container.innerHTML = items.map((item, index) => `
            <div class="tm-order-cart-item">
                <div>
                    <strong>${escapeHtml(item.title)}</strong>
                    ${item.size ? `<span>${escapeHtml(item.size)}</span>` : ''}
                    ${item.note ? `<small>${escapeHtml(item.note)}</small>` : ''}
                </div>
                <div class="tm-order-cart-item-actions">
                    <button type="button" data-cart-qty="${index}" data-delta="-1">-</button>
                    <span>${item.quantity}</span>
                    <button type="button" data-cart-qty="${index}" data-delta="1">+</button>
                    <b>${money(Number(item.unitPrice) * Number(item.quantity))}</b>
                    <button type="button" data-cart-remove="${index}" aria-label="Ürünü kaldır">&times;</button>
                </div>
            </div>
        `).join('');

        container.querySelectorAll('[data-cart-remove]').forEach(button => {
            button.addEventListener('click', () => removeItem(Number(button.dataset.cartRemove)));
        });
        container.querySelectorAll('[data-cart-qty]').forEach(button => {
            button.addEventListener('click', () => changeQuantity(Number(button.dataset.cartQty), Number(button.dataset.delta)));
        });
    }

    function openCart() {
        refreshSession();
        const modal = document.getElementById('tmOrderCartModal');
        if (!modal) return;
        modal.hidden = false;
        requestAnimationFrame(() => modal.classList.add('is-open'));
    }

    function closeCart() {
        const modal = document.getElementById('tmOrderCartModal');
        if (!modal) return;
        modal.classList.remove('is-open');
        setTimeout(() => { modal.hidden = true; }, 160);
    }

    async function submitOrder(event) {
        event.preventDefault();
        const items = readCart();
        if (items.length === 0) {
            toast('warning', 'Sepetiniz boş.');
            return;
        }
        if (!(await ensureSession())) return;

        const form = event.currentTarget;
        const formData = new FormData(form);
        const payload = {
            storeId: config.storeId,
            customerName: String(formData.get('customerName') || '').trim(),
            tableNumber: String(formData.get('tableNumber') || '').trim(),
            note: String(formData.get('note') || '').trim(),
            items: items.map(item => ({
                productId: item.productId,
                productPriceId: item.productPriceId || null,
                quantity: item.quantity,
                note: item.note || null
            }))
        };

        const submitButton = form.querySelector('button[type="submit"]');
        submitButton.disabled = true;
        submitButton.textContent = 'Gönderiliyor...';

        try {
            const response = await fetch('/siparis/olustur', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json', 'Accept': 'application/json' },
                body: JSON.stringify(payload)
            });
            const result = await response.json().catch(() => ({}));
            if (!response.ok) {
                throw new Error(result.message || result.title || 'Sipariş talebi gönderilemedi.');
            }

            writeCart([]);
            form.reset();
            closeCart();
            toast('success', result.message || 'Sipariş talebiniz iletildi.');
        } catch (error) {
            toast('error', error.message || 'Sipariş talebi gönderilemedi.');
        } finally {
            submitButton.disabled = false;
            submitButton.textContent = 'Sipariş Talebini Gönder';
            refreshSession();
        }
    }

    function bindAddButtons() {
        document.querySelectorAll('[data-order-add]').forEach(button => {
            if (button.dataset.orderBound === 'true') return;
            button.dataset.orderBound = 'true';
            button.addEventListener('click', async event => {
                event.preventDefault();
                event.stopPropagation();
                if (!(await ensureSession())) return;

                const item = {
                    productId: button.dataset.productId,
                    productPriceId: button.dataset.priceId || null,
                    title: button.dataset.title,
                    size: button.dataset.size || null,
                    unitPrice: Number(button.dataset.price || 0),
                    quantity: Number(button.dataset.quantity || 1),
                    note: button.dataset.note || null
                };

                addItem(item);
            });
        });

        document.querySelector('[data-order-product-form]')?.addEventListener('submit', async event => {
            event.preventDefault();
            if (!(await ensureSession())) return;
            const form = event.currentTarget;
            const formData = new FormData(form);
            const priceSelect = form.querySelector('[name="priceOption"]');
            const priceOption = priceSelect?.selectedOptions?.[0];
            const quantity = Math.max(1, Math.min(99, Number(formData.get('quantity') || 1)));
            addItem({
                productId: String(formData.get('productId') || ''),
                productPriceId: priceOption?.value || '',
                title: String(formData.get('productTitle') || ''),
                size: priceOption?.dataset.size || '',
                unitPrice: Number(priceOption?.dataset.price || formData.get('basePrice') || 0),
                quantity,
                note: String(formData.get('note') || '').trim() || null
            });
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

    ensureShell();
    bindAddButtons();
    renderCart();
    refreshSession();
    setInterval(refreshSession, 60000);
})();
