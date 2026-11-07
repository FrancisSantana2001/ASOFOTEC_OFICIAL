using ASOFOTEC.Data;
using ASOFOTEC.Dtos;
using ASOFOTEC.Helpers;
using ASOFOTEC.Models;
using ASOFOTEC.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ASOFOTEC.Services.Implementations
{
    public class AdminService : IAdminService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<AdminService> _logger;

        public AdminService(AppDbContext context, ILogger<AdminService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<bool> EsDbaAsync(int usuarioId)
        {
            return await _context.RolesUser.AnyAsync(ur =>
                ur.Usuario_Id == usuarioId &&
                ur.Rol!.Nombre == RolesSistema.Dba &&
                ur.usuario!.Activo);
        }

        public async Task<MetricasDbaDto> ObtenerMetricasAsync()
        {
            var ahora = DateTime.Now;
            var hace7 = ahora.AddDays(-7);
            var hace30 = ahora.AddDays(-30);

            var usuarios = _context.usuarios.AsNoTracking();

            return new MetricasDbaDto
            {
                TotalUsuarios = await usuarios.CountAsync(),
                UsuariosActivos = await usuarios.CountAsync(u => u.Activo),
                UsuariosInactivos = await usuarios.CountAsync(u => !u.Activo),
                NuevosUltimos7Dias = await usuarios.CountAsync(u => u.Fecha_Ingreso >= hace7),
                NuevosUltimos30Dias = await usuarios.CountAsync(u => u.Fecha_Ingreso >= hace30),

                Masculino = await usuarios.CountAsync(u => u.Sexo == "Masculino"),
                Femenino = await usuarios.CountAsync(u => u.Sexo == "Femenino"),
                SinDato = await usuarios.CountAsync(u => u.Sexo == null || (u.Sexo != "Masculino" && u.Sexo != "Femenino")),

                TotalProblemas = await _context.problemas.CountAsync(),
                TotalSoluciones = await _context.soluciones.CountAsync(),
                TotalPublicaciones = await _context.publicaciones.CountAsync(),
                TotalComentarios = await _context.Comentarios.CountAsync(),
                TotalProyectosEnMarcha = await _context.problemas
                    .CountAsync(p => p.Soluciones.Any(s => s.Seleccionada && s.FechaResultados == null)),
                TotalResueltos = await _context.problemas
                    .CountAsync(p => p.Soluciones.Any(s => s.Seleccionada && s.FechaResultados != null))
            };
        }

        public async Task<List<UsuarioAdminDto>> ObtenerUsuariosAsync(string? busqueda)
        {
            var consulta = _context.usuarios.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                var texto = busqueda.Trim();
                consulta = consulta.Where(u =>
                    u.Nombre.Contains(texto) ||
                    (u.Apellido ?? "").Contains(texto) ||
                    (u.Correo ?? "").Contains(texto));
            }

            var filas = await consulta
                .OrderByDescending(u => u.Fecha_Ingreso)
                .Take(200)
                .Select(u => new
                {
                    u.Id,
                    u.Nombre,
                    u.Apellido,
                    u.Correo,
                    u.Activo,
                    u.Fecha_Ingreso,
                    u.FechaNacimiento,
                    u.FotoPerfilUrl,
                    Rol = _context.RolesUser
                        .Where(r => r.Usuario_Id == u.Id)
                        .Select(r => r.Rol!.Nombre)
                        .FirstOrDefault(),
                    Problemas = u.Problemas.Count(),
                    Soluciones = u.Soluciones.Count(),
                    Publicaciones = u.Publicaciones.Count(),
                    Comentarios = u.Comentarios.Count()
                })
                .ToListAsync();

            // La fecha de nacimiento no sale de aquí: solo se entrega la edad
            return filas.Select(f => new UsuarioAdminDto
            {
                Id = f.Id,
                Nombre = $"{f.Nombre} {f.Apellido}".Trim(),
                Correo = f.Correo ?? "",
                Rol = f.Rol ?? RolesSistema.Usuario,
                Activo = f.Activo,
                FechaIngreso = f.Fecha_Ingreso,
                Edad = f.FechaNacimiento.HasValue ? DatosPersonalesHelper.CalcularEdad(f.FechaNacimiento.Value) : null,
                FotoPerfilUrl = f.FotoPerfilUrl,
                Problemas = f.Problemas,
                Soluciones = f.Soluciones,
                Publicaciones = f.Publicaciones,
                Comentarios = f.Comentarios
            }).ToList();
        }

        public async Task<string?> CambiarEstadoAsync(int usuarioId, bool activo, int actorId)
        {
            if (usuarioId == actorId) return "No puedes cambiar el estado de tu propia cuenta.";

            var usuario = await _context.usuarios.FindAsync(usuarioId);
            if (usuario == null) return "El usuario no existe.";

            if (await EsDbaAsync(usuarioId)) return "La cuenta del DBA no se puede desactivar.";

            usuario.Activo = activo;
            await _context.SaveChangesAsync();

            _logger.LogWarning("DBA {Actor} {Accion} la cuenta {Usuario}.", actorId, activo ? "activó" : "desactivó", usuarioId);
            return null;
        }

        public async Task<string?> CambiarRolAsync(int usuarioId, string rol, int actorId)
        {
            if (usuarioId == actorId) return "No puedes cambiar tu propio rol.";
            if (!RolesSistema.Asignables.Contains(rol)) return "Rol no permitido.";

            var usuario = await _context.usuarios.FindAsync(usuarioId);
            if (usuario == null) return "El usuario no existe.";

            if (await EsDbaAsync(usuarioId)) return "El rol del DBA no se puede cambiar.";

            var rolDestino = await _context.Roles.FirstOrDefaultAsync(r => r.Nombre == rol);
            if (rolDestino == null) return "El rol no existe en la base de datos.";

            // Cada usuario tiene un único rol: se reemplazan los anteriores
            var anteriores = await _context.RolesUser.Where(ur => ur.Usuario_Id == usuarioId).ToListAsync();
            _context.RolesUser.RemoveRange(anteriores);
            _context.RolesUser.Add(new UsuarioRol { Usuario_Id = usuarioId, Rol_Id = rolDestino.Id });
            await _context.SaveChangesAsync();

            _logger.LogWarning("DBA {Actor} cambió el rol de la cuenta {Usuario} a {Rol}.", actorId, usuarioId, rol);
            return null;
        }

        public void RegistrarModeracion(int actorId, string tipo, int elementoId)
        {
            _logger.LogWarning("Moderación: el DBA {Actor} eliminó {Tipo} #{Elemento}.", actorId, tipo, elementoId);
        }
    }
}
