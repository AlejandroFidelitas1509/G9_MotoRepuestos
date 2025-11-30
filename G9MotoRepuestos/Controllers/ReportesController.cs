using Microsoft.AspNetCore.Mvc;

namespace G9MotoRepuestos.Controllers
{
    public class ReportesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult RepInventario()
        {
            return View();
        }

        public IActionResult RepUsuarios()
        {
            return View();
        }
        public IActionResult RepVentas()
        {
            return View();
        }
        public IActionResult RepServicios()
        {
            return View();
        }
        public IActionResult RepCitas()
        {
            return View();
        }
    }
}
