using System.ComponentModel.DataAnnotations;

namespace ASOFOTEC.Dtos
{
    public class SolucionDto
    {
        [Key]
        public int Id { get; set; }
        public int Problema_Id { get; set; }
        public string? ProblemaTitulo { get; set; }
        public int UsuarioId { get; set; }
        [Required(ErrorMessage = "La descripción es obligatoria.")]
        public string? Descripcion { get; set; }
        public string? Estado { get; set; }
        public DateTime Fecha_Solucion { get; set; } = DateTime.Now;

        public string UsuarioNombre { get; set; } = string.Empty;
        public string? Impacto { get; set; }
        public string? Costo { get; set; }
        public string? TiempoEstimado { get; set; }
        public bool Seleccionada { get; set; }

        public int TotalLikes { get; set; }
        public bool UsuarioDioLike { get; set; }
        public int TotalComentarios { get; set; }
        public List<ComentarioDto> comentarios { get; set; } = new();



    }
}
