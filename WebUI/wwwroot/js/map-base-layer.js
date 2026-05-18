(function (window) {
    'use strict';

    var DEFAULT_STYLE_URL = 'https://tiles.openfreemap.org/styles/liberty';
    var DEFAULT_TILE_URL = 'https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png';
    var DEFAULT_ATTRIBUTION = '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors';

    function ensureDependencies() {
        if (!window.L) {
            throw new Error('Leaflet is required before map-base-layer.js');
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

        if (!map._tumMenuMapAttributionApplied) {
            map._tumMenuMapAttributionApplied = true;
        }
    }

    function createRasterLayer(options) {
        ensureDependencies();

        options = options || {};

        return window.L.tileLayer(options.tileUrl || DEFAULT_TILE_URL, {
            attribution: options.attribution || DEFAULT_ATTRIBUTION,
            maxZoom: options.maxZoom || 19
        });
    }

    function createVectorLayer(options) {
        ensureDependencies();

        if (typeof window.L.maplibreGL !== 'function') {
            throw new Error('MapLibre GL Leaflet is required to use the vector map base layer');
        }

        options = options || {};

        return window.L.maplibreGL({
            style: options.styleUrl || DEFAULT_STYLE_URL
        });
    }

    function createLayer(options) {
        options = options || {};

        if (options.useVector === true) {
            return createVectorLayer(options);
        }

        return createRasterLayer(options);
    }

    function addTo(map, options) {
        if (!map) {
            return null;
        }

        if (map._tumMenuBaseLayer) {
            return map._tumMenuBaseLayer;
        }

        ensureAttributionControl(map);

        map._tumMenuBaseLayer = createLayer(options).addTo(map);
        return map._tumMenuBaseLayer;
    }

    window.TumMenuMapBaseLayer = {
        DEFAULT_STYLE_URL: DEFAULT_STYLE_URL,
        DEFAULT_TILE_URL: DEFAULT_TILE_URL,
        addTo: addTo,
        createLayer: createLayer,
        createRasterLayer: createRasterLayer,
        createVectorLayer: createVectorLayer,
        ensureAttributionControl: ensureAttributionControl
    };
})(window);
