using ASOFOTEC.Dtos;

namespace ASOFOTEC.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterDto register);
        Task<AuthResponseDto> LoginAsync(LoginDto login);
    }
}
