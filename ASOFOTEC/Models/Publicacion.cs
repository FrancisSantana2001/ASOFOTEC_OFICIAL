namespace ASOFOTEC.Models
{
    public class Publicacion
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public Usuario? Usuario { get; set; }
        public string? Contenido { get; set; }
        public DateTime Fecha_Publicacion { get; set; }
    }
}
