using Microsoft.AspNetCore.Mvc;

namespace G9MotoRepuestos.Controllers
{
    public class ServiciosController : Controller
    {
        public IActionResult GestionServicios()
        {
            return View();
        }
    }
}
