(function (window) {
    'use strict';

    class LocationService {
        constructor() {
            this.cacheKey = 'locationService_cache';
            this.cacheTtlMs = 2 * 60 * 1000;
            this.pendingRequest = null;
            this.lastKnownPosition = null;
            this.lastKnownPosition = this.getCachedPosition();
        }

        normalizePosition(position) {
            return {
                coords: {
                    latitude: position.coords.latitude,
                    longitude: position.coords.longitude,
                    accuracy: position.coords.accuracy,
                    altitude: position.coords.altitude,
                    altitudeAccuracy: position.coords.altitudeAccuracy,
                    heading: position.coords.heading,
                    speed: position.coords.speed
                },
                timestamp: position.timestamp || Date.now()
            };
        }

        getCachedPosition(maxAgeMs = this.cacheTtlMs) {
            try {
                var raw = window.sessionStorage.getItem(this.cacheKey);
                if (!raw) return null;

                var cached = JSON.parse(raw);
                if (!cached.timestamp || Date.now() - cached.timestamp > maxAgeMs) {
                    return null;
                }

                return cached.position || null;
            } catch (err) {
                console.warn('Location cache read failed:', err);
                return null;
            }
        }

        saveCachedPosition(position) {
            try {
                var normalized = this.normalizePosition(position);
                window.sessionStorage.setItem(this.cacheKey, JSON.stringify({
                    position: normalized,
                    timestamp: Date.now()
                }));
                this.lastKnownPosition = normalized;
                return normalized;
            } catch (err) {
                console.warn('Location cache write failed:', err);
                return this.normalizePosition(position);
            }
        }

        getPositionForPageLoad(options = {}) {
            var maxAgeMs = options.maxAgeMs || this.cacheTtlMs;
            return this.getCachedPosition(maxAgeMs);
        }

        getFreshPosition(options = {}) {
            if (!window.navigator.geolocation) {
                return Promise.reject(this.createUnsupportedError());
            }

            if (this.pendingRequest) {
                return this.pendingRequest;
            }

            var geolocationOptions = {
                enableHighAccuracy: options.enableHighAccuracy !== false,
                timeout: options.timeout || 10000,
                maximumAge: 0
            };

            this.pendingRequest = new Promise((resolve, reject) => {
                window.navigator.geolocation.getCurrentPosition(
                    (position) => {
                        var normalized = this.saveCachedPosition(position);
                        this.pendingRequest = null;
                        resolve(normalized);
                    },
                    (error) => {
                        this.pendingRequest = null;
                        reject(error);
                    },
                    geolocationOptions
                );
            });

            return this.pendingRequest;
        }

        getFreshPositionOrCachedFallback(options = {}) {
            var fallbackMaxAgeMs = options.fallbackMaxAgeMs || (10 * 60 * 1000);

            return this.getFreshPosition(options)
                .catch((error) => {
                    if (!this.canUseCachedFallback(error)) {
                        throw error;
                    }

                    var cached = this.getCachedPosition(fallbackMaxAgeMs) || this.lastKnownPosition;
                    if (!cached || !cached.coords) {
                        throw error;
                    }

                    cached.fromFallback = true;
                    cached.fallbackReason = this.getErrorMessage(error);
                    return cached;
                });
        }

        getLocation(options = {}) {
            var {
                showLoading = false,
                buttonId = null,
                onSuccess = null,
                onError = null,
                showErrorPopup = true,
                forceFresh = false,
                maxAgeMs = this.cacheTtlMs,
                enableHighAccuracy = false,
                timeout = enableHighAccuracy ? 10000 : 5000,
                allowCachedFallbackOnError = false,
                fallbackMaxAgeMs = 10 * 60 * 1000
            } = options;

            var button = showLoading && buttonId ? document.getElementById(buttonId) : null;
            var originalHtml = button ? button.innerHTML : '';
            if (button) {
                button.disabled = true;
                button.innerHTML = '<span class="rest-spinner" style="width:14px;height:14px;border-width:2px;margin-right:5px;display:inline-block;border-top-color:var(--color-primary);"></span><span>Bulunuyor...</span>';
            }

            var freshRequest = allowCachedFallbackOnError
                ? this.getFreshPositionOrCachedFallback({
                    enableHighAccuracy: enableHighAccuracy,
                    timeout: timeout,
                    fallbackMaxAgeMs: fallbackMaxAgeMs
                })
                : this.getFreshPosition({ enableHighAccuracy: enableHighAccuracy, timeout: timeout });

            var request = forceFresh
                ? freshRequest
                : Promise.resolve(this.getPositionForPageLoad({ maxAgeMs: maxAgeMs }))
                    .then((cached) => cached || freshRequest);

            return request
                .then((position) => {
                    if (typeof onSuccess === 'function') {
                        onSuccess(position);
                    }
                    return position;
                })
                .catch((error) => {
                    var message = this.getErrorMessage(error);
                    if (typeof onError === 'function') {
                        onError(error, message);
                    }
                    if (showErrorPopup) {
                        this.showErrorPopup(message);
                    }
                    throw error;
                })
                .finally(() => {
                    if (button) {
                        button.disabled = false;
                        button.innerHTML = originalHtml;
                    }
                });
        }

        getLastKnownPosition() {
            return this.lastKnownPosition;
        }

        cancel() {
            this.pendingRequest = null;
        }

        createUnsupportedError() {
            var error = new Error('Tarayıcınız konum özelliğini desteklemiyor.');
            error.code = 0;
            return error;
        }

        getErrorMessage(error) {
            if (!error) {
                return 'Konum alınamadı.';
            }

            switch (error.code) {
                case 1:
                    return 'Konum erişim izni reddedildi. Tarayıcı ayarlarından izin vermeniz gerekmektedir.';
                case 2:
                    return 'Konum bilgisi mevcut değil. Cihazınızın konum servisini kontrol edin.';
                case 3:
                    return 'Konum bulma işlemi zaman aşımına uğradı.';
                default:
                    return error.message || 'Konum alınamadı.';
            }
        }

        canUseCachedFallback(error) {
            return error && (error.code === 1 || error.code === 2 || error.code === 3);
        }

        showErrorPopup(message) {
            if (typeof window.Swal !== 'undefined') {
                window.Swal.fire({
                    icon: 'info',
                    title: 'Konum Alınamadı',
                    text: message,
                    timer: 4000,
                    showConfirmButton: false
                });
                return;
            }

            window.alert(message);
        }
    }

    window.locationService = new LocationService();
})(window);
