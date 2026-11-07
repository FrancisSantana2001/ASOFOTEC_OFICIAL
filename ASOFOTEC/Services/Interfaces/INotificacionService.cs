using ASOFOTEC.Dtos;

namespace ASOFOTEC.Services.Interfaces
{
    public interface INotificacionService
    {
        // Crea un aviso. No hace nada si quien actúa es el mismo que lo recibe.
        // Un fallo aquí nunca debe romper la acción principal (dar like, comentar...).
        Task CrearAsync(int receptorId, int actorId, string tipo, string mensaje, string url, string? extracto = null);

        // Ruta para ir a una solución (página de su problema) o a una publicación
        Task<string?> UrlDestinoAsync(int? solucionId, int? publicacionId);

        Task<int> ContarNoLeidasAsync(int usuarioId);
        Task<List<NotificacionDto>> ObtenerRecientesAsync(int usuarioId, int cantidad);
        Task<(List<NotificacionDto> Items, bool HayMas)> ObtenerPaginaAsync(int usuarioId, int pagina, int tamano);

        // Marca como leída y devuelve la ruta a la que lleva (null si no es de este usuario)
        Task<string?> AbrirAsync(int notificacionId, int usuarioId);
        Task MarcarTodasAsync(int usuarioId);
    }
}
