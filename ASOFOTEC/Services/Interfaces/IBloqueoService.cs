using ASOFOTEC.Dtos;

namespace ASOFOTEC.Services.Interfaces
{
    public interface IBloqueoService
    {
        // Bloquea o desbloquea. Devuelve si quedó bloqueado o un mensaje de error
        Task<(bool? Bloqueado, string? Error)> AlternarAsync(int usuarioId, int otroId);

        Task<List<PersonaDto>> ObtenerBloqueadosAsync(int usuarioId);
    }
}
