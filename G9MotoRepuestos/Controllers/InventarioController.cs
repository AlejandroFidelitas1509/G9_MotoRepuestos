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

            if (antes.Nombre != despues.Nombre)
                cambios.Add($"Nombre: {antes.Nombre} → {despues.Nombre}");

            if (antes.Descripcion != despues.Descripcion)
                cambios.Add($"Descripción modificada");

            if (antes.Marca != despues.Marca)
                cambios.Add($"Marca: {antes.Marca} → {despues.Marca}");

            if (antes.PrecioCosto != despues.PrecioCosto)
                cambios.Add($"PrecioCosto: {antes.PrecioCosto} → {despues.PrecioCosto}");

            if (antes.PrecioVenta != despues.PrecioVenta)
                cambios.Add($"PrecioVenta: {antes.PrecioVenta} → {despues.PrecioVenta}");

            if (antes.CodigoBarras != despues.CodigoBarras)
                cambios.Add($"CódigoBarras: {antes.CodigoBarras} → {despues.CodigoBarras}");

            if (antes.Estado != despues.Estado)
                cambios.Add($"Estado: {(antes.Estado == true ? "Activo" : "Inactivo")} → {(despues.Estado == true ? "Activo" : "Inactivo")}");

            if (antes.IdCategoria != despues.IdCategoria)
                cambios.Add($"Categoría cambiada");

            if (antes.StockActual != despues.StockActual)
                cambios.Add($"StockActual: {antes.StockActual} → {despues.StockActual}");

            if (antes.ImageURL != despues.ImageURL)
                cambios.Add($"Imagen actualizada");

            if (!cambios.Any())
                return "No hubo cambios detectados.";

            return string.Join(" | ", cambios);
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

        public async Task<IActionResult> Index()
        {
            var data = await _productosLN.ListarAsync(soloActivos: true);

            var categorias = await _categoriasLN.ListarAsync(soloActivas: true);
            ViewBag.CategoriasFiltro = categorias
                .Select(c => new SelectListItem
                {
                    Value = c.IdCategoria.ToString(),
                    Text = c.Nombre
                })
                .ToList();

            return View(data);
        }

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
                // Subida de imagen (igual que lo tenías)
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

                // ✅ Crear y capturar ID real
                var nuevoId = await _productosLN.CrearAsync(model);
                model.IdProducto = nuevoId;

                // ✅ Bitácora
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
                //  Capturar "ANTES"
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
                    // mantener imagen anterior
                    model.ImageURL = antes?.ImageURL;
                }

                await _productosLN.ActualizarAsync(model);

                // ✅ Bitácora
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
                // si quieres, puedes mostrar TempData y redirigir
                TempData["Error"] = $"Error al eliminar/desactivar: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

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

            // Para los combos
            ViewBag.Acciones = await _bitacora.ListarAccionesAsync();
            ViewBag.Usuarios = await _bitacora.ListarUsuariosAsync();

            // Guardar filtros en la vista
            ViewBag.Desde = desde?.ToString("yyyy-MM-dd") ?? "";
            ViewBag.Hasta = hasta?.ToString("yyyy-MM-dd") ?? "";
            ViewBag.AccionSel = accion ?? "";
            ViewBag.UsuarioSel = idUsuario?.ToString() ?? "";

            // ✅ Esto es lo importante: TRAE SOLO pageSize registros
            var result = await _bitacora.ListarPaginadoAsync(page, pageSize, desde, hasta, accion, idUsuario);

            return View(result);
        }
    }
}