using ASOFOTEC.Dtos;
using ASOFOTEC.Models;

namespace ASOFOTEC.Services
{
    public static class ComentarioConsultas
    {
        // Proyecta comentarios planos (con likes y si el usuario actual dio like)
        public static IQueryable<ComentarioDto> Proyectar(this IQueryable<Comentario> consulta, int usuarioId)
        {
            return consulta.Select(c => new ComentarioDto
            {
                id = c.Id,
                contenido = c.Contenido,
                UsuarioId = c.UsuarioId,
                UserName = c.Usuario!.Nombre + " " + c.Usuario.Apellido,
                UserFoto = c.Usuario.FotoPerfilUrl,
                FechaComentario = c.FechaComentario,
                SolucionId = c.SolucionId,
                PublicacionId = c.PublicacionId,
                ComentarioPadreId = c.ComentarioPadreId,
                TotalLikes = c.Likes.Count(),
                UsuarioDioLike = c.Likes.Any(l => l.Usuario_Id == usuarioId)
            });
        }

        // Convierte la lista plana en comentarios principales (más recientes primero),
        // cada uno con sus respuestas (más antiguas primero)
        public static List<ComentarioDto> ArmarArbol(IEnumerable<ComentarioDto> planos)
        {
            var lista = planos.ToList();

            var respuestasPorPadre = lista
                .Where(c => c.ComentarioPadreId.HasValue)
                .GroupBy(c => c.ComentarioPadreId!.Value)
                .ToDictionary(g => g.Key, g => g.OrderBy(c => c.FechaComentario).ToList());

            var principales = lista
                .Where(c => !c.ComentarioPadreId.HasValue)
                .OrderByDescending(c => c.FechaComentario)
                .ToList();

            foreach (var principal in principales)
            {
                if (respuestasPorPadre.TryGetValue(principal.id, out var respuestas))
                {
                    principal.Respuestas = respuestas;
                }
            }

            return principales;
        }
    }
}
