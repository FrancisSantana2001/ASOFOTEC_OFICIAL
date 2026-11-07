using ASOFOTEC.Helpers;

namespace ASOFOTEC.Dtos
{
    public class ProblemaDto
    {
        public int Id { get; set; }
        public string? Titulo { get; set; }
        public string? Descripcion { get; set; }
        public string? Categoria { get; set; }
        public string? ImageUrl { get; set; }
        public int? UsuarioId { get; set; }
        public string? UsuarioNombre { get; set; }
        public DateTime Fecha_Creacion { get; set; } = DateTime.Now;

        // Datos con los que se calcula en qué paso de la ruta está el problema
        public bool CausasDocumentadas { get; set; }
        public int TotalSoluciones { get; set; }
        public bool Seleccionada { get; set; }
        public int TotalAvances { get; set; }
        public bool Resuelto { get; set; }

        public int Etapa => RutaProblemaHelper.CalcularEtapa(
            CausasDocumentadas, TotalSoluciones, Seleccionada, TotalAvances, Resuelto);
    }
}
