using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MR.Abstracciones.EntidadesParaUI;
using MR.Abstracciones.LogicaDeNegocio.Bitacora;
using MR.Abstracciones.LogicaDeNegocio.Categorias;
using MR.Abstracciones.LogicaDeNegocio.Productos;
using System.Security.Claims;
using System.Text.Json;

namespace G9MotoRepuestos.Controllers
{
    public class InventarioController : Controller
    {
        private readonly IProductosLN _productosLN;
        private readonly IBitacoraProductosLN _bitacora;
        private readonly ICategoriasLN _categoriasLN;
        private readonly IWebHostEnvironment _env;

        public InventarioController(
            IProductosLN productosLN,
            IBitacoraProductosLN bitacora,
            ICategoriasLN categoriasLN,
            IWebHostEnvironment env)
        {
            _productosLN = productosLN;
            _bitacora = bitacora;
            _categoriasLN = categoriasLN;
            _env = env;
        }

        private (int? idUsuario, string usuarioNombre) GetAuditUser()
        {
            var idUsuarioStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int? idUsuario = int.TryParse(idUsuarioStr, out var n) ? n : (int?)null;
            var usuarioNombre = User.FindFirstValue(ClaimTypes.Name) ?? "N/A";
            return (idUsuario, usuarioNombre);
        }

        private string GenerarDescripcionCambios(ProductoDto? antes, ProductoDto despues)
        {
            if (antes == null)
                return "Producto actualizado (no se encontró estado anterior).";

            var cambios = new List<string>();

            if (antes.Nombre != despues.Nombre) cambios.Add($"Nombre: {antes.Nombre} → {despues.Nombre}");
            if (antes.Descripcion != despues.Descripcion) cambios.Add("Descripción modificada");
            if (antes.Marca != despues.Marca) cambios.Add($"Marca: {antes.Marca} → {despues.Marca}");
            if (antes.PrecioCosto != despues.PrecioCosto) cambios.Add($"PrecioCosto: {antes.PrecioCosto} → {despues.PrecioCosto}");
            if (antes.PrecioVenta != despues.PrecioVenta) cambios.Add($"PrecioVenta: {antes.PrecioVenta} → {despues.PrecioVenta}");
            if (antes.CodigoBarras != despues.CodigoBarras) cambios.Add($"CódigoBarras: {antes.CodigoBarras} → {despues.CodigoBarras}");
            if (antes.Estado != despues.Estado) cambios.Add($"Estado: {(antes.Estado == true ? "Activo" : "Inactivo")} → {(despues.Estado == true ? "Activo" : "Inactivo")}");
            if (antes.IdCategoria != despues.IdCategoria) cambios.Add("Categoría cambiada");
            if (antes.StockActual != despues.StockActual) cambios.Add($"StockActual: {antes.StockActual} → {despues.StockActual}");
            if (antes.ImageURL != despues.ImageURL) cambios.Add("Imagen actualizada");

            return cambios.Any() ? string.Join(" | ", cambios) : "No hubo cambios detectados.";
        }

        private async Task CargarCategoriasAsync(int? idCategoriaSeleccionada = null)
        {
            var categorias = await _categoriasLN.ListarAsync(soloActivas: true);
            ViewBag.Categorias = categorias.Select(c => new SelectListItem
            {
                Value = c.IdCategoria.ToString(),
                Text = c.Nombre,
                Selected = idCategoriaSeleccionada.HasValue && c.IdCategoria == idCategoriaSeleccionada.Value
            }).ToList();
        }

        // ══════════════════════════════════════════════════════════════
        // INDEX — con paginación y filtros server-side
        // ══════════════════════════════════════════════════════════════
        public async Task<IActionResult> Index(
            int pagina = 1,
            int porPagina = 10,
            string? buscar = null,
            string? stock = null,
            int? categoriaId = null)
        {
            if (pagina < 1) pagina = 1;
            if (porPagina < 1) porPagina = 10;

            // Traer todos los productos activos
            var todos = await _productosLN.ListarAsync(soloActivos: true);

            // ── Filtros ───────────────────────────────────────────────
            if (!string.IsNullOrWhiteSpace(buscar))
            {
                var q = buscar.Trim().ToLower();
                todos = todos.Where(p =>
                    (p.Nombre ?? "").ToLower().Contains(q) ||
                    (p.Marca ?? "").ToLower().Contains(q) ||
                    (p.CodigoBarras ?? "").ToLower().Contains(q)
                ).ToList();
            }

            if (!string.IsNullOrWhiteSpace(stock))
            {
                todos = stock switch
                {
                    "zero" => todos.Where(p => (p.StockActual ?? 0) == 0).ToList(),
                    "low" => todos.Where(p => (p.StockActual ?? 0) >= 1 && (p.StockActual ?? 0) <= 3).ToList(),
                    "ok" => todos.Where(p => (p.StockActual ?? 0) >= 4).ToList(),
                    _ => todos
                };
            }

            if (categoriaId.HasValue)
                todos = todos.Where(p => p.IdCategoria == categoriaId.Value).ToList();

            // ── Estadísticas sobre total filtrado ─────────────────────
            var totalFiltrado = todos.Count();
            var stockTotalSum = todos.Sum(p => p.StockActual ?? 0);
            var sinStockCount = todos.Count(p => (p.StockActual ?? 0) == 0);
            var stockBajoCount = todos.Count(p => (p.StockActual ?? 0) >= 1 && (p.StockActual ?? 0) <= 3);

            // ── Paginación ────────────────────────────────────────────
            var totalPaginas = (int)Math.Ceiling((double)totalFiltrado / porPagina);
            if (totalPaginas < 1) totalPaginas = 1;
            if (pagina > totalPaginas) pagina = totalPaginas;

            var productosEnPagina = todos
                .Skip((pagina - 1) * porPagina)
                .Take(porPagina)
                .ToList();

            // ── Categorías para el filtro ─────────────────────────────
            var categorias = await _categoriasLN.ListarAsync(soloActivas: true);
            ViewBag.CategoriasFiltro = categorias
                .Select(c => new SelectListItem
                {
                    Value = c.IdCategoria.ToString(),
                    Text = c.Nombre,
                    Selected = categoriaId.HasValue && c.IdCategoria == categoriaId.Value
                })
                .ToList();

            // ── ViewBag para la vista ─────────────────────────────────
            ViewBag.PaginaActual = pagina;
            ViewBag.TotalPaginas = totalPaginas;
            ViewBag.PorPagina = porPagina;
            ViewBag.TotalFiltrado = totalFiltrado;
            ViewBag.StockTotal = stockTotalSum;
            ViewBag.SinStock = sinStockCount;
            ViewBag.StockBajo = stockBajoCount;
            ViewBag.FiltroBuscar = buscar ?? "";
            ViewBag.FiltroStock = stock ?? "";
            ViewBag.FiltroCategoria = categoriaId?.ToString() ?? "";

            return View(productosEnPagina);
        }

        // ══════════════════════════════════════════════════════════════
        // CREATE
        // ══════════════════════════════════════════════════════════════
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await CargarCategoriasAsync();
            return View(new ProductoDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductoDto model, IFormFile? imagen)
        {
            if (!ModelState.IsValid)
            {
                await CargarCategoriasAsync(model.IdCategoria);
                return View(model);
            }

            try
            {
                if (imagen != null && imagen.Length > 0)
                {
                    if (string.IsNullOrWhiteSpace(imagen.ContentType) || !imagen.ContentType.StartsWith("image/"))
                    {
                        ModelState.AddModelError("", "El archivo debe ser una imagen.");
                        await CargarCategoriasAsync(model.IdCategoria);
                        return View(model);
                    }

                    if (string.IsNullOrWhiteSpace(_env.WebRootPath))
                    {
                        ModelState.AddModelError("", "No se encontró la carpeta wwwroot (WebRootPath está vacío).");
                        await CargarCategoriasAsync(model.IdCategoria);
                        return View(model);
                    }

                    var uploadsFolder = Path.Combine(_env.WebRootPath, "images", "productos");
                    Directory.CreateDirectory(uploadsFolder);

                    var extension = Path.GetExtension(imagen.FileName);
                    if (string.IsNullOrWhiteSpace(extension)) extension = ".jpg";

                    var fileName = $"{Guid.NewGuid():N}{extension}";
                    var fullPath = Path.Combine(uploadsFolder, fileName);

                    using (var stream = new FileStream(fullPath, FileMode.Create))
                        await imagen.CopyToAsync(stream);

                    model.ImageURL = $"/images/productos/{fileName}";
                }

                CalcularPrecioVentaDesdeMargen(model);
                var nuevoId = await _productosLN.CrearAsync(model);
                model.IdProducto = nuevoId;

                var (idUsuario, usuarioNombre) = GetAuditUser();
                await _bitacora.RegistrarAsync(new BitacoraProductosDto
                {
                    Accion = "CREAR",
                    TablaAfectada = "Productos",
                    IdUsuario = idUsuario,
                    UsuarioNombre = usuarioNombre,
                    RegistroId = nuevoId,
                    Descripcion = $"Creó producto #{nuevoId} - {model.Nombre}",
                    AntesJson = null,
                    DespuesJson = JsonSerializer.Serialize(model),
                });

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error al crear producto: {ex.Message}");
                await CargarCategoriasAsync(model.IdCategoria);
                return View(model);
            }
        }

        private void CalcularPrecioVentaDesdeMargen(ProductoDto model)
        {
            if (!model.PrecioCosto.HasValue || !model.MargenPorcentaje.HasValue) return;

            var costo = model.PrecioCosto.Value;
            var margen = model.MargenPorcentaje.Value;

            if (costo < 0) costo = 0;
            if (margen < 0) margen = 0;

            model.PrecioVenta = Math.Round(costo * (1m + (margen / 100m)), 2);
        }

        // ══════════════════════════════════════════════════════════════
        // EDIT
        // ══════════════════════════════════════════════════════════════
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var producto = await _productosLN.ObtenerPorIdAsync(id);
            if (producto == null) return NotFound();

            await CargarCategoriasAsync(producto.IdCategoria);
            return View(producto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProductoDto model, IFormFile? imagen)
        {
            if (!ModelState.IsValid)
            {
                await CargarCategoriasAsync(model.IdCategoria);
                return View(model);
            }

            try
            {
                var antes = await _productosLN.ObtenerPorIdAsync(model.IdProducto);
                model.Estado = antes?.Estado;

                if (imagen != null && imagen.Length > 0)
                {
                    if (string.IsNullOrWhiteSpace(imagen.ContentType) || !imagen.ContentType.StartsWith("image/"))
                    {
                        ModelState.AddModelError("", "El archivo debe ser una imagen.");
                        await CargarCategoriasAsync(model.IdCategoria);
                        return View(model);
                    }

                    var uploadsFolder = Path.Combine(_env.WebRootPath, "images", "productos");
                    Directory.CreateDirectory(uploadsFolder);

                    var extension = Path.GetExtension(imagen.FileName);
                    if (string.IsNullOrWhiteSpace(extension)) extension = ".jpg";

                    var fileName = $"{Guid.NewGuid():N}{extension}";
                    var fullPath = Path.Combine(uploadsFolder, fileName);

                    using (var stream = new FileStream(fullPath, FileMode.Create))
                        await imagen.CopyToAsync(stream);

                    model.ImageURL = $"/images/productos/{fileName}";
                }
                else
                {
                    model.ImageURL = antes?.ImageURL;
                }

                CalcularPrecioVentaDesdeMargen(model);
                await _productosLN.ActualizarAsync(model);

                var (idUsuario, usuarioNombre) = GetAuditUser();
                await _bitacora.RegistrarAsync(new BitacoraProductosDto
                {
                    Accion = "EDITAR",
                    TablaAfectada = "Productos",
                    IdUsuario = idUsuario,
                    UsuarioNombre = usuarioNombre,
                    RegistroId = model.IdProducto,
                    Descripcion = GenerarDescripcionCambios(antes, model),
                    AntesJson = JsonSerializer.Serialize(antes),
                    DespuesJson = JsonSerializer.Serialize(model),
                });

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error al editar: {ex.Message}");
                await CargarCategoriasAsync(model.IdCategoria);
                return View(model);
            }
        }

        // ══════════════════════════════════════════════════════════════
        // DELETE
        // ══════════════════════════════════════════════════════════════
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var antes = await _productosLN.ObtenerPorIdAsync(id);
                await _productosLN.CambiarEstadoAsync(id, false);

                var (idUsuario, usuarioNombre) = GetAuditUser();
                await _bitacora.RegistrarAsync(new BitacoraProductosDto
                {
                    Accion = "ELIMINAR",
                    TablaAfectada = "Productos",
                    IdUsuario = idUsuario,
                    UsuarioNombre = usuarioNombre,
                    RegistroId = id,
                    Descripcion = $"Desactivó producto #{id} - {antes?.Nombre}",
                    AntesJson = JsonSerializer.Serialize(antes),
                    DespuesJson = null,
                });

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al eliminar/desactivar: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // ══════════════════════════════════════════════════════════════
        // BITÁCORA
        // ══════════════════════════════════════════════════════════════
        public async Task<IActionResult> BitacoraEventos(
            int page = 1,
            int pageSize = 15,
            DateTime? desde = null,
            DateTime? hasta = null,
            string? accion = null,
            int? idUsuario = null)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 15;

            ViewBag.Acciones = await _bitacora.ListarAccionesAsync();
            ViewBag.Usuarios = await _bitacora.ListarUsuariosAsync();
            ViewBag.Desde = desde?.ToString("yyyy-MM-dd") ?? "";
            ViewBag.Hasta = hasta?.ToString("yyyy-MM-dd") ?? "";
            ViewBag.AccionSel = accion ?? "";
            ViewBag.UsuarioSel = idUsuario?.ToString() ?? "";

            var result = await _bitacora.ListarPaginadoAsync(page, pageSize, desde, hasta, accion, idUsuario);

            return View(result);
        }
    }
}
