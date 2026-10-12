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
        public SolucionService( AppDbContext dbContext)
        {
            _context = dbContext;
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
                    Descripcion = s.Descripcion,
                    Estado = s.Estado,
                    Fecha_Solucion = s.Fecha
                    
                }).ToListAsync();
        }

        public async Task<List<SolucionDto>> ObtenerPorProblemaIdAsync(int problemaId)
        {
            return await _context.soluciones
                .Where(s => s.ProblemaId == problemaId)
                .OrderByDescending(s => s.Fecha)
                .Select(s => new SolucionDto
                {
                    Id = s.Id,
                    Problema_Id = s.ProblemaId,
                    Descripcion = s.Descripcion,
                    Estado = s.Estado,
                    Fecha_Solucion = s.Fecha,
                    TotalLikes = s.Likes.Count(),
                    TotalComentarios = s.Comentarios.Count(),
                    comentarios = s.Comentarios
                        .OrderByDescending(c => c.FechaComentario)
                        .Select(c => new ComentarioDto
                        {
                            id = c.Id,
                            contenido = c.Contenido,
                            UserName = c.Usuario!.Nombre + " " + c.Usuario.Apellido,
                            FechaComentario = c.FechaComentario
                        }).ToList()
                }).ToListAsync();
        }

        public async Task<bool> CrearAsync(SolucionCreateDto dto, int usuarioId)
        {
            var solucion = new Solucion
            {
                ProblemaId = dto.ProblemaId,
                Descripcion = dto.Descripcion,
                Estado = "Propuesta",
                Fecha = DateTime.Now,
                UsuarioId = usuarioId
            };
            await _context.soluciones.AddAsync(solucion);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> EliminarAsync(int id)
        {
            var solucion = await _context.soluciones.FindAsync(id);

            if (solucion == null)
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
                    Descripcion = s.Descripcion,
                    Estado = s.Estado,
                    Fecha_Solucion = s.Fecha,
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
