namespace ASOFOTEC.Models
{
    // Miembro del equipo que trabaja en la solución seleccionada de un problema
    public class Colaborador
    {
        public int Id { get; set; }

        public int SolucionId { get; set; }
        public Solucion? Solucion { get; set; }

        public int UsuarioId { get; set; }
        public Usuario? Usuario { get; set; }

        public DateTime FechaUnion { get; set; } = DateTime.Now;
    }
}
