/**
 * restaurants.js – Restoran Arama & Filtre
 */
(function () {
    'use strict';

    // ── DOM refs ──
    const searchForm = document.getElementById('searchForm');
    const searchInput = document.getElementById('searchInput');
    const resultGrid = document.getElementById('resultGrid');
    const resultCount = document.getElementById('resultCount');
    const loadingEl = document.getElementById('loadingIndicator');
    const emptyEl = document.getElementById('emptyState');
    const loadMoreWrap = document.getElementById('loadMoreWrap');
    const loadMoreBtn = document.getElementById('loadMoreBtn');
    const clearBtn = document.getElementById('clearFilters');
    const veganToggle = document.getElementById('veganToggle');
    const cityInput = document.getElementById('cityInput');
    const districtInput = document.getElementById('districtInput');
    const useMyLocBtn = document.getElementById('useMyLocation');
    const filterPanel = document.getElementById('filterPanel');
    const filterToggle = document.getElementById('filterToggle');
    const locReqEl = document.getElementById('locationRequiredState');

    // ── Map State ──
    let restMap = null;
    let restMarker = null; // User position marker
    let storeLayerGroup = null; // Layer for restaurant markers

    // ── State ──
    let state = {
        searchTerm: '',
        categoryIds: [],
        isVegan: false,
        maxDistanceKm: 1, // Default 1km
        cityId: '',
        cityName: '',
        districtId: '',
        districtName: '',
        userLat: null,
        userLng: null,
        page: 0,
        pageSize: 12,
        loading: false,
        hasNext: false
    };

    // ── Init ──
    function init() {
        initCategorySelect2();
        readURLParams();
        bindEvents();

        if (state.cityId || state.districtId || state.userLat) {
            doSearch(false);
        } else {
            initRestMap(39.0, 35.0, 5, false);
            requestUserLocation(false);
        }
    }

    function initCategorySelect2() {
        if ($('#categoryInput').length) {
            $('#categoryInput').select2({
                theme: 'bootstrap-5',
                placeholder: 'Kategori Ara...',
                allowClear: true,
                ajax: {
                    url: '/api/categories/search',
                    dataType: 'json',
                    delay: 250,
                    data: function (params) {
                        return {
                            search: params.term || '',
                            page: params.page || 1,
                            pageSize: 15
                        };
                    },
                    processResults: function (data, params) {
                        params.page = params.page || 1;
                        return {
                            results: data.results,
                            pagination: {
                                more: data.pagination.more
                            }
                        };
                    },
                    cache: true
                },
                language: {
                    noResults: function () { return 'Sonuç bulunamadı'; },
                    searching: function () { return 'Aranıyor...'; },
                    loadingMore: function () { return 'Daha fazla yükleniyor...'; }
                }
            }).on('change', function() {
                renderSelectedCategories();
            });

            // Initial render
            renderSelectedCategories();
        }
    }

    function renderSelectedCategories() {
        const $select = $('#categoryInput');
        const $container = $('#selectedCategories');
        if (!$container.length) return;

        $container.empty();
        const selectedData = $select.select2('data');

        selectedData.forEach(item => {
            const $chip = $(`
                <div class="rest-selected-item" data-id="${item.id}">
                    <span>${item.text}</span>
                    <button type="button" class="rest-selected-item__remove" aria-label="Kaldır">
                        <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3">
                            <path d="M18 6 6 18M6 6l12 12"/>
                        </svg>
                    </button>
                </div>
            `);

            $chip.find('.rest-selected-item__remove').on('click', function() {
                const id = $(this).parent().data('id');
                const currentVals = $select.val() || [];
                const newVals = currentVals.filter(v => v !== String(id));
                $select.val(newVals).trigger('change');
            });

            $container.append($chip);
        });
    }

    // ── Map Init ──
    function initRestMap(lat, lng, zoom = 10, addMarker = true) {
        if (!restMap) {
            restMap = L.map('restMap', { attributionControl: false }).setView([lat, lng], zoom);
            L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
                maxZoom: 19
            }).addTo(restMap);

            storeLayerGroup = L.layerGroup().addTo(restMap);

            if (addMarker) {
                restMarker = L.marker([lat, lng], { draggable: true }).addTo(restMap);
                bindMarkerDrag();
            }

            restMap.on('click', function(e) {
                const pos = e.latlng;
                if (!restMarker) {
                    restMarker = L.marker([pos.lat, pos.lng], { draggable: true }).addTo(restMap);
                    bindMarkerDrag();
                } else {
                    restMarker.setLatLng(pos);
                }
                updateLocationFromMarker(pos.lat, pos.lng);
            });

            setTimeout(function() {
                restMap.invalidateSize();
            }, 250);
        } else {
            restMap.setView([lat, lng], zoom);
            if (addMarker) {
                if (!restMarker) {
                    restMarker = L.marker([lat, lng], { draggable: true }).addTo(restMap);
                    bindMarkerDrag();
                } else {
                    restMarker.setLatLng([lat, lng]);
                }
            } else {
                if (restMarker) {
                    restMap.removeLayer(restMarker);
                    restMarker = null;
                }
            }
            setTimeout(function() {
                restMap.invalidateSize();
            }, 250);
        }
    }

    function bindMarkerDrag() {
        if (!restMarker) return;
        restMarker.off('dragend');
        restMarker.on('dragend', function (e) {
            const pos = restMarker.getLatLng();
            updateLocationFromMarker(pos.lat, pos.lng);
        });
    }

    function updateLocationFromMarker(lat, lng) {
        state.userLat = lat;
        state.userLng = lng;
        
        cityInput.value = '';
        districtInput.value = '';
        state.cityId = '';
        state.cityName = '';
        state.districtId = '';
        state.districtName = '';
        districtInput.disabled = true;
        districtInput.innerHTML = '<option value="">Önce İl Seçiniz</option>';

        state.page = 0;
        doSearch(false);
    }

    function readURLParams() {
        const params = new URLSearchParams(window.location.search);

        if (params.has('search')) state.searchTerm = params.get('search');
        if (params.has('cityId')) state.cityId = params.get('cityId');
        if (params.has('cityName')) state.cityName = params.get('cityName');
        if (params.has('districtId')) state.districtId = params.get('districtId');
        if (params.has('districtName')) state.districtName = params.get('districtName');
        if (params.has('vegan') && params.get('vegan') === 'true') state.isVegan = true;
        if (params.has('distance')) state.maxDistanceKm = parseFloat(params.get('distance')) || null;

        if (params.has('categories')) {
            state.categoryIds = params.get('categories').split(',').filter(Boolean);
        }

        // Reflect state in UI
        if (state.searchTerm) searchInput.value = state.searchTerm;
        if (state.cityId) cityInput.value = state.cityId;
        if (state.districtId) districtInput.value = state.districtId;
        if (state.isVegan) veganToggle.checked = true;

        // Set active category Select2
        if (state.categoryIds.length > 0) {
            $('#categoryInput').val(state.categoryIds).trigger('change.select2');
        }

        // Highlight active distance
        if (state.maxDistanceKm) {
            var distBtn = document.querySelector('[data-distance="' + state.maxDistanceKm + '"]');
            if (distBtn) {
                distBtn.classList.add('is-active');
            } else {
                // If no button matches, it might be a custom value
                var customInput = document.getElementById('customDistanceInput');
                if (customInput) customInput.value = state.maxDistanceKm;
            }
        } else {
            // Ensure no button is active if distance is null
            document.querySelectorAll('.rest-dist-btn').forEach(function (b) { b.classList.remove('is-active'); });
        }
    }

    // ── Events ──
    function bindEvents() {
        // Search form
        searchForm.addEventListener('submit', function (e) {
            e.preventDefault();
            state.searchTerm = searchInput.value.trim();
            state.page = 0;
            doSearch(false);
        });

        // Category Select2
        $('#categoryInput').on('change', function () {
            var selected = $(this).val() || [];
            state.categoryIds = selected;
            state.page = 0;
            doSearch(false);
        });

        // Vegan toggle
        veganToggle.addEventListener('change', function () {
            state.isVegan = veganToggle.checked;
            state.page = 0;
            doSearch(false);
        });

        // Distance buttons
        document.getElementById('distanceFilters').addEventListener('click', function (e) {
            var btn = e.target.closest('.rest-dist-btn');
            if (!btn) return;
            var dist = parseFloat(btn.dataset.distance);

            // toggle
            var allDist = document.querySelectorAll('.rest-dist-btn');
            var wasActive = btn.classList.contains('is-active');
            allDist.forEach(function (b) { b.classList.remove('is-active'); });
            
            // Clear custom input when using presets
            var customInput = document.getElementById('customDistanceInput');
            if (customInput) customInput.value = '';

            if (wasActive) {
                state.maxDistanceKm = null;
            } else {
                btn.classList.add('is-active');
                state.maxDistanceKm = dist;
            }

            state.page = 0;
            doSearch(false);
        });

        // Custom Distance Apply
        var applyDistBtn = document.getElementById('applyCustomDistance');
        var customDistInput = document.getElementById('customDistanceInput');
        if (applyDistBtn && customDistInput) {
            applyDistBtn.addEventListener('click', function () {
                var val = parseFloat(customDistInput.value);
                
                // Remove active class from preset buttons
                document.querySelectorAll('.rest-dist-btn').forEach(function (b) { 
                    b.classList.remove('is-active'); 
                });

                if (isNaN(val) || val <= 0) {
                    state.maxDistanceKm = null;
                    customDistInput.value = '';
                } else {
                    state.maxDistanceKm = val;
                }

                state.page = 0;
                doSearch(false);
            });

            // Also search on 'Enter'
            customDistInput.addEventListener('keypress', function (e) {
                if (e.key === 'Enter') {
                    applyDistBtn.click();
                }
            });
        }

        // Load Provinces
        fetch('/api/location/provinces')
            .then(res => res.json())
            .then(data => {
                data.forEach(p => {
                    var opt = document.createElement('option');
                    opt.value = p.id; 
                    opt.textContent = p.name;
                    opt.dataset.name = p.name;
                    if(state.cityId === p.id || state.cityName === p.name) {
                        opt.selected = true;
                        state.cityId = p.id;
                        state.cityName = p.name;
                    }
                    cityInput.appendChild(opt);
                });
                if(state.cityId) {
                    loadDistricts(state.cityId, state.districtName);
                }
            });

        function loadDistricts(provinceId, selectedDistrictName = '') {
            districtInput.innerHTML = '<option value="">İlçe Seçiniz</option>';
            districtInput.disabled = false;
            fetch('/api/location/districts/' + provinceId)
                .then(res => res.json())
                .then(data => {
                    data.forEach(d => {
                        var opt = document.createElement('option');
                        opt.value = d.id; 
                        opt.textContent = d.name;
                        opt.dataset.name = d.name;
                        if(selectedDistrictName === d.name || state.districtId === d.id) {
                            opt.selected = true;
                            state.districtId = d.id;
                            state.districtName = d.name;
                        }
                        districtInput.appendChild(opt);
                    });
                });
        }

        cityInput.addEventListener('change', function() {
            var selectedOpt = cityInput.options[cityInput.selectedIndex];
            state.cityId = selectedOpt.value;
            state.cityName = selectedOpt.dataset.name || '';
            state.districtId = '';
            state.districtName = '';
            
            if(state.cityId) {
                loadDistricts(state.cityId);

                // Locate Province Center via Nominatim
                var url = `https://nominatim.openstreetmap.org/search?format=json&state=${encodeURIComponent(state.cityName)}&country=Türkiye&limit=1`;
                fetch(url).then(res => res.json()).then(data => {
                    if (data && data.length > 0) {
                        state.userLat = parseFloat(data[0].lat);
                        state.userLng = parseFloat(data[0].lon);
                        initRestMap(state.userLat, state.userLng, 10);
                        state.page = 0;
                        doSearch(false);
                    }
                });

            } else {
                districtInput.disabled = true;
                districtInput.innerHTML = '<option value="">Önce İl Seçiniz</option>';
                state.page = 0;
                
                if (restMap) {
                    restMap.setView([39.0, 35.0], 5);
                    if (restMarker) {
                        restMap.removeLayer(restMarker);
                        restMarker = null;
                    }
                }
                showLocationRequiredState();
            }
        });

        districtInput.addEventListener('change', function() {
            var selectedOpt = districtInput.options[districtInput.selectedIndex];
            state.districtId = selectedOpt.value;
            state.districtName = selectedOpt.dataset.name || '';
            
            console.log('--- DISTRICT SEÇİLDİ ---');
            console.log('Sectiginiz İlçe:', state.districtName, 'ID:', state.districtId);

            if (state.cityId && state.districtId) {
                var url = '';
                if (state.districtName.toLowerCase() === 'merkez') {
                    url = `https://nominatim.openstreetmap.org/search?format=json&city=${encodeURIComponent(state.cityName)}&state=${encodeURIComponent(state.cityName)}&country=Türkiye&limit=1`;
                } else {
                    url = `https://nominatim.openstreetmap.org/search?format=json&county=${encodeURIComponent(state.districtName)}&state=${encodeURIComponent(state.cityName)}&country=Türkiye&limit=1`;
                }
                
                console.log('Nominatim Sorgusu (Harita için):', url);

                fetch(url).then(res => res.json()).then(data => {
                    console.log('Nominatim Cevabı:', data);
                    if (data && data.length > 0) {
                        state.userLat = parseFloat(data[0].lat);
                        state.userLng = parseFloat(data[0].lon);
                        console.log('Haritaya gönderilen koordinat:', state.userLat, state.userLng);
                        initRestMap(state.userLat, state.userLng, 13);
                        state.page = 0;
                        doSearch(false);
                    } else {
                        console.warn('Nominatim koordinat bulamadı!');
                        state.page = 0;
                        doSearch(false);
                   }
                }).catch(err => {
                    console.error('Nominatim Hatası:', err);
                    state.page = 0;
                    doSearch(false);
                });
            } else {
                 if (!state.districtId && state.cityId) {
                    // Reset back to city level if district is cleared
                    var url = `https://nominatim.openstreetmap.org/search?format=json&state=${encodeURIComponent(state.cityName)}&country=Türkiye&limit=1`;
                    fetch(url).then(res => res.json()).then(data => {
                        if (data && data.length > 0) {
                            state.userLat = parseFloat(data[0].lat);
                            state.userLng = parseFloat(data[0].lon);
                            initRestMap(state.userLat, state.userLng, 10);
                            state.page = 0;
                            doSearch(false);
                        }
                    });
                 } else {
                    state.page = 0;
                    if (restMap) {
                        restMap.setView([39.0, 35.0], 5);
                        if (restMarker) {
                            restMap.removeLayer(restMarker);
                            restMarker = null;
                        }
                    }
                    showLocationRequiredState();
                 }
            }
        });

        // Use my location
        useMyLocBtn.addEventListener('click', function () {
            requestUserLocation(true);
        });

        // Use my location (Map overlay button)
        var useMapLocBtn = document.getElementById('useMapLocationBtn');
        if (useMapLocBtn) {
            useMapLocBtn.addEventListener('click', function () {
                // Konum izni isterken butonun içeriğini değiştir
                var originalHtml = useMapLocBtn.innerHTML;
                useMapLocBtn.innerHTML = '<span class="rest-spinner" style="width:14px;height:14px;border-width:2px;margin-right:5px;display:inline-block; border-top-color: var(--color-primary);"></span><span>Konum Bulunuyor...</span>';
                
                // Başarılı veya başarısız olduğunda butonu eski haline getirmek için küçük bir callback mantığı
                requestUserLocation(true, function() {
                    useMapLocBtn.innerHTML = originalHtml;
                });
            });
        }

        // Clear all
        clearBtn.addEventListener('click', function () {
            searchInput.value = '';
            cityInput.value = '';
            districtInput.value = '';
            veganToggle.checked = false;
            $('#categoryInput').val(null).trigger('change.select2');
            document.querySelectorAll('.rest-dist-btn.is-active').forEach(function (b) { b.classList.remove('is-active'); });
            var customDistInput = document.getElementById('customDistanceInput');
            if (customDistInput) customDistInput.value = '';

            state.searchTerm = '';
            state.categoryIds = [];
            state.isVegan = false;
            state.maxDistanceKm = null;
            state.cityId = '';
            state.cityName = '';
            state.districtId = '';
            state.districtName = '';
            state.userLat = null;
            state.userLng = null;
            state.page = 0;
            
            if (restMap) {
               restMap.setView([39.0, 35.0], 5); // back to whole country
               if(restMarker) {
                   restMap.removeLayer(restMarker); // Hide marker until picked
                   restMarker = null;
               }
            }

            resultGrid.innerHTML = '';
            resultCount.textContent = '';
            loadMoreWrap.hidden = true;
            emptyEl.hidden = true;
            showLocationRequiredState();
        });

        // Load more
        loadMoreBtn.addEventListener('click', function () {
            state.page++;
            doSearch(true);
        });

        // Mobile filter toggle
        filterToggle.addEventListener('click', function () {
            filterPanel.classList.toggle('is-open');
        });
    }

    function showLocationRequiredState() {
        loadingEl.hidden = true;
        emptyEl.hidden = true;
        resultGrid.innerHTML = '';
        resultCount.innerHTML = '';
        loadMoreWrap.hidden = true;
        if (locReqEl) locReqEl.hidden = false;
    }

    // ── Geolocation ──
    function requestUserLocation(forcePrompt, callback) {
        if (!navigator.geolocation) {
             if (!forcePrompt && !state.cityId && !state.userLat) showLocationRequiredState();
             if (callback) callback();
             return;
        }

        // Try to get location silently if no city/district set
        if (forcePrompt || (!state.cityId && !state.districtId)) {
            
            if (forcePrompt) {
                useMyLocBtn.innerHTML = '<span class="rest-spinner" style="width:14px;height:14px;border-width:2px;margin-right:5px;display:inline-block;"></span> Konum Bulunuyor...';
            }

            navigator.geolocation.getCurrentPosition(
                function (pos) {
                    state.userLat = pos.coords.latitude;
                    state.userLng = pos.coords.longitude;
                    if (forcePrompt) {
                        useMyLocBtn.innerHTML = '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" width="16" height="16"><circle cx="12" cy="12" r="3"/><path d="M12 2v4m0 12v4m10-10h-4M6 12H2"/></svg> Konum Bulundu';
                        cityInput.value = '';
                        districtInput.value = '';
                        state.cityId = '';
                        state.cityName = '';
                        state.districtId = '';
                        state.districtName = '';
                        districtInput.disabled = true;
                        districtInput.innerHTML = '<option value="">Önce İl Seçiniz</option>';
                        
                        initRestMap(state.userLat, state.userLng, 13);
                        state.page = 0;
                        doSearch(false);
                    } else if (state.page === 0) {
                        // Silent check finished and we are still on first page, search again to sort by distance
                        initRestMap(state.userLat, state.userLng, 13);
                        doSearch(false);
                    }
                    if (callback) callback();
                },
                function () { 
                    if (forcePrompt) {
                        useMyLocBtn.innerHTML = '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" width="16" height="16"><circle cx="12" cy="12" r="3"/><path d="M12 2v4m0 12v4m10-10h-4M6 12H2"/></svg> Konum İzni Reddedildi';
                    }
                    if (!forcePrompt && !state.cityId && !state.userLat) {
                        showLocationRequiredState();
                    }
                    if (callback) callback();
                 },
                { timeout: 5000, maximumAge: 300000 }
            );
        } else {
             if (callback) callback();
        }
    }

    // ── API Call ──
    function doSearch(append) {
        if (!state.userLat && !state.cityId) {
            showLocationRequiredState();
            return;
        }

        if (state.loading) return;
        state.loading = true;

        loadingEl.hidden = false;
        emptyEl.hidden = true;
        if (locReqEl) locReqEl.hidden = true;
        if (!append) {
            resultGrid.innerHTML = '';
            loadMoreWrap.hidden = true;
        }

        var params = new URLSearchParams();
        if (state.searchTerm) params.set('SearchTerm', state.searchTerm);
        if (state.categoryIds.length) params.set('CategoryLibraryItemIds', state.categoryIds.join(','));
        if (state.isVegan) params.set('IsVegan', 'true');
        if (state.maxDistanceKm != null) params.set('MaxDistanceKm', state.maxDistanceKm);
        if (state.userLat != null) params.set('UserLatitude', state.userLat);
        if (state.userLng != null) params.set('UserLongitude', state.userLng);
        params.set('Page', state.page);
        params.set('PageSize', state.pageSize);

        var searchUrl = '/api/stores/search?' + params.toString();
        console.log('>>> Backend API Arama Gönderiliyor:', searchUrl);

        fetch(searchUrl)
            .then(function (res) { return res.json(); })
            .then(function (data) {
                console.log('<<< Backend API Cevabı:', data);
                if(data && data.items) {
                    data.items.forEach(function(item) {
                        console.log('  Restoran:', item.title, '| Uzaklık:', item.distanceKm, 'km');
                    });
                }
                state.loading = false;
                loadingEl.hidden = true;

                if (data.items.length === 0 && !append) {
                    emptyEl.hidden = false;
                } else {
                    emptyEl.hidden = true;
                }

                resultCount.innerHTML = '<strong>' + data.totalCount + '</strong> restoran bulundu';

                updateMapMarkers(data.items);

                data.items.forEach(function (store) {
                    resultGrid.appendChild(createStoreCard(store));
                });

                state.hasNext = data.hasNext;
                loadMoreWrap.hidden = !data.hasNext;
            })
            .catch(function (err) {
                console.error('Search error:', err);
                state.loading = false;
                loadingEl.hidden = true;

                if (!append) {
                    emptyEl.hidden = false;
                    resultGrid.innerHTML = '';
                }
            });
    }

    // ── Map Markers ──
    function updateMapMarkers(stores) {
        if (!storeLayerGroup) return;
        storeLayerGroup.clearLayers();

        const customIcon = L.icon({
            iconUrl: '/images/pin-exact.png',
            iconSize: [32, 50], // Adjusted for 539x841 aspect ratio
            iconAnchor: [16, 50] // Bottom center
        });

        stores.forEach(store => {
            if (store.latitude && store.longitude) {
                const marker = L.marker([store.latitude, store.longitude], { icon: customIcon })
                    .addTo(storeLayerGroup);

                // Hover tooltip
                marker.bindTooltip(store.title, {
                    permanent: false,
                    direction: 'top',
                    offset: [0, -50]
                });

                // Click to navigate
                marker.on('click', function() {
                    window.location.href = '/' + store.companySlug + '/' + store.slug;
                });
            }
        });
    }



    // ── Card Builder ──
    function createStoreCard(store) {
        var wrapper = document.createElement('div');
        wrapper.className = 'store-card-wrapper';

        var card = document.createElement('a');
        card.className = 'store-card';
        card.href = '/' + store.companySlug + '/' + store.slug;

        // Image
        var imgWrap = document.createElement('div');
        imgWrap.className = 'store-card__img-wrap';

        if (store.imageUrl) {
            var img = document.createElement('img');
            img.className = 'store-card__img';
            img.src = store.imageUrl;
            img.alt = store.title;
            img.loading = 'lazy';
            imgWrap.appendChild(img);
        } else {
            var noImg = document.createElement('div');
            noImg.className = 'store-card__no-img';
            noImg.textContent = '🍽️';
            imgWrap.appendChild(noImg);
        }

        // Distance badge
        if (store.distanceKm != null) {
            var distBadge = document.createElement('span');
            distBadge.className = 'store-card__distance';
            var distLabel;
            if (store.distanceKm < 1) {
                // Katları 10 olan, 10m precision ile göstermek (örn. 0, 10, 100, 110 vb)
                var meters = store.distanceKm * 1000;
                var roundedMeters = Math.round(meters / 10) * 10;
                distLabel = roundedMeters + ' m';
            } else {
                distLabel = store.distanceKm + ' km';
            }
            distBadge.innerHTML = '📍 ' + distLabel;
            imgWrap.appendChild(distBadge);
        }

        card.appendChild(imgWrap);

        // Body
        var body = document.createElement('div');
        body.className = 'store-card__body';

        var name = document.createElement('h3');
        name.className = 'store-card__name';
        name.textContent = store.title;
        body.appendChild(name);

        // Location
        var location = store.district && store.city
            ? store.district + ', ' + store.city
            : (store.city || store.district || '');

        if (location) {
            var locP = document.createElement('p');
            locP.className = 'store-card__location';
            locP.innerHTML = '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" width="14" height="14">' +
                '<path d="M21 10c0 6-9 13-9 13s-9-7-9-13a9 9 0 0118 0z"/><circle cx="12" cy="10" r="3"/></svg> ' +
                escapeHtml(location);
            body.appendChild(locP);
        }

        card.appendChild(body);
        wrapper.appendChild(card);

        // Matched product
        if (store.matchedProduct) {
            var mp = store.matchedProduct;
            var mpCard = document.createElement('div');
            mpCard.className = 'matched-product';

            if (mp.imageUrl) {
                var mpImg = document.createElement('img');
                mpImg.className = 'matched-product__img';
                mpImg.src = mp.imageUrl;
                mpImg.alt = mp.title;
                mpImg.loading = 'lazy';
                mpCard.appendChild(mpImg);
            }

            var mpInfo = document.createElement('div');
            mpInfo.className = 'matched-product__info';

            var mpBadge = document.createElement('p');
            mpBadge.className = 'matched-product__badge';
            mpBadge.textContent = 'Eşleşen ürün';
            mpInfo.appendChild(mpBadge);

            var mpName = document.createElement('p');
            mpName.className = 'matched-product__name';
            mpName.textContent = mp.title;
            mpInfo.appendChild(mpName);

            var mpPrice = document.createElement('p');
            mpPrice.className = 'matched-product__price';
            mpPrice.textContent = formatPrice(mp.basePrice);
            mpInfo.appendChild(mpPrice);

            mpCard.appendChild(mpInfo);
            wrapper.appendChild(mpCard);
        }

        return wrapper;
    }

    // ── Helpers ──
    function formatPrice(price) {
        return new Intl.NumberFormat('tr-TR', {
            style: 'currency',
            currency: 'TRY',
            minimumFractionDigits: 2
        }).format(price);
    }

    function escapeHtml(str) {
        var div = document.createElement('div');
        div.textContent = str;
        return div.innerHTML;
    }

    // ── Boot ──
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }
})();
