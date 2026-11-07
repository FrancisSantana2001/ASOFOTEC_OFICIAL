namespace ASOFOTEC.Models
{
    public class LikeComentario
    {
        public int Id { get; set; }
        public int Usuario_Id { get; set; }
        public Usuario? Usuario { get; set; }
        public int Comentario_Id { get; set; }
        public Comentario? Comentario { get; set; }
        public DateTime FechaLike { get; set; } = DateTime.Now;
    }
}
