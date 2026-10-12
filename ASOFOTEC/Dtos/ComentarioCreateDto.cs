using System.ComponentModel.DataAnnotations;

namespace ASOFOTEC.Dtos
{
    public class ComentarioCreateDto
    {
        [Required]
        public string Contenido { get; set; } = string.Empty;
        public int SolucionId { get; set; }
    }
}
