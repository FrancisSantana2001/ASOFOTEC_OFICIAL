using ASOFOTEC.Filters;
using ASOFOTEC.Helpers;
using ASOFOTEC.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ASOFOTEC.Controllers
{
    // Panel del administrador único (DBA): métricas y gestión de usuarios.
    // La moderación de contenido (eliminar publicaciones, problemas, soluciones y comentarios ajenos)
    // se hace desde cada elemento, con el botón de eliminar que solo el DBA ve en lo que no es suyo.
    [Authorize]
    [SoloDba]
    [AutoValidateAntiforgeryToken]
    public class DbaController : Controller
    {
        private readonly IAdminService _admin;

        public DbaController(IAdminService admin)
        {
            _admin = admin;
        }

        public async Task<IActionResult> Index()
        {
            var metricas = await _admin.ObtenerMetricasAsync();
            return View(metricas);
        }

        public async Task<IActionResult> Usuarios(string? q)
        {
            ViewData["Busqueda"] = q;
            var usuarios = await _admin.ObtenerUsuariosAsync(q);
            return View(usuarios);
        }

        [HttpPost]
        public async Task<IActionResult> CambiarEstado(int id, bool activo, string? q)
        {
            var actorId = HttpContext.ObtenerUsuarioId()!.Value;
            var error = await _admin.CambiarEstadoAsync(id, activo, actorId);

            if (error != null) TempData["Error"] = error;
            else TempData["Success"] = activo ? "La cuenta fue activada." : "La cuenta fue desactivada.";

            return RedirectToAction(nameof(Usuarios), new { q });
        }

        [HttpPost]
        public async Task<IActionResult> CambiarRol(int id, string rol, string? q)
        {
            var actorId = HttpContext.ObtenerUsuarioId()!.Value;
            var error = await _admin.CambiarRolAsync(id, rol, actorId);

            if (error != null) TempData["Error"] = error;
            else TempData["Success"] = "El rol fue actualizado.";

            return RedirectToAction(nameof(Usuarios), new { q });
        }
    }
}
