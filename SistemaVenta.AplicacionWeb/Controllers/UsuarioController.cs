using Microsoft.AspNetCore.Mvc;

// 
using AutoMapper;
using Newtonsoft.Json;
using SistemaVenta.AplicacionWeb.Models.ViewModels;
using SistemaVenta.AplicacionWeb.Utilidades.Response;
using SistemaVenta.BLL.Interfaces;
using SistemaVenta.Entity;

namespace SistemaVenta.AplicacionWeb.Controllers
{
    public class UsuarioController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IUsuarioService _usuarioServicio;
        private readonly IRolService _rolService;

        public UsuarioController(
            IMapper mapper, 
            IUsuarioService usuarioServicio, 
            IRolService rolService
            )
        {
            _mapper = mapper;
            _usuarioServicio = usuarioServicio;
            _rolService = rolService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ListaRoles()
        {
            // Obtenemos objeto tipo lista de roles
            List<VMRol> vmListaRoles = _mapper.Map<List<VMRol>>(await _rolService.Lista());

            // Retornamos vmListaRoles cuando obtiene un status 200 en el request
            return StatusCode(StatusCodes.Status200OK, vmListaRoles);
        }

        [HttpGet]
        public async Task<IActionResult> Lista()
        {
            // Obtenemos objeto tipo lista de roles
            List<VMUsuario> vmUsuarioLista = _mapper.Map<List<VMUsuario>>(await _usuarioServicio.Lista());

            // Retornamos vmListaRoles cuando obtiene un status 200 en el request
            return StatusCode(StatusCodes.Status200OK, new { data = vmUsuarioLista });
        }

        [HttpPost]
        public async Task<IActionResult> Crear([FromForm] IFormFile foto, [FromForm] string modelo)
        {
            GenericResponse<VMUsuario> gResponse = new GenericResponse<VMUsuario>();

            try
            {
                VMUsuario vmUsuario = JsonConvert.DeserializeObject<VMUsuario>(modelo);

                string nombreFoto = "";
                Stream fotoStream = null;

                // Validamos
                if (foto != null)
                {
                    string nombre_en_codigo = Guid.NewGuid().ToString("N"); // Generamos un nombre con letras y numeros
                    string extension = Path.GetExtension(foto.FileName);    // Obtenemos la extensión del archivo
                    nombreFoto = string.Concat(nombre_en_codigo, extension);
                    fotoStream = foto.OpenReadStream();
                }

                // Construimos la URL de la plantilla del correo.
                string urlPlantillaCorreo = $"{this.Request.Scheme}://{this.Request.Host}/Plantilla/EnviarClave?correo=[correo]&clave=[clave]";

                // Obtenemos el usuario creado
                Usuario usuario_creado = await _usuarioServicio.Crear(_mapper.Map<Usuario>(vmUsuario), fotoStream, nombreFoto, urlPlantillaCorreo);

                // Convertimos el resultado a tipo VMModel para que la vista sea indexada
                vmUsuario = _mapper.Map<VMUsuario>(usuario_creado);

                // Objeto de respuesta
                gResponse.Estado = true;
                gResponse.Objeto = vmUsuario;

            }
            catch (Exception ex)
            {
                // Objeto de respuesta
                gResponse.Estado = false;
                gResponse.Mensaje = ex.Message;

            }

            // Retornamos respuesta...
            return StatusCode(StatusCodes.Status200OK, gResponse);
        }

        [HttpPut]
        public async Task<IActionResult> Editar([FromForm] IFormFile foto, [FromForm] string modelo)
        {
            GenericResponse<VMUsuario> gResponse = new GenericResponse<VMUsuario>();

            try
            {
                VMUsuario vmUsuario = JsonConvert.DeserializeObject<VMUsuario>(modelo);

                string nombreFoto = "";
                Stream fotoStream = null;

                // Validamos
                if (foto != null)
                {
                    string nombre_en_codigo = Guid.NewGuid().ToString("N"); // Generamos un nombre con letras y numeros
                    string extension = Path.GetExtension(foto.FileName);    // Obtenemos la extensión del archivo
                    nombreFoto = string.Concat(nombre_en_codigo, extension);
                    fotoStream = foto.OpenReadStream();
                }

                // Obtenemos el usuario creado
                Usuario usuario_editado = await _usuarioServicio.Editar(_mapper.Map<Usuario>(vmUsuario), fotoStream, nombreFoto);

                // Convertimos el resultado a tipo VMModel para que la vista sea indexada
                vmUsuario = _mapper.Map<VMUsuario>(usuario_editado);

                // Objeto de respuesta
                gResponse.Estado = true;
                gResponse.Objeto = vmUsuario;
            }
            catch (Exception ex)
            {
                // Objeto de respuesta
                gResponse.Estado = false;
                gResponse.Mensaje = ex.Message;

            }

            // Retornamos respuesta...
            return StatusCode(StatusCodes.Status200OK, gResponse);
        }

        [HttpDelete]
        public async Task<IActionResult> Eliminar(int idUsuario)
        {
            GenericResponse<VMUsuario> gResponse = new GenericResponse<VMUsuario>();

            try
            {
                gResponse.Estado = await _usuarioServicio.Eliminar(idUsuario);
            }
            catch(Exception ex)
            {
                gResponse.Estado = false;
                gResponse.Mensaje = ex.Message;
            }

            // Retornamos...
            return StatusCode(StatusCodes.Status200OK, gResponse);
        }
    }
}
