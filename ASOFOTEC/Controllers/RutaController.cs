using ASOFOTEC.Helpers;
using ASOFOTEC.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ASOFOTEC.Controllers
{
    // Acciones de la "ruta" de un problema (pasos 2 al 7). Todas requieren sesión iniciada;
    // las reglas de quién puede hacer qué las valida RutaProblemaService.
    [Authorize]
    [AutoValidateAntiforgeryToken]
    public class RutaController : Controller
    {
        private readonly IRutaProblemaService _ruta;

        public RutaController(IRutaProblemaService ruta)
        {
            _ruta = ruta;
        }

        // Paso 2: causas y consecuencias
        [HttpPost]
        public async Task<IActionResult> GuardarAnalisis(int problemaId, string? causas, string? consecuencias)
        {
            var usuarioId = HttpContext.ObtenerUsuarioId();
            if (usuarioId == null) return RedirectToAction("Login", "Auth");

            var error = await _ruta.GuardarAnalisisAsync(problemaId, causas, consecuencias, usuarioId.Value);
            return Volver(problemaId, 2, error, "Causas y consecuencias guardadas.");
        }

        // Paso 4: el creador del problema elige la solución
        [HttpPost]
        public async Task<IActionResult> SeleccionarSolucion(int problemaId, int solucionId)
        {
            var usuarioId = HttpContext.ObtenerUsuarioId();
            if (usuarioId == null) return RedirectToAction("Login", "Auth");

            var error = await _ruta.SeleccionarSolucionAsync(problemaId, solucionId, usuarioId.Value);
            return Volver(problemaId, error == null ? 5 : 4, error, "Solución elegida. Ahora se puede formar el equipo.");
        }

        // Paso 5: equipo
        [HttpPost]
        public async Task<IActionResult> Unirse(int problemaId, int solucionId)
        {
            var usuarioId = HttpContext.ObtenerUsuarioId();
            if (usuarioId == null) return RedirectToAction("Login", "Auth");

            var error = await _ruta.UnirseAsync(solucionId, usuarioId.Value);
            return Volver(problemaId, 5, error, "Te uniste al equipo.");
        }

        [HttpPost]
        public async Task<IActionResult> Salir(int problemaId, int solucionId)
        {
            var usuarioId = HttpContext.ObtenerUsuarioId();
            if (usuarioId == null) return RedirectToAction("Login", "Auth");

            var error = await _ruta.SalirAsync(solucionId, usuarioId.Value);
            return Volver(problemaId, 5, error, "Saliste del equipo.");
        }

        // Paso 6: avances
        [HttpPost]
        public async Task<IActionResult> AgregarAvance(int problemaId, int solucionId, string? contenido, int? porcentaje)
        {
            var usuarioId = HttpContext.ObtenerUsuarioId();
            if (usuarioId == null) return RedirectToAction("Login", "Auth");

            var error = await _ruta.AgregarAvanceAsync(solucionId, contenido, porcentaje, usuarioId.Value);
            return Volver(problemaId, 6, error, "Avance registrado.");
        }

        [HttpPost]
        public async Task<IActionResult> EliminarAvance(int problemaId, int avanceId)
        {
            var usuarioId = HttpContext.ObtenerUsuarioId();
            if (usuarioId == null) return RedirectToAction("Login", "Auth");

            var error = await _ruta.EliminarAvanceAsync(avanceId, usuarioId.Value);
            return Volver(problemaId, 6, error, "Avance eliminado.");
        }

        // Paso 7: resultados (cierran el problema como resuelto)
        [HttpPost]
        public async Task<IActionResult> DocumentarResultados(int problemaId, int solucionId, string? resultados)
        {
            var usuarioId = HttpContext.ObtenerUsuarioId();
            if (usuarioId == null) return RedirectToAction("Login", "Auth");

            var error = await _ruta.DocumentarResultadosAsync(solucionId, resultados, usuarioId.Value);
            return Volver(problemaId, 7, error, "¡Resultados documentados! El problema quedó marcado como resuelto.");
        }

        // Vuelve a la pantalla del problema, abierta en el paso correspondiente, con un aviso
        private IActionResult Volver(int problemaId, int paso, string? error, string exito)
        {
            if (error != null)
                TempData["Error"] = error;
            else
                TempData["Success"] = exito;

            return RedirectToAction("Details", "Problema", new { id = problemaId, paso });
        }
    }
}
