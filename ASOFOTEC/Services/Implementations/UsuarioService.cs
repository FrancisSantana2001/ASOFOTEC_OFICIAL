using ASOFOTEC.Data;
using ASOFOTEC.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ASOFOTEC.Services.Implementations
{
    public class UsuarioService : IUsuarioService
    {
        private readonly AppDbContext _context;

        public UsuarioService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<int> ObtenerTotalUsuariosAsync()
        {
            return await _context.usuarios.CountAsync();
        }
    }
}
