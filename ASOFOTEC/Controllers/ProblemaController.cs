using ASOFOTEC.Dtos;
using ASOFOTEC.Models;
using ASOFOTEC.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;


namespace ASOFOTEC.Controllers
{
    public class ProblemaController : Controller
    {
        private readonly IProblemaService _ProblemaService;
        private readonly IsolucionService _solucionService;
        public ProblemaController(IProblemaService problemaService, IsolucionService solucionService)
        {
            _ProblemaService = problemaService;
            _solucionService = solucionService;
        }
        //Listar
        public async Task<IActionResult> Index()
        {
            var problemas = await _ProblemaService.ObtenerTodosAsync();
            return View(problemas);
        }
        //Detalle
        public async Task<IActionResult> Details(int id) 
        {
            var problema = await _ProblemaService.ObtenerPorIdAsync(id);
            if (problema == null)
            {
                return NotFound();
            }
            
            var soluciones = await _solucionService.ObtenerPorProblemaIdAsync(id);
            var model = new ProblemaDetailsDto
            {
                Problema = problema,
                Soluciones = soluciones,
                NuevaSolucion = new SolucionCreateDto 
                { 
                    ProblemaId = id 
                }
            };
            return View(model);
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearSolucion(ProblemaDetailsDto model)
        {
            if (model?.NuevaSolucion == null)
                return RedirectToAction("Index");

            // Obtener usuario autenticado desde Session
            var userIdString = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(userIdString))
            {
                TempData["Error"] = "Debes iniciar sesión para publicar una solución.";
                return RedirectToAction("Login", "Auth");
            }

            int usuarioId = int.Parse(userIdString);

            var creada = await _solucionService.CrearAsync(model.NuevaSolucion, usuarioId);

            if (!creada)
            {
                TempData["Error"] = "No fue posible guardar la solución.";
            }
            else
            {
                TempData["Success"] = "La solución fue publicada correctamente.";
            }

            return RedirectToAction("Details", new { id = model.NuevaSolucion.ProblemaId });
        }


        //Formulario
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProblemaCreateDto problemaCreateDto)
        {
            if (!ModelState.IsValid)
                return View(problemaCreateDto);

            await _ProblemaService.CreateAsync(problemaCreateDto);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            await _ProblemaService.ElimiarAsync(id);
            return RedirectToAction(nameof(Index));
        }

        
    }
    
}
