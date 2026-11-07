using ASOFOTEC.Dtos;

namespace ASOFOTEC.Services.Interfaces
{
    // Pasos 2 al 7 de la ruta de un problema.
    // Los métodos de acción devuelven null si todo salió bien o un mensaje de error para mostrar al usuario.
    public interface IRutaProblemaService
    {
        Task<RutaProblemaVm> ObtenerAsync(int problemaId, int? usuarioId);

        Task<string?> GuardarAnalisisAsync(int problemaId, string? causas, string? consecuencias, int usuarioId);
        Task<string?> SeleccionarSolucionAsync(int problemaId, int solucionId, int usuarioId);
        Task<string?> UnirseAsync(int solucionId, int usuarioId);
        Task<string?> SalirAsync(int solucionId, int usuarioId);
        Task<string?> AgregarAvanceAsync(int solucionId, string? contenido, int? porcentaje, int usuarioId);
        Task<string?> EliminarAvanceAsync(int avanceId, int usuarioId);
        Task<string?> DocumentarResultadosAsync(int solucionId, string? resultados, int usuarioId);
    }
}
