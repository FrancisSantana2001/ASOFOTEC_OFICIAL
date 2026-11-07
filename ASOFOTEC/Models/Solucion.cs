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

        //Paso 4: datos para poder comparar las propuestas (opcionales). Valores: Bajo / Medio / Alto
        public string? Impacto { get; set; }
        public string? Costo { get; set; }
        public string? TiempoEstimado { get; set; }

        //Paso 4 -> 5: solución elegida por el creador del problema (solo una por problema)
        public bool Seleccionada { get; set; }

        //Paso 7: resultados documentados (cierran el problema como resuelto)
        public string? Resultados { get; set; }
        public DateTime? FechaResultados { get; set; }

        //Pasos 5 y 6
        public ICollection<Colaborador> Colaboradores { get; set; } = new List<Colaborador>();
        public ICollection<Avance> Avances { get; set; } = new List<Avance>();

        public ICollection<Comentario> Comentarios { get; set; } = new List<Comentario>();
        public ICollection<LikeSolucion> Likes { get; set; } = new List<LikeSolucion>();
    }
}
