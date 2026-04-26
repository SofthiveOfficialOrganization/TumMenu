// beqr.js - QR Landing Page Interactive Functionality

document.addEventListener('DOMContentLoaded', function () {

    // ========================
    // REAL QR CODE WITH BUILD ANIMATION
    // ========================
    const initRealQRCode = () => {
        const qrContainer = document.getElementById('qr-real-code');
        const phoneOverlay = document.getElementById('qr-phone-overlay');
        const menuPreview = document.getElementById('qr-menu-preview');
        const scanTrigger = document.getElementById('qr-scan-trigger');
        const touchHint = document.getElementById('qr-touch-hint');
        const qrCodeElement = document.querySelector('.qr-hero__qr-code');

        if (!qrContainer) return;

        // Real QR Code URL
        const qrUrl = 'https://tummenu.com.tr';

        // Create build overlay for animation
        const buildOverlay = document.createElement('div');
        buildOverlay.className = 'qr-build-overlay';

        // Create 21x21 grid of cells for build animation
        const gridSize = 21;
        const cells = [];
        for (let i = 0; i < gridSize * gridSize; i++) {
            const cell = document.createElement('div');
            cell.className = 'qr-build-cell';
            buildOverlay.appendChild(cell);
            cells.push(cell);
        }
        qrContainer.appendChild(buildOverlay);

        // Generate real QR code (hidden initially)
        const qrInstance = new QRCode(qrContainer, {
            text: qrUrl,
            width: 200,
            height: 200,
            colorDark: '#688745',
            colorLight: '#ffffff',
            correctLevel: QRCode.CorrectLevel.H
        });

        // QR Build Animation - cells appear randomly then fade, revealing real QR
        const animateBuild = () => {
            // Shuffle cells for random animation order
            const shuffledCells = [...cells].sort(() => Math.random() - 0.5);
            const totalDuration = 1800; // ms
            const delayBetweenCells = totalDuration / shuffledCells.length;

            // Animate each cell with stagger
            shuffledCells.forEach((cell, index) => {
                setTimeout(() => {
                    cell.classList.add('animate');
                }, index * delayBetweenCells);
            });

            // After animation, show real QR code and hide overlay
            setTimeout(() => {
                qrContainer.classList.add('qr-built');
                buildOverlay.style.opacity = '0';
                setTimeout(() => {
                    buildOverlay.remove();
                }, 500);
            }, totalDuration + 300);
        };

        // Start build animation after short delay
        setTimeout(animateBuild, 300);

        // ========================
        // PHONE SCAN SIMULATION
        // ========================
        let isScanning = false;
        let scanTimeout = null;
        let resetTimeout = null;

        const startScanSimulation = () => {
            if (isScanning) return;
            isScanning = true;

            // Clear any pending resets
            if (resetTimeout) {
                clearTimeout(resetTimeout);
                resetTimeout = null;
            }

            // Hide touch hint
            if (touchHint) touchHint.style.opacity = '0';

            // Show phone overlay
            phoneOverlay.classList.add('active');

            // Start scanning after phone appears
            setTimeout(() => {
                phoneOverlay.classList.add('scanning');
            }, 400);

            // After scan completes, show menu preview
            scanTimeout = setTimeout(() => {
                phoneOverlay.classList.remove('scanning');
                phoneOverlay.classList.remove('active');
                qrCodeElement.classList.add('scanned');
                menuPreview.classList.add('visible');
            }, 2800); // 400ms delay + 2s scan + 400ms buffer
        };

        const resetScanSimulation = () => {
            resetTimeout = setTimeout(() => {
                isScanning = false;
                phoneOverlay.classList.remove('active', 'scanning');
                qrCodeElement.classList.remove('scanned');
                menuPreview.classList.remove('visible');

                if (touchHint) touchHint.style.opacity = '1';

                if (scanTimeout) {
                    clearTimeout(scanTimeout);
                    scanTimeout = null;
                }
            }, 500);
        };

        // Desktop: hover trigger
        if (window.innerWidth > 768) {
            scanTrigger.addEventListener('mouseenter', startScanSimulation);
            scanTrigger.addEventListener('mouseleave', resetScanSimulation);
        }

        // Mobile: click/touch trigger
        scanTrigger.addEventListener('click', (e) => {
            if (window.innerWidth <= 768) {
                if (isScanning) {
                    // If already scanning, reset
                    if (resetTimeout) clearTimeout(resetTimeout);
                    resetScanSimulation();
                } else {
                    startScanSimulation();
                    // Auto reset after showing menu preview
                    setTimeout(() => {
                        resetScanSimulation();
                    }, 4500);
                }
            }
        });

        // Touch hint click also triggers
        if (touchHint) {
            touchHint.addEventListener('click', (e) => {
                e.stopPropagation();
                if (!isScanning) {
                    startScanSimulation();
                    setTimeout(() => {
                        resetScanSimulation();
                    }, 4500);
                }
            });
        }
    };

    // Initialize real QR code with animation
    initRealQRCode();


    // ========================
    // HERO STATS COUNTER ANIMATION
    // ========================
    const animateCounter = (element, target, duration = 2000) => {
        const start = 0;
        const increment = target / (duration / 16);
        let current = start;

        const timer = setInterval(() => {
            current += increment;
            if (current >= target) {
                element.textContent = target.toLocaleString('tr-TR');
                clearInterval(timer);
            } else {
                element.textContent = Math.floor(current).toLocaleString('tr-TR');
            }
        }, 16);
    };

    // Animate hero stats on page load
    const heroStats = document.querySelectorAll('.qr-hero__stat-number');
    setTimeout(() => {
        heroStats.forEach(stat => {
            const target = parseInt(stat.getAttribute('data-target'));
            animateCounter(stat, target);
        });
    }, 500);


    // ========================
    // QR CODE GENERATOR
    // ========================
    const businessNameInput = document.getElementById('business-name');
    const generateBtn = document.getElementById('generate-qr-btn');
    const qrOutput = document.getElementById('qr-code-output');
    const qrActions = document.getElementById('qr-actions');
    const downloadBtn = document.getElementById('download-qr-btn');
    const demoBtn = document.getElementById('try-demo-btn');

    let qrCodeInstance = null;

    const generateQRCode = () => {
        const businessName = businessNameInput.value.trim();

        if (!businessName) {
            alert('Lütfen bir işletme adı girin!');
            businessNameInput.focus();
            return;
        }

        // Clear previous QR code
        qrOutput.innerHTML = '';
        qrOutput.classList.add('has-qr');

        // Generate QR code URL (you can customize this)
        const qrUrl = `https://tummenu.com/restoran/${encodeURIComponent(businessName.toLowerCase().replace(/\s+/g, '-'))}`;

        // Create QR code
        qrCodeInstance = new QRCode(qrOutput, {
            text: qrUrl,
            width: 220,
            height: 220,
            colorDark: '#688745',
            colorLight: '#ffffff',
            correctLevel: QRCode.CorrectLevel.H
        });

        // Show download/demo buttons
        qrActions.style.display = 'flex';

        // Add animation
        qrOutput.style.animation = 'none';
        setTimeout(() => {
            qrOutput.style.animation = 'slideIn 0.5s ease-out';
        }, 10);
    };

    // Generate QR on button click
    if (generateBtn) {
        generateBtn.addEventListener('click', generateQRCode);
    }

    // Generate QR on Enter key
    if (businessNameInput) {
        businessNameInput.addEventListener('keypress', (e) => {
            if (e.key === 'Enter') {
                e.preventDefault();
                generateQRCode();
            }
        });
    }

    // Download QR code
    if (downloadBtn) {
        downloadBtn.addEventListener('click', () => {
            if (!qrCodeInstance) return;

            const canvas = qrOutput.querySelector('canvas');
            if (canvas) {
                const link = document.createElement('a');
                link.download = `${businessNameInput.value.trim()}-qr-kod.png`;
                link.href = canvas.toDataURL('image/png');
                link.click();
            }
        });
    }

    // Demo button
    if (demoBtn) {
        demoBtn.addEventListener('click', () => {
            alert('Demo özelliği yakında aktif olacak!\n\nŞimdilik QR kodunuzu indirebilir ve test edebilirsiniz.');
        });
    }


    // ========================
    // STATISTICS SECTION - SCROLL ANIMATION
    // ========================
    const statCards = document.querySelectorAll('.stat-card__number');

    const observerOptions = {
        threshold: 0.5,
        rootMargin: '0px'
    };

    const statsObserver = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting && !entry.target.classList.contains('animated')) {
                const target = parseInt(entry.target.getAttribute('data-count'));
                animateCounter(entry.target, target, 2500);
                entry.target.classList.add('animated');
            }
        });
    }, observerOptions);

    statCards.forEach(card => {
        statsObserver.observe(card);
    });


    // ========================
    // CAROUSEL FUNCTIONALITY
    // ========================
    const carousel = document.querySelector('.carousel');
    if (carousel) {
        const slides = carousel.querySelectorAll('.carousel__slide');
        const prevBtn = carousel.querySelector('.carousel__btn--prev');
        const nextBtn = carousel.querySelector('.carousel__btn--next');
        const indicators = carousel.querySelectorAll('.carousel__indicator');

        let currentSlide = 0;
        const totalSlides = slides.length;
        let autoplayInterval;

        const goToSlide = (index) => {
            // Remove active class from all slides and indicators
            slides.forEach(slide => slide.classList.remove('carousel__slide--active'));
            indicators.forEach(indicator => indicator.classList.remove('carousel__indicator--active'));

            // Add active class to current slide and indicator
            slides[index].classList.add('carousel__slide--active');
            indicators[index].classList.add('carousel__indicator--active');

            currentSlide = index;
        };

        const nextSlide = () => {
            const next = (currentSlide + 1) % totalSlides;
            goToSlide(next);
        };

        const prevSlide = () => {
            const prev = (currentSlide - 1 + totalSlides) % totalSlides;
            goToSlide(prev);
        };

        // Next button
        if (nextBtn) {
            nextBtn.addEventListener('click', () => {
                nextSlide();
                resetAutoplay();
            });
        }

        // Previous button
        if (prevBtn) {
            prevBtn.addEventListener('click', () => {
                prevSlide();
                resetAutoplay();
            });
        }

        // Indicator clicks
        indicators.forEach((indicator, index) => {
            indicator.addEventListener('click', () => {
                goToSlide(index);
                resetAutoplay();
            });
        });

        // Autoplay
        const startAutoplay = () => {
            autoplayInterval = setInterval(nextSlide, 5000);
        };

        const resetAutoplay = () => {
            clearInterval(autoplayInterval);
            startAutoplay();
        };

        // Start autoplay
        startAutoplay();

        // Pause autoplay on hover
        carousel.addEventListener('mouseenter', () => {
            clearInterval(autoplayInterval);
        });

        carousel.addEventListener('mouseleave', () => {
            startAutoplay();
        });

        // Keyboard navigation
        document.addEventListener('keydown', (e) => {
            if (e.key === 'ArrowLeft') {
                prevSlide();
                resetAutoplay();
            } else if (e.key === 'ArrowRight') {
                nextSlide();
                resetAutoplay();
            }
        });
    }


    // ========================
    // FLIP CARDS - TOUCH SUPPORT FOR MOBILE
    // ========================
    const flipCards = document.querySelectorAll('.flip-card');

    flipCards.forEach(card => {
        // Add click/tap to flip on mobile
        card.addEventListener('click', function () {
            if (window.innerWidth <= 768) {
                this.classList.toggle('flipped');
            }
        });
    });

    // Add CSS for mobile flip
    if (window.innerWidth <= 768) {
        const style = document.createElement('style');
        style.textContent = `
            .flip-card.flipped .flip-card__inner {
                transform: rotateY(180deg);
            }
        `;
        document.head.appendChild(style);
    }


    // ========================
    // SMOOTH SCROLL FOR ANCHOR LINKS
    // ========================
    document.querySelectorAll('a[href^="#"]').forEach(anchor => {
        anchor.addEventListener('click', function (e) {
            const href = this.getAttribute('href');
            if (href !== '#' && href.length > 1) {
                const target = document.querySelector(href);
                if (target) {
                    e.preventDefault();
                    target.scrollIntoView({
                        behavior: 'smooth',
                        block: 'start'
                    });
                }
            }
        });
    });


    // ========================
    // PARALLAX EFFECT FOR HERO QR CODE (Optional)
    // ========================
    const qrContainer = document.querySelector('.qr-hero__qr-container');

    if (qrContainer && window.innerWidth > 768) {
        window.addEventListener('scroll', () => {
            const scrolled = window.pageYOffset;
            const parallax = scrolled * 0.3;
            qrContainer.style.transform = `translateY(${parallax}px)`;
        });
    }

    // ========================
    // ROADMAP SVG PATH
    // ========================
    const initRoadmapPath = () => {
        const wrapper = document.getElementById('roadmapWrapper');
        const svg = document.getElementById('roadSvg');
        const roadShadow = document.getElementById('roadShadow');
        const roadWhite = document.getElementById('roadWhite');
        const roadColor = document.getElementById('roadColor');
        const dotsGroup = document.getElementById('dotsGroup');

        if (!wrapper || !svg || !roadShadow || !roadWhite || !roadColor || !dotsGroup) {
            return;
        }

        const makeSvgEl = (tag, attrs) => {
            const el = document.createElementNS('http://www.w3.org/2000/svg', tag);
            Object.entries(attrs).forEach(([key, value]) => el.setAttribute(key, value));
            return el;
        };

        const f = (value) => value.toFixed(2);

        const getCenter = (element, containerRect) => {
            const rect = element.getBoundingClientRect();
            return {
                x: rect.left - containerRect.left + (rect.width / 2),
                y: rect.top - containerRect.top + (rect.height / 2)
            };
        };

        const buildRoad = () => {
            const steps = Array.from(wrapper.querySelectorAll('.qr-roadmap__step-row'));
            const wrapperRect = wrapper.getBoundingClientRect();

            const getRect = (el) => {
                const r = el.getBoundingClientRect();
                return {
                    left:   r.left   - wrapperRect.left,
                    top:    r.top    - wrapperRect.top,
                    right:  r.right  - wrapperRect.left,
                    bottom: r.bottom - wrapperRect.top,
                    cx: r.left - wrapperRect.left + r.width  / 2,
                    cy: r.top  - wrapperRect.top  + r.height / 2
                };
            };

            const stepData = steps
                .map((step) => {
                    const block = step.querySelector('.qr-roadmap__video-block');
                    if (!block) return null;
                    return {
                        rect:   getRect(block),
                        isLeft: step.classList.contains('qr-roadmap__step-row--left'),
                        color:  getComputedStyle(step).getPropertyValue('--c').trim() || '#7f9d58'
                    };
                })
                .filter(Boolean);

            if (stepData.length < 2) return;

            svg.setAttribute('viewBox', `0 0 ${wrapper.clientWidth} ${wrapper.clientHeight}`);

            // Horizontal clearance from the video's outer edge (must be < gap 44px to stay out of text)
            const OH = 26;

            const dotPoints = [];

            // For each video: a vertical track just outside the video's outer edge.
            // topY/botY are the exact top/bottom of the video so the track runs the
            // full height of the video on that side.
            const tracks = stepData.map(({ rect, isLeft, color }) => ({
                x:    isLeft ? rect.right + OH : rect.left - OH,
                topY: rect.top,
                midY: rect.cy,
                botY: rect.bottom,
                color
            }));

            const t0 = tracks[0];

            // Start 40 px above the first video's top edge, then drop straight down
            // along its outer side.
            let pathData = `M ${f(t0.x)} ${f(t0.topY - 40)} L ${f(t0.x)} ${f(t0.botY)}`;
            dotPoints.push({ x: t0.x, y: t0.midY, color: t0.color });

            for (let i = 1; i < tracks.length; i++) {
                const prev = tracks[i - 1];
                const curr = tracks[i];

                // Gap between bottom of video N and top of video N+1.
                // The S-curve lives entirely inside this vertical gap, so it
                // never overlaps either video.
                const gap     = curr.topY - prev.botY;
                const tension = Math.max(gap * 0.45, 30);

                // Cubic bezier: vertical tangent at both ends ensures a smooth
                // join with the straight L segments above and below.
                pathData += ` C ${f(prev.x)} ${f(prev.botY + tension)},`
                          + ` ${f(curr.x)} ${f(curr.topY - tension)},`
                          + ` ${f(curr.x)} ${f(curr.topY)}`;

                // Straight down along this video's outer side
                pathData += ` L ${f(curr.x)} ${f(curr.botY)}`;

                dotPoints.push({ x: curr.x, y: curr.midY, color: curr.color });
            }

            roadShadow.setAttribute('d', pathData);
            roadWhite.setAttribute('d', pathData);
            roadColor.setAttribute('d', pathData);

            dotsGroup.innerHTML = '';
            dotPoints.forEach(({ x, y, color }) => {
                dotsGroup.appendChild(makeSvgEl('circle', {
                    cx: x, cy: y, r: 20,
                    fill: color, opacity: '0.18'
                }));
                dotsGroup.appendChild(makeSvgEl('circle', {
                    cx: x, cy: y, r: 13,
                    fill: color, stroke: 'white', 'stroke-width': '3.5'
                }));
                dotsGroup.appendChild(makeSvgEl('circle', {
                    cx: x, cy: y, r: 4.5,
                    fill: 'white'
                }));
            });
        };

        let resizeTimeoutId = null;
        const scheduleRoadBuild = () => {
            window.requestAnimationFrame(buildRoad);
        };

        scheduleRoadBuild();
        window.addEventListener('load', scheduleRoadBuild, { once: true });
        window.addEventListener('resize', () => {
            window.clearTimeout(resizeTimeoutId);
            resizeTimeoutId = window.setTimeout(scheduleRoadBuild, 60);
        });

        wrapper.querySelectorAll('video').forEach((video) => {
            video.addEventListener('loadedmetadata', scheduleRoadBuild, { once: true });
        });

        window.setTimeout(scheduleRoadBuild, 120);
        window.setTimeout(scheduleRoadBuild, 420);
    };

    initRoadmapPath();


    // ========================
    // LOG SUCCESS
    // ========================
    console.log('QR Landing Page initialized successfully.');
    console.log('Features loaded:');
    console.log('  - Hero stats counter animation');
    console.log('  - Interactive QR code generator');
    console.log('  - Scroll-triggered statistics');
    console.log('  - Autoplay carousel');
    console.log('  - Mobile-friendly flip cards');
    console.log('  - Smooth scrolling');

    // ========================
    // ROADMAP ANIMATION LOOP
    // ========================
    const ANIM_DURATION = 8000; // ms — matches CSS total timeline

    function restartAnimations(container) {
        const animated = container.querySelectorAll('[class*="tm-a-"]');
        animated.forEach(function (el) {
            el.style.animationName = 'none';
        });
        void container.offsetHeight;
        animated.forEach(function (el) {
            el.style.animationName = '';
        });
    }

    document.querySelectorAll('.tm-anim').forEach(function (anim) {
        setInterval(function () {
            restartAnimations(anim);
        }, ANIM_DURATION);
    });
});
