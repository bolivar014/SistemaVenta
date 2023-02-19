using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// 
using SistemaVenta.Entity;
namespace SistemaVenta.BLL.Interfaces
{
    public interface IUsuarioService
    {
        // Listamos todos los usuarios
        Task<List<Usuario>> Lista();

        // Evento para crear usuario
        Task<Usuario> Crear(Usuario entidad, Stream Foto = null, string NombreFoto = "", string UrlPlantillaCorreo = null);

        // Evento para la edición de usuario
        Task<Usuario> Editar(Usuario entidad, Stream Foto = null, string NombreFoto = "");

        // Evento para la eliminación de usuarios
        Task<bool> Eliminar(int idUsuario);

        // Evento para consultar por ID de usuario
        Task<Usuario> ObtenerPorId(int idUsuario);

        // Evento para identificar el usuario por medio de la autenticación
        Task<Usuario> ObtenerPorUsuario(string correo, string clave);

        // Evento para actualizar su perfil
        Task<bool> GuardarPerfil(Usuario entidad);

        // Evento para cambiar contraseña
        Task<bool> CambiarClave(int idUsuario, string ClaveActual, string ClaveNueva);
        
        // Evento para restablecer clave
        Task<bool> RestablecerClave(string correo, string UrlPlantillaCorreo);


    }
}
