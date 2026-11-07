using System.ComponentModel.DataAnnotations;

namespace ASOFOTEC.Dtos
{
    public class ProblemaCreateDto
    {
        [Required(ErrorMessage = "El título es requerido.")]
        [MaxLength(100)]
        public string? Titulo { get; set; }
        [Required]
        public string? Descripcion { get; set; }
        [Required]
        public IFormFile? Imagen { get; set; }

        [Required]
        public string? Categoria { get; set; }

        // Paso 2 de la ruta (opcional: se puede completar después)
        [MaxLength(1000)]
        public string? Causas { get; set; }

        [MaxLength(1000)]
        public string? Consecuencias { get; set; }
    }
}
