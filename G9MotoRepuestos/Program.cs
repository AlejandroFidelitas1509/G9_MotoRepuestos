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
using MR.AccesoDatos.Categorias;
using MR.LogicaNegocio.Categorias;
using MR.Abstracciones.AccesoADatos.Categorias;
using MR.Abstracciones.LogicaDeNegocio.Categorias;
using MR.AccesoDatos.Repositorios;
using MR.LogicaNegocio.Servicios;
using MR.LogicaNegocio.Mapeos;
using MR.AccesoDatos;


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

builder.Services.AddDbContext<Contexto>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IBitacoraProductosAD>(_ => new BitacoraProductosAD(connectionString));
builder.Services.AddScoped<IBitacoraProductosLN, BitacoraProductosLN>();

// Productos
builder.Services.AddScoped<IProductosAD>(_ => new ProductosAD(connectionString));
builder.Services.AddScoped<IProductosLN, ProductosLN>();

builder.Services.AddScoped<ICategoriasAD>(_ => new CategoriasAD(connectionString));
builder.Services.AddScoped<ICategoriasLN, CategoriasLN>();

builder.Services.AddScoped<ICitasRepositorio, CitasRepositorio>();
builder.Services.AddScoped<ICitasServicio, CitasServicio>();

builder.Services.AddAutoMapper(cfg => { }, typeof(MapeoClases));


builder.WebHost.ConfigureKestrel(options =>
{
    options.ConfigureEndpointDefaults(lo =>
    {
        lo.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1;
    });
});


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

app.MapControllerRoute(
    name: "Citas",
    pattern: "{controller=Citas}/{action=Index}/{id?}");

app.Run();