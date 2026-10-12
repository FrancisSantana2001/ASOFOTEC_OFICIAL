using ASOFOTEC.Dtos;

namespace ASOFOTEC.Services.Interfaces
{
    public interface IProblemaService
    {
        Task<List<ProblemaDto>> ObtenerTodosAsync();
        Task<ProblemaDto?> ObtenerPorIdAsync(int id);
        Task<bool> CreateAsync(ProblemaCreateDto ProblemaCreateDto);
        Task<bool> ElimiarAsync(int id);
        Task<List<ProblemaDto>> ObtenerDestacadosAsync(int cantidad = 3);
        Task<int> ObtenerTotalProblemasAsync();

    }
}
