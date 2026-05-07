/**
 * TumMenu Standard Alerts & Toasts Library
 * Wrapper around SweetAlert2 to ensure consistent styling and behavior.
 */

const TumMenuAlerts = {
    normalizeMessage: function (message, fallback) {
        const fallbackMessage = fallback || 'Bir hata oluştu.';
        if (!message) return fallbackMessage;

        let value = String(message);

        if (/^\s*</.test(value)) {
            const doc = new DOMParser().parseFromString(value, 'text/html');
            const title = doc.querySelector('title')?.textContent;
            const heading = doc.querySelector('h1, h2, h3')?.textContent;
            const bodyText = doc.body?.textContent;
            value = title || heading || bodyText || fallbackMessage;
        }

        value = value
            .replace(/<[^>]*>/g, ' ')
            .replace(/\s+/g, ' ')
            .trim();

        if (!value) return fallbackMessage;

        return value.length > 180 ? value.slice(0, 177).trimEnd() + '...' : value;
    },

    getResponseMessage: async function (response, fallback) {
        const fallbackMessage = fallback || 'Bir hata oluştu.';
        const contentType = response.headers.get('content-type') || '';

        if (contentType.includes('application/json')) {
            const payload = await response.json().catch(() => null);
            const errors = payload?.errors
                ? Object.values(payload.errors).flat().filter(Boolean).join(' ')
                : '';

            return this.normalizeMessage(
                errors || payload?.message || payload?.title || payload?.detail,
                fallbackMessage
            );
        }

        const text = await response.text().catch(() => '');
        return this.normalizeMessage(text, fallbackMessage);
    },

    /**
     * Shows a toast notification at the top-right
     * @param {string} type - 'success', 'error', 'warning', 'info'
     * @param {string} message - The message to display
     */
    toast: function (type, message) {
        const safeMessage = this.normalizeMessage(message, '');
        if (!safeMessage) return;
        
        const Toast = Swal.mixin({
            toast: true,
            position: 'top-end',
            showConfirmButton: false,
            timer: 3000,
            timerProgressBar: true,
            didOpen: (toast) => {
                toast.addEventListener('mouseenter', Swal.stopTimer);
                toast.addEventListener('mouseleave', Swal.resumeTimer);
            }
        });

        Toast.fire({
            icon: type,
            title: safeMessage
        });
    },

    /**
     * Shows a confirmation popup
     * @param {string} title - Title of the popup (default: 'Emin misiniz?')
     * @param {string} text - Description text
     * @param {string} confirmButtonText - Text for confirm button (default: 'Evet, Sil!')
     * @returns {Promise<boolean>} - Resolves to true if confirmed, false otherwise
     */
    confirm: function (title, text, confirmButtonText) {
        return Swal.fire({
            title: title || 'Emin misiniz?',
            text: text || "Bu işlem geri alınamaz!",
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: 'var(--horizon-primary)', // Uses CSS variable
            cancelButtonColor: '#d33',
            confirmButtonText: confirmButtonText || 'Evet, Sil!',
            cancelButtonText: 'İptal',
            // Styling hooks are handled via CSS (see admin-tummenu.css)
            customClass: {
                popup: 'tummenu-swal-popup',
                title: 'tummenu-swal-title',
                content: 'tummenu-swal-content'
            }
        }).then((result) => {
            return result.isConfirmed;
        });
    },

    /**
     * standard popup for info/error/success that requires user acknowledgement
     */
    popup: function (type, title, text) {
        return Swal.fire({
            icon: type,
            title: title,
            text: text,
            confirmButtonColor: 'var(--horizon-primary)',
            customClass: {
                popup: 'tummenu-swal-popup',
                title: 'tummenu-swal-title',
                content: 'tummenu-swal-content'
            }
        });
    }
};

// Expose globally
window.TumMenuAlerts = TumMenuAlerts;
