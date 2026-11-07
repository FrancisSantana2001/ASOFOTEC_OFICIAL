using ASOFOTEC.Dtos;

namespace ASOFOTEC.Services.Interfaces
{
    public interface IPerfilService
    {
        Task<PerfilVm?> ObtenerAsync(int usuarioId);

        // Devuelve null si todo salió bien o un mensaje de error
        Task<string?> ActualizarDatosAsync(int usuarioId, PerfilEditDto dto);

        Task<string?> ObtenerFotoAsync(int usuarioId, bool esPortada);
        Task EstablecerFotoAsync(int usuarioId, bool esPortada, string? url);
    }
}
