using ASOFOTEC.Helpers;
using System.ComponentModel.DataAnnotations;

namespace ASOFOTEC.Dtos
{
    public class RegisterDto
    {
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [MaxLength(50)]
        public string Nombre { get; set; } = null!;

        [Required(ErrorMessage = "El apellido es obligatorio.")]
        [MaxLength(50)]
        public string Apellido { get; set; } = null!;

        [Required(ErrorMessage = "El correo es obligatorio.")]
        [EmailAddress(ErrorMessage = "Ingresa un correo válido.")]
        [MaxLength(150)]
        public string Correo { get; set; } = null!;

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres.")]
        public string Contraseña { get; set; } = null!;

        [Required(ErrorMessage = "El número de teléfono es obligatorio.")]
        [Telefono]
        [Display(Name = "Número de teléfono")]
        public string? Telefono { get; set; }

        // La edad se calcula a partir de esta fecha. No se permiten menores de 15 años
        [Required(ErrorMessage = "La fecha de nacimiento es obligatoria.")]
        [EdadMinima]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Fecha de nacimiento")]
        public DateTime? FechaNacimiento { get; set; }

        [Required(ErrorMessage = "Selecciona tu sexo.")]
        [RegularExpression("^(Masculino|Femenino)$", ErrorMessage = "Selecciona Masculino o Femenino.")]
        public string? Sexo { get; set; }

        // Consentimiento para tratar los datos según la política de privacidad
        [Range(typeof(bool), "true", "true", ErrorMessage = "Debes aceptar la política de privacidad para crear tu cuenta.")]
        [Display(Name = "Acepto la política de privacidad")]
        public bool AceptaPolitica { get; set; }
    }
}
