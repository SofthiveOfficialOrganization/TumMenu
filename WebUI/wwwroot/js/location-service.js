/**
 * Merkezi Konum Servisi
 * Tüm uygulama genelindeki konum işlemlerini yönetir
 */
class LocationService {
    constructor() {
        this.isLocating = false;
        this.lastKnownPosition = null;
        this.callbacks = [];
        
        // Cache properties
        this.cacheKey = 'locationService_cache';
        this.cacheTtlMs = 2 * 60 * 1000; // 2 minutes
        
        // Load cached position on initialization
        this.loadCachedPosition();
    }

    /**
     * Cached position from sessionStorage
     */
    loadCachedPosition() {
        try {
            const cached = sessionStorage.getItem(this.cacheKey);
            if (cached) {
                const data = JSON.parse(cached);
                if (data.timestamp && (Date.now() - data.timestamp < this.cacheTtlMs)) {
                    this.lastKnownPosition = data.position;
                }
            }
        } catch (e) {
            console.warn('Failed to load cached position:', e);
        }
    }

    /**
     * Save position to sessionStorage
     */
    saveCachedPosition(position) {
        try {
            const data = {
                position: this.normalizePosition(position),
                timestamp: Date.now()
            };
            sessionStorage.setItem(this.cacheKey, JSON.stringify(data));
        } catch (e) {
            console.warn('Failed to save cached position:', e);
        }
    }

    /**
     * Normalize position object for consistent format
     */
    normalizePosition(raw) {
        return {
            coords: {
                latitude: raw.coords.latitude,
                longitude: raw.coords.longitude,
                accuracy: raw.coords.accuracy,
                altitude: raw.coords.altitude,
                altitudeAccuracy: raw.coords.altitudeAccuracy,
                heading: raw.coords.heading,
                speed: raw.coords.speed
            },
            timestamp: raw.timestamp
        };
    }

    /**
     * Get cached position if valid
     */
    getCachedPosition(maxAgeMs = this.cacheTtlMs) {
        try {
            const cached = sessionStorage.getItem(this.cacheKey);
            if (cached) {
                const data = JSON.parse(cached);
                if (data.timestamp && (Date.now() - data.timestamp < maxAgeMs)) {
                    return data.position;
                }
            }
        } catch (e) {
            console.warn('Failed to get cached position:', e);
        }
        return null;
    }

    /**
     * Konum al
     * @param {Object} options - Konum alma seçenekleri
     * @param {boolean} options.showLoading - Loading gösterilsin mi
     * @param {string} options.buttonId - Loading gösterilecek buton ID
     * @param {Function} options.onSuccess - Başarı callback
     * @param {Function} options.onError - Hata callback
     * @param {boolean} options.showErrorPopup - Hata popup'ı gösterilsin mi
     * @param {boolean} options.forceFresh - Yeni konum zorla
     * @param {number} options.maxAgeMs - Cache max yaş (ms)
     * @param {boolean} options.enableHighAccuracy - Yüksek hassasiyet
     * @returns {Promise}
     */
    async getLocation(options = {}) {
        const {
            showLoading = false,
            buttonId = null,
            onSuccess = null,
            onError = null,
            showErrorPopup = true,
            forceFresh = false,
            maxAgeMs = this.cacheTtlMs,
            enableHighAccuracy = false
        } = options;

        if (!navigator.geolocation) {
            const error = new Error('Tarayıcınız konum özelliğini desteklemiyor.');
            if (onError) onError(error);
            throw error;
        }

        // Check cache first (unless forceFresh)
        if (!forceFresh) {
            const cachedPosition = this.getCachedPosition(maxAgeMs);
            if (cachedPosition) {
                if (onSuccess) onSuccess(cachedPosition);
                return cachedPosition;
            }
        }

        // Eğer konum alma işlemi devam ediyorsa bekle
        if (this.isLocating) {
            return new Promise((resolve, reject) => {
                this.callbacks.push({ resolve, reject, options });
            });
        }

        this.isLocating = true;

        // Loading göster
        let btn = null;
        let originalContent = '';
        if (showLoading && buttonId) {
            btn = document.getElementById(buttonId);
            if (btn) {
                originalContent = btn.innerHTML;
                btn.innerHTML = '<span class="rest-spinner" style="width:14px;height:14px;border-width:2px;margin-right:5px;display:inline-block; border-top-color: var(--color-primary);"></span><span>Bulunuyor...</span>';
            }
        }

        return new Promise((resolve, reject) => {
            navigator.geolocation.getCurrentPosition(
                (position) => {
                    this.isLocating = false;
                    this.lastKnownPosition = position;

                    // Save to cache
                    this.saveCachedPosition(position);

                    // Loading'i gizle
                    if (btn) {
                        btn.innerHTML = originalContent;
                    }

                    // Callback'leri çalıştır
                    if (onSuccess) onSuccess(position);
                    
                    // Bekleyen callback'leri çalıştır
                    this.callbacks.forEach(cb => cb.options.onSuccess && cb.options.onSuccess(position));
                    this.callbacks = [];

                    resolve(position);
                },
                (error) => {
                    this.isLocating = false;

                    // Loading'i gizle
                    if (btn) {
                        btn.innerHTML = originalContent;
                    }

                    // Hata mesajı oluştur
                    let errorMessage = 'Konum alınamadı.';
                    let showPopup = showErrorPopup;

                    switch(error.code) {
                        case 1: // PERMISSION_DENIED
                            errorMessage = 'Konum erişim izni reddedildi. Tarayıcı ayarlarından izin vermeniz gerekmektedir.';
                            break;
                        case 2: // POSITION_UNAVAILABLE
                            errorMessage = 'Konum bilgisi mevcut değil. Cihazınızın konum servisini kontrol edin.';
                            break;
                        case 3: // TIMEOUT
                            errorMessage = 'Konum bulma işlemi zaman aşımına uğradı.';
                            showPopup = false; // Timeout'da popup gösterme
                            break;
                    }

                    // Fallback to cached/last known position for timeout or unavailable errors
                    let fallbackPosition = null;
                    if (error.code === 2 || error.code === 3) {
                        fallbackPosition = this.getCachedPosition(this.cacheTtlMs * 10) || this.lastKnownPosition;
                    }

                    if (fallbackPosition) {
                        // Use cached position as fallback
                        if (onSuccess) onSuccess(fallbackPosition);
                        this.callbacks.forEach(cb => cb.options.onSuccess && cb.options.onSuccess(fallbackPosition));
                        this.callbacks = [];
                        resolve(fallbackPosition);
                        return;
                    }

                    // Callback'leri çalıştır
                    if (onError) onError(error, errorMessage);
                    
                    // Bekleyen callback'leri çalıştır
                    this.callbacks.forEach(cb => cb.options.onError && cb.options.onError(error, errorMessage));
                    this.callbacks = [];

                    // Popup göster
                    if (showPopup && showErrorPopup && !this.lastKnownPosition) {
                        this.showErrorPopup(errorMessage);
                    }

                    reject(error);
                },
                {
                    timeout: enableHighAccuracy ? 10000 : 5000,  // High accuracy için daha uzun timeout
                    maximumAge: maxAgeMs,                        // Cache'den kullan
                    enableHighAccuracy: enableHighAccuracy        // Options'dan gelen değeri kullan
                }
            );
        });
    }

    /**
     * Hata popup'ı göster
     */
    showErrorPopup(message) {
        if (typeof Swal !== 'undefined') {
            Swal.fire({
                icon: 'info',
                title: 'Konum Alınamadı',
                text: message,
                timer: 4000,
                showConfirmButton: false
            });
        } else {
            alert(message);
        }
    }

    /**
     * Son bilinen konumu al
     */
    getLastKnownPosition() {
        return this.lastKnownPosition;
    }

    /**
     * Konum alma işlemini iptal et
     */
    cancel() {
        this.isLocating = false;
        this.callbacks = [];
    }
}

// Global instance oluştur
window.locationService = new LocationService();

// Debug için console'a yaz
console.log('LocationService initialized:', window.locationService);
console.log('LocationService script loaded successfully at:', new Date().toISOString());
