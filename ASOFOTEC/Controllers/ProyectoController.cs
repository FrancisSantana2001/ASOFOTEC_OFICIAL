using ASOFOTEC.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ASOFOTEC.Controllers
{
    // "Proyectos": problemas que ya tienen una solución elegida (en ejecución o resueltos)
    public class ProyectoController : Controller
    {
        private readonly IProblemaService _problemaService;

        public ProyectoController(IProblemaService problemaService)
        {
            _problemaService = problemaService;
        }

        public async Task<IActionResult> Index()
        {
            var proyectos = await _problemaService.ObtenerProyectosAsync();
            return View(proyectos);
        }
    }
}
