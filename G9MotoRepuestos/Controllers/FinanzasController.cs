using G9MotoRepuestos.Data;
using G9MotoRepuestos.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace G9MotoRepuestos.Controllers
{
    public class FinanzasController : Controller
    {
        private readonly ApplicationDbContext _db;

        public FinanzasController(ApplicationDbContext db)
        {
            _db = db;
        }

        // LISTA
        public async Task<IActionResult> Index()
        {
            var lista = await _db.Finanzas
                .OrderByDescending(x => x.Fecha)
                .ThenByDescending(x => x.IdFinanzas)
                .Take(200)
                .ToListAsync();

            return View(lista);
        }

        // CREATE GET
        public IActionResult Create()
        {
            return View(new Finanzas { Fecha = DateTime.Today });
        }

        // CREATE POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Finanzas model)
        {
            if (!ModelState.IsValid) return View(model);

            // ✅ BLOQUEO: no permitir si el periodo está cerrado
            if (await EstaEnPeriodoCerrado(model.Fecha))
            {
                ModelState.AddModelError("", "No se puede registrar movimientos: el período ya cuenta con un cierre contable.");
                return View(model);
            }

            _db.Finanzas.Add(model);
            await _db.SaveChangesAsync();

            TempData["Ok"] = "Movimiento registrado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // EDIT GET
        public async Task<IActionResult> Edit(int id)
        {
            var mov = await _db.Finanzas.FindAsync(id);
            if (mov == null) return NotFound();

            // ✅ BLOQUEO
            if (await EstaEnPeriodoCerrado(mov.Fecha))
            {
                TempData["Error"] = "No se puede editar: el período ya cuenta con un cierre contable.";
                return RedirectToAction(nameof(Index));
            }

            return View(mov);
        }

        // EDIT POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Finanzas model)
        {
            if (!ModelState.IsValid) return View(model);

            var mov = await _db.Finanzas.FindAsync(model.IdFinanzas);
            if (mov == null) return NotFound();

            // ✅ BLOQUEO (con fecha anterior y nueva)
            if (await EstaEnPeriodoCerrado(mov.Fecha) || await EstaEnPeriodoCerrado(model.Fecha))
            {
                ModelState.AddModelError("", "No se puede editar: el período ya cuenta con un cierre contable.");
                return View(model);
            }

            mov.Tipo = model.Tipo;
            mov.Monto = model.Monto;
            mov.Fecha = model.Fecha;
            mov.Categoria = model.Categoria;
            mov.Descripcion = model.Descripcion;

            await _db.SaveChangesAsync();
            TempData["Ok"] = "Movimiento actualizado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // DELETE GET (confirmación)
        public async Task<IActionResult> Delete(int id)
        {
            var mov = await _db.Finanzas.FindAsync(id);
            if (mov == null) return NotFound();

            // ✅ BLOQUEO
            if (await EstaEnPeriodoCerrado(mov.Fecha))
            {
                TempData["Error"] = "No se puede eliminar: el período ya cuenta con un cierre contable.";
                return RedirectToAction(nameof(Index));
            }

            return View(mov);
        }

        // DELETE POST
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirm(int id)
        {
            var mov = await _db.Finanzas.FindAsync(id);
            if (mov == null) return NotFound();

            // ✅ BLOQUEO
            if (await EstaEnPeriodoCerrado(mov.Fecha))
            {
                TempData["Error"] = "No se puede eliminar: el período ya cuenta con un cierre contable.";
                return RedirectToAction(nameof(Index));
            }

            _db.Finanzas.Remove(mov);
            await _db.SaveChangesAsync();

            TempData["Ok"] = "Movimiento eliminado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        private async Task<bool> EstaEnPeriodoCerrado(DateTime fecha)
        {
            // Si hay cualquier cierre (diario/semanal/mensual) que cubra esa fecha → bloqueado
            return await _db.Cierres.AnyAsync(c => fecha >= c.FechaInicio && fecha <= c.FechaFin);
        }
    }
}
