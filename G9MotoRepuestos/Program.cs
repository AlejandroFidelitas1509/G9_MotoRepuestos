using G9MotoRepuestos.Data;
using Microsoft.AspNetCore.Authentication.Cookies; // Agregado para Cookies
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Configuración de la Base de Datos
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// 2. REEMPLAZO DE IDENTITY POR COOKIES (Lógica para tus Claims)
// Quitamos builder.Services.AddDefaultIdentity... y ponemos esto:
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Usuarios/Login";    // Ruta de tu login
        options.AccessDeniedPath = "/Home/Index"; // A donde va si no tiene permiso
        options.ExpireTimeSpan = TimeSpan.FromHours(8); // Duración de jornada en el taller
    });

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// 3. EL ORDEN DE SEGURIDAD (¡Importante!)
app.UseAuthentication(); // Primero: ¿Quién es el usuario?
app.UseAuthorization();  // Segundo: ¿Qué tiene permiso de hacer?

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Quitamos app.MapRazorPages() porque no usaremos las páginas automáticas de Identity
app.Run();
