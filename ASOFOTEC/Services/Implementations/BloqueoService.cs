using ASOFOTEC.Data;
using ASOFOTEC.Dtos;
using ASOFOTEC.Models;
using ASOFOTEC.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ASOFOTEC.Services.Implementations
{
    public class BloqueoService : IBloqueoService
    {
        private readonly AppDbContext _context;

        public BloqueoService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<(bool? Bloqueado, string? Error)> AlternarAsync(int usuarioId, int otroId)
        {
            if (usuarioId == otroId) return (null, "No puedes bloquearte a ti mismo.");

            if (!await _context.usuarios.AnyAsync(u => u.Id == otroId))
                return (null, "La persona no existe.");

            var existente = await _context.Bloqueos
                .FirstOrDefaultAsync(b => b.BloqueadorId == usuarioId && b.BloqueadoId == otroId);

            // Desbloquear
            if (existente != null)
            {
                _context.Bloqueos.Remove(existente);
                await _context.SaveChangesAsync();
                return (false, null);
            }

            // Bloquear: además se deja de seguir en ambos sentidos, así dejan de ser amigos
            await using var transaccion = await _context.Database.BeginTransactionAsync();

            _context.Bloqueos.Add(new Bloqueo { BloqueadorId = usuarioId, BloqueadoId = otroId, Fecha = DateTime.Now });
            await _context.SaveChangesAsync();

            await _context.Seguimientos
                .Where(s => (s.SeguidorId == usuarioId && s.SeguidoId == otroId) ||
                            (s.SeguidorId == otroId && s.SeguidoId == usuarioId))
                .ExecuteDeleteAsync();

            await transaccion.CommitAsync();
            return (true, null);
        }

        public async Task<List<PersonaDto>> ObtenerBloqueadosAsync(int usuarioId)
        {
            return await _context.Bloqueos
                .AsNoTracking()
                .Where(b => b.BloqueadorId == usuarioId)
                .OrderByDescending(b => b.Fecha)
                .Select(b => new PersonaDto
                {
                    Id = b.BloqueadoId,
                    Nombre = b.Bloqueado!.Nombre + " " + b.Bloqueado.Apellido,
                    FotoUrl = b.Bloqueado.FotoPerfilUrl
                })
                .ToListAsync();
        }
    }
}
