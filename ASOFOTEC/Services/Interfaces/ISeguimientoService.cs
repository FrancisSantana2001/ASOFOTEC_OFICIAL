using ASOFOTEC.Dtos;

namespace ASOFOTEC.Services.Interfaces
{
    public interface ISeguimientoService
    {
        // Sigue o deja de seguir. Devuelve el resultado o un mensaje de error
        Task<(SeguimientoResultado? Resultado, string? Error)> AlternarAsync(int seguidorId, int seguidoId);

        // Perfil que ve otro usuario (null si no existe o está desactivado)
        Task<PerfilPublicoVm?> ObtenerPerfilAsync(int perfilId, int visitanteId);

        // Personas que quizá conozcas: seguidas por gente que sigues, y quienes te siguen y tú no
        Task<List<PersonaDto>> ObtenerSugerenciasAsync(int usuarioId, int cantidad);

        Task<List<PersonaDto>> BuscarAsync(int usuarioId, string? texto, int cantidad);

        // Lista de seguidores o de seguidos de un perfil
        Task<List<PersonaDto>> ObtenerConexionesAsync(int perfilId, bool seguidores, int visitanteId, int cantidad);
    }
}
