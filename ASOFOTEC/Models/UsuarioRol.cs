
namespace ASOFOTEC.Models
{
    public class UsuarioRol
    {
        public int Usuario_Id { get; set; }
        public Usuario? usuario { get; set; }

        public int Rol_Id { get; set; }
        public Roles? Rol { get; set; }
        
    }
}
