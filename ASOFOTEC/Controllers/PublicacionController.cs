using ASOFOTEC.Dtos;
using ASOFOTEC.Helpers;
using ASOFOTEC.Services.Implementations;
using ASOFOTEC.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ASOFOTEC.Controllers
{
    [Authorize]
    public class PublicacionController : Controller
    {
        private readonly IPublicacionService _publicacionService;
        private readonly IMediaService _mediaService;
        private readonly IAdminService _admin;
        private readonly IFeedService _feed;

        public PublicacionController(IPublicacionService publicacionService, IMediaService mediaService,
                                     IAdminService admin, IFeedService feed)
        {
            _publicacionService = publicacionService;
            _mediaService = mediaService;
            _admin = admin;
            _feed = feed;
        }

        // Página de una publicación. Es pública para que el enlace compartido se pueda abrir sin cuenta
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Ver(int id)
        {
            var usuarioId = HttpContext.ObtenerUsuarioId() ?? 0;
            var publicacion = await _feed.ObtenerPublicacionAsync(id, usuarioId);

            if (publicacion == null)
            {
                TempData["Error"] = "La publicación no existe o fue eliminada.";
                return RedirectToAction("Index", "Home");
            }

            return View(publicacion);
        }

        // Se permite hasta el tamaño máximo de un video (50 MB) más un margen para el resto del formulario
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(MediaService.MaxBytesVideo + 2 * 1024 * 1024)]
        [RequestFormLimits(MultipartBodyLengthLimit = MediaService.MaxBytesVideo + 2 * 1024 * 1024)]
        public async Task<IActionResult> Crear(PublicacionCreateDto dto)
        {
            var usuarioId = HttpContext.ObtenerUsuarioId();
            if (usuarioId == null)
                return RedirectToAction("Login", "Auth");

            var hayArchivo = dto.Archivo != null && dto.Archivo.Length > 0;
            var hayTexto = !string.IsNullOrWhiteSpace(dto.Contenido);

            if (!ModelState.IsValid || (!hayTexto && !hayArchivo))
            {
                TempData["Error"] = "Escribe algo (máximo 500 caracteres) o adjunta una foto o un video.";
                return RedirectToAction("Index", "Home");
            }

            MediaGuardada? media = null;
            if (hayArchivo)
            {
                var (guardada, error) = await _mediaService.GuardarAsync(dto.Archivo!);
                if (guardada == null)
                {
                    TempData["Error"] = error ?? "No se pudo guardar el archivo.";
                    return RedirectToAction("Index", "Home");
                }
                media = guardada;
            }

            var creada = await _publicacionService.CrearAsync(dto.Contenido, media?.Url, media?.Tipo, usuarioId.Value);
            if (!creada)
            {
                // No quedó registrada: no dejar el archivo huérfano en el disco
                _mediaService.Eliminar(media?.Url);
                TempData["Error"] = "No fue posible guardar la publicación.";
            }
            else
            {
                TempData["Success"] = "Tu publicación ya está en el feed.";
            }

            return RedirectToAction("Index", "Home");
        }

        // Solo el usuario que hizo la publicación puede eliminarla
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int id)
        {
            var usuarioId = HttpContext.ObtenerUsuarioId();
            if (usuarioId == null)
                return RedirectToAction("Login", "Auth");

            var esModerador = await _admin.EsDbaAsync(usuarioId.Value);
            var eliminada = await _publicacionService.EliminarAsync(id, usuarioId.Value, esModerador);
            if (eliminada && esModerador) _admin.RegistrarModeracion(usuarioId.Value, "publicación", id);

            if (!eliminada)
                TempData["Error"] = "No tienes permiso para eliminar esta publicación.";
            else
                TempData["Success"] = "La publicación fue eliminada.";

            return RedirectToAction("Index", "Home");
        }
    }
}
