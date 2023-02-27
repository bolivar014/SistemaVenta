// 
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace SistemaVenta.AplicacionWeb.Utilidades.ViewComponents
{
    public class MenuUsuarioViewComponent : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync()
        {
            ClaimsPrincipal claimUser = HttpContext.User;

            // Inicializamos 
            string nombreUsuario = "";
            string urlFotoUsuario = "";

            // Verificamos que el Claim este autenticado y accedemos a sus propiedades
            if(claimUser.Identity.IsAuthenticated)
            {
                nombreUsuario = claimUser.Claims
                    .Where(c => c.Type == ClaimTypes.Name) // Accedemos a ClaimTypes.Name de cuando se creo el inicio de sesión
                    .Select(c => c.Value).SingleOrDefault();

                urlFotoUsuario = ((ClaimsIdentity)claimUser.Identity).FindFirst("UrlFoto").Value;
            }

            // Generamos ViewData de información
            ViewData["nombreUsuario"] = nombreUsuario;
            ViewData["urlFotoUsuario"] = urlFotoUsuario;

            // Retornamos...
            return View();
        }
    }
}
