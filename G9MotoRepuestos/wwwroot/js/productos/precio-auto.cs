document.addEventListener("DOMContentLoaded", () => {
    const costoEl = document.getElementById("PrecioCosto");
    const margenEl = document.getElementById("MargenPorcentaje");
    const ventaEl = document.getElementById("PrecioVenta");

    if (!costoEl || !margenEl || !ventaEl) return;

    let lock = false;

    const toNumber = (v) => {
        if (v === null || v === undefined) return NaN;
        v = v.toString().trim();
        if (!v) return NaN;

        // soporta coma o punto
        v = v.replace(/\s / g, "").replace(",", ".");
        return parseFloat(v);
    };

    const setValue = (el, num, decimals = 2) => {
        if (!el) return;
        el.value = Number.isFinite(num) ? num.toFixed(decimals) : "";
    };

    const calcVenta = () => {
        if (lock) return;
        lock = true;

        const costo = toNumber(costoEl.value);
        const margen = toNumber(margenEl.value);

        if (Number.isFinite(costo) && Number.isFinite(margen) && costo >= 0 && margen >= 0)
        {
            const venta = costo * (1 + (margen / 100));
            setValue(ventaEl, venta, 2);
        }

        lock = false;
    };

    const calcMargen = () => {
        if (lock) return;
        lock = true;

        const costo = toNumber(costoEl.value);
        const venta = toNumber(ventaEl.value);

        // margen = ((venta/costo) - 1) * 100
        if (Number.isFinite(costo) && Number.isFinite(venta) && costo > 0 && venta >= 0)
        {
            const margen = ((venta / costo) - 1) * 100;
            setValue(margenEl, margen, 2);
        }

        lock = false;
    };

    // Eventos:
    // - Si cambias costo, recalcula según lo que haya (preferimos margen->venta si hay margen)
    costoEl.addEventListener("input", () => {
        const margen = toNumber(margenEl.value);
        if (Number.isFinite(margen)) calcVenta();
        else calcMargen();
    });

    // - Si cambias margen, siempre recalcula venta
    margenEl.addEventListener("input", calcVenta);

    // - Si cambias venta, recalcula margen
    ventaEl.addEventListener("input", calcMargen);

    // Auto-llenar margen al abrir en Edit si ya hay costo y venta
    const costoIni = toNumber(costoEl.value);
    const ventaIni = toNumber(ventaEl.value);
    const margenIni = toNumber(margenEl.value);

    if (!Number.isFinite(margenIni) && Number.isFinite(costoIni) && Number.isFinite(ventaIni) && costoIni > 0)
    {
        calcMargen();
    }
});