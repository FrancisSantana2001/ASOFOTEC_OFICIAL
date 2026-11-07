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

        //Paso 2 de la ruta: análisis del problema (opcional)
        public string? Causas { get; set; }
        public string? Consecuencias { get; set; }
        public DateTime FechaCreación { get; set; } = DateTime.Now;

        //Usuario que publicó el problema (nullable: los problemas antiguos no tienen dueño)
        public int? UsuarioId { get; set; }
        public Usuario? Usuario { get; set; }

        //Relaciones
        public ICollection<Solucion> Soluciones { get; set; } = new List<Solucion>();
    }
}
