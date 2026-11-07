using ASOFOTEC.Dtos;
using ASOFOTEC.Helpers;
using ASOFOTEC.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ASOFOTEC.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterDto register)
        {
            if (!ModelState.IsValid)
            {
                return View(register);
            }

            var result = await _authService.RegisterAsync(register);
            if (!result.Success)
            {
                ModelState.AddModelError("", result.Message);
                return View(register);
            }

            TempData["Success"] = result.Message;
            return RedirectToAction("Login");

        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            // Si ya tiene una sesión válida, no tiene sentido mostrar el login
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirigirTrasLogin(returnUrl);
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(LoginDto login, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
            {
                return View(login);
            }

            var result = await _authService.LoginAsync(login);
            if (!result.Success)
            {
                ModelState.AddModelError("", result.Message);
                return View(login);
            }

            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(result.Token!);

            var nombre = jwt.Claims
                .FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

            var rol = jwt.Claims
                .FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

            var userId = jwt.Claims
                .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            HttpContext.Session.SetString("Token", result.Token!);
            HttpContext.Session.SetString("Nombre", nombre ?? "");
            HttpContext.Session.SetString("Rol", rol ?? "Usuario");
            HttpContext.Session.SetString("UserId", userId ?? "");
            HttpContext.Session.SetString("Foto", result.FotoPerfilUrl ?? "");

            Response.Cookies.Append("Jwt", result.Token!, new CookieOptions
            {
                HttpOnly = true,
                Secure = true, // Cambiar a true en producción con HTTPS
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddHours(2)
            });
            TempData["Success"] = $"¡Bienvenido, {nombre}!";
            return RedirigirTrasLogin(returnUrl);

        }

        // Vuelve a la página que el usuario quería abrir (solo rutas del propio sitio) o al inicio
        private IActionResult RedirigirTrasLogin(string? returnUrl)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return LocalRedirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            Response.Cookies.Delete("Jwt");

            return RedirectToAction("Index", "Home");
        }
    }
}