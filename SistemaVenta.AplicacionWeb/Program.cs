// Agregamos referencia a automapper
using SistemaVenta.AplicacionWeb.Utilidades.Automapper;

// Agregamos referencia
using SistemaVenta.IOC;
using System.Text.Json.Serialization;

// Libreria DinkToPDF
using SistemaVenta.AplicacionWeb.Utilidades.Extensiones;
using DinkToPdf;
using DinkToPdf.Contracts;

// Inicializamos libreria de autenticación por cookies.
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Inyectamos inicio de sesión por cookies
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(option =>
    {
        option.LoginPath = "/Acceso/Login";
        option.ExpireTimeSpan = TimeSpan.FromMinutes(20); // Tiempo de expiración de la cookie
    });

// Inyección de dependencias
builder.Services.InyectarDependencia(builder.Configuration);

// Inyectamos depenencias AutoMapper
builder.Services.AddAutoMapper(typeof(AutoMapperProfile));

// Contexto para DinkToPDF
var context = new CustomAssemblyLoadContext();
context.LoadUnmanagedLibrary(Path.Combine(Directory.GetCurrentDirectory(), "Utilidades/LibreriaPDF/libwkhtmltox.dll"));

// Hacemos reconocer extensión DinkToPDF
builder.Services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));

builder.Services.AddControllers().AddJsonOptions(x =>
                x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();

// Integramos atenticación
app.UseAuthentication();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Acceso}/{action=Login}/{id?}");

app.Run();
