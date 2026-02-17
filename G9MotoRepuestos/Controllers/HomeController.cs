using System.Diagnostics;
using G9MotoRepuestos.Models;
using G9MotoRepuestos.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

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
            var userRole = User.FindFirstValue(ClaimTypes.Role) ?? User.Claims.FirstOrDefault(c => c.Type == "Role")?.Value;

            if (userRole == "Admin" || userRole == "Administrador" || userRole == "Vendedor")
            {
                return RedirectToAction("PanelControl");
            }
            return View();
        }

        [Authorize(Roles = "Admin,Administrador,Vendedor")]
        public IActionResult PanelControl()
        {

            return View();
        }

        public IActionResult Catalogo()
        {
            return View();
        }

        public IActionResult Nosotros()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult TestConexion()
        {
            try
            {
                bool conecta = _context.Database.CanConnect();
                return Content(conecta ? "Conexión a la Base de Datos: OK ?" : "Error: No se pudo conectar a la base de datos ?");
            }
            catch (Exception ex)
            {
                return Content($"Error crítico: {ex.Message}");
            }
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