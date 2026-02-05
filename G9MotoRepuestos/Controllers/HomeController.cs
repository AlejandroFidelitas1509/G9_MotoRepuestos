using System.Diagnostics;
using G9MotoRepuestos.Models;
using G9MotoRepuestos.Data;
using Microsoft.AspNetCore.Mvc;

namespace G9MotoRepuestos.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(
            ILogger<HomeController> logger,
            ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Nosotros()
        {
            return View();
        }

        // ?? PRUEBA DE CONEXIÓN
        public IActionResult TestConexion()
        {
            bool conecta = _context.Database.CanConnect();
            return Content(conecta ? "Conexión OK ?" : "No conecta ?");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}
