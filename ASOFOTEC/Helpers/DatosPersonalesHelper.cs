using System.Text.RegularExpressions;

namespace ASOFOTEC.Helpers
{
    // Reglas de edad, teléfono y sexo usadas por el registro y por el perfil
    public static class DatosPersonalesHelper
    {
        public const int EdadMinima = 15;
        public const int EdadMaxima = 120;

        public static readonly string[] SexosPermitidos = { "Masculino", "Femenino" };

        /// <summary>Edad exacta en años cumplidos a la fecha indicada.</summary>
        public static int CalcularEdad(DateTime nacimiento, DateTime hoy)
        {
            var edad = hoy.Year - nacimiento.Year;

            // Si todavía no llegó el cumpleaños de este año, aún no cumple el año
            if (nacimiento.Date > hoy.Date.AddYears(-edad))
            {
                edad--;
            }
            return edad;
        }

        public static int CalcularEdad(DateTime nacimiento) => CalcularEdad(nacimiento, DateTime.Today);

        /// <summary>Devuelve un mensaje de error si la fecha no es válida, o null si está bien.</summary>
        public static string? ValidarFechaNacimiento(DateTime? fecha)
        {
            if (fecha == null) return "La fecha de nacimiento es obligatoria.";
            if (fecha.Value.Date > DateTime.Today) return "La fecha de nacimiento no puede estar en el futuro.";

            var edad = CalcularEdad(fecha.Value);
            if (edad < EdadMinima) return $"Debes tener al menos {EdadMinima} años para usar ASOFOTEC.";
            if (edad > EdadMaxima) return "La fecha de nacimiento no es válida.";
            return null;
        }

        /// <summary>Quita espacios, guiones y paréntesis. Devuelve null si no parece un teléfono válido.</summary>
        public static string? NormalizarTelefono(string? telefono)
        {
            if (string.IsNullOrWhiteSpace(telefono)) return null;

            var limpio = Regex.Replace(telefono.Trim(), @"[\s\-\(\)\.]", "");

            // Opcional "+" inicial y entre 7 y 15 dígitos
            return Regex.IsMatch(limpio, @"^\+?\d{7,15}$") ? limpio : null;
        }

        public static bool SexoValido(string? sexo) =>
            sexo != null && SexosPermitidos.Contains(sexo);
    }
}
