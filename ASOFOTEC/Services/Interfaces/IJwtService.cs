using ASOFOTEC.Models;

namespace ASOFOTEC.Services.Interfaces
{
    public interface IJwtService
    {
        string GenerateToken(Usuario usuario, string rol);
    }
}
