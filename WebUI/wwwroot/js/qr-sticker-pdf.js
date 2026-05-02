(function () {
    'use strict';

    const PRESETS = {
        '3x4': { w: 30, h: 40, label: '3×4 cm', cols: 6, rows: 6 },
        '4x6': { w: 40, h: 60, label: '4×6 cm', cols: 4, rows: 4 },
        '5x5': { w: 50, h: 50, label: '5×5 cm', cols: 3, rows: 5 },
        '6x9': { w: 60, h: 90, label: '6×9 cm', cols: 3, rows: 3 },
    };

    const MARGIN = 10;
    const PADDING_MM = 3;
    const DPI = 150;
    const MM_TO_PX = DPI / 25.4;

    function mmToPx(mm) {
        return Math.round(mm * MM_TO_PX);
    }

    function generateQRCanvas(url, sizePx) {
        const qr = qrcode(0, 'M');
        qr.addData(url);
        qr.make();

        const canvas = document.createElement('canvas');
        canvas.width = sizePx;
        canvas.height = sizePx;
        const ctx = canvas.getContext('2d');

        const modules = qr.getModuleCount();
        const cellSize = sizePx / modules;

        ctx.fillStyle = '#ffffff';
        ctx.fillRect(0, 0, sizePx, sizePx);

        ctx.fillStyle = '#000000';
        for (let row = 0; row < modules; row++) {
            for (let col = 0; col < modules; col++) {
                if (qr.isDark(row, col)) {
                    ctx.fillRect(
                        Math.floor(col * cellSize),
                        Math.floor(row * cellSize),
                        Math.ceil(cellSize),
                        Math.ceil(cellSize)
                    );
                }
            }
        }

        return canvas;
    }

    function fitText(ctx, text, maxWidth) {
        text = (text || '').trim();
        if (!text || ctx.measureText(text).width <= maxWidth) return text;

        const ellipsis = '...';
        while (text.length > 1 && ctx.measureText(text + ellipsis).width > maxWidth) {
            text = text.slice(0, -1).trimEnd();
        }

        return text + ellipsis;
    }

    function wrapText(ctx, text, maxWidth, maxLines) {
        text = (text || '').trim();
        if (!text) return [];

        const words = text.split(/\s+/);
        const lines = [];
        let current = '';

        words.forEach((word) => {
            const next = current ? `${current} ${word}` : word;
            if (ctx.measureText(next).width <= maxWidth) {
                current = next;
                return;
            }

            if (current) lines.push(current);
            current = word;
        });

        if (current) lines.push(current);

        if (lines.length > maxLines) {
            const visibleLines = lines.slice(0, maxLines);
            visibleLines[maxLines - 1] = fitText(ctx, visibleLines[maxLines - 1], maxWidth);
            return visibleLines;
        }

        return lines.map((line) => fitText(ctx, line, maxWidth));
    }

    function fitFontToLongestWord(ctx, text, maxWidth, initialFontPx, minFontPx, fontWeight) {
        const longestWord = (text || '')
            .trim()
            .split(/\s+/)
            .reduce((longest, word) => word.length > longest.length ? word : longest, '');
        let fontPx = initialFontPx;

        while (fontPx > minFontPx) {
            ctx.font = `${fontWeight ? fontWeight + ' ' : ''}${fontPx}px Arial, sans-serif`;
            if (!longestWord || ctx.measureText(longestWord).width <= maxWidth) break;
            fontPx -= 1;
        }

        return fontPx;
    }

    function buildStickerCanvas(qrUrl, storeTitle, companyTitle, widthMm, heightMm, includeLabels) {
        const widthPx = mmToPx(widthMm);
        const heightPx = mmToPx(heightMm);
        const paddingPx = mmToPx(PADDING_MM);

        const canvas = document.createElement('canvas');
        canvas.width = widthPx;
        canvas.height = heightPx;
        const ctx = canvas.getContext('2d');

        ctx.fillStyle = '#ffffff';
        ctx.fillRect(0, 0, widthPx, heightPx);

        const textMaxWidth = widthPx - paddingPx * 2;
        const hasCompanyTitle = includeLabels && Boolean((companyTitle || '').trim());
        const storeInitialFontPx = includeLabels ? Math.max(10, Math.min(22, Math.round(widthPx / 9))) : 0;
        const storeFontPx = includeLabels
            ? fitFontToLongestWord(ctx, storeTitle || '', textMaxWidth, storeInitialFontPx, 8, 'bold')
            : 0;
        const companyInitialFontPx = includeLabels ? Math.max(8, Math.round(storeFontPx * 0.72)) : 0;
        const companyFontPx = includeLabels
            ? fitFontToLongestWord(ctx, companyTitle || '', textMaxWidth, companyInitialFontPx, 7, '')
            : 0;
        const lineGapPx = includeLabels ? Math.max(3, Math.round(storeFontPx * 0.28)) : 0;
        const storeLineHeightPx = includeLabels ? Math.round(storeFontPx * 1.05) : 0;
        const companyLineHeightPx = includeLabels ? Math.round(companyFontPx * 1.05) : 0;
        const storeMaxLines = 2;
        const storeTextAreaPx = includeLabels ? storeLineHeightPx * storeMaxLines : 0;
        const companyTextAreaPx = hasCompanyTitle ? companyLineHeightPx : 0;
        const labelAreaPx = includeLabels
            ? storeTextAreaPx + companyTextAreaPx + lineGapPx + (hasCompanyTitle ? lineGapPx : 0)
            : 0;
        const availableQrHeightPx = heightPx - paddingPx * 2 - labelAreaPx;
        const qrSizePx = Math.max(1, Math.min(widthPx - paddingPx * 2, availableQrHeightPx));
        const qrCanvas = generateQRCanvas(qrUrl, qrSizePx);
        const qrX = Math.round((widthPx - qrSizePx) / 2);
        const qrY = includeLabels ? paddingPx : Math.round((heightPx - qrSizePx) / 2);

        ctx.drawImage(qrCanvas, qrX, qrY, qrSizePx, qrSizePx);

        if (!includeLabels) {
            return canvas;
        }

        let textY = qrY + qrSizePx + lineGapPx;

        ctx.textAlign = 'center';
        ctx.textBaseline = 'top';

        ctx.fillStyle = '#1f2937';
        ctx.font = `bold ${storeFontPx}px Arial, sans-serif`;
        wrapText(ctx, storeTitle || '', textMaxWidth, storeMaxLines).forEach((line) => {
            ctx.fillText(line, widthPx / 2, textY);
            textY += storeLineHeightPx;
        });

        if (hasCompanyTitle) {
            textY += lineGapPx;
            ctx.fillStyle = '#6b7280';
            ctx.font = `${companyFontPx}px Arial, sans-serif`;
            ctx.fillText(fitText(ctx, companyTitle, textMaxWidth), widthPx / 2, textY);
        }

        return canvas;
    }

    function generate({ qrUrl, storeTitle, companyTitle, presetKey, includeLabels = true }) {
        const preset = PRESETS[presetKey];
        if (!preset) throw new Error('Geçersiz preset: ' + presetKey);

        const { jsPDF } = window.jspdf;
        const doc = new jsPDF({ unit: 'mm', format: 'a4', orientation: 'portrait' });

        const stickerDataUrl = buildStickerCanvas(
            qrUrl, storeTitle, companyTitle, preset.w, preset.h, includeLabels
        ).toDataURL('image/png');

        for (let row = 0; row < preset.rows; row++) {
            for (let col = 0; col < preset.cols; col++) {
                const x = MARGIN + col * preset.w;
                const y = MARGIN + row * preset.h;
                doc.addImage(stickerDataUrl, 'PNG', x, y, preset.w, preset.h);
            }
        }

        const safeName = (storeTitle || 'qr')
            .replace(/[^\w\s-]/g, '')
            .trim()
            .replace(/\s+/g, '-')
            .toLowerCase()
            .substring(0, 40);
        const labelSuffix = includeLabels ? 'isimli' : 'isimsiz';
        doc.save(`${safeName}-cikartma-${presetKey}-${labelSuffix}.pdf`);
    }

    function getPresets() {
        return PRESETS;
    }

    window.QRStickerPDF = { generate, getPresets };
})();
