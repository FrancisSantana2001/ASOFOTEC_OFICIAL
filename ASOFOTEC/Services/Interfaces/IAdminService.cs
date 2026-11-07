using ASOFOTEC.Dtos;

namespace ASOFOTEC.Services.Interfaces
{
    // Funciones del administrador único (DBA)
    public interface IAdminService
    {
        // Comprueba contra la base de datos (no contra el token) si el usuario es el DBA activo
        Task<bool> EsDbaAsync(int usuarioId);

        Task<MetricasDbaDto> ObtenerMetricasAsync();
        Task<List<UsuarioAdminDto>> ObtenerUsuariosAsync(string? busqueda);

        // Devuelven null si todo salió bien o un mensaje de error
        Task<string?> CambiarEstadoAsync(int usuarioId, bool activo, int actorId);
        Task<string?> CambiarRolAsync(int usuarioId, string rol, int actorId);

        // Deja constancia en el registro de la aplicación de una eliminación hecha por moderación
        void RegistrarModeracion(int actorId, string tipo, int elementoId);
    }
}
