using Microsoft.AspNetCore.Mvc;
using MR.Abstracciones.EntidadesParaUI;
using MR.Abstracciones.LogicaDeNegocio.Productos;
using MR.Abstracciones.LogicaDeNegocio.Bitacora;

namespace G9MotoRepuestos.Controllers
{
    public class InventarioController : Controller
    {
        private readonly IProductosLN _ln;
        private readonly IBitacoraProductosLN _bitacora;

        public InventarioController(
            IProductosLN ln,
            IBitacoraProductosLN bitacora)
        {
            _ln = ln;
            _bitacora = bitacora;
        }

        public async Task<IActionResult> Index()
        {
            var data = await _ln.ListarAsync(soloActivos: true);
            return View(data);
        }

        public IActionResult Create()
        {
            return View(new ProductoDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductoDto model)
        {
            if (!ModelState.IsValid)
                return View(model);

            await _ln.CrearAsync(model);


            await _bitacora.RegistrarAsync("CREAR", "Productos", null);

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var model = await _ln.ObtenerPorIdAsync(id);
            if (model == null)
                return NotFound();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProductoDto model)
        {
            if (!ModelState.IsValid)
                return View(model);

            await _ln.ActualizarAsync(model);


            await _bitacora.RegistrarAsync("EDITAR", "Productos", null);

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _ln.DesactivarAsync(id);


            await _bitacora.RegistrarAsync("ELIMINAR", "Productos", null);

            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> BitacoraEventos()
        {
            var data = await _bitacora.ListarAsync(200);
            return View(data);
        }
    }
}
