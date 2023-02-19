using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// 
using SistemaVenta.DAL.DBContext;
using SistemaVenta.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace SistemaVenta.DAL.Implementacion
{
    public class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : class
    {
        // Obtenemos el contexto 
        private readonly DbventaContext _dbContext;

        // Constructor para inicialización del contexto
        public GenericRepository(DbventaContext dbContext)
        {
            _dbContext = dbContext;
        }
        
        // Evento para obtener
        public async Task<TEntity> Obtener(Expression<Func<TEntity, bool>> filtro)
        {
            try
            {
                // Accedemos al contexto de la entidad a procesar y recuperamos el primero de las coincidencias
                TEntity entidad = await _dbContext.Set<TEntity>().FirstOrDefaultAsync(filtro);

                // Retornamos
                return entidad;
            } 
            catch (Exception ex)
            {
                throw;
            }
        }

        // 
        public async Task<TEntity> Crear(TEntity entidad)
        {
            try
            {
                // Creamos un nuevo registro de la entidad a procesar
               _dbContext.Set<TEntity>().Add(entidad);

                // Guardamos
                await _dbContext.SaveChangesAsync();

                // Retornamos
                return entidad;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        // 
        public async Task<bool> Editar(TEntity entidad)
        {
            try
            {
                // Actualizamos contexto de la entidad
                _dbContext.Update(entidad);

                // Guardamos
                await _dbContext.SaveChangesAsync();

                // Retornamos
                return true;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        // 
        public async Task<bool> Eliminar(TEntity entidad)
        {
            try
            {
                // Actualizamos contexto de la entidad
                _dbContext.Remove(entidad);

                // Guardamos
                await _dbContext.SaveChangesAsync();

                // Retornamos
                return true;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        // 
        public async Task<IQueryable<TEntity>> Consultar(Expression<Func<TEntity, bool>> filtro = null)
        {
            // Buscamos por medio de filtro, en caso que no existan datos a filtrar trae lista por default
            IQueryable<TEntity> queryEntidad = filtro == null ? _dbContext.Set<TEntity>() : _dbContext.Set<TEntity>().Where(filtro);

            // Retornamos
            return queryEntidad;
        }

    }
}
