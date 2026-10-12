namespace ASOFOTEC.Models
{
    public class Comentario
    {
        public int Id { get; set; }
        public string Contenido { get; set; } = string.Empty;
        public DateTime FechaComentario { get; set; } = DateTime.Now;

        //Relacion usuario
        public int UsuarioId { get; set; }
        public Usuario? Usuario { get; set; }

        //Relación solución
        public int SolucionId { get; set; }
        public Solucion? Solucion { get; set; }
    }
}
