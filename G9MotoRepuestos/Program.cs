using G9MotoRepuestos.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using MR.AccesoDatos.Bitacora;
using MR.LogicaNegocio.Bitacora;
using MR.AccesoDatos.Productos;
using MR.LogicaNegocio.Productos;
using MR.Abstracciones.AccesoADatos.Productos;
using MR.Abstracciones.LogicaDeNegocio.Productos;
using MR.Abstracciones.AccesoADatos.Bitacora;
using MR.Abstracciones.LogicaDeNegocio.Bitacora;

var builder = WebApplication.CreateBuilder(args);

// --- CONFIGURACIÓN DE BASE DE DATOS ---
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// --- SISTEMA DE AUTENTICACIÓN (Login Personalizado) ---
// Quitamos Identity y dejamos solo Cookies para que tu Login funcione al 100%
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Usuarios/Login";
        options.AccessDeniedPath = "/Home/Privacy";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
        options.SlidingExpiration = true; // Renueva el tiempo si el usuario está activo
        options.Cookie.Name = "MotoRepuestosRojas.Session"; // Nombre personalizado para la cookie
    });

builder.Services.AddControllersWithViews();

// --- INYECCIÓN DE DEPENDENCIAS (Capas de tus compañeros) ---
builder.Services.AddScoped<IBitacoraProductosAD>(_ => new BitacoraProductosAD(connectionString));
builder.Services.AddScoped<IBitacoraProductosLN, BitacoraProductosLN>();
builder.Services.AddScoped<IProductosAD>(_ => new ProductosAD(connectionString));
builder.Services.AddScoped<IProductosLN, ProductosLN>();

var app = builder.Build();

// --- PIPELINE DE LA APLICACIÓN ---
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

// ✅ El orden es fundamental para que sepa quién eres antes de darte permiso
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// MapRazorPages se queda quitado para no cargar el registro viejo de Identity

app.Run();