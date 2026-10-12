using ASOFOTEC.Data;
using ASOFOTEC.Models;
using ASOFOTEC.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ASOFOTEC.Services.Implementations
{
    public class LikeService: ILikeService
    {
        private readonly AppDbContext _Context;
        public LikeService(AppDbContext context)
        {
            _Context = context;
        }

        public async Task<bool> ToggleLikesAsync(int solucionId, int usuarioId)
        {
            var existe = await _Context.LikeSoluciones.FirstOrDefaultAsync(l => 
            l.Solucion_Id == solucionId && l.Usuario_Id == usuarioId);

            //unlike
            if(existe != null)
            {
                _Context.LikeSoluciones.Remove(existe);
                await _Context.SaveChangesAsync();
                return false;
            }

            //like
            var like = new LikeSolucion
            {
                Solucion_Id = solucionId,
                Usuario_Id = usuarioId
            };
            like.FechaLike = DateTime.Now;
            await _Context.LikeSoluciones.AddAsync(like);
            await _Context.SaveChangesAsync();
            return true;

        }

        public async Task<int> ObtenerTotalLikesAsync(int solucionId)
        {
            return await _Context.LikeSoluciones.CountAsync(l => l.Solucion_Id == solucionId);
        }

        public async Task<bool> UsuarioDioLikeAsync(int solucionId, int usuarioId)
        {
            return await _Context.LikeSoluciones.AnyAsync(l =>
            l.Solucion_Id == solucionId && l.Usuario_Id == usuarioId);
        }

    }
}
