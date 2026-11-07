namespace ASOFOTEC.Dtos
{
    public class FeedViewModel
    {
        public List<FeedItemDto> Items { get; set; } = new();

        // Filtros actuales
        public string? Busqueda { get; set; }
        public string Orden { get; set; } = "reciente";
        public int Cantidad { get; set; } = 10;
        public bool HayMas { get; set; }

        // Barras laterales
        public UsuarioResumenDto Usuario { get; set; } = new();
        public List<ProblemaResumenDto> ProblemasDestacados { get; set; } = new();
        public List<PersonaDto> Sugerencias { get; set; } = new();
        public int TotalUsuarios { get; set; }
        public int TotalProblemas { get; set; }
        public int TotalSoluciones { get; set; }
    }

    public class UsuarioResumenDto
    {
        public string Nombre { get; set; } = string.Empty;
        public string? Correo { get; set; }
        public string? Foto { get; set; }
        public string? Portada { get; set; }
        public int TotalPublicaciones { get; set; }
        public int TotalProblemas { get; set; }
        public int TotalSoluciones { get; set; }
    }

    public class ProblemaResumenDto
    {
        public int Id { get; set; }
        public string? Titulo { get; set; }
        public string? Categoria { get; set; }
        public int TotalSoluciones { get; set; }
    }
}
