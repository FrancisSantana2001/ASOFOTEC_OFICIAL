using System.ComponentModel.DataAnnotations;

namespace ASOFOTEC.Helpers
{
    // Valida la fecha de nacimiento: no futura y con la edad mínima permitida (15 años)
    [AttributeUsage(AttributeTargets.Property)]
    public class EdadMinimaAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is not DateTime fecha)
            {
                // La obligatoriedad la controla [Required]
                return ValidationResult.Success;
            }

            var error = DatosPersonalesHelper.ValidarFechaNacimiento(fecha);
            return error == null ? ValidationResult.Success : new ValidationResult(error);
        }
    }
}
