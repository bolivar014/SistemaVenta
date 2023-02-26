using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// 
using Microsoft.EntityFrameworkCore;
using SistemaVenta.BLL.Interfaces;
using SistemaVenta.DAL.Interfaces;
using SistemaVenta.Entity;
using System.Globalization;

namespace SistemaVenta.BLL.Implementacion
{
    public class DashBoardService : IDashBoardService
    {
        private readonly IVentaRepository _repositorioVenta;
        private readonly IGenericRepository<DetalleVenta> _repositorioDetalleVenta;
        private readonly IGenericRepository<Categoria> _repositorioCategoria;
        private readonly IGenericRepository<Producto> _repositorioProducto;
        private DateTime FechaInicio = DateTime.Now;

        public DashBoardService(
            IVentaRepository repositorioVenta, 
            IGenericRepository<DetalleVenta> repositorioDetalleVenta, 
            IGenericRepository<Categoria> repositorioCategoria, 
            IGenericRepository<Producto> repositorioProducto)
        {
            _repositorioVenta = repositorioVenta;
            _repositorioDetalleVenta = repositorioDetalleVenta;
            _repositorioCategoria = repositorioCategoria;
            _repositorioProducto = repositorioProducto;
            FechaInicio = FechaInicio.AddDays(-7);
        }

        public async Task<int> TotalVentasUltimaSemana()
        {
            try
            {
                // Buscamos todas las ventas de los ultimos 7 días
                IQueryable<Venta> query = await _repositorioVenta.Consultar(v => v.FechaRegistro.Value.Date >= FechaInicio.Date);

                // Contamos total de ventas
                int total = query.Count();

                // Retornamos
                return total;
            }
            catch(Exception ex)
            {
                throw;
            }
        }

        public async Task<string> TotalIngresosUltimaSemana()
        {
            try
            {
                // Buscamos todas las ventas de los ultimos 7 días
                IQueryable<Venta> query = await _repositorioVenta.Consultar(v => v.FechaRegistro.Value.Date >= FechaInicio.Date);

                // sumamos total de ventas
                decimal resultado = query
                    .Select(v => v.Total)
                    .Sum(v => v.Value);

                // Retornamos
                return Convert.ToString(resultado, new CultureInfo("es-CO"));
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<int> TotalProductos()
        {
            try
            {
                // Buscamos 
                IQueryable<Producto> query = await _repositorioProducto.Consultar();

                // Contamos total
                int total = query.Count();

                // Retornamos
                return total;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<int> TotalCategorias()
        {
            try
            {
                // Buscamos 
                IQueryable<Categoria> query = await _repositorioCategoria.Consultar();

                // Contamos total
                int total = query.Count();

                // Retornamos
                return total;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public async Task<Dictionary<string, int>> VentasUltimaSemana()
        {
            try
            {
                // Buscamos 
                IQueryable<Venta> query = await _repositorioVenta
                    .Consultar(v => v.FechaRegistro.Value.Date >= FechaInicio.Date);

                // Creamos diccionario
                Dictionary<string, int> resultado = query
                    .GroupBy(v => v.FechaRegistro.Value.Date)
                    .OrderByDescending(g => g.Key)
                    .Select(dv => new { fecha = dv.Key.ToString("dd/MM/yy"), total = dv.Count() })
                    .ToDictionary(keySelector: r => r.fecha, elementSelector : r => r.total);

                // Retornamos...
                return resultado;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<Dictionary<string, int>> ProductosTopUltimaSemana()
        {
            try
            {
                // Buscamos 
                IQueryable<DetalleVenta> query = await _repositorioDetalleVenta.Consultar();

                // Creamos diccionario
                Dictionary<string, int> resultado = query
                    .Include(v => v.IdVentaNavigation)
                    .Where(dv  => dv.IdVentaNavigation.FechaRegistro.Value.Date >= FechaInicio.Date)
                    .GroupBy(dv => dv.DescripcionProducto)
                    .OrderByDescending(g => g.Count())
                    .Select(dv => new { producto = dv.Key, total = dv.Count() }).Take(4)
                    .ToDictionary(keySelector: r => r.producto, elementSelector: r => r.total);

                // Retornamos...
                return resultado;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

    }
}
