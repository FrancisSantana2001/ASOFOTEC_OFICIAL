namespace ASOFOTEC.Models
{
    public class Roles
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = null!;

        //Relaciones
        public ICollection<UsuarioRol> UserRol { get; set; } = new List<UsuarioRol>();
    }
}
