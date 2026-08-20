(function (window, $) {
    'use strict';

    function getText(selector) {
        return ($(selector).val() || '').toString().trim();
    }

    function setFeedback($feedback, message, type) {
        if (!$feedback.length) {
            return;
        }

        $feedback
            .removeClass('text-muted text-danger text-success')
            .addClass(type === 'error' ? 'text-danger' : type === 'success' ? 'text-success' : 'text-muted')
            .text(message);
    }

    function firstValidResult(results) {
        if (!Array.isArray(results)) {
            return null;
        }

        return results.find(function (item) {
            return item &&
                Number.isFinite(Number(item.latitude)) &&
                Number.isFinite(Number(item.longitude));
        }) || null;
    }

    function buildQuery(options) {
        var city = getText(options.cityNameSelector);
        var district = getText(options.districtNameSelector);
        var neighborhood = getText(options.neighborhoodSelector);
        var fullAddress = getText(options.fullAddressSelector);
        var fullQuery = [fullAddress, neighborhood, district, city, 'Türkiye']
            .filter(function (part) { return part.length > 0; })
            .join(', ');
        var fallbackQueries = [];

        fallbackQueries.push({
            text: fullQuery,
            approximate: false
        });

        if (neighborhood) {
            fallbackQueries.push({
                text: [neighborhood, district, city, 'Türkiye']
                    .filter(function (part) { return part.length > 0; })
                    .join(', '),
                approximate: true,
                label: 'Mahalle bulundu; sokak veya kapı numarası OSM verisinde bulunamadı.'
            });
        }

        fallbackQueries.push({
            text: [district, city, 'Türkiye']
                .filter(function (part) { return part.length > 0; })
                .join(', '),
            approximate: true,
            label: 'İlçe bulundu; adresi pin sürükleyerek netleştirin.'
        });

        return {
            city: city,
            district: district,
            fullAddress: fullAddress,
            text: fullQuery,
            fallbackQueries: fallbackQueries.filter(function (item, index, items) {
                return item.text && items.findIndex(function (candidate) {
                    return candidate.text === item.text;
                }) === index;
            })
        };
    }

    function searchCandidates(candidates) {
        var index = 0;

        function next() {
            if (index >= candidates.length) {
                return $.Deferred().resolve(null).promise();
            }

            var candidate = candidates[index++];
            return $.get('/api/location/geocode', { q: candidate.text, limit: 1 })
                .then(function (results) {
                    var result = firstValidResult(results && results.data ? results.data : results);

                    if (result) {
                        result._query = candidate.text;
                        result._approximate = candidate.approximate === true;
                        result._approximateLabel = candidate.label || '';
                        return result;
                    }

                    return next();
                });
        }

        return next();
    }

    function init(options) {
        var $button = $(options.buttonSelector);
        var $feedback = $(options.feedbackSelector);

        if (!$button.length) {
            return;
        }

        $button.on('click', function () {
            var query = buildQuery(options);

            if (!query.city || !query.district) {
                setFeedback($feedback, 'Haritada aramak için önce il ve ilçe seçin.', 'error');
                return;
            }

            if (!query.fullAddress) {
                setFeedback($feedback, 'Haritada aramak için açık adres girin.', 'error');
                return;
            }

            var originalHtml = $button.html();
            $button.prop('disabled', true).html('<i class="fas fa-spinner fa-spin me-2"></i>Aranıyor');
            setFeedback($feedback, 'Adres haritada aranıyor...', 'muted');

            searchCandidates(query.fallbackQueries)
                .done(function (result) {
                    if (!result) {
                        setFeedback($feedback, 'Adres bulunamadı. Pin’i haritada elle konumlandırabilirsiniz.', 'error');
                        return;
                    }

                    var lat = Number(result.latitude);
                    var lng = Number(result.longitude);

                    $(options.latitudeSelector).val(lat.toFixed(6));
                    $(options.longitudeSelector).val(lng.toFixed(6));

                    if (typeof options.onLocated === 'function') {
                        options.onLocated(lat, lng, result);
                    }

                    if (result._approximate) {
                        setFeedback($feedback, result._approximateLabel + ' Pin’i tam dükkan konumuna sürükleyin.', 'muted');
                    } else {
                        setFeedback($feedback, result.displayName || 'Konum bulundu ve haritada işaretlendi.', 'success');
                    }
                })
                .fail(function () {
                    setFeedback($feedback, 'Adres aranırken hata oluştu. Lütfen tekrar deneyin.', 'error');
                })
                .always(function () {
                    $button.prop('disabled', false).html(originalHtml);
                });
        });
    }

    window.TumMenuAddressMapGeocoder = {
        init: init
    };
})(window, window.jQuery);
