namespace ControleGastos.Application.Auth;

public sealed class GeneratedRefreshToken
{
    public string RawToken { get; init; } = string.Empty;
    public string Hash { get; init; } = string.Empty;
    public DateTime ExpiresAtUtc { get; init; }
}
