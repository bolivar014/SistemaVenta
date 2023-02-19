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
    public class RolService : IRolService
    {
        private readonly IGenericRepository<Rol> _repositorio;

        public RolService(IGenericRepository<Rol> repositorio)
        {
            _repositorio = repositorio;
        }
        // Lista todos los roles existentes
        public async Task<List<Rol>> Lista()
        {
            // Creamos query para consultar todos los roles existentes...
            IQueryable<Rol> query = await _repositorio.Consultar();

            // Retornamos el resultado del query
            return query.ToList();
        }
    }
}
