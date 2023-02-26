using Microsoft.AspNetCore.Mvc;

// 
using SistemaVenta.AplicacionWeb.Models.ViewModels;
using SistemaVenta.BLL.Interfaces;
using SistemaVenta.Entity;

// Autenticación por cookies
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;

namespace SistemaVenta.AplicacionWeb.Controllers
{
    public class AccesoController : Controller
    {
        private readonly IUsuarioService _usuarioServicio;

        public AccesoController(IUsuarioService usuarioServicio)
        {
            _usuarioServicio = usuarioServicio;
        }

        public IActionResult Login()
        {
            // Verificamos si ya existe una sesión
            ClaimsPrincipal claimUser = HttpContext.User;

            // Si existe una sesión anterior, redireccionamos...
            if(claimUser.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(VMUsuarioLogin modelo)
        {
            // Buscamos usuario si existe en el modelo
            Usuario usuario_encontrado = await _usuarioServicio.ObtenerPorUsuario(modelo.Correo, modelo.Clave);
            
            if(usuario_encontrado == null)
            {
                ViewData["Mensaje"] = "No se encontraron coincidencias...";
                return View();
            }

            // En caso que exista, dejo mensaje null
            ViewData["Mensaje"] = null;

            // Guardamos la información del usuario a autenticar
            List<Claim> claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, usuario_encontrado.Nombre),
                new Claim(ClaimTypes.NameIdentifier, usuario_encontrado.IdUsuario.ToString()),
                new Claim(ClaimTypes.Role, usuario_encontrado.IdRol.ToString()),
                new Claim("UrlFoto", usuario_encontrado.UrlFoto),
            };

            // Registramos el claims a autenticar
            ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            // Propiedades a permitir
            AuthenticationProperties properties = new AuthenticationProperties()
            {
                AllowRefresh = true, // Permitira refrescar el navegador
                IsPersistent = modelo.MantenerSesion
            };

            // Registramos propiedades para el inicio de sesión
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme, 
                new ClaimsPrincipal(claimsIdentity),
                properties);

            // Redireccionamos...
            return RedirectToAction("Index", "Home");
        }
    }
}
