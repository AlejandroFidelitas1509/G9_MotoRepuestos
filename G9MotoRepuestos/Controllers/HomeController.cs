using System.Diagnostics;
using G9MotoRepuestos.Models;
using G9MotoRepuestos.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using MR.Abstracciones.LogicaDeNegocio.Productos;
using MR.Abstracciones.LogicaDeNegocio.Categorias;

namespace G9MotoRepuestos.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        private readonly IProductosLN _productosLN;

        private readonly ICategoriasLN _categoriasLN;

        public HomeController(
            ILogger<HomeController> logger,
            ApplicationDbContext context,
            IProductosLN productosLN,
            ICategoriasLN categoriasLN
        )
        {
            _logger = logger;
            _context = context;
            _productosLN = productosLN;
            _categoriasLN = categoriasLN;
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


        public async Task<IActionResult> Catalogo(int? categoriaId)
        {
            var categorias = await _categoriasLN.ListarAsync(true);     // solo activas
            var productos = await _productosLN.ListarAsync(true);       

            if (categoriaId.HasValue)
                productos = productos.Where(p => p.IdCategoria == categoriaId.Value);

            var vm = new CatalogoVm
            {
                Categorias = categorias,
                Productos = productos,
                CategoriaId = categoriaId
            };

            return View(vm);
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