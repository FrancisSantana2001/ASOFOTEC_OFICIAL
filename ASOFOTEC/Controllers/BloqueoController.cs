using ASOFOTEC.Helpers;
using ASOFOTEC.Hubs;
using ASOFOTEC.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace ASOFOTEC.Controllers
{
    [Authorize]
    [AutoValidateAntiforgeryToken]
    public class BloqueoController : Controller
    {
        private readonly IBloqueoService _bloqueos;
        private readonly IHubContext<ChatHub> _hub;

        public BloqueoController(IBloqueoService bloqueos, IHubContext<ChatHub> hub)
        {
            _bloqueos = bloqueos;
            _hub = hub;
        }

        // Bloquear o desbloquear (AJAX)
        [HttpPost]
        public async Task<IActionResult> Alternar(int id)
        {
            var usuarioId = HttpContext.ObtenerUsuarioId();
            if (usuarioId == null) return Unauthorized();

            var (bloqueado, error) = await _bloqueos.AlternarAsync(usuarioId.Value, id);
            if (bloqueado == null)
            {
                return BadRequest(new { success = false, message = error });
            }

            // Si alguna de las dos personas tiene el chat abierto, se entera de inmediato
            await _hub.Clients.User(usuarioId.Value.ToString()).SendAsync("EstadoChatCambiado", id);
            await _hub.Clients.User(id.ToString()).SendAsync("EstadoChatCambiado", usuarioId.Value);

            return Json(new { success = true, bloqueado = bloqueado.Value });
        }
    }
}
