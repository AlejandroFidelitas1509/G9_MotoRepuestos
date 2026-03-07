using Microsoft.AspNetCore.Mvc;
using MR.Abstracciones.EntidadesParaUI;
using MR.Abstracciones.LogicaDeNegocio.Calendarios;

namespace G9MotoRepuestos.Controllers
{
    public class BloqueosCalendarioController : Controller
    {
        private readonly IBloqueosCalendarioServicio _bloqueosCalendarioServicio;

        public BloqueosCalendarioController(IBloqueosCalendarioServicio bloqueosCalendarioServicio)
        {
            _bloqueosCalendarioServicio = bloqueosCalendarioServicio;
        }

        public async Task<IActionResult> Index()
        {
            var respuesta = await _bloqueosCalendarioServicio.ObtenerBloqueosAsync();
            return View(respuesta.Data);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BloqueosCalendarioDto dto)
        {
            if (ModelState.IsValid)
            {
                var respuesta = await _bloqueosCalendarioServicio.AgregarBloqueoAsync(dto);

                if (!respuesta.EsError)
                    return RedirectToAction(nameof(Index));

                ModelState.AddModelError(string.Empty, respuesta.Mensaje);
            }

            return View(dto);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var respuesta = await _bloqueosCalendarioServicio.ObtenerBloqueoPorIdAsync(id);

            if (respuesta.EsError || respuesta.Data == null)
                return NotFound();

            return View(respuesta.Data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, BloqueosCalendarioDto dto)
        {
            if (ModelState.IsValid)
            {
                var respuesta = await _bloqueosCalendarioServicio.ActualizarBloqueoAsync(dto);

                if (!respuesta.EsError)
                    return RedirectToAction(nameof(Index));

                ModelState.AddModelError(string.Empty, respuesta.Mensaje);
            }

            return View(dto);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var respuesta = await _bloqueosCalendarioServicio.ObtenerBloqueoPorIdAsync(id);

            if (respuesta.EsError || respuesta.Data == null)
                return NotFound();

            return View(respuesta.Data);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var respuesta = await _bloqueosCalendarioServicio.EliminarBloqueoAsync(id);

            if (!respuesta.EsError)
                return RedirectToAction(nameof(Index));

            TempData["ErrorMessage"] = respuesta.Mensaje;
            return RedirectToAction(nameof(Index));
        }
    }
}
