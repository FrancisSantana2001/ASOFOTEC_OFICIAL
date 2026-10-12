namespace ASOFOTEC.Dtos
{
    public class HomeDashboardDto
    {
        // Estadísticas generales
        public int TotalUsuarios { get; set; }
        public int TotalProblemas { get; set; }
        public int TotalSoluciones { get; set; }
        public int TotalProyectos { get; set; }
        public int TotalComunidades { get; set; }

        // Listados
        public List<ProblemaDto> ProblemasDestacados { get; set; } = new();
        public List<SolucionDto> SolucionesRecientes { get; set; } = new();
    }
}
