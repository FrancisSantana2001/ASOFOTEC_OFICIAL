namespace ASOFOTEC.Services.Interfaces
{
    public record MediaGuardada(string Url, string Tipo);

    public interface IMediaService
    {
        // Valida (extensión, tamaño y contenido real) y guarda una foto o un video.
        // Devuelve el archivo guardado o un mensaje de error.
        Task<(MediaGuardada? Media, string? Error)> GuardarAsync(IFormFile archivo);

        // Guarda una foto de perfil (máx. 3 MB) o de portada (máx. 5 MB). Solo JPG, PNG o WEBP
        Task<(MediaGuardada? Media, string? Error)> GuardarImagenPerfilAsync(IFormFile archivo, bool esPortada);

        // Borra del disco un archivo guardado previamente (publicaciones o perfiles)
        void Eliminar(string? url);
    }
}
