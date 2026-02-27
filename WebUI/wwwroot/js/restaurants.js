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

    // ── State ──
    let state = {
        searchTerm: '',
        categoryIds: [],
        isVegan: false,
        maxDistanceKm: null,
        city: '',
        district: '',
        userLat: null,
        userLng: null,
        page: 0,
        pageSize: 12,
        loading: false,
        hasNext: false
    };

    // ── Init ──
    function init() {
        readURLParams();
        bindEvents();
        requestUserLocation();

        // Sayfa açıldığında her zaman arama yap (parametre olsun veya olmasın)
        doSearch(false);
    }

    function readURLParams() {
        const params = new URLSearchParams(window.location.search);

        if (params.has('search')) state.searchTerm = params.get('search');
        if (params.has('city')) state.city = params.get('city');
        if (params.has('district')) state.district = params.get('district');
        if (params.has('vegan') && params.get('vegan') === 'true') state.isVegan = true;
        if (params.has('distance')) state.maxDistanceKm = parseFloat(params.get('distance')) || null;

        if (params.has('categories')) {
            state.categoryIds = params.get('categories').split(',').filter(Boolean);
        }

        // Reflect state in UI
        if (state.searchTerm) searchInput.value = state.searchTerm;
        if (state.city) cityInput.value = state.city;
        if (state.district) districtInput.value = state.district;
        if (state.isVegan) veganToggle.checked = true;

        // Highlight active category chips
        state.categoryIds.forEach(function (id) {
            var chip = document.querySelector('[data-category-id="' + id + '"]');
            if (chip) chip.classList.add('is-active');
        });

        // Highlight active distance
        if (state.maxDistanceKm) {
            var distBtn = document.querySelector('[data-distance="' + state.maxDistanceKm + '"]');
            if (distBtn) distBtn.classList.add('is-active');
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

        // Category chips
        document.getElementById('categoryFilters').addEventListener('click', function (e) {
            var chip = e.target.closest('.rest-chip');
            if (!chip) return;
            var id = chip.dataset.categoryId;
            chip.classList.toggle('is-active');

            var idx = state.categoryIds.indexOf(id);
            if (idx > -1) state.categoryIds.splice(idx, 1);
            else state.categoryIds.push(id);

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
            var dist = parseInt(btn.dataset.distance);

            // toggle
            var allDist = document.querySelectorAll('.rest-dist-btn');
            var wasActive = btn.classList.contains('is-active');
            allDist.forEach(function (b) { b.classList.remove('is-active'); });

            if (wasActive) {
                state.maxDistanceKm = null;
            } else {
                btn.classList.add('is-active');
                state.maxDistanceKm = dist;
            }

            state.page = 0;
            doSearch(false);
        });

        // Load Provinces
        fetch('/api/location/provinces')
            .then(res => res.json())
            .then(data => {
                data.forEach(p => {
                    var opt = document.createElement('option');
                    opt.value = p.name; // We send the name to the backend as requested by SearchStoresQuery
                    opt.textContent = p.name;
                    opt.dataset.id = p.id;
                    if(state.city === p.name) opt.selected = true;
                    cityInput.appendChild(opt);
                });
                if(state.city) {
                    var selectedOpt = cityInput.options[cityInput.selectedIndex];
                    if(selectedOpt && selectedOpt.dataset.id) {
                        loadDistricts(selectedOpt.dataset.id, state.district);
                    }
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
                        opt.value = d.name; // Send name
                        opt.textContent = d.name;
                        if(selectedDistrictName === d.name) opt.selected = true;
                        districtInput.appendChild(opt);
                    });
                });
        }

        cityInput.addEventListener('change', function() {
            var selectedOpt = cityInput.options[cityInput.selectedIndex];
            state.city = selectedOpt.value;
            state.district = '';
            
            if(selectedOpt.value && selectedOpt.dataset.id) {
                loadDistricts(selectedOpt.dataset.id);
            } else {
                districtInput.disabled = true;
                districtInput.innerHTML = '<option value="">Önce İl Seçiniz</option>';
            }
            state.page = 0;
            doSearch(false);
        });

        districtInput.addEventListener('change', function() {
            state.district = districtInput.value;
            state.page = 0;
            doSearch(false);
        });

        // Use my location
        useMyLocBtn.addEventListener('click', function () {
            requestUserLocation(true);
        });

        // Clear all
        clearBtn.addEventListener('click', function () {
            searchInput.value = '';
            cityInput.value = '';
            districtInput.value = '';
            veganToggle.checked = false;
            document.querySelectorAll('.rest-chip.is-active').forEach(function (c) { c.classList.remove('is-active'); });
            document.querySelectorAll('.rest-dist-btn.is-active').forEach(function (b) { b.classList.remove('is-active'); });

            state.searchTerm = '';
            state.categoryIds = [];
            state.isVegan = false;
            state.maxDistanceKm = null;
            state.city = '';
            state.district = '';
            state.page = 0;

            resultGrid.innerHTML = '';
            resultCount.textContent = '';
            loadMoreWrap.hidden = true;
            emptyEl.hidden = true;
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

    // ── Geolocation ──
    function requestUserLocation(forcePrompt) {
        if (!navigator.geolocation) return;

        // Try to get location silently if no city/district set
        if (forcePrompt || (!state.city && !state.district)) {
            navigator.geolocation.getCurrentPosition(
                function (pos) {
                    state.userLat = pos.coords.latitude;
                    state.userLng = pos.coords.longitude;
                    if (forcePrompt) {
                        cityInput.value = '';
                        districtInput.value = '';
                        state.city = '';
                        state.district = '';
                        state.page = 0;
                        doSearch(false);
                    }
                },
                function () { /* denied – silently ignore */ },
                { timeout: 5000, maximumAge: 300000 }
            );
        }
    }

    // ── API Call ──
    function doSearch(append) {
        if (state.loading) return;
        state.loading = true;

        loadingEl.hidden = false;
        emptyEl.hidden = true;
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
        if (state.city) params.set('City', state.city);
        if (state.district) params.set('District', state.district);
        params.set('Page', state.page);
        params.set('PageSize', state.pageSize);

        fetch('/api/stores/search?' + params.toString())
            .then(function (res) { return res.json(); })
            .then(function (data) {
                state.loading = false;
                loadingEl.hidden = true;

                if (data.items.length === 0 && !append) {
                    emptyEl.hidden = false;
                } else {
                    emptyEl.hidden = true;
                }

                resultCount.innerHTML = '<strong>' + data.totalCount + '</strong> restoran bulundu';

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



    // ── Card Builder ──
    function createStoreCard(store) {
        var wrapper = document.createElement('div');
        wrapper.className = 'store-card-wrapper';

        var card = document.createElement('a');
        card.className = 'store-card';
        card.href = '/Store/Details/' + store.id;

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
            distBadge.textContent = store.distanceKm + ' km';
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
