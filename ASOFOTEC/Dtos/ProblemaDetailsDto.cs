using ASOFOTEC.Models;

namespace ASOFOTEC.Dtos
{
    public class ProblemaDetailsDto
    {
        public ProblemaDto Problema { get; set; } = null!;
        public List<SolucionDto> Soluciones { get; set; } = new();
        public SolucionCreateDto NuevaSolucion { get; set; } = new();
        
    }
}
