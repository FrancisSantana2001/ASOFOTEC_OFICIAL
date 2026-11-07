using ASOFOTEC.Dtos;

namespace ASOFOTEC.Services.Interfaces
{
    public interface IsolucionService
    {
        Task<List<SolucionDto>> ObtenerTodosAsync();
        Task<List<SolucionDto>> ObtenerRecientesAsync();
        Task<List<SolucionDto>> ObtenerPorProblemaIdAsync(int problemaId, int? usuarioId = null);
        Task<bool> CrearAsync(SolucionCreateDto dto, int usuarioId);
        Task<bool> EliminarAsync(int id, int usuarioId, bool esModerador = false);
        Task<int> ObtenerTotalSolucionesAsync();
        Task<List<SolucionDto>> ObtenerRecientesAsync(int cantidad = 5);
        Task<int> ObtenerTotalSolucionAsync();

    }
}
