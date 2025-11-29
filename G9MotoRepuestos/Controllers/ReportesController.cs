using Microsoft.AspNetCore.Mvc;

namespace G9MotoRepuestos.Controllers
{
    public class ReportesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
