using ASOFOTEC.Dtos;
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
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(LoginDto login)
        {
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

            Response.Cookies.Append("Jwt", result.Token!, new CookieOptions
            {
                HttpOnly = true,
                Secure = true, // Cambiar a true en producción con HTTPS
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddHours(2)
            });
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