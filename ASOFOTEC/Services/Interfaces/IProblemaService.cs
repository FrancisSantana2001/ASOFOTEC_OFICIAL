using ASOFOTEC.Dtos;

namespace ASOFOTEC.Services.Interfaces
{
    public interface IProblemaService
    {
        Task<List<ProblemaDto>> ObtenerTodosAsync(string? busqueda = null);
        Task<ProblemaDto?> ObtenerPorIdAsync(int id);
        Task<bool> CreateAsync(ProblemaCreateDto ProblemaCreateDto, int usuarioId);
        Task<bool> ElimiarAsync(int id, int usuarioId, bool esModerador = false);
        Task<List<ProblemaDto>> ObtenerDestacadosAsync(int cantidad = 3);
        Task<int> ObtenerTotalProblemasAsync();

        // Problemas que ya tienen una solución elegida (en ejecución o resueltos)
        Task<List<ProblemaDto>> ObtenerProyectosAsync();
        Task<int> ObtenerTotalProyectosAsync();   // en ejecución
        Task<int> ObtenerTotalResueltosAsync();   // con resultados documentados

    }
}
