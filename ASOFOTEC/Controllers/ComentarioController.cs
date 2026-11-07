using ASOFOTEC.Dtos;
using ASOFOTEC.Helpers;
using ASOFOTEC.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ASOFOTEC.Controllers
{
    [Authorize]
    public class ComentarioController : Controller
    {
        private readonly IComentarioService _ComentarioService;
        private readonly IAdminService _admin;
        public ComentarioController(IComentarioService comentarioService, IAdminService admin)
        {
            _ComentarioService = comentarioService;
            _admin = admin;
        }

        // Crear: responde con el HTML del comentario nuevo (para insertarlo sin recargar la página)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearComentario(ComentarioCreateDto dto)
        {
            var usuarioId = HttpContext.ObtenerUsuarioId();
            if (usuarioId == null)
            {
                return Unauthorized();
            }

            if (!ModelState.IsValid || string.IsNullOrWhiteSpace(dto.Contenido))
            {
                return BadRequest();
            }

            var comentario = await _ComentarioService.CrearAsync(dto, usuarioId.Value);
            if (comentario == null)
            {
                return NotFound();
            }

            if (Request.EsAjax())
            {
                // Respuesta -> solo el comentario; comentario principal -> el hilo completo
                return PartialView(comentario.ComentarioPadreId.HasValue ? "_ComentarioItem" : "_Comentario", comentario);
            }

            // Fallback sin JavaScript
            var referer = Request.Headers.Referer.ToString();
            return string.IsNullOrEmpty(referer)
                ? RedirectToAction("Index", "Problema")
                : Redirect(referer);
        }

        // Editar: solo el autor del comentario
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(ComentarioEditDto dto)
        {
            var usuarioId = HttpContext.ObtenerUsuarioId();
            if (usuarioId == null)
            {
                return Unauthorized();
            }

            if (!ModelState.IsValid || string.IsNullOrWhiteSpace(dto.Contenido))
            {
                return BadRequest(new { success = false, message = "El comentario no puede estar vacío." });
            }

            var editado = await _ComentarioService.EditarAsync(dto.Id, dto.Contenido, usuarioId.Value);
            if (!editado)
            {
                return StatusCode(StatusCodes.Status403Forbidden,
                    new { success = false, message = "No puedes editar este comentario." });
            }

            return Json(new { success = true, contenido = dto.Contenido.Trim() });
        }

        // Eliminar: solo el autor del comentario
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int id)
        {
            var usuarioId = HttpContext.ObtenerUsuarioId();
            if (usuarioId == null)
            {
                return Unauthorized();
            }

            var esModerador = await _admin.EsDbaAsync(usuarioId.Value);
            var eliminado = await _ComentarioService.EliminarAsync(id, usuarioId.Value, esModerador);
            if (eliminado && esModerador) _admin.RegistrarModeracion(usuarioId.Value, "comentario", id);
            if (!eliminado)
            {
                return StatusCode(StatusCodes.Status403Forbidden,
                    new { success = false, message = "No puedes eliminar este comentario." });
            }

            return Json(new { success = true });
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
