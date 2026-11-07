using ASOFOTEC.Data;
using ASOFOTEC.Dtos;
using ASOFOTEC.Models;
using ASOFOTEC.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ASOFOTEC.Services.Implementations
{
    public class ChatService : IChatService
    {
        public const int MaxCaracteres = 1000;
        private const int TamanoPagina = 50;

        private readonly AppDbContext _context;

        public ChatService(AppDbContext context)
        {
            _context = context;
        }

        // ---------------------------------------------------------------- reglas

        public async Task<string> EstadoDeChatAsync(int usuarioId, int otroId)
        {
            if (usuarioId == otroId) return "inactivo";

            var otroActivo = await _context.usuarios.AnyAsync(u => u.Id == otroId && u.Activo);
            if (!otroActivo) return "inactivo";

            var bloqueadores = await _context.Bloqueos
                .Where(b => (b.BloqueadorId == usuarioId && b.BloqueadoId == otroId) ||
                            (b.BloqueadorId == otroId && b.BloqueadoId == usuarioId))
                .Select(b => b.BloqueadorId)
                .ToListAsync();

            if (bloqueadores.Contains(usuarioId)) return "bloqueado_por_mi";
            if (bloqueadores.Count > 0) return "bloqueado";

            // Amigos = se siguen mutuamente (el índice único garantiza 2 filas como máximo)
            var relaciones = await _context.Seguimientos.CountAsync(s =>
                (s.SeguidorId == usuarioId && s.SeguidoId == otroId) ||
                (s.SeguidorId == otroId && s.SeguidoId == usuarioId));

            return relaciones == 2 ? "ok" : "no_amigos";
        }

        private static string? MotivoDe(string estado, string nombre) => estado switch
        {
            "ok" => null,
            "no_amigos" => "Solo pueden chatear las personas que se siguen mutuamente.",
            "bloqueado_por_mi" => $"Bloqueaste a {nombre}. Desbloquéalo para volver a escribirle.",
            _ => "No puedes enviar mensajes a esta persona."
        };

        private static (int A, int B) Par(int x, int y) => x < y ? (x, y) : (y, x);

        private static DateTime Utc(DateTime fecha) => DateTime.SpecifyKind(fecha, DateTimeKind.Utc);

        // ---------------------------------------------------------------- listas

        public async Task<List<ConversacionDto>> ObtenerConversacionesAsync(int usuarioId)
        {
            var filas = await _context.Conversaciones
                .AsNoTracking()
                .Where(c => (c.UsuarioAId == usuarioId || c.UsuarioBId == usuarioId) && c.Mensajes.Any())
                .OrderByDescending(c => c.UltimaActividad)
                .Take(100)
                .Select(c => new
                {
                    c.Id,
                    OtroId = c.UsuarioAId == usuarioId ? c.UsuarioBId : c.UsuarioAId,
                    Ultimo = c.Mensajes
                        .OrderByDescending(m => m.Id)
                        .Select(m => new { m.Texto, m.Fecha, m.RemitenteId })
                        .FirstOrDefault(),
                    NoLeidos = c.Mensajes.Count(m => m.RemitenteId != usuarioId && m.LeidoEn == null)
                })
                .ToListAsync();

            var ids = filas.Select(f => f.OtroId).ToList();
            var usuarios = await _context.usuarios
                .AsNoTracking()
                .Where(u => ids.Contains(u.Id))
                .Select(u => new { u.Id, u.Nombre, u.Apellido, u.FotoPerfilUrl })
                .ToDictionaryAsync(u => u.Id);

            var resultado = new List<ConversacionDto>();
            foreach (var f in filas)
            {
                if (!usuarios.TryGetValue(f.OtroId, out var otro) || f.Ultimo == null) continue;

                resultado.Add(new ConversacionDto
                {
                    Id = f.Id,
                    OtroId = f.OtroId,
                    OtroNombre = $"{otro.Nombre} {otro.Apellido}".Trim(),
                    OtroFoto = otro.FotoPerfilUrl,
                    UltimoTexto = f.Ultimo.Texto,
                    UltimaFecha = Utc(f.Ultimo.Fecha),
                    UltimoEsMio = f.Ultimo.RemitenteId == usuarioId,
                    NoLeidos = f.NoLeidos
                });
            }
            return resultado;
        }

        public async Task<List<PersonaDto>> ObtenerAmigosAsync(int usuarioId)
        {
            var ids = await _context.Seguimientos
                .Where(s => s.SeguidorId == usuarioId &&
                            _context.Seguimientos.Any(r => r.SeguidorId == s.SeguidoId && r.SeguidoId == usuarioId) &&
                            !_context.Bloqueos.Any(b =>
                                (b.BloqueadorId == usuarioId && b.BloqueadoId == s.SeguidoId) ||
                                (b.BloqueadorId == s.SeguidoId && b.BloqueadoId == usuarioId)))
                .Select(s => s.SeguidoId)
                .ToListAsync();

            return await _context.usuarios
                .AsNoTracking()
                .Where(u => ids.Contains(u.Id) && u.Activo)
                .OrderBy(u => u.Nombre)
                .Select(u => new PersonaDto
                {
                    Id = u.Id,
                    Nombre = u.Nombre + " " + u.Apellido,
                    FotoUrl = u.FotoPerfilUrl,
                    SigoYo = true,
                    MeSigue = true
                })
                .ToListAsync();
        }

        // ---------------------------------------------------------------- conversación

        public async Task<(int? ConversacionId, string? Error)> IniciarAsync(int usuarioId, int otroId)
        {
            var estado = await EstadoDeChatAsync(usuarioId, otroId);
            if (estado != "ok") return (null, MotivoDe(estado, "esta persona"));

            var (a, b) = Par(usuarioId, otroId);

            var existente = await _context.Conversaciones
                .Where(c => c.UsuarioAId == a && c.UsuarioBId == b)
                .Select(c => (int?)c.Id)
                .FirstOrDefaultAsync();
            if (existente.HasValue) return (existente, null);

            try
            {
                var conversacion = new Conversacion { UsuarioAId = a, UsuarioBId = b };
                _context.Conversaciones.Add(conversacion);
                await _context.SaveChangesAsync();
                return (conversacion.Id, null);
            }
            catch (DbUpdateException)
            {
                // Las dos personas la crearon a la vez: se usa la que ganó
                _context.ChangeTracker.Clear();
                var id = await _context.Conversaciones
                    .Where(c => c.UsuarioAId == a && c.UsuarioBId == b)
                    .Select(c => (int?)c.Id)
                    .FirstOrDefaultAsync();
                return (id, id == null ? "No se pudo iniciar el chat." : null);
            }
        }

        public async Task<int?> ObtenerOtroParticipanteAsync(int usuarioId, int conversacionId)
        {
            return await _context.Conversaciones
                .AsNoTracking()
                .Where(c => c.Id == conversacionId && (c.UsuarioAId == usuarioId || c.UsuarioBId == usuarioId))
                .Select(c => (int?)(c.UsuarioAId == usuarioId ? c.UsuarioBId : c.UsuarioAId))
                .FirstOrDefaultAsync();
        }

        private IQueryable<MensajeDto> Proyectar(IQueryable<Mensaje> consulta)
        {
            return consulta.Select(m => new MensajeDto
            {
                Id = m.Id,
                ConversacionId = m.ConversacionId,
                RemitenteId = m.RemitenteId,
                RemitenteNombre = m.Remitente!.Nombre + " " + m.Remitente.Apellido,
                Texto = m.Texto,
                Fecha = m.Fecha,
                Leido = m.LeidoEn != null
            });
        }

        public async Task<ConversacionActualVm?> ObtenerConversacionAsync(int usuarioId, int conversacionId)
        {
            var otroId = await ObtenerOtroParticipanteAsync(usuarioId, conversacionId);
            if (otroId == null) return null;

            var otro = await _context.usuarios
                .AsNoTracking()
                .Where(u => u.Id == otroId.Value)
                .Select(u => new { u.Nombre, u.Apellido, u.FotoPerfilUrl })
                .FirstOrDefaultAsync();
            if (otro == null) return null;

            var nombre = $"{otro.Nombre} {otro.Apellido}".Trim();
            var estado = await EstadoDeChatAsync(usuarioId, otroId.Value);

            var pagina = await ObtenerMensajesAsync(usuarioId, conversacionId, null, null);

            return new ConversacionActualVm
            {
                Id = conversacionId,
                MiId = usuarioId,
                OtroId = otroId.Value,
                OtroNombre = nombre,
                OtroFoto = otro.FotoPerfilUrl,
                Mensajes = pagina?.Mensajes ?? new List<MensajeDto>(),
                HayMas = pagina?.HayMas ?? false,
                PuedeEscribir = estado == "ok",
                Motivo = MotivoDe(estado, nombre),
                BloqueadoPorMi = estado == "bloqueado_por_mi"
            };
        }

        // antesDeId: mensajes más antiguos (para "cargar anteriores").
        // despuesDeId: mensajes nuevos (para recuperar lo perdido tras una reconexión).
        public async Task<MensajesPaginaDto?> ObtenerMensajesAsync(int usuarioId, int conversacionId, int? antesDeId, int? despuesDeId)
        {
            if (await ObtenerOtroParticipanteAsync(usuarioId, conversacionId) == null) return null;

            var consulta = _context.Mensajes.AsNoTracking().Where(m => m.ConversacionId == conversacionId);

            List<MensajeDto> lista;
            bool hayMas;

            if (despuesDeId.HasValue)
            {
                lista = await Proyectar(consulta.Where(m => m.Id > despuesDeId.Value).OrderBy(m => m.Id).Take(200)).ToListAsync();
                hayMas = false;
            }
            else
            {
                if (antesDeId.HasValue) consulta = consulta.Where(m => m.Id < antesDeId.Value);

                // Se piden TamanoPagina + 1 para saber si hay más
                var descendente = await Proyectar(consulta.OrderByDescending(m => m.Id).Take(TamanoPagina + 1)).ToListAsync();
                hayMas = descendente.Count > TamanoPagina;
                lista = descendente.Take(TamanoPagina).OrderBy(m => m.Id).ToList();
            }

            foreach (var m in lista) m.Fecha = Utc(m.Fecha);
            return new MensajesPaginaDto { Mensajes = lista, HayMas = hayMas };
        }

        // ---------------------------------------------------------------- enviar y leer

        public async Task<(MensajeDto? Mensaje, int? DestinatarioId, string? Error)> EnviarAsync(int usuarioId, int conversacionId, string? texto)
        {
            texto = (texto ?? string.Empty).Trim();
            if (texto.Length == 0) return (null, null, "Escribe un mensaje.");
            if (texto.Length > MaxCaracteres) return (null, null, $"El mensaje admite hasta {MaxCaracteres} caracteres.");

            var conversacion = await _context.Conversaciones
                .FirstOrDefaultAsync(c => c.Id == conversacionId && (c.UsuarioAId == usuarioId || c.UsuarioBId == usuarioId));
            if (conversacion == null) return (null, null, "La conversación no existe.");

            var otroId = conversacion.UsuarioAId == usuarioId ? conversacion.UsuarioBId : conversacion.UsuarioAId;

            var estado = await EstadoDeChatAsync(usuarioId, otroId);
            if (estado != "ok") return (null, null, MotivoDe(estado, "esta persona"));

            var mensaje = new Mensaje
            {
                ConversacionId = conversacionId,
                RemitenteId = usuarioId,
                Texto = texto,
                Fecha = DateTime.UtcNow
            };
            _context.Mensajes.Add(mensaje);
            conversacion.UltimaActividad = mensaje.Fecha;
            await _context.SaveChangesAsync();

            var nombre = await _context.usuarios
                .AsNoTracking()
                .Where(u => u.Id == usuarioId)
                .Select(u => u.Nombre + " " + u.Apellido)
                .FirstOrDefaultAsync() ?? "";

            return (new MensajeDto
            {
                Id = mensaje.Id,
                ConversacionId = conversacionId,
                RemitenteId = usuarioId,
                RemitenteNombre = nombre,
                Texto = mensaje.Texto,
                Fecha = Utc(mensaje.Fecha),
                Leido = false
            }, otroId, null);
        }

        public async Task<(int? OtroId, int Marcados)> MarcarLeidosAsync(int usuarioId, int conversacionId)
        {
            var otroId = await ObtenerOtroParticipanteAsync(usuarioId, conversacionId);
            if (otroId == null) return (null, 0);

            var ahora = DateTime.UtcNow;
            var marcados = await _context.Mensajes
                .Where(m => m.ConversacionId == conversacionId && m.RemitenteId != usuarioId && m.LeidoEn == null)
                .ExecuteUpdateAsync(s => s.SetProperty(m => m.LeidoEn, (DateTime?)ahora));

            return (otroId, marcados);
        }

        public async Task<int> ContarNoLeidosAsync(int usuarioId)
        {
            return await _context.Mensajes.CountAsync(m =>
                m.RemitenteId != usuarioId && m.LeidoEn == null &&
                (m.Conversacion!.UsuarioAId == usuarioId || m.Conversacion.UsuarioBId == usuarioId));
        }
    }
}
