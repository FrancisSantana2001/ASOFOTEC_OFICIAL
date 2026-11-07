using ASOFOTEC.Data;
using ASOFOTEC.Dtos;
using ASOFOTEC.Models;
using ASOFOTEC.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ASOFOTEC.Services.Implementations
{
    public class NotificacionService : INotificacionService
    {
        private const int MaxExtracto = 80;

        private readonly AppDbContext _context;
        private readonly ILogger<NotificacionService> _logger;

        public NotificacionService(AppDbContext context, ILogger<NotificacionService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task CrearAsync(int receptorId, int actorId, string tipo, string mensaje, string url, string? extracto = null)
        {
            Notificacion? nueva = null;
            try
            {
                // Nadie recibe avisos de sus propias acciones
                if (receptorId == actorId) return;

                // Ni de personas con las que hay un bloqueo
                var hayBloqueo = await _context.Bloqueos.AnyAsync(b =>
                    (b.BloqueadorId == receptorId && b.BloqueadoId == actorId) ||
                    (b.BloqueadorId == actorId && b.BloqueadoId == receptorId));
                if (hayBloqueo) return;

                // Evita repetir el mismo aviso mientras el anterior siga sin leerse
                // (por ejemplo, quitar y volver a poner un "me gusta")
                var desde = DateTime.Now.AddHours(-24);
                var repetida = await _context.Notificaciones.AnyAsync(n =>
                    n.UsuarioId == receptorId && n.ActorId == actorId &&
                    n.Tipo == tipo && n.Url == url && !n.Leida && n.Fecha >= desde);
                if (repetida) return;

                if (extracto != null && extracto.Length > MaxExtracto)
                {
                    extracto = extracto.Substring(0, MaxExtracto).TrimEnd() + "...";
                }

                nueva = new Notificacion
                {
                    UsuarioId = receptorId,
                    ActorId = actorId,
                    Tipo = tipo,
                    Mensaje = mensaje,
                    Extracto = string.IsNullOrWhiteSpace(extracto) ? null : extracto,
                    Url = url,
                    Leida = false,
                    Fecha = DateTime.Now
                };
                _context.Notificaciones.Add(nueva);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                if (nueva != null)
                {
                    _context.Entry(nueva).State = EntityState.Detached;
                }
                // Por ejemplo, si todavía no se aplicó la migración de notificaciones
                _logger.LogWarning(ex, "No se pudo crear la notificación ({Tipo}).", tipo);
            }
        }

        public async Task<string?> UrlDestinoAsync(int? solucionId, int? publicacionId)
        {
            if (publicacionId.HasValue)
            {
                return $"/Publicacion/Ver/{publicacionId.Value}";
            }

            if (solucionId.HasValue)
            {
                var problemaId = await _context.soluciones
                    .AsNoTracking()
                    .Where(s => s.Id == solucionId.Value)
                    .Select(s => (int?)s.ProblemaId)
                    .FirstOrDefaultAsync();

                return problemaId.HasValue ? $"/Problema/Details/{problemaId.Value}?paso=3" : null;
            }

            return null;
        }

        public async Task<int> ContarNoLeidasAsync(int usuarioId)
        {
            return await _context.Notificaciones.CountAsync(n => n.UsuarioId == usuarioId && !n.Leida);
        }

        private IQueryable<NotificacionDto> Proyectar(IQueryable<Notificacion> consulta)
        {
            return consulta.Select(n => new NotificacionDto
            {
                Id = n.Id,
                ActorId = n.ActorId,
                ActorNombre = n.Actor!.Nombre + " " + n.Actor.Apellido,
                ActorFoto = n.Actor.FotoPerfilUrl,
                Tipo = n.Tipo,
                Mensaje = n.Mensaje,
                Extracto = n.Extracto,
                Url = n.Url,
                Leida = n.Leida,
                Fecha = n.Fecha
            });
        }

        public async Task<List<NotificacionDto>> ObtenerRecientesAsync(int usuarioId, int cantidad)
        {
            return await Proyectar(_context.Notificaciones
                    .AsNoTracking()
                    .Where(n => n.UsuarioId == usuarioId)
                    .OrderByDescending(n => n.Fecha)
                    .Take(cantidad))
                .ToListAsync();
        }

        public async Task<(List<NotificacionDto> Items, bool HayMas)> ObtenerPaginaAsync(int usuarioId, int pagina, int tamano)
        {
            if (pagina < 1) pagina = 1;

            var items = await Proyectar(_context.Notificaciones
                    .AsNoTracking()
                    .Where(n => n.UsuarioId == usuarioId)
                    .OrderByDescending(n => n.Fecha)
                    .Skip((pagina - 1) * tamano)
                    .Take(tamano + 1))
                .ToListAsync();

            var hayMas = items.Count > tamano;
            return (items.Take(tamano).ToList(), hayMas);
        }

        public async Task<string?> AbrirAsync(int notificacionId, int usuarioId)
        {
            var notificacion = await _context.Notificaciones
                .FirstOrDefaultAsync(n => n.Id == notificacionId && n.UsuarioId == usuarioId);

            if (notificacion == null) return null;

            if (!notificacion.Leida)
            {
                notificacion.Leida = true;
                await _context.SaveChangesAsync();
            }
            return notificacion.Url;
        }

        public async Task MarcarTodasAsync(int usuarioId)
        {
            await _context.Notificaciones
                .Where(n => n.UsuarioId == usuarioId && !n.Leida)
                .ExecuteUpdateAsync(s => s.SetProperty(n => n.Leida, true));
        }
    }
}
