using ASOFOTEC.Data;
using ASOFOTEC.Dtos;
using ASOFOTEC.Helpers;
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
            // Validaciones del servidor (no dependen de lo que haga el navegador)
            var errorFecha = DatosPersonalesHelper.ValidarFechaNacimiento(register.FechaNacimiento);
            if (errorFecha != null)
            {
                return new AuthResponseDto { Success = false, Message = errorFecha };
            }

            var telefono = DatosPersonalesHelper.NormalizarTelefono(register.Telefono);
            if (telefono == null)
            {
                return new AuthResponseDto { Success = false, Message = "Ingresa un número de teléfono válido." };
            }

            if (!DatosPersonalesHelper.SexoValido(register.Sexo))
            {
                return new AuthResponseDto { Success = false, Message = "Selecciona Masculino o Femenino." };
            }

            if (!register.AceptaPolitica)
            {
                return new AuthResponseDto { Success = false, Message = "Debes aceptar la política de privacidad." };
            }

            var correo = register.Correo.Trim();
            var exist = await _Context.usuarios
                .AnyAsync(u => u.Correo == correo);
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
                Nombre = register.Nombre.Trim(),
                Apellido = register.Apellido.Trim(),
                Correo = correo,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(register.Contraseña),
                Fecha_Ingreso = DateTime.Now,
                Telefono = telefono,
                FechaNacimiento = register.FechaNacimiento!.Value.Date,
                Sexo = register.Sexo,
                Activo = true
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
                .FirstOrDefaultAsync(u=> u.Correo == login.Correo.Trim());
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

            // Las cuentas desactivadas por el administrador no pueden iniciar sesión
            if (!usuario.Activo)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Tu cuenta está desactivada. Comunícate con el administrador de ASOFOTEC."
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
                Rol = rol,
                FotoPerfilUrl = usuario.FotoPerfilUrl
            };
        }
    }
}
