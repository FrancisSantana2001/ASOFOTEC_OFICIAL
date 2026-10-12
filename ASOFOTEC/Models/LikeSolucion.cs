namespace ASOFOTEC.Models
{
    public class LikeSolucion
    {
        public int Id { get; set; }
        public int Usuario_Id { get; set; }
        public Usuario? Usuario { get; set; }
        public int Solucion_Id { get; set; }
        public Solucion? Solucion { get; set; }
        public DateTime FechaLike { get; set; } = DateTime.Now;

    }
}
