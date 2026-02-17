using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MR.Abstracciones.EntidadesParaUI;
using MR.Abstracciones.LogicaDeNegocio.Bitacora;
using MR.Abstracciones.LogicaDeNegocio.Categorias;
using MR.Abstracciones.LogicaDeNegocio.Productos;

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
                    {
                        await imagen.CopyToAsync(stream);
                    }

                    model.ImageURL = $"/images/productos/{fileName}";
                }

                await _productosLN.CrearAsync(model);
                await _bitacora.RegistrarAsync("CREAR", "Productos", null);

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error subiendo imagen: {ex.Message}");
                await CargarCategoriasAsync(model.IdCategoria);
                return View(model);
            }
        }

        // (Si quieres también categoría en Edit, lo hacemos después)
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
                    {
                        await imagen.CopyToAsync(stream);
                    }

                    model.ImageURL = $"/images/productos/{fileName}";
                }
                else
                {
                    var actual = await _productosLN.ObtenerPorIdAsync(model.IdProducto);
                    model.ImageURL = actual?.ImageURL;
                }

                await _productosLN.ActualizarAsync(model);
                await _bitacora.RegistrarAsync("EDITAR", "Productos", null);

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
            await _productosLN.DesactivarAsync(id);
            await _bitacora.RegistrarAsync("ELIMINAR", "Productos", null);

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> BitacoraEventos()
        {
            var data = await _bitacora.ListarAsync(200);
            return View(data);
        }
    }
}
