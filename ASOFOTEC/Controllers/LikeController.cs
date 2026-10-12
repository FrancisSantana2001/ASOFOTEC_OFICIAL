using ASOFOTEC.Services.Interfaces;
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
    }
}
