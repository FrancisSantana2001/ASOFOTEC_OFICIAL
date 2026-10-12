using ASOFOTEC.Data;
using ASOFOTEC.Dtos;
using ASOFOTEC.Models;
using ASOFOTEC.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ASOFOTEC.Services.Implementations
{
    public class AuthService:IAuthService
    {
        private readonly AppDbContext _Context;
        private readonly IJwtService _jwtservice;
        public AuthService(AppDbContext context, IJwtService jwtservice)
        {
            _Context = context;
            _jwtservice = jwtservice;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto register)
        {
            var exist = await _Context.usuarios
                .AnyAsync(u => u.Correo == register.Correo);
            if (exist)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "El correo ya esta registrado."
                };
            }

            var usuario = new Usuario
            {
                Nombre = register.Nombre,
                Apellido = register.Apellido,
                Correo = register.Correo,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(register.Contraseña),
                Fecha_Ingreso = DateTime.Now
            };

            await _Context.usuarios.AddAsync(usuario);
            await _Context.SaveChangesAsync();

            //Buscar Rol del usuario
            var rol = await _Context.Roles
                .FirstOrDefaultAsync(r => r.Nombre == "Usuario");
            if (rol != null)
            {
                await _Context.RolesUser.AddAsync(new UsuarioRol
                {
                    Usuario_Id = usuario.Id,
                    Rol_Id = rol.Id
                });
                await _Context.SaveChangesAsync();
            }

            var token = _jwtservice.GenerateToken(usuario, "Usuario");

            return new AuthResponseDto
            {
                Success = true,
                Message = "Usuario registrado exitosamente.",
                Token = token,
                Nombre = $"{usuario.Nombre} {usuario.Apellido}",
                Rol = "Usuario"
            };

        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto login) 
        { 
            var usuario = await _Context.usuarios
                .FirstOrDefaultAsync(u=> u.Correo == login.Correo);
            if(usuario == null)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Correo o contraseña incorrectos."
                };
            }

            var valid = BCrypt.Net.BCrypt.Verify(login.Password, usuario.PasswordHash);
            if (!valid) 
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Correo o contraseña incorrectos."
                };
            }

            var rol = await _Context.RolesUser
                .Where(u => u.Usuario_Id == usuario.Id)
                .Select(ur=> ur.Rol.Nombre)
                .FirstOrDefaultAsync() ?? "Usuario";

            var token = _jwtservice.GenerateToken(usuario, rol);

            return new AuthResponseDto
            {
                Success = true,
                Message = "Inicio de sesión exitoso.",
                Token = token,
                Nombre = $"{usuario.Nombre} {usuario.Apellido}",
                Rol = rol
            };
        }
    }
}
