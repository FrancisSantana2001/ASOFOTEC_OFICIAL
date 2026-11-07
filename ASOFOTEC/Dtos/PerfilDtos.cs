using ASOFOTEC.Helpers;
using System.ComponentModel.DataAnnotations;

namespace ASOFOTEC.Dtos
{
    // Datos que el usuario puede cambiar en "Mi perfil" (el correo no se cambia)
    public class PerfilEditDto
    {
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [MaxLength(50)]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido es obligatorio.")]
        [MaxLength(50)]
        public string Apellido { get; set; } = string.Empty;

        [Required(ErrorMessage = "El número de teléfono es obligatorio.")]
        [Telefono]
        public string? Telefono { get; set; }

        [Required(ErrorMessage = "La fecha de nacimiento es obligatoria.")]
        [EdadMinima]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? FechaNacimiento { get; set; }

        [Required(ErrorMessage = "Selecciona tu sexo.")]
        [RegularExpression("^(Masculino|Femenino)$", ErrorMessage = "Selecciona Masculino o Femenino.")]
        public string? Sexo { get; set; }
    }

    // Todo lo que muestra la página "Mi perfil"
    public class PerfilVm
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public DateTime? FechaNacimiento { get; set; }
        public string? Sexo { get; set; }
        public string? FotoPerfilUrl { get; set; }
        public string? FotoPortadaUrl { get; set; }
        public string Rol { get; set; } = RolesSistema.Usuario;
        public DateTime FechaIngreso { get; set; }

        public int TotalPublicaciones { get; set; }
        public int TotalProblemas { get; set; }
        public int TotalSoluciones { get; set; }
        public int TotalComentarios { get; set; }
        public int Seguidores { get; set; }
        public int Siguiendo { get; set; }

        public string NombreCompleto => $"{Nombre} {Apellido}".Trim();
        public int? Edad => FechaNacimiento.HasValue ? DatosPersonalesHelper.CalcularEdad(FechaNacimiento.Value) : null;

        // Formulario de edición precargado
        public PerfilEditDto Edicion { get; set; } = new();
    }

    // Foto de una persona que se muestra en tarjetas, comentarios y menús
    public class AvatarVm
    {
        public string? Nombre { get; set; }
        public string? FotoUrl { get; set; }
        public string Clase { get; set; } = "feed-avatar";

        public static AvatarVm De(string? nombre, string? fotoUrl, string clase) =>
            new() { Nombre = nombre, FotoUrl = fotoUrl, Clase = clase };
    }
}
