namespace ASOFOTEC.Models
{
    // Registro de avance de la solución seleccionada
    public class Avance
    {
        public int Id { get; set; }

        public int SolucionId { get; set; }
        public Solucion? Solucion { get; set; }

        public int UsuarioId { get; set; }
        public Usuario? Usuario { get; set; }

        public string Contenido { get; set; } = string.Empty;

        // Porcentaje de avance total reportado (0 a 100). Opcional
        public int? Porcentaje { get; set; }

        public DateTime Fecha { get; set; } = DateTime.Now;
    }
}
