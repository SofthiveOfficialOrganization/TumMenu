(function () {
    'use strict';

    const $ = (id) => document.getElementById(id);

    const sourceInput = $('dm-source');
    const targetInput = $('dm-target');
    const sourceStatus = $('dm-source-status');
    const targetStatus = $('dm-target-status');
    const tableSelect = $('dm-table');
    const testBtn = $('dm-test');
    const migrateBtn = $('dm-migrate');
    const runningLabel = $('dm-running');
    const resultPane = $('dm-result');

    const antiForgeryToken = () => {
        const el = document.querySelector('input[name="__RequestVerificationToken"]');
        return el ? el.value : '';
    };

    const setStatus = (el, ok, message) => {
        el.textContent = ok ? '✓ OK' : ('✗ ' + (message || 'Failed'));
        el.className = 'small mt-1 ' + (ok ? 'text-success' : 'text-danger');
    };

    const renderResult = (r) => {
        $('dm-r-read').textContent = r.totalReadFromSource;
        $('dm-r-exists').textContent = r.alreadyExistsInTarget;
        $('dm-r-inserted').textContent = r.inserted;
        $('dm-r-errors').textContent = r.skippedDueToError;

        const wrap = $('dm-r-error-wrap');
        const rows = $('dm-r-error-rows');
        rows.innerHTML = '';
        if (r.errors && r.errors.length > 0) {
            wrap.classList.remove('d-none');
            for (const e of r.errors) {
                const tr = document.createElement('tr');
                const idTd = document.createElement('td');
                idTd.textContent = e.id;
                const msgTd = document.createElement('td');
                msgTd.textContent = e.message;
                tr.appendChild(idTd);
                tr.appendChild(msgTd);
                rows.appendChild(tr);
            }
        } else {
            wrap.classList.add('d-none');
        }

        resultPane.classList.remove('d-none');
    };

    const postJson = async (url, body) => {
        const resp = await fetch(url, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': antiForgeryToken()
            },
            body: JSON.stringify(body)
        });
        if (!resp.ok) {
            const text = await resp.text();
            throw new Error(text || ('HTTP ' + resp.status));
        }
        return await resp.json();
    };

    const loadTables = async () => {
        const resp = await fetch('/Admin/DataMigration/Tables');
        if (!resp.ok) return;
        const tables = await resp.json();
        tableSelect.innerHTML = '<option value="">— Tablo seçin —</option>';
        for (const t of tables) {
            const opt = document.createElement('option');
            opt.value = t.name;
            opt.textContent = t.name;
            tableSelect.appendChild(opt);
        }
    };

    testBtn.addEventListener('click', async () => {
        sourceStatus.textContent = '';
        targetStatus.textContent = '';
        tableSelect.disabled = true;
        migrateBtn.disabled = true;

        testBtn.disabled = true;
        try {
            const result = await postJson('/Admin/DataMigration/TestConnections', {
                source: sourceInput.value,
                target: targetInput.value
            });

            setStatus(sourceStatus, result.sourceOk, result.sourceError);
            setStatus(targetStatus, result.targetOk, result.targetError);

            if (result.sourceOk && result.targetOk) {
                await loadTables();
                tableSelect.disabled = false;
            }
        } catch (err) {
            setStatus(sourceStatus, false, err.message);
            setStatus(targetStatus, false, err.message);
        } finally {
            testBtn.disabled = false;
        }
    });

    tableSelect.addEventListener('change', () => {
        migrateBtn.disabled = !tableSelect.value;
    });

    migrateBtn.addEventListener('click', async () => {
        const table = tableSelect.value;
        if (!table) return;

        if (!confirm(table + ' tablosunu source → target yönünde aktarmak üzeresiniz. Devam?')) return;

        migrateBtn.disabled = true;
        testBtn.disabled = true;
        runningLabel.classList.remove('d-none');
        resultPane.classList.add('d-none');

        try {
            const result = await postJson('/Admin/DataMigration/Migrate', {
                source: sourceInput.value,
                target: targetInput.value,
                tableName: table
            });
            renderResult(result);
        } catch (err) {
            alert('Aktarım hatası: ' + err.message);
        } finally {
            runningLabel.classList.add('d-none');
            migrateBtn.disabled = false;
            testBtn.disabled = false;
        }
    });
})();
