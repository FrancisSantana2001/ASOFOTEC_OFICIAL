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
        public ComentarioService(AppDbContext dbContext)
        {
            _context = dbContext;
        }

        public async Task CrearAsync(ComentarioCreateDto dto, int usuarioId)
        {
            var comentario = new Comentario
            {
                Contenido = dto.Contenido,
                SolucionId = dto.SolucionId,
                UsuarioId = usuarioId,
                FechaComentario = DateTime.Now
            };
            await _context.Comentarios.AddAsync(comentario);
            await _context.SaveChangesAsync();
        }

        public async Task<List<ComentarioDto>> ObtenerComentariosPorSolucionIdAsync(int solucionId)
        {
            return await _context.Comentarios
                .Where(c => c.SolucionId == solucionId)
                .Include(c => c.Usuario)
                .OrderByDescending(c => c.FechaComentario)
                .Select(c => new ComentarioDto
                {
                    id = c.Id,
                    contenido = c.Contenido,
                    UserName =
                    c.Usuario!.Nombre + " " + c.Usuario.Apellido,
                    FechaComentario = c.FechaComentario
                }).ToListAsync();

        }

        public async Task<int> ObtenerTotalComentariosAsync()
        {
            return await _context.Comentarios.CountAsync();
        }

        public async Task EliminarAsync(int id)
        {
            var comentario = await _context.Comentarios.FindAsync(id);
            if (comentario != null)
            {
                _context.Comentarios.Remove(comentario);
                await _context.SaveChangesAsync();
            }
            return;
        }
    }
}
