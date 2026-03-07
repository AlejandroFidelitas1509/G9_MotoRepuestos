using G9MotoRepuestos.Data;
using G9MotoRepuestos.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MR.Abstracciones.LogicaDeNegocio.Categorias;
using MR.Abstracciones.LogicaDeNegocio.Productos;
using System.Diagnostics;
using System.Security.Claims;

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
        public async Task<IActionResult> PanelControl()
        {
            int ventasHoyCantidad = 0;
            decimal ventasHoyMonto = 0;

            try
            {
                await using var conn = _context.Database.GetDbConnection();
                if (conn.State != System.Data.ConnectionState.Open)
                    await conn.OpenAsync();

                // Cantidad de ventas de hoy
                await using (var cmdCantidad = conn.CreateCommand())
                {
                    cmdCantidad.CommandText = @"
                SELECT COUNT(*)
                FROM dbo.Ventas
                WHERE CAST(Fecha AS date) = CAST(GETDATE() AS date);";

                    var resultCantidad = await cmdCantidad.ExecuteScalarAsync();
                    ventasHoyCantidad = resultCantidad != null ? Convert.ToInt32(resultCantidad) : 0;
                }

                // Monto total vendido hoy
                await using (var cmdMonto = conn.CreateCommand())
                {
                    cmdMonto.CommandText = @"
                SELECT ISNULL(SUM(Total), 0)
                FROM dbo.Ventas
                WHERE CAST(Fecha AS date) = CAST(GETDATE() AS date);";

                    var resultMonto = await cmdMonto.ExecuteScalarAsync();
                    ventasHoyMonto = resultMonto != null ? Convert.ToDecimal(resultMonto) : 0;
                }
            }
            catch
            {
                ventasHoyCantidad = 0;
                ventasHoyMonto = 0;
            }

            ViewBag.VentasHoyCantidad = ventasHoyCantidad;
            ViewBag.VentasHoyMonto = ventasHoyMonto;

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