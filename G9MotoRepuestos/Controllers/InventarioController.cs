using Microsoft.AspNetCore.Mvc;
using MR.Abstracciones.EntidadesParaUI;
using MR.Abstracciones.LogicaDeNegocio.Productos;
using MR.Abstracciones.LogicaDeNegocio.Bitacora;
using Microsoft.AspNetCore.Hosting;


namespace G9MotoRepuestos.Controllers
{
    public class InventarioController : Controller
    {
        private readonly IProductosLN _ln;
        private readonly IBitacoraProductosLN _bitacora;
        private readonly IWebHostEnvironment _env;


        public InventarioController(
            IProductosLN ln,
            IBitacoraProductosLN bitacora,
            IWebHostEnvironment env)
        {
            _ln = ln;
            _bitacora = bitacora;
            _env = env;
        }

        public async Task<IActionResult> Index()
        {
            var data = await _ln.ListarAsync(soloActivos: true);
            return View(data);
        }

        public IActionResult Create()
        {
            return View(new ProductoDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductoDto model, IFormFile? imagen)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                if (imagen != null && imagen.Length > 0)
                {
                    if (string.IsNullOrWhiteSpace(imagen.ContentType) || !imagen.ContentType.StartsWith("image/"))
                    {
                        ModelState.AddModelError("", "El archivo debe ser una imagen.");
                        return View(model);
                    }

                    if (string.IsNullOrWhiteSpace(_env.WebRootPath))
                    {
                        ModelState.AddModelError("", "No se encontró la carpeta wwwroot (WebRootPath está vacío).");
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

                await _ln.CrearAsync(model);
                await _bitacora.RegistrarAsync("CREAR", "Productos", null);

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error subiendo imagen: {ex.Message}");
                return View(model);
            }
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProductoDto model)
        {
            if (!ModelState.IsValid)
                return View(model);

            await _ln.ActualizarAsync(model);


            await _bitacora.RegistrarAsync("EDITAR", "Productos", null);

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _ln.DesactivarAsync(id);


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
