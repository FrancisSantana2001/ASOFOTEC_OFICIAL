using System.ComponentModel.DataAnnotations;

namespace ASOFOTEC.Dtos
{
    public class PublicacionCreateDto
    {
        // Opcional si se adjunta una foto o un video
        [MaxLength(500, ErrorMessage = "Máximo 500 caracteres.")]
        public string? Contenido { get; set; }

        // Foto o video adjunto (opcional)
        public IFormFile? Archivo { get; set; }
    }
}
