using ASOFOTEC.Data;
using ASOFOTEC.Dtos;
using ASOFOTEC.Helpers;
using ASOFOTEC.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ASOFOTEC.Services.Implementations
{
    public class FeedService : IFeedService
    {
        private readonly AppDbContext _context;

        public FeedService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<FeedViewModel> ObtenerFeedAsync(int usuarioId, string? busqueda, string orden, int cantidad, int? autorId = null)
        {
            busqueda = string.IsNullOrWhiteSpace(busqueda) ? null : busqueda.Trim();
            var popular = string.Equals(orden, "popular", StringComparison.OrdinalIgnoreCase);

            // Se pide uno más de lo necesario para saber si existe una página siguiente
            var limite = cantidad + 1;

            // ---------- Problemas ----------
            var qProblemas = _context.problemas.AsQueryable();
            if (autorId.HasValue)
            {
                qProblemas = qProblemas.Where(p => p.UsuarioId == autorId.Value);
            }
            if (busqueda != null)
            {
                qProblemas = qProblemas.Where(p =>
                    (p.Titulo ?? "").Contains(busqueda) || (p.Descripcion ?? "").Contains(busqueda));
            }
            qProblemas = popular
                ? qProblemas.OrderByDescending(p => p.Soluciones.Count()).ThenByDescending(p => p.FechaCreación)
                : qProblemas.OrderByDescending(p => p.FechaCreación);

            var problemas = await qProblemas
                .Take(limite)
                .Select(p => new FeedItemDto
                {
                    Tipo = FeedItemDto.TipoProblema,
                    Id = p.Id,
                    Fecha = p.FechaCreación,
                    UsuarioId = p.UsuarioId,
                    UsuarioNombre = p.Usuario != null ? p.Usuario.Nombre + " " + p.Usuario.Apellido : "ASOFOTEC",
                    UsuarioFoto = p.Usuario != null ? p.Usuario.FotoPerfilUrl : null,
                    Titulo = p.Titulo,
                    Categoria = p.Categoria,
                    Contenido = p.Descripcion,
                    ImageUrl = p.ImgeUrl,
                    TotalSoluciones = p.Soluciones.Count(),
                    CausasDocumentadas = (p.Causas ?? "") != "" || (p.Consecuencias ?? "") != "",
                    Seleccionada = p.Soluciones.Any(s => s.Seleccionada),
                    TotalAvances = p.Soluciones.Where(s => s.Seleccionada).SelectMany(s => s.Avances).Count(),
                    Resuelto = p.Soluciones.Any(s => s.Seleccionada && s.FechaResultados != null)
                })
                .ToListAsync();

            // Paso de la ruta en el que va cada problema
            foreach (var problema in problemas)
            {
                problema.Etapa = RutaProblemaHelper.CalcularEtapa(
                    problema.CausasDocumentadas, problema.TotalSoluciones,
                    problema.Seleccionada, problema.TotalAvances, problema.Resuelto);
            }

            // ---------- Soluciones ----------
            var qSoluciones = _context.soluciones.AsQueryable();
            if (autorId.HasValue)
            {
                qSoluciones = qSoluciones.Where(s => s.UsuarioId == autorId.Value);
            }
            if (busqueda != null)
            {
                qSoluciones = qSoluciones.Where(s => (s.Descripcion ?? "").Contains(busqueda));
            }
            qSoluciones = popular
                ? qSoluciones.OrderByDescending(s => s.Likes.Count() + s.Comentarios.Count()).ThenByDescending(s => s.Fecha)
                : qSoluciones.OrderByDescending(s => s.Fecha);

            var soluciones = await qSoluciones
                .Take(limite)
                .Select(s => new FeedItemDto
                {
                    Tipo = FeedItemDto.TipoSolucion,
                    Id = s.Id,
                    Fecha = s.Fecha,
                    UsuarioId = s.UsuarioId,
                    UsuarioNombre = s.Usuario!.Nombre + " " + s.Usuario.Apellido,
                    UsuarioFoto = s.Usuario.FotoPerfilUrl,
                    Contenido = s.Descripcion,
                    Estado = s.Estado,
                    ProblemaId = s.ProblemaId,
                    ProblemaTitulo = s.problema!.Titulo,
                    TotalLikes = s.Likes.Count(),
                    TotalComentarios = s.Comentarios.Count(),
                    UsuarioDioLike = s.Likes.Any(l => l.Usuario_Id == usuarioId)
                })
                .ToListAsync();

            // ---------- Publicaciones de usuarios ----------
            var qPublicaciones = _context.publicaciones.AsQueryable();
            if (autorId.HasValue)
            {
                qPublicaciones = qPublicaciones.Where(p => p.UsuarioId == autorId.Value);
            }
            if (busqueda != null)
            {
                qPublicaciones = qPublicaciones.Where(p => (p.Contenido ?? "").Contains(busqueda));
            }

            qPublicaciones = popular
                ? qPublicaciones.OrderByDescending(p => p.Likes.Count() + p.Comentarios.Count()).ThenByDescending(p => p.Fecha_Publicacion)
                : qPublicaciones.OrderByDescending(p => p.Fecha_Publicacion);

            var publicaciones = await qPublicaciones
                .Take(limite)
                .Select(p => new FeedItemDto
                {
                    Tipo = FeedItemDto.TipoPublicacion,
                    Id = p.Id,
                    Fecha = p.Fecha_Publicacion,
                    UsuarioId = p.UsuarioId,
                    UsuarioNombre = p.Usuario!.Nombre + " " + p.Usuario.Apellido,
                    UsuarioFoto = p.Usuario.FotoPerfilUrl,
                    Contenido = p.Contenido,
                    MediaUrl = p.MediaUrl,
                    MediaTipo = p.MediaTipo,
                    TotalLikes = p.Likes.Count(),
                    TotalComentarios = p.Comentarios.Count(),
                    UsuarioDioLike = p.Likes.Any(l => l.Usuario_Id == usuarioId)
                })
                .ToListAsync();

            // ---------- Mezcla y orden final ----------
            var todos = problemas.Concat(soluciones).Concat(publicaciones);

            var ordenados = popular
                ? todos.OrderByDescending(i => i.Popularidad).ThenByDescending(i => i.Fecha).ToList()
                : todos.OrderByDescending(i => i.Fecha).ToList();

            var modelo = new FeedViewModel
            {
                Busqueda = busqueda,
                Orden = popular ? "popular" : "reciente",
                Cantidad = cantidad,
                HayMas = ordenados.Count > cantidad,
                Items = ordenados.Take(cantidad).ToList()
            };

            // ---------- Comentarios (con respuestas y likes) de lo que se muestra ----------
            var solucionIds = modelo.Items.Where(i => i.Tipo == FeedItemDto.TipoSolucion).Select(i => i.Id).ToList();
            var publicacionIds = modelo.Items.Where(i => i.Tipo == FeedItemDto.TipoPublicacion).Select(i => i.Id).ToList();

            var planos = await _context.Comentarios
                .Where(c => (c.SolucionId != null && solucionIds.Contains(c.SolucionId.Value))
                         || (c.PublicacionId != null && publicacionIds.Contains(c.PublicacionId.Value)))
                .Proyectar(usuarioId)
                .ToListAsync();

            foreach (var item in modelo.Items)
            {
                if (item.Tipo == FeedItemDto.TipoSolucion)
                {
                    var deEste = planos.Where(c => c.SolucionId == item.Id).ToList();
                    item.TotalComentarios = deEste.Count;
                    item.Comentarios = ComentarioConsultas.ArmarArbol(deEste);
                }
                else if (item.Tipo == FeedItemDto.TipoPublicacion)
                {
                    var deEste = planos.Where(c => c.PublicacionId == item.Id).ToList();
                    item.TotalComentarios = deEste.Count;
                    item.Comentarios = ComentarioConsultas.ArmarArbol(deEste);
                }
            }

            // ---------- Barras laterales (solo en el feed general, no en la actividad de un perfil) ----------
            if (autorId == null)
            {
                modelo.Usuario = await _context.usuarios
                    .Where(u => u.Id == usuarioId)
                    .Select(u => new UsuarioResumenDto
                    {
                        Nombre = u.Nombre + " " + u.Apellido,
                        Correo = u.Correo,
                        Foto = u.FotoPerfilUrl,
                        Portada = u.FotoPortadaUrl,
                        TotalPublicaciones = u.Publicaciones.Count(),
                        TotalProblemas = u.Problemas.Count(),
                        TotalSoluciones = u.Soluciones.Count()
                    })
                    .FirstOrDefaultAsync() ?? new UsuarioResumenDto();

                modelo.ProblemasDestacados = await _context.problemas
                    .OrderByDescending(p => p.Soluciones.Count())
                    .ThenByDescending(p => p.FechaCreación)
                    .Take(5)
                    .Select(p => new ProblemaResumenDto
                    {
                        Id = p.Id,
                        Titulo = p.Titulo,
                        Categoria = p.Categoria,
                        TotalSoluciones = p.Soluciones.Count()
                    })
                    .ToListAsync();

                modelo.TotalUsuarios = await _context.usuarios.CountAsync();
                modelo.TotalProblemas = await _context.problemas.CountAsync();
                modelo.TotalSoluciones = await _context.soluciones.CountAsync();
            }

            return modelo;
        }

        // Una sola publicación con sus comentarios (página que se comparte por enlace)
        public async Task<FeedItemDto?> ObtenerPublicacionAsync(int id, int usuarioId)
        {
            var item = await _context.publicaciones
                .AsNoTracking()
                .Where(p => p.Id == id)
                .Select(p => new FeedItemDto
                {
                    Tipo = FeedItemDto.TipoPublicacion,
                    Id = p.Id,
                    Fecha = p.Fecha_Publicacion,
                    UsuarioId = p.UsuarioId,
                    UsuarioNombre = p.Usuario!.Nombre + " " + p.Usuario.Apellido,
                    UsuarioFoto = p.Usuario.FotoPerfilUrl,
                    Contenido = p.Contenido,
                    MediaUrl = p.MediaUrl,
                    MediaTipo = p.MediaTipo,
                    TotalLikes = p.Likes.Count(),
                    UsuarioDioLike = p.Likes.Any(l => l.Usuario_Id == usuarioId)
                })
                .FirstOrDefaultAsync();

            if (item == null) return null;

            var planos = await _context.Comentarios
                .Where(c => c.PublicacionId == id)
                .Proyectar(usuarioId)
                .ToListAsync();

            item.TotalComentarios = planos.Count;
            item.Comentarios = ComentarioConsultas.ArmarArbol(planos);
            return item;
        }
    }
}
