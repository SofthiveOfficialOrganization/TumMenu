(function (window) {
    'use strict';

    var storeMap = null;

    function initStoreLandingMap() {
        var el = document.getElementById('slStoreMap');
        if (!el || !window.L || !window.TumMenuMapBaseLayer) {
            return;
        }

        var lat = parseFloat(el.getAttribute('data-lat'));
        var lng = parseFloat(el.getAttribute('data-lng'));
        if (!Number.isFinite(lat) || !Number.isFinite(lng)) {
            return;
        }

        var title = el.getAttribute('data-title') || 'Konum';
        var zoom = 16;

        if (!storeMap) {
            storeMap = window.L.map('slStoreMap', { attributionControl: false }).setView([lat, lng], zoom);
            window.TumMenuMapBaseLayer.addTo(storeMap);

            var storeIcon = window.L.icon({
                iconUrl: '/images/pin-exact.png',
                iconSize: [32, 50],
                iconAnchor: [16, 50]
            });

            var marker = window.L.marker([lat, lng], { icon: storeIcon }).addTo(storeMap);
            marker.bindTooltip(title, {
                permanent: true,
                direction: 'top',
                offset: [0, -50],
                className: 'tm-map-store-label'
            }).openTooltip();
        } else {
            storeMap.setView([lat, lng], zoom);
        }

        function refreshSize() {
            if (storeMap) {
                storeMap.invalidateSize();
            }
        }

        window.setTimeout(refreshSize, 250);
        window.setTimeout(refreshSize, 800);

        window.addEventListener('pageshow', function (event) {
            if (event.persisted) {
                window.setTimeout(refreshSize, 300);
            }
        });

        if (window.ResizeObserver && el.parentElement) {
            var resizeObserver = new window.ResizeObserver(refreshSize);
            resizeObserver.observe(el.parentElement);
        }
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initStoreLandingMap);
    } else {
        initStoreLandingMap();
    }
})(window);
