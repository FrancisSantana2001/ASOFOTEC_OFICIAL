using ASOFOTEC.Dtos;

namespace ASOFOTEC.Services.Interfaces
{
    public interface IComentarioService
    {
        Task<ComentarioDto?> CrearAsync(ComentarioCreateDto dto, int usuarioId);
        Task<List<ComentarioDto>> ObtenerComentariosPorSolucionIdAsync(int solucionId);
        Task<int> ObtenerTotalComentariosAsync();
        Task<bool> EditarAsync(int id, string contenido, int usuarioId);
        Task<bool> EliminarAsync(int id, int usuarioId, bool esModerador = false);
    }
}
