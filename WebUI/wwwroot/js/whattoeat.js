// ══════════════════════════════════════════════
//  NE YESEM – whattoeat.js
//  Çark, chip, konfeti, shake, bütçe
// ══════════════════════════════════════════════

(function () {
    'use strict';

    // ── Yemek veritabanı ──
    const FOODS = [
        { name: 'Margarita Pizza', emoji: '🍕', tags: ['hizli'], reason: 'Klasik bir seçim, herkesi mutlu eder!', alt: 'Sucuklu Pide' },
        { name: 'Mercimek Çorbası', emoji: '🍜', tags: ['hafif', 'vegan', 'ekonomik'], reason: 'Sıcacık, doyurucu ve bütçe dostu.', alt: 'Domates Çorbası' },
        { name: 'Tavuk Dürüm', emoji: '🌯', tags: ['protein', 'hizli'], reason: 'Hızlı ve protein dolu bir öğün.', alt: 'Adana Dürüm' },
        { name: 'Caesar Salata', emoji: '🥗', tags: ['hafif', 'protein'], reason: 'Hafif ama doyurucu, mükemmel denge.', alt: 'Ton Balıklı Salata' },
        { name: 'Cheese Burger', emoji: '🍔', tags: ['protein'], reason: 'Günün stresini bu çözer 😌', alt: 'Veggie Burger' },
        { name: 'Falafel Wrap', emoji: '🧆', tags: ['vegan', 'ekonomik'], reason: 'Vegan dostu, lezzet dolu!', alt: 'Humus Tabağı' },
        { name: 'Karnıyarık', emoji: '🍆', tags: ['protein'], reason: 'Ev yemeği özlemi? İşte burada.', alt: 'İmam Bayıldı' },
        { name: 'Sushi Set', emoji: '🍣', tags: [], reason: 'Farklı bir şey deneyelim bugün!', alt: 'Poke Bowl' },
        { name: 'Makarna', emoji: '🍝', tags: ['hizli', 'ekonomik'], reason: '10 dakikada hazır, her zaman güzel.', alt: 'Lazanya' },
        { name: 'Güveç', emoji: '🥘', tags: ['protein'], reason: 'Yavaş pişmiş, derin tat.', alt: 'Türlü' },
        { name: 'Acılı Tavuk', emoji: '🌶️', tags: ['aci', 'protein'], reason: 'Biraz ateş lazım bugün 🔥', alt: 'Acılı Kanat' },
        { name: 'Waffle', emoji: '🧇', tags: ['tatli', 'hizli'], reason: 'Tatlı krizine birebir!', alt: 'Krep' },
        { name: 'Brownie', emoji: '🍫', tags: ['tatli'], reason: 'Çikolata her şeyi çözer.', alt: 'Sufle' },
        { name: 'Smoothie Bowl', emoji: '🫐', tags: ['hafif', 'vegan'], reason: 'Sağlıklı ama lezzetli!', alt: 'Açai Bowl' },
        { name: 'Mercimek Köftesi', emoji: '🌿', tags: ['vegan', 'ekonomik', 'hafif'], reason: 'Hafif, sağlıklı, ekonomik üçlüsü.', alt: 'Kısır' },
        { name: 'Tantuni', emoji: '🌮', tags: ['aci', 'hizli', 'protein'], reason: 'Acılı, hızlı, doyurucu. Ne istersin?', alt: 'Çiğ Köfte Dürüm' },
        { name: 'Künefe', emoji: '🧀', tags: ['tatli'], reason: 'Sıcak peynirli, şerbetli mutluluk.', alt: 'Katmer' },
        { name: 'Menemen', emoji: '🍳', tags: ['ekonomik', 'hizli', 'hafif'], reason: 'Sabah akşam her zaman iyi gider.', alt: 'Omlet' },
    ];

    // ── Eğlence metinleri ──
    const FUN_TEXTS = [
        'Tam senlik bir şey buldum 😌',
        'Bunu seçersen günün toparlanır.',
        'Kararsızlık level: efsane. Hallediyoruz.',
        'Sürprize açık mısın? 🎲',
        'Bugün şansın açık! 🍀',
        'Bu üçlü seni hayal kırıklığına uğratmaz 💯',
        'Midene güveniyoruz 😎',
        'Hadi bakalım, karar zamanı! 🚀',
    ];

    // ── Bütçe önerileri ──
    const BUDGET_SUGGESTIONS = [
        {
            max: 50, items: [
                { emoji: '🍜', name: 'Mercimek Çorbası', price: '₺35' },
                { emoji: '🍳', name: 'Menemen', price: '₺40' },
                { emoji: '🌿', name: 'Mercimek Köftesi', price: '₺30' },
            ]
        },
        {
            max: 100, items: [
                { emoji: '🍕', name: 'Pizza', price: '₺80' },
                { emoji: '🌯', name: 'Tavuk Dürüm', price: '₺75' },
                { emoji: '🍝', name: 'Makarna', price: '₺65' },
            ]
        },
        {
            max: 200, items: [
                { emoji: '🍔', name: 'Cheese Burger', price: '₺120' },
                { emoji: '🥘', name: 'Güveç', price: '₺110' },
                { emoji: '🍣', name: 'Sushi Set', price: '₺180' },
            ]
        },
        {
            max: Infinity, items: [
                { emoji: '🥩', name: 'Steak', price: '₺350' },
                { emoji: '🍣', name: 'Omakase Sushi', price: '₺450' },
                { emoji: '🦞', name: 'Deniz Mahsulleri', price: '₺400' },
            ]
        },
    ];

    // ── DOM ──
    const chipWrap = document.getElementById('wte-chips-wrap');
    const wheel = document.getElementById('wte-wheel');
    const spinBtn = document.getElementById('wte-spin-btn');
    const funTextEl = document.getElementById('wte-fun-text');
    const resultsSection = document.getElementById('wte-results');
    const resultsGrid = document.getElementById('wte-results-grid');
    const resultsFun = document.getElementById('wte-results-fun');
    const refreshBtn = document.getElementById('wte-refresh-btn');
    const confettiCanvas = document.getElementById('wte-confetti-canvas');
    const budgetSlider = document.getElementById('wte-budget-slider');
    const budgetAmount = document.getElementById('wte-budget-amount');
    const budgetSuggestions = document.getElementById('wte-budget-suggestions');

    let activeTags = new Set();
    let currentRotation = 0;
    let isSpinning = false;

    // ═══════════════════
    //  CHIP SEÇİMİ
    // ═══════════════════
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

            // Pop animasyonu
            chip.style.animation = 'none';
            chip.offsetHeight; // reflow
            chip.style.animation = '';
        });
    }

    // ═══════════════════
    //  ÇARK ÇEVİRME
    // ═══════════════════
    function spinWheel() {
        if (isSpinning) return;
        isSpinning = true;

        spinBtn.classList.add('is-disabled');
        wheel.classList.add('is-spinning');

        // Eğlence metni değiştir
        funTextEl.textContent = FUN_TEXTS[Math.floor(Math.random() * FUN_TEXTS.length)];

        // Rastgele döndür (en az 5 tur + rastgele açı)
        const extraDeg = Math.floor(Math.random() * 360);
        const totalDeg = currentRotation + 1800 + extraDeg;
        currentRotation = totalDeg;

        wheel.style.transform = `rotate(${totalDeg}deg)`;

        // Döndürme bitince sonuçları göster
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

    // ═══════════════════
    //  SONUÇLARI GÖSTER
    // ═══════════════════
    function showResults() {
        let pool = [...FOODS];

        // Aktif tag'lara göre filtrele
        if (activeTags.size > 0) {
            const filtered = pool.filter(f =>
                f.tags.some(t => activeTags.has(t))
            );
            if (filtered.length >= 3) pool = filtered;
        }

        // Karıştır ve 3 seç
        shuffle(pool);
        const picks = pool.slice(0, 3);

        // Fun metin
        resultsFun.textContent = FUN_TEXTS[Math.floor(Math.random() * FUN_TEXTS.length)];

        // Grid doldur
        resultsGrid.innerHTML = picks.map((food, i) => `
            <div class="wte-result-card" style="transition-delay: ${i * 0.15}s">
                <span class="wte-result-card__emoji">${food.emoji}</span>
                <h3 class="wte-result-card__name">${food.name}</h3>
                <p class="wte-result-card__reason">
                    <strong>Neden bunu seçtin?</strong><br>${food.reason}
                </p>
                <div class="wte-result-card__actions">
                    <button class="wte-btn wte-btn--ghost" onclick="swapCard(this, '${food.alt}')">
                        🔄 Alternatif: ${food.alt}
                    </button>
                    <button class="wte-btn wte-btn--ghost" onclick="shareFood('${food.emoji} ${food.name}')">
                        📤 Arkadaşa Gönder
                    </button>
                </div>
            </div>
        `).join('');

        // Göster
        resultsSection.classList.remove('is-hidden');

        // Kartları canlandır
        requestAnimationFrame(() => {
            const cards = resultsGrid.querySelectorAll('.wte-result-card');
            cards.forEach((card, i) => {
                setTimeout(() => card.classList.add('is-visible'), i * 150);
            });
        });

        // Konfeti patlat
        fireConfetti();

        // Sonuçlara scroll et
        setTimeout(() => {
            resultsSection.scrollIntoView({ behavior: 'smooth', block: 'start' });
        }, 300);
    }

    // Kart değiştir
    window.swapCard = function (btn, altName) {
        const card = btn.closest('.wte-result-card');
        const altFood = FOODS.find(f => f.name === altName) || FOODS[Math.floor(Math.random() * FOODS.length)];

        card.classList.remove('is-visible');
        setTimeout(() => {
            card.querySelector('.wte-result-card__emoji').textContent = altFood.emoji;
            card.querySelector('.wte-result-card__name').textContent = altFood.name;
            card.querySelector('.wte-result-card__reason').innerHTML =
                `<strong>Neden bunu seçtin?</strong><br>${altFood.reason}`;

            const btns = card.querySelectorAll('.wte-btn--ghost');
            btns[0].innerHTML = `🔄 Alternatif: ${altFood.alt}`;
            btns[0].setAttribute('onclick', `swapCard(this, '${altFood.alt}')`);
            btns[1].setAttribute('onclick', `shareFood('${altFood.emoji} ${altFood.name}')`);

            card.classList.add('is-visible');
        }, 300);
    };

    // Paylaşım
    window.shareFood = function (text) {
        const shareData = {
            title: 'Ne Yesem? 🍽️',
            text: `Bugün bana önerilen: ${text}\nSen de dene → `,
            url: window.location.href,
        };

        if (navigator.share) {
            navigator.share(shareData).catch(() => { });
        } else {
            navigator.clipboard.writeText(`${shareData.text}${shareData.url}`).then(() => {
                showToast('📋 Kopyalandı!');
            });
        }
    };

    // Basit toast
    function showToast(msg) {
        const toast = document.createElement('div');
        toast.textContent = msg;
        Object.assign(toast.style, {
            position: 'fixed',
            bottom: '30px',
            left: '50%',
            transform: 'translateX(-50%)',
            background: '#333',
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

    // Yenile
    if (refreshBtn) {
        refreshBtn.addEventListener('click', () => {
            refreshBtn.classList.add('wte-shake');
            setTimeout(() => refreshBtn.classList.remove('wte-shake'), 500);
            spinWheel();
        });
    }

    // ═══════════════════
    //  KONFETİ
    // ═══════════════════
    function fireConfetti() {
        const ctx = confettiCanvas.getContext('2d');
        confettiCanvas.width = window.innerWidth;
        confettiCanvas.height = window.innerHeight;

        const colors = ['#FF6B4A', '#2DD4BF', '#A855F7', '#FACC15', '#F9A8D4', '#60A5FA'];
        const pieces = [];

        for (let i = 0; i < 120; i++) {
            pieces.push({
                x: confettiCanvas.width / 2 + (Math.random() - 0.5) * 300,
                y: confettiCanvas.height / 2,
                vx: (Math.random() - 0.5) * 12,
                vy: -Math.random() * 16 - 4,
                w: Math.random() * 8 + 4,
                h: Math.random() * 6 + 3,
                color: colors[Math.floor(Math.random() * colors.length)],
                rotation: Math.random() * 360,
                rotSpeed: (Math.random() - 0.5) * 10,
                gravity: 0.25 + Math.random() * 0.15,
                opacity: 1,
            });
        }

        let frame = 0;
        const maxFrames = 120;

        function animate() {
            if (frame > maxFrames) {
                ctx.clearRect(0, 0, confettiCanvas.width, confettiCanvas.height);
                return;
            }

            ctx.clearRect(0, 0, confettiCanvas.width, confettiCanvas.height);

            pieces.forEach(p => {
                p.x += p.vx;
                p.vy += p.gravity;
                p.y += p.vy;
                p.rotation += p.rotSpeed;
                p.opacity = Math.max(0, 1 - frame / maxFrames);

                ctx.save();
                ctx.translate(p.x, p.y);
                ctx.rotate((p.rotation * Math.PI) / 180);
                ctx.globalAlpha = p.opacity;
                ctx.fillStyle = p.color;
                ctx.fillRect(-p.w / 2, -p.h / 2, p.w, p.h);
                ctx.restore();
            });

            frame++;
            requestAnimationFrame(animate);
        }

        animate();
    }

    // ═══════════════════
    //  BÜTÇE SLIDER
    // ═══════════════════
    function updateBudget() {
        const val = parseInt(budgetSlider.value);
        budgetAmount.textContent = `₺${val}`;

        const tier = BUDGET_SUGGESTIONS.find(t => val <= t.max);
        if (tier && budgetSuggestions) {
            budgetSuggestions.innerHTML = tier.items.map(item => `
                <div class="wte-budget__suggestion">
                    <span>${item.emoji}</span>
                    <span style="flex:1">${item.name}</span>
                    <span style="font-weight:800; color: var(--wte-turquoise)">${item.price}</span>
                </div>
            `).join('');
        }
    }

    if (budgetSlider) {
        budgetSlider.addEventListener('input', updateBudget);
        updateBudget();
    }

    // ═══════════════════
    //  TELEFONU SALLA
    // ═══════════════════
    let shakeThreshold = 15;
    let lastX, lastY, lastZ;
    let lastShakeTime = 0;

    function handleMotion(event) {
        const acc = event.accelerationIncludingGravity;
        if (!acc) return;

        const now = Date.now();
        if (now - lastShakeTime < 2000) return; // 2sn cooldown

        if (lastX !== undefined) {
            const dx = Math.abs(acc.x - lastX);
            const dy = Math.abs(acc.y - lastY);
            const dz = Math.abs(acc.z - lastZ);

            if (dx + dy + dz > shakeThreshold) {
                lastShakeTime = now;
                spinWheel();
            }
        }

        lastX = acc.x;
        lastY = acc.y;
        lastZ = acc.z;
    }

    if (window.DeviceMotionEvent) {
        // iOS 13+ permisyon kontrolü
        if (typeof DeviceMotionEvent.requestPermission === 'function') {
            document.addEventListener('click', function enableShake() {
                DeviceMotionEvent.requestPermission()
                    .then(response => {
                        if (response === 'granted') {
                            window.addEventListener('devicemotion', handleMotion);
                        }
                    })
                    .catch(() => { });
                document.removeEventListener('click', enableShake);
            }, { once: true });
        } else {
            window.addEventListener('devicemotion', handleMotion);
        }
    }

    // ═══════════════════
    //  MEVSIM HİGHLIGHT
    // ═══════════════════
    function highlightSeason() {
        const month = new Date().getMonth(); // 0-11
        let season;
        if (month >= 2 && month <= 4) season = 'spring';
        else if (month >= 5 && month <= 7) season = 'summer';
        else if (month >= 8 && month <= 10) season = 'autumn';
        else season = 'winter';

        const seasonCard = document.getElementById('wte-season-card');
        if (!seasonCard) return;

        const items = seasonCard.querySelectorAll('.wte-season-item');
        items.forEach(item => {
            if (item.classList.contains(`wte-season-item--${season}`)) {
                item.style.transform = 'scale(1.05)';
                item.style.boxShadow = '0 4px 16px rgba(0,0,0,0.15)';
            } else {
                item.style.opacity = '0.6';
            }
        });
    }

    highlightSeason();

    // ═══════════════════
    //  UTILITY
    // ═══════════════════
    function shuffle(arr) {
        for (let i = arr.length - 1; i > 0; i--) {
            const j = Math.floor(Math.random() * (i + 1));
            [arr[i], arr[j]] = [arr[j], arr[i]];
        }
        return arr;
    }

    // ═══════════════════
    //  EĞLENCELİ METİN DÖNGÜSÜ
    // ═══════════════════
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

    // "Hızlı öner" butonu – tek tıkla direkt çarkı çevir
    const quickBtn = document.getElementById('wte-quick-suggest');
    if (quickBtn) {
        quickBtn.addEventListener('click', () => {
            setTimeout(spinWheel, 600);
        });
    }

})();
