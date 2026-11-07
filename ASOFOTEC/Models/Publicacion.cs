namespace ASOFOTEC.Models
{
    public class Publicacion
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public Usuario? Usuario { get; set; }
        public string? Contenido { get; set; }
        public DateTime Fecha_Publicacion { get; set; }

        //Foto o video adjunto (opcional). MediaTipo: "imagen" o "video"
        public string? MediaUrl { get; set; }
        public string? MediaTipo { get; set; }

        public ICollection<Comentario> Comentarios { get; set; } = new List<Comentario>();
        public ICollection<LikePublicacion> Likes { get; set; } = new List<LikePublicacion>();
    }
}
