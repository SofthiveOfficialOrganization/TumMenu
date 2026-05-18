// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

(function () {
    const handleNumberInputWheel = (event) => {
        const input = event.target;
        if (
            input instanceof HTMLInputElement &&
            input.type === 'number' &&
            document.activeElement === input
        ) {
            event.preventDefault();
        }
    };

    document.addEventListener('wheel', handleNumberInputWheel, { passive: false });
})();

(function () {
    const getFallbackUrl = (trigger) => {
        const explicitFallback = trigger.getAttribute('data-fallback-url');
        if (explicitFallback) {
            return explicitFallback;
        }

        if (trigger instanceof HTMLAnchorElement && trigger.href) {
            return trigger.href;
        }

        return '/Admin';
    };

    const hasSameOriginReferrer = () => {
        if (!document.referrer) {
            return false;
        }

        try {
            return new URL(document.referrer).origin === window.location.origin;
        } catch (_) {
            return false;
        }
    };

    document.addEventListener('click', (event) => {
        const target = event.target instanceof Element ? event.target : null;
        const trigger = target?.closest('[data-tummenu-back]');
        if (!trigger) {
            return;
        }

        event.preventDefault();

        const fallbackUrl = getFallbackUrl(trigger);
        if (window.history.length > 1 && hasSameOriginReferrer()) {
            window.history.back();

            window.setTimeout(() => {
                if (!document.hidden) {
                    window.location.assign(fallbackUrl);
                }
            }, 700);
            return;
        }

        window.location.assign(fallbackUrl);
    });
})();

(function () {
    const interactiveSelector = [
        'a',
        'button',
        'input',
        'select',
        'textarea',
        'form',
        '[data-bs-toggle]',
        '[data-tummenu-click-ignore]',
        '.handle',
        '.select2-container'
    ].join(',');

    const getClickableTarget = (event) => {
        const target = event.target instanceof Element ? event.target : null;
        if (!target) {
            return null;
        }

        const clickable = target.closest('[data-tummenu-click-url]');
        if (!clickable || target.closest(interactiveSelector)) {
            return null;
        }

        const url = clickable.getAttribute('data-tummenu-click-url');
        return url ? { clickable, url } : null;
    };

    document.addEventListener('click', (event) => {
        if (event.defaultPrevented || event.button !== 0) {
            return;
        }

        const target = getClickableTarget(event);
        if (!target) {
            return;
        }

        event.preventDefault();

        if (event.ctrlKey || event.metaKey) {
            window.open(target.url, '_blank', 'noopener');
            return;
        }

        window.location.assign(target.url);
    });

    document.addEventListener('auxclick', (event) => {
        if (event.defaultPrevented || event.button !== 1) {
            return;
        }

        const target = getClickableTarget(event);
        if (!target) {
            return;
        }

        event.preventDefault();
        window.open(target.url, '_blank', 'noopener');
    });
})();

(function () {
    const selector = [
        'input[type="tel"]',
        'input[name*="Phone"]',
        'input[id*="Phone"]',
        'input[name*="phone"]',
        'input[id*="phone"]',
        'input[name*="Telefon"]',
        'input[id*="Telefon"]',
        'input[name*="telefon"]',
        'input[id*="telefon"]'
    ].join(',');

    const onlyDigits = (value) => String(value ?? '').replace(/\D/g, '');

    const countDigitsToPos = (value, pos) => onlyDigits(value.slice(0, pos)).length;

    const setCaret = (input, pos) => {
        try {
            input.setSelectionRange(pos, pos);
        } catch (_) {
            // Some input types do not support selection ranges.
        }
    };

    const caretPosFromDigitIndex = (formatted, digitIndex) => {
        if (digitIndex <= 0) {
            return 0;
        }

        let seen = 0;
        for (let i = 0; i < formatted.length; i++) {
            if (/\d/.test(formatted[i])) {
                seen++;
                if (seen === digitIndex) {
                    return i + 1;
                }
            }
        }

        return formatted.length;
    };

    const normalizeTR = (rawDigits) => {
        let digits = String(rawDigits ?? '');
        let removedPrefix = 0;
        let addedPrefix = 0;

        if (digits.startsWith('0090')) {
            removedPrefix = 4;
            digits = digits.slice(4);
        } else if (digits.startsWith('90') && digits.length >= 12) {
            removedPrefix = 2;
            digits = digits.slice(2);
        }

        if (digits.length > 0 && digits[0] !== '0') {
            addedPrefix = 1;
            digits = `0${digits}`;
        }

        return {
            digits: digits.slice(0, 11),
            removedPrefix,
            addedPrefix
        };
    };

    const formatTR = (digits) => {
        if (!digits) {
            return '';
        }

        const p0 = digits.slice(0, 1);
        const p1 = digits.slice(1, 4);
        const p2 = digits.slice(4, 7);
        const p3 = digits.slice(7, 9);
        const p4 = digits.slice(9, 11);

        let formatted = p0;

        if (digits.length > 1) formatted += ` (${p1}`;
        if (digits.length >= 4) formatted += ')';
        if (digits.length > 4) formatted += ` ${p2}`;
        if (digits.length > 7) formatted += ` ${p3}`;
        if (digits.length > 9) formatted += ` ${p4}`;

        return formatted;
    };

    const isPhoneInput = (element) => element instanceof HTMLInputElement && element.matches(selector);

    const formatInput = (input, preserveCaret = true) => {
        const previous = input.value || '';
        const caret = input.selectionStart ?? previous.length;
        let digitIndex = preserveCaret ? countDigitsToPos(previous, caret) : onlyDigits(previous).length;

        const normalized = normalizeTR(onlyDigits(previous));
        digitIndex = Math.max(0, digitIndex - normalized.removedPrefix);
        if (normalized.addedPrefix && digitIndex > 0) {
            digitIndex += normalized.addedPrefix;
        }
        digitIndex = Math.min(digitIndex, normalized.digits.length);

        const formatted = formatTR(normalized.digits);
        if (formatted !== previous) {
            input.value = formatted;
            if (preserveCaret) {
                setCaret(input, caretPosFromDigitIndex(formatted, digitIndex));
            }
        }
    };

    const handleKeydown = (event) => {
        const input = event.target;
        if (!isPhoneInput(input)) {
            return;
        }

        const value = input.value || '';
        const start = input.selectionStart ?? 0;
        const end = input.selectionEnd ?? 0;

        if (start !== end) {
            return;
        }

        if (event.key === 'Backspace') {
            let i = start - 1;
            while (i >= 0 && !/\d/.test(value[i])) i--;
            if (i !== start - 1) {
                event.preventDefault();
                setCaret(input, i + 1);
            }
        }

        if (event.key === 'Delete') {
            let i = start;
            while (i < value.length && !/\d/.test(value[i])) i++;
            if (i !== start) {
                event.preventDefault();
                setCaret(input, i);
            }
        }
    };

    const handleInput = (event) => {
        if (isPhoneInput(event.target)) {
            formatInput(event.target);
        }
    };

    const init = (root = document) => {
        root.querySelectorAll(selector).forEach((input) => {
            input.setAttribute('inputmode', 'tel');
            input.setAttribute('maxlength', '17');
            if (!input.getAttribute('autocomplete')) {
                input.setAttribute('autocomplete', 'tel');
            }
            formatInput(input, false);
        });
    };

    document.addEventListener('keydown', handleKeydown);
    document.addEventListener('input', handleInput);

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', () => init());
    } else {
        init();
    }

    window.TumMenuPhoneFormatter = {
        formatDigits: (value) => formatTR(normalizeTR(onlyDigits(value)).digits),
        init
    };
})();
