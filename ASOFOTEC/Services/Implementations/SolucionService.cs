using ASOFOTEC.Data;
using ASOFOTEC.Dtos;
using ASOFOTEC.Models;
using ASOFOTEC.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ASOFOTEC.Services.Implementations
{
    public class SolucionService: IsolucionService
    {
        private readonly AppDbContext _context;
        private readonly INotificacionService _notificaciones;
        public SolucionService( AppDbContext dbContext, INotificacionService notificaciones)
        {
            _context = dbContext;
            _notificaciones = notificaciones;
        }

        public async Task<List<SolucionDto>> ObtenerTodosAsync()
        {
            return await _context.soluciones
                .Include(s => s.Usuario)
                .Include(s => s.Likes)
                .Include(s => s.Comentarios)
                .OrderByDescending(s => s.Fecha)
                .Select(s => new SolucionDto
                {
                    Id = s.Id,
                    Problema_Id = s.ProblemaId,
                    UsuarioId = s.UsuarioId,
                    Descripcion = s.Descripcion,
                    Estado = s.Estado,
                    Fecha_Solucion = s.Fecha,
                    UsuarioNombre = s.Usuario!.Nombre + " " + s.Usuario.Apellido,
                    TotalLikes = s.Likes.Count(),
                    TotalComentarios = s.Comentarios.Count()
                }).ToListAsync();
        }

        public async Task<List<SolucionDto>> ObtenerRecientesAsync()
        {
            return await _context.soluciones
                .OrderByDescending(s => s.Fecha)
                .Take(5)
                .Select(s => new SolucionDto
                {
                    Id = s.Id,
                    Problema_Id = s.ProblemaId,
                    UsuarioId = s.UsuarioId,
                    Descripcion = s.Descripcion,
                    Estado = s.Estado,
                    Fecha_Solucion = s.Fecha
                    
                }).ToListAsync();
        }

        public async Task<List<SolucionDto>> ObtenerPorProblemaIdAsync(int problemaId, int? usuarioId = null)
        {
            var uid = usuarioId ?? 0;

            var soluciones = await _context.soluciones
                .Where(s => s.ProblemaId == problemaId)
                .OrderByDescending(s => s.Fecha)
                .Select(s => new SolucionDto
                {
                    Id = s.Id,
                    Problema_Id = s.ProblemaId,
                    UsuarioId = s.UsuarioId,
                    Descripcion = s.Descripcion,
                    Estado = s.Estado,
                    Fecha_Solucion = s.Fecha,
                    UsuarioNombre = s.Usuario!.Nombre + " " + s.Usuario.Apellido,
                    TotalLikes = s.Likes.Count(),
                    UsuarioDioLike = s.Likes.Any(l => l.Usuario_Id == uid)
                }).ToListAsync();

            // Comentarios (con respuestas y likes) de todas las soluciones en una sola consulta
            var ids = soluciones.Select(s => s.Id).ToList();

            var planos = await _context.Comentarios
                .Where(c => c.SolucionId != null && ids.Contains(c.SolucionId.Value))
                .Proyectar(uid)
                .ToListAsync();

            foreach (var solucion in soluciones)
            {
                var deEstaSolucion = planos.Where(c => c.SolucionId == solucion.Id).ToList();
                solucion.TotalComentarios = deEstaSolucion.Count;
                solucion.comentarios = ComentarioConsultas.ArmarArbol(deEstaSolucion);
            }

            return soluciones;
        }

        private static readonly string[] Niveles = { "Bajo", "Medio", "Alto" };

        public async Task<bool> CrearAsync(SolucionCreateDto dto, int usuarioId)
        {
            // El problema debe existir y no estar ya resuelto
            if (!await _context.problemas.AnyAsync(p => p.Id == dto.ProblemaId))
            {
                return false;
            }

            if (await _context.soluciones.AnyAsync(s => s.ProblemaId == dto.ProblemaId && s.FechaResultados != null))
            {
                return false;
            }

            var solucion = new Solucion
            {
                ProblemaId = dto.ProblemaId,
                Descripcion = dto.Descripcion,
                Estado = "Propuesta",
                Fecha = DateTime.Now,
                UsuarioId = usuarioId,
                // Datos para comparar propuestas: solo se aceptan los valores permitidos
                Impacto = Niveles.Contains(dto.Impacto) ? dto.Impacto : null,
                Costo = Niveles.Contains(dto.Costo) ? dto.Costo : null,
                TiempoEstimado = string.IsNullOrWhiteSpace(dto.TiempoEstimado) ? null : dto.TiempoEstimado.Trim()
            };
            await _context.soluciones.AddAsync(solucion);
            await _context.SaveChangesAsync();

            // Aviso al creador del problema
            var problema = await _context.problemas.AsNoTracking()
                .Where(p => p.Id == dto.ProblemaId)
                .Select(p => new { p.UsuarioId, p.Titulo })
                .FirstOrDefaultAsync();
            if (problema?.UsuarioId != null)
            {
                await _notificaciones.CrearAsync(problema.UsuarioId.Value, usuarioId, TiposNotificacion.Solucion,
                    "propuso una solución a tu problema", $"/Problema/Details/{dto.ProblemaId}?paso=3", problema.Titulo);
            }
            return true;
        }

        public async Task<bool> EliminarAsync(int id, int usuarioId, bool esModerador = false)
        {
            var solucion = await _context.soluciones.FindAsync(id);

            // Solo puede eliminarla quien la publicó (o el DBA, como moderación)
            if (solucion == null || (solucion.UsuarioId != usuarioId && !esModerador))
            { 
                return false;
            }

            _context.soluciones.Remove(solucion);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> ObtenerTotalSolucionesAsync()
        {
            return await _context.soluciones.CountAsync();
        }

        public async Task<List<SolucionDto>> ObtenerRecientesAsync(int cantidad = 3)
        {
            return await _context.soluciones
                .Include(s => s.Usuario)
                .Include(s => s.Likes)
                .Include(s => s.Comentarios)
                .OrderByDescending(s => s.Fecha)
                .Take(cantidad)
                .Select(s => new SolucionDto
                {
                    Id = s.Id,
                    Problema_Id = s.ProblemaId,
                    UsuarioId = s.UsuarioId,
                    Descripcion = s.Descripcion,
                    Estado = s.Estado,
                    Fecha_Solucion = s.Fecha,
                    ProblemaTitulo = s.problema!.Titulo,
                    UsuarioNombre = s.Usuario!.Nombre + " " + s.Usuario.Apellido,
                    TotalLikes = s.Likes.Count(),
                    TotalComentarios = s.Comentarios.Count()
                }).ToListAsync();
        }

        public async Task<int> ObtenerTotalSolucionAsync()
        {
            return await _context.soluciones.CountAsync();
        }


    }
}
