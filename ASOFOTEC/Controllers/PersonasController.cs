using ASOFOTEC.Dtos;
using ASOFOTEC.Helpers;
using ASOFOTEC.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ASOFOTEC.Controllers
{
    // Descubrir personas: buscar por nombre y ver sugerencias
    [Authorize]
    public class PersonasController : Controller
    {
        private readonly ISeguimientoService _seguimiento;

        public PersonasController(ISeguimientoService seguimiento)
        {
            _seguimiento = seguimiento;
        }

        public async Task<IActionResult> Index(string? q)
        {
            var usuarioId = HttpContext.ObtenerUsuarioId();
            if (usuarioId == null) return RedirectToAction("Login", "Auth");

            var modelo = new PersonasVm
            {
                Busqueda = q,
                Resultados = await _seguimiento.BuscarAsync(usuarioId.Value, q, 30),
                Sugerencias = await _seguimiento.ObtenerSugerenciasAsync(usuarioId.Value, 12)
            };
            return View(modelo);
        }
    }
}
