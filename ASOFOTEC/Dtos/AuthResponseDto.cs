namespace ASOFOTEC.Dtos
{
    public class AuthResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = null!;
        public string? Token { get; set; }
        public string? Nombre { get; set; }
        public string? Rol { get; set; }
        public string? FotoPerfilUrl { get; set; }
    }
}
