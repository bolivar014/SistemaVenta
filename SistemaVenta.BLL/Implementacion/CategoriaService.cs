using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//
using SistemaVenta.BLL.Interfaces;
using SistemaVenta.DAL.Interfaces;
using SistemaVenta.Entity;

namespace SistemaVenta.BLL.Implementacion
{
    public class CategoriaService : ICategoriaService
    {
        private readonly IGenericRepository<Categoria> _repositorio;

        public CategoriaService(IGenericRepository<Categoria> repositorio)
        {
            _repositorio = repositorio;
        }

        public async Task<List<Categoria>> Lista()
        {
            // Query
            IQueryable<Categoria> query = await _repositorio.Consultar();

            // Retornamos query
            return query.ToList();
        }

        public async Task<Categoria> Crear(Categoria entidad)
        {
            try
            {
                Categoria categoria_creada = await _repositorio.Crear(entidad);

                if(categoria_creada.IdCategoria == 0)
                {
                    throw new TaskCanceledException("No se pudo crear la categoria...");
                }

                return categoria_creada;
            }
            catch(Exception ex)
            {
                throw;
            }
        }

        public async Task<Categoria> Editar(Categoria entidad)
        {
            try
            {
                // Buscamos la categoria en el model
                Categoria categoria_encontrada = await _repositorio.Obtener(c => c.IdCategoria == entidad.IdCategoria);

                // Generamos objeto a actualizar
                categoria_encontrada.Descripcion = entidad.Descripcion;
                categoria_encontrada.EsActivo = entidad.EsActivo;

                bool respuesta = await _repositorio.Editar(categoria_encontrada);

                if(!respuesta)
                {
                    throw new TaskCanceledException("No se pudo editar la categoria...");
                }

                // Retornamos
                return categoria_encontrada;

            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<bool> Eliminar(int idCategoria)
        {
            try
            {
                // Buscamos la categoria en el model
                Categoria categoria_encontrada = await _repositorio.Obtener(c => c.IdCategoria == idCategoria);

                // Verificamos
                if(categoria_encontrada == null)
                {
                    throw new TaskCanceledException("La categoria no existe...");
                }

                // Eliminamos
                bool respuesta = await _repositorio.Eliminar(categoria_encontrada);

                return respuesta;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

    }
}
