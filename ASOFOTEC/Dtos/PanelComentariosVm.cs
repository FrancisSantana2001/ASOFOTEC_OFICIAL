namespace ASOFOTEC.Dtos
{
    // Datos para pintar la caja de comentarios desplegable de una solución o de una publicación
    public class PanelComentariosVm
    {
        // "SolucionId" o "PublicacionId": nombre del campo que recibe el servidor al comentar
        public string Campo { get; set; } = "SolucionId";
        public int DestinoId { get; set; }
        public List<ComentarioDto> Comentarios { get; set; } = new();
        public bool Abierto { get; set; }

        public string Prefijo => Campo == "PublicacionId" ? "pub" : "sol";
        public string PanelId => $"panel-comentarios-{Prefijo}-{DestinoId}";
        public string ListaId => $"comentarios-{Prefijo}-{DestinoId}";
    }
}
