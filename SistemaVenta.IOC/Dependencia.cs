using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Inyección de dependencias
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SistemaVenta.DAL.DBContext;
using Microsoft.EntityFrameworkCore;
using SistemaVenta.DAL.Interfaces;
using SistemaVenta.DAL.Implementacion;
using SistemaVenta.BLL.Interfaces;
using SistemaVenta.BLL.Implementacion;
using Firebase.Auth;

namespace SistemaVenta.IOC
{
    public static class Dependencia
    {
        public static void InyectarDependencia(this IServiceCollection services, IConfiguration Configuration)
        {
            services.AddDbContext<DbventaContext>(options =>
            {
                options.UseSqlServer(Configuration.GetConnectionString("CadenaSQL"));
            });

            // 
            services.AddTransient(typeof(IGenericRepository<>), typeof(GenericRepository<>));

            // 
            services.AddScoped<IVentaRepository, VentaRepository>();

            // Dependencia para envio de emails
            services.AddScoped<ICorreoService, CorreoService>();

            // Dependencia para FireBase
            services.AddScoped<IFireBaseService, FireBaseService>();

            // Dependencia para encriptación y generación de contraseñas
            services.AddScoped<IUtilidadesService, UtilidadesService>();

            // Dependencia para el rol
            services.AddScoped<IRolService, RolService>();

            // Dependencia de usuario
            services.AddScoped<IUsuarioService, UsuarioService>();

            // Dependencia de negocio
            services.AddScoped<INegocioService, NegocioService>();

            // Dependencia de categoria
            services.AddScoped<ICategoriaService, CategoriaService>();

            // Dependencia de producto
            services.AddScoped<IProductoService, ProductoService>();

            // Dependencia de tipo documento venta
            services.AddScoped<ITipoDocumentoVentaService, TipoDocumentoVentaService>();

            // Dependencia de Venta
            services.AddScoped<IVentaService, VentaService>();

            // Dependencia de Dashboard
            services.AddScoped<IDashBoardService, DashBoardService>();

            // Dependencia de menu
            services.AddScoped<IMenuService, MenuService>();
        }
    }
}
