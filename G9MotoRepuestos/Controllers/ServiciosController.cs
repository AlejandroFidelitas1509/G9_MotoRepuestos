using G9MotoRepuestos.Data;
using G9MotoRepuestos.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace G9MotoRepuestos.Controllers
{
    public class ServiciosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;

        public ServiciosController(ApplicationDbContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
        }

        public async Task<IActionResult> GestionServicios()
        {
            var servicios = await _context.Servicios.ToListAsync();
            return View(servicios);
        }

        public IActionResult AgregarServicio()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AgregarServicio(Servicios servicio, IFormFile? ImagenFile)
        {
            ModelState.Remove("ImagenUrl");
            ModelState.Remove("IdServicio");

            if (ModelState.IsValid)
            {
                if (ImagenFile != null)
                {
                    servicio.ImagenUrl = await GuardarImagen(ImagenFile);
                }
                else
                {
                    servicio.ImagenUrl = "/images/default-servicio.png";
                }

                _context.Add(servicio);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(GestionServicios));
            }

            return View(servicio);
        }

        public async Task<IActionResult> EditarServicio(int? id)
        {
            if (id == null) return NotFound();

            var servicio = await _context.Servicios.FindAsync(id);
            if (servicio == null) return NotFound();

            return View(servicio);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarServicio(int id, Servicios servicio, IFormFile? ImagenFile)
        {
            if (id != servicio.IdServicio) return NotFound();

            ModelState.Remove("ImagenUrl");

            if (ModelState.IsValid)
            {
                try
                {
                    if (ImagenFile != null)
                    {
                        servicio.ImagenUrl = await GuardarImagen(ImagenFile);
                    }

                    _context.Update(servicio);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ServicioExists(servicio.IdServicio)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(GestionServicios));
            }
            return View(servicio);
        }

        public async Task<IActionResult> EliminarServicio(int? id)
        {
            if (id == null) return NotFound();

            var servicio = await _context.Servicios.FirstOrDefaultAsync(m => m.IdServicio == id);
            if (servicio == null) return NotFound();

            return View(servicio);
        }

        [HttpPost, ActionName("EliminarConfirmado")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarConfirmado(int id)
        {
            var servicio = await _context.Servicios.FindAsync(id);
            if (servicio != null)
            {
                _context.Servicios.Remove(servicio);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(GestionServicios));
        }

        private bool ServicioExists(int id)
        {
            return _context.Servicios.Any(e => e.IdServicio == id);
        }

        private async Task<string> GuardarImagen(IFormFile archivo)
        {
            string rutaRoot = _hostEnvironment.WebRootPath;
            string nombreArchivo = Guid.NewGuid().ToString() + Path.GetExtension(archivo.FileName);
            string rutaCarpeta = Path.Combine(rutaRoot, "images", "servicios");

            if (!Directory.Exists(rutaCarpeta))
                Directory.CreateDirectory(rutaCarpeta);

            string rutaCompleta = Path.Combine(rutaCarpeta, nombreArchivo);

            using (var fileStream = new FileStream(rutaCompleta, FileMode.Create))
            {
                await archivo.CopyToAsync(fileStream);
            }

            return "/images/servicios/" + nombreArchivo;
        }
    }
}