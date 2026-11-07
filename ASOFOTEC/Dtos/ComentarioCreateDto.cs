using System.ComponentModel.DataAnnotations;

namespace ASOFOTEC.Dtos
{
    public class ComentarioCreateDto
    {
        [Required]
        [MaxLength(300)]
        public string Contenido { get; set; } = string.Empty;

        // Se envía UNO de los dos: solución o publicación...
        public int? SolucionId { get; set; }
        public int? PublicacionId { get; set; }

        // ...o el comentario al que se responde (el destino se toma del comentario padre)
        public int? ComentarioPadreId { get; set; }
    }
}
