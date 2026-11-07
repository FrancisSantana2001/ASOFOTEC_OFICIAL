namespace ASOFOTEC.Models
{
    // Aviso para un usuario (UsuarioId) sobre algo que hizo otra persona (ActorId)
    public class Notificacion
    {
        public int Id { get; set; }

        // Quien recibe el aviso
        public int UsuarioId { get; set; }
        public Usuario? Usuario { get; set; }

        // Quien hizo la acción (dio like, comentó, empezó a seguir...)
        public int ActorId { get; set; }
        public Usuario? Actor { get; set; }

        // Ver TiposNotificacion
        public string Tipo { get; set; } = string.Empty;

        // Texto sin el nombre de quien actuó. Ej.: "le dio me gusta a tu publicación"
        public string Mensaje { get; set; } = string.Empty;

        // Fragmento opcional (parte del comentario, título del problema...)
        public string? Extracto { get; set; }

        // Ruta del propio sitio a la que lleva el aviso
        public string Url { get; set; } = "/";

        public bool Leida { get; set; }
        public DateTime Fecha { get; set; } = DateTime.Now;
    }

    public static class TiposNotificacion
    {
        public const string Like = "like";
        public const string Comentario = "comentario";
        public const string Respuesta = "respuesta";
        public const string Seguidor = "seguidor";
        public const string Solucion = "solucion";
    }
}
