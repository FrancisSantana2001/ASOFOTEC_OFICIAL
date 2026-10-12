using System.ComponentModel.DataAnnotations;

namespace ASOFOTEC.Dtos
{
    public class RegisterDto
    {
        [Required]
        public string Nombre { get; set; } = null!;
        [Required]
        public string Apellido { get; set; } = null!;
        [Required]
        [EmailAddress]
        public string Correo { get; set; } = null!;
        [Required]
        [MinLength(6)]
        public string Contraseña { get; set; } = null!;

    }
}
