using Microsoft.AspNetCore.Mvc;
using MR.Abstracciones.EntidadesParaUI;
using MR.Abstracciones.LogicaDeNegocio.Productos;

namespace G9MotoRepuestos.Controllers
{
    public class InventarioController : Controller
    {
        private readonly IProductosLN _ln;

        public InventarioController(IProductosLN ln)
        {
            _ln = ln;
        }

        public IActionResult Create() => View(new ProductoDto());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductoDto model)
        {
            if (!ModelState.IsValid) return View(model);

            await _ln.CrearAsync(model);
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Index()
        {
            // por ahora puede estar quemado, no llames ListarAsync aún
            return View();
        }
    }
}
