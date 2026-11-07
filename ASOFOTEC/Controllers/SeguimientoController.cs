using ASOFOTEC.Helpers;
using ASOFOTEC.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ASOFOTEC.Controllers
{
    [Authorize]
    [AutoValidateAntiforgeryToken]
    public class SeguimientoController : Controller
    {
        private readonly ISeguimientoService _seguimiento;

        public SeguimientoController(ISeguimientoService seguimiento)
        {
            _seguimiento = seguimiento;
        }

        // Seguir / dejar de seguir (AJAX). Devuelve el nuevo estado y el total de seguidores
        [HttpPost]
        public async Task<IActionResult> Alternar(int id)
        {
            var usuarioId = HttpContext.ObtenerUsuarioId();
            if (usuarioId == null) return Unauthorized();

            var (resultado, error) = await _seguimiento.AlternarAsync(usuarioId.Value, id);
            if (resultado == null)
            {
                return BadRequest(new { success = false, message = error });
            }

            return Json(new { success = true, siguiendo = resultado.Siguiendo, seguidores = resultado.TotalSeguidores });
        }
    }
}
