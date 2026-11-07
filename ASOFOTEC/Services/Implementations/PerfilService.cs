using ASOFOTEC.Data;
using ASOFOTEC.Dtos;
using ASOFOTEC.Helpers;
using ASOFOTEC.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ASOFOTEC.Services.Implementations
{
    public class PerfilService : IPerfilService
    {
        private readonly AppDbContext _context;

        public PerfilService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<PerfilVm?> ObtenerAsync(int usuarioId)
        {
            var fila = await _context.usuarios
                .AsNoTracking()
                .Where(u => u.Id == usuarioId)
                .Select(u => new
                {
                    u.Id,
                    u.Nombre,
                    u.Apellido,
                    u.Correo,
                    u.Telefono,
                    u.FechaNacimiento,
                    u.Sexo,
                    u.FotoPerfilUrl,
                    u.FotoPortadaUrl,
                    u.Fecha_Ingreso,
                    Rol = _context.RolesUser
                        .Where(r => r.Usuario_Id == u.Id)
                        .Select(r => r.Rol!.Nombre)
                        .FirstOrDefault(),
                    Publicaciones = u.Publicaciones.Count(),
                    Problemas = u.Problemas.Count(),
                    Soluciones = u.Soluciones.Count(),
                    Comentarios = u.Comentarios.Count()
                })
                .FirstOrDefaultAsync();

            if (fila == null) return null;

            var seguidores = await _context.Seguimientos.CountAsync(s => s.SeguidoId == usuarioId);
            var siguiendo = await _context.Seguimientos.CountAsync(s => s.SeguidorId == usuarioId);

            return new PerfilVm
            {
                Id = fila.Id,
                Nombre = fila.Nombre,
                Apellido = fila.Apellido ?? "",
                Correo = fila.Correo ?? "",
                Telefono = fila.Telefono,
                FechaNacimiento = fila.FechaNacimiento,
                Sexo = fila.Sexo,
                FotoPerfilUrl = fila.FotoPerfilUrl,
                FotoPortadaUrl = fila.FotoPortadaUrl,
                Rol = fila.Rol ?? RolesSistema.Usuario,
                FechaIngreso = fila.Fecha_Ingreso,
                TotalPublicaciones = fila.Publicaciones,
                TotalProblemas = fila.Problemas,
                TotalSoluciones = fila.Soluciones,
                TotalComentarios = fila.Comentarios,
                Seguidores = seguidores,
                Siguiendo = siguiendo,
                Edicion = new PerfilEditDto
                {
                    Nombre = fila.Nombre,
                    Apellido = fila.Apellido ?? "",
                    Telefono = fila.Telefono,
                    FechaNacimiento = fila.FechaNacimiento,
                    Sexo = fila.Sexo
                }
            };
        }

        public async Task<string?> ActualizarDatosAsync(int usuarioId, PerfilEditDto dto)
        {
            var errorFecha = DatosPersonalesHelper.ValidarFechaNacimiento(dto.FechaNacimiento);
            if (errorFecha != null) return errorFecha;

            var telefono = DatosPersonalesHelper.NormalizarTelefono(dto.Telefono);
            if (telefono == null) return "Ingresa un número de teléfono válido.";

            if (!DatosPersonalesHelper.SexoValido(dto.Sexo)) return "Selecciona Masculino o Femenino.";

            if (string.IsNullOrWhiteSpace(dto.Nombre) || string.IsNullOrWhiteSpace(dto.Apellido))
                return "El nombre y el apellido son obligatorios.";

            var usuario = await _context.usuarios.FindAsync(usuarioId);
            if (usuario == null) return "La cuenta no existe.";

            usuario.Nombre = dto.Nombre.Trim();
            usuario.Apellido = dto.Apellido.Trim();
            usuario.Telefono = telefono;
            usuario.FechaNacimiento = dto.FechaNacimiento!.Value.Date;
            usuario.Sexo = dto.Sexo;

            await _context.SaveChangesAsync();
            return null;
        }

        public async Task<string?> ObtenerFotoAsync(int usuarioId, bool esPortada)
        {
            return await _context.usuarios
                .AsNoTracking()
                .Where(u => u.Id == usuarioId)
                .Select(u => esPortada ? u.FotoPortadaUrl : u.FotoPerfilUrl)
                .FirstOrDefaultAsync();
        }

        public async Task EstablecerFotoAsync(int usuarioId, bool esPortada, string? url)
        {
            var usuario = await _context.usuarios.FindAsync(usuarioId);
            if (usuario == null) return;

            if (esPortada)
                usuario.FotoPortadaUrl = url;
            else
                usuario.FotoPerfilUrl = url;

            await _context.SaveChangesAsync();
        }
    }
}
