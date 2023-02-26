// Agregamos referencia a automapper
using SistemaVenta.AplicacionWeb.Utilidades.Automapper;

// Agregamos referencia
using SistemaVenta.IOC;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Inyección de dependencias
builder.Services.InyectarDependencia(builder.Configuration);

// Inyectamos depenencias AutoMapper
builder.Services.AddAutoMapper(typeof(AutoMapperProfile));

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

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
