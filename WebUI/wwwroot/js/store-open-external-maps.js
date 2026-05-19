(function (window) {
    'use strict';

    function isAppleDevice() {
        var ua = window.navigator.userAgent || '';
        if (/iPad|iPhone|iPod/i.test(ua)) {
            return true;
        }

        return window.navigator.platform === 'MacIntel' && window.navigator.maxTouchPoints > 1;
    }

    function buildExternalMapsUrl(link) {
        var lat = link.getAttribute('data-lat');
        var lng = link.getAttribute('data-lng');
        var address = link.getAttribute('data-address');
        var label = link.getAttribute('data-label') || '';

        if (lat === '' || lat === null) lat = null;
        if (lng === '' || lng === null) lng = null;
        if (address === '' || address === null) address = null;

        if (isAppleDevice()) {
            if (lat && lng) {
                var appleQuery = encodeURIComponent(label || (lat + ',' + lng));
                return 'https://maps.apple.com/?ll=' + encodeURIComponent(lat + ',' + lng) + '&q=' + appleQuery;
            }

            if (address) {
                return 'https://maps.apple.com/?q=' + encodeURIComponent(address);
            }
        }

        if (lat && lng) {
            return 'https://www.google.com/maps/search/?api=1&query=' + encodeURIComponent(lat + ',' + lng);
        }

        if (address) {
            return 'https://www.google.com/maps/search/?api=1&query=' + encodeURIComponent(address);
        }

        return link.getAttribute('href') || '#';
    }

    function initExternalMapsLinks() {
        var links = document.querySelectorAll('.js-sl-external-maps');
        links.forEach(function (link) {
            link.href = buildExternalMapsUrl(link);
        });
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initExternalMapsLinks);
    } else {
        initExternalMapsLinks();
    }
})(window);
