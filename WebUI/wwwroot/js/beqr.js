// beqr.js - QR Landing Page Interactive Functionality

document.addEventListener('DOMContentLoaded', function () {

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
            alert('Demo özelliği yakında aktif olacak! 🚀\n\nŞimdilik QR kodunuzu indirebilir ve test edebilirsiniz.');
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
    // LOG SUCCESS
    // ========================
    console.log('🎉 QR Landing Page initialized successfully!');
    console.log('✨ Features loaded:');
    console.log('  - Hero stats counter animation');
    console.log('  - Interactive QR code generator');
    console.log('  - Scroll-triggered statistics');
    console.log('  - Autoplay carousel');
    console.log('  - Mobile-friendly flip cards');
    console.log('  - Smooth scrolling');
});
