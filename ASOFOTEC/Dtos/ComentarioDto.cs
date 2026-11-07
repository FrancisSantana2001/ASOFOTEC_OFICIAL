namespace ASOFOTEC.Dtos
{
    public class ComentarioDto
    {
        public int id { get; set; }
        public string contenido { get; set; } = string.Empty;
        public int UsuarioId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string? UserFoto { get; set; }
        public DateTime FechaComentario { get; set; }

        // A qué pertenece (solución o publicación) y, si es respuesta, de qué comentario
        public int? SolucionId { get; set; }
        public int? PublicacionId { get; set; }
        public int? ComentarioPadreId { get; set; }

        public int TotalLikes { get; set; }
        public bool UsuarioDioLike { get; set; }

        // Solo los comentarios principales traen respuestas
        public List<ComentarioDto> Respuestas { get; set; } = new();
    }
}
