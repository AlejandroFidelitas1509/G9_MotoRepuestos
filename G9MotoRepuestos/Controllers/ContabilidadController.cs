using G9MotoRepuestos.Data;
using G9MotoRepuestos.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace G9MotoRepuestos.Controllers
{
    public class ContabilidadController : Controller
    {
        private readonly ApplicationDbContext _db;

        public ContabilidadController(ApplicationDbContext db)
        {
            _db = db;
        }

        public IActionResult Index() => View();

        public IActionResult Cierres() => View();

        public async Task<IActionResult> Historial()
        {
            var cierres = await _db.Cierres
                .OrderByDescending(x => x.FechaRegistro)
                .ToListAsync();

            return View(cierres);
        }

        // ✅ Realizar cierre (diario, semanal, mensual)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RealizarCierre(string tipo, DateTime? fecha, DateTime? fechaInicio, DateTime? fechaFin)
        {
            tipo = (tipo ?? "").Trim().ToLower();

            DateTime inicio;
            DateTime fin;

            // ✅ Validación: fecha requerida y rango
            if (tipo == "diario")
            {
                if (fecha == null)
                {
                    TempData["Error"] = "La fecha es requerida";
                    return RedirectToAction(nameof(Cierres));
                }

                inicio = fecha.Value.Date;
                fin = fecha.Value.Date.AddDays(1).AddTicks(-1);
            }
            else if (tipo == "semanal")
            {
                if (fechaInicio == null || fechaFin == null)
                {
                    TempData["Error"] = "La fecha es requerida";
                    return RedirectToAction(nameof(Cierres));
                }

                if (fechaFin.Value.Date < fechaInicio.Value.Date)
                {
                    TempData["Error"] = "El rango es inválido: la fecha final no puede ser menor que la inicial.";
                    return RedirectToAction(nameof(Cierres));
                }

                inicio = fechaInicio.Value.Date;
                fin = fechaFin.Value.Date.AddDays(1).AddTicks(-1);

                // ✅ Validación: ya tiene cierre semanal
                bool yaCerrado = await _db.Cierres.AnyAsync(c =>
                    c.Tipo == "Semanal" && c.FechaInicio == inicio && c.FechaFin == fin);

                if (yaCerrado)
                {
                    TempData["Error"] = "Ese periodo ya cuenta con un cierre semanal.";
                    return RedirectToAction(nameof(Cierres));
                }
            }
            else if (tipo == "mensual")
            {
                if (fechaInicio == null)
                {
                    TempData["Error"] = "La fecha es requerida";
                    return RedirectToAction(nameof(Cierres));
                }

                var mesSeleccionado = new DateTime(fechaInicio.Value.Year, fechaInicio.Value.Month, 1);
                var hoy = DateTime.Today;

                // ✅ Validación: no cierre futuro
                if (mesSeleccionado > new DateTime(hoy.Year, hoy.Month, 1))
                {
                    TempData["Error"] = "No es posible realizar un cierre para una fecha futura";
                    return RedirectToAction(nameof(Cierres));
                }

                inicio = mesSeleccionado.Date;
                fin = mesSeleccionado.AddMonths(1).AddTicks(-1);

                // ✅ Evitar cierre duplicado mensual
                bool yaCerrado = await _db.Cierres.AnyAsync(c =>
                    c.Tipo == "Mensual" && c.FechaInicio == inicio && c.FechaFin == fin);

                if (yaCerrado)
                {
                    TempData["Error"] = "Ese periodo ya cuenta con un cierre mensual.";
                    return RedirectToAction(nameof(Cierres));
                }
            }
            else
            {
                TempData["Error"] = "Tipo de cierre inválido.";
                return RedirectToAction(nameof(Cierres));
            }

            // ✅ Validación: debe haber movimientos
            var movimientos = await _db.Finanzas
                .Where(f => f.Fecha >= inicio && f.Fecha <= fin)
                .ToListAsync();

            if (movimientos.Count == 0)
            {
                TempData["Warning"] = "No existen ventas/movimientos para cerrar";
                return RedirectToAction(nameof(Cierres));
            }

            decimal totalIngresos = movimientos
                .Where(x => (x.Tipo ?? "").ToLower() == "ingreso")
                .Sum(x => x.Monto);

            decimal totalEgresos = movimientos
                .Where(x => (x.Tipo ?? "").ToLower() == "egreso")
                .Sum(x => x.Monto);

            decimal saldoFinal = totalIngresos - totalEgresos;

            var cierre = new Cierres
            {
                FechaInicio = inicio,
                FechaFin = fin,
                FechaRegistro = DateTime.Now,
                Tipo = tipo == "diario" ? "Diario" : tipo == "semanal" ? "Semanal" : "Mensual",
                BalanceTotal = saldoFinal,
                IdUsuario = null
            };

            _db.Cierres.Add(cierre);
            await _db.SaveChangesAsync();

            TempData["Ok"] = "Cierre realizado exitosamente.";
            return RedirectToAction(nameof(CierreResultado), new { id = cierre.IdCierres });
        }

        [HttpGet]
        public async Task<IActionResult> CierreResultado(int id)
        {
            var cierre = await _db.Cierres.FirstOrDefaultAsync(x => x.IdCierres == id);
            if (cierre == null) return RedirectToAction(nameof(Cierres));

            var movimientos = await _db.Finanzas
                .Where(f => f.Fecha >= cierre.FechaInicio && f.Fecha <= cierre.FechaFin)
                .OrderBy(f => f.Fecha)
                .ToListAsync();

            decimal totalIngresos = movimientos.Where(x => (x.Tipo ?? "").ToLower() == "ingreso").Sum(x => x.Monto);
            decimal totalEgresos = movimientos.Where(x => (x.Tipo ?? "").ToLower() == "egreso").Sum(x => x.Monto);

            ViewBag.Tipo = cierre.Tipo;
            ViewBag.FechaInicio = cierre.FechaInicio;
            ViewBag.FechaFin = cierre.FechaFin;
            ViewBag.TotalIngresos = totalIngresos;
            ViewBag.TotalEgresos = totalEgresos;
            ViewBag.SaldoFinal = cierre.BalanceTotal;

            return View(movimientos);
        }
    }
}



