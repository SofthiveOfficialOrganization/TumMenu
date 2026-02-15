/**
 * TumMenu Standard Alerts & Toasts Library
 * Wrapper around SweetAlert2 to ensure consistent styling and behavior.
 */

const TumMenuAlerts = {
    /**
     * Shows a toast notification at the top-right
     * @param {string} type - 'success', 'error', 'warning', 'info'
     * @param {string} message - The message to display
     */
    toast: function (type, message) {
        if (!message) return;
        
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
            title: message
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
