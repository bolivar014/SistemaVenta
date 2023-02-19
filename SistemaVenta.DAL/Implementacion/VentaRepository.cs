using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// 
using Microsoft.EntityFrameworkCore;
using SistemaVenta.DAL.DBContext;
using SistemaVenta.DAL.Interfaces;
using SistemaVenta.Entity;

namespace SistemaVenta.DAL.Implementacion
{
    public class VentaRepository : GenericRepository<Venta>, IVentaRepository
    {
        private readonly DbventaContext _dbContext;
        public VentaRepository(DbventaContext dbContext): base(dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<Venta> Registrar(Venta entidad)
        {
            Venta ventaGenerada = new Venta();

            using(var transaction = _dbContext.Database.BeginTransaction())
            {
                try
                {
                    foreach(DetalleVenta dv in entidad.DetalleVenta)
                    {
                        // Buscamos el producto a iterar para restar al stock
                        Producto producto_encontrado = _dbContext.Productos.Where(p => p.IdProducto == dv.IdProducto).First();

                        // Restamos stock de producto
                        producto_encontrado.Stock = producto_encontrado.Stock - dv.Cantidad;

                        // Actualizamos
                        _dbContext.Productos.Update(producto_encontrado);
                    }

                    // Esperamos evento...
                    await _dbContext.SaveChangesAsync();

                    // Numero correlativo...
                    NumeroCorrelativo correlativo = _dbContext.NumeroCorrelativos.Where(n => n.Gestion == "venta").First();

                    correlativo.UltimoNumero = correlativo.UltimoNumero + 1;
                    correlativo.FechaActualizacion = DateTime.Now;

                    // Actualizamos numero correlativo...
                    _dbContext.NumeroCorrelativos.Update(correlativo);

                    // Esperamos evento...
                    await _dbContext.SaveChangesAsync();

                    // Generamos string de ceros
                    string ceros = string.Concat(Enumerable.Repeat("0", correlativo.CantidadDigitos.Value));

                    // Generamos numero de venta
                    string numeroVenta = ceros + correlativo.UltimoNumero.ToString();

                    // 
                    numeroVenta = numeroVenta.Substring(numeroVenta.Length - correlativo.CantidadDigitos.Value, correlativo.CantidadDigitos.Value);

                    // 
                    entidad.NumeroVenta = numeroVenta;

                    // Agregamos registro a modelo de datos
                    await _dbContext.Venta.AddAsync(entidad);

                    // Guardamos...
                    await _dbContext.SaveChangesAsync();

                    // 
                    ventaGenerada = entidad;

                    // Evento que se encarga de confirmar la creación de la transacción de manera exitosa.
                    transaction.Commit();

                }
                catch(Exception ex)
                {
                    // En caso que suceda algún error en la transacción, devolvemos el error
                    transaction.Rollback();

                    throw ex;
                }

                // Retornamos...
                return ventaGenerada;
            }
        }

        public async Task<List<DetalleVenta>> Reporte(DateTime FechaInicio, DateTime FechaFin)
        {
            List<DetalleVenta> listaResumen = await _dbContext.DetalleVenta
                .Include(v => v.IdVentaNavigation)
                .ThenInclude(u => u.IdUsuarioNavigation)
                .Include(v => v.IdVentaNavigation)
                .ThenInclude(tdv => tdv.IdTipoDocumentoVentaNavigation)
                .Where(dv => dv.IdVentaNavigation.FechaRegistro.Value.Date >= FechaInicio.Date &&
                dv.IdVentaNavigation.FechaRegistro.Value.Date <= FechaFin.Date).ToListAsync();

            // Retornamos...
            return listaResumen;
        }
    }
}
