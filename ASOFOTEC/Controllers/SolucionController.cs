using ASOFOTEC.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using ASOFOTEC.Dtos;

namespace ASOFOTEC.Controllers
{
    public class SolucionController : Controller
    {
        private readonly IsolucionService _solucionService;
        public SolucionController(IsolucionService solucionService)
        {
            _solucionService = solucionService;
        }
        public async Task <IActionResult> Index()
        {
            var solucion = await _solucionService.ObtenerTodosAsync();
            return View(solucion);
        }

        //Formulario para crear una solucion
        public async Task<IActionResult> Create(int problemaId)
        {
            return View(new SolucionCreateDto
            { 
                ProblemaId = problemaId
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SolucionCreateDto dto)
        {
            if(!ModelState.IsValid)
                return View(dto);
            int usuarioId = 1; // Simulación de usuario autenticado (Luego agregaré login para que cresca de forma automática)

            await _solucionService.CrearAsync(dto, usuarioId);
            return RedirectToAction("Details", "Problema", new { id = dto.ProblemaId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _solucionService.EliminarAsync(id);

            return Redirect(Request.Headers["Referer"].ToString());
        }


    }
}
