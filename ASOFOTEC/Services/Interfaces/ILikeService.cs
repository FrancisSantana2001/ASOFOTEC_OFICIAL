namespace ASOFOTEC.Services.Interfaces
{
    public interface ILikeService
    {
        Task<bool> ToggleLikesAsync(int solucionId, int usuarioId);
        Task<int> ObtenerTotalLikesAsync(int solucionId);
        Task<bool> UsuarioDioLikeAsync(int solucionId, int usuarioId);
    }
}
