using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASOFOTEC.Models
{
    public class Problema
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string? Titulo { get; set; }
        public string? Categoria { get; set; }
        public string? Descripcion { get; set; }
        public string? ImgeUrl { get; set; }
        public DateTime FechaCreación { get; set; } = DateTime.Now;

        //Relaciones
        public ICollection<Solucion> Soluciones { get; set; } = new List<Solucion>();
    }
}
