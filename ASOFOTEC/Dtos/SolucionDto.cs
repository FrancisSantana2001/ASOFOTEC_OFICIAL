using System.ComponentModel.DataAnnotations;

namespace ASOFOTEC.Dtos
{
    public class SolucionDto
    {
        [Key]
        public int Id { get; set; }
        public int Problema_Id { get; set; }
        [Required(ErrorMessage = "La descripción es obligatoria.")]
        public string? Descripcion { get; set; }
        public string? Estado { get; set; }
        public DateTime Fecha_Solucion { get; set; } = DateTime.Now;

        public string UsuarioNombre { get; set; } = string.Empty;
        public int TotalLikes { get; set; }
        public int TotalComentarios { get; set; }
        public List<ComentarioDto> comentarios { get; set; } = new();



    }
}
