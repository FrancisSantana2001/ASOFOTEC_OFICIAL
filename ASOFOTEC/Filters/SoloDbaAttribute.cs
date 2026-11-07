using ASOFOTEC.Helpers;
using ASOFOTEC.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ASOFOTEC.Filters
{
    // Restringe una acción o controlador al administrador DBA.
    // Se consulta la base de datos en cada petición, así que perder el rol surte efecto de inmediato
    // (aunque el token del usuario todavía no haya vencido).
    public class SoloDbaAttribute : TypeFilterAttribute
    {
        public SoloDbaAttribute() : base(typeof(SoloDbaFilter))
        {
        }

        public class SoloDbaFilter : IAsyncAuthorizationFilter
        {
            private readonly IAdminService _admin;

            public SoloDbaFilter(IAdminService admin)
            {
                _admin = admin;
            }

            public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
            {
                var usuarioId = context.HttpContext.ObtenerUsuarioId();
                if (usuarioId == null)
                {
                    context.Result = new ChallengeResult();
                    return;
                }

                if (!await _admin.EsDbaAsync(usuarioId.Value))
                {
                    // Sin pistas sobre la existencia del panel: se devuelve al inicio
                    context.Result = new RedirectToActionResult("Index", "Home", null);
                }
            }
        }
    }
}
