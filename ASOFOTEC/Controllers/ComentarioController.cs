using ASOFOTEC.Dtos;
using ASOFOTEC.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ASOFOTEC.Controllers
{
    [Authorize]
    public class ComentarioController : Controller
    {
        private readonly IComentarioService _ComentarioService;
        public ComentarioController(IComentarioService comentarioService)
        {
            _ComentarioService = comentarioService;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearComentario(ComentarioCreateDto dto)
        {
            var userId = HttpContext.Session.GetString("UserId");

            if (!int.TryParse(userId, out var usuarioId))
            {
                return RedirectToAction("Login", "Auth");
            }

            if (!ModelState.IsValid || string.IsNullOrWhiteSpace(dto.Contenido))
            {
                return BadRequest();
            }

            await _ComentarioService.CrearAsync(dto, usuarioId);

            var referer = Request.Headers.Referer.ToString();
            return string.IsNullOrEmpty(referer)
                ? RedirectToAction("Index", "Problema")
                : Redirect(referer);

        }


        public IActionResult Index()
        {
            return View();
        }
    }
}
