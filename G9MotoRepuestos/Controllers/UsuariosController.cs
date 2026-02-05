using G9MotoRepuestos.Data;
using G9MotoRepuestos.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace G9MotoRepuestos.Controllers
{
    public class UsuariosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UsuariosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // --- LOGIN ---
        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string correo, string password)
        {
            var usuario = await _context.Usuarios.Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.Correo == correo && u.PasswordHash == password);

            if (usuario != null)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, usuario.NombreCompleto ?? "Usuario"),
                    new Claim(ClaimTypes.Email, usuario.Correo ?? ""),
                    new Claim("Role", usuario.Rol?.Tipo ?? "Cliente"),
                    new Claim(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString())
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                return RedirectToAction("Index", "Home");
            }
            ViewBag.Error = "Correo o contraseña incorrectos.";
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Perfil()
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return RedirectToAction("Logout");

            var usuario = await _context.Usuarios.Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.IdUsuario == int.Parse(userId));

            if (usuario == null) return NotFound();

            return View(usuario);
        }


        [HttpGet]
        public async Task<IActionResult> EditarPerfil()
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return RedirectToAction("Logout");

            var usuario = await _context.Usuarios.Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.IdUsuario == int.Parse(userId));

            if (usuario == null) return NotFound();

            ViewBag.Roles = new SelectList(await _context.Roles.ToListAsync(), "IdRol", "Tipo", usuario.IdRol);
            return View(usuario);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarPerfil(Usuario model, IFormFile? fotoArchivo)
        {
            var original = await _context.Usuarios.AsNoTracking().FirstOrDefaultAsync(u => u.IdUsuario == model.IdUsuario);
            if (original == null) return NotFound();


            if (fotoArchivo != null && fotoArchivo.Length > 0)
            {
                string carpeta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/perfiles");
                if (!Directory.Exists(carpeta)) Directory.CreateDirectory(carpeta);

                string nombreArchivo = Guid.NewGuid().ToString() + Path.GetExtension(fotoArchivo.FileName);
                string rutaFisica = Path.Combine(carpeta, nombreArchivo);

                using (var stream = new FileStream(rutaFisica, FileMode.Create))
                {
                    await fotoArchivo.CopyToAsync(stream);
                }
                model.ImagenURL = "/perfiles/" + nombreArchivo;
            }
            else
            {
                model.ImagenURL = original.ImagenURL;
            }

            var userRole = User.Claims.FirstOrDefault(c => c.Type == "Role")?.Value;
            if (!string.Equals(userRole, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                model.IdRol = original.IdRol;
                model.Estado = original.Estado;
            }

            ModelState.Remove("Rol");
            ModelState.Remove("fotoArchivo");

            if (ModelState.IsValid)
            {
                _context.Update(model);
                await _context.SaveChangesAsync();
                TempData["Mensaje"] = "Perfil actualizado con éxito.";
                return RedirectToAction("Perfil");
            }

            ViewBag.Roles = new SelectList(await _context.Roles.ToListAsync(), "IdRol", "Tipo", model.IdRol);
            return View(model);
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Registro() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registro(Usuario usuario)
        {
            ModelState.Remove("Rol");
            if (ModelState.IsValid)
            {
                usuario.IdRol = 2; 
                usuario.Estado = true;
                _context.Add(usuario);
                await _context.SaveChangesAsync();
                return RedirectToAction("Login");
            }
            return View(usuario);
        }
    }
}