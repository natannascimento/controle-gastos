namespace ControleGastos.Application.DTOs;

public sealed class AuthResponseDto
{
    public string AccessToken { get; set; } = string.Empty;
    public int ExpiresIn { get; set; }
    public AuthUserDto User { get; set; } = new();
}
