(function () {
    'use strict';
    const ICON_SPRITE_PATH = '/assets/icons/tm-sprite.svg';

    const FOODS = [
        { name: 'Margherita Pizza', icon: 'icon-menu', tags: ['hizli'], reason: 'Klasik bir seçim, herkesi mutlu eder!', alt: 'Sucuklu Pide' },
        { name: 'Mercimek Çorbası', icon: 'icon-menu', tags: ['hafif', 'vegan', 'ekonomik'], reason: 'Sıcacık, doyurucu ve bütçe dostu.', alt: 'Domates Çorbası' },
        { name: 'Tavuk Dürüm', icon: 'icon-menu', tags: ['protein', 'hizli'], reason: 'Hızlı ve protein dolu bir öğün.', alt: 'Adana Dürüm' },
        { name: 'Sezar Salata', icon: 'icon-menu', tags: ['hafif', 'protein'], reason: 'Hafif ama doyurucu, mükemmel denge.', alt: 'Ton Balıklı Salata' },
        { name: 'Peynirli Burger', icon: 'icon-menu', tags: ['protein'], reason: 'Günün stresini bu çözer.', alt: 'Sebzeli Burger' },
        { name: 'Falafel Wrap', icon: 'icon-menu', tags: ['vegan', 'ekonomik'], reason: 'Vegan dostu, lezzet dolu!', alt: 'Humus Tabağı' },
        { name: 'Karnıyarık', icon: 'icon-menu', tags: ['protein'], reason: 'Ev yemeği özlemi? İşte burada.', alt: 'İmam Bayıldı' },
        { name: 'Suşi Tabağı', icon: 'icon-menu', tags: [], reason: 'Farklı bir şey deneyelim bugün!', alt: 'Poké Kasesi' },
        { name: 'Makarna', icon: 'icon-menu', tags: ['hizli', 'ekonomik'], reason: '10 dakikada hazır, her zaman güzel.', alt: 'Lazanya' },
        { name: 'Güveç', icon: 'icon-menu', tags: ['protein'], reason: 'Yavaş pişmiş, derin tat.', alt: 'Türlü' },
        { name: 'Acılı Tavuk', icon: 'icon-menu', tags: ['aci', 'protein'], reason: 'Biraz ateş lazım bugün', alt: 'Acılı Kanat' },
        { name: 'Waffle', icon: 'icon-menu', tags: ['tatli', 'hizli'], reason: 'Tatlı krizine birebir!', alt: 'Krep' },
        { name: 'Brownie', icon: 'icon-menu', tags: ['tatli'], reason: 'Çikolata her şeyi çözer.', alt: 'Sufle' },
        { name: 'Smoothie Kasesi', icon: 'icon-menu', tags: ['hafif', 'vegan'], reason: 'Sağlıklı ama lezzetli!', alt: 'Açai Kasesi' },
        { name: 'Mercimek Köftesi', icon: 'icon-menu', tags: ['vegan', 'ekonomik', 'hafif'], reason: 'Hafif, sağlıklı, ekonomik üçlüsü.', alt: 'Kısır' },
        { name: 'Tantuni', icon: 'icon-menu', tags: ['aci', 'hizli', 'protein'], reason: 'Acılı, hızlı, doyurucu. Ne istersin?', alt: 'Çiğ Köfte Dürüm' },
        { name: 'Künefe', icon: 'icon-menu', tags: ['tatli'], reason: 'Sıcak peynirli, şerbetli mutluluk.', alt: 'Katmer' },
        { name: 'Menemen', icon: 'icon-menu', tags: ['ekonomik', 'hizli', 'hafif'], reason: 'Sabah akşam her zaman iyi gider.', alt: 'Omlet' },
        { name: 'Lahmacun', icon: 'icon-menu', tags: ['hizli', 'ekonomik'], reason: 'İnce hamur, bolca lezzet. Klasiklerin klasiği.', alt: 'Etli Ekmek' },
        { name: 'Döner', icon: 'icon-menu', tags: ['protein', 'hizli'], reason: 'Her zaman, her yerde güzel!', alt: 'İskender' },
        { name: 'İskender Kebap', icon: 'icon-menu', tags: ['protein'], reason: 'Tereyağlı, yoğurtlu bir şölen.', alt: 'Döner' },
        { name: 'Köfte Ekmek', icon: 'icon-menu', tags: ['protein', 'hizli', 'ekonomik'], reason: 'Sokak lezzetlerinin kralı.', alt: 'Kasap Burger' },
        { name: 'Pide', icon: 'icon-menu', tags: ['protein'], reason: 'Karadeniz usulü, içi dolu dolu.', alt: 'Lahmacun' },
        { name: 'Tost', icon: 'icon-menu', tags: ['hizli', 'ekonomik'], reason: 'Basit ama asla sıradan değil.', alt: 'Kumru' },
        { name: 'Kokoreç', icon: 'icon-menu', tags: ['aci', 'protein', 'hizli'], reason: 'Cesurların tercihi, pişman olmayacaksın!', alt: 'Midye Dolma' },
        { name: 'Mantı', icon: 'icon-menu', tags: ['protein'], reason: 'Anneannenin mutfağından geldi sanki.', alt: 'Düşes Patates' },
        { name: 'Çiğ Köfte Dürüm', icon: 'icon-menu', tags: ['vegan', 'aci', 'hizli', 'ekonomik'], reason: 'Acılı, limonlu, tam kıvamında.', alt: 'Falafel Wrap' },
        { name: 'Adana Kebap', icon: 'icon-menu', tags: ['aci', 'protein'], reason: 'Biber acısıyla efsane bir lezzet.', alt: 'Urfa Kebap' },
        { name: 'Pilav Üstü Kuru Fasulye', icon: 'icon-menu', tags: ['protein', 'ekonomik'], reason: 'Türkiye\'nin resmi fast food\'u', alt: 'Nohut Yemeği' },
        { name: 'Balık Ekmek', icon: 'icon-menu', tags: ['protein', 'hafif'], reason: 'Deniz kenarı havası evine gelsin.', alt: 'Balık Tava' },
        { name: 'Kumpir', icon: 'icon-menu', tags: ['protein', 'hizli'], reason: 'İçine ne istersen koyabilirsin!', alt: 'Patates Kızartması' },
        { name: 'Gözleme', icon: 'icon-menu', tags: ['ekonomik', 'hafif'], reason: 'Yufka + peynir = mutluluk formülü.', alt: 'Börek' },
        { name: 'Börek', icon: 'icon-menu', tags: ['ekonomik'], reason: 'Çıtır çıtır, katkat lezzet.', alt: 'Poğaça' },
        { name: 'Midye Dolma', icon: 'icon-menu', tags: ['hizli', 'ekonomik'], reason: 'Sokak lezzetlerinin vazgeçilmezi.', alt: 'Midye Tava' },
        { name: 'Etli Ekmek', icon: 'icon-menu', tags: ['protein'], reason: 'Konya usulü, uzun ve doyurucu.', alt: 'Lahmacun' },
        { name: 'Izgara Tavuk', icon: 'icon-menu', tags: ['protein', 'hafif'], reason: 'Sağlıklı ve lezzetli protein kaynağı.', alt: 'Tavuk Şiş' },
        { name: 'Noodle', icon: 'icon-menu', tags: ['hizli'], reason: 'Asya esintili, hızlı ve doyurucu.', alt: 'Ramen' },
        { name: 'Ramen', icon: 'icon-menu', tags: ['protein'], reason: 'Derin tat, zengin et suyu.', alt: 'Noodle' },
        { name: 'Taco', icon: 'icon-menu', tags: ['aci', 'hizli'], reason: 'Meksika esintisi, eğlenceli yemek!', alt: 'Burrito' },
        { name: 'Poké Kasesi', icon: 'icon-menu', tags: ['hafif', 'protein'], reason: 'Taze, renkli ve sağlıklı.', alt: 'Suşi Tabağı' },
        { name: 'Simit', icon: 'icon-menu', tags: ['ekonomik', 'hizli', 'hafif'], reason: 'Çay yanında efsane ikili.', alt: 'Poğaça' },
        { name: 'Baklava', icon: 'icon-menu', tags: ['tatli'], reason: 'Antep fıstıklı, şerbetli mükemmellik.', alt: 'Künefe' },
        { name: 'Dondurma', icon: 'icon-menu', tags: ['tatli', 'hizli'], reason: 'Her mevsim, her zaman tatlı kriz ilacı.', alt: 'Profiterol' },
        { name: 'Profiterol', icon: 'icon-menu', tags: ['tatli'], reason: 'Çikolata şelalesi altında ekler.', alt: 'Brownie' },
        { name: 'Tavuk Kanat', icon: 'icon-menu', tags: ['protein', 'aci', 'hizli'], reason: 'Soslu kanatlar, parmak yalatan lezzet.', alt: 'Acılı Tavuk' },
        { name: 'Wrap', icon: 'icon-menu', tags: ['hafif', 'hizli'], reason: 'İçi renkli, hafif ve pratik.', alt: 'Falafel Wrap' },
        { name: 'Kısır', icon: 'icon-menu', tags: ['vegan', 'hafif', 'ekonomik'], reason: 'Narli, limonlu, ferahlatıcı.', alt: 'Mercimek Köftesi' },
        { name: 'Nohut Yemeği', icon: 'icon-menu', tags: ['vegan', 'ekonomik', 'protein'], reason: 'Pilavla beraber enfes!', alt: 'Kuru Fasulye' },
        { name: 'Sarma', icon: 'icon-menu', tags: ['hafif', 'ekonomik'], reason: 'Zeytinyağlı, soğuk ya da sıcak muhteşem.', alt: 'Biber Dolma' },
    ];

    const FUN_TEXTS = [
        'Standart öneriler hazır.',
        'Seçim havuzu güncellendi.',
        'Karar seti daha net hale geliyor.',
        'Tercihlerine uygun alternatifler hazır.',
        'Bugün için dengeli bir seçim listesi çıkıyor.',
    ];

    const BUDGET_SUGGESTIONS = [
        {
            max: 50, items: [
                { icon: 'icon-menu', name: 'Mercimek Çorbası', price: '₺35' },
                { icon: 'icon-menu', name: 'Menemen', price: '₺40' },
                { icon: 'icon-menu', name: 'Mercimek Köftesi', price: '₺30' },
            ]
        },
        {
            max: 100, items: [
                { icon: 'icon-menu', name: 'Margherita Pizza', price: '₺80' },
                { icon: 'icon-menu', name: 'Tavuk Dürüm', price: '₺75' },
                { icon: 'icon-menu', name: 'Makarna', price: '₺65' },
            ]
        },
        {
            max: 200, items: [
                { icon: 'icon-menu', name: 'Peynirli Burger', price: '₺120' },
                { icon: 'icon-menu', name: 'Güveç', price: '₺110' },
                { icon: 'icon-menu', name: 'Suşi Tabağı', price: '₺180' },
            ]
        },
        {
            max: Infinity, items: [
                { icon: 'icon-menu', name: 'Bonfile', price: '₺350' },
                { icon: 'icon-menu', name: 'Omakase Suşi', price: '₺450' },
                { icon: 'icon-menu', name: 'Deniz Mahsulleri', price: '₺400' },
            ]
        },
    ];

    const WHEEL_ARTWORKS = {
        burger: '/images/wte/burger.svg',
        pizza: '/images/wte/pizza.svg',
        durum: '/images/wte/durum.svg',
        corba: '/images/wte/corba.svg',
        salata: '/images/wte/salata.svg',
        sushi: '/images/wte/sushi.svg',
        guvec: '/images/wte/guvec.svg',
        makarna: '/images/wte/makarna.svg',
        tatli: '/images/wte/tatli.svg',
    };

    const TAG_LABELS = {
        hafif: 'Hafif',
        aci: 'Acılı',
        tatli: 'Tatlı',
        vegan: 'Vegan',
        hizli: 'Hızlı',
        ekonomik: 'Ekonomik',
        protein: 'Protein ağırlıklı',
    };

    const chipWrap = document.getElementById('wte-chips-wrap');
    const wheel = document.getElementById('wte-wheel');
    const spinBtn = document.getElementById('wte-spin-btn');
    const funTextEl = document.getElementById('wte-fun-text');
    const resultsSection = document.getElementById('wte-results');
    const resultsGrid = document.getElementById('wte-results-grid');
    const resultsFun = document.getElementById('wte-results-fun');
    const refreshBtn = document.getElementById('wte-refresh-btn');
    const budgetSlider = document.getElementById('wte-budget-slider');
    const budgetAmount = document.getElementById('wte-budget-amount');
    const budgetSuggestions = document.getElementById('wte-budget-suggestions');

    let activeTags = new Set();
    let currentRotation = 0;
    let isSpinning = false;
    let lastPicks = new Set();
    const wheelFace = document.querySelector('.wte-wheel__face');

    function resolveFoodIconId(foodName) {
        const name = normalizeText(foodName);
        if (name.includes('pizza') || name.includes('lahmacun') || name.includes('pide')) return 'icon-pizza';
        if (name.includes('burger') || name.includes('kofte') || name.includes('iskender') || name.includes('kebap')) return 'icon-burger';
        if (name.includes('salata') || name.includes('vegan') || name.includes('kisir') || name.includes('sarma')) return 'icon-leaf';
        if (name.includes('makarna') || name.includes('corba') || name.includes('ramen') || name.includes('noodle')) return 'icon-bowl';
        if (name.includes('baklava') || name.includes('kunefe') || name.includes('waffle') || name.includes('tatli')) return 'icon-star';
        if (name.includes('balik') || name.includes('sushi') || name.includes('susi') || name.includes('poke')) return 'icon-bowl';
        return 'icon-menu';
    }

    function getIconMarkup(iconId) {
        return `<svg class="tm-icon" aria-hidden="true"><use href="${ICON_SPRITE_PATH}#${iconId || 'icon-menu'}"></use></svg>`;
    }

    function getCurrentBudget() {
        return budgetSlider ? parseInt(budgetSlider.value, 10) : 100;
    }

    function getCurrentSeason() {
        const month = new Date().getMonth();
        if (month >= 2 && month <= 4) return 'spring';
        if (month >= 5 && month <= 7) return 'summer';
        if (month >= 8 && month <= 10) return 'autumn';
        return 'winter';
    }

    function estimateFoodPrice(food) {
        const name = normalizeText(food.name);
        let price = 110;

        if (food.tags.includes('ekonomik')) price -= 35;
        if (food.tags.includes('protein')) price += 20;
        if (food.tags.includes('hizli')) price -= 8;
        if (food.tags.includes('tatli')) price -= 15;

        if (name.includes('pizza') || name.includes('burger') || name.includes('sushi') || name.includes('susi')) price += 35;
        if (name.includes('kebap') || name.includes('bonfile') || name.includes('iskender')) price += 70;
        if (name.includes('corba') || name.includes('menemen') || name.includes('simit') || name.includes('borek')) price -= 25;
        if (name.includes('lahmacun') || name.includes('tost') || name.includes('durum') || name.includes('tantuni')) price -= 10;

        return Math.max(30, price);
    }

    function getSeasonScore(food) {
        const season = getCurrentSeason();
        const name = normalizeText(food.name);

        if (season === 'winter' && (name.includes('corba') || name.includes('guvec') || name.includes('ramen') || name.includes('makarna'))) return 24;
        if (season === 'spring' && (name.includes('salata') || name.includes('durum') || name.includes('wrap') || name.includes('mercimek koftesi'))) return 24;
        if (season === 'summer' && (name.includes('salata') || name.includes('poke') || name.includes('sushi') || name.includes('susi') || name.includes('dondurma'))) return 24;
        if (season === 'autumn' && (name.includes('guvec') || name.includes('makarna') || name.includes('kebap') || name.includes('karniyarik'))) return 24;

        return 0;
    }

    function rankFoods() {
        const selectedTags = [...activeTags];
        const budget = getCurrentBudget();

        return FOODS.map(food => {
            const matches = food.tags.filter(tag => activeTags.has(tag));
            const estimatedPrice = estimateFoodPrice(food);
            const budgetGap = Math.abs(estimatedPrice - budget);
            let score = 0;

            score += matches.length * 38;
            score += getSeasonScore(food);
            score += Math.max(-30, 28 - (budgetGap / 6));

            if (selectedTags.length > 0) {
                const coverage = matches.length / selectedTags.length;
                score += coverage * 42;

                if (matches.length === selectedTags.length) score += 22;
                else if (selectedTags.length > 1 && matches.length === selectedTags.length - 1) score += 10;
                else if (matches.length === 0) score -= 65;
            }

            if (food.tags.includes('ekonomik') && budget <= 90) score += 12;
            if (food.tags.includes('protein') && activeTags.has('protein')) score += 10;
            if (food.tags.includes('hafif') && activeTags.has('hafif')) score += 10;
            if (lastPicks.has(food.name)) score -= 18;

            return { food, score, matches, estimatedPrice };
        }).sort((a, b) => b.score - a.score);
    }

    function getRecommendationPool(limit) {
        const ranked = rankFoods();
        const selectedCount = activeTags.size;

        if (selectedCount === 0) {
            return ranked.slice(0, Math.max(limit * 2, 10));
        }

        const minMatches = selectedCount === 1 ? 1 : Math.max(1, Math.ceil(selectedCount / 2));
        let pool = ranked.filter(item => item.matches.length >= minMatches);

        if (pool.length < limit) {
            pool = ranked.filter(item => item.matches.length >= 1);
        }

        if (pool.length < limit) {
            pool = ranked;
        }

        return pool.slice(0, Math.max(limit * 2, 10));
    }

    function pickRecommendations(limit) {
        return getRecommendationPool(limit)
            .map(item => ({ ...item, score: item.score + (Math.random() * 6) }))
            .sort((a, b) => b.score - a.score)
            .slice(0, limit);
    }

    function pickWheelRecommendations(limit) {
        const pool = getRecommendationPool(limit * 3);
        const selected = [];
        const usedIllustrations = new Set();

        for (const item of pool) {
            const illustrationPath = resolveWheelIllustration(item.food.name);
            if (usedIllustrations.has(illustrationPath)) continue;

            selected.push(item);
            usedIllustrations.add(illustrationPath);

            if (selected.length === limit) return selected;
        }

        for (const item of pool) {
            if (selected.length === limit) break;
            if (!selected.includes(item)) selected.push(item);
        }

        return selected.slice(0, limit);
    }

    function getMatchSummary(matches) {
        if (!matches.length) return '';
        return matches.map(tag => TAG_LABELS[tag] || tag).join(', ');
    }

    function buildRecommendationReason(food, meta) {
        const details = [food.reason];

        if (meta.matches.length) {
            details.push(`Seçtiğin tercihlerle uyumlu: ${getMatchSummary(meta.matches)}.`);
        }

        details.push(`Tahmini fiyat: yaklaşık ₺${meta.estimatedPrice}.`);

        return details.join(' ');
    }

    if (chipWrap) {
        chipWrap.addEventListener('click', (e) => {
            const chip = e.target.closest('.wte-chip');
            if (!chip) return;

            const tag = chip.dataset.tag;
            chip.classList.toggle('is-active');

            if (activeTags.has(tag)) {
                activeTags.delete(tag);
            } else {
                activeTags.add(tag);
            }

            chip.style.animation = 'none';
            chip.offsetHeight;
            chip.style.animation = '';

            updateWheelLabels();
        });
    }

    function updateWheelLabels() {
        if (!wheelFace) return;
        const items = pickWheelRecommendations(8);

        const labels = wheelFace.querySelectorAll('.wte-wheel__label');
        labels.forEach((label, i) => {
            if (items[i]) {
                label.style.opacity = '0';
                label.style.transition = 'opacity 0.25s ease';
                setTimeout(() => {
                    label.setAttribute('aria-label', items[i].food.name);
                    label.innerHTML = getWheelIllustrationMarkup(items[i].food);
                    label.style.opacity = '1';
                }, 250);
            }
        });
    }

    function spinWheel() {
        if (isSpinning || !spinBtn || !wheel) return;
        isSpinning = true;

        spinBtn.classList.add('is-disabled');
        wheel.classList.add('is-spinning');

        if (funTextEl) {
            funTextEl.textContent = FUN_TEXTS[Math.floor(Math.random() * FUN_TEXTS.length)];
        }

        const extraDeg = Math.floor(Math.random() * 360);
        const totalDeg = currentRotation + 1800 + extraDeg;
        currentRotation = totalDeg;

        wheel.style.transform = `rotate(${totalDeg}deg)`;

        setTimeout(() => {
            isSpinning = false;
            spinBtn.classList.remove('is-disabled');
            wheel.classList.remove('is-spinning');
            showResults();
        }, 4200);
    }

    if (spinBtn) {
        spinBtn.addEventListener('click', spinWheel);
    }

    function showResults() {
        if (!resultsSection || !resultsGrid) return;

        let picks = pickRecommendations(3);
        if (picks.length < 3) {
            lastPicks.clear();
            picks = pickRecommendations(3);
        }

        picks.forEach(p => lastPicks.add(p.food.name));

        if (resultsFun) {
            resultsFun.textContent = FUN_TEXTS[Math.floor(Math.random() * FUN_TEXTS.length)];
        }

        resultsGrid.innerHTML = picks.map((item, i) => `
            <div class="wte-result-card" style="transition-delay: ${i * 0.15}s">
                <div class="wte-result-card__top">
                    <span class="wte-result-card__emoji">${getIconMarkup(resolveFoodIconId(item.food.name))}</span>
                    <div>
                        <h3 class="wte-result-card__name">${item.food.name}</h3>
                        <span class="wte-result-card__label">önerilen seçim</span>
                    </div>
                </div>
                <p class="wte-result-card__reason"><strong>Neden uygun?</strong> ${buildRecommendationReason(item.food, item)}</p>
                <div class="wte-result-card__actions">
                    <button class="wte-btn wte-btn--ghost" onclick="swapCard(this, '${item.food.alt}')">
                        Alternatif getir
                    </button>
                    <button class="wte-btn wte-btn--ghost" onclick="shareFood('${item.food.name}')">
                        Paylaş
                    </button>
                </div>
            </div>
        `).join('');

        resultsSection.classList.remove('is-hidden');

        requestAnimationFrame(() => {
            const cards = resultsGrid.querySelectorAll('.wte-result-card');
            cards.forEach((card, i) => {
                setTimeout(() => card.classList.add('is-visible'), i * 150);
            });
        });

        setTimeout(() => {
            resultsSection.scrollIntoView({ behavior: 'smooth', block: 'start' });
        }, 300);
    }

    window.swapCard = function (btn, altName) {
        const card = btn.closest('.wte-result-card');
        const altFood = FOODS.find(f => f.name === altName) || FOODS[Math.floor(Math.random() * FOODS.length)];
        const altMeta = {
            matches: altFood.tags.filter(tag => activeTags.has(tag)),
            estimatedPrice: estimateFoodPrice(altFood),
        };

        card.classList.remove('is-visible');
        setTimeout(() => {
            card.querySelector('.wte-result-card__emoji').innerHTML = getIconMarkup(resolveFoodIconId(altFood.name));
            card.querySelector('.wte-result-card__name').textContent = altFood.name;
            card.querySelector('.wte-result-card__reason').innerHTML =
                `<strong>Neden uygun?</strong> ${buildRecommendationReason(altFood, altMeta)}`;

            const btns = card.querySelectorAll('.wte-btn--ghost');
            btns[0].innerHTML = 'Alternatif getir';
            btns[0].setAttribute('onclick', `swapCard(this, '${altFood.alt}')`);
            btns[1].setAttribute('onclick', `shareFood('${altFood.name}')`);

            card.classList.add('is-visible');
        }, 300);
    };

    window.shareFood = function (text) {
        const shareData = {
            title: 'Ne Yesem?',
            text: `Bugün bana önerilen: ${text}\nSen de dene → `,
            url: window.location.href,
        };

        if (navigator.share) {
            navigator.share(shareData).catch(() => { });
        } else {
            navigator.clipboard.writeText(`${shareData.text}${shareData.url}`).then(() => {
                showToast('Seçim panoya kopyalandı.');
            });
        }
    };

    function showToast(msg) {
        const toast = document.createElement('div');
        toast.textContent = msg;
        Object.assign(toast.style, {
            position: 'fixed',
            bottom: '30px',
            left: '50%',
            transform: 'translateX(-50%)',
            background: '#4B3A2D',
            color: '#fff',
            padding: '12px 24px',
            borderRadius: '999px',
            fontSize: '14px',
            fontWeight: '700',
            zIndex: '9999',
            animation: 'wte-fade-in-up 0.3s ease',
        });
        document.body.appendChild(toast);
        setTimeout(() => toast.remove(), 2000);
    }

    if (refreshBtn) {
        refreshBtn.addEventListener('click', () => {
            refreshBtn.classList.add('wte-shake');
            setTimeout(() => refreshBtn.classList.remove('wte-shake'), 500);
            spinWheel();
        });
    }

    function updateBudget() {
        if (!budgetSlider || !budgetAmount || !budgetSuggestions) return;

        const val = parseInt(budgetSlider.value);
        budgetAmount.textContent = `₺${val}`;

        const tier = BUDGET_SUGGESTIONS.find(t => val <= t.max);
        if (tier) {
            budgetSuggestions.innerHTML = tier.items.map(item => `
                <div class="wte-budget__suggestion">
                    <span>${getIconMarkup(resolveFoodIconId(item.name))}</span>
                    <span style="flex:1">${item.name}</span>
                    <span style="font-weight:800; color: var(--color-primary-dark)">${item.price}</span>
                </div>
            `).join('');
        }
    }

    if (budgetSlider) {
        budgetSlider.addEventListener('input', () => {
            updateBudget();
            updateWheelLabels();
        });
        updateBudget();
    }

    function highlightSeason() {
        const season = getCurrentSeason();

        const seasonCard = document.getElementById('wte-season-card');
        if (!seasonCard) return;

        const items = seasonCard.querySelectorAll('.wte-season-item');
        items.forEach(item => {
            if (item.classList.contains(`wte-season-item--${season}`)) {
                item.classList.add('is-highlighted');
            } else {
                item.classList.remove('is-highlighted');
            }
        });
    }

    highlightSeason();

    function getWheelIllustrationMarkup(food) {
        const illustrationPath = resolveWheelIllustration(food.name);
        return `<img class="wte-wheel__icon" src="${illustrationPath}" alt="">`;
    }

    function resolveWheelIllustration(foodName) {
        const name = normalizeText(foodName);

        if (name.includes('pizza') || name.includes('lahmacun') || name.includes('pide') || name.includes('etli ekmek')) {
            return WHEEL_ARTWORKS.pizza;
        }
        if (name.includes('corba') || name.includes('ramen') || name.includes('noodle') || name.includes('menemen')) {
            return WHEEL_ARTWORKS.corba;
        }
        if (name.includes('durum') || name.includes('wrap') || name.includes('tantuni') || name.includes('taco') || name.includes('doner') || name.includes('kokorec')) {
            return WHEEL_ARTWORKS.durum;
        }
        if (name.includes('burger') || name.includes('kofte ekmek') || name.includes('tost')) {
            return WHEEL_ARTWORKS.burger;
        }
        if (name.includes('salata') || name.includes('bowl') || name.includes('kasir') || name.includes('kisir') || name.includes('mercimek koftesi') || name.includes('sarma')) {
            return WHEEL_ARTWORKS.salata;
        }
        if (name.includes('sushi') || name.includes('susi') || name.includes('poke')) {
            return WHEEL_ARTWORKS.sushi;
        }
        if (name.includes('guvec') || name.includes('karniyarik') || name.includes('kebap') || name.includes('tavuk') || name.includes('fasulye') || name.includes('nohut')) {
            return WHEEL_ARTWORKS.guvec;
        }
        if (name.includes('makarna') || name.includes('manti') || name.includes('lazanya')) {
            return WHEEL_ARTWORKS.makarna;
        }
        if (name.includes('kunefe') || name.includes('baklava') || name.includes('brownie') || name.includes('waffle') || name.includes('profiterol') || name.includes('dondurma')) {
            return WHEEL_ARTWORKS.tatli;
        }

        return WHEEL_ARTWORKS.pizza;
    }

    function normalizeText(text) {
        return text
            .toLocaleLowerCase('tr-TR')
            .replace(/ç/g, 'c')
            .replace(/ğ/g, 'g')
            .replace(/ı/g, 'i')
            .replace(/ö/g, 'o')
            .replace(/ş/g, 's')
            .replace(/ü/g, 'u');
    }

    function shuffle(arr) {
        for (let i = arr.length - 1; i > 0; i--) {
            const j = Math.floor(Math.random() * (i + 1));
            [arr[i], arr[j]] = [arr[j], arr[i]];
        }
        return arr;
    }

    let funTextIdx = 0;
    setInterval(() => {
        if (isSpinning) return;
        funTextIdx = (funTextIdx + 1) % FUN_TEXTS.length;
        if (funTextEl) {
            funTextEl.style.opacity = '0';
            setTimeout(() => {
                funTextEl.textContent = FUN_TEXTS[funTextIdx];
                funTextEl.style.opacity = '1';
            }, 300);
        }
    }, 4000);

    const quickBtn = document.getElementById('wte-quick-suggest');
    if (quickBtn) {
        quickBtn.addEventListener('click', () => {
            document.getElementById('wte-wheel-area')?.scrollIntoView({ behavior: 'smooth', block: 'start' });
            setTimeout(spinWheel, 300);
        });
    }

    const customBtn = document.getElementById('wte-custom-flow');
    if (customBtn) {
        customBtn.addEventListener('click', () => {
            document.getElementById('wte-chips-area')?.scrollIntoView({ behavior: 'smooth', block: 'start' });
        });
    }

    updateWheelLabels();

})();


