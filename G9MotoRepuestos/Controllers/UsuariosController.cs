using G9MotoRepuestos.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MotoRepuestosRojas.Models;
using System.Security.Claims;

namespace MotoRepuestosRojas.Controllers
{
    public class UsuariosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UsuariosController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (User?.Identity != null && User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string correo, string password)
        {
            var usuario = await _context.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.Correo == correo && u.PasswordHash == password);

            if (usuario != null)
            {
                if (usuario.Estado == false)
                {
                    ViewBag.Error = "Esta cuenta se encuentra desactivada.";
                    return View();
                }

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, usuario.NombreCompleto ?? "Usuario"),
                    new Claim(ClaimTypes.Email, usuario.Correo ?? ""),
                    new Claim("Role", usuario.Rol?.Tipo ?? "Cliente")
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                var authProperties = new AuthenticationProperties
                {
                    AllowRefresh = true,
                    IsPersistent = true
                };

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "Correo o contraseña incorrectos.";
            return View();
        }

        [HttpGet]
        public IActionResult Registro()
        {
            if (User?.Identity != null && User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registro(Usuario usuario)
        {

            ModelState.Remove("Rol");
            ModelState.Remove("Telefono");
            ModelState.Remove("ImagenURL");

            if (ModelState.IsValid)
            {
                var existe = await _context.Usuarios.AnyAsync(u => u.Correo == usuario.Correo);
                if (existe)
                {
                    ModelState.AddModelError("Correo", "Este correo ya está registrado.");
                    return View(usuario);
                }

                usuario.IdRol = 2;   
                usuario.Estado = true; 

                _context.Add(usuario);
                await _context.SaveChangesAsync();
                TempData["Mensaje"] = "¡Cuenta creada con éxito! Ya puedes iniciar sesión.";
                return RedirectToAction("Login");
            }

            return View(usuario);
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }
    }
}