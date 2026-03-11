using G9MotoRepuestos.Data;
using G9MotoRepuestos.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MR.Abstracciones.LogicaDeNegocio.Categorias;
using MR.Abstracciones.LogicaDeNegocio.Productos;
using System.Data;
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
            ICategoriasLN categoriasLN)
        {
            _logger = logger;
            _context = context;
            _productosLN = productosLN;
            _categoriasLN = categoriasLN;
        }

        public IActionResult Index()
        {
            var userRole = User.FindFirstValue(ClaimTypes.Role)
                          ?? User.Claims.FirstOrDefault(c => c.Type == "Role")?.Value;

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
            int serviciosActivos = 0;
            int serviciosTotales = 0;
            int usuariosTotales = 0;
            int usuariosActivos = 0;

            try
            {
                serviciosTotales = await _context.Servicios.CountAsync();
                serviciosActivos = await _context.Servicios.CountAsync(s => s.Estado == true);




                await using var conn = _context.Database.GetDbConnection();

                if (conn.State != ConnectionState.Open)
                    await conn.OpenAsync();

                await using (var cmdVentas = conn.CreateCommand())
                {
                    cmdVentas.CommandText = @"
                        SELECT COUNT(*), ISNULL(SUM(Total), 0)
                        FROM dbo.Ventas
                        WHERE CAST(Fecha AS date) = CAST(GETDATE() AS date);";

                    await using var reader = await cmdVentas.ExecuteReaderAsync();

                    if (await reader.ReadAsync())
                    {
                        ventasHoyCantidad = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
                        ventasHoyMonto = reader.IsDBNull(1) ? 0 : reader.GetDecimal(1);
                    }
                }

                await using (var cmdUsers = conn.CreateCommand())
                {
                    cmdUsers.CommandText = @"
                        SELECT 
                            (SELECT COUNT(*) FROM dbo.Usuarios),
                            (SELECT COUNT(*) FROM dbo.Usuarios WHERE Estado = 1);";

                    await using var reader = await cmdUsers.ExecuteReaderAsync();

                    if (await reader.ReadAsync())
                    {
                        usuariosTotales = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
                        usuariosActivos = reader.IsDBNull(1) ? 0 : reader.GetInt32(1);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en PanelControl al obtener estadísticas.");
            }

            ViewBag.VentasHoyCantidad = ventasHoyCantidad;
            ViewBag.VentasHoyMonto = ventasHoyMonto;
            ViewBag.ServiciosActivos = serviciosActivos;
            ViewBag.ServiciosTotales = serviciosTotales;
            ViewBag.UsuariosTotales = usuariosTotales;
            ViewBag.UsuariosActivos = usuariosActivos;

            return View();
        }



        public IActionResult Catalogo()
        {
            return View();

        public async Task<IActionResult> Catalogo(int? categoriaId)
        {
            var categorias = await _categoriasLN.ListarAsync(true);
            var productos = await _productosLN.ListarAsync(true);

            if (categoriaId.HasValue)
            {
                productos = productos.Where(p => p.IdCategoria == categoriaId.Value).ToList();
            }

            var vm = new CatalogoVm
            {
                Categorias = categorias,
                Productos = productos,
                CategoriaId = categoriaId
            };

            return View(vm);

        }

        public async Task<IActionResult> Servicios()
        {
            var servicios = await _context.Servicios
                .Where(s => s.Estado == true)
                .ToListAsync();

            return View(servicios);
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
                return Content(_context.Database.CanConnect() ? "OK ✅" : "Error ❌");
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
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