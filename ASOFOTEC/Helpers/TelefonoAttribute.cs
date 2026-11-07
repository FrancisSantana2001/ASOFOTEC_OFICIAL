using System.ComponentModel.DataAnnotations;

namespace ASOFOTEC.Helpers
{
    // Valida un número telefónico (7 a 15 dígitos, con "+" opcional; se aceptan espacios y guiones)
    [AttributeUsage(AttributeTargets.Property)]
    public class TelefonoAttribute : ValidationAttribute
    {
        public TelefonoAttribute()
        {
            ErrorMessage = "Ingresa un número de teléfono válido (solo números, de 7 a 15 dígitos).";
        }

        public override bool IsValid(object? value)
        {
            if (value is not string texto || string.IsNullOrWhiteSpace(texto))
            {
                // La obligatoriedad la controla [Required]
                return true;
            }
            return DatosPersonalesHelper.NormalizarTelefono(texto) != null;
        }
    }
}
