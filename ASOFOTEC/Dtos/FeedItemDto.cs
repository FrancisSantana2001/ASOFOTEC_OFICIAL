namespace ASOFOTEC.Dtos
{
    // Un elemento del feed: puede ser un problema, una solución o una publicación de usuario
    public class FeedItemDto
    {
        public const string TipoProblema = "Problema";
        public const string TipoSolucion = "Solucion";
        public const string TipoPublicacion = "Publicacion";

        public string Tipo { get; set; } = string.Empty;
        public int Id { get; set; }
        public DateTime Fecha { get; set; }

        // Autor (null en problemas antiguos que no tienen dueño)
        public int? UsuarioId { get; set; }
        public string UsuarioNombre { get; set; } = string.Empty;
        public string? UsuarioFoto { get; set; }

        // Contenido
        public string? Titulo { get; set; }
        public string? Categoria { get; set; }
        public string? Estado { get; set; }
        public string? Contenido { get; set; }
        public string? ImageUrl { get; set; }

        // Foto o video adjunto a una publicación (MediaTipo: "imagen" o "video")
        public string? MediaUrl { get; set; }
        public string? MediaTipo { get; set; }

        // Solo para soluciones: problema al que responde
        public int ProblemaId { get; set; }
        public string? ProblemaTitulo { get; set; }

        // Interacción
        public int TotalSoluciones { get; set; }

        // Solo para problemas: datos para mostrar en qué paso de la ruta va
        public bool CausasDocumentadas { get; set; }
        public bool Seleccionada { get; set; }
        public int TotalAvances { get; set; }
        public bool Resuelto { get; set; }
        public int Etapa { get; set; }
        public int TotalLikes { get; set; }
        public int TotalComentarios { get; set; }
        public bool UsuarioDioLike { get; set; }
        public List<ComentarioDto> Comentarios { get; set; } = new();

        public int Popularidad => TotalLikes + TotalComentarios + TotalSoluciones;
    }
}
