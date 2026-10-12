using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASOFOTEC.Models
{
    public class Usuario
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        public string Nombre { get; set; } = null!;
        [Required]
        public string? Apellido { get; set; }
        [Required]
        public string? Correo { get; set; }

        public string? PasswordHash { get; set; }
        [DataType(DataType.Date)]
        public DateTime Fecha_Ingreso { get; set; } = DateTime.Now;
        
        public bool Activo { get; set; } = true;

        //Relaciones
        public ICollection<Publicacion> Publicaciones { get; set; } = new List<Publicacion>();
        public ICollection<Solucion> Soluciones { get; set; } = new List<Solucion>();

        public ICollection<Comentario> Comentarios { get; set; } = new List<Comentario>();
        public ICollection<LikeSolucion> Likes { get; set; } = new List<LikeSolucion>();


    }

}
