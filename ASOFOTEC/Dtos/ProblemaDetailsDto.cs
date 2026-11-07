using ASOFOTEC.Models;

namespace ASOFOTEC.Dtos
{
    public class ProblemaDetailsDto
    {
        public ProblemaDto Problema { get; set; } = null!;
        public List<SolucionDto> Soluciones { get; set; } = new();
        public SolucionCreateDto NuevaSolucion { get; set; } = new();

        // Paso 2 al 7 de la ruta del problema y paso que se abre al entrar
        public RutaProblemaVm Ruta { get; set; } = new();
        public int PasoInicial { get; set; } = 1;
        
    }
}
