(function (window, $) {
    'use strict';

    function valueOf(selector) {
        return ($(selector).val() || '').toString().trim();
    }

    function setCoordinate($input, value) {
        $input.val(Number(value).toFixed(6));
    }

    function firstValidResult(results) {
        if (!Array.isArray(results)) return null;
        return results.find(function (item) {
            return item && Number.isFinite(Number(item.latitude)) && Number.isFinite(Number(item.longitude));
        }) || null;
    }

    function geocode(query) {
        if (!query) {
            return $.Deferred().resolve(null).promise();
        }

        return $.get('/api/location/geocode', { q: query, limit: 1 })
            .then(function (results) {
                return firstValidResult(results && results.data ? results.data : results);
            });
    }

    function init(options) {
        var $citySelect = $(options.citySelector);
        var $districtSelect = $(options.districtSelector);
        var $cityNameInput = $(options.cityNameSelector);
        var $districtNameInput = $(options.districtNameSelector);
        var $latInput = $(options.latitudeSelector);
        var $lngInput = $(options.longitudeSelector);
        var $overlay = $(options.overlaySelector);

        if (!$citySelect.length || !$districtSelect.length || !window.L) {
            return;
        }

        var map = null;
        var marker = null;
        var suppressDistrictChange = false;

        function hasSavedCoordinates() {
            var lat = Number(valueOf(options.latitudeSelector).replace(',', '.'));
            var lng = Number(valueOf(options.longitudeSelector).replace(',', '.'));
            return Number.isFinite(lat) && Number.isFinite(lng) && lat >= -90 && lat <= 90 && lng >= -180 && lng <= 180;
        }

        function writeCoordinates(lat, lng) {
            setCoordinate($latInput, lat);
            setCoordinate($lngInput, lng);
        }

        function ensureMarker(lat, lng) {
            if (!marker) {
                marker = window.L.marker([lat, lng], { draggable: true }).addTo(map);
                marker.on('dragend', function () {
                    var pos = marker.getLatLng();
                    writeCoordinates(pos.lat, pos.lng);
                });
            } else {
                marker.setLatLng([lat, lng]);
            }
        }

        function initMap(lat, lng, zoom, writeValue) {
            lat = Number(lat);
            lng = Number(lng);
            if (!Number.isFinite(lat) || !Number.isFinite(lng)) {
                lat = 39.0;
                lng = 35.0;
                zoom = 5;
            }

            if (!map) {
                map = window.L.map(options.mapId).setView([lat, lng], zoom || 10);
                window.TumMenuMapBaseLayer.addTo(map);
                ensureMarker(lat, lng);
            } else {
                map.setView([lat, lng], zoom || 10);
                ensureMarker(lat, lng);
            }

            if (writeValue) {
                writeCoordinates(lat, lng);
            }

            $overlay.hide();
            setTimeout(function () {
                map.invalidateSize();
            }, 250);
        }

        function centerMap(lat, lng, zoom) {
            initMap(lat, lng, zoom || 10, false);
        }

        function centerProvince(provinceId, zoom) {
            if (!provinceId) return;
            $.get('/api/location/provinces/' + provinceId)
                .done(function (province) {
                    if (province && Number.isFinite(Number(province.latitude)) && Number.isFinite(Number(province.longitude))) {
                        centerMap(province.latitude, province.longitude, zoom || 10);
                    }
                });
        }

        function centerDistrict(cityName, districtName) {
            if (!cityName || !districtName) return;

            var query = districtName.toLocaleLowerCase('tr-TR') === 'merkez'
                ? [cityName, 'Türkiye'].join(', ')
                : [districtName, cityName, 'Türkiye'].join(', ');

            geocode(query).done(function (result) {
                if (result) {
                    centerMap(result.latitude, result.longitude, 13);
                }
            });
        }

        function loadDistricts(provinceId, selectedDistrictId) {
            $districtSelect.empty().append('<option value="">İlçe Seçiniz</option>');
            $districtSelect.prop('disabled', !provinceId);

            if (!provinceId) {
                $districtSelect.empty().append('<option value="">Önce İl Seçiniz</option>').trigger('change.select2');
                return;
            }

            $.get('/api/location/districts/' + provinceId, function (districts) {
                suppressDistrictChange = true;
                districts.forEach(function (district) {
                    $districtSelect.append($('<option>', {
                        value: district.id,
                        text: district.name
                    }));
                });

                if (selectedDistrictId) {
                    $districtSelect.val(selectedDistrictId.toLowerCase());
                    var selected = $districtSelect.find('option:selected');
                    if (selected.val()) {
                        $districtNameInput.val(selected.text());
                    }
                }

                $districtSelect.trigger('change.select2');
                suppressDistrictChange = false;
            });
        }

        $citySelect.select2({ theme: 'bootstrap-5', width: '100%' });
        $districtSelect.select2({ theme: 'bootstrap-5', width: '100%' });

        $.get('/api/location/provinces', function (provinces) {
            provinces.forEach(function (province) {
                $citySelect.append($('<option>', {
                    value: province.id,
                    text: province.name
                }));
            });

            if (options.initialCityId) {
                $citySelect.val(options.initialCityId.toLowerCase()).trigger('change.select2');
                var selectedCity = $citySelect.find('option:selected');
                if (selectedCity.val()) {
                    $cityNameInput.val(selectedCity.text());
                    loadDistricts(selectedCity.val(), options.initialDistrictId || null);
                    if (!hasSavedCoordinates()) {
                        centerProvince(selectedCity.val(), 10);
                    }
                }
            }

            var initialLat = options.initialLatitude === null || typeof options.initialLatitude === 'undefined'
                ? NaN
                : Number(options.initialLatitude);
            var initialLng = options.initialLongitude === null || typeof options.initialLongitude === 'undefined'
                ? NaN
                : Number(options.initialLongitude);
            if (Number.isFinite(initialLat) && Number.isFinite(initialLng)) {
                initMap(initialLat, initialLng, 15, false);
            } else if (!options.initialCityId) {
                $overlay.show();
            }
        });

        $citySelect.on('change', function () {
            var selected = $citySelect.find('option:selected');
            var provinceId = selected.val();

            $cityNameInput.val(provinceId ? selected.text() : '');
            $districtNameInput.val('');

            if (!provinceId) {
                $latInput.val('');
                $lngInput.val('');
                loadDistricts(null, null);
                $overlay.show();
                return;
            }

            $latInput.val('');
            $lngInput.val('');
            loadDistricts(provinceId, null);
            centerProvince(provinceId, 10);
        });

        $districtSelect.on('change', function () {
            var selected = $districtSelect.find('option:selected');
            var districtId = selected.val();
            var districtName = districtId ? selected.text() : '';
            $districtNameInput.val(districtName);

            if (suppressDistrictChange || !districtName) {
                return;
            }

            $latInput.val('');
            $lngInput.val('');
            centerDistrict(valueOf(options.cityNameSelector), districtName);
        });

        window.TumMenuAddressMapGeocoder.init({
            buttonSelector: options.findAddressButtonSelector,
            feedbackSelector: options.feedbackSelector,
            cityNameSelector: options.cityNameSelector,
            districtNameSelector: options.districtNameSelector,
            neighborhoodSelector: options.neighborhoodSelector,
            fullAddressSelector: options.fullAddressSelector,
            latitudeSelector: options.latitudeSelector,
            longitudeSelector: options.longitudeSelector,
            onLocated: function (lat, lng) {
                initMap(lat, lng, 17, true);
            }
        });

        $(options.centerButtonSelector).on('click', function () {
            if (!map) return;
            var center = map.getCenter();
            initMap(center.lat, center.lng, map.getZoom(), true);
        });

        $(options.useMyLocationButtonSelector).on('click', function () {
            if (!window.locationService) return;

            window.locationService.getLocation({
                showLoading: true,
                buttonId: options.useMyLocationButtonId,
                forceFresh: true,
                enableHighAccuracy: true,
                showErrorPopup: true,
                onSuccess: function (position) {
                    initMap(position.coords.latitude, position.coords.longitude, 15, true);
                }
            }).catch(function (err) {
                console.warn('Location request failed:', err);
            });
        });
    }

    window.TumMenuAdminStoreLocationMap = {
        init: init
    };
})(window, window.jQuery);
