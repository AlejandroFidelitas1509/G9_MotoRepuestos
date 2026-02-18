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

        public async Task<IActionResult> Index(bool soloActivas = true)
        {
            var data = await _ln.ListarAsync(soloActivas);
            ViewBag.SoloActivas = soloActivas;
            return View(data);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new CategoriaDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoriaDto model)
        {
            if (!ModelState.IsValid)
                return View(model);

            await _ln.CrearAsync(model);
            return RedirectToAction(nameof(Index));
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
            return RedirectToAction(nameof(Index), new { soloActivas = false });
        }
    }
}
