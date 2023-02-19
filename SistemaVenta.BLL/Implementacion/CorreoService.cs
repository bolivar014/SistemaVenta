using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// 
using System.Net;
using System.Net.Mail;

using SistemaVenta.BLL.Interfaces;
using SistemaVenta.DAL.Interfaces;
using SistemaVenta.Entity;

namespace SistemaVenta.BLL.Implementacion
{
    public class CorreoService : ICorreoService
    {
        private readonly IGenericRepository<Configuracion> _repositorio;

        public CorreoService(IGenericRepository<Configuracion> repositorio)
        {
            _repositorio = repositorio;
        }
        public async Task<bool> EnviarCorreo(string CorreoDestino, string Asunto, string Mensaje)
        {
            try
            {
                // Creamos variable tipo query de configuración para consultar todos los recursos "Servicio_Correo"
                IQueryable<Configuracion> query = await _repositorio.Consultar(c => c.Recurso.Equals("Servicio_Correo"));

                // Creamos diccionario de datos con el objeto de la consulta anterior...
                Dictionary<string, string> Config = query.ToDictionary(keySelector : c => c.Propiedad, elementSelector : c => c.Valor);

                // Creamos credenciales
                var credenciales = new NetworkCredential(Config["correo"], Config["clave"]);

                // Configuramos el cuerpo del email
                var correo = new MailMessage()
                {
                    From = new MailAddress(Config["correo"], Config["alias"]),
                    Subject = Asunto,
                    Body = Mensaje,
                    IsBodyHtml = true
                };

                // Hacía quien se envia el email
                correo.To.Add(new MailAddress(CorreoDestino));

                // Creamos SMTP de envio de email
                var clienteServidor = new SmtpClient()
                {
                    Host = Config["host"],
                    Port = int.Parse(Config["puerto"]),
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    EnableSsl = true
                };

                // Ejecutamos envio de correo
                clienteServidor.Send(correo);

                // Retornamos true cuando todo se envia correctamente
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
