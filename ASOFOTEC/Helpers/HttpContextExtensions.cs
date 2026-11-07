using System.Security.Claims;

namespace ASOFOTEC.Helpers
{
    public static class HttpContextExtensions
    {
        /// <summary>
        /// Devuelve el Id del usuario autenticado (según el token JWT), o null si no hay sesión válida.
        /// No se usa la sesión de ASP.NET como respaldo: si el token venció o se invalidó,
        /// el usuario debe considerarse sin sesión para que el login y las páginas protegidas coincidan.
        /// </summary>
        public static int? ObtenerUsuarioId(this HttpContext context)
        {
            if (context.User.Identity?.IsAuthenticated != true)
            {
                return null;
            }

            var valor = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(valor, out var id) ? id : null;
        }

        /// <summary>Rol del usuario autenticado (Usuario, Admin o DBA).</summary>
        public static string? ObtenerRol(this HttpContext context)
        {
            return context.User.Identity?.IsAuthenticated == true
                ? context.User.FindFirstValue(ClaimTypes.Role)
                : null;
        }

        /// <summary>
        /// Solo para decidir qué botones mostrar. La autorización real se valida siempre en el
        /// servidor contra la base de datos (IAdminService.EsDbaAsync).
        /// </summary>
        public static bool EsDba(this HttpContext context)
        {
            return context.ObtenerRol() == RolesSistema.Dba;
        }

        public static bool EsAjax(this HttpRequest request)
        {
            return request.Headers["X-Requested-With"] == "XMLHttpRequest";
        }
    }
}
