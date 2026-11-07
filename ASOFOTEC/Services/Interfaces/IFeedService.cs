using ASOFOTEC.Dtos;

namespace ASOFOTEC.Services.Interfaces
{
    public interface IFeedService
    {
        // autorId: si se indica, solo trae lo publicado por esa persona (actividad de un perfil)
        Task<FeedViewModel> ObtenerFeedAsync(int usuarioId, string? busqueda, string orden, int cantidad, int? autorId = null);

        Task<FeedItemDto?> ObtenerPublicacionAsync(int id, int usuarioId);
    }
}
