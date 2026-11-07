using ASOFOTEC.Dtos;
using ASOFOTEC.Helpers;
using ASOFOTEC.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ASOFOTEC.Controllers
{
    [Authorize]
    [AutoValidateAntiforgeryToken]
    public class NotificacionesController : Controller
    {
        private const int TamanoPagina = 20;

        private readonly INotificacionService _notificaciones;

        public NotificacionesController(INotificacionService notificaciones)
        {
            _notificaciones = notificaciones;
        }

        // Página con todas las notificaciones
        [HttpGet]
        public async Task<IActionResult> Index(int pagina = 1)
        {
            var usuarioId = HttpContext.ObtenerUsuarioId();
            if (usuarioId == null) return RedirectToAction("Login", "Auth");

            var (items, hayMas) = await _notificaciones.ObtenerPaginaAsync(usuarioId.Value, pagina, TamanoPagina);
            return View(new NotificacionesVm { Items = items, HayMas = hayMas, Pagina = Math.Max(1, pagina) });
        }

        // Número de avisos sin leer (la campana lo consulta cada cierto tiempo)
        [HttpGet]
        public async Task<IActionResult> Resumen()
        {
            var usuarioId = HttpContext.ObtenerUsuarioId();
            if (usuarioId == null) return Unauthorized();

            Response.Headers.CacheControl = "no-store";
            var noLeidas = await _notificaciones.ContarNoLeidasAsync(usuarioId.Value);
            return Json(new { noLeidas });
        }

        // Contenido del desplegable de la campana (HTML)
        [HttpGet]
        public async Task<IActionResult> Recientes()
        {
            var usuarioId = HttpContext.ObtenerUsuarioId();
            if (usuarioId == null) return Unauthorized();

            Response.Headers.CacheControl = "no-store";
            var lista = await _notificaciones.ObtenerRecientesAsync(usuarioId.Value, 8);
            return PartialView("_NotificacionesLista", lista);
        }

        // Marca el aviso como leído y lleva a la publicación, solución, problema o perfil correspondiente
        [HttpGet]
        public async Task<IActionResult> Abrir(int id)
        {
            var usuarioId = HttpContext.ObtenerUsuarioId();
            if (usuarioId == null) return RedirectToAction("Login", "Auth");

            var url = await _notificaciones.AbrirAsync(id, usuarioId.Value);

            // Solo se redirige a rutas del propio sitio
            if (url != null && Url.IsLocalUrl(url))
            {
                return LocalRedirect(url);
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> MarcarTodas()
        {
            var usuarioId = HttpContext.ObtenerUsuarioId();
            if (usuarioId == null) return Unauthorized();

            await _notificaciones.MarcarTodasAsync(usuarioId.Value);

            if (Request.EsAjax()) return Json(new { success = true });
            return RedirectToAction(nameof(Index));
        }
    }
}
