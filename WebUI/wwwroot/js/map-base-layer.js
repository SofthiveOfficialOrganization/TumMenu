(function (window) {
    'use strict';

    var DEFAULT_STYLE_URL = 'https://tiles.openfreemap.org/styles/liberty';
    function ensureDependencies() {
        if (!window.L) {
            throw new Error('Leaflet is required before map-base-layer.js');
        }

        if (typeof window.L.maplibreGL !== 'function') {
            throw new Error('MapLibre GL Leaflet is required before map-base-layer.js');
        }
    }

    function ensureAttributionControl(map) {
        if (!map) {
            return;
        }

        if (!map.attributionControl) {
            map.attributionControl = window.L.control.attribution({
                position: 'bottomleft',
                prefix: false
            }).addTo(map);
        }

        if (!map._openFreeMapAttributionApplied) {
            map._openFreeMapAttributionApplied = true;
        }
    }

    function createLayer(options) {
        ensureDependencies();

        options = options || {};

        return window.L.maplibreGL({
            style: options.styleUrl || DEFAULT_STYLE_URL
        });
    }

    function addTo(map, options) {
        if (!map) {
            return null;
        }

        if (map._openFreeMapBaseLayer) {
            return map._openFreeMapBaseLayer;
        }

        ensureAttributionControl(map);

        map._openFreeMapBaseLayer = createLayer(options).addTo(map);
        return map._openFreeMapBaseLayer;
    }

    window.TumMenuMapBaseLayer = {
        DEFAULT_STYLE_URL: DEFAULT_STYLE_URL,
        addTo: addTo,
        createLayer: createLayer,
        ensureAttributionControl: ensureAttributionControl
    };
})(window);
