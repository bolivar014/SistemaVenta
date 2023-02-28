using SistemaVenta.BLL.Interfaces;
using SistemaVenta.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// 
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Text;
using SistemaVenta.BLL.Interfaces;
using SistemaVenta.DAL.Interfaces;
using SistemaVenta.Entity;

namespace SistemaVenta.BLL.Implementacion
{
    public class UsuarioService : IUsuarioService
    {
        private readonly IGenericRepository<Usuario> _repositorio;
        private readonly IFireBaseService _firebaseService;
        private readonly IUtilidadesService _utilidadesService;
        private readonly ICorreoService _correoService;

        public UsuarioService(
            IGenericRepository<Usuario> repositorio, 
            IFireBaseService firebaseService, 
            IUtilidadesService utilidadesService, 
            ICorreoService correoService
            )
        {
            _repositorio = repositorio;
            _firebaseService = firebaseService;
            _utilidadesService = utilidadesService;
            _correoService = correoService;

        }
        public async Task<List<Usuario>> Lista()
        {
            // Script para consultar todos los usuarios
            IQueryable<Usuario> query = await _repositorio.Consultar();

            // Retornamos...
            return query.Include(r => r.IdRolNavigation).ToList();
        }

        public async Task<Usuario> Crear(Usuario entidad, Stream Foto = null, string NombreFoto = "", string UrlPlantillaCorreo = null)
        {
            // Validamos si el correo existe en el objeto de base de datos
            Usuario usuario_existe = await _repositorio.Obtener(u => u.Correo == entidad.Correo);

            // Comprobamos
            if(usuario_existe != null)
            {
                throw new TaskCanceledException("El correo ya existe...");
            }

            try
            {
                // Generamos una clave aleatoria
                string clave_generada = _utilidadesService.GenerarClave();

                // Ciframos la clave generada
                entidad.Clave = _utilidadesService.ConvertirSHA256(clave_generada);

                entidad.NombreFoto = NombreFoto;

                if(Foto != null)
                {
                    string urlFoto = await _firebaseService.SubirStorage(Foto, "carpeta_usuario", NombreFoto);
                    entidad.UrlFoto = urlFoto;
                }

                Usuario usuario_creado = await _repositorio.Crear(entidad);

                if(usuario_creado.IdUsuario == 0)
                {
                    throw new TaskCanceledException("No se pudo crear el usuario...");
                }

                if(UrlPlantillaCorreo != "")
                {
                    UrlPlantillaCorreo = UrlPlantillaCorreo.Replace("[correo]", usuario_creado.Correo).Replace("[clave]", clave_generada);

                    string htmlCorreo = "";

                    // Hacemos una petición hacía una URL para la creación de correo
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(UrlPlantillaCorreo);

                    // Hacemos la solicitud para responder.
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                
                    // En caso que el request sea http 200
                    if(response.StatusCode == HttpStatusCode.OK)
                    {

                        using (Stream dataStream = response.GetResponseStream())
                        {
                            StreamReader readerStream = null;

                            if(response.CharacterSet == null)
                            {
                                readerStream = new StreamReader(dataStream);
                            } 
                            else
                            {
                                readerStream = new StreamReader(dataStream, Encoding.GetEncoding(response.CharacterSet));
                            }

                            htmlCorreo = readerStream.ReadToEnd();

                            response.Close();
                            readerStream.Close();
                        }
                    }

                    // Verificamos para notificar creación de correo
                    if(htmlCorreo != "")
                    {
                        // Enviamos request
                        await _correoService.EnviarCorreo(usuario_creado.Correo, "Cuenta Creada", htmlCorreo);
                    }
                }

                // Consultamos lista de usuarios a crear
                IQueryable<Usuario> query = await _repositorio.Consultar(u => u.IdUsuario == usuario_creado.IdUsuario);

                // Obtenemos el usuario creado
                usuario_creado = query.Include(r => r.IdRolNavigation).First();

                // Retornamos...
                return usuario_creado;
            }
            catch(Exception ex)
            {
                throw;
            }
        }

        public async Task<Usuario> Editar(Usuario entidad, Stream Foto = null, string NombreFoto = "")
        {

            // Validamos si el correo existe en el objeto de base de datos
            Usuario usuario_existe = await _repositorio.Obtener(u => u.Correo == entidad.Correo && u.IdUsuario != entidad.IdUsuario);

            // Comprobamos
            if (usuario_existe != null)
            {
                throw new TaskCanceledException("El correo ya existe...");
            }

            try
            {
                // Consultamos usuarios
                IQueryable<Usuario> queryUsuario = await _repositorio.Consultar(u => u.IdUsuario == entidad.IdUsuario);

                // Consultamos el primer usuario a editar
                Usuario usuario_editar = queryUsuario.First();

                // Sincronizamos campos a actualizar
                usuario_editar.Nombre = entidad.Nombre;
                usuario_editar.Correo = entidad.Correo;
                usuario_editar.Telefono = entidad.Telefono;
                usuario_editar.IdRol = entidad.IdRol;
                usuario_editar.EsActivo = entidad.EsActivo;

                // Si nombre de foto es vacio | procedemos a sincronizar
                if(usuario_editar.NombreFoto == "")
                {
                    usuario_editar.NombreFoto = NombreFoto;
                }

                // Si llega algun stream para una nueva foto
                if(Foto != null)
                {
                    string urlFoto = await _firebaseService.SubirStorage(Foto, "carpeta_usuario", usuario_editar.NombreFoto);
                    usuario_editar.UrlFoto = urlFoto;
                }

                // Verificamos respuesta
                bool respuesta = await _repositorio.Editar(usuario_editar);

                if (!respuesta)
                {
                    throw new TaskCanceledException("No se pudo modificar el usuario...");
                }

                // Buscamos información de usuario a actualizar
                Usuario usuario_editado = queryUsuario.Include(r => r.IdRolNavigation).First();

                // Retornamos
                return usuario_editado;
            }
            catch(Exception ex)
            {
                throw;
            }
        }

        public async Task<bool> Eliminar(int idUsuario)
        {
            try
            {
                // Buscamos el usuario a eliminar
                Usuario usuario_encontrado = await _repositorio.Obtener(u => u.IdUsuario == idUsuario);

                // Verificamos si el usuario existe
                if(usuario_encontrado == null)
                {
                    throw new TaskCanceledException("El usuario no existe");
                }

                // Recuperamos el nombre de la foto del usuario a eliminar
                string nombreFoto = usuario_encontrado.NombreFoto;

                // Obtenemos respuesta de evento para eliminar
                bool respuesta = await _repositorio.Eliminar(usuario_encontrado);

                // Verificamos si el usuario se elimino correctamente del modelo, para proceder con la eliminación de la imagen en firebase
                if (respuesta)
                {
                    await _firebaseService.EliminarStorage("carpeta_usuario", nombreFoto);
                }

                return true;

            }
            catch(Exception ex)
            {
                throw;
            }
        }

        public async Task<Usuario> ObtenerPorId(int idUsuario)
        {
            // Consultamos
            IQueryable<Usuario> query = await _repositorio.Consultar(u => u.IdUsuario == idUsuario);

            // Buscamos el primer argumento que coincida
            Usuario resultado = query.Include(r => r.IdRolNavigation).FirstOrDefault();

            // Retornamos...
            return resultado;
        }

        public async Task<Usuario> ObtenerPorUsuario(string correo, string clave)
        {
            // Generamos cifrado de contraseña a procesar
            string clave_encriptada = _utilidadesService.ConvertirSHA256(clave);

            // Buscamos el usuario por contraseña y email
            Usuario usuario_encontrado = await _repositorio.Obtener(u => u.Correo.Equals(correo) && u.Clave.Equals(clave_encriptada));

            // Retornamos...
            return usuario_encontrado;
        }


        public async Task<bool> GuardarPerfil(Usuario entidad)
        {
            try
            {
                // Buscamos el usuario a actualizar
                Usuario usuario_encontrado = await _repositorio.Obtener(u => u.IdUsuario == entidad.IdUsuario);

                // Verificamos
                if(usuario_encontrado == null)
                {
                    throw new TaskCanceledException("El usuario no existe...");
                }

                // Campos a actualizar
                usuario_encontrado.Correo = entidad.Correo;
                usuario_encontrado.Telefono = entidad.Telefono;

                // Esperamos respuesta del servidor por actualización de perfil
                bool respuesta = await _repositorio.Editar(usuario_encontrado);

                // Retornamos
                return respuesta;
            }
            catch(Exception ex)
            {
                throw;
            }
        }

        public async Task<bool> CambiarClave(int idUsuario, string ClaveActual, string ClaveNueva)
        {
            try
            {
                // Buscamos el usuario a actualizar la clave
                Usuario usuario_encontrado = await _repositorio.Obtener(u => u.IdUsuario == idUsuario);

                // Verificamos
                if (usuario_encontrado == null)
                {
                    throw new TaskCanceledException("El usuario no existe...");
                }

                // Verificamos si la contraseña ingresada actualmente, es correcta
                if ((usuario_encontrado.Clave).ToUpper() != _utilidadesService.ConvertirSHA256(ClaveActual))
                {
                    throw new TaskCanceledException("La contraseña ingresada como actual, no es correcta...");
                }

                // Generamos nueva contraseña cifrada
                usuario_encontrado.Clave = _utilidadesService.ConvertirSHA256(ClaveNueva);

                // Obtenemos respuesta
                bool respuesta = await _repositorio.Editar(usuario_encontrado);

                // Retornamos
                return respuesta;
            }
            catch(Exception ex)
            {
                throw;
            }
        }

        public async Task<bool> RestablecerClave(string correo, string UrlPlantillaCorreo)
        {
            try
            {
                // Buscamos el usuario a restablecer la clave
                Usuario usuario_encontrado = await _repositorio.Obtener(u => u.Correo == correo);

                // Verificamos
                if (usuario_encontrado == null)
                {
                    throw new TaskCanceledException("No encontramos ningún usuario asociado al correo...");
                }

                // Generamos nueva contraseña
                string claveGenerada = _utilidadesService.GenerarClave();

                // Obtenemos nueva contraseña cifrada
                usuario_encontrado.Clave = _utilidadesService.ConvertirSHA256(claveGenerada);

                UrlPlantillaCorreo = UrlPlantillaCorreo.Replace("[clave]", claveGenerada);

                string htmlCorreo = "";

                // Hacemos una petición hacía una URL para la creación de correo
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(UrlPlantillaCorreo);

                // Hacemos la solicitud para responder.
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                // En caso que el request sea http 200
                if (response.StatusCode == HttpStatusCode.OK)
                {

                    using (Stream dataStream = response.GetResponseStream())
                    {
                        StreamReader readerStream = null;

                        if (response.CharacterSet == null)
                        {
                            readerStream = new StreamReader(dataStream);
                        }
                        else
                        {
                            readerStream = new StreamReader(dataStream, Encoding.GetEncoding(response.CharacterSet));
                        }

                        htmlCorreo = readerStream.ReadToEnd();

                        response.Close();
                        readerStream.Close();
                    }
                }

                bool correo_enviado = false;

                // Verificamos para notificar creación de correo
                if (htmlCorreo != "")
                {
                    // Enviamos request
                    correo_enviado = await _correoService.EnviarCorreo(correo, "Contraseña Restablecida.", htmlCorreo);
                }

                // Verificamos si email se envio...
                if(!correo_enviado)
                {
                    throw new TaskCanceledException("Tenemos problemas. Por favor inténtalo más tarde...");
                }

                // Obtenemos respuesta
                bool respuesta = await _repositorio.Editar(usuario_encontrado);

                // Retornamos
                return respuesta;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
