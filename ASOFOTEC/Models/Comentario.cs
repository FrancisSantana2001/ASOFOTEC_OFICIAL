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

        //Un comentario pertenece a una solución O a una publicación (uno de los dos)
        public int? SolucionId { get; set; }
        public Solucion? Solucion { get; set; }

        public int? PublicacionId { get; set; }
        public Publicacion? Publicacion { get; set; }

        //Respuestas: si ComentarioPadreId tiene valor, este comentario es una respuesta
        public int? ComentarioPadreId { get; set; }
        public Comentario? ComentarioPadre { get; set; }
        public ICollection<Comentario> Respuestas { get; set; } = new List<Comentario>();

        public ICollection<LikeComentario> Likes { get; set; } = new List<LikeComentario>();
    }
}
