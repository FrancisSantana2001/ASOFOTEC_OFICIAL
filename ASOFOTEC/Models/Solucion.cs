using System.ComponentModel.DataAnnotations.Schema;

namespace ASOFOTEC.Models
{
    public class Solucion
    {
        
        public int Id { get; set; }

        public int ProblemaId { get; set; }
        public Problema? problema { get; set; }

        public int UsuarioId { get; set; }
        public Usuario? Usuario { get; set; }

        public string? Descripcion { get; set; }
        public string? Estado { get; set; } = "Propuesta";
        public DateTime Fecha { get; set; } = DateTime.Now;

        public ICollection<Comentario> Comentarios { get; set; } = new List<Comentario>();
        public ICollection<LikeSolucion> Likes { get; set; } = new List<LikeSolucion>();
    }
}
