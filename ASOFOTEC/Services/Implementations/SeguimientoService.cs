using ASOFOTEC.Data;
using ASOFOTEC.Dtos;
using ASOFOTEC.Models;
using ASOFOTEC.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ASOFOTEC.Services.Implementations
{
    public class SeguimientoService : ISeguimientoService
    {
        private readonly AppDbContext _context;
        private readonly INotificacionService _notificaciones;

        public SeguimientoService(AppDbContext context, INotificacionService notificaciones)
        {
            _context = context;
            _notificaciones = notificaciones;
        }

        public async Task<(SeguimientoResultado? Resultado, string? Error)> AlternarAsync(int seguidorId, int seguidoId)
        {
            if (seguidorId == seguidoId) return (null, "No puedes seguirte a ti mismo.");

            var existeSeguido = await _context.usuarios.AnyAsync(u => u.Id == seguidoId && u.Activo);
            if (!existeSeguido) return (null, "La persona no existe.");

            // Con un bloqueo de por medio (de cualquiera de los dos) no se puede seguir
            var hayBloqueo = await _context.Bloqueos.AnyAsync(b =>
                (b.BloqueadorId == seguidorId && b.BloqueadoId == seguidoId) ||
                (b.BloqueadorId == seguidoId && b.BloqueadoId == seguidorId));
            if (hayBloqueo) return (null, "No puedes seguir a esta persona.");

            var relacion = await _context.Seguimientos
                .FirstOrDefaultAsync(s => s.SeguidorId == seguidorId && s.SeguidoId == seguidoId);

            bool siguiendo;
            if (relacion != null)
            {
                _context.Seguimientos.Remove(relacion);
                siguiendo = false;
            }
            else
            {
                _context.Seguimientos.Add(new Seguimiento
                {
                    SeguidorId = seguidorId,
                    SeguidoId = seguidoId,
                    Fecha = DateTime.Now
                });
                siguiendo = true;
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                // Dos clics seguidos pueden chocar con el índice único: se vuelve a leer el estado real
                _context.ChangeTracker.Clear();
                siguiendo = await _context.Seguimientos.AnyAsync(s => s.SeguidorId == seguidorId && s.SeguidoId == seguidoId);
            }

            if (siguiendo)
            {
                await _notificaciones.CrearAsync(seguidoId, seguidorId, TiposNotificacion.Seguidor,
                    "empezó a seguirte", $"/Perfil/Ver/{seguidorId}");
            }

            var total = await _context.Seguimientos.CountAsync(s => s.SeguidoId == seguidoId);
            return (new SeguimientoResultado(siguiendo, total), null);
        }

        // ---------------------------------------------------------------- perfil público

        public async Task<PerfilPublicoVm?> ObtenerPerfilAsync(int perfilId, int visitanteId)
        {
            var usuario = await _context.usuarios
                .AsNoTracking()
                .Where(u => u.Id == perfilId && u.Activo)
                .Select(u => new PerfilPublicoVm
                {
                    Id = u.Id,
                    Nombre = u.Nombre + " " + u.Apellido,
                    FotoPerfilUrl = u.FotoPerfilUrl,
                    FotoPortadaUrl = u.FotoPortadaUrl,
                    FechaIngreso = u.Fecha_Ingreso,
                    TotalPublicaciones = u.Publicaciones.Count(),
                    TotalProblemas = u.Problemas.Count(),
                    TotalSoluciones = u.Soluciones.Count()
                })
                .FirstOrDefaultAsync();

            if (usuario == null) return null;

            usuario.Seguidores = await _context.Seguimientos.CountAsync(s => s.SeguidoId == perfilId);
            usuario.Siguiendo = await _context.Seguimientos.CountAsync(s => s.SeguidorId == perfilId);
            usuario.SigoYo = await _context.Seguimientos.AnyAsync(s => s.SeguidorId == visitanteId && s.SeguidoId == perfilId);
            usuario.MeSigue = await _context.Seguimientos.AnyAsync(s => s.SeguidorId == perfilId && s.SeguidoId == visitanteId);
            usuario.BloqueadoPorMi = await _context.Bloqueos.AnyAsync(b => b.BloqueadorId == visitanteId && b.BloqueadoId == perfilId);
            usuario.MeBloqueo = await _context.Bloqueos.AnyAsync(b => b.BloqueadorId == perfilId && b.BloqueadoId == visitanteId);

            // En común: personas que yo sigo y que también siguen a este usuario
            if (visitanteId != perfilId)
            {
                var idsEnComun = await _context.Seguimientos
                    .Where(s => s.SeguidoId == perfilId &&
                                _context.Seguimientos.Any(m => m.SeguidorId == visitanteId && m.SeguidoId == s.SeguidorId))
                    .OrderByDescending(s => s.Fecha)
                    .Select(s => s.SeguidorId)
                    .Take(12)
                    .ToListAsync();

                usuario.EnComun = await ArmarPersonasAsync(idsEnComun, visitanteId);
            }

            return usuario;
        }

        // ---------------------------------------------------------------- sugerencias y búsqueda

        public async Task<List<PersonaDto>> ObtenerSugerenciasAsync(int usuarioId, int cantidad)
        {
            var siguiendo = await _context.Seguimientos
                .Where(s => s.SeguidorId == usuarioId)
                .Select(s => s.SeguidoId)
                .ToListAsync();

            // Personas con las que hay un bloqueo (en cualquier sentido): no se sugieren
            var bloqueados = await IdsBloqueadosAsync(usuarioId);

            // A quién no sugerir: a mí, a quienes ya sigo y a las personas bloqueadas
            var excluidos = siguiendo.Append(usuarioId).Concat(bloqueados).ToList();

            // 1. Personas seguidas por gente que yo sigo (conexiones en común)
            var porConexion = await _context.Seguimientos
                .Where(s => siguiendo.Contains(s.SeguidorId) && s.SeguidoId != usuarioId && !siguiendo.Contains(s.SeguidoId))
                .GroupBy(s => s.SeguidoId)
                .Select(g => new { Id = g.Key, Total = g.Count() })
                .ToListAsync();

            // 2. Personas que me siguen y yo todavía no sigo
            var meSiguen = await _context.Seguimientos
                .Where(s => s.SeguidoId == usuarioId && !siguiendo.Contains(s.SeguidorId))
                .Select(s => s.SeguidorId)
                .ToListAsync();

            var puntaje = new Dictionary<int, int>();
            foreach (var c in porConexion) puntaje[c.Id] = c.Total * 2;
            foreach (var id in meSiguen) puntaje[id] = (puntaje.TryGetValue(id, out var p) ? p : 0) + 3;

            var elegidos = puntaje
                .Where(kv => !bloqueados.Contains(kv.Key))
                .OrderByDescending(kv => kv.Value)
                .ThenBy(kv => kv.Key)
                .Select(kv => kv.Key)
                .Take(cantidad * 2)   // margen por si alguna cuenta está desactivada
                .ToList();

            // 3. Si faltan, se completan con las personas más activas de la comunidad
            if (elegidos.Count < cantidad)
            {
                var yaElegidos = elegidos.ToList();
                var activos = await _context.usuarios
                    .AsNoTracking()
                    .Where(u => u.Activo && !excluidos.Contains(u.Id) && !yaElegidos.Contains(u.Id))
                    .OrderByDescending(u => u.Problemas.Count() + u.Soluciones.Count() + u.Publicaciones.Count())
                    .ThenByDescending(u => u.Fecha_Ingreso)
                    .Select(u => u.Id)
                    .Take(cantidad)
                    .ToListAsync();
                elegidos.AddRange(activos);
            }

            var personas = await ArmarPersonasAsync(elegidos, usuarioId);
            return personas.Take(cantidad).ToList();
        }

        public async Task<List<PersonaDto>> BuscarAsync(int usuarioId, string? texto, int cantidad)
        {
            if (string.IsNullOrWhiteSpace(texto)) return new List<PersonaDto>();

            var t = texto.Trim();
            var bloqueados = await IdsBloqueadosAsync(usuarioId);
            var ids = await _context.usuarios
                .AsNoTracking()
                .Where(u => u.Activo && u.Id != usuarioId && !bloqueados.Contains(u.Id) &&
                            (u.Nombre + " " + u.Apellido).Contains(t))
                .OrderBy(u => u.Nombre)
                .Select(u => u.Id)
                .Take(cantidad)
                .ToListAsync();

            return await ArmarPersonasAsync(ids, usuarioId);
        }

        public async Task<List<PersonaDto>> ObtenerConexionesAsync(int perfilId, bool seguidores, int visitanteId, int cantidad)
        {
            var consulta = seguidores
                ? _context.Seguimientos.Where(s => s.SeguidoId == perfilId).OrderByDescending(s => s.Fecha).Select(s => s.SeguidorId)
                : _context.Seguimientos.Where(s => s.SeguidorId == perfilId).OrderByDescending(s => s.Fecha).Select(s => s.SeguidoId);

            var ids = await consulta.Take(cantidad).ToListAsync();
            return await ArmarPersonasAsync(ids, visitanteId);
        }

        // ---------------------------------------------------------------- utilidades

        // Ids de las personas con las que hay un bloqueo, ya lo haya hecho yo o ellas
        private async Task<List<int>> IdsBloqueadosAsync(int usuarioId)
        {
            return await _context.Bloqueos
                .Where(b => b.BloqueadorId == usuarioId || b.BloqueadoId == usuarioId)
                .Select(b => b.BloqueadorId == usuarioId ? b.BloqueadoId : b.BloqueadorId)
                .ToListAsync();
        }

        // Convierte ids en personas (en el mismo orden), con el estado de seguimiento respecto al visitante
        private async Task<List<PersonaDto>> ArmarPersonasAsync(List<int> ids, int visitanteId)
        {
            if (ids.Count == 0) return new List<PersonaDto>();

            var usuarios = await _context.usuarios
                .AsNoTracking()
                .Where(u => ids.Contains(u.Id) && u.Activo)
                .Select(u => new { u.Id, u.Nombre, u.Apellido, u.FotoPerfilUrl })
                .ToListAsync();

            var siguiendo = await _context.Seguimientos
                .Where(s => s.SeguidorId == visitanteId)
                .Select(s => s.SeguidoId)
                .ToListAsync();

            var meSiguen = await _context.Seguimientos
                .Where(s => s.SeguidoId == visitanteId && ids.Contains(s.SeguidorId))
                .Select(s => s.SeguidorId)
                .ToListAsync();

            var enComun = await _context.Seguimientos
                .Where(s => ids.Contains(s.SeguidoId) && siguiendo.Contains(s.SeguidorId))
                .GroupBy(s => s.SeguidoId)
                .Select(g => new { Id = g.Key, Total = g.Count() })
                .ToListAsync();

            var porId = usuarios.ToDictionary(u => u.Id);
            var siguiendoSet = new HashSet<int>(siguiendo);
            var meSiguenSet = new HashSet<int>(meSiguen);
            var enComunPorId = enComun.ToDictionary(e => e.Id, e => e.Total);

            var resultado = new List<PersonaDto>();
            foreach (var id in ids.Distinct())
            {
                if (!porId.TryGetValue(id, out var u)) continue;

                resultado.Add(new PersonaDto
                {
                    Id = u.Id,
                    Nombre = $"{u.Nombre} {u.Apellido}".Trim(),
                    FotoUrl = u.FotoPerfilUrl,
                    SigoYo = siguiendoSet.Contains(id),
                    MeSigue = meSiguenSet.Contains(id),
                    EnComun = enComunPorId.TryGetValue(id, out var total) ? total : 0
                });
            }
            return resultado;
        }
    }
}
