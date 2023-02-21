using SistemaVenta.BLL.Interfaces;
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
    public class NegocioService : INegocioService
    {
        private readonly IGenericRepository<Negocio> _repositorio;
        private readonly IFireBaseService _fireBaseService;

        public NegocioService(
            IGenericRepository<Negocio> repositorio,
            IFireBaseService fireBaseService
            )
        {
            _repositorio = repositorio;
            _fireBaseService = fireBaseService;
        }
        
        // 
        public async Task<Negocio> Obtener()
        {
            try
            {
                // Buscamos el negocio con id 1
                Negocio negocio_encontrado = await _repositorio.Obtener(n => n.IdNegocio == 1);

                // Retornamos negocio encontrado
                return negocio_encontrado;
            }
            catch(Exception ex)
            {
                throw;
            }
        }

        // 
        public async Task<Negocio> GuardarCambios(Negocio entidad, Stream Logo = null, string NombreLogo = "")
        {
            try
            {
                // Buscamos el negocio con id 1
                Negocio negocio_encontrado = await _repositorio.Obtener(n => n.IdNegocio == 1);

                negocio_encontrado.NumeroDocumento = entidad.NumeroDocumento;
                negocio_encontrado.Nombre = entidad.Nombre;
                negocio_encontrado.Correo = entidad.Correo;
                negocio_encontrado.Direccion = entidad.Direccion;
                negocio_encontrado.Telefono = entidad.Telefono;
                negocio_encontrado.PorcentajeImpuesto = entidad.PorcentajeImpuesto;
                negocio_encontrado.SimboloMoneda = entidad.SimboloMoneda;

                negocio_encontrado.NombreLogo = negocio_encontrado.NombreLogo == "" ? NombreLogo : negocio_encontrado.NombreLogo;

                // Verificamos que llegue algún logo, para realizar su carga en firebase...
                if(Logo != null)
                {
                    string urlLogo = await _fireBaseService.SubirStorage(Logo, "carpeta_logo", negocio_encontrado.NombreLogo);
                    negocio_encontrado.UrlLogo = urlLogo;

                }

                // Esperamos request de evento
                await _repositorio.Editar(negocio_encontrado);

                // Retornamos el negocio encontrado
                return negocio_encontrado;
            }
            catch(Exception ex)
            {
                throw;
            }
        }
    }
}
