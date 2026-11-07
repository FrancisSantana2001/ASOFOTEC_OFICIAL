using ASOFOTEC.Data;
using ASOFOTEC.Dtos;
using ASOFOTEC.Models;
using ASOFOTEC.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ASOFOTEC.Services.Implementations
{
    public class ComentarioService: IComentarioService
    {
        private readonly AppDbContext _context;
        private readonly INotificacionService _notificaciones;
        public ComentarioService(AppDbContext dbContext, INotificacionService notificaciones)
        {
            _context = dbContext;
            _notificaciones = notificaciones;
        }

        public async Task<ComentarioDto?> CrearAsync(ComentarioCreateDto dto, int usuarioId)
        {
            int? solucionId = dto.SolucionId;
            int? publicacionId = dto.PublicacionId;
            int? padreId = null;
            int? autorRespondidoId = null;   // autor del comentario al que se responde

            if (dto.ComentarioPadreId.HasValue)
            {
                // Es una respuesta: el destino (solución o publicación) se toma del comentario padre
                var padre = await _context.Comentarios
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Id == dto.ComentarioPadreId.Value);

                if (padre == null)
                {
                    return null;
                }

                // Las respuestas siempre cuelgan del comentario principal (un solo nivel),
                // incluso cuando se responde a otra respuesta
                padreId = padre.ComentarioPadreId ?? padre.Id;
                solucionId = padre.SolucionId;
                publicacionId = padre.PublicacionId;
                autorRespondidoId = padre.UsuarioId;
            }
            else
            {
                // Comentario principal: debe tener exactamente un destino y este debe existir
                if (solucionId.HasValue == publicacionId.HasValue)
                {
                    return null;
                }

                if (solucionId.HasValue && !await _context.soluciones.AnyAsync(s => s.Id == solucionId.Value))
                {
                    return null;
                }

                if (publicacionId.HasValue && !await _context.publicaciones.AnyAsync(p => p.Id == publicacionId.Value))
                {
                    return null;
                }
            }

            var comentario = new Comentario
            {
                Contenido = dto.Contenido.Trim(),
                SolucionId = solucionId,
                PublicacionId = publicacionId,
                ComentarioPadreId = padreId,
                UsuarioId = usuarioId,
                FechaComentario = DateTime.Now
            };
            await _context.Comentarios.AddAsync(comentario);
            await _context.SaveChangesAsync();

            var usuario = await _context.usuarios.FindAsync(usuarioId);

            await NotificarAsync(comentario, autorRespondidoId, usuarioId);

            return new ComentarioDto
            {
                id = comentario.Id,
                contenido = comentario.Contenido,
                UsuarioId = usuarioId,
                UserName = usuario != null ? $"{usuario.Nombre} {usuario.Apellido}" : string.Empty,
                UserFoto = usuario?.FotoPerfilUrl,
                FechaComentario = comentario.FechaComentario,
                SolucionId = solucionId,
                PublicacionId = publicacionId,
                ComentarioPadreId = padreId
            };
        }

        // Avisa a quien corresponda: al autor del comentario respondido o al dueño de la solución / publicación
        private async Task NotificarAsync(Comentario comentario, int? autorRespondidoId, int actorId)
        {
            var url = await _notificaciones.UrlDestinoAsync(comentario.SolucionId, comentario.PublicacionId);
            if (url == null) return;

            if (autorRespondidoId.HasValue)
            {
                await _notificaciones.CrearAsync(autorRespondidoId.Value, actorId, TiposNotificacion.Respuesta,
                    "respondió a tu comentario", url, comentario.Contenido);
                return;
            }

            if (comentario.SolucionId.HasValue)
            {
                var duenoId = await _context.soluciones.AsNoTracking()
                    .Where(s => s.Id == comentario.SolucionId.Value)
                    .Select(s => s.UsuarioId)
                    .FirstOrDefaultAsync();
                await _notificaciones.CrearAsync(duenoId, actorId, TiposNotificacion.Comentario,
                    "comentó tu solución", url, comentario.Contenido);
            }
            else if (comentario.PublicacionId.HasValue)
            {
                var duenoId = await _context.publicaciones.AsNoTracking()
                    .Where(p => p.Id == comentario.PublicacionId.Value)
                    .Select(p => p.UsuarioId)
                    .FirstOrDefaultAsync();
                await _notificaciones.CrearAsync(duenoId, actorId, TiposNotificacion.Comentario,
                    "comentó tu publicación", url, comentario.Contenido);
            }
        }

        public async Task<List<ComentarioDto>> ObtenerComentariosPorSolucionIdAsync(int solucionId)
        {
            var planos = await _context.Comentarios
                .Where(c => c.SolucionId == solucionId)
                .Proyectar(0)
                .ToListAsync();

            return ComentarioConsultas.ArmarArbol(planos);
        }

        public async Task<int> ObtenerTotalComentariosAsync()
        {
            return await _context.Comentarios.CountAsync();
        }

        public async Task<bool> EditarAsync(int id, string contenido, int usuarioId)
        {
            var comentario = await _context.Comentarios.FindAsync(id);

            // Solo puede editarlo el usuario que hizo el comentario
            if (comentario == null || comentario.UsuarioId != usuarioId)
            {
                return false;
            }

            comentario.Contenido = contenido.Trim();
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> EliminarAsync(int id, int usuarioId, bool esModerador = false)
        {
            var comentario = await _context.Comentarios.FindAsync(id);

            // Solo puede eliminarlo quien lo escribió (o el DBA, como moderación)
            if (comentario == null || (comentario.UsuarioId != usuarioId && !esModerador))
            {
                return false;
            }

            // Al eliminar un comentario principal también se eliminan sus respuestas
            // (la autorreferencia no admite cascada en SQL Server). Los likes sí se borran en cascada.
            await _context.Comentarios
                .Where(c => c.ComentarioPadreId == id)
                .ExecuteDeleteAsync();

            _context.Comentarios.Remove(comentario);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
