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
            int totalProductos = 0;
            var ventasSemana = new int[7];
            var labSemana = new string[7];
            var actividad = new List<(string Color, string Icono, string Texto, DateTime Fecha)>();

            try
            {
                serviciosTotales = await _context.Servicios.CountAsync();
                serviciosActivos = await _context.Servicios.CountAsync(s => s.Estado == true);

                await using var conn = _context.Database.GetDbConnection();

                if (conn.State != ConnectionState.Open)
                    await conn.OpenAsync();

                // ── Ventas de hoy ─────────────────────────────────────────────
                await using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT COUNT(*), ISNULL(SUM(Total), 0)
                        FROM dbo.Ventas
                        WHERE CAST(Fecha AS date) = CAST(GETDATE() AS date);";

                    await using var reader = await cmd.ExecuteReaderAsync();
                    if (await reader.ReadAsync())
                    {
                        ventasHoyCantidad = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
                        ventasHoyMonto = reader.IsDBNull(1) ? 0 : reader.GetDecimal(1);
                    }
                }

                // ── Usuarios totales y activos ────────────────────────────────
                await using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT 
                            (SELECT COUNT(*) FROM dbo.Usuarios),
                            (SELECT COUNT(*) FROM dbo.Usuarios WHERE Estado = 1);";

                    await using var reader = await cmd.ExecuteReaderAsync();
                    if (await reader.ReadAsync())
                    {
                        usuariosTotales = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
                        usuariosActivos = reader.IsDBNull(1) ? 0 : reader.GetInt32(1);
                    }
                }

                // ── Total de productos en catálogo ────────────────────────────
                await using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT COUNT(*) FROM dbo.Productos;";
                    var result = await cmd.ExecuteScalarAsync();
                    totalProductos = result == DBNull.Value ? 0 : Convert.ToInt32(result);
                }

                // ── Ventas de los últimos 7 días (gráfico) ────────────────────
                await using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT 
                            CAST(Fecha AS date) AS Dia,
                            COUNT(*)            AS Cantidad
                        FROM dbo.Ventas
                        WHERE Fecha >= CAST(DATEADD(day, -6, GETDATE()) AS date)
                        GROUP BY CAST(Fecha AS date)
                        ORDER BY Dia;";

                    var hoy = DateTime.Today;
                    var culturaES = new System.Globalization.CultureInfo("es-CR");

                    for (int i = 0; i < 7; i++)
                    {
                        var dia = hoy.AddDays(-6 + i);
                        ventasSemana[i] = 0;
                        labSemana[i] = i == 6
                            ? "Hoy"
                            : dia.ToString("ddd", culturaES)[..1].ToUpper();
                    }

                    await using var reader = await cmd.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        var dia = reader.GetDateTime(0).Date;
                        var cantidad = reader.GetInt32(1);
                        var idx = (dia - hoy.AddDays(-6)).Days;
                        if (idx >= 0 && idx < 7)
                            ventasSemana[idx] = cantidad;
                    }
                }

                // ── Actividad reciente — últimas ventas ───────────────────────
                await using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT TOP 3 Fecha, Total, MetodoPago
                        FROM dbo.Ventas
                        WHERE Fecha IS NOT NULL
                        ORDER BY Fecha DESC;";

                    await using var reader = await cmd.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        var fecha = reader.IsDBNull(0) ? DateTime.MinValue : reader.GetDateTime(0);
                        var total = reader.IsDBNull(1) ? 0m : reader.GetDecimal(1);
                        var metodo = reader.IsDBNull(2) ? "" : reader.GetString(2);
                        actividad.Add((
                            Color: "#2f9e44",
                            Icono: "fa-shopping-cart",
                            Texto: $"Nueva venta por ₡{total:N2} — {metodo}",
                            Fecha: fecha
                        ));
                    }
                }

                // ── Actividad reciente — últimas citas ────────────────────────
                await using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT TOP 2 Fecha, Modelo, Detalle
                        FROM dbo.Citas
                        WHERE Fecha IS NOT NULL
                        ORDER BY Fecha DESC;";

                    await using var reader = await cmd.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        var fecha = reader.IsDBNull(0) ? DateTime.MinValue : reader.GetDateTime(0);
                        var modelo = reader.IsDBNull(1) ? "" : reader.GetString(1);
                        var det = reader.IsDBNull(2) ? "" : reader.GetString(2);
                        actividad.Add((
                            Color: "#1971c2",
                            Icono: "fa-calendar-check",
                            Texto: $"Cita agendada — {(string.IsNullOrWhiteSpace(modelo) ? det : modelo)}",
                            Fecha: fecha
                        ));
                    }
                }

                // ── Actividad reciente — últimos usuarios registrados ─────────
                await using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT TOP 2 NombreCompleto
                        FROM dbo.Usuarios
                        ORDER BY IdUsuario DESC;";

                    await using var reader = await cmd.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        var nombre = reader.IsDBNull(0) ? "Usuario" : reader.GetString(0);
                        actividad.Add((
                            Color: "#c92a2a",
                            Icono: "fa-user-plus",
                            Texto: $"Nuevo usuario registrado: {nombre}",
                            Fecha: DateTime.MinValue
                        ));
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
            ViewBag.TotalProductos = totalProductos;
            ViewBag.VentasSemana = ventasSemana;
            ViewBag.VentasSemanaLabels = labSemana;

            ViewBag.ActividadReciente = actividad
                .OrderByDescending(a => a.Fecha)
                .Take(6)
                .ToList();

            return View();
        }

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
