using ControleGastos.Application.DTOs;

namespace ControleGastos.Application.Auth;

public sealed class AuthSessionResult
{
    public AuthResponseDto Response { get; init; } = new();
    public string RefreshToken { get; init; } = string.Empty;
    public DateTime RefreshTokenExpiresAtUtc { get; init; }
}
