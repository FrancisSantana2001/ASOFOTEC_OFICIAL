namespace ASOFOTEC.Dtos
{
    public class ProblemaDto
    {
        public int Id { get; set; }
        public string? Titulo { get; set; }
        public string? Descripcion { get; set; }
        public string? Categoria { get; set; }
        public string? ImageUrl { get; set; }
        public DateTime Fecha_Creacion { get; set; } = DateTime.Now;
    }
}
