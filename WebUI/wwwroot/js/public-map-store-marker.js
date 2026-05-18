(function (window) {
    'use strict';

    function escapeHtml(value) {
        var div = document.createElement('div');
        div.textContent = value == null ? '' : String(value);
        return div.innerHTML;
    }

    function buildStoreUrl(store) {
        return '/' + encodeURIComponent(store.companySlug) + '/' + encodeURIComponent(store.slug) + '/magaza';
    }

    function bindStoreMarker(marker, store, options) {
        options = options || {};

        var storeTitle = store.title || store.companyName || 'Restoran';
        var storeUrl = buildStoreUrl(store);
        var tooltipOffset = options.tooltipOffset || [0, -50];

        marker.bindTooltip(escapeHtml(storeTitle), {
            permanent: true,
            direction: 'top',
            offset: tooltipOffset,
            className: 'tm-map-store-label'
        }).openTooltip();

        marker.on('click', function (event) {
            if (event && event.originalEvent) {
                window.L.DomEvent.stop(event.originalEvent);
            }

            window.location.href = storeUrl;
        });
    }

    window.TumMenuPublicMapStoreMarker = {
        bind: bindStoreMarker
    };
})(window);
