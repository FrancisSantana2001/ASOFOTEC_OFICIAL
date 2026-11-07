namespace ASOFOTEC.Models
{
    // Chat entre dos personas. Siempre se cumple UsuarioAId < UsuarioBId, así que
    // cada pareja tiene una única conversación sin importar quién la inicie.
    public class Conversacion
    {
        public int Id { get; set; }

        public int UsuarioAId { get; set; }
        public Usuario? UsuarioA { get; set; }

        public int UsuarioBId { get; set; }
        public Usuario? UsuarioB { get; set; }

        public DateTime Creada { get; set; } = DateTime.UtcNow;
        public DateTime UltimaActividad { get; set; } = DateTime.UtcNow;

        public ICollection<Mensaje> Mensajes { get; set; } = new List<Mensaje>();
    }
}
