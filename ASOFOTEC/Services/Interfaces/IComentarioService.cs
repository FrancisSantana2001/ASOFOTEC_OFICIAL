using ASOFOTEC.Dtos;

namespace ASOFOTEC.Services.Interfaces
{
    public interface IComentarioService
    {
        Task CrearAsync(ComentarioCreateDto dto, int usuarioId);
        Task<List<ComentarioDto>> ObtenerComentariosPorSolucionIdAsync(int solucionId);
        Task<int> ObtenerTotalComentariosAsync();
        Task EliminarAsync(int id);
    }
}
