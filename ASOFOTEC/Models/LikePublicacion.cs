namespace ASOFOTEC.Models
{
    public class LikePublicacion
    {
        public int Id { get; set; }
        public int Usuario_Id { get; set; }
        public Usuario? Usuario { get; set; }
        public int Publicacion_Id { get; set; }
        public Publicacion? Publicacion { get; set; }
        public DateTime FechaLike { get; set; } = DateTime.Now;
    }
}
