using System.ComponentModel.DataAnnotations;

namespace ASOFOTEC.Dtos
{
    public class LoginDto
    {
        [Required(ErrorMessage = "El correo es requerido.")]
        [EmailAddress]
        public string Correo { get; set; } = null!;
        [Required(ErrorMessage = "La contraseña es requerida.")]
        public string Password { get; set; } = null!;
    }
}
