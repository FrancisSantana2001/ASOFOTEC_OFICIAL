using ASOFOTEC.Data;
using ASOFOTEC.Dtos;
using ASOFOTEC.Models;
using ASOFOTEC.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ASOFOTEC.Services.Implementations
{
    public class LikeService: ILikeService
    {
        private readonly AppDbContext _Context;
        private readonly INotificacionService _notificaciones;
        public LikeService(AppDbContext context, INotificacionService notificaciones)
        {
            _Context = context;
            _notificaciones = notificaciones;
        }

        public async Task<bool> ToggleLikesAsync(int solucionId, int usuarioId)
        {
            var existe = await _Context.LikeSoluciones.FirstOrDefaultAsync(l => 
            l.Solucion_Id == solucionId && l.Usuario_Id == usuarioId);

            //unlike
            if(existe != null)
            {
                _Context.LikeSoluciones.Remove(existe);
                await _Context.SaveChangesAsync();
                return false;
            }

            //like
            var like = new LikeSolucion
            {
                Solucion_Id = solucionId,
                Usuario_Id = usuarioId
            };
            like.FechaLike = DateTime.Now;
            await _Context.LikeSoluciones.AddAsync(like);
            await _Context.SaveChangesAsync();

            // Aviso al autor de la solución
            var solucion = await _Context.soluciones.AsNoTracking()
                .Where(s => s.Id == solucionId)
                .Select(s => new { s.UsuarioId, s.ProblemaId })
                .FirstOrDefaultAsync();
            if (solucion != null)
            {
                await _notificaciones.CrearAsync(solucion.UsuarioId, usuarioId, TiposNotificacion.Like,
                    "le dio me gusta a tu solución", $"/Problema/Details/{solucion.ProblemaId}?paso=3");
            }
            return true;

        }

        public async Task<int> ObtenerTotalLikesAsync(int solucionId)
        {
            return await _Context.LikeSoluciones.CountAsync(l => l.Solucion_Id == solucionId);
        }

        public async Task<bool> UsuarioDioLikeAsync(int solucionId, int usuarioId)
        {
            return await _Context.LikeSoluciones.AnyAsync(l =>
            l.Solucion_Id == solucionId && l.Usuario_Id == usuarioId);
        }

        public async Task<LikeResultado?> ToggleLikePublicacionAsync(int publicacionId, int usuarioId)
        {
            if (!await _Context.publicaciones.AnyAsync(p => p.Id == publicacionId))
            {
                return null;
            }

            var existe = await _Context.LikePublicaciones.FirstOrDefaultAsync(l =>
                l.Publicacion_Id == publicacionId && l.Usuario_Id == usuarioId);

            bool liked;
            if (existe != null)
            {
                _Context.LikePublicaciones.Remove(existe);
                liked = false;
            }
            else
            {
                await _Context.LikePublicaciones.AddAsync(new LikePublicacion
                {
                    Publicacion_Id = publicacionId,
                    Usuario_Id = usuarioId,
                    FechaLike = DateTime.Now
                });
                liked = true;
            }
            await _Context.SaveChangesAsync();

            if (liked)
            {
                var autorId = await _Context.publicaciones.AsNoTracking()
                    .Where(pb => pb.Id == publicacionId)
                    .Select(pb => pb.UsuarioId)
                    .FirstOrDefaultAsync();
                await _notificaciones.CrearAsync(autorId, usuarioId, TiposNotificacion.Like,
                    "le dio me gusta a tu publicación", $"/Publicacion/Ver/{publicacionId}");
            }

            var total = await _Context.LikePublicaciones.CountAsync(l => l.Publicacion_Id == publicacionId);
            return new LikeResultado(liked, total);
        }

        public async Task<LikeResultado?> ToggleLikeComentarioAsync(int comentarioId, int usuarioId)
        {
            if (!await _Context.Comentarios.AnyAsync(c => c.Id == comentarioId))
            {
                return null;
            }

            var existe = await _Context.LikeComentarios.FirstOrDefaultAsync(l =>
                l.Comentario_Id == comentarioId && l.Usuario_Id == usuarioId);

            bool liked;
            if (existe != null)
            {
                _Context.LikeComentarios.Remove(existe);
                liked = false;
            }
            else
            {
                await _Context.LikeComentarios.AddAsync(new LikeComentario
                {
                    Comentario_Id = comentarioId,
                    Usuario_Id = usuarioId,
                    FechaLike = DateTime.Now
                });
                liked = true;
            }
            await _Context.SaveChangesAsync();

            if (liked)
            {
                var comentario = await _Context.Comentarios.AsNoTracking()
                    .Where(c => c.Id == comentarioId)
                    .Select(c => new { c.UsuarioId, c.SolucionId, c.PublicacionId, c.Contenido })
                    .FirstOrDefaultAsync();

                if (comentario != null)
                {
                    var url = await _notificaciones.UrlDestinoAsync(comentario.SolucionId, comentario.PublicacionId);
                    if (url != null)
                    {
                        await _notificaciones.CrearAsync(comentario.UsuarioId, usuarioId, TiposNotificacion.Like,
                            "le dio me gusta a tu comentario", url, comentario.Contenido);
                    }
                }
            }

            var total = await _Context.LikeComentarios.CountAsync(l => l.Comentario_Id == comentarioId);
            return new LikeResultado(liked, total);
        }

    }
}
