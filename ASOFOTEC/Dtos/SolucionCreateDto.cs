using System.ComponentModel.DataAnnotations;

namespace ASOFOTEC.Dtos
{
    public class SolucionCreateDto
    {
        public int ProblemaId { get; set; }

        [Required(ErrorMessage = "La descripción es requerida.")]
        [MaxLength(300)]
        public string? Descripcion { get; set; }

        // Datos opcionales para poder comparar propuestas (paso 4)
        public string? Impacto { get; set; }
        public string? Costo { get; set; }

        [MaxLength(60)]
        public string? TiempoEstimado { get; set; }
    }
}
