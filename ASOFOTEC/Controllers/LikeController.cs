using ASOFOTEC.Helpers;
using ASOFOTEC.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ASOFOTEC.Controllers
{
    public class LikeController : Controller
    {
        private readonly ILikeService _likeService;
        public LikeController(ILikeService likeService)
        {
            _likeService = likeService;
        }
        [Route("Like/Toggle")]
        [HttpPost]
        public async Task<IActionResult>Toggle([FromForm] int solucionId)
        {
            
            var userId = HttpContext.Session.GetString("UserId");

            if (!int.TryParse(userId, out var usuarioId))
                return Unauthorized(new { success = false });

            var liked = await _likeService.ToggleLikesAsync(solucionId, usuarioId);
            var totalLikes = await _likeService.ObtenerTotalLikesAsync(solucionId);

            return Json(new { success = true, liked, totalLikes });

        }

        // Like / quitar like a una publicación
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TogglePublicacion([FromForm] int publicacionId)
        {
            var usuarioId = HttpContext.ObtenerUsuarioId();
            if (usuarioId == null)
                return Unauthorized(new { success = false });

            var resultado = await _likeService.ToggleLikePublicacionAsync(publicacionId, usuarioId.Value);
            if (resultado == null)
                return NotFound(new { success = false });

            return Json(new { success = true, liked = resultado.Liked, totalLikes = resultado.TotalLikes });
        }

        // Like / quitar like a un comentario o respuesta
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleComentario([FromForm] int comentarioId)
        {
            var usuarioId = HttpContext.ObtenerUsuarioId();
            if (usuarioId == null)
                return Unauthorized(new { success = false });

            var resultado = await _likeService.ToggleLikeComentarioAsync(comentarioId, usuarioId.Value);
            if (resultado == null)
                return NotFound(new { success = false });

            return Json(new { success = true, liked = resultado.Liked, totalLikes = resultado.TotalLikes });
        }
    }
}
