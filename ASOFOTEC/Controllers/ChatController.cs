using ASOFOTEC.Dtos;
using ASOFOTEC.Helpers;
using ASOFOTEC.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ASOFOTEC.Controllers
{
    // Mensajes entre dos amigos. Cada acción filtra por la persona con sesión iniciada:
    // no existe ninguna ruta que permita ver una conversación ajena (ni siquiera al DBA).
    [Authorize]
    public class ChatController : Controller
    {
        private readonly IChatService _chat;
        private readonly IBloqueoService _bloqueos;

        public ChatController(IChatService chat, IBloqueoService bloqueos)
        {
            _chat = chat;
            _bloqueos = bloqueos;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int? id)
        {
            var usuarioId = HttpContext.ObtenerUsuarioId();
            if (usuarioId == null) return RedirectToAction("Login", "Auth");

            var modelo = new ChatVm
            {
                MiId = usuarioId.Value,
                Conversaciones = await _chat.ObtenerConversacionesAsync(usuarioId.Value)
            };

            if (id.HasValue)
            {
                modelo.Actual = await _chat.ObtenerConversacionAsync(usuarioId.Value, id.Value);
                if (modelo.Actual == null)
                {
                    TempData["Error"] = "Esa conversación no existe.";
                    return RedirectToAction(nameof(Index));
                }
            }
            else
            {
                modelo.Amigos = await _chat.ObtenerAmigosAsync(usuarioId.Value);
            }

            return View(modelo);
        }

        // Abre (o crea) la conversación con un amigo
        [HttpGet]
        public async Task<IActionResult> Iniciar(int id)
        {
            var usuarioId = HttpContext.ObtenerUsuarioId();
            if (usuarioId == null) return RedirectToAction("Login", "Auth");

            var (conversacionId, error) = await _chat.IniciarAsync(usuarioId.Value, id);
            if (conversacionId == null)
            {
                TempData["Error"] = error ?? "No se pudo iniciar el chat.";
                return RedirectToAction(nameof(Index));
            }

            return RedirectToAction(nameof(Index), new { id = conversacionId.Value });
        }

        // Lista de conversaciones (HTML) para refrescarla sin recargar la página
        [HttpGet]
        public async Task<IActionResult> Lista(int? activa)
        {
            var usuarioId = HttpContext.ObtenerUsuarioId();
            if (usuarioId == null) return Unauthorized();

            Response.Headers.CacheControl = "no-store";
            ViewData["Activa"] = activa;
            var lista = await _chat.ObtenerConversacionesAsync(usuarioId.Value);
            return PartialView("_ChatLista", lista);
        }

        // Mensajes anteriores (antesDeId) o posteriores (despuesDeId) a uno dado
        [HttpGet]
        public async Task<IActionResult> Mensajes(int conversacionId, int? antesDeId, int? despuesDeId)
        {
            var usuarioId = HttpContext.ObtenerUsuarioId();
            if (usuarioId == null) return Unauthorized();

            var pagina = await _chat.ObtenerMensajesAsync(usuarioId.Value, conversacionId, antesDeId, despuesDeId);
            if (pagina == null) return NotFound();

            Response.Headers.CacheControl = "no-store";
            return Json(pagina);
        }

        [HttpGet]
        public async Task<IActionResult> NoLeidos()
        {
            var usuarioId = HttpContext.ObtenerUsuarioId();
            if (usuarioId == null) return Unauthorized();

            Response.Headers.CacheControl = "no-store";
            var noLeidos = await _chat.ContarNoLeidosAsync(usuarioId.Value);
            return Json(new { noLeidos });
        }

        [HttpGet]
        public async Task<IActionResult> Bloqueados()
        {
            var usuarioId = HttpContext.ObtenerUsuarioId();
            if (usuarioId == null) return RedirectToAction("Login", "Auth");

            var lista = await _bloqueos.ObtenerBloqueadosAsync(usuarioId.Value);
            return View(lista);
        }
    }
}
