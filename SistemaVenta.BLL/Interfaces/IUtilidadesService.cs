using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaVenta.BLL.Interfaces
{
    public interface IUtilidadesService
    {

        // Metodo para la generación de claves
        string GenerarClave();

        //
        string ConvertirSHA256(string texto);
    }
}
