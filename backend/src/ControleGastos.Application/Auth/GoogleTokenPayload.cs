namespace ControleGastos.Application.Auth;

public sealed class GoogleTokenPayload
{
    public string Sub { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public bool EmailVerified { get; init; }
}
