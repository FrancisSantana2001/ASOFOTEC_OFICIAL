using ASOFOTEC.Dtos;
using ASOFOTEC.Helpers;
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
        private readonly IFeedService _feedService;
        private readonly ISeguimientoService _seguimientoService;

        public HomeController(
        IProblemaService problemaService,
            IsolucionService solucionService,
            IUsuarioService usuarioService,
            IFeedService feedService,
            ISeguimientoService seguimientoService)
        {
            _seguimientoService = seguimientoService;
            _problemaService = problemaService;
            _solucionService = solucionService;
            _usuarioService = usuarioService;
            _feedService = feedService;
        }

        public async Task<IActionResult> Index(string? q, string orden = "reciente", int cantidad = 10)
        {
            // Usuario con sesión iniciada: solo ve el feed (sin hero ni botón de registro)
            var usuarioId = HttpContext.ObtenerUsuarioId();
            if (usuarioId != null)
            {
                cantidad = Math.Clamp(cantidad, 10, 100);
                var feed = await _feedService.ObtenerFeedAsync(usuarioId.Value, q, orden, cantidad);
                feed.Sugerencias = await _seguimientoService.ObtenerSugerenciasAsync(usuarioId.Value, 4);
                return View("Feed", feed);
            }

            // Visitante: portada de bienvenida
            var model = new HomeDashboardDto
            {
                TotalUsuarios = await _usuarioService.ObtenerTotalUsuariosAsync(),
                TotalProblemas = await _problemaService.ObtenerTotalProblemasAsync(),
                TotalSoluciones = await _solucionService.ObtenerTotalSolucionAsync(),

                TotalProyectos = await _problemaService.ObtenerTotalProyectosAsync(),
                TotalResueltos = await _problemaService.ObtenerTotalResueltosAsync(),

                ProblemasDestacados = await _problemaService.ObtenerDestacadosAsync(3),
                SolucionesRecientes = await _solucionService.ObtenerRecientesAsync(3)
            };

            return View(model);
        }

        // Página "Comunidad": cifras y miembros que más aportan
        public async Task<IActionResult> Comunidad()
        {
            var model = new HomeDashboardDto
            {
                TotalUsuarios = await _usuarioService.ObtenerTotalUsuariosAsync(),
                TotalProblemas = await _problemaService.ObtenerTotalProblemasAsync(),
                TotalSoluciones = await _solucionService.ObtenerTotalSolucionAsync(),
                TotalProyectos = await _problemaService.ObtenerTotalProyectosAsync(),
                TotalResueltos = await _problemaService.ObtenerTotalResueltosAsync()
            };

            return View(model);
        }

        // Página "Sobre Nosotros" (contenido estático)
        public IActionResult Nosotros()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}