namespace ASOFOTEC.Dtos
{
    public class HomeDashboardDto
    {
        // Estadísticas generales (todas con datos reales)
        public int TotalUsuarios { get; set; }
        public int TotalProblemas { get; set; }
        public int TotalSoluciones { get; set; }
        public int TotalProyectos { get; set; }   // problemas con solución elegida y en marcha
        public int TotalResueltos { get; set; }   // problemas con resultados documentados

        // Listados
        public List<ProblemaDto> ProblemasDestacados { get; set; } = new();
        public List<SolucionDto> SolucionesRecientes { get; set; } = new();
    }
}
