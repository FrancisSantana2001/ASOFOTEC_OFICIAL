using System.ComponentModel.DataAnnotations;

namespace ASOFOTEC.Dtos
{
    public class SolucionCreateDto
    {
        public int ProblemaId { get; set; }

        [Required(ErrorMessage = "La descripción es requerida.")]
        [MaxLength(300)]
        public string? Descripcion { get; set; }
    }
}
