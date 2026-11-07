using ASOFOTEC.Dtos;

namespace ASOFOTEC.Services.Interfaces
{
    // Reglas del chat: solo entre amigos (se siguen mutuamente), sin bloqueos y con cuentas activas.
    // Ninguna función de este servicio está disponible para el administrador: solo quienes participan
    // en una conversación pueden leerla.
    public interface IChatService
    {
        // "ok" | "no_amigos" | "bloqueado_por_mi" | "bloqueado" | "inactivo"
        Task<string> EstadoDeChatAsync(int usuarioId, int otroId);

        Task<List<ConversacionDto>> ObtenerConversacionesAsync(int usuarioId);
        Task<List<PersonaDto>> ObtenerAmigosAsync(int usuarioId);

        // Busca o crea la conversación con un amigo. Devuelve su id o un mensaje de error
        Task<(int? ConversacionId, string? Error)> IniciarAsync(int usuarioId, int otroId);

        // null si la conversación no existe o la persona no participa en ella
        Task<ConversacionActualVm?> ObtenerConversacionAsync(int usuarioId, int conversacionId);
        Task<MensajesPaginaDto?> ObtenerMensajesAsync(int usuarioId, int conversacionId, int? antesDeId, int? despuesDeId);

        // Guarda el mensaje. Devuelve el mensaje, quién lo recibe, o un error
        Task<(MensajeDto? Mensaje, int? DestinatarioId, string? Error)> EnviarAsync(int usuarioId, int conversacionId, string? texto);

        // Marca como vistos los mensajes recibidos. Devuelve el otro participante y cuántos se marcaron
        Task<(int? OtroId, int Marcados)> MarcarLeidosAsync(int usuarioId, int conversacionId);

        // Id del otro participante, si la persona participa en la conversación
        Task<int?> ObtenerOtroParticipanteAsync(int usuarioId, int conversacionId);

        Task<int> ContarNoLeidosAsync(int usuarioId);
    }
}
