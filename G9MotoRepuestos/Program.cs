using G9MotoRepuestos.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MR.AccesoDatos.Bitacora;
using MR.LogicaNegocio.Bitacora;
using MR.AccesoDatos.Productos;
using MR.LogicaNegocio.Productos;
using MR.Abstracciones.AccesoADatos.Productos;
using MR.Abstracciones.LogicaDeNegocio.Productos;
using MR.Abstracciones.AccesoADatos.Bitacora;
using MR.Abstracciones.LogicaDeNegocio.Bitacora;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllersWithViews();

// Bitácora Productos
builder.Services.AddScoped<IBitacoraProductosAD>(_ => new BitacoraProductosAD(connectionString));
builder.Services.AddScoped<IBitacoraProductosLN, BitacoraProductosLN>();


// ✅ AGREGA ESTO (inyección de dependencias para tu CRUD Productos con Dapper)
builder.Services.AddScoped<IProductosAD>(_ => new ProductosAD(connectionString));
builder.Services.AddScoped<IProductosLN, ProductosLN>();

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

// ✅ Recomendado con Identity (normalmente va antes de Authorization)
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();
