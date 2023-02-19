using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// 
using SistemaVenta.BLL.Interfaces;
using System.Security.Cryptography;

namespace SistemaVenta.BLL.Implementacion
{
    public class UtilidadesService : IUtilidadesService
    {
        // Función para Generar clave
        public string GenerarClave()
        {
            // Generamos string de clave
            string clave = Guid.NewGuid().ToString("N").Substring(0, 6);

            // Retornamos clave generada
            return clave;
        }

        // Evento para encriptar contraseña
        public string ConvertirSHA256(string texto)
        {
            // Instanciamos
            StringBuilder sb = new StringBuilder();

            // Generamos hash SHA256
            using(SHA256 hash = SHA256Managed.Create())
            {

                Encoding enc = Encoding.UTF8;

                // Convertimos string de texto en array de bytes
                byte[] result = hash.ComputeHash(enc.GetBytes(texto));

                // Iteramos result
                foreach(byte b in result)
                {
                    sb.Append(b.ToString("X2"));
                }
            }

            // Retornamos...
            return sb.ToString();
        }

    }
}
