using ASOFOTEC.Dtos;
using ASOFOTEC.Services.Implementations;
using ASOFOTEC.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ASOFOTEC.Controllers
{
    public class HomeController : Controller
    {
        private readonly IProblemaService _problemaService;
        private readonly IsolucionService _solucionService;
        private readonly IUsuarioService _usuarioService;

        public HomeController(
        IProblemaService problemaService,
            IsolucionService solucionService,
            IUsuarioService usuarioService)
        {
            _problemaService = problemaService;
            _solucionService = solucionService;
            _usuarioService = usuarioService;
        }

        public async Task<IActionResult> Index()
        {
            var model = new HomeDashboardDto
            {
                TotalUsuarios = await _usuarioService.ObtenerTotalUsuariosAsync(),
                TotalProblemas = await _problemaService.ObtenerTotalProblemasAsync(),
                TotalSoluciones = await _solucionService.ObtenerTotalSolucionAsync(),

                // Valores temporales hasta crear módulos
                TotalProyectos = 35,
                TotalComunidades = 15,

                ProblemasDestacados = await _problemaService.ObtenerDestacadosAsync(3),
                SolucionesRecientes = await _solucionService.ObtenerRecientesAsync(3)
            };

            return View(model);
        }
    }
}