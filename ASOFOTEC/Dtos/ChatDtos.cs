namespace ASOFOTEC.Dtos
{
    public class MensajeDto
    {
        public int Id { get; set; }
        public int ConversacionId { get; set; }
        public int RemitenteId { get; set; }
        public string RemitenteNombre { get; set; } = string.Empty;
        public string Texto { get; set; } = string.Empty;

        // UTC (el navegador la muestra en la hora local de quien la ve)
        public DateTime Fecha { get; set; }

        // true = el destinatario ya lo vio
        public bool Leido { get; set; }
    }

    // Una fila de la lista de conversaciones
    public class ConversacionDto
    {
        public int Id { get; set; }
        public int OtroId { get; set; }
        public string OtroNombre { get; set; } = string.Empty;
        public string? OtroFoto { get; set; }

        public string? UltimoTexto { get; set; }
        public DateTime UltimaFecha { get; set; }
        public bool UltimoEsMio { get; set; }
        public int NoLeidos { get; set; }
    }

    // Conversación abierta en el panel derecho
    public class ConversacionActualVm
    {
        public int Id { get; set; }
        public int MiId { get; set; }
        public int OtroId { get; set; }
        public string OtroNombre { get; set; } = string.Empty;
        public string? OtroFoto { get; set; }

        public List<MensajeDto> Mensajes { get; set; } = new();
        public bool HayMas { get; set; }

        // Si no se puede escribir, el motivo explica por qué (se muestra en lugar del cuadro de texto)
        public bool PuedeEscribir { get; set; }
        public string? Motivo { get; set; }
        public bool BloqueadoPorMi { get; set; }
    }

    public class ChatVm
    {
        public int MiId { get; set; }
        public List<ConversacionDto> Conversaciones { get; set; } = new();
        public ConversacionActualVm? Actual { get; set; }

        // Amigos con quienes empezar un chat (solo cuando no hay una conversación abierta)
        public List<PersonaDto> Amigos { get; set; } = new();
    }

    public class MensajesPaginaDto
    {
        public List<MensajeDto> Mensajes { get; set; } = new();
        public bool HayMas { get; set; }
    }
}
