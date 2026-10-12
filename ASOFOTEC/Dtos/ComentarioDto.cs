namespace ASOFOTEC.Dtos
{
    public class ComentarioDto
    {
        public int id { get; set; }
        public string contenido { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public DateTime FechaComentario { get; set; }
    }
}
