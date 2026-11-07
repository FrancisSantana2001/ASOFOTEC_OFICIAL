using ASOFOTEC.Dtos;

namespace ASOFOTEC.Services.Interfaces
{
    public interface ILikeService
    {
        Task<bool> ToggleLikesAsync(int solucionId, int usuarioId);
        Task<int> ObtenerTotalLikesAsync(int solucionId);
        Task<bool> UsuarioDioLikeAsync(int solucionId, int usuarioId);

        // Publicaciones y comentarios (devuelven null si el elemento no existe)
        Task<LikeResultado?> ToggleLikePublicacionAsync(int publicacionId, int usuarioId);
        Task<LikeResultado?> ToggleLikeComentarioAsync(int comentarioId, int usuarioId);
    }
}
