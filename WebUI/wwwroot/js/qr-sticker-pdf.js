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
        if (ctx.measureText(text).width <= maxWidth) return text;
        const ellipsis = '…';
        while (text.length > 1 && ctx.measureText(text + ellipsis).width > maxWidth) {
            text = text.slice(0, -1);
        }
        return text + ellipsis;
    }

    function buildStickerCanvas(qrUrl, storeTitle, companyTitle, widthMm, heightMm) {
        const widthPx = mmToPx(widthMm);
        const heightPx = mmToPx(heightMm);
        const paddingPx = mmToPx(PADDING_MM);

        const qrSizePx = widthPx - paddingPx * 2;
        const qrCanvas = generateQRCanvas(qrUrl, qrSizePx);

        const canvas = document.createElement('canvas');
        canvas.width = widthPx;
        canvas.height = heightPx;
        const ctx = canvas.getContext('2d');

        ctx.fillStyle = '#ffffff';
        ctx.fillRect(0, 0, widthPx, heightPx);

        ctx.drawImage(qrCanvas, paddingPx, paddingPx, qrSizePx, qrSizePx);

        const storeFontPx = Math.max(12, Math.round(widthPx / 7));
        const companyFontPx = Math.max(10, storeFontPx - 3);
        const lineGapPx = Math.round(storeFontPx * 0.4);

        const textMaxWidth = widthPx - paddingPx * 2;
        let textY = paddingPx + qrSizePx + lineGapPx + storeFontPx;

        ctx.textAlign = 'center';
        ctx.textBaseline = 'alphabetic';

        ctx.fillStyle = '#1f2937';
        ctx.font = `bold ${storeFontPx}px Arial, sans-serif`;
        ctx.fillText(fitText(ctx, storeTitle || '', textMaxWidth), widthPx / 2, textY);

        if (companyTitle) {
            ctx.fillStyle = '#6b7280';
            ctx.font = `${companyFontPx}px Arial, sans-serif`;
            ctx.fillText(fitText(ctx, companyTitle, textMaxWidth), widthPx / 2, textY + storeFontPx + lineGapPx);
        }

        return canvas;
    }

    function generate({ qrUrl, storeTitle, companyTitle, presetKey }) {
        const preset = PRESETS[presetKey];
        if (!preset) throw new Error('Geçersiz preset: ' + presetKey);

        const { jsPDF } = window.jspdf;
        const doc = new jsPDF({ unit: 'mm', format: 'a4', orientation: 'portrait' });

        const stickerDataUrl = buildStickerCanvas(
            qrUrl, storeTitle, companyTitle, preset.w, preset.h
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
        doc.save(`${safeName}-cikartma-${presetKey}.pdf`);
    }

    function getPresets() {
        return PRESETS;
    }

    window.QRStickerPDF = { generate, getPresets };
})();
