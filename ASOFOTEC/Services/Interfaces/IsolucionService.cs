using ASOFOTEC.Dtos;

namespace ASOFOTEC.Services.Interfaces
{
    public interface IsolucionService
    {
        Task<List<SolucionDto>> ObtenerTodosAsync();
        Task<List<SolucionDto>> ObtenerRecientesAsync();
        Task<List<SolucionDto>> ObtenerPorProblemaIdAsync(int problemaId);
        Task<bool> CrearAsync(SolucionCreateDto dto, int usuarioId);
        Task<bool> EliminarAsync(int id);
        Task<int> ObtenerTotalSolucionesAsync();
        Task<List<SolucionDto>> ObtenerRecientesAsync(int cantidad = 5);
        Task<int> ObtenerTotalSolucionAsync();

    }
}
