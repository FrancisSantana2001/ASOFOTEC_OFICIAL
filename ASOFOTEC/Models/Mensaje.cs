namespace ASOFOTEC.Models
{
    public class Mensaje
    {
        public int Id { get; set; }

        public int ConversacionId { get; set; }
        public Conversacion? Conversacion { get; set; }

        public int RemitenteId { get; set; }
        public Usuario? Remitente { get; set; }

        public string Texto { get; set; } = string.Empty;

        // Fechas en UTC: cada navegador las muestra en la hora local de quien las ve
        public DateTime Fecha { get; set; } = DateTime.UtcNow;

        // null = todavía no lo ha visto el destinatario
        public DateTime? LeidoEn { get; set; }
    }
}
