using ASOFOTEC.Dtos;
using ASOFOTEC.Helpers;
using ASOFOTEC.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ASOFOTEC.Controllers
{
    public class SolucionController : Controller
    {
        private readonly IsolucionService _solucionService;
        private readonly IAdminService _admin;
        public SolucionController(IsolucionService solucionService, IAdminService admin)
        {
            _solucionService = solucionService;
            _admin = admin;
        }
        public async Task <IActionResult> Index()
        {
            var solucion = await _solucionService.ObtenerTodosAsync();
            return View(solucion);
        }

        //Formulario para crear una solucion
        [Authorize]
        public IActionResult Create(int problemaId)
        {
            return View(new SolucionCreateDto
            { 
                ProblemaId = problemaId
            });
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SolucionCreateDto dto)
        {
            if(!ModelState.IsValid)
                return View(dto);

            var usuarioId = HttpContext.ObtenerUsuarioId();
            if (usuarioId == null)
                return RedirectToAction("Login", "Auth");

            await _solucionService.CrearAsync(dto, usuarioId.Value);
            return RedirectToAction("Details", "Problema", new { id = dto.ProblemaId });
        }

        // Solo el usuario que publicó la solución puede eliminarla
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var usuarioId = HttpContext.ObtenerUsuarioId();
            if (usuarioId == null)
                return RedirectToAction("Login", "Auth");

            var esModerador = await _admin.EsDbaAsync(usuarioId.Value);
            var eliminada = await _solucionService.EliminarAsync(id, usuarioId.Value, esModerador);
            if (eliminada && esModerador) _admin.RegistrarModeracion(usuarioId.Value, "solución", id);

            if (!eliminada)
                TempData["Error"] = "No tienes permiso para eliminar esta solución.";
            else
                TempData["Success"] = "La solución fue eliminada.";

            var referer = Request.Headers.Referer.ToString();
            return string.IsNullOrEmpty(referer)
                ? RedirectToAction(nameof(Index))
                : Redirect(referer);
        }


    }
}
