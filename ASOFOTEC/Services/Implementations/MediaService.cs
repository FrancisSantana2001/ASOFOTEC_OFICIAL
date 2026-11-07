using ASOFOTEC.Services.Interfaces;

namespace ASOFOTEC.Services.Implementations
{
    public class MediaService : IMediaService
    {
        public const long MaxBytesImagen = 5L * 1024 * 1024;     // 5 MB
        public const long MaxBytesVideo = 50L * 1024 * 1024;     // 50 MB

        public const long MaxBytesFotoPerfil = 3L * 1024 * 1024;   // 3 MB
        public const long MaxBytesFotoPortada = 5L * 1024 * 1024;  // 5 MB

        private const string CarpetaUrl = "/uploads/publicaciones/";
        private const string CarpetaPerfilesUrl = "/uploads/perfiles/";

        private static readonly HashSet<string> ExtensionesPerfil =
            new(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".webp" };

        private static readonly HashSet<string> ExtensionesImagen =
            new(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

        private static readonly HashSet<string> ExtensionesVideo =
            new(StringComparer.OrdinalIgnoreCase) { ".mp4", ".webm", ".mov" };

        private readonly IWebHostEnvironment _env;

        public MediaService(IWebHostEnvironment env)
        {
            _env = env;
        }

        public async Task<(MediaGuardada? Media, string? Error)> GuardarAsync(IFormFile archivo)
        {
            if (archivo == null || archivo.Length == 0)
            {
                return (null, "El archivo está vacío.");
            }

            var extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();
            var esImagen = ExtensionesImagen.Contains(extension);
            var esVideo = ExtensionesVideo.Contains(extension);

            if (!esImagen && !esVideo)
            {
                return (null, "Formato no permitido. Usa JPG, PNG, GIF o WEBP para fotos, y MP4, WEBM o MOV para videos.");
            }

            if (esImagen && archivo.Length > MaxBytesImagen)
            {
                return (null, "La foto supera el máximo de 5 MB.");
            }

            if (esVideo && archivo.Length > MaxBytesVideo)
            {
                return (null, "El video supera el máximo de 50 MB.");
            }

            // No basta con confiar en la extensión: se revisa que el contenido real coincida
            var cabecera = new byte[12];
            await using (var lectura = archivo.OpenReadStream())
            {
                var leidos = await lectura.ReadAsync(cabecera, 0, cabecera.Length);
                if (leidos < cabecera.Length)
                {
                    return (null, "El archivo no es válido.");
                }
            }

            if (!FirmaValida(cabecera, extension))
            {
                return (null, "El contenido del archivo no coincide con su formato.");
            }

            var carpeta = Path.Combine(_env.WebRootPath, "uploads", "publicaciones");
            Directory.CreateDirectory(carpeta);

            // Nombre generado por el servidor: nunca se usa el nombre que envía el usuario
            var nombre = Guid.NewGuid().ToString("N") + extension;
            var ruta = Path.Combine(carpeta, nombre);

            await using (var destino = new FileStream(ruta, FileMode.Create))
            {
                await archivo.CopyToAsync(destino);
            }

            return (new MediaGuardada(CarpetaUrl + nombre, esVideo ? "video" : "imagen"), null);
        }

        public async Task<(MediaGuardada? Media, string? Error)> GuardarImagenPerfilAsync(IFormFile archivo, bool esPortada)
        {
            if (archivo == null || archivo.Length == 0)
            {
                return (null, "El archivo está vacío.");
            }

            var extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();
            if (!ExtensionesPerfil.Contains(extension))
            {
                return (null, "Formato no permitido. Usa una imagen JPG, PNG o WEBP.");
            }

            var maximo = esPortada ? MaxBytesFotoPortada : MaxBytesFotoPerfil;
            if (archivo.Length > maximo)
            {
                return (null, $"La imagen supera el máximo de {maximo / (1024 * 1024)} MB.");
            }

            // Se comprueba que el contenido real coincida con la extensión
            var cabecera = new byte[12];
            await using (var lectura = archivo.OpenReadStream())
            {
                var leidos = await lectura.ReadAsync(cabecera, 0, cabecera.Length);
                if (leidos < cabecera.Length || !FirmaValida(cabecera, extension))
                {
                    return (null, "El contenido del archivo no coincide con su formato.");
                }
            }

            var carpeta = Path.Combine(_env.WebRootPath, "uploads", "perfiles");
            Directory.CreateDirectory(carpeta);

            var nombre = Guid.NewGuid().ToString("N") + extension;
            await using (var destino = new FileStream(Path.Combine(carpeta, nombre), FileMode.Create))
            {
                await archivo.CopyToAsync(destino);
            }

            return (new MediaGuardada(CarpetaPerfilesUrl + nombre, "imagen"), null);
        }

        public void Eliminar(string? url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return;
            }

            string subcarpeta;
            if (url.StartsWith(CarpetaUrl, StringComparison.Ordinal))
            {
                subcarpeta = "publicaciones";
            }
            else if (url.StartsWith(CarpetaPerfilesUrl, StringComparison.Ordinal))
            {
                subcarpeta = "perfiles";
            }
            else
            {
                return;
            }

            // GetFileName evita rutas tipo ../../
            var ruta = Path.Combine(_env.WebRootPath, "uploads", subcarpeta, Path.GetFileName(url));

            try
            {
                if (File.Exists(ruta))
                {
                    File.Delete(ruta);
                }
            }
            catch (IOException)
            {
                // Si el archivo está en uso no se interrumpe la eliminación de la publicación
            }
        }

        private static bool FirmaValida(byte[] b, string extension)
        {
            switch (extension)
            {
                case ".jpg":
                case ".jpeg":
                    return b[0] == 0xFF && b[1] == 0xD8 && b[2] == 0xFF;

                case ".png":
                    return b[0] == 0x89 && b[1] == 0x50 && b[2] == 0x4E && b[3] == 0x47;

                case ".gif":
                    return b[0] == 'G' && b[1] == 'I' && b[2] == 'F' && b[3] == '8';

                case ".webp":
                    return b[0] == 'R' && b[1] == 'I' && b[2] == 'F' && b[3] == 'F'
                        && b[8] == 'W' && b[9] == 'E' && b[10] == 'B' && b[11] == 'P';

                case ".mp4":
                case ".mov":
                    // Los contenedores MP4/MOV llevan "ftyp" en los bytes 4 a 7
                    return b[4] == 'f' && b[5] == 't' && b[6] == 'y' && b[7] == 'p';

                case ".webm":
                    return b[0] == 0x1A && b[1] == 0x45 && b[2] == 0xDF && b[3] == 0xA3;

                default:
                    return false;
            }
        }
    }
}
