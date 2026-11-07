namespace ASOFOTEC.Services.Interfaces
{
    public interface IPublicacionService
    {
        Task<bool> CrearAsync(string? contenido, string? mediaUrl, string? mediaTipo, int usuarioId);
        Task<bool> EliminarAsync(int id, int usuarioId, bool esModerador = false);
    }
}
