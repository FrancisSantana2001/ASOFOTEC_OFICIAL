using ASOFOTEC.Dtos;
using ASOFOTEC.Models;
using ASOFOTEC.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using ASOFOTEC.Helpers;


namespace ASOFOTEC.Controllers
{
    public class ProblemaController : Controller
    {
        private readonly IProblemaService _ProblemaService;
        private readonly IsolucionService _solucionService;
        private readonly IRutaProblemaService _rutaService;
        private readonly IAdminService _admin;
        public ProblemaController(IProblemaService problemaService, IsolucionService solucionService,
                                  IRutaProblemaService rutaService, IAdminService admin)
        {
            _ProblemaService = problemaService;
            _solucionService = solucionService;
            _rutaService = rutaService;
            _admin = admin;
        }
        //Listar
        public async Task<IActionResult> Index(string? q)
        {
            ViewData["Busqueda"] = q;
            var problemas = await _ProblemaService.ObtenerTodosAsync(q);
            return View(problemas);
        }
        //Detalle
        public async Task<IActionResult> Details(int id, int? paso) 
        {
            var problema = await _ProblemaService.ObtenerPorIdAsync(id);
            if (problema == null)
            {
                return NotFound();
            }
            
            var usuarioId = HttpContext.ObtenerUsuarioId();
            var soluciones = await _solucionService.ObtenerPorProblemaIdAsync(id, usuarioId);
            var ruta = await _rutaService.ObtenerAsync(id, usuarioId);

            var model = new ProblemaDetailsDto
            {
                Problema = problema,
                Soluciones = soluciones,
                Ruta = ruta,
                // Se abre el paso pedido o, si no, el paso en el que va el problema
                PasoInicial = paso is >= 1 and <= RutaProblemaHelper.TotalPasos ? paso.Value : ruta.Etapa,
                NuevaSolucion = new SolucionCreateDto 
                { 
                    ProblemaId = id 
                }
            };
            return View(model);
        }

        
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearSolucion(ProblemaDetailsDto model)
        {
            if (model?.NuevaSolucion == null)
                return RedirectToAction("Index");

            var usuarioIdActual = HttpContext.ObtenerUsuarioId();

            if (usuarioIdActual == null)
            {
                TempData["Error"] = "Debes iniciar sesión para publicar una solución.";
                return RedirectToAction("Login", "Auth");
            }

            int usuarioId = usuarioIdActual.Value;

            var creada = await _solucionService.CrearAsync(model.NuevaSolucion, usuarioId);

            if (!creada)
            {
                TempData["Error"] = "No fue posible guardar la solución.";
            }
            else
            {
                TempData["Success"] = "La solución fue publicada correctamente.";
            }

            return RedirectToAction("Details", new { id = model.NuevaSolucion.ProblemaId, paso = 3 });
        }


        //Formulario (hay que iniciar sesión para publicar, así el problema queda a nombre de su autor)
        [Authorize]
        public IActionResult Create()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProblemaCreateDto problemaCreateDto)
        {
            if (!ModelState.IsValid)
                return View(problemaCreateDto);

            var usuarioId = HttpContext.ObtenerUsuarioId();
            if (usuarioId == null)
                return RedirectToAction("Login", "Auth");

            await _ProblemaService.CreateAsync(problemaCreateDto, usuarioId.Value);
            TempData["Success"] = "Tu problema fue publicado. ¡Ahora la comunidad puede proponer soluciones!";
            return RedirectToAction(nameof(Index));
        }

        // Solo el usuario que publicó el problema puede eliminarlo
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var usuarioId = HttpContext.ObtenerUsuarioId();
            if (usuarioId == null)
                return RedirectToAction("Login", "Auth");

            var esModerador = await _admin.EsDbaAsync(usuarioId.Value);
            var eliminado = await _ProblemaService.ElimiarAsync(id, usuarioId.Value, esModerador);
            if (eliminado && esModerador) _admin.RegistrarModeracion(usuarioId.Value, "problema", id);

            if (!eliminado)
                TempData["Error"] = "No tienes permiso para eliminar este problema.";
            else
                TempData["Success"] = "El problema fue eliminado.";

            // Vuelve a la página desde la que se eliminó (feed o lista de problemas)
            var referer = Request.Headers.Referer.ToString();
            return string.IsNullOrEmpty(referer)
                ? RedirectToAction(nameof(Index))
                : Redirect(referer);
        }

        
    }
    
}
