(function () {
    'use strict';

    const PRESETS = {
        '3x4': { w: 30, h: 40, label: '3×4 cm', cols: 6, rows: 6 },
        '4x6': { w: 40, h: 60, label: '4×6 cm', cols: 4, rows: 4 },
        '5x5': { w: 50, h: 50, label: '5×5 cm', cols: 3, rows: 5 },
        '6x9': { w: 60, h: 90, label: '6×9 cm', cols: 3, rows: 3 },
    };

    const MARGIN = 10;
    const PADDING = 3;
    const QR_CANVAS_PX = 256;

    function buildQRDataUrl(url) {
        const qr = qrcode(0, 'M');
        qr.addData(url);
        qr.make();

        const canvas = document.createElement('canvas');
        canvas.width = QR_CANVAS_PX;
        canvas.height = QR_CANVAS_PX;
        const ctx = canvas.getContext('2d');

        const modules = qr.getModuleCount();
        const cellSize = QR_CANVAS_PX / modules;

        ctx.fillStyle = '#ffffff';
        ctx.fillRect(0, 0, QR_CANVAS_PX, QR_CANVAS_PX);

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

        return canvas.toDataURL('image/png');
    }

    function truncateText(doc, text, maxWidth) {
        const ellipsis = '...';
        if (doc.getTextWidth(text) <= maxWidth) return text;
        while (text.length > 1 && doc.getTextWidth(text + ellipsis) > maxWidth) {
            text = text.slice(0, -1);
        }
        return text + ellipsis;
    }

    function generate({ qrUrl, storeTitle, companyTitle, presetKey }) {
        const preset = PRESETS[presetKey];
        if (!preset) throw new Error('Geçersiz preset: ' + presetKey);

        const { jsPDF } = window.jspdf;
        const doc = new jsPDF({ unit: 'mm', format: 'a4', orientation: 'portrait' });

        const qrDataUrl = buildQRDataUrl(qrUrl);

        const stickerW = preset.w;
        const stickerH = preset.h;
        const qrSize = stickerW - PADDING * 2;

        const storeFontSize = Math.max(6, Math.round(stickerW / 8));
        const companyFontSize = Math.max(5, storeFontSize - 1);
        const lineHeight = storeFontSize * 0.35 + 0.5;

        const textAreaH = lineHeight * 2 + 1;
        const qrAreaH = stickerH - textAreaH - PADDING * 2;
        const actualQrSize = Math.min(qrSize, qrAreaH);

        for (let row = 0; row < preset.rows; row++) {
            for (let col = 0; col < preset.cols; col++) {
                const x = MARGIN + col * stickerW;
                const y = MARGIN + row * stickerH;

                const qrX = x + PADDING + (qrSize - actualQrSize) / 2;
                const qrY = y + PADDING;
                doc.addImage(qrDataUrl, 'PNG', qrX, qrY, actualQrSize, actualQrSize);

                const textX = x + stickerW / 2;
                const storeY = qrY + actualQrSize + lineHeight;

                doc.setFont('helvetica', 'bold');
                doc.setFontSize(storeFontSize);
                doc.setTextColor(31, 41, 55);
                const storeLine = truncateText(doc, storeTitle || '', stickerW - PADDING * 2);
                doc.text(storeLine, textX, storeY, { align: 'center' });

                if (companyTitle) {
                    doc.setFont('helvetica', 'normal');
                    doc.setFontSize(companyFontSize);
                    doc.setTextColor(107, 114, 128);
                    const companyLine = truncateText(doc, companyTitle, stickerW - PADDING * 2);
                    doc.text(companyLine, textX, storeY + lineHeight, { align: 'center' });
                }
            }
        }

        const safeName = (storeTitle || 'qr').replace(/[^a-z0-9çğışöü\s-]/gi, '').trim().replace(/\s+/g, '-').toLowerCase();
        doc.save(`${safeName}-cikartma-${presetKey}.pdf`);
    }

    function getPresets() {
        return PRESETS;
    }

    window.QRStickerPDF = { generate, getPresets };
})();
