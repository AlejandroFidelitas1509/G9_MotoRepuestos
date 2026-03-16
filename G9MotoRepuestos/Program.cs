using G9MotoRepuestos.Data;
using G9MotoRepuestos.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using MR.Abstracciones.AccesoADatos.Bitacora;
using MR.Abstracciones.AccesoADatos.Calendarios;
using MR.Abstracciones.AccesoADatos.Categorias;
using MR.Abstracciones.AccesoADatos.Finanzas;
using MR.Abstracciones.AccesoADatos.Productos;
using MR.Abstracciones.LogicaDeNegocio.Bitacora;
using MR.Abstracciones.LogicaDeNegocio.Calendarios;
using MR.Abstracciones.LogicaDeNegocio.Categorias;
using MR.Abstracciones.LogicaDeNegocio.Finanzas;
using MR.Abstracciones.LogicaDeNegocio.Productos;
using MR.AccesoDatos;
using MR.AccesoDatos.Bitacora;
using MR.AccesoDatos.Calendarios;
using MR.AccesoDatos.Categorias;
using MR.AccesoDatos.Finanzas;
using MR.AccesoDatos.Productos;
using MR.AccesoDatos.Repositorios;
using MR.LogicaNegocio.Bitacora;
using MR.LogicaNegocio.Calendarios;
using MR.LogicaNegocio.Categorias;
using MR.LogicaNegocio.Finanzas;
using MR.LogicaNegocio.Mapeos;
using MR.LogicaNegocio.Productos;
using MR.LogicaNegocio.Servicios;
using QuestPDF.Infrastructure;


var builder = WebApplication.CreateBuilder(args);

// ✅ QuestPDF license (Community)
QuestPDF.Settings.License = LicenseType.Community;

// --- CONFIGURACIÓN DE BASE DE DATOS ---
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// --- SISTEMA DE AUTENTICACIÓN ---
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Usuarios/Login";
        options.AccessDeniedPath = "/Home/Privacy";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
        options.SlidingExpiration = true;
        options.Cookie.Name = "MotoRepuestosRojas.Session";
    });

// --- SESSION ---
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});


builder.Services.AddControllersWithViews();

// --- DI (Servicios propios) ---
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<IPuntoVentaService, PuntoVentaService>();
builder.Services.AddScoped<IVentasService, VentasService>();

// --- Bitácora / Capas ---
builder.Services.AddDbContext<Contexto>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddScoped<IBitacoraProductosAD>(_ => new BitacoraProductosAD(connectionString));
builder.Services.AddScoped<IBitacoraProductosLN, BitacoraProductosLN>();


builder.Services.AddScoped<IProductosAD>(_ => new ProductosAD(connectionString));
builder.Services.AddScoped<IProductosLN, ProductosLN>();

builder.Services.AddScoped<ICategoriasAD>(_ => new CategoriasAD(connectionString));
builder.Services.AddScoped<ICategoriasLN, CategoriasLN>();

builder.Services.AddScoped<ICitasRepositorio, CitasRepositorio>();
builder.Services.AddScoped<ICitasServicio, CitasServicio>();

builder.Services.AddAutoMapper(cfg => { }, typeof(MapeoClases));

builder.Services.AddScoped<IBloqueosCalendarioRepositorio, BloqueosCalendarioRepositorio>();
builder.Services.AddScoped<IBloqueosCalendarioServicio, BloqueosCalendarioServicio>();

builder.Services.AddScoped<IFinanzasAD>(sp =>
    new FinanzasAD(builder.Configuration.GetConnectionString("DefaultConnection")!));

builder.Services.AddScoped<IFinanzasLN, FinanzasLN>();


builder.WebHost.ConfigureKestrel(options =>
{
    options.ConfigureEndpointDefaults(lo =>
    {
        lo.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1;
    });
});


var app = builder.Build();


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
app.UseSession();


app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "Citas",
    pattern: "{controller=Citas}/{action=Index}/{id?}");

app.Run();
