using ASOFOTEC.Data;
using ASOFOTEC.Models;
using ASOFOTEC.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ASOFOTEC.Services.Implementations
{
    public class PublicacionService : IPublicacionService
    {
        private readonly AppDbContext _context;
        private readonly IMediaService _mediaService;

        public PublicacionService(AppDbContext context, IMediaService mediaService)
        {
            _context = context;
            _mediaService = mediaService;
        }

        public async Task<bool> CrearAsync(string? contenido, string? mediaUrl, string? mediaTipo, int usuarioId)
        {
            // Debe tener texto o una foto/video
            if (string.IsNullOrWhiteSpace(contenido) && string.IsNullOrEmpty(mediaUrl))
            {
                return false;
            }

            var publicacion = new Publicacion
            {
                Contenido = string.IsNullOrWhiteSpace(contenido) ? null : contenido.Trim(),
                MediaUrl = mediaUrl,
                MediaTipo = mediaTipo,
                UsuarioId = usuarioId,
                Fecha_Publicacion = DateTime.Now
            };

            await _context.publicaciones.AddAsync(publicacion);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> EliminarAsync(int id, int usuarioId, bool esModerador = false)
        {
            var publicacion = await _context.publicaciones.FindAsync(id);

            // Solo puede eliminarla quien la publicó (o el DBA, como moderación)
            if (publicacion == null || (publicacion.UsuarioId != usuarioId && !esModerador))
            {
                return false;
            }

            var mediaUrl = publicacion.MediaUrl;

            await using var transaccion = await _context.Database.BeginTransactionAsync();

            // Los comentarios de la publicación se borran a mano (respuestas primero);
            // los likes de esos comentarios y de la publicación se borran en cascada.
            await _context.Comentarios
                .Where(c => c.PublicacionId == id && c.ComentarioPadreId != null)
                .ExecuteDeleteAsync();

            await _context.Comentarios
                .Where(c => c.PublicacionId == id)
                .ExecuteDeleteAsync();

            _context.publicaciones.Remove(publicacion);
            await _context.SaveChangesAsync();

            await transaccion.CommitAsync();

            // El archivo se borra del disco solo cuando la eliminación en base de datos fue exitosa
            _mediaService.Eliminar(mediaUrl);
            return true;
        }
    }
}
