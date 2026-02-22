using G9MotoRepuestos.Data;
using G9MotoRepuestos.Models;
using G9MotoRepuestos.Services; 
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
        private readonly EmailService _emailService; 

        public UsuariosController(ApplicationDbContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string correo, string password)
        {
            var usuario = await _context.Usuarios.Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.Correo == correo);

            if (usuario != null && BCrypt.Net.BCrypt.Verify(password, usuario.PasswordHash))
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
        public IActionResult OlvidePassword() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OlvidePassword(string correo)
        {
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Correo == correo);
            if (usuario != null)
            {
                try
                {
                    string token = Guid.NewGuid().ToString();
                    usuario.TokenRecuperacion = token;
                    _context.Update(usuario);
                    await _context.SaveChangesAsync();

                    var enlace = Url.Action("RestablecerPassword", "Usuarios", new { token = token }, Request.Scheme);
                    string mensajeHtml = $@"
                        <div style='font-family: Arial, sans-serif; border: 1px solid #ddd; padding: 20px; border-radius: 10px;'>
                            <h2 style='color: #d9534f;'>Recuperación de Contraseña - Moto Repuestos Rojas</h2>
                            <p>Hola {usuario.NombreCompleto},</p>
                            <p>Has solicitado restablecer tu contraseña. Haz clic en el siguiente botón para continuar:</p>
                            <div style='text-align: center; margin: 30px 0;'>
                                <a href='{enlace}' style='background-color: #d9534f; color: white; padding: 12px 25px; text-decoration: none; border-radius: 50px; font-weight: bold;'>Restablecer Contraseña</a>
                            </div>
                            <p style='font-size: 0.8em; color: #777;'>Si no solicitaste este cambio, puedes ignorar este correo.</p>
                        </div>";

                    await _emailService.EnviarCorreo(correo, "Recuperar Contraseña - Moto Repuestos Rojas", mensajeHtml);

                    TempData["Mensaje"] = "Se ha enviado un enlace de recuperación a tu correo electrónico.";
                    return RedirectToAction("Login");
                }
                catch (Exception ex)
                {
                    ViewBag.Error = "Ocurrió un error al enviar el correo: " + ex.Message;
                    return View();
                }
            }

            ViewBag.Error = "El correo electrónico no está registrado.";
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> RestablecerPassword(string token)
        {
            if (string.IsNullOrEmpty(token)) return RedirectToAction("Login");

            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.TokenRecuperacion == token);
            if (usuario == null)
            {
                TempData["Error"] = "El enlace es inválido o ha expirado.";
                return RedirectToAction("Login");
            }

            ViewBag.Token = token;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestablecerPassword(string token, string nuevaPassword)
        {
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.TokenRecuperacion == token);
            if (usuario == null) return RedirectToAction("Login");

            if (!string.IsNullOrWhiteSpace(nuevaPassword))
            {
                usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(nuevaPassword);
                usuario.TokenRecuperacion = null;

                _context.Update(usuario);
                await _context.SaveChangesAsync();

                TempData["Mensaje"] = "Contraseña actualizada correctamente. Ya puedes iniciar sesión.";
                return RedirectToAction("Login");
            }

            ViewBag.Error = "La contraseña no puede estar vacía.";
            ViewBag.Token = token;
            return View();
        }


        [HttpGet]
        [Authorize(Roles = "Admin,Administrador")]
        public async Task<IActionResult> GestionUsuarios()
        {
            var listaUsuarios = await _context.Usuarios
                .Include(u => u.Rol)
                .ToListAsync();
            return View(listaUsuarios);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Administrador")]
        public async Task<IActionResult> CrearUsuario()
        {
            ViewBag.IdRol = new SelectList(await _context.Roles.ToListAsync(), "IdRol", "Tipo");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Administrador")]
        public async Task<IActionResult> CrearUsuario(Usuario usuario)
        {
            ModelState.Remove("Rol");
            if (ModelState.IsValid)
            {
                usuario.Estado = true;
                usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(usuario.PasswordHash);

                _context.Add(usuario);
                await _context.SaveChangesAsync();
                TempData["Mensaje"] = "Usuario creado exitosamente.";
                return RedirectToAction(nameof(GestionUsuarios));
            }

            ViewBag.IdRol = new SelectList(await _context.Roles.ToListAsync(), "IdRol", "Tipo", usuario.IdRol);
            return View(usuario);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Administrador")]
        public async Task<IActionResult> EditarUsuario(int? id)
        {
            if (id == null) return NotFound();
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null) return NotFound();

            ViewBag.IdRol = new SelectList(await _context.Roles.ToListAsync(), "IdRol", "Tipo", usuario.IdRol);
            return View(usuario);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Administrador")]
        public async Task<IActionResult> EditarUsuario(int id, Usuario model, string? NuevaPassword)
        {
            if (id != model.IdUsuario) return NotFound();

            var original = await _context.Usuarios.AsNoTracking().FirstOrDefaultAsync(u => u.IdUsuario == id);
            if (original == null) return NotFound();

            if (!string.IsNullOrWhiteSpace(NuevaPassword))
                model.PasswordHash = BCrypt.Net.BCrypt.HashPassword(NuevaPassword);
            else
                model.PasswordHash = original.PasswordHash;

            model.ImagenURL = original.ImagenURL;

            ModelState.Remove("Rol");
            ModelState.Remove("PasswordHash");

            if (ModelState.IsValid)
            {
                _context.Update(model);
                await _context.SaveChangesAsync();
                TempData["Mensaje"] = "Usuario actualizado correctamente.";
                return RedirectToAction(nameof(GestionUsuarios));
            }

            ViewBag.IdRol = new SelectList(await _context.Roles.ToListAsync(), "IdRol", "Tipo", model.IdRol);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Administrador")]
        public async Task<IActionResult> CambiarEstado(int id)
        {
            var usuario = await _context.Usuarios.Include(u => u.Rol).FirstOrDefaultAsync(u => u.IdUsuario == id);
            if (usuario == null) return NotFound();

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (id.ToString() == currentUserId)
            {
                TempData["Error"] = "No puedes bloquear tu propia cuenta.";
                return RedirectToAction(nameof(GestionUsuarios));
            }

            usuario.Estado = !(usuario.Estado ?? false);
            _context.Update(usuario);
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = $"Usuario {(usuario.Estado == true ? "activado" : "bloqueado")} correctamente.";
            return RedirectToAction(nameof(GestionUsuarios));
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> VerPerfil()
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
                model.PasswordHash = BCrypt.Net.BCrypt.HashPassword(NuevaPassword);
            else
                model.PasswordHash = original.PasswordHash;

            if (fotoArchivo != null && fotoArchivo.Length > 0)
            {
                string carpeta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/perfiles");
                if (!Directory.Exists(carpeta)) Directory.CreateDirectory(carpeta);
                string nombreArchivo = Guid.NewGuid().ToString() + Path.GetExtension(fotoArchivo.FileName);
                string rutaFisica = Path.Combine(carpeta, nombreArchivo);
                using (var stream = new FileStream(rutaFisica, FileMode.Create)) { await fotoArchivo.CopyToAsync(stream); }
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
                    return RedirectToAction("GestionUsuarios");

                return RedirectToAction("VerPerfil");
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
                usuario.IdRol = 2;
                usuario.Estado = true;
                usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(usuario.PasswordHash);

                _context.Add(usuario);
                await _context.SaveChangesAsync();
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