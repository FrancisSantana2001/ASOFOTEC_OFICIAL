using System.ComponentModel.DataAnnotations;

namespace ASOFOTEC.Dtos
{
    public class ComentarioEditDto
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(300)]
        public string Contenido { get; set; } = string.Empty;
    }
}
