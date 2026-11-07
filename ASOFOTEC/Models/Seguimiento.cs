namespace ASOFOTEC.Models
{
    // Un usuario (Seguidor) sigue a otro (Seguido). No requiere aprobación.
    // Si ambos se siguen entre sí, se consideran "amigos".
    public class Seguimiento
    {
        public int Id { get; set; }

        public int SeguidorId { get; set; }
        public Usuario? Seguidor { get; set; }

        public int SeguidoId { get; set; }
        public Usuario? Seguido { get; set; }

        public DateTime Fecha { get; set; } = DateTime.Now;
    }
}
