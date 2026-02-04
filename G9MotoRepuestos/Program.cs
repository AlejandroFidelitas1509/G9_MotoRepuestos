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

// --- SISTEMA DE AUTENTICACIÓN (Tu Login con Cookies) ---
// Quitamos el Identity por defecto para que no choque con tu lógica
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Usuarios/Login";
        options.AccessDeniedPath = "/Home/Privacy";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
        options.SlidingExpiration = true;
        options.Cookie.Name = "MotoRepuestosRojas.Session";
    });

builder.Services.AddControllersWithViews();

// --- INYECCIÓN DE DEPENDENCIAS (Arquitectura por capas) ---
// Bitácora
builder.Services.AddScoped<IBitacoraProductosAD>(_ => new BitacoraProductosAD(connectionString));
builder.Services.AddScoped<IBitacoraProductosLN, BitacoraProductosLN>();

// Productos
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
app.UseStaticFiles(); // ? Fundamental para que se vean las fotos en wwwroot/perfiles

app.UseRouting();

// ? El orden es Sagrado: Autenticación antes que Autorización
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();