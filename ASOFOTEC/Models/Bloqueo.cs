namespace ASOFOTEC.Models
{
    // BloqueadorId bloqueó a BloqueadoId. Mientras exista, no pueden escribirse ni seguirse.
    public class Bloqueo
    {
        public int Id { get; set; }

        public int BloqueadorId { get; set; }
        public Usuario? Bloqueador { get; set; }

        public int BloqueadoId { get; set; }
        public Usuario? Bloqueado { get; set; }

        public DateTime Fecha { get; set; } = DateTime.Now;
    }
}
