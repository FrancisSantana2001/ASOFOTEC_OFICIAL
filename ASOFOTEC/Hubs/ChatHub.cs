using ASOFOTEC.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace ASOFOTEC.Hubs
{
    // Chat en tiempo real entre dos personas amigas.
    //
    // Seguridad:
    //  - Solo con sesión iniciada (la cookie "Jwt" viaja en la conexión).
    //  - Cada acción comprueba en el servidor que la persona participa en la conversación,
    //    que siguen siendo amigas, que no hay bloqueos y que las cuentas están activas.
    //  - Nadie más (tampoco el administrador DBA) tiene forma de leer una conversación ajena:
    //    los mensajes solo se envían a quienes participan en ella.
    [Authorize]
    public class ChatHub : Hub
    {
        // Límite simple contra el envío masivo: 20 mensajes cada 10 segundos por persona
        private const int MaxMensajes = 20;
        private static readonly TimeSpan Ventana = TimeSpan.FromSeconds(10);
        private static readonly ConcurrentDictionary<int, Queue<DateTime>> Envios = new();

        private readonly IChatService _chat;

        public ChatHub(IChatService chat)
        {
            _chat = chat;
        }

        private int? UsuarioId() =>
            int.TryParse(Context.UserIdentifier, out var id) ? id : null;

        private static string Grupo(int usuarioId) => usuarioId.ToString();

        public override async Task OnConnectedAsync()
        {
            var usuarioId = UsuarioId();
            if (usuarioId != null)
            {
                // Al conectarse se informa cuántos mensajes sin leer tiene
                var noLeidos = await _chat.ContarNoLeidosAsync(usuarioId.Value);
                await Clients.Caller.SendAsync("NoLeidos", noLeidos);
            }

            await base.OnConnectedAsync();
        }

        private static bool SuperaLimite(int usuarioId)
        {
            var cola = Envios.GetOrAdd(usuarioId, _ => new Queue<DateTime>());
            lock (cola)
            {
                var ahora = DateTime.UtcNow;
                while (cola.Count > 0 && ahora - cola.Peek() > Ventana) cola.Dequeue();

                if (cola.Count >= MaxMensajes) return true;
                cola.Enqueue(ahora);
                return false;
            }
        }

        // Devuelve null si salió bien o el mensaje de error que debe ver quien escribe
        public async Task<string?> EnviarMensaje(int conversacionId, string? texto)
        {
            var usuarioId = UsuarioId();
            if (usuarioId == null) return "Tu sesión no es válida. Vuelve a iniciar sesión.";

            if (SuperaLimite(usuarioId.Value)) return "Estás enviando mensajes muy rápido. Espera unos segundos.";

            var (mensaje, destinatarioId, error) = await _chat.EnviarAsync(usuarioId.Value, conversacionId, texto);
            if (mensaje == null || destinatarioId == null) return error ?? "No se pudo enviar el mensaje.";

            // El mensaje se entrega a quien lo envía (todas sus pestañas) y a quien lo recibe
            await Clients.Users(Grupo(usuarioId.Value), Grupo(destinatarioId.Value)).SendAsync("MensajeRecibido", mensaje);

            // Actualiza el contador de mensajes sin leer de la otra persona
            var noLeidos = await _chat.ContarNoLeidosAsync(destinatarioId.Value);
            await Clients.User(Grupo(destinatarioId.Value)).SendAsync("NoLeidos", noLeidos);

            return null;
        }

        // Indicador "escribiendo...": no se guarda en ninguna parte, solo se reenvía
        public async Task Escribiendo(int conversacionId, bool escribiendo)
        {
            var usuarioId = UsuarioId();
            if (usuarioId == null) return;

            var otroId = await _chat.ObtenerOtroParticipanteAsync(usuarioId.Value, conversacionId);
            if (otroId == null) return;

            if (await _chat.EstadoDeChatAsync(usuarioId.Value, otroId.Value) != "ok") return;

            await Clients.User(Grupo(otroId.Value)).SendAsync("Escribiendo", conversacionId, escribiendo);
        }

        // La persona vio los mensajes de la conversación: se marcan y se avisa a quien los envió
        public async Task MarcarLeidos(int conversacionId)
        {
            var usuarioId = UsuarioId();
            if (usuarioId == null) return;

            var (otroId, marcados) = await _chat.MarcarLeidosAsync(usuarioId.Value, conversacionId);
            if (otroId == null || marcados == 0) return;

            await Clients.User(Grupo(otroId.Value)).SendAsync("MensajesLeidos", conversacionId);

            // Actualiza el contador en todas las pestañas de quien los leyó
            var noLeidos = await _chat.ContarNoLeidosAsync(usuarioId.Value);
            await Clients.User(Grupo(usuarioId.Value)).SendAsync("NoLeidos", noLeidos);
        }
    }
}
