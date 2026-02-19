using Microsoft.AspNetCore.Mvc;
using MR.Abstracciones.EntidadesParaUI;
using MR.Abstracciones.LogicaDeNegocio.Categorias;

namespace G9MotoRepuestos.Controllers
{
    public class CategoriasController : Controller
    {
        private readonly ICategoriasLN _ln;

        public CategoriasController(ICategoriasLN ln)
        {
            _ln = ln;
        }

        public async Task<IActionResult> Index()
        {
            var data = await _ln.ListarAsync(soloActivas: false);
            return View(data);
        }

        // Ya no usamos Create GET
        [HttpGet]
        public IActionResult Create()
        {
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoriaDto model)
        {
            if (!ModelState.IsValid || string.IsNullOrWhiteSpace(model.Nombre))
            {
                TempData["CatCreateError"] = "El nombre es obligatorio.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                await _ln.CrearAsync(model);
                TempData["CatCreateOk"] = "Categoría creada correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["CatCreateError"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // ✅ EDITAR (solo nombre)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CategoriaDto model)
        {
            if (model.IdCategoria <= 0 || string.IsNullOrWhiteSpace(model.Nombre))
            {
                TempData["CatEditError"] = "Datos inválidos para editar.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                await _ln.ActualizarAsync(model);
                TempData["CatEditOk"] = "Categoría actualizada.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["CatEditError"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Desactivar(int id)
        {
            await _ln.DesactivarAsync(id);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Activar(int id)
        {
            await _ln.ActivarAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
