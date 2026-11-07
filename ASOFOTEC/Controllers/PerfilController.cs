using ASOFOTEC.Dtos;
using ASOFOTEC.Helpers;
using ASOFOTEC.Services.Implementations;
using ASOFOTEC.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ASOFOTEC.Controllers
{
    // "Mi perfil": disponible para cualquier cuenta con sesión (Usuario, Admin o DBA)
    [Authorize]
    [AutoValidateAntiforgeryToken]
    public class PerfilController : Controller
    {
        private const long LimiteSolicitud = MediaService.MaxBytesFotoPortada + 1024 * 1024;

        private readonly IPerfilService _perfil;
        private readonly IMediaService _media;
        private readonly ISeguimientoService _seguimiento;
        private readonly IFeedService _feed;

        public PerfilController(IPerfilService perfil, IMediaService media,
                                ISeguimientoService seguimiento, IFeedService feed)
        {
            _perfil = perfil;
            _media = media;
            _seguimiento = seguimiento;
            _feed = feed;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var usuarioId = HttpContext.ObtenerUsuarioId();
            if (usuarioId == null) return RedirectToAction("Login", "Auth");

            var perfil = await _perfil.ObtenerAsync(usuarioId.Value);
            if (perfil == null) return RedirectToAction("Logout", "Auth");

            return View(perfil);
        }

        // Perfil de otra persona (sin correo, teléfono, fecha de nacimiento ni sexo)
        [HttpGet]
        public async Task<IActionResult> Ver(int id)
        {
            var usuarioId = HttpContext.ObtenerUsuarioId();
            if (usuarioId == null) return RedirectToAction("Login", "Auth");

            // Tu propio perfil se ve (y se edita) en "Mi perfil"
            if (id == usuarioId.Value) return RedirectToAction(nameof(Index));

            var perfil = await _seguimiento.ObtenerPerfilAsync(id, usuarioId.Value);
            if (perfil == null)
            {
                TempData["Error"] = "Ese perfil no existe.";
                return RedirectToAction("Index", "Home");
            }

            var actividad = await _feed.ObtenerFeedAsync(usuarioId.Value, null, "reciente", 10, id);
            perfil.Actividad = actividad.Items;

            return View(perfil);
        }

        // Lista de seguidores o de seguidos. tipo: "seguidores" | "siguiendo"
        [HttpGet]
        public async Task<IActionResult> Conexiones(int id, string tipo = "seguidores")
        {
            var usuarioId = HttpContext.ObtenerUsuarioId();
            if (usuarioId == null) return RedirectToAction("Login", "Auth");

            var perfil = await _seguimiento.ObtenerPerfilAsync(id, usuarioId.Value);
            if (perfil == null) return RedirectToAction("Index", "Home");

            var esSeguidores = tipo != "siguiendo";
            var modelo = new ConexionesVm
            {
                PerfilId = id,
                PerfilNombre = perfil.Nombre,
                EsSeguidores = esSeguidores,
                Personas = await _seguimiento.ObtenerConexionesAsync(id, esSeguidores, usuarioId.Value, 200)
            };
            return View(modelo);
        }

        // Cambia nombre, apellido, teléfono, fecha de nacimiento y sexo (el correo no se cambia)
        [HttpPost]
        public async Task<IActionResult> Editar([Bind(Prefix = "Edicion")] PerfilEditDto dto)
        {
            var usuarioId = HttpContext.ObtenerUsuarioId();
            if (usuarioId == null) return RedirectToAction("Login", "Auth");

            string? error = null;
            if (ModelState.IsValid)
            {
                error = await _perfil.ActualizarDatosAsync(usuarioId.Value, dto);
                if (error == null)
                {
                    // El nombre que se muestra en el menú sale de la sesión
                    HttpContext.Session.SetString("Nombre", $"{dto.Nombre.Trim()} {dto.Apellido.Trim()}");
                    TempData["Success"] = "Tu perfil fue actualizado.";
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError("", error);
            }

            // Hay errores: se vuelve a mostrar la página con lo que escribió la persona
            var perfil = await _perfil.ObtenerAsync(usuarioId.Value);
            if (perfil == null) return RedirectToAction("Logout", "Auth");
            perfil.Edicion = dto;
            return View(nameof(Index), perfil);
        }

        // Agrega o reemplaza la foto de perfil o de portada. tipo: "perfil" | "portada"
        [HttpPost]
        [RequestSizeLimit(LimiteSolicitud)]
        [RequestFormLimits(MultipartBodyLengthLimit = LimiteSolicitud)]
        public async Task<IActionResult> CambiarFoto(string tipo, IFormFile? archivo)
        {
            var usuarioId = HttpContext.ObtenerUsuarioId();
            if (usuarioId == null) return RedirectToAction("Login", "Auth");

            var esPortada = tipo == "portada";
            if (!esPortada && tipo != "perfil") return RedirectToAction(nameof(Index));

            if (archivo == null || archivo.Length == 0)
            {
                TempData["Error"] = "Selecciona una imagen.";
                return RedirectToAction(nameof(Index));
            }

            var (guardada, error) = await _media.GuardarImagenPerfilAsync(archivo, esPortada);
            if (guardada == null)
            {
                TempData["Error"] = error ?? "No se pudo guardar la imagen.";
                return RedirectToAction(nameof(Index));
            }

            var anterior = await _perfil.ObtenerFotoAsync(usuarioId.Value, esPortada);
            await _perfil.EstablecerFotoAsync(usuarioId.Value, esPortada, guardada.Url);
            _media.Eliminar(anterior);   // no se acumulan fotos viejas en el disco

            if (!esPortada) HttpContext.Session.SetString("Foto", guardada.Url);

            TempData["Success"] = esPortada ? "Tu foto de portada fue actualizada." : "Tu foto de perfil fue actualizada.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> EliminarFoto(string tipo)
        {
            var usuarioId = HttpContext.ObtenerUsuarioId();
            if (usuarioId == null) return RedirectToAction("Login", "Auth");

            var esPortada = tipo == "portada";
            if (!esPortada && tipo != "perfil") return RedirectToAction(nameof(Index));

            var anterior = await _perfil.ObtenerFotoAsync(usuarioId.Value, esPortada);
            await _perfil.EstablecerFotoAsync(usuarioId.Value, esPortada, null);
            _media.Eliminar(anterior);

            if (!esPortada) HttpContext.Session.SetString("Foto", "");

            TempData["Success"] = esPortada ? "Se eliminó tu foto de portada." : "Se eliminó tu foto de perfil.";
            return RedirectToAction(nameof(Index));
        }
    }
}
