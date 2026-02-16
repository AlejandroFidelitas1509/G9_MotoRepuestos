using G9MotoRepuestos.Data;
using G9MotoRepuestos.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
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
                if (usuario.Estado == false)
                {
                    ViewBag.Error = "Tu cuenta ha sido desactivada. Contacta al administrador.";
                    return View();
                }

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, usuario.NombreCompleto ?? "Usuario"),
                    new Claim(ClaimTypes.Email, usuario.Correo ?? ""),
                    new Claim(ClaimTypes.Role, usuario.Rol?.Tipo ?? "Cliente"),
                    new Claim("Role", usuario.Rol?.Tipo ?? "Cliente"),
                    new Claim(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString())
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                if (usuario.Rol?.Tipo == "Admin" || usuario.Rol?.Tipo == "Administrador" || usuario.Rol?.Tipo == "Vendedor")
                {
                    return RedirectToAction("PanelControl", "Home");
                }

                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "Correo o contraseña incorrectos.";
            return View();
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Administrador")]
        public async Task<IActionResult> GestionUsuarios()
        {
            var listaUsuarios = await _context.Usuarios
                .Include(u => u.Rol)
                .Select(u => new Usuario
                {
                    IdUsuario = u.IdUsuario,
                    NombreCompleto = u.NombreCompleto,
                    Correo = u.Correo,
                    Estado = u.Estado,
                    IdRol = u.IdRol,
                    Rol = u.Rol
                })
                .ToListAsync();

            return View(listaUsuarios);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Administrador")]
        public async Task<IActionResult> CambiarEstado(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null) return NotFound();

            if (usuario.IdRol == 1 || usuario.Rol?.Tipo == "Admin")
                return BadRequest("No se pueden bloquear cuentas administrativas.");

            usuario.Estado = !(usuario.Estado ?? false);
            _context.Update(usuario);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(GestionUsuarios));
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Perfil()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return RedirectToAction("Logout");

            var usuario = await _context.Usuarios.Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.IdUsuario == int.Parse(userId));

            if (usuario == null) return NotFound();

            return View(usuario);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> EditarPerfil(int? id)
        {
            var userIdString = id?.ToString() ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdString)) return RedirectToAction("Logout");

            var usuario = await _context.Usuarios.Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.IdUsuario == int.Parse(userIdString));

            if (usuario == null) return NotFound();

            ViewBag.Roles = new SelectList(await _context.Roles.ToListAsync(), "IdRol", "Tipo", usuario.IdRol);
            return View(usuario);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> EditarPerfil(Usuario model, IFormFile? fotoArchivo, string? NuevaPassword)
        {
            var original = await _context.Usuarios.AsNoTracking().FirstOrDefaultAsync(u => u.IdUsuario == model.IdUsuario);
            if (original == null) return NotFound();

            if (!string.IsNullOrWhiteSpace(NuevaPassword))
            {
                model.PasswordHash = NuevaPassword;
            }
            else
            {
                model.PasswordHash = original.PasswordHash;
            }

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

            if (!User.IsInRole("Admin") && !User.IsInRole("Administrador"))
            {
                model.IdRol = original.IdRol;
                model.Estado = original.Estado;
            }

            ModelState.Remove("Rol");
            ModelState.Remove("fotoArchivo");
            ModelState.Remove("PasswordHash");

            if (ModelState.IsValid)
            {
                _context.Update(model);
                await _context.SaveChangesAsync();

                TempData["Mensaje"] = "Perfil actualizado con éxito.";


                if (User.IsInRole("Admin") || User.IsInRole("Administrador"))
                {
                    return RedirectToAction("GestionUsuarios");
                }

                return RedirectToAction("Perfil");
            }

            ViewBag.Roles = new SelectList(await _context.Roles.ToListAsync(), "IdRol", "Tipo", model.IdRol);
            return View(model);
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
                if (!User.IsInRole("Admin") && !User.IsInRole("Administrador"))
                {
                    usuario.IdRol = 2; 
                }

                usuario.Estado = true;
                _context.Add(usuario);
                await _context.SaveChangesAsync();

                if (User.IsInRole("Admin") || User.IsInRole("Administrador"))
                    return RedirectToAction("GestionUsuarios");

                return RedirectToAction("Login");
            }
            return View(usuario);
        }


        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }
    }
}