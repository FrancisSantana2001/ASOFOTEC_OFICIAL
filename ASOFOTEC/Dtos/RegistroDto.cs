using System.ComponentModel.DataAnnotations;

namespace ASOFOTEC.Dtos
{
    public class RegistroDto
    {
        [Required]
        public string? Nombre { get; set; }
        [Required]
        public string? Apellido { get; set; }
        [Required]
        public string? Correo { get; set; }
        [Required]
        public string? Password { get; set; }
    }
}
